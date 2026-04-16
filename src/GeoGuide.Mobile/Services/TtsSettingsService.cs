namespace MauiApp1.Services;

public sealed class TtsSettings
{
    public string PreferredLanguageCode { get; init; } = string.Empty;
    public float Pitch { get; init; } = 1.0f;
    public float Volume { get; init; } = 1.0f;
    public float SpeechRate { get; init; } = 1.0f;
}

public sealed class TtsSettingsService
{
    private const string PreferredLanguageKey = "tts_preferred_language";
    private const string PitchKey = "tts_pitch";
    private const string VolumeKey = "tts_volume";
    private const string RateKey = "tts_rate";

    public TtsSettings Get()
    {
        return new TtsSettings
        {
            PreferredLanguageCode = Preferences.Default.Get(PreferredLanguageKey, string.Empty),
            Pitch = Clamp(Preferences.Default.Get(PitchKey, 1.0f), 0.5f, 2.0f),
            Volume = Clamp(Preferences.Default.Get(VolumeKey, 1.0f), 0.0f, 1.0f),
            SpeechRate = Clamp(Preferences.Default.Get(RateKey, 1.0f), 0.25f, 2.0f)
        };
    }

    public void Save(TtsSettings settings)
    {
        Preferences.Default.Set(PreferredLanguageKey, settings.PreferredLanguageCode ?? string.Empty);
        Preferences.Default.Set(PitchKey, Clamp(settings.Pitch, 0.5f, 2.0f));
        Preferences.Default.Set(VolumeKey, Clamp(settings.Volume, 0.0f, 1.0f));
        Preferences.Default.Set(RateKey, Clamp(settings.SpeechRate, 0.25f, 2.0f));
    }

    private static float Clamp(float value, float min, float max) =>
        Math.Max(min, Math.Min(max, value));
}
