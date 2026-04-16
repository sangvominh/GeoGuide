using MauiApp1.Models;

namespace MauiApp1.Services;

public sealed class TriggerGuardService
{
    private readonly Dictionary<string, DateTimeOffset> _enteredAtByPoiId = [];
    private readonly Dictionary<string, DateTimeOffset> _lastTriggeredAtByPoiId = [];

    public TimeSpan DebounceDuration { get; set; } = TimeSpan.FromSeconds(8);

    public bool CanTrigger(PointOfInterest poi, DateTimeOffset nowUtc)
    {
        if (poi.DistanceMeters > poi.TriggerRadiusMeters)
        {
            _enteredAtByPoiId.Remove(poi.Id);
            return false;
        }

        if (!_enteredAtByPoiId.TryGetValue(poi.Id, out var enteredAt))
        {
            _enteredAtByPoiId[poi.Id] = nowUtc;
            return false;
        }

        if (nowUtc - enteredAt < DebounceDuration)
        {
            return false;
        }

        var cooldownMinutes = poi.CooldownMinutes > 0 ? poi.CooldownMinutes : 5;
        var cooldown = TimeSpan.FromMinutes(cooldownMinutes);
        if (_lastTriggeredAtByPoiId.TryGetValue(poi.Id, out var lastTriggeredAt)
            && nowUtc - lastTriggeredAt < cooldown)
        {
            return false;
        }

        _lastTriggeredAtByPoiId[poi.Id] = nowUtc;
        _enteredAtByPoiId[poi.Id] = nowUtc;
        return true;
    }
}
