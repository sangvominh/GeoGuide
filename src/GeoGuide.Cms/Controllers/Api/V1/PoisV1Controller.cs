using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/pois")]
public class PoisV1Controller(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pois = await dbContext.Pois
            .Where(p => p.IsActive)
            .Include(p => p.Audios)
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return Ok(pois.Select(MapPoi));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var poi = await dbContext.Pois
            .Where(p => p.Id == id && p.IsActive)
            .Include(p => p.Audios)
            .FirstOrDefaultAsync();

        return poi is null ? NotFound() : Ok(MapPoi(poi));
    }

    private static PoiDto MapPoi(GeoGuide.Cms.Models.Poi poi)
    {
        return new PoiDto
        {
            Id = poi.Id,
            Name = poi.Name,
            Description = poi.Description,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            TriggerRadiusMeters = poi.TriggerRadiusMeters,
            CooldownMinutes = poi.CooldownMinutes,
            Priority = poi.Priority,
            CategoryKey = poi.CategoryKey,
            CategoryLabel = poi.CategoryLabel,
            ImageUrl = poi.ImageUrl,
            MapUrl = poi.MapUrl,
            IsActive = poi.IsActive,
            IsDeleted = poi.IsDeleted,
            UpdatedAt = poi.UpdatedAt,
            Contents = poi.Audios
                .OrderBy(a => a.LanguageCode)
                .Select(a => new PoiContentDto
                {
                    Id = a.Id,
                    LanguageCode = a.LanguageCode,
                    ContentType = a.ContentType.ToString(),
                    AudioUrl = a.AudioUrl,
                    TtsContent = a.TtsContent,
                    UpdatedAt = a.UpdatedAt
                })
                .ToList()
        };
    }
}
