using MauiApp1.Models;

namespace MauiApp1.Services;

public class NarrationService
{
    private const string DeviceIdPreferenceKey = "mobile_device_id";
    private readonly PoiApiService _poiApiService;

    public NarrationService(PoiApiService poiApiService)
    {
        _poiApiService = poiApiService;
    }

    public async Task PlayAsync(
        PointOfInterest poi,
        string triggerType,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var narrationText = BuildNarrationText(poi);

        var locale = await ResolveLocaleAsync(poi.LanguageCode);
        await SpeakWithFallbackAsync(narrationText, locale, cancellationToken);

        _ = TryLogPlaybackAsync(new PlaybackLogEntry
        {
            PoiId = poi.Id,
            PlayedAt = DateTimeOffset.UtcNow,
            TriggerType = triggerType,
            DurationSeconds = Math.Max(1, (int)Math.Round((DateTimeOffset.UtcNow - startedAt).TotalSeconds)),
            DeviceId = GetOrCreateDeviceId()
        });
    }

    private async Task<Locale?> ResolveLocaleAsync(string languageCode)
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        var normalizedCode = (languageCode ?? string.Empty).Trim();
        var languagePrefix = normalizedCode.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];

        return locales.FirstOrDefault(locale => string.Equals(locale.Language, normalizedCode, StringComparison.OrdinalIgnoreCase))
            ?? locales.FirstOrDefault(locale => locale.Language.StartsWith(languagePrefix, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task SpeakWithFallbackAsync(string narrationText, Locale? locale, CancellationToken cancellationToken)
    {
        try
        {
            await TextToSpeech.Default.SpeakAsync(narrationText, new SpeechOptions
            {
                Locale = locale,
                Pitch = 1.0f,
                Volume = 1.0f
            }, cancellationToken);
        }
        catch when (locale != null)
        {
            await TextToSpeech.Default.SpeakAsync(narrationText, new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 1.0f
            }, cancellationToken);
        }
    }

    private static string BuildNarrationText(PointOfInterest poi)
    {
        if (!string.IsNullOrWhiteSpace(poi.TtsScript))
        {
            return poi.TtsScript.Trim();
        }

        var parts = new List<string> { poi.Name.Trim() };

        if (!string.IsNullOrWhiteSpace(poi.Description))
        {
            parts.Add(poi.Description.Trim());
        }

        if (!string.IsNullOrWhiteSpace(poi.CategoryLabel))
        {
            parts.Add($"Đây là điểm {poi.CategoryLabel.ToLowerInvariant()} nổi bật trong khu vực.");
        }

        return string.Join(". ", parts.Where(static part => !string.IsNullOrWhiteSpace(part)));
    }

    private async Task TryLogPlaybackAsync(PlaybackLogEntry entry)
    {
        try
        {
            await _poiApiService.PostPlaybackLogAsync(entry);
        }
        catch
        {
        }
    }

    private static string GetOrCreateDeviceId()
    {
        var existing = Preferences.Default.Get(DeviceIdPreferenceKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var created = Guid.NewGuid().ToString("N");
        Preferences.Default.Set(DeviceIdPreferenceKey, created);
        return created;
    }
}
