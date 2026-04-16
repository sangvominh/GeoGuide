namespace GeoGuide.Cms.Models.Api;

public class AnalyticsDashboardDto
{
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public int TotalUsers { get; init; }
    public int TotalListens { get; init; }
    public int AverageDurationSeconds { get; init; }
    public IReadOnlyList<TopPoiAnalyticsDto> TopPois { get; init; } = [];
}

public class TopPoiAnalyticsDto
{
    public Guid PoiId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ListenCount { get; init; }
    public int AverageDurationSeconds { get; init; }
}
