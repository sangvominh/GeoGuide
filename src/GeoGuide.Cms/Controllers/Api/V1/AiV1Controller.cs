using System.Security.Claims;
using GeoGuide.Cms.Models.Api.V1;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/ai")]
public class AiV1Controller : ControllerBase
{
    private readonly IAiAdvisorService _aiAdvisorService;

    public AiV1Controller(IAiAdvisorService aiAdvisorService)
    {
        _aiAdvisorService = aiAdvisorService;
    }

    [HttpPost("enhance-description")]
    public async Task<IActionResult> EnhanceDescription([FromBody] EnhanceDescriptionRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString() 
                         ?? "anonymous";

            var response = await _aiAdvisorService.EnhanceDescriptionAsync(request, userId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(429, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred processing the enhancement request." });
        }
    }
}
