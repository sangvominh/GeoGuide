using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/logs")]
public class PlaybackLogsV1Controller(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost("playback")]
    public async Task<IActionResult> Create([FromBody] PlaybackLogRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var poiExists = await dbContext.Pois.AnyAsync(p => p.Id == request.PoiId);
        if (!poiExists)
        {
            ModelState.AddModelError(nameof(request.PoiId), "POI không tồn tại.");
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
            message = "Đã ghi nhận nhật ký phát thuyết minh.",
            logId = log.Id
        });
    }
}
