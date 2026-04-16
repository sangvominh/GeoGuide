using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api;

[ApiController]
[Route("api/logs")]
public class LogsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost("playback")]
    public async Task<IActionResult> CreatePlaybackLog([FromBody] PlaybackLogRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var poiExists = await dbContext.Pois.AnyAsync(p => p.Id == request.PoiId);
        if (!poiExists)
        {
            ModelState.AddModelError(nameof(request.PoiId), "POI does not exist.");
            return ValidationProblem(ModelState);
        }

        var log = new PlaybackLog
        {
            Id = Guid.NewGuid(),
            PoiId = request.PoiId,
            PlayedAt = request.PlayedAt == default ? DateTimeOffset.UtcNow : request.PlayedAt,
            TriggerType = request.TriggerType,
            DurationSeconds = request.DurationSeconds,
            DeviceId = request.DeviceId
        };

        dbContext.PlaybackLogs.Add(log);
        await dbContext.SaveChangesAsync();

        return Accepted(new
        {
            message = "Playback log accepted.",
            logId = log.Id
        });
    }
}
