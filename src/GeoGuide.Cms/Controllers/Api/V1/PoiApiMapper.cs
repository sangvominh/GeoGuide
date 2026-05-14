using GeoGuide.Cms.Models;
using GeoGuide.Cms.Models.Api;

namespace GeoGuide.Cms.Controllers.Api.V1;

internal static class PoiApiMapper
{
    private const double EarthRadiusMeters = 6371000;

    public static PoiDto MapPoi(Poi poi, string? languageCode = null, double? distanceMeters = null)
    {
        var fallbackChain = BuildFallbackChain(languageCode);
        var contents = SelectContents(poi.Audios, fallbackChain);
        var resolvedLanguage = contents.FirstOrDefault()?.LanguageCode ?? fallbackChain[0];

        return new PoiDto
        {
            Id = poi.Id,
            Name = poi.Name,
            Description = poi.Description,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            TriggerRadiusMeters = poi.TriggerRadiusMeters,
            CooldownMinutes = poi.CooldownMinutes,
            Priority = poi.Priority,
            CategoryKey = poi.CategoryKey,
            CategoryLabel = poi.CategoryLabel,
            ImageUrl = poi.ImageUrl,
            MapUrl = poi.MapUrl,
            IsActive = poi.IsActive,
            IsDeleted = poi.IsDeleted,
            UpdatedAt = poi.UpdatedAt,
            DistanceMeters = distanceMeters,
            RequestedLanguage = fallbackChain[0],
            ResolvedLanguage = resolvedLanguage,
            FallbackChain = fallbackChain,
            Contents = contents
        };
    }

    public static double CalculateDistanceMeters(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude)
    {
        var fromLatRadians = DegreesToRadians(fromLatitude);
        var toLatRadians = DegreesToRadians(toLatitude);
        var latDelta = DegreesToRadians(toLatitude - fromLatitude);
        var lngDelta = DegreesToRadians(toLongitude - fromLongitude);

        var a = Math.Sin(latDelta / 2) * Math.Sin(latDelta / 2)
            + Math.Cos(fromLatRadians) * Math.Cos(toLatRadians)
            * Math.Sin(lngDelta / 2) * Math.Sin(lngDelta / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    private static IReadOnlyList<PoiContentDto> SelectContents(IEnumerable<PoiAudio> audios, IReadOnlyList<string> fallbackChain)
    {
        return audios
            .Where(a => !a.IsDeleted)
            .GroupBy(a => a.ContentType)
            .Select(group => group
                .OrderBy(a => GetFallbackRank(a.LanguageCode, fallbackChain))
                .ThenByDescending(a => a.UpdatedAt)
                .FirstOrDefault(a => GetFallbackRank(a.LanguageCode, fallbackChain) < fallbackChain.Count))
            .Where(a => a is not null)
            .Select(a => new PoiContentDto
            {
                Id = a!.Id,
                LanguageCode = a.LanguageCode,
                ContentType = a.ContentType.ToString(),
                IsFallback = !string.Equals(a.LanguageCode, fallbackChain[0], StringComparison.OrdinalIgnoreCase),
                AudioUrl = a.AudioUrl,
                TtsContent = a.TtsContent,
                UpdatedAt = a.UpdatedAt
            })
            .OrderBy(c => c.ContentType)
            .ThenBy(c => c.LanguageCode)
            .ToList();
    }

    private static IReadOnlyList<string> BuildFallbackChain(string? languageCode)
    {
        var target = NormalizeLanguage(languageCode);
        var chain = new List<string> { target };

        AddIfMissing(chain, "en");
        AddIfMissing(chain, "vi");

        return chain;
    }

    private static string NormalizeLanguage(string? languageCode)
    {
        var normalized = languageCode?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? "en" : normalized;
    }

    private static int GetFallbackRank(string languageCode, IReadOnlyList<string> fallbackChain)
    {
        var normalized = NormalizeLanguage(languageCode);
        for (var i = 0; i < fallbackChain.Count; i++)
        {
            if (string.Equals(fallbackChain[i], normalized, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return fallbackChain.Count;
    }

    private static void AddIfMissing(List<string> chain, string languageCode)
    {
        if (!chain.Contains(languageCode, StringComparer.OrdinalIgnoreCase))
        {
            chain.Add(languageCode);
        }
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
