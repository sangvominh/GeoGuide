using MauiApp1.Models;

namespace MauiApp1.Services;

public enum NarrationPlaybackState
{
    Queued,
    Started,
    Completed,
    Failed
}

public sealed class NarrationPlaybackEventArgs : EventArgs
{
    public string PoiId { get; init; } = string.Empty;
    public string PoiName { get; init; } = string.Empty;
    public NarrationPlaybackState State { get; init; }
    public string Message { get; init; } = string.Empty;
}

internal sealed class NarrationQueueItem
{
    public required PointOfInterest Poi { get; init; }
    public required string TriggerType { get; init; }
    public required bool IsManualTrigger { get; init; }
    public required Func<Uri, CancellationToken, Task>? PlayAudioAsync { get; init; }
    public int Priority => IsManualTrigger ? 0 : 1;
}

public class NarrationService
{
    private const string DeviceIdPreferenceKey = "mobile_device_id";
    private readonly PoiApiService _poiApiService;
    private readonly MediaPrefetchService _mediaPrefetchService;
    private readonly PriorityQueue<NarrationQueueItem, int> _queue = new();
    private readonly HashSet<string> _queuedPoiIds = [];
    private readonly SemaphoreSlim _queueLock = new(1, 1);
    private bool _isProcessingQueue;
    private NarrationQueueItem? _currentItem;
    private CancellationTokenSource? _currentPlaybackCts;

    public event EventHandler<NarrationPlaybackEventArgs>? PlaybackChanged;
    public bool IsBusy => _currentItem != null || _queue.Count > 0;

    public NarrationService(PoiApiService poiApiService, MediaPrefetchService mediaPrefetchService)
    {
        _poiApiService = poiApiService;
        _mediaPrefetchService = mediaPrefetchService;
    }

    public async Task EnqueueAsync(
        PointOfInterest poi,
        string triggerType,
        bool isManualTrigger,
        Func<Uri, CancellationToken, Task>? playAudioAsync = null,
        CancellationToken cancellationToken = default)
    {
        var item = new NarrationQueueItem
        {
            Poi = poi,
            TriggerType = triggerType,
            IsManualTrigger = isManualTrigger,
            PlayAudioAsync = playAudioAsync
        };

        var shouldStartWorker = false;

        await _queueLock.WaitAsync(cancellationToken);
        try
        {
            if (_currentItem?.Poi.Id == poi.Id || _queuedPoiIds.Contains(poi.Id))
            {
                return;
            }

            _queue.Enqueue(item, item.Priority);
            _queuedPoiIds.Add(poi.Id);
            PublishPlaybackState(item.Poi, NarrationPlaybackState.Queued, $"Đã thêm vào hàng đợi: {item.Poi.Name}");

            if (_currentItem != null && item.Priority < _currentItem.Priority)
            {
                _currentPlaybackCts?.Cancel();
            }

            if (!_isProcessingQueue)
            {
                _isProcessingQueue = true;
                shouldStartWorker = true;
            }
        }
        finally
        {
            _queueLock.Release();
        }

        if (shouldStartWorker)
        {
            _ = ProcessQueueAsync();
        }
    }

    private async Task ProcessQueueAsync()
    {
        while (true)
        {
            NarrationQueueItem? item = null;

            await _queueLock.WaitAsync();
            try
            {
                if (!_queue.TryDequeue(out item, out _))
                {
                    _isProcessingQueue = false;
                    return;
                }

                _queuedPoiIds.Remove(item.Poi.Id);
                _currentItem = item;
                _currentPlaybackCts = new CancellationTokenSource();
            }
            finally
            {
                _queueLock.Release();
            }

            var startedAt = DateTimeOffset.UtcNow;

            try
            {
                PublishPlaybackState(item.Poi, NarrationPlaybackState.Started, $"Đang phát: {item.Poi.Name}");
                await ExecutePlaybackAsync(item, _currentPlaybackCts.Token);
                PublishPlaybackState(item.Poi, NarrationPlaybackState.Completed, $"Đã phát xong: {item.Poi.Name}");
                _ = TryLogPlaybackAsync(CreateLogEntry(item.Poi, item.TriggerType, startedAt));
            }
            catch (OperationCanceledException)
            {
                PublishPlaybackState(item.Poi, NarrationPlaybackState.Queued, $"Ưu tiên nội dung khác, tạm dừng: {item.Poi.Name}");
            }
            catch (Exception ex)
            {
                PublishPlaybackState(item.Poi, NarrationPlaybackState.Failed, $"Lỗi phát thuyết minh: {ex.Message}");
            }
            finally
            {
                await _queueLock.WaitAsync();
                try
                {
                    _currentPlaybackCts?.Dispose();
                    _currentPlaybackCts = null;
                    _currentItem = null;
                }
                finally
                {
                    _queueLock.Release();
                }
            }
        }
    }

    private async Task ExecutePlaybackAsync(NarrationQueueItem item, CancellationToken cancellationToken)
    {
        var audioUri = await _mediaPrefetchService.ResolvePlaybackUriAsync(item.Poi.AudioUrl, cancellationToken);
        if (audioUri != null && item.PlayAudioAsync != null)
        {
            await item.PlayAudioAsync(audioUri, cancellationToken);
            return;
        }

        var narrationText = BuildNarrationText(item.Poi);
        var locale = await ResolveLocaleAsync(item.Poi.LanguageCode);
        EnsureSupportedVoice(item.Poi.LanguageCode, locale);
        await SpeakWithFallbackAsync(narrationText, locale, cancellationToken);
    }

    private async Task<Locale?> ResolveLocaleAsync(string languageCode)
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        var normalizedCode = (languageCode ?? string.Empty).Trim();
        var languagePrefix = normalizedCode.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];

        return locales.FirstOrDefault(locale => string.Equals(locale.Language, normalizedCode, StringComparison.OrdinalIgnoreCase))
            ?? locales.FirstOrDefault(locale => locale.Language.StartsWith(languagePrefix, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task SpeakWithFallbackAsync(string narrationText, Locale? locale, CancellationToken cancellationToken)
    {
        if (locale == null)
        {
            await TextToSpeech.Default.SpeakAsync(narrationText, new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 1.0f
            }, cancellationToken);
            return;
        }

        await TextToSpeech.Default.SpeakAsync(narrationText, new SpeechOptions
        {
            Locale = locale,
            Pitch = 1.0f,
            Volume = 1.0f
        }, cancellationToken);
    }

    private static string BuildNarrationText(PointOfInterest poi)
    {
        if (!string.IsNullOrWhiteSpace(poi.TtsScript))
        {
            return poi.TtsScript.Trim();
        }

        var parts = new List<string> { poi.Name.Trim() };

        if (!string.IsNullOrWhiteSpace(poi.Description))
        {
            parts.Add(poi.Description.Trim());
        }

        if (!string.IsNullOrWhiteSpace(poi.CategoryLabel))
        {
            parts.Add($"Đây là điểm {poi.CategoryLabel.ToLowerInvariant()} nổi bật trong khu vực.");
        }

        return string.Join(". ", parts.Where(static part => !string.IsNullOrWhiteSpace(part)));
    }

    private async Task TryLogPlaybackAsync(PlaybackLogEntry entry)
    {
        try
        {
            await _poiApiService.PostPlaybackLogAsync(entry);
        }
        catch
        {
        }
    }

    private PlaybackLogEntry CreateLogEntry(PointOfInterest poi, string triggerType, DateTimeOffset startedAt)
    {
        return new PlaybackLogEntry
        {
            PoiId = poi.Id,
            PlayedAt = DateTimeOffset.UtcNow,
            TriggerType = triggerType,
            DurationSeconds = Math.Max(1, (int)Math.Round((DateTimeOffset.UtcNow - startedAt).TotalSeconds)),
            DeviceId = GetOrCreateDeviceId()
        };
    }

    private static void EnsureSupportedVoice(string languageCode, Locale? locale)
    {
        var normalizedCode = (languageCode ?? string.Empty).Trim();
        if (!normalizedCode.StartsWith("vi", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (locale == null)
        {
            throw new InvalidOperationException("Thiết bị chưa có giọng đọc tiếng Việt. Hãy dùng audio từ backend hoặc cài gói speech tiếng Việt.");
        }
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

    private void PublishPlaybackState(PointOfInterest poi, NarrationPlaybackState state, string message)
    {
        PlaybackChanged?.Invoke(this, new NarrationPlaybackEventArgs
        {
            PoiId = poi.Id,
            PoiName = poi.Name,
            State = state,
            Message = message
        });
    }
}
