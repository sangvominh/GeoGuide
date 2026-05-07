namespace MauiApp1.Services
{
    public sealed class WarmupResult
    {
        public bool IsFreshDataAvailable { get; init; }
        public bool IsLocationReliable { get; init; }
        public bool IsOfflineFallback { get; init; }
    }

    /// <summary>
    /// Coordinates lightweight startup tasks so Splash can stay responsive.
    /// </summary>
    public class StartupWarmupService
    {
        private readonly LocationService _locationService;
        private readonly PoiRepository _poiRepository;
        private readonly MediaPrefetchService _mediaPrefetchService;
        private readonly OfflineAnalyticsLogService _offlineAnalyticsLogService;

        public StartupWarmupService(
            LocationService locationService,
            PoiRepository poiRepository,
            MediaPrefetchService mediaPrefetchService,
            OfflineAnalyticsLogService offlineAnalyticsLogService)
        {
            _locationService = locationService;
            _poiRepository = poiRepository;
            _mediaPrefetchService = mediaPrefetchService;
            _offlineAnalyticsLogService = offlineAnalyticsLogService;
        }

        public async Task PrepareLocalPoiAsync(CancellationToken cancellationToken = default)
        {
            await _poiRepository.GetPoisAsync(cancellationToken);
        }

        public async Task<WarmupResult> SyncAndPrefetchNearbyAsync(CancellationToken cancellationToken = default)
        {
            var location = await _locationService.GetCurrentLocationAsync();
            var isReliable = location?.Accuracy is not null && location.Accuracy <= 50;

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            try
            {
                var pois = await _poiRepository.GetPoisAsync(timeoutCts.Token);
                await PrefetchNearbyAudioAsync(pois.Pois, maxItems: 10, timeoutCts.Token);
                _ = _offlineAnalyticsLogService.SyncPendingLogsAsync(batchSize: 50, timeoutCts.Token);

                return new WarmupResult
                {
                    IsFreshDataAvailable = pois.DataSource == PoiDataSource.Api,
                    IsLocationReliable = isReliable,
                    IsOfflineFallback = pois.DataSource != PoiDataSource.Api
                };
            }
            catch (OperationCanceledException)
            {
                return new WarmupResult
                {
                    IsFreshDataAvailable = false,
                    IsLocationReliable = isReliable,
                    IsOfflineFallback = true
                };
            }
        }

        private async Task PrefetchNearbyAudioAsync(IReadOnlyList<Models.PointOfInterest> pois, int maxItems, CancellationToken cancellationToken)
        {
            await _mediaPrefetchService.PrefetchNearbyAsync(pois, maxItems, cancellationToken);
        }
    }
}
