namespace MauiApp1.Services
{
    public sealed class WarmupResult
    {
        public bool IsFreshDataAvailable { get; init; }
        public bool IsLocationReliable { get; init; }
        public bool IsOfflineFallback { get; init; }
        public bool HasPendingPoiRefresh { get; init; }
        public int StagedPoiCount { get; init; }
    }

    /// <summary>
    /// Coordinates lightweight startup tasks so Splash can stay responsive.
    /// </summary>
    public class StartupWarmupService
    {
        private readonly LocationService _locationService;
        private readonly OpenStreetMapService _openStreetMapService;
        private readonly PoiSyncCacheService _poiSyncCacheService;
        private readonly AudioCacheService _audioCacheService;

        private const string LanguagePreferenceKey = "app_language";

        public StartupWarmupService(
            LocationService locationService,
            OpenStreetMapService openStreetMapService,
            PoiSyncCacheService poiSyncCacheService,
            AudioCacheService audioCacheService)
        {
            _locationService = locationService;
            _openStreetMapService = openStreetMapService;
            _poiSyncCacheService = poiSyncCacheService;
            _audioCacheService = audioCacheService;
        }

        public async Task PrepareLocalPoiAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Keep splash responsive while ensuring we can render from cached POIs instantly.
            _ = await _poiSyncCacheService.GetCachedPoiAsync();
            await Task.Delay(150, cancellationToken);
        }

        public async Task<WarmupResult> SyncAndPrefetchNearbyAsync(CancellationToken cancellationToken = default)
        {
            var location = await _locationService.GetCurrentLocationAsync();
            var isReliable = location?.Accuracy is not null && location.Accuracy <= 50;
            var languageCode = Preferences.Default.Get(LanguagePreferenceKey, "en-US");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            try
            {
                if (location == null)
                {
                    return new WarmupResult
                    {
                        IsFreshDataAvailable = false,
                        IsLocationReliable = false,
                        IsOfflineFallback = true,
                        HasPendingPoiRefresh = false,
                        StagedPoiCount = 0
                    };
                }

                var nearbyPois = await _openStreetMapService.GetNearbyPlacesAsync(
                    location.Latitude,
                    location.Longitude,
                    radiusMeters: 1500,
                    maxItems: 10,
                    cancellationToken: timeoutCts.Token);

                var hasPendingRefresh = await _poiSyncCacheService.StageIncomingRefreshAsync(nearbyPois);

                await _audioCacheService.PrefetchNearbyAudioAsync(
                    nearbyPois,
                    languageCode,
                    maxItems: 10,
                    cancellationToken: timeoutCts.Token);

                await _audioCacheService.PruneByLruAsync(
                    activeLanguageCode: languageCode,
                    maxAge: TimeSpan.FromMinutes(30),
                    keepMaxItems: 30);

                return new WarmupResult
                {
                    IsFreshDataAvailable = nearbyPois.Count > 0,
                    IsLocationReliable = isReliable,
                    IsOfflineFallback = false,
                    HasPendingPoiRefresh = hasPendingRefresh,
                    StagedPoiCount = nearbyPois.Count
                };
            }
            catch (OperationCanceledException)
            {
                return new WarmupResult
                {
                    IsFreshDataAvailable = false,
                    IsLocationReliable = isReliable,
                    IsOfflineFallback = true,
                    HasPendingPoiRefresh = false,
                    StagedPoiCount = 0
                };
            }
        }
    }
}
