using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using MauiApp1.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace MauiApp1
{
    [IntentFilter(
        [Intent.ActionView],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        DataScheme = "geoguide",
        DataHost = "join")]
    [Activity(Theme = "@style/Maui.SplashTheme", ResizeableActivity = true, MainLauncher = true, LaunchMode = LaunchMode.SingleTask, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleDeepLink(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            HandleDeepLink(intent);
        }

        private static void HandleDeepLink(Intent? intent)
        {
            var rawUri = intent?.DataString;
            if (string.IsNullOrWhiteSpace(rawUri) || !Uri.TryCreate(rawUri, UriKind.Absolute, out var uri))
            {
                return;
            }

            var services = IPlatformApplication.Current?.Services;
            var deepLinkService = services?.GetService<DeepLinkActivationService>();
            deepLinkService?.Publish(uri);
        }
    }
}
