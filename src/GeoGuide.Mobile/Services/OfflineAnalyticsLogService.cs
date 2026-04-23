namespace MauiApp1.Services;

public sealed class OfflineAnalyticsLogService
{
    private const int SyncStatusPending = 0;
    private const int SyncStatusSynced = 1;
    private const int SyncStatusSkipped = 2;
    private readonly LocalDatabaseService _localDatabaseService;
    private readonly PoiApiService _poiApiService;
    private readonly DeviceIdentityService _deviceIdentityService;
    private readonly AccessModeService _accessModeService;

    public OfflineAnalyticsLogService(
        LocalDatabaseService localDatabaseService,
        PoiApiService poiApiService,
        DeviceIdentityService deviceIdentityService,
        AccessModeService accessModeService)
    {
        _localDatabaseService = localDatabaseService;
        _poiApiService = poiApiService;
        _deviceIdentityService = deviceIdentityService;
        _accessModeService = accessModeService;
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

    public async Task<int> SyncPendingLogsAsync(int batchSize = 50, CancellationToken cancellationToken = default)
    {
        var pendingLogs = await _localDatabaseService.GetPendingOfflineLogsAsync(batchSize, cancellationToken);
        if (pendingLogs.Count == 0)
        {
            return 0;
        }

        var syncedCount = 0;
        foreach (var log in pendingLogs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (log.SyncStatus != SyncStatusPending)
            {
                continue;
            }

            if (log.EventType != (int)OfflineAnalyticsEventType.AudioCompleted)
            {
                await _localDatabaseService.UpdateOfflineLogSyncStatusAsync(log.Id, SyncStatusSkipped, cancellationToken);
                continue;
            }

            if (!Guid.TryParse(log.PoiId, out _))
            {
                await _localDatabaseService.UpdateOfflineLogSyncStatusAsync(log.Id, SyncStatusSkipped, cancellationToken);
                continue;
            }

            var entry = new Models.PlaybackLogEntry
            {
                PoiId = log.PoiId!,
                PlayedAt = DateTimeOffset.TryParse(log.TimestampUtcIso, out var playedAt) ? playedAt : DateTimeOffset.UtcNow,
                TriggerType = "manual",
                DurationSeconds = Math.Max(1, log.DurationSeconds),
                DeviceId = string.IsNullOrWhiteSpace(log.DeviceId) ? _deviceIdentityService.GetOrCreateDeviceId() : log.DeviceId,
                SessionToken = string.IsNullOrWhiteSpace(log.SessionToken) ? _accessModeService.GetState().SessionToken : log.SessionToken,
                ClientType = string.IsNullOrWhiteSpace(log.ClientType) ? "mobile" : log.ClientType
            };

            try
            {
                await _poiApiService.PostPlaybackLogAsync(entry, cancellationToken);
                await _localDatabaseService.UpdateOfflineLogSyncStatusAsync(log.Id, SyncStatusSynced, cancellationToken);
                syncedCount++;
            }
            catch
            {
                break;
            }
        }

        return syncedCount;
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
            DeviceId = _deviceIdentityService.GetOrCreateDeviceId(),
            PoiId = poiId,
            EventType = (int)eventType,
            Latitude = latitude,
            Longitude = longitude,
            DurationSeconds = Math.Max(0, durationSeconds),
            SessionToken = _accessModeService.GetState().SessionToken,
            ClientType = "mobile",
            TimestampUtcIso = DateTimeOffset.UtcNow.ToString("O"),
            SyncStatus = SyncStatusPending
        };

        await _localDatabaseService.InsertOfflineLogAsync(record, cancellationToken);
    }
}
