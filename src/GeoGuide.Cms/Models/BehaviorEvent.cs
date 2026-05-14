using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models;

public class BehaviorEvent
{
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string DeviceId { get; set; } = string.Empty;

    public Guid? PoiId { get; set; }

    public Poi? Poi { get; set; }

    [Required, RegularExpression("position|audio_started|audio_completed|qr_scanned")]
    public string EventType { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    [Range(0, 86400)]
    public int DurationSeconds { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    [StringLength(120)]
    public string? SessionToken { get; set; }

    [StringLength(20)]
    public string? ClientType { get; set; }
}
