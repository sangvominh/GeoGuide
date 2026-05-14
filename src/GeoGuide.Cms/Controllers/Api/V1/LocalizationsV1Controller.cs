using GeoGuide.Cms.Models.Api;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/localizations")]
public class LocalizationsV1Controller(AudioLocalizationService localizationService) : ControllerBase
{
    [HttpPost("prepare-hotset")]
    public async Task<IActionResult> PrepareHotset(
        PrepareLocalizationHotsetRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await localizationService.PrepareHotsetAsync(request, cancellationToken));
    }

    [HttpPost("on-demand")]
    public async Task<IActionResult> OnDemand(
        LocalizationOnDemandRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await localizationService.OnDemandAsync(request, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("warmup")]
    public async Task<IActionResult> Warmup(
        LocalizationWarmupRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await localizationService.WarmupAsync(request, cancellationToken));
    }

    [HttpGet("warmup/{lang}/status")]
    public IActionResult WarmupStatus(string lang)
    {
        return Ok(localizationService.GetWarmupStatus(lang));
    }

    [HttpGet("tasks/{taskId}/status")]
    public IActionResult TaskStatus(string taskId)
    {
        var status = localizationService.GetTaskStatus(taskId);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpGet("tasks/{taskId}/events")]
    public IActionResult TaskEvents(string taskId)
    {
        var status = localizationService.GetTaskStatus(taskId);
        if (status is null)
        {
            return NotFound();
        }

        Response.Headers.ContentType = "text/event-stream";
        return Content($"event: status\ndata: {System.Text.Json.JsonSerializer.Serialize(status)}\n\n");
    }
}
