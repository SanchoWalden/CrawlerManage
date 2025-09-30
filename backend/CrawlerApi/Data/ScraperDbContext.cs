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

        var entity = modelBuilder.Entity<ScrapedItem>();

        entity.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(256);

        entity.Property(p => p.Url)
            .IsRequired()
            .HasMaxLength(2048);

        entity.Property(p => p.Source)
            .HasMaxLength(128);

        entity.Property(p => p.Summary)
            .HasMaxLength(1024);

        entity.Property(p => p.CollectedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(p => p.MetadataJson)
            .HasColumnType("NVARCHAR(MAX)");

        entity.HasIndex(p => p.Url);
        entity.HasIndex(p => p.Source);
    }
}
