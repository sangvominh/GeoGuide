using MauiApp1.Models;

namespace MauiApp1.Services;

public class NarrationService
{
    private const string DeviceIdPreferenceKey = "mobile_device_id";
    private readonly PoiApiService _poiApiService;
    private readonly MediaPrefetchService _mediaPrefetchService;

    public NarrationService(PoiApiService poiApiService, MediaPrefetchService mediaPrefetchService)
    {
        _poiApiService = poiApiService;
        _mediaPrefetchService = mediaPrefetchService;
    }

    public async Task PlayAsync(
        PointOfInterest poi,
        string triggerType,
        Func<Uri, CancellationToken, Task>? playAudioAsync = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var audioUri = await _mediaPrefetchService.ResolvePlaybackUriAsync(poi.AudioUrl, cancellationToken);
        if (audioUri != null && playAudioAsync != null)
        {
            await playAudioAsync(audioUri, cancellationToken);
            _ = TryLogPlaybackAsync(CreateLogEntry(poi, triggerType, startedAt));
            return;
        }

        var narrationText = BuildNarrationText(poi);
        var locale = await ResolveLocaleAsync(poi.LanguageCode);
        EnsureSupportedVoice(poi.LanguageCode, locale);
        await SpeakWithFallbackAsync(narrationText, locale, cancellationToken);

        _ = TryLogPlaybackAsync(CreateLogEntry(poi, triggerType, startedAt));
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
        if (locale == null)
        {
            await TextToSpeech.Default.SpeakAsync(narrationText, new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 1.0f
            }, cancellationToken);
            return;
        }

        await TextToSpeech.Default.SpeakAsync(narrationText, new SpeechOptions
        {
            Locale = locale,
            Pitch = 1.0f,
            Volume = 1.0f
        }, cancellationToken);
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

    private PlaybackLogEntry CreateLogEntry(PointOfInterest poi, string triggerType, DateTimeOffset startedAt)
    {
        return new PlaybackLogEntry
        {
            PoiId = poi.Id,
            PlayedAt = DateTimeOffset.UtcNow,
            TriggerType = triggerType,
            DurationSeconds = Math.Max(1, (int)Math.Round((DateTimeOffset.UtcNow - startedAt).TotalSeconds)),
            DeviceId = GetOrCreateDeviceId()
        };
    }

    private static void EnsureSupportedVoice(string languageCode, Locale? locale)
    {
        var normalizedCode = (languageCode ?? string.Empty).Trim();
        if (!normalizedCode.StartsWith("vi", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (locale == null)
        {
            throw new InvalidOperationException("Thiết bị chưa có giọng đọc tiếng Việt. Hãy dùng audio từ backend hoặc cài gói speech tiếng Việt.");
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
