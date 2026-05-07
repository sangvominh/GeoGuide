using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/analytics")]
public class AnalyticsV1Controller(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        var range = NormalizeRange(startDate, endDate);

        var query = dbContext.PlaybackLogs
            .Where(log => log.PlayedAt >= range.StartUtc && log.PlayedAt < range.EndExclusiveUtc);

        var totalUsers = await query
            .Select(log => log.DeviceId)
            .Distinct()
            .CountAsync();

        var totalListens = await query.CountAsync();

        var averageDurationSeconds = await query.AnyAsync()
            ? (int)Math.Round(await query.AverageAsync(log => log.DurationSeconds))
            : 0;

        var topPoiStats = await query
            .GroupBy(log => log.PoiId)
            .Select(group => new
            {
                PoiId = group.Key,
                ListenCount = group.Count(),
                AverageDurationSeconds = (int)Math.Round(group.Average(x => x.DurationSeconds))
            })
            .OrderByDescending(row => row.ListenCount)
            .Take(10)
            .ToListAsync();

        var poiLookup = await dbContext.Pois
            .IgnoreQueryFilters()
            .Where(poi => topPoiStats.Select(row => row.PoiId).Contains(poi.Id))
            .Select(poi => new { poi.Id, poi.Name })
            .ToDictionaryAsync(poi => poi.Id, poi => poi.Name);

        var topPois = topPoiStats
            .Select(row => new TopPoiAnalyticsDto
            {
                PoiId = row.PoiId,
                Name = poiLookup.TryGetValue(row.PoiId, out var name) ? name : "Unknown POI",
                ListenCount = row.ListenCount,
                AverageDurationSeconds = row.AverageDurationSeconds
            })
            .OrderByDescending(row => row.ListenCount)
            .ThenBy(row => row.Name)
            .ToList();

        var response = new AnalyticsDashboardDto
        {
            StartDate = range.StartUtc,
            EndDate = range.EndExclusiveUtc.AddTicks(-1),
            TotalUsers = totalUsers,
            TotalListens = totalListens,
            AverageDurationSeconds = averageDurationSeconds,
            TopPois = topPois
        };

        return Ok(response);
    }

    [HttpGet("heatmap")]
    public async Task<IActionResult> Heatmap([FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        var range = NormalizeRange(startDate, endDate);

        var heatmapStats = await dbContext.PlaybackLogs
            .Where(log => log.PlayedAt >= range.StartUtc && log.PlayedAt < range.EndExclusiveUtc)
            .GroupBy(log => log.PoiId)
            .Select(group => new
            {
                PoiId = group.Key,
                Weight = group.Count()
            })
            .OrderByDescending(row => row.Weight)
            .Take(10000)
            .ToListAsync();

        var poiLookup = await dbContext.Pois
            .IgnoreQueryFilters()
            .Where(poi => heatmapStats.Select(row => row.PoiId).Contains(poi.Id))
            .Select(poi => new { poi.Id, poi.Latitude, poi.Longitude })
            .ToDictionaryAsync(poi => poi.Id);

        var heatmap = heatmapStats
            .Where(row => poiLookup.ContainsKey(row.PoiId))
            .GroupBy(row => new
            {
                Lat = Math.Round(poiLookup[row.PoiId].Latitude, 4),
                Lng = Math.Round(poiLookup[row.PoiId].Longitude, 4)
            })
            .Select(group => new HeatmapPointDto
            {
                Lat = group.Key.Lat,
                Lng = group.Key.Lng,
                Weight = group.Sum(x => x.Weight)
            })
            .OrderByDescending(row => row.Weight)
            .Take(10000)
            .ToList();

        return Ok(heatmap);
    }

    private static AnalyticsRange NormalizeRange(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        var localToday = DateTime.Today;
        var startLocalDate = startDate?.Date ?? localToday.AddDays(-30);
        var endLocalDate = endDate?.Date ?? localToday;

        if (startLocalDate > endLocalDate)
        {
            (startLocalDate, endLocalDate) = (endLocalDate, startLocalDate);
        }

        var startLocal = new DateTimeOffset(startLocalDate, TimeZoneInfo.Local.GetUtcOffset(startLocalDate));
        var endExclusiveLocalDate = endLocalDate.AddDays(1);
        var endExclusiveLocal = new DateTimeOffset(endExclusiveLocalDate, TimeZoneInfo.Local.GetUtcOffset(endExclusiveLocalDate));

        return new AnalyticsRange
        {
            StartUtc = startLocal.ToUniversalTime(),
            EndExclusiveUtc = endExclusiveLocal.ToUniversalTime()
        };
    }

    private sealed class AnalyticsRange
    {
        public required DateTimeOffset StartUtc { get; init; }
        public required DateTimeOffset EndExclusiveUtc { get; init; }
    }
}
