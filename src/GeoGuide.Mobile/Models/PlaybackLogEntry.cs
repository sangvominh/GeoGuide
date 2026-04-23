namespace MauiApp1.Models;

public class PlaybackLogEntry
{
    public string PoiId { get; set; } = string.Empty;
    public DateTimeOffset PlayedAt { get; set; }
    public string TriggerType { get; set; } = "manual";
    public int DurationSeconds { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public string? ClientType { get; set; }
}
