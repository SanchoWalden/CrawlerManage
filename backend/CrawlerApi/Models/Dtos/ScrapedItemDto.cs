using System.Text.Json;

namespace CrawlerApi.Models.Dtos;

public record ScrapedItemDto(
    int Id,
    string Title,
    string Url,
    string? Source,
    string? Summary,
    string? Content,
    DateTime CollectedAt,
    IReadOnlyDictionary<string, string>? Metadata
)
{
    public static ScrapedItemDto FromEntity(Models.ScrapedItem entity)
    {
        IReadOnlyDictionary<string, string>? metadata = null;

        if (!string.IsNullOrWhiteSpace(entity.MetadataJson))
        {
            try
            {
                metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MetadataJson);
            }
            catch (JsonException)
            {
                metadata = null;
            }
        }

        return new ScrapedItemDto(
            entity.Id,
            entity.Title,
            entity.Url,
            entity.Source,
            entity.Summary,
            entity.Content,
            entity.CollectedAt,
            metadata
        );
    }
}
