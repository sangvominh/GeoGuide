using System.Net.Http.Json;
using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1.Services;

public class PoiApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly PoiApiOptions _options;

    public PoiApiService(HttpClient httpClient, PoiApiOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<IReadOnlyList<PointOfInterest>> GetPoisAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/pois", cancellationToken);
        response.EnsureSuccessStatusCode();

        var pois = await response.Content.ReadFromJsonAsync<List<PointOfInterest>>(JsonOptions, cancellationToken)
            ?? [];

        return pois
            .Where(static poi => poi.IsActive)
            .OrderByDescending(static poi => poi.Priority)
            .ThenBy(static poi => poi.Name)
            .ToList();
    }

    public async Task PostPlaybackLogAsync(PlaybackLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (!_options.EnablePlaybackLogs)
        {
            return;
        }

        using var response = await _httpClient.PostAsJsonAsync("api/logs/playback", entry, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
