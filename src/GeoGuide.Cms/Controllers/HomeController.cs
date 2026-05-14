using System.Diagnostics;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole(CmsRoles.PoiTenant) && !User.IsInRole(CmsRoles.SystemAdmin))
        {
            return RedirectToAction("Index", "OwnerPortal");
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
