using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class AnalyticsAdminController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        var (start, end) = NormalizeRange(startDate, endDate);

        var query = dbContext.PlaybackLogs
            .Where(log => log.PlayedAt >= start && log.PlayedAt <= end);

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

        var heatmapStats = await query
            .GroupBy(log => log.PoiId)
            .Select(group => new
            {
                PoiId = group.Key,
                Weight = group.Count()
            })
            .OrderByDescending(row => row.Weight)
            .Take(200)
            .ToListAsync();

        var poiIds = topPoiStats
            .Select(row => row.PoiId)
            .Concat(heatmapStats.Select(row => row.PoiId))
            .Distinct()
            .ToList();

        var poiLookup = await dbContext.Pois
            .IgnoreQueryFilters()
            .Where(poi => poiIds.Contains(poi.Id))
            .Select(poi => new { poi.Id, poi.Name, poi.Latitude, poi.Longitude })
            .ToDictionaryAsync(poi => poi.Id);

        var topPois = topPoiStats
            .Select(row => new AnalyticsTopPoiRow
            {
                PoiId = row.PoiId,
                Name = poiLookup.TryGetValue(row.PoiId, out var poi) ? poi.Name : "Unknown POI",
                ListenCount = row.ListenCount,
                AverageDurationSeconds = row.AverageDurationSeconds
            })
            .OrderByDescending(row => row.ListenCount)
            .ThenBy(row => row.Name)
            .ToList();

        var heatmap = heatmapStats
            .Where(row => poiLookup.ContainsKey(row.PoiId))
            .GroupBy(row => new
            {
                Lat = Math.Round(poiLookup[row.PoiId].Latitude, 4),
                Lng = Math.Round(poiLookup[row.PoiId].Longitude, 4)
            })
            .Select(group => new AnalyticsHeatmapRow
            {
                Lat = group.Key.Lat,
                Lng = group.Key.Lng,
                Weight = group.Sum(x => x.Weight)
            })
            .OrderByDescending(row => row.Weight)
            .Take(200)
            .ToList();

        var vm = new AnalyticsDashboardViewModel
        {
            StartDate = start,
            EndDate = end,
            TotalUsers = totalUsers,
            TotalListens = totalListens,
            AverageDurationSeconds = averageDurationSeconds,
            TopPois = topPois,
            HeatmapPoints = heatmap
        };

        return View(vm);
    }

    private static (DateTimeOffset Start, DateTimeOffset End) NormalizeRange(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        var end = endDate ?? DateTimeOffset.UtcNow;
        var start = startDate ?? end.AddDays(-30);
        if (start > end)
        {
            (start, end) = (end, start);
        }

        return (start, end);
    }
}
