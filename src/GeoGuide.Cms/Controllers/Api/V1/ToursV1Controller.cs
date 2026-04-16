using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/tours")]
public class ToursV1Controller(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tours = await dbContext.Tours
            .Where(t => t.IsActive)
            .Include(t => t.PoiMappings)
            .OrderBy(t => t.Name)
            .ToListAsync();

        return Ok(tours.Select(MapTour));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tour = await dbContext.Tours
            .Where(t => t.Id == id && t.IsActive)
            .Include(t => t.PoiMappings)
            .FirstOrDefaultAsync();

        return tour is null ? NotFound() : Ok(MapTour(tour));
    }

    private static TourDto MapTour(GeoGuide.Cms.Models.Tour tour)
    {
        return new TourDto
        {
            Id = tour.Id,
            Name = tour.Name,
            Description = tour.Description,
            ThumbnailUrl = tour.ThumbnailUrl,
            IsActive = tour.IsActive,
            IsDeleted = tour.IsDeleted,
            UpdatedAt = tour.UpdatedAt,
            Pois = tour.PoiMappings
                .OrderBy(m => m.OrderIndex)
                .Select(m => new TourPoiDto
                {
                    PoiId = m.PoiId,
                    OrderIndex = m.OrderIndex
                })
                .ToList()
        };
    }
}
