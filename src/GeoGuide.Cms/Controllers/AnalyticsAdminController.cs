using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class AnalyticsAdminController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        string? sessionToken = null)
    {
        var range = NormalizeRange(startDate, endDate);
        var normalizedSessionToken = NormalizeSessionToken(sessionToken);

        var query = dbContext.PlaybackLogs
            .Where(log => log.PlayedAt >= range.StartUtc && log.PlayedAt < range.EndExclusiveUtc);

        if (!string.IsNullOrWhiteSpace(normalizedSessionToken))
        {
            query = query.Where(log => log.SessionToken == normalizedSessionToken);
        }

        var totalDevices = await query
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

        var sessionDevices = await BuildSessionDevicesAsync(normalizedSessionToken);

        var vm = new AnalyticsDashboardViewModel
        {
            StartDate = range.StartUtc,
            EndDate = range.EndExclusiveUtc.AddTicks(-1),
            StartDateInput = range.StartDateInput,
            EndDateInput = range.EndDateInput,
            SessionTokenInput = normalizedSessionToken ?? string.Empty,
            TotalDevices = totalDevices,
            TotalListens = totalListens,
            AverageDurationSeconds = averageDurationSeconds,
            TopPois = topPois,
            HeatmapPoints = heatmap,
            SessionDevices = sessionDevices
        };

        return View(vm);
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
            EndExclusiveUtc = endExclusiveLocal.ToUniversalTime(),
            StartDateInput = startLocalDate.ToString("yyyy-MM-dd"),
            EndDateInput = endLocalDate.ToString("yyyy-MM-dd")
        };
    }

    private sealed class AnalyticsRange
    {
        public required DateTimeOffset StartUtc { get; init; }
        public required DateTimeOffset EndExclusiveUtc { get; init; }
        public required string StartDateInput { get; init; }
        public required string EndDateInput { get; init; }
    }

    private static string? NormalizeSessionToken(string? sessionToken)
    {
        return string.IsNullOrWhiteSpace(sessionToken) ? null : sessionToken.Trim();
    }

    private async Task<IReadOnlyList<AnalyticsSessionDeviceRow>> BuildSessionDevicesAsync(string? sessionToken)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return [];
        }

        var joins = await dbContext.DeviceSessionJoins
            .Where(row => row.SessionToken == sessionToken)
            .OrderBy(row => row.JoinedAt)
            .ToListAsync();

        var logs = await dbContext.PlaybackLogs
            .Where(row => row.SessionToken == sessionToken)
            .OrderByDescending(row => row.PlayedAt)
            .ToListAsync();

        var poiLookup = await dbContext.Pois
            .IgnoreQueryFilters()
            .Where(poi => logs.Select(log => log.PoiId).Distinct().Contains(poi.Id))
            .ToDictionaryAsync(poi => poi.Id, poi => poi.Name);

        return joins
            .Select(join =>
            {
                var deviceLogs = logs.Where(log => log.DeviceId == join.DeviceId).ToList();
                var lastPoiName = deviceLogs.Count == 0
                    ? string.Empty
                    : poiLookup.TryGetValue(deviceLogs[0].PoiId, out var poiName)
                        ? poiName
                        : string.Empty;

                return new AnalyticsSessionDeviceRow
                {
                    DeviceId = join.DeviceId,
                    ClientType = join.ClientType,
                    AccessMode = join.AccessMode,
                    JoinedAt = join.JoinedAt,
                    LastSeenAt = join.LastSeenAt,
                    ListenCount = deviceLogs.Count,
                    TotalDurationSeconds = deviceLogs.Sum(log => log.DurationSeconds),
                    LastPoiName = lastPoiName
                };
            })
            .ToList();
    }
}
