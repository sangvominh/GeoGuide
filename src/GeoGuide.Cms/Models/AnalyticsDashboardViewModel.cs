namespace GeoGuide.Cms.Models;

public class AnalyticsDashboardViewModel
{
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public string StartDateInput { get; init; } = string.Empty;
    public string EndDateInput { get; init; } = string.Empty;

    public int TotalUsers { get; init; }
    public int TotalListens { get; init; }
    public int AverageDurationSeconds { get; init; }

    public IReadOnlyList<AnalyticsTopPoiRow> TopPois { get; init; } = [];
    public IReadOnlyList<AnalyticsHeatmapRow> HeatmapPoints { get; init; } = [];
}

public class AnalyticsTopPoiRow
{
    public Guid PoiId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ListenCount { get; init; }
    public int AverageDurationSeconds { get; init; }
}

public class AnalyticsHeatmapRow
{
    public double Lat { get; init; }
    public double Lng { get; init; }
    public int Weight { get; init; }
}
