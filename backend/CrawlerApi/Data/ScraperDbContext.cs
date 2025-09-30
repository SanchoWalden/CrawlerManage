using CrawlerApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrawlerApi.Data;

public class ScraperDbContext(DbContextOptions<ScraperDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<ScrapedItem> ScrapedItems => Set<ScrapedItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScraperDbContext).Assembly);
    }
}
