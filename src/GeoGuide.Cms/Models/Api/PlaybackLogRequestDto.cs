using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models.Api;

public class PlaybackLogRequestDto
{
    [Required]
    public Guid PoiId { get; set; }

    public DateTimeOffset PlayedAt { get; set; }

    [Required, RegularExpression("gps|qr|manual")]
    public string TriggerType { get; set; } = string.Empty;

    [Range(0, 86400)]
    public int DurationSeconds { get; set; }

    [Required, StringLength(200)]
    public string DeviceId { get; set; } = string.Empty;
}
