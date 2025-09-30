using CrawlerApi.Models;
using CrawlerApi.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CrawlerApi.Services;

public record JwtTokenResult(string Token, DateTime ExpiresAt);

public interface IJwtTokenService
{
    Task<JwtTokenResult> GenerateTokenAsync(AppUser user, CancellationToken cancellationToken = default);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly UserManager<AppUser> _userManager;

    public JwtTokenService(
        IOptions<JwtOptions> options,
        SymmetricSecurityKey signingKey,
        UserManager<AppUser> userManager)
    {
        _options = options.Value;
        _signingKey = signingKey;
        _userManager = userManager;
    }

    public async Task<JwtTokenResult> GenerateTokenAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? user.Id),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            claims.Add(new Claim("display_name", user.DisplayName));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        var serializedToken = handler.WriteToken(token);

        return new JwtTokenResult(serializedToken, expires);
    }
}
