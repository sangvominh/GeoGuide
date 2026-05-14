using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models.Api;

public class PoiUpsertDto
{
    [Required, StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; init; }

    [Range(-180, 180)]
    public double Longitude { get; init; }

    [Range(1, 10000)]
    public int TriggerRadiusMeters { get; init; } = 80;

    [Range(0, 1440)]
    public int CooldownMinutes { get; init; } = 15;

    [Range(0, 1000)]
    public int Priority { get; init; } = 1;

    [Required, StringLength(100)]
    public string CategoryKey { get; init; } = "attraction";

    [Required, StringLength(200)]
    public string CategoryLabel { get; init; } = "Tham quan";

    [Url]
    public string ImageUrl { get; init; } = string.Empty;

    [Url]
    public string MapUrl { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;

    public Guid? TenantId { get; init; }

    public IReadOnlyList<PoiContentUpsertDto>? Contents { get; init; }
}
