namespace GeoGuide.Cms.Models.Api;

public class PoiContentDto
{
    public Guid Id { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public bool IsFallback { get; init; }

    public string? AudioUrl { get; init; }

    public string? TtsContent { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
