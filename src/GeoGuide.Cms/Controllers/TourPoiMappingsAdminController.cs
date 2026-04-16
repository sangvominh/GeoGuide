using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class TourPoiMappingsAdminController(ApplicationDbContext dbContext, CmsAccessService accessService) : Controller
{
    public async Task<IActionResult> Index(Guid tourId)
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        var tour = await dbContext.Tours.FirstOrDefaultAsync(t => t.Id == tourId);
        if (tour is null)
        {
            return NotFound();
        }

        var mappings = await dbContext.TourPoiMappings
            .Where(m => m.TourId == tourId)
            .Include(m => m.Poi)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync();

        var mappedPoiIds = mappings.Select(m => m.PoiId).ToHashSet();
        var availablePois = await dbContext.Pois
            .Where(p => p.IsActive && !mappedPoiIds.Contains(p.Id))
            .OrderBy(p => p.Name)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            })
            .ToListAsync();

        return View(new TourPoiMappingsAdminViewModel
        {
            Tour = tour,
            Mappings = mappings,
            AvailablePois = availablePois
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid tourId, Guid poiId, int orderIndex)
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        var exists = await dbContext.TourPoiMappings.AnyAsync(m => m.TourId == tourId && m.PoiId == poiId);
        if (!exists)
        {
            dbContext.TourPoiMappings.Add(new TourPoiMapping
            {
                TourId = tourId,
                PoiId = poiId,
                OrderIndex = orderIndex
            });
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { tourId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrder(Guid tourId, Guid poiId, int orderIndex)
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        var mapping = await dbContext.TourPoiMappings.FirstOrDefaultAsync(m => m.TourId == tourId && m.PoiId == poiId);
        if (mapping is null)
        {
            return NotFound();
        }

        mapping.OrderIndex = orderIndex;
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { tourId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid tourId, Guid poiId)
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        var mapping = await dbContext.TourPoiMappings.FirstOrDefaultAsync(m => m.TourId == tourId && m.PoiId == poiId);
        if (mapping is null)
        {
            return NotFound();
        }

        dbContext.TourPoiMappings.Remove(mapping);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { tourId });
    }

    private async Task<bool> IsSystemAdminAsync()
    {
        var scope = await accessService.GetScopeAsync();
        return scope.IsSystemAdmin;
    }
}
