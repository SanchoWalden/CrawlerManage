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

        builder.HasIndex(p => p.Url);
        builder.HasIndex(p => p.Source);
    }
}