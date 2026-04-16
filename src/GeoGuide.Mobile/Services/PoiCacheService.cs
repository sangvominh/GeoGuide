using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1.Services;

public class PoiCacheService
{
    private const string LastSyncAtKey = "last_sync_at_utc";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private readonly LocalDatabaseService _databaseService;

    public PoiCacheService(LocalDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        _databaseService.InitializeAsync(cancellationToken);

    public async Task SaveAsync(IReadOnlyList<PointOfInterest> pois, CancellationToken cancellationToken = default)
    {
        var records = pois.Select(ToRecord).ToList();
        await _databaseService.ReplacePoisAsync(records, cancellationToken);
    }

    public async Task<IReadOnlyList<PointOfInterest>> LoadCachedAsync(CancellationToken cancellationToken = default)
    {
        var records = await _databaseService.GetPoisAsync(cancellationToken);
        return records.Select(ToPoi).ToList();
    }

    public async Task<IReadOnlyList<PointOfInterest>> LoadBundledFallbackAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = await FileSystem.Current.OpenAppPackageFileAsync("poi-fallback.json");
        return await JsonSerializer.DeserializeAsync<List<PointOfInterest>>(stream, JsonOptions, cancellationToken) ?? [];
    }

    public async Task<DateTimeOffset?> GetLastSyncAtAsync(CancellationToken cancellationToken = default)
    {
        var value = await _databaseService.GetSyncStateValueAsync(LastSyncAtKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, out var parsed) ? parsed : null;
    }

    public Task SetLastSyncAtAsync(DateTimeOffset value, CancellationToken cancellationToken = default) =>
        _databaseService.SetSyncStateValueAsync(LastSyncAtKey, value.ToString("O"), cancellationToken);

    private static LocalPoiRecord ToRecord(PointOfInterest poi)
    {
        return new LocalPoiRecord
        {
            Id = poi.Id,
            Name = poi.Name,
            Description = poi.Description,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            TriggerRadiusMeters = poi.TriggerRadiusMeters,
            CooldownMinutes = poi.CooldownMinutes,
            Priority = poi.Priority,
            CategoryKey = poi.CategoryKey,
            CategoryLabel = poi.CategoryLabel,
            ImageUrl = poi.ImageUrl,
            MapUrl = poi.MapUrl,
            AudioUrl = poi.AudioUrl,
            TtsScript = poi.TtsScript,
            LanguageCode = poi.LanguageCode,
            IsActive = poi.IsActive,
            UpdatedAtIso = poi.UpdatedAt.ToString("O")
        };
    }

    private static PointOfInterest ToPoi(LocalPoiRecord record)
    {
        DateTimeOffset.TryParse(record.UpdatedAtIso, out var updatedAt);

        return new PointOfInterest
        {
            Id = record.Id,
            Name = record.Name,
            Description = record.Description,
            Latitude = record.Latitude,
            Longitude = record.Longitude,
            TriggerRadiusMeters = record.TriggerRadiusMeters,
            CooldownMinutes = record.CooldownMinutes,
            Priority = record.Priority,
            CategoryKey = record.CategoryKey,
            CategoryLabel = record.CategoryLabel,
            ImageUrl = record.ImageUrl,
            MapUrl = record.MapUrl,
            AudioUrl = record.AudioUrl,
            TtsScript = record.TtsScript,
            LanguageCode = record.LanguageCode,
            IsActive = record.IsActive,
            UpdatedAt = updatedAt
        };
    }
}
