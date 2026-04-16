using MauiApp1.Models;

namespace MauiApp1.Services;

public enum PoiDataSource
{
    Api,
    Cache,
    BundledFallback
}

public sealed class PoiLoadResult
{
    public IReadOnlyList<PointOfInterest> Pois { get; init; } = [];
    public PoiDataSource DataSource { get; init; }
}

public class PoiRepository
{
    private readonly PoiApiService _poiApiService;
    private readonly PoiCacheService _poiCacheService;

    public PoiRepository(PoiApiService poiApiService, PoiCacheService poiCacheService)
    {
        _poiApiService = poiApiService;
        _poiCacheService = poiCacheService;
    }

    public async Task<PoiLoadResult> GetPoisAsync(CancellationToken cancellationToken = default)
    {
        await _poiCacheService.InitializeAsync(cancellationToken);

        try
        {
            var syncedPois = await SyncPoisAsync(cancellationToken);
            if (syncedPois.Count > 0)
            {
                return new PoiLoadResult
                {
                    Pois = syncedPois,
                    DataSource = PoiDataSource.Api
                };
            }
        }
        catch
        {
        }

        var cachedPois = await _poiCacheService.LoadCachedAsync(cancellationToken);
        if (cachedPois.Count > 0)
        {
            return new PoiLoadResult
            {
                Pois = cachedPois,
                DataSource = PoiDataSource.Cache
            };
        }

        var fallbackPois = await _poiCacheService.LoadBundledFallbackAsync(cancellationToken);
        return new PoiLoadResult
        {
            Pois = fallbackPois,
            DataSource = PoiDataSource.BundledFallback
        };
    }

    private async Task<IReadOnlyList<PointOfInterest>> SyncPoisAsync(CancellationToken cancellationToken)
    {
        var lastSyncAt = await _poiCacheService.GetLastSyncAtAsync(cancellationToken);
        if (!lastSyncAt.HasValue)
        {
            var bootstrap = await _poiApiService.GetSyncBootstrapAsync(cancellationToken);
            var bootstrapPois = bootstrap.Pois
                .Where(static poi => poi.IsActive && !poi.IsDeleted)
                .Select(MapSyncPoi)
                .OrderByDescending(static poi => poi.Priority)
                .ThenBy(static poi => poi.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            await _poiCacheService.SaveAsync(bootstrapPois, cancellationToken);
            await _poiCacheService.SetLastSyncAtAsync(bootstrap.ServerTime, cancellationToken);
            return bootstrapPois;
        }

        var delta = await _poiApiService.GetSyncDeltaAsync(lastSyncAt.Value, cancellationToken);
        var currentPois = await _poiCacheService.LoadCachedAsync(cancellationToken);
        var poiMap = currentPois.ToDictionary(poi => poi.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var item in delta.Pois)
        {
            var poiId = item.Id.ToString();
            if (item.IsDeleted || !item.IsActive)
            {
                poiMap.Remove(poiId);
                continue;
            }

            poiMap[poiId] = MapSyncPoi(item);
        }

        var merged = poiMap.Values
            .OrderByDescending(static poi => poi.Priority)
            .ThenBy(static poi => poi.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        await _poiCacheService.SaveAsync(merged, cancellationToken);
        await _poiCacheService.SetLastSyncAtAsync(delta.ServerTime, cancellationToken);

        return merged;
    }

    private static PointOfInterest MapSyncPoi(SyncPoiDto source)
    {
        var preferredContent = source.Contents
            .OrderBy(static content => string.Equals(content.LanguageCode, "vi-VN", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(static content => string.IsNullOrWhiteSpace(content.TtsContent) ? 1 : 0)
            .ThenBy(static content => string.IsNullOrWhiteSpace(content.AudioUrl) ? 1 : 0)
            .FirstOrDefault();

        return new PointOfInterest
        {
            Id = source.Id.ToString(),
            Name = source.Name,
            Description = source.Description,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            TriggerRadiusMeters = source.TriggerRadiusMeters,
            CooldownMinutes = source.CooldownMinutes,
            Priority = source.Priority,
            CategoryKey = source.CategoryKey,
            CategoryLabel = source.CategoryLabel,
            ImageUrl = source.ImageUrl,
            MapUrl = source.MapUrl,
            AudioUrl = preferredContent?.AudioUrl ?? string.Empty,
            TtsScript = preferredContent?.TtsContent ?? string.Empty,
            LanguageCode = preferredContent?.LanguageCode ?? "vi-VN",
            IsActive = source.IsActive && !source.IsDeleted,
            UpdatedAt = source.UpdatedAt
        };
    }
}
