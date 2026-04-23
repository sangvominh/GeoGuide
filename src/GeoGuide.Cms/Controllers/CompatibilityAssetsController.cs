using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers;

[AllowAnonymous]
public class CompatibilityAssetsController : Controller
{
    [HttpGet("GeoGuide.Cms.styles.css")]
    public IActionResult Stylesheet()
    {
        return Content("/* compatibility placeholder for stale cached layouts */", "text/css");
    }
}
