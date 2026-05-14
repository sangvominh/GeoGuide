namespace MauiApp1.Models;

public class BehaviorEventEntry
{
    public string DeviceId { get; set; } = string.Empty;
    public string? PoiId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int DurationSeconds { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? SessionToken { get; set; }
    public string? ClientType { get; set; }
}
