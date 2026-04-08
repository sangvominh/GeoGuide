namespace MauiApp1.Services
{
    /// <summary>
    /// Service to handle location permission requests and location retrieval.
    /// </summary>
    public class LocationService
    {
        private CancellationTokenSource? _cancelTokenSource;

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
    }
}
