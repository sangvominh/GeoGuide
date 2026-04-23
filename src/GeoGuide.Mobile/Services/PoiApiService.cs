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

    public async Task PostPlaybackLogAsync(PlaybackLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (!_options.EnablePlaybackLogs)
        {
            return;
        }

        using var response = await _httpClient.PostAsJsonAsync("api/v1/logs/playback", entry, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<SessionJoinResponse> JoinSessionAsync(SessionJoinRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/v1/sessions/join", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SessionJoinResponse>(JsonOptions, cancellationToken);
        return payload ?? new SessionJoinResponse
        {
            SessionToken = request.SessionToken,
            DeviceId = request.DeviceId,
            ClientType = request.ClientType,
            AccessMode = request.AccessMode,
            JoinedAt = request.JoinedAt,
            ServerTime = DateTimeOffset.UtcNow
        };
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
}
