using MauiApp1.Models;
using Npgsql;
using System.Globalization;

namespace MauiApp1.Services;

public class PostgresPoiService
{
    private readonly string _connectionString;

    public PostgresPoiService(PostgresDbOptions options)
    {
        _connectionString = options.ConnectionString;
    }

    public async Task<IReadOnlyList<PointOfInterest>> GetNearbyRegisteredPoisAsync(
        double latitude,
        double longitude,
        int maxItems = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return Array.Empty<PointOfInterest>();
        }

        const string sql = """
            SELECT
                id,
                name,
                COALESCE(category_key, 'other') AS category_key,
                COALESCE(category_label, 'Dia diem khac') AS category_label,
                COALESCE(description, '') AS description,
                latitude,
                longitude,
                has_audio,
                ROUND(
                    6371000 * 2 * ASIN(
                        SQRT(
                            POWER(SIN(RADIANS((latitude - @lat) / 2)), 2) +
                            COS(RADIANS(@lat)) * COS(RADIANS(latitude)) *
                            POWER(SIN(RADIANS((longitude - @lon) / 2)), 2)
                        )
                    )
                ) AS distance_meters
            FROM poi
            WHERE is_registered = TRUE
            ORDER BY distance_meters ASC
            LIMIT @maxItems;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("lat", latitude);
        command.Parameters.AddWithValue("lon", longitude);
        command.Parameters.AddWithValue("maxItems", maxItems);

        var result = new List<PointOfInterest>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var categoryKey = reader.GetString(reader.GetOrdinal("category_key"));
            var categoryLabel = reader.GetString(reader.GetOrdinal("category_label"));
            var distanceMeters = reader.GetDouble(reader.GetOrdinal("distance_meters"));

            result.Add(new PointOfInterest
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")).ToString("N"),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CategoryKey = categoryKey,
                Category = categoryLabel,
                Description = reader.GetString(reader.GetOrdinal("description")),
                Latitude = reader.GetDouble(reader.GetOrdinal("latitude")),
                Longitude = reader.GetDouble(reader.GetOrdinal("longitude")),
                Distance = MauiApp1.Utils.GeoUtils.FormatDistance(distanceMeters),
                HasAudio = reader.GetBoolean(reader.GetOrdinal("has_audio")),
                AudioStatus = reader.GetBoolean(reader.GetOrdinal("has_audio")) ? "AUDIO" : "NONE",
                IconGlyph = MauiApp1.Utils.CategoryUtils.ToIconGlyph(categoryKey, useMaterialIcons: true),
                Rating = 0
            });
        }

        return result;
    }
}
