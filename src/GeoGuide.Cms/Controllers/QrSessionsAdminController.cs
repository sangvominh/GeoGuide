using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class QrSessionsAdminController(ApplicationDbContext dbContext, IWebHostEnvironment environment) : Controller
{
    private const string DefaultSessionToken = "geoguide-public";

    public async Task<IActionResult> Index(string? sessionToken = null, string? accessMode = null, string? publicBaseUrl = null)
    {
        var normalizedSessionToken = string.IsNullOrWhiteSpace(sessionToken)
            ? ResolveDefaultSessionToken()
            : sessionToken.Trim();

        var normalizedAccessMode = string.Equals(accessMode, "trial", StringComparison.OrdinalIgnoreCase)
            ? "trial"
            : "full";
        var resolvedPublicBaseUrl = ResolvePublicBaseUrl(publicBaseUrl);
        var joinUrl = $"{resolvedPublicBaseUrl}/join?session={Uri.EscapeDataString(normalizedSessionToken)}&mode={normalizedAccessMode}";
        var deepLinkUrl = BuildDeepLink(normalizedSessionToken, normalizedAccessMode, resolvedPublicBaseUrl);

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

        if (IsLoopbackHost(Request.Host.Host) && TryResolveLanBaseUrl(out var lanBaseUrl))
        {
            return lanBaseUrl;
        }

        return $"{Request.Scheme}://{Request.Host}";
    }

    private string BuildBaseUrl(IPAddress ipAddress)
    {
        var port = Request.Host.Port is null ? string.Empty : $":{Request.Host.Port}";
        return $"{Request.Scheme}://{ipAddress}{port}";
    }

    private bool TryResolveLanBaseUrl(out string baseUrl)
    {
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up ||
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
            {
                continue;
            }

            var ipAddress = networkInterface
                .GetIPProperties()
                .UnicastAddresses
                .Select(address => address.Address)
                .FirstOrDefault(address =>
                    address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(address));

            if (ipAddress is null)
            {
                continue;
            }

            baseUrl = BuildBaseUrl(ipAddress);
            return true;
        }

        baseUrl = string.Empty;
        return false;
    }

    private static bool IsLoopbackHost(string host)
    {
        return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveDefaultSessionToken()
    {
        var configured = Environment.GetEnvironmentVariable("GEOGUIDE_DEFAULT_SESSION_TOKEN");
        return string.IsNullOrWhiteSpace(configured) ? DefaultSessionToken : configured.Trim();
    }

    private static string BuildDeepLink(string sessionToken, string accessMode, string apiBaseUrl)
    {
        return $"geoguide://join?session={Uri.EscapeDataString(sessionToken)}&mode={accessMode}&apiBaseUrl={Uri.EscapeDataString(apiBaseUrl)}";
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
