using MauiApp1.Models;

namespace MauiApp1.Services;

public sealed class GeofenceEngineService
{
    private const double EarthRadiusMeters = 6_371_000;

    public IReadOnlyList<PointOfInterest> BuildRankedSnapshot(IReadOnlyList<PointOfInterest> pois, Location? currentLocation)
    {
        if (currentLocation == null)
        {
            return pois
                .Select(poi =>
                {
                    poi.DistanceMeters = double.MaxValue;
                    return poi;
                })
                .OrderByDescending(static poi => poi.Priority)
                .ThenBy(static poi => poi.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        var currentLat = currentLocation.Latitude;
        var currentLng = currentLocation.Longitude;

        return pois
            .Select(poi =>
            {
                poi.DistanceMeters = CalculateDistanceMeters(
                    currentLat,
                    currentLng,
                    poi.Latitude,
                    poi.Longitude);
                return poi;
            })
            .OrderBy(static poi => poi.DistanceMeters)
            .ThenByDescending(static poi => poi.Priority)
            .ToList();
    }

    public PointOfInterest? SelectNearest(IReadOnlyList<PointOfInterest> rankedPois) =>
        rankedPois.FirstOrDefault();

    public PointOfInterest? SelectTriggerCandidate(IReadOnlyList<PointOfInterest> rankedPois)
    {
        return rankedPois
            .Where(static poi => poi.IsActive && poi.DistanceMeters <= poi.TriggerRadiusMeters)
            .OrderBy(static poi => poi.DistanceMeters)
            .ThenByDescending(static poi => poi.Priority)
            .FirstOrDefault();
    }

    public double CalculateDistanceMeters(double startLatitude, double startLongitude, double endLatitude, double endLongitude)
    {
        var latitudeDeltaRadians = DegreesToRadians(endLatitude - startLatitude);
        var longitudeDeltaRadians = DegreesToRadians(endLongitude - startLongitude);
        var startLatitudeRadians = DegreesToRadians(startLatitude);
        var endLatitudeRadians = DegreesToRadians(endLatitude);

        var haversine =
            Math.Sin(latitudeDeltaRadians / 2) * Math.Sin(latitudeDeltaRadians / 2) +
            Math.Cos(startLatitudeRadians) * Math.Cos(endLatitudeRadians) *
            Math.Sin(longitudeDeltaRadians / 2) * Math.Sin(longitudeDeltaRadians / 2);

        var angularDistance = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
        return EarthRadiusMeters * angularDistance;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180d);
}
