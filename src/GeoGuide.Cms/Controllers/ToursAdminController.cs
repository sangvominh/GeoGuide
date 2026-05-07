using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class ToursAdminController(ApplicationDbContext dbContext, CmsAccessService accessService) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        var tours = await dbContext.Tours
            .Include(t => t.PoiMappings)
            .OrderBy(t => t.Name)
            .ToListAsync();

        return View(new ToursAdminIndexViewModel
        {
            Tours = tours
        });
    }

    public async Task<IActionResult> Create()
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        return View(new Tour());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tour tour)
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(tour);
        }

        tour.Id = Guid.NewGuid();
        tour.CreatedAt = DateTimeOffset.UtcNow;
        tour.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Tours.Add(tour);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        var tour = await dbContext.Tours.FirstOrDefaultAsync(t => t.Id == id);
        return tour is null ? NotFound() : View(tour);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Tour tour)
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        if (id != tour.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(tour);
        }

        var existing = await dbContext.Tours.FirstOrDefaultAsync(t => t.Id == id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Name = tour.Name;
        existing.Description = tour.Description;
        existing.ThumbnailUrl = tour.ThumbnailUrl;
        existing.IsActive = tour.IsActive;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        var tour = await dbContext.Tours.FirstOrDefaultAsync(t => t.Id == id);
        return tour is null ? NotFound() : View(tour);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await IsSystemAdminAsync())
        {
            return Forbid();
        }

        var tour = await dbContext.Tours.FirstOrDefaultAsync(t => t.Id == id);
        if (tour is null)
        {
            return NotFound();
        }

        tour.IsDeleted = true;
        tour.IsActive = false;
        tour.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> IsSystemAdminAsync()
    {
        var scope = await accessService.GetScopeAsync();
        return scope.IsSystemAdmin;
    }
}
