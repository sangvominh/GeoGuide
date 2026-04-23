using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MauiApp1.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui.Controls;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitMediaElement(isAndroidForegroundServiceEnabled: false)
                .UseSkiaSharp()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            var apiBaseUrl = Environment.GetEnvironmentVariable("POI_API_BASE_URL");
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
                    ? "http://10.0.2.2:5005/"
                    : "http://localhost:5005/";
            }

            var apiOptions = new PoiApiOptions
            {
                BaseUrl = apiBaseUrl.EndsWith("/") ? apiBaseUrl : $"{apiBaseUrl}/",
                TimeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("POI_API_TIMEOUT_SECONDS"), out var timeoutSeconds)
                    ? timeoutSeconds
                    : 10,
                EnablePlaybackLogs = !string.Equals(
                    Environment.GetEnvironmentVariable("POI_PLAYBACK_LOGS_ENABLED"),
                    "false",
                    StringComparison.OrdinalIgnoreCase)
            };

            builder.Services.AddSingleton<LocationService>();
            builder.Services.AddSingleton(apiOptions);
            builder.Services.AddSingleton<ApiBaseUrlService>();
            builder.Services.AddSingleton<LocalDatabaseService>();
            builder.Services.AddSingleton<PoiCacheService>();
            builder.Services.AddSingleton(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<PoiApiOptions>();
                return new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
                };
            });
            builder.Services.AddSingleton<PoiApiService>();
            builder.Services.AddSingleton<MediaPrefetchService>();
            builder.Services.AddSingleton<PoiRepository>();
            builder.Services.AddSingleton<GeofenceEngineService>();
            builder.Services.AddSingleton<TriggerGuardService>();
            builder.Services.AddSingleton<TtsSettingsService>();
            builder.Services.AddSingleton<AccessModeService>();
            builder.Services.AddSingleton<DeviceIdentityService>();
            builder.Services.AddSingleton<DeepLinkActivationService>();
            builder.Services.AddSingleton<OfflineAnalyticsLogService>();
            builder.Services.AddSingleton<NarrationService>();
            builder.Services.AddSingleton<StartupWarmupService>();

            builder.Services.AddTransient<Pages.SplashPermissionPage>();
            builder.Services.AddTransient<Pages.MainMapPage>();
            builder.Services.AddTransient<Pages.QrScannerPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Logging.AddFilter("Mapsui", LogLevel.Warning);

            return builder.Build();
        }
    }
}
