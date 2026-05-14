using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        var vm = new JoinLandingViewModel
        {
            SessionToken = normalizedSession,
            AccessMode = normalizedMode,
            JoinUrl = $"{Request.Scheme}://{Request.Host}/join?session={Uri.EscapeDataString(normalizedSession)}&mode={normalizedMode}",
            DeepLinkUrl = BuildDeepLink(normalizedSession, normalizedMode, $"{Request.Scheme}://{Request.Host}"),
            AndroidApkUrl = ResolveAndroidApkUrl(environment)
        };

        return View(vm);
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
