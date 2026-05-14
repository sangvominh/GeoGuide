namespace GeoGuide.Cms.Models.Api;

public class PoiDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public int TriggerRadiusMeters { get; init; }

    public int CooldownMinutes { get; init; }

    public int Priority { get; init; }

    public string CategoryKey { get; init; } = string.Empty;

    public string CategoryLabel { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public string MapUrl { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public double? DistanceMeters { get; init; }

    public string RequestedLanguage { get; init; } = string.Empty;

    public string ResolvedLanguage { get; init; } = string.Empty;

    public IReadOnlyList<string> FallbackChain { get; init; } = [];

    public IReadOnlyList<PoiContentDto> Contents { get; init; } = [];
}
