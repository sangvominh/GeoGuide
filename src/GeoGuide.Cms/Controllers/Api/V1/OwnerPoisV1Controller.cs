using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Models.Api;
using GeoGuide.Cms.Security;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/owner/pois")]
[Authorize(Roles = CmsRoles.PoiTenant)]
public class OwnerPoisV1Controller(
    ApplicationDbContext dbContext,
    CmsAccessService accessService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePoi([FromBody] OwnerPoiCreateRequestDto dto)
    {
        var scope = await accessService.GetScopeAsync();
        if (!scope.TenantId.HasValue)
        {
            return Forbid();
        }

        var poi = new Poi
        {
            Id = Guid.NewGuid(),
            TenantId = scope.TenantId.Value,
            Name = dto.Name,
            Description = dto.Description,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            TriggerRadiusMeters = dto.TriggerRadiusMeters,
            CooldownMinutes = dto.CooldownMinutes,
            CategoryKey = dto.CategoryKey,
            CategoryLabel = dto.CategoryLabel,
            ImageUrl = dto.ImageUrl,
            MapUrl = dto.MapUrl,
            ApprovalStatus = PoiApprovalStatus.PendingApproval,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Pois.Add(poi);
        await dbContext.SaveChangesAsync();

        return Ok(poi);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePoi(Guid id, [FromBody] OwnerPoiUpdateRequestDto dto)
    {
        var scope = await accessService.GetScopeAsync();
        if (!scope.TenantId.HasValue)
        {
            return Forbid();
        }

        var poi = await dbContext.Pois.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == scope.TenantId.Value);
        if (poi == null)
        {
            return NotFound();
        }

        poi.Name = dto.Name;
        poi.Description = dto.Description;
        poi.Latitude = dto.Latitude;
        poi.Longitude = dto.Longitude;
        poi.TriggerRadiusMeters = dto.TriggerRadiusMeters;
        poi.CooldownMinutes = dto.CooldownMinutes;
        poi.CategoryKey = dto.CategoryKey;
        poi.CategoryLabel = dto.CategoryLabel;
        poi.ImageUrl = dto.ImageUrl;
        poi.MapUrl = dto.MapUrl;
        poi.UpdatedAt = DateTimeOffset.UtcNow;
        poi.ApprovalStatus = PoiApprovalStatus.PendingApproval;
        poi.IsActive = false;

        await dbContext.SaveChangesAsync();

        return Ok(poi);
    }
}
