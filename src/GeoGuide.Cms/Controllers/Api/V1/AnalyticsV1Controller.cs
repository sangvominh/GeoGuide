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
            .Select(group => new TopPoiAnalyticsDto
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

        var response = new AnalyticsDashboardDto
        {
            StartDate = start,
            EndDate = end,
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
        var (start, end) = NormalizeRange(startDate, endDate);

        var heatmap = await dbContext.PlaybackLogs
            .Where(log => log.PlayedAt >= start && log.PlayedAt <= end)
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
            .Select(group => new HeatmapPointDto
            {
                Lat = group.Key.Lat,
                Lng = group.Key.Lng,
                Weight = group.Count()
            })
            .OrderByDescending(row => row.Weight)
            .Take(10000)
            .ToListAsync();

        return Ok(heatmap);
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
