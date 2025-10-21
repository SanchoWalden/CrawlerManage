using CrawlerApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrawlerApi.Data.Configurations;

public class ScrapedItemConfiguration : IEntityTypeConfiguration<ScrapedItem>
{
    public void Configure(EntityTypeBuilder<ScrapedItem> builder)
    {
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(p => p.Source)
            .HasMaxLength(128);

        builder.Property(p => p.Summary)
            .HasMaxLength(1024);

        builder.Property(p => p.CollectedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.MetadataJson)
            .HasColumnType("NVARCHAR(MAX)");

        // Indexes for performance
        builder.HasIndex(p => p.Url)
            .HasDatabaseName("IX_ScrapedItems_Url");

        builder.HasIndex(p => p.Source)
            .HasDatabaseName("IX_ScrapedItems_Source");

        // Composite index for common query patterns (time-based filtering + source)
        builder.HasIndex(p => new { p.CollectedAt, p.Source })
            .HasDatabaseName("IX_ScrapedItems_CollectedAt_Source");

        // Index for time-based queries (most recent items)
        builder.HasIndex(p => p.CollectedAt)
            .IsDescending()
            .HasDatabaseName("IX_ScrapedItems_CollectedAt_Desc");

        // Index for full-text search on Title
        builder.HasIndex(p => p.Title)
            .HasDatabaseName("IX_ScrapedItems_Title");
    }
}