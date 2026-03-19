using Microsoft.Extensions.Logging;
using MauiApp1.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // Register services
            builder.Services.AddSingleton<LocationService>();
            builder.Services.AddSingleton<StartupWarmupService>();

            // Register pages
            builder.Services.AddTransient<Pages.SplashPermissionPage>();
            builder.Services.AddTransient<Pages.MainMapPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Logging.AddFilter("Mapsui", LogLevel.Warning);

            return builder.Build();
        }
    }
}
