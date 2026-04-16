namespace GeoGuide.Cms.Models.Api;

public class TourDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string ThumbnailUrl { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public IReadOnlyList<TourPoiDto> Pois { get; init; } = [];
}
