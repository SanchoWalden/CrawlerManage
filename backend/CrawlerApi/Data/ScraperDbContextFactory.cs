using CrawlerApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CrawlerApi.Data;

public class ScraperDbContextFactory : IDesignTimeDbContextFactory<ScraperDbContext>
{
    public ScraperDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=ScraperDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        var builder = new DbContextOptionsBuilder<ScraperDbContext>()
            .UseSqlServer(connectionString, options => options.EnableRetryOnFailure());

        return new ScraperDbContext(builder.Options);
    }
}
