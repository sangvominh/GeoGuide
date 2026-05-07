using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/sync")]
public class SyncV1Controller(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("bootstrap")]
    public async Task<IActionResult> Bootstrap()
    {
        var pois = await dbContext.Pois
            .Where(p => p.IsActive)
            .Include(p => p.Audios)
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync();

        var tours = await dbContext.Tours
            .Where(t => t.IsActive)
            .Include(t => t.PoiMappings)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var response = new SyncBootstrapDto
        {
            ServerTime = DateTimeOffset.UtcNow,
            Pois = pois.Select(p => new PoiDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                TriggerRadiusMeters = p.TriggerRadiusMeters,
                CooldownMinutes = p.CooldownMinutes,
                Priority = p.Priority,
                CategoryKey = p.CategoryKey,
                        CategoryLabel = p.CategoryLabel,
                        ImageUrl = p.ImageUrl,
                        MapUrl = p.MapUrl,
                        IsActive = p.IsActive,
                        IsDeleted = p.IsDeleted,
                        UpdatedAt = p.UpdatedAt,
                        Contents = p.Audios
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
            }).ToList(),
            Tours = tours.Select(t => new TourDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                ThumbnailUrl = t.ThumbnailUrl,
                IsActive = t.IsActive,
                IsDeleted = t.IsDeleted,
                UpdatedAt = t.UpdatedAt,
                Pois = t.PoiMappings
                    .OrderBy(m => m.OrderIndex)
                    .Select(m => new TourPoiDto
                    {
                        PoiId = m.PoiId,
                        OrderIndex = m.OrderIndex
                    })
                    .ToList()
            }).ToList()
        };

        return Ok(response);
    }

    [HttpGet("delta")]
    public async Task<IActionResult> Delta([FromQuery] DateTimeOffset? lastSyncAt = null)
    {
        var poiQuery = dbContext.Pois
            .IgnoreQueryFilters()
            .Include(p => p.Audios)
            .AsQueryable();

        var tourQuery = dbContext.Tours
            .IgnoreQueryFilters()
            .Include(t => t.PoiMappings)
            .AsQueryable();

        if (lastSyncAt.HasValue)
        {
            poiQuery = poiQuery.Where(p => p.UpdatedAt >= lastSyncAt.Value);
            tourQuery = tourQuery.Where(t => t.UpdatedAt >= lastSyncAt.Value);
        }
        else
        {
            poiQuery = poiQuery.Where(p => !p.IsDeleted);
            tourQuery = tourQuery.Where(t => !t.IsDeleted);
        }

        var pois = await poiQuery
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync();

        var tours = await tourQuery
            .OrderBy(t => t.Name)
            .ToListAsync();

        var response = new SyncBootstrapDto
        {
            ServerTime = DateTimeOffset.UtcNow,
            Pois = pois.Select(p => new PoiDto
            {
                Id = p.Id,
                Name = p.IsDeleted ? string.Empty : p.Name,
                Description = p.IsDeleted ? string.Empty : p.Description,
                Latitude = p.IsDeleted ? 0 : p.Latitude,
                Longitude = p.IsDeleted ? 0 : p.Longitude,
                TriggerRadiusMeters = p.IsDeleted ? 0 : p.TriggerRadiusMeters,
                CooldownMinutes = p.IsDeleted ? 0 : p.CooldownMinutes,
                Priority = p.IsDeleted ? 0 : p.Priority,
                CategoryKey = p.IsDeleted ? string.Empty : p.CategoryKey,
                CategoryLabel = p.IsDeleted ? string.Empty : p.CategoryLabel,
                ImageUrl = p.IsDeleted ? string.Empty : p.ImageUrl,
                MapUrl = p.IsDeleted ? string.Empty : p.MapUrl,
                IsActive = p.IsActive,
                IsDeleted = p.IsDeleted,
                UpdatedAt = p.UpdatedAt,
                Contents = p.IsDeleted
                    ? []
                    : p.Audios
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
            }).ToList(),
            Tours = tours.Select(t => new TourDto
            {
                Id = t.Id,
                Name = t.IsDeleted ? string.Empty : t.Name,
                Description = t.IsDeleted ? string.Empty : t.Description,
                ThumbnailUrl = t.IsDeleted ? string.Empty : t.ThumbnailUrl,
                IsActive = t.IsActive,
                IsDeleted = t.IsDeleted,
                UpdatedAt = t.UpdatedAt,
                Pois = t.IsDeleted
                    ? []
                    : t.PoiMappings
                        .OrderBy(m => m.OrderIndex)
                        .Select(m => new TourPoiDto
                        {
                            PoiId = m.PoiId,
                            OrderIndex = m.OrderIndex
                        })
                        .ToList()
            }).ToList()
        };

        return Ok(response);
    }
}
