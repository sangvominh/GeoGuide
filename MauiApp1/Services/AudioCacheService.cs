using System.Globalization;
using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public sealed class AudioCacheEntry
    {
        public string PoiId { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = "en-US";
        public DateTime CachedAtUtc { get; set; }
        public DateTime LastAccessUtc { get; set; }
    }

    public class AudioCacheService
    {
        private const string AudioCacheIndexKey = "audio_cache_index_v1";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task PrefetchNearbyAudioAsync(
            IEnumerable<PointOfInterest> pois,
            string languageCode,
            int maxItems,
            CancellationToken cancellationToken = default)
        {
            var cache = ReadCacheIndex();
            var now = DateTime.UtcNow;

            foreach (var poi in pois.Take(maxItems))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var existing = cache.FirstOrDefault(c => c.PoiId == poi.Id && c.LanguageCode == languageCode);
                if (existing != null)
                {
                    existing.LastAccessUtc = now;
                }
                else
                {
                    cache.Add(new AudioCacheEntry
                    {
                        PoiId = poi.Id,
                        LanguageCode = languageCode,
                        CachedAtUtc = now,
                        LastAccessUtc = now
                    });
                }

                // Simulate a short prefetch task; actual audio download is integrated later.
                await Task.Delay(40, cancellationToken);
            }

            WriteCacheIndex(cache);
        }

        public Task<int> PruneByLruAsync(
            string activeLanguageCode,
            TimeSpan maxAge,
            int keepMaxItems = 30)
        {
            var cache = ReadCacheIndex();
            var now = DateTime.UtcNow;

            var filtered = cache
                .Where(c => c.LanguageCode == activeLanguageCode)
                .Where(c => now - c.LastAccessUtc <= maxAge)
                .OrderByDescending(c => c.LastAccessUtc)
                .Take(keepMaxItems)
                .ToList();

            var removed = cache.Count - filtered.Count;
            WriteCacheIndex(filtered);
            return Task.FromResult(Math.Max(removed, 0));
        }

        public Task TouchAudioAsync(string poiId, string languageCode)
        {
            var cache = ReadCacheIndex();
            var now = DateTime.UtcNow;

            var existing = cache.FirstOrDefault(c => c.PoiId == poiId && c.LanguageCode == languageCode);
            if (existing != null)
            {
                existing.LastAccessUtc = now;
            }
            else
            {
                cache.Add(new AudioCacheEntry
                {
                    PoiId = poiId,
                    LanguageCode = languageCode,
                    CachedAtUtc = now,
                    LastAccessUtc = now
                });
            }

            WriteCacheIndex(cache);
            return Task.CompletedTask;
        }

        private static List<AudioCacheEntry> ReadCacheIndex()
        {
            var raw = Preferences.Default.Get(AudioCacheIndexKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<AudioCacheEntry>>(raw, JsonOptions) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static void WriteCacheIndex(List<AudioCacheEntry> entries)
        {
            var normalized = entries
                .Where(e => !string.IsNullOrWhiteSpace(e.PoiId))
                .GroupBy(e => string.Create(CultureInfo.InvariantCulture, $"{e.PoiId}|{e.LanguageCode}"))
                .Select(g => g.OrderByDescending(e => e.LastAccessUtc).First())
                .ToList();

            var json = JsonSerializer.Serialize(normalized, JsonOptions);
            Preferences.Default.Set(AudioCacheIndexKey, json);
        }
    }
}
