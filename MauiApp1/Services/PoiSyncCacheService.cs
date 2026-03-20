using System.Globalization;
using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class PoiSyncCacheService
    {
        private const string CachedPoiKey = "poi_cache_v1";
        private const string PendingPoiKey = "poi_pending_refresh_v1";
        private const string PendingRefreshKey = "poi_has_pending_refresh_v1";
        private const string LastSyncUtcKey = "poi_last_sync_utc_v1";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public Task<IReadOnlyList<PointOfInterest>> GetCachedPoiAsync()
        {
            return Task.FromResult((IReadOnlyList<PointOfInterest>)ReadPoiList(CachedPoiKey));
        }

        public Task<IReadOnlyList<PointOfInterest>> GetPendingPoiAsync()
        {
            return Task.FromResult((IReadOnlyList<PointOfInterest>)ReadPoiList(PendingPoiKey));
        }

        public bool HasPendingRefresh()
        {
            return Preferences.Default.Get(PendingRefreshKey, false);
        }

        public DateTime? GetLastSyncUtc()
        {
            var raw = Preferences.Default.Get(LastSyncUtcKey, string.Empty);
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                return parsed;
            }

            return null;
        }

        public Task UpsertCachedPoiAsync(IEnumerable<PointOfInterest> incoming)
        {
            var existing = ReadPoiList(CachedPoiKey);
            var merged = MergePoi(existing, incoming);
            WritePoiList(CachedPoiKey, merged);
            Preferences.Default.Set(LastSyncUtcKey, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            return Task.CompletedTask;
        }

        public Task<bool> StageIncomingRefreshAsync(IEnumerable<PointOfInterest> incoming)
        {
            var current = ReadPoiList(CachedPoiKey);
            var merged = MergePoi(current, incoming);

            if (AreEquivalent(current, merged))
            {
                return Task.FromResult(false);
            }

            WritePoiList(PendingPoiKey, merged);
            Preferences.Default.Set(PendingRefreshKey, true);
            Preferences.Default.Set(LastSyncUtcKey, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            return Task.FromResult(true);
        }

        public async Task<IReadOnlyList<PointOfInterest>> ApplyPendingRefreshAsync()
        {
            if (!HasPendingRefresh())
            {
                return await GetCachedPoiAsync();
            }

            var pending = ReadPoiList(PendingPoiKey);
            WritePoiList(CachedPoiKey, pending);
            ClearPendingRefresh();
            return pending;
        }

        public void ClearPendingRefresh()
        {
            Preferences.Default.Set(PendingRefreshKey, false);
            Preferences.Default.Remove(PendingPoiKey);
        }

        private static List<PointOfInterest> ReadPoiList(string key)
        {
            var raw = Preferences.Default.Get(key, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<PointOfInterest>>(raw, JsonOptions) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static void WritePoiList(string key, List<PointOfInterest> pois)
        {
            var normalized = pois
                .Where(p => !string.IsNullOrWhiteSpace(p.Id))
                .GroupBy(p => p.Id)
                .Select(g => g.Last())
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var json = JsonSerializer.Serialize(normalized, JsonOptions);
            Preferences.Default.Set(key, json);
        }

        private static List<PointOfInterest> MergePoi(IEnumerable<PointOfInterest> existing, IEnumerable<PointOfInterest> incoming)
        {
            var map = new Dictionary<string, PointOfInterest>(StringComparer.OrdinalIgnoreCase);

            foreach (var poi in existing)
            {
                if (!string.IsNullOrWhiteSpace(poi.Id))
                {
                    map[poi.Id] = poi;
                }
            }

            foreach (var poi in incoming)
            {
                if (!string.IsNullOrWhiteSpace(poi.Id))
                {
                    map[poi.Id] = poi;
                }
            }

            return map.Values.ToList();
        }

        private static bool AreEquivalent(IReadOnlyCollection<PointOfInterest> left, IReadOnlyCollection<PointOfInterest> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            var leftSignature = BuildSignature(left);
            var rightSignature = BuildSignature(right);
            return string.Equals(leftSignature, rightSignature, StringComparison.Ordinal);
        }

        private static string BuildSignature(IEnumerable<PointOfInterest> pois)
        {
            var pieces = pois
                .OrderBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                .Select(p => string.Join('|',
                    p.Id,
                    p.Name,
                    p.CategoryKey,
                    Math.Round(p.Latitude, 6).ToString(CultureInfo.InvariantCulture),
                    Math.Round(p.Longitude, 6).ToString(CultureInfo.InvariantCulture),
                    p.Distance,
                    p.Description,
                    p.AudioStatus));

            return string.Join(';', pieces);
        }
    }
}
