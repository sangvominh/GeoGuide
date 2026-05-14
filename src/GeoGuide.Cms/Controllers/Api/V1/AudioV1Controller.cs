using GeoGuide.Cms.Models.Api;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/audio")]
public class AudioV1Controller(AudioLocalizationService audioService) : ControllerBase
{
    [HttpGet("voices")]
    public IActionResult Voices()
    {
        return Ok(audioService.GetVoices());
    }

    [HttpPost("tts")]
    public async Task<IActionResult> Tts(TtsRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(ModelState);
        }

        var asset = await audioService.GenerateTtsAsync(request, cancellationToken);
        return Ok(new
        {
            asset,
            provider = "local-demo-placeholder",
            note = "Generated deterministic WAV compatible demo placeholder; no external TTS service was called."
        });
    }

    [HttpGet("pack-manifest")]
    public async Task<IActionResult> PackManifest([FromQuery] string? lang, CancellationToken cancellationToken)
    {
        return Ok(await audioService.GetPackManifestAsync(lang, cancellationToken));
    }
}
