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

        public StartupWarmupService(LocationService locationService)
        {
            _locationService = locationService;
        }

        public async Task PrepareLocalPoiAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(220, cancellationToken);
        }

        public async Task<WarmupResult> SyncAndPrefetchNearbyAsync(CancellationToken cancellationToken = default)
        {
            var location = await _locationService.GetCurrentLocationAsync();
            var isReliable = location?.Accuracy is not null && location.Accuracy <= 50;

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            try
            {
                // Placeholder pipeline for future API: fetch latest POI data + translated audio metadata.
                await SimulateServerSyncAsync(timeoutCts.Token);

                // Placeholder prefetch for 10 nearest POI audio files in a 1.5km radius.
                await PrefetchNearbyAudioAsync(maxItems: 10, timeoutCts.Token);

                return new WarmupResult
                {
                    IsFreshDataAvailable = true,
                    IsLocationReliable = isReliable,
                    IsOfflineFallback = false
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

        private static async Task SimulateServerSyncAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(700, cancellationToken);
        }

        private static async Task PrefetchNearbyAudioAsync(int maxItems, CancellationToken cancellationToken)
        {
            for (var i = 0; i < maxItems; i++)
            {
                await Task.Delay(90, cancellationToken);
            }
        }
    }
}
