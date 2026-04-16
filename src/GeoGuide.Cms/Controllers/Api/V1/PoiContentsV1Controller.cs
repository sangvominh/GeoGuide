using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/pois/{poiId:guid}/contents")]
public class PoiContentsV1Controller(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByPoi(Guid poiId)
    {
        var poiExists = await dbContext.Pois.AnyAsync(p => p.Id == poiId && p.IsActive);
        if (!poiExists)
        {
            return NotFound();
        }

        var contents = await dbContext.PoiAudios
            .Where(c => c.PoiId == poiId)
            .OrderBy(c => c.LanguageCode)
            .ThenBy(c => c.ContentType)
            .Select(c => new PoiContentDto
            {
                Id = c.Id,
                LanguageCode = c.LanguageCode,
                ContentType = c.ContentType.ToString(),
                AudioUrl = c.AudioUrl,
                TtsContent = c.TtsContent,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return Ok(contents);
    }
}
