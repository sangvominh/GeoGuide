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
        var narrationText = string.IsNullOrWhiteSpace(poi.NarrationText)
            ? poi.Name
            : poi.NarrationText;

        var locale = await ResolveLocaleAsync(poi.LanguageCode);
        await TextToSpeech.Default.SpeakAsync(narrationText, new SpeechOptions
        {
            Locale = locale,
            Pitch = 1.0f,
            Volume = 1.0f
        }, cancellationToken);

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
        return locales.FirstOrDefault(locale => string.Equals(locale.Language, languageCode, StringComparison.OrdinalIgnoreCase))
            ?? locales.FirstOrDefault(locale => locale.Language.StartsWith(languageCode.Split('-')[0], StringComparison.OrdinalIgnoreCase));
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
