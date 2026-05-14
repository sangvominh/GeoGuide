using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Models.Api;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Services;

public class LocalizationTaskStore
{
    private readonly ConcurrentDictionary<string, LocalizationTaskStatusDto> tasks = new();

    public LocalizationTaskStatusDto Upsert(
        string taskId,
        string languageCode,
        string scope,
        string status,
        int requestedCount,
        int generatedCount,
        int skippedCount,
        string? message = null)
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = tasks.AddOrUpdate(
            taskId,
            _ => new LocalizationTaskStatusDto
            {
                TaskId = taskId,
                LanguageCode = languageCode,
                Scope = scope,
                Status = status,
                RequestedCount = requestedCount,
                GeneratedCount = generatedCount,
                SkippedCount = skippedCount,
                StartedAt = now,
                UpdatedAt = now,
                Message = message
            },
            (_, existing) => new LocalizationTaskStatusDto
            {
                TaskId = existing.TaskId,
                LanguageCode = languageCode,
                Scope = scope,
                Status = status,
                RequestedCount = requestedCount,
                GeneratedCount = generatedCount,
                SkippedCount = skippedCount,
                StartedAt = existing.StartedAt,
                UpdatedAt = now,
                Message = message
            });

        return snapshot;
    }

    public LocalizationTaskStatusDto? Get(string taskId)
    {
        return tasks.TryGetValue(taskId, out var task) ? task : null;
    }

    public LocalizationTaskStatusDto? GetLatestForLanguage(string languageCode)
    {
        var normalized = NormalizeLanguage(languageCode);
        return tasks.Values
            .Where(t => string.Equals(t.LanguageCode, normalized, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(t => t.UpdatedAt)
            .FirstOrDefault();
    }

    private static string NormalizeLanguage(string languageCode)
    {
        return string.IsNullOrWhiteSpace(languageCode) ? "vi" : languageCode.Trim().ToLowerInvariant();
    }
}

public class AudioLocalizationService(
    ApplicationDbContext dbContext,
    IWebHostEnvironment environment,
    LocalizationTaskStore taskStore)
{
    private const string Provider = "local-demo-placeholder";
    private const string AudioDirectorySegment = "static/audio";

    private static readonly VoiceDto[] Voices =
    [
        new() { Id = "vi-VN-demo-neutral", Label = "Vietnamese demo neutral", LanguageCode = "vi", Provider = Provider },
        new() { Id = "en-US-demo-neutral", Label = "English demo neutral", LanguageCode = "en", Provider = Provider },
        new() { Id = "ja-JP-demo-neutral", Label = "Japanese demo neutral", LanguageCode = "ja", Provider = Provider },
        new() { Id = "ko-KR-demo-neutral", Label = "Korean demo neutral", LanguageCode = "ko", Provider = Provider }
    ];

    public IReadOnlyList<VoiceDto> GetVoices()
    {
        return Voices;
    }

    public async Task<AudioAssetDto> GenerateTtsAsync(TtsRequestDto request, CancellationToken cancellationToken)
    {
        var languageCode = NormalizeLanguage(request.LanguageCode);
        var text = NormalizeText(request.Text);
        var voiceId = string.IsNullOrWhiteSpace(request.VoiceId)
            ? Voices.FirstOrDefault(v => v.LanguageCode == languageCode)?.Id ?? $"{languageCode}-demo-neutral"
            : request.VoiceId.Trim();

        var audioUrl = await WriteDemoWavAsync(text, languageCode, voiceId, cancellationToken);
        return new AudioAssetDto
        {
            LanguageCode = languageCode,
            AudioUrl = audioUrl,
            Provider = Provider,
            Status = "generated",
            Text = text,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task<LocalizationJobResponseDto> PrepareHotsetAsync(
        PrepareLocalizationHotsetRequestDto request,
        CancellationToken cancellationToken)
    {
        var languageCode = NormalizeLanguage(request.LanguageCode);
        var pois = await dbContext.Pois
            .Where(p => p.IsActive)
            .Include(p => p.Audios)
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return await GenerateForPoisAsync(
            "hotset",
            languageCode,
            pois,
            request.ForceRegenerate,
            cancellationToken);
    }

    public async Task<LocalizationJobResponseDto> WarmupAsync(
        LocalizationWarmupRequestDto request,
        CancellationToken cancellationToken)
    {
        var languageCode = NormalizeLanguage(request.LanguageCode);
        var pois = await dbContext.Pois
            .Where(p => p.IsActive)
            .Include(p => p.Audios)
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return await GenerateForPoisAsync(
            "warmup",
            languageCode,
            pois,
            request.ForceRegenerate,
            cancellationToken);
    }

    public async Task<LocalizationJobResponseDto?> OnDemandAsync(
        LocalizationOnDemandRequestDto request,
        CancellationToken cancellationToken)
    {
        var languageCode = NormalizeLanguage(request.LanguageCode);
        var poi = await dbContext.Pois
            .Where(p => p.Id == request.PoiId && p.IsActive)
            .Include(p => p.Audios)
            .FirstOrDefaultAsync(cancellationToken);

        if (poi is null)
        {
            return null;
        }

        return await GenerateForPoisAsync(
            "on-demand",
            languageCode,
            [poi],
            request.ForceRegenerate,
            cancellationToken,
            request.Text);
    }

    public LocalizationTaskStatusDto? GetTaskStatus(string taskId)
    {
        return taskStore.Get(taskId);
    }

    public LocalizationTaskStatusDto GetWarmupStatus(string languageCode)
    {
        var normalized = NormalizeLanguage(languageCode);
        return taskStore.GetLatestForLanguage(normalized)
            ?? new LocalizationTaskStatusDto
            {
                TaskId = string.Empty,
                LanguageCode = normalized,
                Scope = "warmup",
                Status = "not-started",
                StartedAt = DateTimeOffset.MinValue,
                UpdatedAt = DateTimeOffset.MinValue
            };
    }

    public async Task<AudioManifestDto> GetPackManifestAsync(string? languageCode, CancellationToken cancellationToken)
    {
        var normalized = string.IsNullOrWhiteSpace(languageCode) ? null : NormalizeLanguage(languageCode);
        var query = dbContext.PoiAudios
            .Include(a => a.Poi)
            .Where(a => a.Poi != null && a.Poi.IsActive && a.AudioUrl != null);

        if (normalized is not null)
        {
            query = query.Where(a => a.LanguageCode == normalized);
        }

        var audios = await query
            .OrderBy(a => a.LanguageCode)
            .ThenBy(a => a.Poi!.Priority)
            .ThenBy(a => a.Poi!.Name)
            .ToListAsync(cancellationToken);

        return new AudioManifestDto
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            Assets = audios.Select(audio => MapAudio(audio)).ToList()
        };
    }

    private async Task<LocalizationJobResponseDto> GenerateForPoisAsync(
        string scope,
        string languageCode,
        IReadOnlyList<Poi> pois,
        bool forceRegenerate,
        CancellationToken cancellationToken,
        string? overrideText = null)
    {
        var taskId = $"{scope}-{languageCode}-{Guid.NewGuid():N}";
        taskStore.Upsert(taskId, languageCode, scope, "running", pois.Count, 0, 0);

        var generated = 0;
        var skipped = 0;
        var assets = new List<AudioAssetDto>();

        foreach (var poi in pois)
        {
            var existing = poi.Audios
                .Where(a => a.LanguageCode == languageCode)
                .OrderByDescending(a => a.AudioUrl != null)
                .ThenByDescending(a => a.UpdatedAt)
                .FirstOrDefault();

            if (!forceRegenerate && existing?.AudioUrl is not null)
            {
                skipped++;
                assets.Add(MapAudio(existing, "skipped-existing"));
                taskStore.Upsert(taskId, languageCode, scope, "running", pois.Count, generated, skipped);
                continue;
            }

            var sourceText = string.IsNullOrWhiteSpace(overrideText)
                ? existing?.TtsContent ?? BuildDemoLocalizedText(poi, languageCode)
                : overrideText;
            var text = NormalizeText(sourceText);
            var tts = await GenerateTtsAsync(
                new TtsRequestDto { Text = text, LanguageCode = languageCode },
                cancellationToken);
            var audio = existing ?? new PoiAudio
            {
                Id = Guid.NewGuid(),
                PoiId = poi.Id,
                LanguageCode = languageCode,
                CreatedAt = DateTimeOffset.UtcNow
            };

            audio.ContentType = PoiContentType.AudioFile;
            audio.AudioUrl = tts.AudioUrl;
            audio.TtsContent = text;
            audio.UpdatedAt = DateTimeOffset.UtcNow;

            if (existing is null)
            {
                dbContext.PoiAudios.Add(audio);
            }

            generated++;
            assets.Add(MapAudio(audio, "generated"));
            taskStore.Upsert(taskId, languageCode, scope, "running", pois.Count, generated, skipped);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        taskStore.Upsert(taskId, languageCode, scope, "completed", pois.Count, generated, skipped);

        return new LocalizationJobResponseDto
        {
            TaskId = taskId,
            LanguageCode = languageCode,
            Status = "completed",
            RequestedCount = pois.Count,
            GeneratedCount = generated,
            SkippedCount = skipped,
            Assets = assets
        };
    }

    private async Task<string> WriteDemoWavAsync(
        string text,
        string languageCode,
        string voiceId,
        CancellationToken cancellationToken)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{languageCode}|{voiceId}|{text}")))
            .ToLowerInvariant();
        var fileName = $"{languageCode}-{hash[..16]}.wav";
        var webRoot = environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var audioDirectory = Path.Combine(webRoot, "static", "audio");
        Directory.CreateDirectory(audioDirectory);

        var path = Path.Combine(audioDirectory, fileName);
        if (!File.Exists(path))
        {
            var bytes = CreateDemoWav(text, hash);
            await File.WriteAllBytesAsync(path, bytes, cancellationToken);
        }

        return $"/{AudioDirectorySegment}/{fileName}";
    }

    private static byte[] CreateDemoWav(string text, string hash)
    {
        const int sampleRate = 22050;
        const short channels = 1;
        const short bitsPerSample = 16;
        var durationSeconds = Math.Clamp(0.7 + text.Length / 120.0, 0.7, 3.0);
        var sampleCount = (int)(sampleRate * durationSeconds);
        var dataSize = sampleCount * channels * bitsPerSample / 8;
        var frequency = 360 + Convert.ToInt32(hash[..2], 16);

        using var stream = new MemoryStream(44 + dataSize);
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8);
        writer.Write((short)(channels * bitsPerSample / 8));
        writer.Write(bitsPerSample);
        writer.Write("data"u8.ToArray());
        writer.Write(dataSize);

        for (var i = 0; i < sampleCount; i++)
        {
            var envelope = Math.Min(1.0, i / (sampleRate * 0.08));
            envelope = Math.Min(envelope, (sampleCount - i) / (sampleRate * 0.08));
            var sample = Math.Sin(2 * Math.PI * frequency * i / sampleRate) * 0.18 * Math.Max(0, envelope);
            writer.Write((short)(sample * short.MaxValue));
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static AudioAssetDto MapAudio(PoiAudio audio, string status = "available")
    {
        return new AudioAssetDto
        {
            PoiId = audio.PoiId,
            AudioId = audio.Id,
            LanguageCode = audio.LanguageCode,
            AudioUrl = audio.AudioUrl ?? string.Empty,
            Provider = IsLocalDemoAudio(audio.AudioUrl) ? Provider : "external-or-uploaded",
            Status = status,
            Text = audio.TtsContent,
            UpdatedAt = audio.UpdatedAt
        };
    }

    private static bool IsLocalDemoAudio(string? audioUrl)
    {
        return audioUrl?.StartsWith($"/{AudioDirectorySegment}/", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string BuildDemoLocalizedText(Poi poi, string languageCode)
    {
        if (languageCode == "vi")
        {
            return $"{poi.Name}. {poi.Description}";
        }

        return $"Demo {languageCode} localization for {poi.Name}. Source description: {poi.Description}";
    }

    private static string NormalizeLanguage(string languageCode)
    {
        return string.IsNullOrWhiteSpace(languageCode) ? "vi" : languageCode.Trim().ToLowerInvariant();
    }

    private static string NormalizeText(string text)
    {
        return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}
