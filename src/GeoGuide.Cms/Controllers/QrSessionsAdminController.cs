using System.Net;
using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class QrSessionsAdminController(ApplicationDbContext dbContext, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(string? sessionToken = null, string? accessMode = null, string? publicBaseUrl = null)
    {
        var normalizedSessionToken = string.IsNullOrWhiteSpace(sessionToken)
            ? $"demo-{DateTime.UtcNow:yyyyMMdd-HHmmss}"
            : sessionToken.Trim();

        var normalizedAccessMode = string.Equals(accessMode, "trial", StringComparison.OrdinalIgnoreCase)
            ? "trial"
            : "full";
        var resolvedPublicBaseUrl = ResolvePublicBaseUrl(publicBaseUrl);
        var joinUrl = $"{resolvedPublicBaseUrl}/join?session={Uri.EscapeDataString(normalizedSessionToken)}&mode={normalizedAccessMode}";
        var deepLinkUrl = BuildDeepLink(normalizedSessionToken, normalizedAccessMode);

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
            PublicBaseUrl = resolvedPublicBaseUrl,
            JoinUrl = joinUrl,
            DeepLinkUrl = deepLinkUrl,
            AndroidApkUrl = ResolveAndroidApkUrl(environment),
            QrImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=280x280&data={WebUtility.UrlEncode(joinUrl)}",
            Devices = devices
        };

        return View(vm);
    }

    private string ResolvePublicBaseUrl(string? publicBaseUrl)
    {
        var configured = string.IsNullOrWhiteSpace(publicBaseUrl)
            ? Environment.GetEnvironmentVariable("GEOGUIDE_PUBLIC_BASE_URL")
            : publicBaseUrl;

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.Trim().TrimEnd('/');
        }

        return $"{Request.Scheme}://{Request.Host}";
    }

    private static string BuildDeepLink(string sessionToken, string accessMode)
    {
        return $"geoguide://join?session={Uri.EscapeDataString(sessionToken)}&mode={accessMode}";
    }

    private static string ResolveAndroidApkUrl(IWebHostEnvironment environment)
    {
        var configured = Environment.GetEnvironmentVariable("GEOGUIDE_ANDROID_APK_URL");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.Trim();
        }

        var localApkPath = Path.Combine(environment.WebRootPath, "downloads", "GeoGuide.Mobile.apk");
        return System.IO.File.Exists(localApkPath) ? "/downloads/GeoGuide.Mobile.apk" : string.Empty;
    }
}
