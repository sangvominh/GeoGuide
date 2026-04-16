namespace MauiApp1.Services
{
    /// <summary>
    /// Service to handle location permission requests and location retrieval.
    /// </summary>
    public class LocationService
    {
        private CancellationTokenSource? _cancelTokenSource;
        private Task? _trackingTask;
        private CancellationTokenSource? _trackingCancellationTokenSource;

        public event EventHandler<Location>? LocationUpdated;
        public bool IsTracking { get; private set; }

        /// <summary>
        /// Checks if location permission is already granted.
        /// </summary>
        public async Task<PermissionStatus> CheckLocationPermissionAsync()
        {
            return await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        }

        /// <summary>
        /// Requests location permission from the user.
        /// Returns the resulting PermissionStatus.
        /// </summary>
        public async Task<PermissionStatus> RequestLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return status;

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                // Optionally show rationale to the user
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status;
        }

        public async Task<PermissionStatus> RequestBackgroundLocationPermissionAsync()
        {
            var whenInUseStatus = await RequestLocationPermissionAsync();
            if (whenInUseStatus != PermissionStatus.Granted)
            {
                return whenInUseStatus;
            }

            try
            {
                var alwaysStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                if (alwaysStatus == PermissionStatus.Granted)
                {
                    return alwaysStatus;
                }

                return await Permissions.RequestAsync<Permissions.LocationAlways>();
            }
            catch
            {
                return whenInUseStatus;
            }
        }

        /// <summary>
        /// Gets the current device location.
        /// Returns null if location cannot be obtained.
        /// </summary>
        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                var status = await CheckLocationPermissionAsync();
                if (status != PermissionStatus.Granted)
                    return null;

                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                _cancelTokenSource = new CancellationTokenSource();

                var location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
                return location;
            }
            catch (FeatureNotSupportedException)
            {
                // Location not supported on device
                return null;
            }
            catch (FeatureNotEnabledException)
            {
                // Location not enabled on device
                return null;
            }
            catch (PermissionException)
            {
                // Permission not granted
                return null;
            }
            catch (Exception)
            {
                // Unable to get location
                return null;
            }
        }

        /// <summary>
        /// Gets the last known location (faster, no GPS required).
        /// </summary>
        public async Task<Location?> GetLastKnownLocationAsync()
        {
            try
            {
                var location = await Geolocation.Default.GetLastKnownLocationAsync();
                return location;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Cancel any ongoing location request.
        /// </summary>
        public void CancelRequest()
        {
            if (_cancelTokenSource != null && !_cancelTokenSource.IsCancellationRequested)
                _cancelTokenSource.Cancel();
        }

        public async Task StartTrackingAsync(
            TimeSpan? interval = null,
            GeolocationAccuracy accuracy = GeolocationAccuracy.Best,
            CancellationToken cancellationToken = default)
        {
            if (IsTracking)
            {
                return;
            }

            var status = await CheckLocationPermissionAsync();
            if (status != PermissionStatus.Granted)
            {
                return;
            }

            IsTracking = true;
            var effectiveInterval = interval ?? TimeSpan.FromSeconds(8);
            _trackingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var trackingToken = _trackingCancellationTokenSource.Token;

            _trackingTask = Task.Run(async () =>
            {
                while (!trackingToken.IsCancellationRequested)
                {
                    try
                    {
                        var request = new GeolocationRequest(accuracy, TimeSpan.FromSeconds(8));
                        var location = await Geolocation.Default.GetLocationAsync(request, trackingToken);
                        if (location != null)
                        {
                            LocationUpdated?.Invoke(this, location);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                    }

                    try
                    {
                        await Task.Delay(effectiveInterval, trackingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, trackingToken);
        }

        public async Task StopTrackingAsync()
        {
            if (!IsTracking)
            {
                return;
            }

            IsTracking = false;

            if (_trackingCancellationTokenSource != null && !_trackingCancellationTokenSource.IsCancellationRequested)
            {
                _trackingCancellationTokenSource.Cancel();
            }

            if (_trackingTask != null)
            {
                try
                {
                    await _trackingTask;
                }
                catch
                {
                }
            }

            _trackingTask = null;
            _trackingCancellationTokenSource?.Dispose();
            _trackingCancellationTokenSource = null;
        }
    }
}
