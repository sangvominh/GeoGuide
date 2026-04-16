using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class PoiContentsAdminController(
    ApplicationDbContext dbContext,
    CmsAccessService accessService,
    IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(Guid poiId)
    {
        var poi = await FindAuthorizedPoiAsync(poiId);
        if (poi is null)
        {
            return NotFound();
        }

        var scope = await accessService.GetScopeAsync();
        var contents = await dbContext.PoiAudios
            .Where(c => c.PoiId == poiId)
            .OrderBy(c => c.LanguageCode)
            .ThenBy(c => c.ContentType)
            .ToListAsync();

        return View(new PoiContentsAdminIndexViewModel
        {
            Poi = poi,
            Contents = contents,
            IsSystemAdmin = scope.IsSystemAdmin
        });
    }

    public async Task<IActionResult> Create(Guid poiId)
    {
        var poi = await FindAuthorizedPoiAsync(poiId);
        if (poi is null)
        {
            return NotFound();
        }

        ViewBag.PoiName = poi.Name;
        ViewBag.PoiId = poi.Id;

        return View(new PoiAudio
        {
            PoiId = poi.Id,
            LanguageCode = "vi",
            ContentType = PoiContentType.TtsScript
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PoiAudio content)
    {
        var poi = await FindAuthorizedPoiAsync(content.PoiId);
        if (poi is null)
        {
            return NotFound();
        }

        ValidateContent(content, ModelState);
        if (!ModelState.IsValid)
        {
            ViewBag.PoiName = poi.Name;
            ViewBag.PoiId = poi.Id;
            return View(content);
        }

        await ApplyAudioUploadAsync(content);
        content.Id = Guid.NewGuid();
        content.CreatedAt = DateTimeOffset.UtcNow;
        content.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.PoiAudios.Add(content);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { poiId = content.PoiId });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var content = await dbContext.PoiAudios.FirstOrDefaultAsync(c => c.Id == id);
        if (content is null)
        {
            return NotFound();
        }

        var poi = await FindAuthorizedPoiAsync(content.PoiId);
        if (poi is null)
        {
            return NotFound();
        }

        ViewBag.PoiName = poi.Name;
        ViewBag.PoiId = poi.Id;
        return View(content);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PoiAudio content)
    {
        if (id != content.Id)
        {
            return NotFound();
        }

        var existing = await dbContext.PoiAudios.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null)
        {
            return NotFound();
        }

        var poi = await FindAuthorizedPoiAsync(existing.PoiId);
        if (poi is null)
        {
            return NotFound();
        }

        ValidateContent(content, ModelState);
        if (!ModelState.IsValid)
        {
            ViewBag.PoiName = poi.Name;
            ViewBag.PoiId = poi.Id;
            return View(content);
        }

        await ApplyAudioUploadAsync(content);
        existing.LanguageCode = content.LanguageCode;
        existing.ContentType = content.ContentType;
        existing.AudioUrl = content.AudioUrl;
        existing.TtsContent = content.TtsContent;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { poiId = existing.PoiId });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var content = await dbContext.PoiAudios.Include(c => c.Poi).FirstOrDefaultAsync(c => c.Id == id);
        if (content is null)
        {
            return NotFound();
        }

        var poi = await FindAuthorizedPoiAsync(content.PoiId);
        if (poi is null)
        {
            return NotFound();
        }

        return View(content);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var content = await dbContext.PoiAudios.FirstOrDefaultAsync(c => c.Id == id);
        if (content is null)
        {
            return NotFound();
        }

        var poi = await FindAuthorizedPoiAsync(content.PoiId);
        if (poi is null)
        {
            return NotFound();
        }

        content.IsDeleted = true;
        content.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { poiId = content.PoiId });
    }

    private static void ValidateContent(PoiAudio content, ModelStateDictionary modelState)
    {
        if (content.ContentType == PoiContentType.AudioFile
            && string.IsNullOrWhiteSpace(content.AudioUrl)
            && content.UploadFile is null)
        {
            modelState.AddModelError(nameof(PoiAudio.AudioUrl), "UploadFile or AudioUrl is required for AudioFile content.");
        }

        if (content.ContentType == PoiContentType.TtsScript && string.IsNullOrWhiteSpace(content.TtsContent))
        {
            modelState.AddModelError(nameof(PoiAudio.TtsContent), "TtsContent is required for TtsScript content.");
        }
    }

    private async Task ApplyAudioUploadAsync(PoiAudio content)
    {
        if (content.ContentType != PoiContentType.AudioFile || content.UploadFile is null || content.UploadFile.Length == 0)
        {
            return;
        }

        var uploadsRoot = Path.Combine(environment.WebRootPath, "uploads", "poi-audios");
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(content.UploadFile.FileName);
        var fileName = $"{content.PoiId:N}-{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await content.UploadFile.CopyToAsync(stream);

        content.AudioUrl = $"/uploads/poi-audios/{fileName}";
    }

    private async Task<Poi?> FindAuthorizedPoiAsync(Guid poiId)
    {
        var scope = await accessService.GetScopeAsync();
        var query = dbContext.Pois.AsQueryable();

        if (!scope.IsSystemAdmin)
        {
            query = query.Where(p => p.TenantId == scope.TenantId);
        }

        return await query.FirstOrDefaultAsync(p => p.Id == poiId);
    }
}
