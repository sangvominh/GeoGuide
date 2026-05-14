using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/logs")]
public class BehaviorLogsV1Controller(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost("behavior")]
    public async Task<IActionResult> Create([FromBody] BehaviorEventRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        Guid? poiId = null;
        if (!string.IsNullOrWhiteSpace(request.PoiId))
        {
            if (!Guid.TryParse(request.PoiId, out var parsedPoiId))
            {
                ModelState.AddModelError(nameof(request.PoiId), "POI không hợp lệ.");
                return ValidationProblem(ModelState);
            }

            var poiExists = await dbContext.Pois.AnyAsync(p => p.Id == parsedPoiId);
            if (!poiExists)
            {
                ModelState.AddModelError(nameof(request.PoiId), "POI không tồn tại.");
                return ValidationProblem(ModelState);
            }

            poiId = parsedPoiId;
        }

        var occurredAt = request.OccurredAt == default ? DateTimeOffset.UtcNow : request.OccurredAt;
        var sessionToken = string.IsNullOrWhiteSpace(request.SessionToken) ? null : request.SessionToken.Trim();
        var deviceId = request.DeviceId.Trim();
        var clientType = string.IsNullOrWhiteSpace(request.ClientType) ? "mobile" : request.ClientType.Trim().ToLowerInvariant();

        dbContext.BehaviorEvents.Add(new BehaviorEvent
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            PoiId = poiId,
            EventType = request.EventType,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            DurationSeconds = request.DurationSeconds,
            OccurredAt = occurredAt,
            SessionToken = sessionToken,
            ClientType = clientType
        });

        if (!string.IsNullOrWhiteSpace(sessionToken))
        {
            var existingJoin = await dbContext.DeviceSessionJoins
                .FirstOrDefaultAsync(row => row.SessionToken == sessionToken && row.DeviceId == deviceId);

            if (existingJoin == null)
            {
                dbContext.DeviceSessionJoins.Add(new DeviceSessionJoin
                {
                    Id = Guid.NewGuid(),
                    SessionToken = sessionToken,
                    DeviceId = deviceId,
                    ClientType = clientType,
                    AccessMode = "trial",
                    JoinedAt = occurredAt,
                    LastSeenAt = occurredAt
                });
            }
            else
            {
                existingJoin.ClientType = clientType;
                existingJoin.LastSeenAt = occurredAt;
            }
        }

        await dbContext.SaveChangesAsync();

        return Accepted(new
        {
            message = "Đã ghi nhận hành vi người dùng.",
            occurredAt
        });
    }
}
