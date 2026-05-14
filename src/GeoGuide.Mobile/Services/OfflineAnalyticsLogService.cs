namespace MauiApp1.Services;

public sealed class OfflineAnalyticsLogService
{
    private const int SyncStatusPending = 0;
    private const int SyncStatusSynced = 1;
    private const int SyncStatusSkipped = 2;
    private static readonly TimeSpan BackgroundSyncThrottle = TimeSpan.FromSeconds(20);
    private readonly LocalDatabaseService _localDatabaseService;
    private readonly PoiApiService _poiApiService;
    private readonly DeviceIdentityService _deviceIdentityService;
    private readonly AccessModeService _accessModeService;
    private readonly SemaphoreSlim _backgroundSyncLock = new(1, 1);
    private DateTimeOffset _lastBackgroundSyncUtc = DateTimeOffset.MinValue;

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

            try
            {
                var occurredAt = DateTimeOffset.TryParse(log.TimestampUtcIso, out var parsedOccurredAt)
                    ? parsedOccurredAt
                    : DateTimeOffset.UtcNow;
                var deviceId = string.IsNullOrWhiteSpace(log.DeviceId)
                    ? _deviceIdentityService.GetOrCreateDeviceId()
                    : log.DeviceId;
                var sessionToken = string.IsNullOrWhiteSpace(log.SessionToken)
                    ? _accessModeService.GetState().SessionToken
                    : log.SessionToken;
                var clientType = string.IsNullOrWhiteSpace(log.ClientType) ? "mobile" : log.ClientType;

                await _poiApiService.PostBehaviorEventAsync(new Models.BehaviorEventEntry
                {
                    DeviceId = deviceId,
                    PoiId = log.PoiId,
                    EventType = MapEventType(log.EventType),
                    Latitude = log.Latitude,
                    Longitude = log.Longitude,
                    DurationSeconds = Math.Max(0, log.DurationSeconds),
                    OccurredAt = occurredAt,
                    SessionToken = sessionToken,
                    ClientType = clientType
                }, cancellationToken);

                if (log.EventType == (int)OfflineAnalyticsEventType.AudioCompleted && Guid.TryParse(log.PoiId, out _))
                {
                    await _poiApiService.PostPlaybackLogAsync(new Models.PlaybackLogEntry
                    {
                        PoiId = log.PoiId!,
                        PlayedAt = occurredAt,
                        TriggerType = "manual",
                        DurationSeconds = Math.Max(1, log.DurationSeconds),
                        DeviceId = deviceId,
                        SessionToken = sessionToken,
                        ClientType = clientType
                    }, cancellationToken);
                }

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

    public void TriggerBackgroundSync(int batchSize = 50)
    {
        _ = Task.Run(() => SyncPendingLogsThrottledAsync(batchSize));
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
        TriggerBackgroundSync();
    }

    private async Task SyncPendingLogsThrottledAsync(int batchSize)
    {
        if (DateTimeOffset.UtcNow - _lastBackgroundSyncUtc < BackgroundSyncThrottle)
        {
            return;
        }

        if (!await _backgroundSyncLock.WaitAsync(0))
        {
            return;
        }

        try
        {
            if (DateTimeOffset.UtcNow - _lastBackgroundSyncUtc < BackgroundSyncThrottle)
            {
                return;
            }

            _lastBackgroundSyncUtc = DateTimeOffset.UtcNow;
            await SyncPendingLogsAsync(batchSize);
        }
        catch
        {
        }
        finally
        {
            _backgroundSyncLock.Release();
        }
    }

    private static string MapEventType(int eventType)
    {
        return eventType switch
        {
            (int)OfflineAnalyticsEventType.PositionUpdate => "position",
            (int)OfflineAnalyticsEventType.AudioStarted => "audio_started",
            (int)OfflineAnalyticsEventType.AudioCompleted => "audio_completed",
            (int)OfflineAnalyticsEventType.QrScanned => "qr_scanned",
            _ => "position"
        };
    }
}
