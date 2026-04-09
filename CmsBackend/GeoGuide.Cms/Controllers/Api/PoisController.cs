using GeoGuide.Cms.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PoisController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pois = await dbContext.Pois
            .Where(p => p.IsActive)
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return Ok(pois);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var poi = await dbContext.Pois.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        return poi is null ? NotFound() : Ok(poi);
    }
}
