using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models;

public class Poi
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; set; }

    public Guid? TenantId { get; set; }

    public PoiTenant? Tenant { get; set; }

    public ICollection<PoiAudio> Audios { get; set; } = [];

    public ICollection<TourPoiMapping> TourMappings { get; set; } = [];

    public PoiApprovalStatus ApprovalStatus { get; set; } = PoiApprovalStatus.PendingApproval;

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(1, 10000)]
    public int TriggerRadiusMeters { get; set; } = 80;

    [Range(0, 1440)]
    public int CooldownMinutes { get; set; } = 15;

    [Range(0, 1000)]
    public int Priority { get; set; } = 1;

    [Required, StringLength(100)]
    public string CategoryKey { get; set; } = "attraction";

    [Required, StringLength(200)]
    public string CategoryLabel { get; set; } = "Tham quan";

    [Url]
    public string ImageUrl { get; set; } = string.Empty;

    [Url]
    public string MapUrl { get; set; } = string.Empty;

    [Url]
    public string AudioUrl { get; set; } = string.Empty;

    [Required]
    public string TtsScript { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string LanguageCode { get; set; } = "vi-VN";

    public bool IsActive { get; set; } = true;
}
