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
                        var categoryKey = ToCategoryKey(amenity, leisure, tourism);
                        var cuisine = GetTagValue(item, "cuisine");
                        var name = GetTagValue(item, "name") ?? BuildFallbackName(categoryKey, item);
                        var distanceMeters = CalculateDistanceMeters(latitude, longitude, lat, lon);

                        pois.Add(new PointOfInterest
                        {
                            Id = item.TryGetProperty("id", out var idProperty) ? idProperty.ToString() : Guid.NewGuid().ToString("N"),
                            Name = name,
                            CategoryKey = categoryKey,
                            Category = ToCategoryLabel(categoryKey),
                            Description = BuildDescription(categoryKey, amenity, leisure, tourism, cuisine),
                            Distance = FormatDistance(distanceMeters),
                            Latitude = lat,
                            Longitude = lon,
                            IconGlyph = ToIconGlyph(categoryKey),
                            HasAudio = true,
                            AudioStatus = "AUDIO",
                            Rating = 0
                        });
                    }

                    var sorted = pois
                        .OrderBy(p => ParseDistanceToMeters(p.Distance))
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

        private static string ToCategoryKey(string? amenity, string? leisure, string? tourism)
        {
            if (amenity is "restaurant" or "fast_food" or "food_court")
            {
                return "food";
            }

            if (amenity is "cafe" or "bar")
            {
                return "cafe";
            }

            if (amenity is "theatre" or "cinema" or "arts_centre")
            {
                return "theatre";
            }

            if (leisure is "park" or "garden" or "nature_reserve")
            {
                return "park";
            }

            if (leisure is "playground" or "sports_centre" || tourism == "theme_park")
            {
                return "play";
            }

            if (tourism is "attraction" or "museum" or "zoo")
            {
                return "attraction";
            }

            return "other";
        }

        private static string ToCategoryLabel(string categoryKey)
        {
            return categoryKey switch
            {
                "food" => "Quan an",
                "cafe" => "Cafe",
                "park" => "Cong vien",
                "play" => "Khu vui choi",
                "theatre" => "Nha hat",
                "attraction" => "Tham quan",
                _ => "Dia diem khac"
            };
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

        private static string ToIconGlyph(string categoryKey)
        {
            return categoryKey switch
            {
                "food" => "\ue56c",
                "cafe" => "\ue541",
                "park" => "\ueb2f",
                "play" => "\uea44",
                "theatre" => "\ue03d",
                "attraction" => "\ue56b",
                _ => "\ue55f"
            };
        }

        private static string FormatDistance(double meters)
        {
            if (meters < 1000)
            {
                return $"{Math.Round(meters)}m";
            }

            return $"{meters / 1000:0.0}km";
        }

        private static double ParseDistanceToMeters(string value)
        {
            if (value.EndsWith("km", StringComparison.OrdinalIgnoreCase)
                && double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var km))
            {
                return km * 1000;
            }

            if (value.EndsWith("m", StringComparison.OrdinalIgnoreCase)
                && double.TryParse(value[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var m))
            {
                return m;
            }

            return double.MaxValue;
        }

        private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6371000;
            var lat1Rad = DegreesToRadians(lat1);
            var lat2Rad = DegreesToRadians(lat2);
            var deltaLat = DegreesToRadians(lat2 - lat1);
            var deltaLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
                    + Math.Cos(lat1Rad) * Math.Cos(lat2Rad)
                    * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadius * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
