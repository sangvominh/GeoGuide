using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/sessions")]
public class SessionsV1Controller(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] SessionJoinRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var normalizedSessionToken = request.SessionToken.Trim();
        var normalizedDeviceId = request.DeviceId.Trim();
        var joinedAt = request.JoinedAt == default ? DateTimeOffset.UtcNow : request.JoinedAt;

        var existingJoin = await dbContext.DeviceSessionJoins
            .FirstOrDefaultAsync(row => row.SessionToken == normalizedSessionToken && row.DeviceId == normalizedDeviceId);

        if (existingJoin == null)
        {
            existingJoin = new DeviceSessionJoin
            {
                Id = Guid.NewGuid(),
                SessionToken = normalizedSessionToken,
                DeviceId = normalizedDeviceId,
                ClientType = NormalizeClientType(request.ClientType),
                AccessMode = NormalizeAccessMode(request.AccessMode),
                JoinedAt = joinedAt,
                LastSeenAt = DateTimeOffset.UtcNow
            };

            dbContext.DeviceSessionJoins.Add(existingJoin);
        }
        else
        {
            existingJoin.ClientType = NormalizeClientType(request.ClientType);
            existingJoin.AccessMode = NormalizeAccessMode(request.AccessMode);
            existingJoin.LastSeenAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync();

        return Ok(new SessionJoinResponseDto
        {
            SessionToken = existingJoin.SessionToken,
            DeviceId = existingJoin.DeviceId,
            ClientType = existingJoin.ClientType,
            AccessMode = existingJoin.AccessMode,
            JoinedAt = existingJoin.JoinedAt,
            ServerTime = DateTimeOffset.UtcNow
        });
    }

    private static string NormalizeClientType(string? clientType)
    {
        return string.Equals(clientType, "web", StringComparison.OrdinalIgnoreCase) ? "web" : "mobile";
    }

    private static string NormalizeAccessMode(string? accessMode)
    {
        return string.Equals(accessMode, "full", StringComparison.OrdinalIgnoreCase) ? "full" : "trial";
    }
}
