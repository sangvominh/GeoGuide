using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers;

[AllowAnonymous]
[Route("join")]
public class JoinController : Controller
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
            DeepLinkUrl = BuildDeepLink(normalizedSession, normalizedMode),
            AndroidApkUrl = ResolveAndroidApkUrl()
        };

        return View(vm);
    }

    private static string BuildDeepLink(string sessionToken, string accessMode)
    {
        return $"geoguide://join?session={Uri.EscapeDataString(sessionToken)}&mode={accessMode}";
    }

    private static string ResolveAndroidApkUrl()
    {
        var configured = Environment.GetEnvironmentVariable("GEOGUIDE_ANDROID_APK_URL");
        return string.IsNullOrWhiteSpace(configured) ? "/downloads/GeoGuide.Mobile.apk" : configured.Trim();
    }
}
