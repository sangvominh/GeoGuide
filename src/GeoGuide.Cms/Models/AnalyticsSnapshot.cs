namespace GeoGuide.Cms.Models;

public class AnalyticsSnapshot
{
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public string SessionToken { get; init; } = string.Empty;
    public int TotalDevices { get; init; }
    public int TotalSessionJoins { get; init; }
    public int TotalListens { get; init; }
    public int AverageDurationSeconds { get; init; }
    public int TotalBehaviorEvents { get; init; }
    public int TotalStops { get; init; }
    public int AverageStopDurationSeconds { get; init; }
    public IReadOnlyList<AnalyticsTopPoiRow> TopPois { get; init; } = [];
    public IReadOnlyList<AnalyticsVisitPoiRow> TopVisitedPois { get; init; } = [];
    public IReadOnlyList<AnalyticsHeatmapRow> HeatmapPoints { get; init; } = [];
    public IReadOnlyList<AnalyticsSessionDeviceRow> SessionDevices { get; init; } = [];
}
