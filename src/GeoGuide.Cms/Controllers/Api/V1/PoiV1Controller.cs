using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Models.Api;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/poi")]
public class PoiV1Controller(ApplicationDbContext dbContext, CmsAccessService accessService) : ControllerBase
{
    [HttpGet("load-all")]
    public async Task<IActionResult> LoadAll([FromQuery] string? lang = "en")
    {
        var pois = await ActivePois()
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return Ok(pois.Select(p => PoiApiMapper.MapPoi(p, lang)));
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> Nearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radiusMeters = 1500,
        [FromQuery] string? lang = "en")
    {
        if (lat is < -90 or > 90 || lng is < -180 or > 180 || radiusMeters <= 0)
        {
            return BadRequest("lat, lng, and radiusMeters must describe a valid search area.");
        }

        var pois = await ActivePois().ToListAsync();
        var nearbyPois = pois
            .Select(p => new
            {
                Poi = p,
                DistanceMeters = PoiApiMapper.CalculateDistanceMeters(lat, lng, p.Latitude, p.Longitude)
            })
            .Where(p => p.DistanceMeters <= radiusMeters)
            .OrderBy(p => p.DistanceMeters)
            .ThenBy(p => p.Poi.Priority)
            .Select(p => PoiApiMapper.MapPoi(p.Poi, lang, Math.Round(p.DistanceMeters, 2)))
            .ToList();

        return Ok(nearbyPois);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] string? lang = "en")
    {
        var poi = await ActivePois().FirstOrDefaultAsync(p => p.Id == id);
        return poi is null ? NotFound() : Ok(PoiApiMapper.MapPoi(poi, lang));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(PoiUpsertDto request, [FromQuery] string? lang = "en")
    {
        var scope = await accessService.GetScopeAsync();
        var now = DateTimeOffset.UtcNow;
        var poi = new Poi
        {
            Id = Guid.NewGuid(),
            CreatedAt = now,
            UpdatedAt = now
        };

        ApplyRequest(poi, request);
        ApplyScope(poi, scope, isCreate: true);

        AddContents(poi, request.Contents, now);

        dbContext.Pois.Add(poi);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = poi.Id, lang }, PoiApiMapper.MapPoi(poi, lang));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PoiUpsertDto request, [FromQuery] string? lang = "en")
    {
        var scope = await accessService.GetScopeAsync();
        var poi = await FindAuthorizedPoiAsync(id, scope);
        if (poi is null)
        {
            return NotFound();
        }

        ApplyRequest(poi, request);
        ApplyScope(poi, scope, isCreate: false);
        poi.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.Contents is not null)
        {
            ReplaceContents(poi, request.Contents, poi.UpdatedAt);
        }

        await dbContext.SaveChangesAsync();
        return Ok(PoiApiMapper.MapPoi(poi, lang));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var scope = await accessService.GetScopeAsync();
        var poi = await FindAuthorizedPoiAsync(id, scope);
        if (poi is null)
        {
            return NotFound();
        }

        var now = DateTimeOffset.UtcNow;
        poi.IsDeleted = true;
        poi.IsActive = false;
        poi.UpdatedAt = now;

        foreach (var content in poi.Audios)
        {
            content.IsDeleted = true;
            content.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<Poi> ActivePois()
    {
        return dbContext.Pois
            .Where(p => p.IsActive)
            .Include(p => p.Audios);
    }

    private async Task<Poi?> FindAuthorizedPoiAsync(Guid id, CmsAccessScope scope)
    {
        var query = dbContext.Pois
            .Include(p => p.Audios)
            .AsQueryable();

        if (!scope.IsSystemAdmin)
        {
            query = query.Where(p => p.TenantId == scope.TenantId);
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    private static void ApplyRequest(Poi poi, PoiUpsertDto request)
    {
        poi.Name = request.Name;
        poi.Description = request.Description;
        poi.Latitude = request.Latitude;
        poi.Longitude = request.Longitude;
        poi.TriggerRadiusMeters = request.TriggerRadiusMeters;
        poi.CooldownMinutes = request.CooldownMinutes;
        poi.Priority = request.Priority;
        poi.CategoryKey = request.CategoryKey;
        poi.CategoryLabel = request.CategoryLabel;
        poi.ImageUrl = request.ImageUrl;
        poi.MapUrl = request.MapUrl;
        poi.TenantId = request.TenantId;
        poi.IsActive = request.IsActive;
    }

    private static void ApplyScope(Poi poi, CmsAccessScope scope, bool isCreate)
    {
        if (!scope.IsSystemAdmin)
        {
            poi.TenantId = scope.TenantId;
            poi.ApprovalStatus = PoiApprovalStatus.PendingApproval;
            poi.IsActive = false;
            return;
        }

        if (isCreate && poi.TenantId is null)
        {
            poi.TenantId = PoiTenant.DemoTenantId;
        }

        if (isCreate)
        {
            poi.ApprovalStatus = PoiApprovalStatus.Approved;
        }
    }

    private static void ReplaceContents(Poi poi, IReadOnlyList<PoiContentUpsertDto> contents, DateTimeOffset now)
    {
        foreach (var existingContent in poi.Audios)
        {
            existingContent.IsDeleted = true;
            existingContent.UpdatedAt = now;
        }

        AddContents(poi, contents, now);
    }

    private static void AddContents(Poi poi, IReadOnlyList<PoiContentUpsertDto>? contents, DateTimeOffset now)
    {
        foreach (var content in contents ?? [])
        {
            if (!Enum.TryParse<PoiContentType>(content.ContentType, ignoreCase: true, out var contentType))
            {
                contentType = PoiContentType.TtsScript;
            }

            poi.Audios.Add(new PoiAudio
            {
                Id = Guid.NewGuid(),
                PoiId = poi.Id,
                LanguageCode = content.LanguageCode.Trim().ToLowerInvariant(),
                ContentType = contentType,
                AudioUrl = content.AudioUrl,
                TtsContent = content.TtsContent,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }
}
