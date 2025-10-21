using System.Text;
using System.Text.Json;
using CrawlerApi.Constants;
using CrawlerApi.Data;
using CrawlerApi.Extensions;
using CrawlerApi.Middleware;
using CrawlerApi.Models;
using CrawlerApi.Models.Dtos;
using CrawlerApi.Models.Dtos.Auth;
using CrawlerApi.Options;
using CrawlerApi.Services;
using CrawlerApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Crawler API",
        Version = "v1",
        Description = "HTTP endpoints for the SchoolManage crawler backend."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Input your JWT like: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
    ?? new CorsOptions();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        if (corsOptions.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsOptions.AllowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Fallback for production - should be configured via environment variables
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

builder.Services.AddDbContext<ScraperDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'Default' is not configured. " +
            "Please set it in appsettings.json, User Secrets, or environment variables.");
    }

    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
});

builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireDigit = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ScraperDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 16)
{
    throw new InvalidOperationException("JWT secret must be configured and at least 16 characters long.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

builder.Services.AddSingleton(signingKey);
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole(AppRoles.Administrator));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScraperDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in AppRoles.All)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Crawler API v1");
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");

api.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous();

var authGroup = api.MapGroup("/auth");

authGroup.MapPost("/register", async (
    RegisterRequest request,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IJwtTokenService tokenService,
    CancellationToken cancellationToken) =>
{
    var email = request.Email.Trim();
    var userName = request.UserName.Trim();

    if (await userManager.FindByEmailAsync(email) is not null)
    {
        return Results.Conflict(new { message = "��������ע��" });
    }

    if (await userManager.FindByNameAsync(userName) is not null)
    {
        return Results.Conflict(new { message = "���û����Ѵ���" });
    }

    var user = new AppUser
    {
        Email = email,
        UserName = userName,
        DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? userName : request.DisplayName.Trim(),
        EmailConfirmed = true
    };

    var createResult = await userManager.CreateAsync(user, request.Password);
    if (!createResult.Succeeded)
    {
        var errors = createResult.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
        return Results.ValidationProblem(errors);
    }

    if (!await roleManager.RoleExistsAsync(AppRoles.User))
    {
        await roleManager.CreateAsync(new IdentityRole(AppRoles.User));
    }

    await userManager.AddToRoleAsync(user, AppRoles.User);

    var tokenResult = await tokenService.GenerateTokenAsync(user, cancellationToken);
    var roles = await userManager.GetRolesAsync(user);

    var dto = new AuthenticatedUserDto(user.Id, user.UserName, user.Email, user.DisplayName, roles.ToArray());
    var response = new AuthResponse(tokenResult.Token, tokenResult.ExpiresAt, dto);

    return Results.Ok(response);
}).WithOpenApi().AllowAnonymous();

authGroup.MapPost("/login", async (
    LoginRequest request,
    UserManager<AppUser> userManager,
    IJwtTokenService tokenService,
    CancellationToken cancellationToken) =>
{
    var identifier = request.EmailOrUserName.Trim();

    AppUser? user = await userManager.FindByEmailAsync(identifier)
        ?? await userManager.FindByNameAsync(identifier);

    if (user is null)
    {
        return Results.BadRequest(new { message = "�˺Ż��������" });
    }

    if (!await userManager.CheckPasswordAsync(user, request.Password))
    {
        return Results.BadRequest(new { message = "�˺Ż��������" });
    }

    var tokenResult = await tokenService.GenerateTokenAsync(user, cancellationToken);
    var roles = await userManager.GetRolesAsync(user);

    var dto = new AuthenticatedUserDto(user.Id, user.UserName, user.Email, user.DisplayName, roles.ToArray());
    var response = new AuthResponse(tokenResult.Token, tokenResult.ExpiresAt, dto);

    return Results.Ok(response);
}).WithOpenApi().AllowAnonymous();

var scrapedItemsGroup = api.MapGroup("/scraped-items")
    .RequireAuthorization();

scrapedItemsGroup.MapGet("", async (
    string? search,
    string? source,
    DateTime? collectedFrom,
    DateTime? collectedTo,
    int page,
    int pageSize,
    ScraperDbContext db,
    CancellationToken cancellationToken) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 ? 20 : Math.Clamp(pageSize, 1, 100);

    var query = db.ScrapedItems.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var pattern = $"%{search.Trim()}%";
        query = query.Where(item =>
            EF.Functions.Like(item.Title, pattern) ||
            (item.Summary != null && EF.Functions.Like(item.Summary, pattern)) ||
            (item.Content != null && EF.Functions.Like(item.Content, pattern)));
    }

    if (!string.IsNullOrWhiteSpace(source))
    {
        query = query.Where(item => item.Source == source);
    }

    if (collectedFrom.HasValue)
    {
        query = query.Where(item => item.CollectedAt >= collectedFrom.Value);
    }

    if (collectedTo.HasValue)
    {
        query = query.Where(item => item.CollectedAt <= collectedTo.Value);
    }

    var total = await query.CountAsync(cancellationToken);

    var items = await query
        .OrderByDescending(item => item.CollectedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    var results = items.Select(ScrapedItemDto.FromEntity).ToList();

    return Results.Ok(new { total, page, pageSize, items = results });
}).WithOpenApi();

scrapedItemsGroup.MapGet("/{id:int}", async (int id, ScraperDbContext db, CancellationToken cancellationToken) =>
{
    var entity = await db.ScrapedItems.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    return entity is null
        ? Results.NotFound()
        : Results.Ok(ScrapedItemDto.FromEntity(entity));
}).WithOpenApi();

scrapedItemsGroup.MapPost("", async (
    CreateScrapedItemRequest request,
    IValidator<CreateScrapedItemRequest> validator,
    ScraperDbContext db,
    CancellationToken cancellationToken) =>
{
    var validationResult = await validator.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        return Results.ValidationProblem(errors);
    }

    var entity = new ScrapedItem
    {
        Title = request.Title,
        Url = request.Url,
        Source = request.Source,
        Summary = request.Summary,
        Content = request.Content,
        CollectedAt = request.CollectedAt ?? DateTime.UtcNow,
        MetadataJson = request.Metadata is { Count: > 0 }
            ? JsonSerializer.Serialize(request.Metadata)
            : null
    };

    db.ScrapedItems.Add(entity);
    await db.SaveChangesAsync(cancellationToken);

    var dto = ScrapedItemDto.FromEntity(entity);
    return Results.Created($"/api/scraped-items/{entity.Id}", dto);
}).WithOpenApi();

scrapedItemsGroup.MapPut("/{id:int}", async (
    int id,
    UpdateScrapedItemRequest request,
    ScraperDbContext db,
    CancellationToken cancellationToken) =>
{
    var entity = await db.ScrapedItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    if (entity is null)
    {
        return Results.NotFound();
    }

    if (request.Title is not null)
    {
        entity.Title = request.Title;
    }

    if (request.Url is not null)
    {
        entity.Url = request.Url;
    }

    if (request.Source is not null)
    {
        entity.Source = request.Source;
    }

    if (request.Summary is not null)
    {
        entity.Summary = request.Summary;
    }

    if (request.Content is not null)
    {
        entity.Content = request.Content;
    }

    if (request.CollectedAt.HasValue)
    {
        entity.CollectedAt = request.CollectedAt.Value;
    }

    if (request.Metadata is not null)
    {
        entity.MetadataJson = request.Metadata.Count > 0
            ? JsonSerializer.Serialize(request.Metadata)
            : null;
    }

    await db.SaveChangesAsync(cancellationToken);

    return Results.Ok(ScrapedItemDto.FromEntity(entity));
}).WithOpenApi();

scrapedItemsGroup.MapDelete("/{id:int}", async (int id, ScraperDbContext db, CancellationToken cancellationToken) =>
{
    var entity = await db.ScrapedItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    if (entity is null)
    {
        return Results.NotFound();
    }

    db.ScrapedItems.Remove(entity);
    await db.SaveChangesAsync(cancellationToken);
    return Results.NoContent();
}).WithOpenApi();

app.Run();


