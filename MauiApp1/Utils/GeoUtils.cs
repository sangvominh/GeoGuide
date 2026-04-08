using System;
using System.Globalization;

namespace MauiApp1.Utils;

public static class GeoUtils
{
    public static string FormatDistance(double meters)
    {
        if (meters < 1000)
        {
            return $"{Math.Round(meters)}m";
        }

        return $"{meters / 1000:0.0}km";
    }

    public static double ParseDistanceToMeters(string value)
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

    public static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
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
