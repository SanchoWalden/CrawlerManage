using System.ComponentModel.DataAnnotations;

namespace CrawlerApi.Models;

public class ScrapedItem
{
    public int Id { get; set; }

    [Required, MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Source { get; set; }

    [MaxLength(1024)]
    public string? Summary { get; set; }

    public string? Content { get; set; }

    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;

    // Optional JSON payload storing arbitrary metadata captured by the crawler.
    public string? MetadataJson { get; set; }
}
