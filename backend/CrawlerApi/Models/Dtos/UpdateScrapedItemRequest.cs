using System.ComponentModel.DataAnnotations;

namespace CrawlerApi.Models.Dtos;

public class UpdateScrapedItemRequest
{
    [MaxLength(256)]
    public string? Title { get; set; }

    [Url, MaxLength(2048)]
    public string? Url { get; set; }

    [MaxLength(128)]
    public string? Source { get; set; }

    [MaxLength(1024)]
    public string? Summary { get; set; }

    public string? Content { get; set; }

    public DateTime? CollectedAt { get; set; }

    public IDictionary<string, string>? Metadata { get; set; }
}
