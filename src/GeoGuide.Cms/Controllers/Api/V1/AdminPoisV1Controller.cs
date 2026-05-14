using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/admin/pois")]
[Authorize(Roles = CmsRoles.SystemAdmin)]
public class AdminPoisV1Controller(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApprovePoi(Guid id)
    {
        var poi = await dbContext.Pois.FirstOrDefaultAsync(p => p.Id == id);
        if (poi == null)
        {
            return NotFound();
        }

        poi.ApprovalStatus = PoiApprovalStatus.Approved;
        poi.IsActive = true;
        poi.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(poi);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectPoi(Guid id)
    {
        var poi = await dbContext.Pois.FirstOrDefaultAsync(p => p.Id == id);
        if (poi == null)
        {
            return NotFound();
        }

        poi.ApprovalStatus = PoiApprovalStatus.Rejected;
        poi.IsActive = false;
        poi.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(poi);
    }
}
