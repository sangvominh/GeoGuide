namespace MauiApp1.Services;

public sealed class OfflineAnalyticsLogService
{
    private const string DeviceIdPreferenceKey = "mobile_device_id";
    private readonly LocalDatabaseService _localDatabaseService;

    public OfflineAnalyticsLogService(LocalDatabaseService localDatabaseService)
    {
        _localDatabaseService = localDatabaseService;
    }

    public Task LogPositionUpdateAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        return LogAsync(
            poiId: null,
            eventType: OfflineAnalyticsEventType.PositionUpdate,
            latitude: latitude,
            longitude: longitude,
            durationSeconds: 0,
            cancellationToken: cancellationToken);
    }

    public Task LogAudioStartedAsync(string poiId, double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        return LogAsync(
            poiId: poiId,
            eventType: OfflineAnalyticsEventType.AudioStarted,
            latitude: latitude,
            longitude: longitude,
            durationSeconds: 0,
            cancellationToken: cancellationToken);
    }

    public Task LogAudioCompletedAsync(string poiId, double latitude, double longitude, int durationSeconds, CancellationToken cancellationToken = default)
    {
        return LogAsync(
            poiId: poiId,
            eventType: OfflineAnalyticsEventType.AudioCompleted,
            latitude: latitude,
            longitude: longitude,
            durationSeconds: durationSeconds,
            cancellationToken: cancellationToken);
    }

    public Task LogQrScannedAsync(string token, CancellationToken cancellationToken = default)
    {
        return LogAsync(
            poiId: token,
            eventType: OfflineAnalyticsEventType.QrScanned,
            latitude: 0,
            longitude: 0,
            durationSeconds: 0,
            cancellationToken: cancellationToken);
    }

    private async Task LogAsync(
        string? poiId,
        OfflineAnalyticsEventType eventType,
        double latitude,
        double longitude,
        int durationSeconds,
        CancellationToken cancellationToken)
    {
        var record = new LocalOfflineLogRecord
        {
            Id = Guid.NewGuid().ToString("N"),
            DeviceId = GetOrCreateDeviceId(),
            PoiId = poiId,
            EventType = (int)eventType,
            Latitude = latitude,
            Longitude = longitude,
            DurationSeconds = Math.Max(0, durationSeconds),
            TimestampUtcIso = DateTimeOffset.UtcNow.ToString("O"),
            SyncStatus = 0
        };

        await _localDatabaseService.InsertOfflineLogAsync(record, cancellationToken);
    }

    private static string GetOrCreateDeviceId()
    {
        var existing = Preferences.Default.Get(DeviceIdPreferenceKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var created = Guid.NewGuid().ToString("N");
        Preferences.Default.Set(DeviceIdPreferenceKey, created);
        return created;
    }
}
