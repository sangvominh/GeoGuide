using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GeoGuide.Cms.Controllers;

[AllowAnonymous]
[Route("join")]
public class JoinController(IWebHostEnvironment environment) : Controller
{
    [HttpGet("")]
    public IActionResult Index(string session, string? mode = null)
    {
        if (string.IsNullOrWhiteSpace(session))
        {
            return BadRequest("Thiếu session.");
        }

        var normalizedSession = session.Trim();
        var normalizedMode = string.Equals(mode, "trial", StringComparison.OrdinalIgnoreCase)
            ? "trial"
            : "full";

        var publicBaseUrl = ResolvePublicBaseUrl();

        var vm = new JoinLandingViewModel
        {
            SessionToken = normalizedSession,
            AccessMode = normalizedMode,
            JoinUrl = $"{publicBaseUrl}/join?session={Uri.EscapeDataString(normalizedSession)}&mode={normalizedMode}",
            DeepLinkUrl = BuildDeepLink(normalizedSession, normalizedMode, publicBaseUrl),
            AndroidApkUrl = ResolveAndroidApkUrl(environment)
        };

        return View(vm);
    }

    private static string BuildDeepLink(string sessionToken, string accessMode, string apiBaseUrl)
    {
        return $"geoguide://join?session={Uri.EscapeDataString(sessionToken)}&mode={accessMode}&apiBaseUrl={Uri.EscapeDataString(apiBaseUrl)}";
    }

    private string ResolvePublicBaseUrl()
    {
        var configured = Environment.GetEnvironmentVariable("GEOGUIDE_PUBLIC_BASE_URL");
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
