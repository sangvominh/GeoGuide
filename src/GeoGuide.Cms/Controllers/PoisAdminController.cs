using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class PoisAdminController(ApplicationDbContext dbContext, CmsAccessService accessService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var scope = await accessService.GetScopeAsync();
        var query = dbContext.Pois
            .Include(p => p.Tenant)
            .AsQueryable();

        if (!scope.IsSystemAdmin)
        {
            query = query.Where(p => p.TenantId == scope.TenantId);
        }

        var pois = await query
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return View(pois);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateTenantOptionsAsync();
        return View(new Poi());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Poi poi)
    {
        var scope = await accessService.GetScopeAsync();
        ApplyScope(poi, scope, isCreate: true);

        if (!ModelState.IsValid)
        {
            await PopulateTenantOptionsAsync(poi.TenantId);
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
        var poi = await FindAuthorizedPoiAsync(id);
        await PopulateTenantOptionsAsync(poi?.TenantId);
        return poi is null ? NotFound() : View(poi);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Poi poi)
    {
        var scope = await accessService.GetScopeAsync();
        ApplyScope(poi, scope, isCreate: false);

        if (id != poi.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateTenantOptionsAsync(poi.TenantId);
            return View(poi);
        }

        var existingPoi = await FindAuthorizedPoiAsync(id);
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
        existingPoi.TenantId = poi.TenantId;
        existingPoi.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var poi = await FindAuthorizedPoiAsync(id);
        return poi is null ? NotFound() : View(poi);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var poi = await FindAuthorizedPoiAsync(id);
        if (poi is null)
        {
            return NotFound();
        }

        dbContext.Pois.Remove(poi);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task<Poi?> FindAuthorizedPoiAsync(Guid id)
    {
        var scope = await accessService.GetScopeAsync();
        var query = dbContext.Pois.AsQueryable();

        if (!scope.IsSystemAdmin)
        {
            query = query.Where(p => p.TenantId == scope.TenantId);
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    private static void ApplyScope(Poi poi, CmsAccessScope scope, bool isCreate)
    {
        if (!scope.IsSystemAdmin)
        {
            poi.TenantId = scope.TenantId;
            return;
        }

        if (isCreate && poi.TenantId is null)
        {
            poi.TenantId = PoiTenant.DemoTenantId;
        }
    }

    private async Task PopulateTenantOptionsAsync(Guid? selectedTenantId = null)
    {
        var scope = await accessService.GetScopeAsync();
        var tenants = await dbContext.PoiTenants
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();

        if (!scope.IsSystemAdmin)
        {
            tenants = tenants.Where(t => t.Id == scope.TenantId).ToList();
            selectedTenantId = scope.TenantId;
        }

        ViewBag.IsSystemAdmin = scope.IsSystemAdmin;
        ViewBag.TenantOptions = new SelectList(tenants, nameof(PoiTenant.Id), nameof(PoiTenant.Name), selectedTenantId);
    }
}
