using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

public class PoisAdminController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var pois = await dbContext.Pois
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return View(pois);
    }

    public IActionResult Create()
    {
        return View(new Poi());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Poi poi)
    {
        if (!ModelState.IsValid)
        {
            return View(poi);
        }

        poi.Id = Guid.NewGuid();
        poi.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.Pois.Add(poi);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var poi = await dbContext.Pois.FindAsync(id);
        return poi is null ? NotFound() : View(poi);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Poi poi)
    {
        if (id != poi.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(poi);
        }

        var existingPoi = await dbContext.Pois.FindAsync(id);
        if (existingPoi is null)
        {
            return NotFound();
        }

        existingPoi.Name = poi.Name;
        existingPoi.Description = poi.Description;
        existingPoi.Latitude = poi.Latitude;
        existingPoi.Longitude = poi.Longitude;
        existingPoi.TriggerRadiusMeters = poi.TriggerRadiusMeters;
        existingPoi.Priority = poi.Priority;
        existingPoi.CategoryKey = poi.CategoryKey;
        existingPoi.CategoryLabel = poi.CategoryLabel;
        existingPoi.ImageUrl = poi.ImageUrl;
        existingPoi.MapUrl = poi.MapUrl;
        existingPoi.AudioUrl = poi.AudioUrl;
        existingPoi.TtsScript = poi.TtsScript;
        existingPoi.LanguageCode = poi.LanguageCode;
        existingPoi.IsActive = poi.IsActive;
        existingPoi.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var poi = await dbContext.Pois.FindAsync(id);
        return poi is null ? NotFound() : View(poi);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var poi = await dbContext.Pois.FindAsync(id);
        if (poi is null)
        {
            return NotFound();
        }

        dbContext.Pois.Remove(poi);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
