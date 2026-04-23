namespace GeoGuide.Cms.Models;

public class AnalyticsDashboardViewModel
{
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public string StartDateInput { get; init; } = string.Empty;
    public string EndDateInput { get; init; } = string.Empty;
    public string SessionTokenInput { get; init; } = string.Empty;

    public int TotalDevices { get; init; }
    public int TotalListens { get; init; }
    public int AverageDurationSeconds { get; init; }

    public IReadOnlyList<AnalyticsTopPoiRow> TopPois { get; init; } = [];
    public IReadOnlyList<AnalyticsHeatmapRow> HeatmapPoints { get; init; } = [];
    public IReadOnlyList<AnalyticsSessionDeviceRow> SessionDevices { get; init; } = [];
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

public class AnalyticsSessionDeviceRow
{
    public string DeviceId { get; init; } = string.Empty;
    public string ClientType { get; init; } = string.Empty;
    public string AccessMode { get; init; } = string.Empty;
    public DateTimeOffset JoinedAt { get; init; }
    public DateTimeOffset LastSeenAt { get; init; }
    public int ListenCount { get; init; }
    public int TotalDurationSeconds { get; init; }
    public string LastPoiName { get; init; } = string.Empty;
}
