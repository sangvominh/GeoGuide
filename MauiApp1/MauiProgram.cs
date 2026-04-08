using Microsoft.Extensions.Logging;
using MauiApp1.Services;
using Npgsql;
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

            var dbConnectionString = Environment.GetEnvironmentVariable("POI_DB_CONNECTION");
            if (string.IsNullOrWhiteSpace(dbConnectionString))
            {
                var csb = new NpgsqlConnectionStringBuilder
                {
                    Host = Environment.GetEnvironmentVariable("POI_DB_HOST") ?? "localhost",
                    Port = int.TryParse(Environment.GetEnvironmentVariable("POI_DB_PORT"), out var port) ? port : 5432,
                    Database = Environment.GetEnvironmentVariable("POI_DB_NAME") ?? "POIcsharpApp",
                    Username = Environment.GetEnvironmentVariable("POI_DB_USER") ?? "postgres",
                    Password = Environment.GetEnvironmentVariable("POI_DB_PASSWORD") ?? "1234",
                    SslMode = SslMode.Disable,
                    TrustServerCertificate = true
                };

                dbConnectionString = csb.ConnectionString;
            }

            // Register services
            builder.Services.AddSingleton<LocationService>();
            builder.Services.AddSingleton<StartupWarmupService>();
            builder.Services.AddSingleton(new PostgresDbOptions { ConnectionString = dbConnectionString });
            builder.Services.AddSingleton<PostgresPoiService>();

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
