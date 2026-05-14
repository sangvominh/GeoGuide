using System.Globalization;
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

    public async Task<SyncPayload> GetSyncBootstrapAsync(CancellationToken cancellationToken = default)
    {
        var payload = await _httpClient.GetFromJsonAsync<SyncPayload>("api/v1/sync/bootstrap", JsonOptions, cancellationToken);
        return payload ?? new SyncPayload();
    }

    public async Task<SyncPayload> GetSyncDeltaAsync(DateTimeOffset lastSyncAt, CancellationToken cancellationToken = default)
    {
        var query = Uri.EscapeDataString(lastSyncAt.ToString("O"));
        var payload = await _httpClient.GetFromJsonAsync<SyncPayload>($"api/v1/sync/delta?lastSyncAt={query}", JsonOptions, cancellationToken);
        return payload ?? new SyncPayload();
    }

    public async Task<IReadOnlyList<PointOfInterest>> GetPoisAsync(CancellationToken cancellationToken = default)
    {
        var payload = await GetSyncBootstrapAsync(cancellationToken);

        return payload.Pois
            .Where(static poi => poi.IsActive && !poi.IsDeleted)
            .Select(MapToPoi)
            .OrderByDescending(static poi => poi.Priority)
            .ThenBy(static poi => poi.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<PointOfInterest>> GetAllPoisAsync(string languageCode, CancellationToken cancellationToken = default)
    {
        var query = Uri.EscapeDataString(languageCode);
        var content = await _httpClient.GetFromJsonAsync<List<SyncPoiDto>>($"api/v1/poi/load-all?lang={query}", JsonOptions, cancellationToken)
            ?? new List<SyncPoiDto>();

        return content
            .Where(poi => poi.IsActive && !poi.IsDeleted)
            .Select(poi => MapToPoi(poi, languageCode))
            .OrderByDescending(poi => poi.Priority)
            .ThenBy(poi => poi.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<PointOfInterest>> GetNearbyPoisAsync(double latitude, double longitude, string languageCode, CancellationToken cancellationToken = default)
    {
        var query = $"api/v1/poi/nearby?lat={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lang={Uri.EscapeDataString(languageCode)}";
        var content = await _httpClient.GetFromJsonAsync<List<SyncPoiDto>>(query, JsonOptions, cancellationToken)
            ?? new List<SyncPoiDto>();

        return content
            .Where(poi => poi.IsActive && !poi.IsDeleted)
            .Select(poi => MapToPoi(poi, languageCode))
            .OrderByDescending(poi => poi.Priority)
            .ThenBy(poi => poi.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task PrepareLocalizationHotsetAsync(string languageCode, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/v1/localizations/prepare-hotset?lang={Uri.EscapeDataString(languageCode)}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<LocalizationResponse?> RequestLocalizationOnDemandAsync(string poiId, string languageCode, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            poiId,
            languageCode
        };

        using var response = await _httpClient.PostAsJsonAsync("api/v1/localizations/on-demand", payload, JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LocalizationResponse>(JsonOptions, cancellationToken);
    }

    public async Task PostPlaybackLogAsync(PlaybackLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (!_options.EnablePlaybackLogs)
        {
            return;
        }

        using var response = await _httpClient.PostAsJsonAsync("api/v1/logs/playback", entry, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static PointOfInterest MapToPoi(SyncPoiDto source)
    {
        var preferredContent = source.Contents
            .OrderBy(static content => string.Equals(content.LanguageCode, "vi-VN", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(static content => string.IsNullOrWhiteSpace(content.TtsContent) ? 1 : 0)
            .ThenBy(static content => string.IsNullOrWhiteSpace(content.AudioUrl) ? 1 : 0)
            .FirstOrDefault();

        return new PointOfInterest
        {
            Id = source.Id.ToString(),
            Name = source.Name,
            Description = source.Description,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            TriggerRadiusMeters = source.TriggerRadiusMeters,
            CooldownMinutes = source.CooldownMinutes,
            Priority = source.Priority,
            CategoryKey = source.CategoryKey,
            CategoryLabel = source.CategoryLabel,
            ImageUrl = source.ImageUrl,
            MapUrl = source.MapUrl,
            AudioUrl = preferredContent?.AudioUrl ?? string.Empty,
            TtsScript = preferredContent?.TtsContent ?? string.Empty,
            LanguageCode = preferredContent?.LanguageCode ?? "vi-VN",
            IsActive = source.IsActive && !source.IsDeleted,
            UpdatedAt = source.UpdatedAt
        };
    }

    private static PointOfInterest MapToPoi(SyncPoiDto source, string requestedLanguage)
    {
        var preferredContent = source.Contents
            .OrderBy(content => string.Equals(content.LanguageCode, requestedLanguage, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(content => string.IsNullOrWhiteSpace(content.TtsContent) ? 1 : 0)
            .ThenBy(content => string.IsNullOrWhiteSpace(content.AudioUrl) ? 1 : 0)
            .FirstOrDefault();

        return new PointOfInterest
        {
            Id = source.Id.ToString(),
            Name = source.Name,
            Description = source.Description,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            TriggerRadiusMeters = source.TriggerRadiusMeters,
            CooldownMinutes = source.CooldownMinutes,
            Priority = source.Priority,
            CategoryKey = source.CategoryKey,
            CategoryLabel = source.CategoryLabel,
            ImageUrl = source.ImageUrl,
            MapUrl = source.MapUrl,
            AudioUrl = preferredContent?.AudioUrl ?? string.Empty,
            TtsScript = preferredContent?.TtsContent ?? string.Empty,
            LanguageCode = preferredContent?.LanguageCode ?? requestedLanguage,
            IsActive = source.IsActive && !source.IsDeleted,
            UpdatedAt = source.UpdatedAt
        };
    }
}
