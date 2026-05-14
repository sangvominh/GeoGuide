using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.Pages;

public partial class DemoStatusPage : ContentPage
{
    private readonly PoiApiOptions _apiOptions;
    private readonly PoiRepository _poiRepository;
    private readonly LocalDatabaseService _localDatabaseService;
    private readonly NarrationService _narrationService;
    private readonly LocationService _locationService;

    public DemoStatusPage()
    {
        InitializeComponent();
        
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider not available.");

        _apiOptions = services.GetRequiredService<PoiApiOptions>();
        _poiRepository = services.GetRequiredService<PoiRepository>();
        _localDatabaseService = services.GetRequiredService<LocalDatabaseService>();
        _narrationService = services.GetRequiredService<NarrationService>();
        _locationService = services.GetRequiredService<LocationService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshStatusAsync();
    }

    private async Task RefreshStatusAsync()
    {
        ApiUrlLabel.Text = _apiOptions.BaseUrl;
        
        var lang = Preferences.Default.Get("app_language", "vi-VN");
        LanguageLabel.Text = lang;

        var loadResult = await _poiRepository.GetPoisAsync();
        SyncResultLabel.Text = loadResult.DataSource.ToString();
        PoiCountLabel.Text = loadResult.Pois.Count.ToString();

        var location = await _locationService.GetLastKnownLocationAsync();
        int nearbyCount = 0;
        if (location != null)
        {
            var nearby = await _poiRepository.GetNearbyPoisAsync(location);
            nearbyCount = nearby.Count;
        }
        HotsetResultLabel.Text = location != null ? $"{nearbyCount} nearby POIs" : "No location";

        LocalizationStatusLabel.Text = loadResult.DataSource == PoiDataSource.Api ? "Available" : "Offline fallback";

        FallbackModeLabel.Text = loadResult.DataSource != PoiDataSource.Api ? "Active (Offline TTS/Bundled)" : "Inactive (Using API)";

        var logs = await _localDatabaseService.GetPendingOfflineLogsAsync(100);
        QueueCountLabel.Text = logs.Count.ToString();
    }

    private async void OnRefreshPoisClicked(object sender, EventArgs e)
    {
        ActionStatusLabel.Text = "Refreshing POIs...";
        ActionStatusLabel.TextColor = Colors.Orange;
        await _poiRepository.GetPoisAsync();
        await RefreshStatusAsync();
        ActionStatusLabel.Text = "POIs refreshed";
        ActionStatusLabel.TextColor = Colors.Green;
    }

    private async void OnPrepareHotsetClicked(object sender, EventArgs e)
    {
        ActionStatusLabel.Text = "Preparing hotset...";
        ActionStatusLabel.TextColor = Colors.Orange;
        await _poiRepository.PrepareLocalizationHotsetAsync();
        await RefreshStatusAsync();
        ActionStatusLabel.Text = "Hotset prepared";
        ActionStatusLabel.TextColor = Colors.Green;
    }

    private async void OnTestNarrationClicked(object sender, EventArgs e)
    {
        ActionStatusLabel.Text = "Testing narration...";
        ActionStatusLabel.TextColor = Colors.Orange;
        
        var loadResult = await _poiRepository.GetPoisAsync();
        if (loadResult.Pois.Count > 0)
        {
            var randomPoi = loadResult.Pois[new Random().Next(loadResult.Pois.Count)];
            try
            {
                await _narrationService.EnqueueAsync(randomPoi, "demo-test", true, async (uri, ct) => {
                    MainThread.BeginInvokeOnMainThread(() => {
                        ActionStatusLabel.Text = $"Audio ready for {randomPoi.Name}\nURI: {uri}";
                        ActionStatusLabel.TextColor = Colors.Green;
                    });
                    await Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                ActionStatusLabel.Text = $"Error: {ex.Message}";
                ActionStatusLabel.TextColor = Colors.Red;
            }
        }
        else
        {
            ActionStatusLabel.Text = "No POIs available to test.";
            ActionStatusLabel.TextColor = Colors.Red;
        }
    }
}
