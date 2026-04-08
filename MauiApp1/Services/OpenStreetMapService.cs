using MauiApp1.Models;
using System.Globalization;
using System.Text.Json;

namespace MauiApp1.Services
{
    public class OpenStreetMapService
    {
        private static readonly string[] OverpassEndpoints =
        {
            "https://overpass-api.de/api/interpreter",
            "https://overpass.kumi.systems/api/interpreter"
        };

        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        static OpenStreetMapService()
        {
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MauiApp1/1.0 (SmartAudioGuide)");
        }

        public async Task<IReadOnlyList<PointOfInterest>> GetNearbyPlacesAsync(
            double latitude,
            double longitude,
            int radiusMeters = 1500,
            int maxItems = 10,
            CancellationToken cancellationToken = default)
        {
            var searchRadii = new[] { radiusMeters, 3000, 5000 };

            foreach (var radius in searchRadii)
            {
                var query = BuildOverpassQuery(latitude, longitude, radius);
                JsonDocument? document = null;

                foreach (var endpoint in OverpassEndpoints)
                {
                    try
                    {
                        document = await QueryOverpassAsync(endpoint, query, cancellationToken);
                        break;
                    }
                    catch
                    {
                        // Try next endpoint.
                    }
                }

                if (document == null)
                {
                    continue;
                }

                using (document)
                {
                    if (!document.RootElement.TryGetProperty("elements", out var elements))
                    {
                        continue;
                    }

                    var pois = new List<PointOfInterest>();

                    foreach (var item in elements.EnumerateArray())
                    {
                        if (!TryReadCoordinates(item, out var lat, out var lon))
                        {
                            continue;
                        }

                        var amenity = GetTagValue(item, "amenity");
                        var leisure = GetTagValue(item, "leisure");
                        var tourism = GetTagValue(item, "tourism");
                        var categoryKey = MauiApp1.Utils.CategoryUtils.ToCategoryKey(amenity, leisure, tourism);
                        var cuisine = GetTagValue(item, "cuisine");
                        var name = GetTagValue(item, "name") ?? BuildFallbackName(categoryKey, item);
                        var distanceMeters = MauiApp1.Utils.GeoUtils.CalculateDistanceMeters(latitude, longitude, lat, lon);

                        pois.Add(new PointOfInterest
                        {
                            Id = item.TryGetProperty("id", out var idProperty) ? idProperty.ToString() : Guid.NewGuid().ToString("N"),
                            Name = name,
                            CategoryKey = categoryKey,
                            Category = MauiApp1.Utils.CategoryUtils.ToCategoryLabel(categoryKey),
                            Description = BuildDescription(categoryKey, amenity, leisure, tourism, cuisine),
                            Distance = MauiApp1.Utils.GeoUtils.FormatDistance(distanceMeters),
                            Latitude = lat,
                            Longitude = lon,
                            IconGlyph = MauiApp1.Utils.CategoryUtils.ToIconGlyph(categoryKey),
                            HasAudio = true,
                            AudioStatus = "AUDIO",
                            Rating = 0
                        });
                    }

                    var sorted = pois
                        .OrderBy(p => MauiApp1.Utils.GeoUtils.ParseDistanceToMeters(p.Distance))
                        .Take(maxItems)
                        .ToList();

                    if (sorted.Count > 0)
                    {
                        return sorted;
                    }
                }
            }

            return Array.Empty<PointOfInterest>();
        }

        private static string BuildFallbackName(string categoryKey, JsonElement item)
        {
            var label = ToCategoryLabel(categoryKey);
            var id = item.TryGetProperty("id", out var idProperty) ? idProperty.ToString() : "0";
            return $"{label} #{id}";
        }

        private static async Task<JsonDocument> QueryOverpassAsync(string endpoint, string query, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("data", query)
                })
            };

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);
        }

        private static string BuildOverpassQuery(double latitude, double longitude, int radiusMeters)
        {
            var lat = latitude.ToString("F6", CultureInfo.InvariantCulture);
            var lon = longitude.ToString("F6", CultureInfo.InvariantCulture);

                 return $"[out:json][timeout:5];("
                     + $"node[\"amenity\"~\"restaurant|cafe|fast_food|food_court|bar|theatre|cinema|arts_centre\"](around:{radiusMeters},{lat},{lon});"
                     + $"way[\"amenity\"~\"restaurant|cafe|fast_food|food_court|bar|theatre|cinema|arts_centre\"](around:{radiusMeters},{lat},{lon});"
                     + $"relation[\"amenity\"~\"restaurant|cafe|fast_food|food_court|bar|theatre|cinema|arts_centre\"](around:{radiusMeters},{lat},{lon});"
                     + $"node[\"leisure\"~\"park|garden|playground|sports_centre|nature_reserve\"](around:{radiusMeters},{lat},{lon});"
                     + $"way[\"leisure\"~\"park|garden|playground|sports_centre|nature_reserve\"](around:{radiusMeters},{lat},{lon});"
                     + $"relation[\"leisure\"~\"park|garden|playground|sports_centre|nature_reserve\"](around:{radiusMeters},{lat},{lon});"
                     + $"node[\"tourism\"~\"attraction|theme_park|museum|zoo\"](around:{radiusMeters},{lat},{lon});"
                     + $"way[\"tourism\"~\"attraction|theme_park|museum|zoo\"](around:{radiusMeters},{lat},{lon});"
                     + $"relation[\"tourism\"~\"attraction|theme_park|museum|zoo\"](around:{radiusMeters},{lat},{lon});"
                     + ");out center;";
        }

        private static string? GetTagValue(JsonElement element, string key)
        {
            if (!element.TryGetProperty("tags", out var tags))
            {
                return null;
            }

            return tags.TryGetProperty(key, out var value) ? value.GetString() : null;
        }

        private static bool TryReadCoordinates(JsonElement element, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;

            if (element.TryGetProperty("lat", out var latElement) && element.TryGetProperty("lon", out var lonElement))
            {
                latitude = latElement.GetDouble();
                longitude = lonElement.GetDouble();
                return true;
            }

            if (element.TryGetProperty("center", out var center)
                && center.TryGetProperty("lat", out var centerLat)
                && center.TryGetProperty("lon", out var centerLon))
            {
                latitude = centerLat.GetDouble();
                longitude = centerLon.GetDouble();
                return true;
            }

            return false;
        }

        private static string BuildDescription(string categoryKey, string? amenity, string? leisure, string? tourism, string? cuisine)
        {
            if (!string.IsNullOrWhiteSpace(cuisine) && categoryKey is "food" or "cafe")
            {
                return $"Cuisine: {cuisine.Replace(';', ',')}";
            }

            return categoryKey switch
            {
                "food" => "Diem an uong gan ban",
                "cafe" => "Khong gian thu gian va do uong",
                "park" => "Khong gian xanh thu gian",
                "play" => "Khu vui choi va hoat dong",
                "theatre" => "Khong gian nghe thuat giai tri",
                "attraction" => "Diem tham quan noi bat",
                _ => $"OSM tags: {amenity ?? leisure ?? tourism ?? "unknown"}"
            };
        }

    }
}
