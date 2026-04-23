using System.Net;
using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class QrSessionsAdminController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(string? sessionToken = null, string? accessMode = null)
    {
        var normalizedSessionToken = string.IsNullOrWhiteSpace(sessionToken)
            ? $"demo-{DateTime.UtcNow:yyyyMMdd-HHmmss}"
            : sessionToken.Trim();

        var normalizedAccessMode = string.Equals(accessMode, "trial", StringComparison.OrdinalIgnoreCase)
            ? "trial"
            : "full";

        var payload = $"GEOGUIDE:JOIN:{normalizedSessionToken}:{normalizedAccessMode.ToUpperInvariant()}";

        var joins = await dbContext.DeviceSessionJoins
            .Where(row => row.SessionToken == normalizedSessionToken)
            .OrderBy(row => row.JoinedAt)
            .ToListAsync();

        var logs = await dbContext.PlaybackLogs
            .Where(row => row.SessionToken == normalizedSessionToken)
            .OrderByDescending(row => row.PlayedAt)
            .ToListAsync();

        var poiLookup = await dbContext.Pois
            .IgnoreQueryFilters()
            .Where(poi => logs.Select(log => log.PoiId).Distinct().Contains(poi.Id))
            .ToDictionaryAsync(poi => poi.Id, poi => poi.Name);

        var devices = joins
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

        var vm = new QrSessionAdminViewModel
        {
            SessionToken = normalizedSessionToken,
            AccessMode = normalizedAccessMode,
            PayloadText = payload,
            QrImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=280x280&data={WebUtility.UrlEncode(payload)}",
            Devices = devices
        };

        return View(vm);
    }
}
