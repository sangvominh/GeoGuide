using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class AdminReviewController(ApplicationDbContext dbContext, CmsAccessService accessService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var scope = await accessService.GetScopeAsync();
        if (!scope.IsSystemAdmin)
        {
            return Forbid();
        }

        // Get Pending POIs
        var pendingPois = await dbContext.Pois
            .Include(p => p.Tenant)
            .Where(p => p.ApprovalStatus == PoiApprovalStatus.PendingApproval)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        // Get recently approved/rejected POIs (e.g. last 10)
        var recentPois = await dbContext.Pois
            .Include(p => p.Tenant)
            .Where(p => p.ApprovalStatus == PoiApprovalStatus.Approved || p.ApprovalStatus == PoiApprovalStatus.Rejected)
            .OrderByDescending(p => p.UpdatedAt)
            .Take(10)
            .ToListAsync();

        var model = new AdminReviewIndexViewModel
        {
            PendingPois = pendingPois,
            RecentPois = recentPois
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id)
    {
        var scope = await accessService.GetScopeAsync();
        if (!scope.IsSystemAdmin)
        {
            return Forbid();
        }

        var poi = await dbContext.Pois.FirstOrDefaultAsync(p => p.Id == id);
        if (poi is null)
        {
            return NotFound();
        }

        poi.ApprovalStatus = PoiApprovalStatus.Approved;
        poi.IsActive = true;
        poi.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id)
    {
        var scope = await accessService.GetScopeAsync();
        if (!scope.IsSystemAdmin)
        {
            return Forbid();
        }

        var poi = await dbContext.Pois.FirstOrDefaultAsync(p => p.Id == id);
        if (poi is null)
        {
            return NotFound();
        }

        poi.ApprovalStatus = PoiApprovalStatus.Rejected;
        poi.IsActive = false;
        poi.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
