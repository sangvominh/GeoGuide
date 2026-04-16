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
        try
        {
            var apiPois = await _poiApiService.GetPoisAsync(cancellationToken);
            if (apiPois.Count > 0)
            {
                await _poiCacheService.SaveAsync(apiPois, cancellationToken);
                return new PoiLoadResult
                {
                    Pois = apiPois,
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
}
