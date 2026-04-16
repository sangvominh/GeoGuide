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

        var topPois = await query
            .Join(
                dbContext.Pois.IgnoreQueryFilters(),
                log => log.PoiId,
                poi => poi.Id,
                (log, poi) => new { log, poi })
            .GroupBy(row => new { row.poi.Id, row.poi.Name })
            .Select(group => new AnalyticsTopPoiRow
            {
                PoiId = group.Key.Id,
                Name = group.Key.Name,
                ListenCount = group.Count(),
                AverageDurationSeconds = (int)Math.Round(group.Average(x => x.log.DurationSeconds))
            })
            .OrderByDescending(row => row.ListenCount)
            .ThenBy(row => row.Name)
            .Take(10)
            .ToListAsync();

        var heatmap = await query
            .Join(
                dbContext.Pois.IgnoreQueryFilters(),
                log => log.PoiId,
                poi => poi.Id,
                (log, poi) => new { poi.Latitude, poi.Longitude })
            .GroupBy(row => new
            {
                Lat = Math.Round(row.Latitude, 4),
                Lng = Math.Round(row.Longitude, 4)
            })
            .Select(group => new AnalyticsHeatmapRow
            {
                Lat = group.Key.Lat,
                Lng = group.Key.Lng,
                Weight = group.Count()
            })
            .OrderByDescending(row => row.Weight)
            .Take(200)
            .ToListAsync();

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
