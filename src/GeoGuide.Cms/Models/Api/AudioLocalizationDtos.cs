using System.ComponentModel.DataAnnotations;

namespace GeoGuide.Cms.Models.Api;

public class PrepareLocalizationHotsetRequestDto
{
    public string LanguageCode { get; init; } = "vi";

    [Range(1, 200)]
    public int Limit { get; init; } = 25;

    public bool ForceRegenerate { get; init; }
}

public class LocalizationOnDemandRequestDto
{
    [Required]
    public Guid PoiId { get; init; }

    public string LanguageCode { get; init; } = "vi";

    public string? Text { get; init; }

    public bool ForceRegenerate { get; init; } = true;
}

public class LocalizationWarmupRequestDto
{
    public string LanguageCode { get; init; } = "vi";

    public bool ForceRegenerate { get; init; }
}

public class TtsRequestDto
{
    [Required]
    public string Text { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = "vi";

    public string? VoiceId { get; init; }
}

public class VoiceDto
{
    public string Id { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = string.Empty;

    public string Provider { get; init; } = string.Empty;
}

public class AudioAssetDto
{
    public Guid? PoiId { get; init; }

    public Guid? AudioId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string AudioUrl { get; init; } = string.Empty;

    public string Format { get; init; } = "wav";

    public string Provider { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string? Text { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}

public class AudioManifestDto
{
    public DateTimeOffset GeneratedAt { get; init; }

    public string Provider { get; init; } = "local-demo-placeholder";

    public string Note { get; init; } = "Local deterministic WAV files are compatible demo placeholders, not real narrated speech.";

    public IReadOnlyList<AudioAssetDto> Assets { get; init; } = [];
}

public class LocalizationJobResponseDto
{
    public string TaskId { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public int RequestedCount { get; init; }

    public int GeneratedCount { get; init; }

    public int SkippedCount { get; init; }

    public IReadOnlyList<AudioAssetDto> Assets { get; init; } = [];

    public string Provider { get; init; } = "local-demo-placeholder";

    public string Note { get; init; } = "Fallback generated deterministic WAV demo placeholders because Edge-TTS is not configured.";
}

public class LocalizationTaskStatusDto
{
    public string TaskId { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = string.Empty;

    public string Scope { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public int RequestedCount { get; init; }

    public int GeneratedCount { get; init; }

    public int SkippedCount { get; init; }

    public DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public string? Message { get; init; }
}
