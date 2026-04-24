using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Services;

public sealed class AnalyticsQueryService(ApplicationDbContext dbContext)
{
    private const double StopClusterDistanceMeters = 25;
    private const int StopClusterGapMinutes = 5;
    private const int LongStopThresholdSeconds = 60;
    private const double EarthRadiusMeters = 6_371_000;

    public async Task<AnalyticsSnapshot> BuildSnapshotAsync(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        string? sessionToken = null,
        CancellationToken cancellationToken = default)
    {
        var range = NormalizeRange(startDate, endDate);
        var normalizedSessionToken = NormalizeSessionToken(sessionToken);

        var playbackQuery = dbContext.PlaybackLogs
            .Where(log => log.PlayedAt >= range.StartUtc && log.PlayedAt < range.EndExclusiveUtc);
        var joinQuery = dbContext.DeviceSessionJoins
            .Where(row => row.JoinedAt >= range.StartUtc && row.JoinedAt < range.EndExclusiveUtc);
        var behaviorQuery = dbContext.BehaviorEvents
            .Where(row => row.OccurredAt >= range.StartUtc && row.OccurredAt < range.EndExclusiveUtc);

        if (!string.IsNullOrWhiteSpace(normalizedSessionToken))
        {
            playbackQuery = playbackQuery.Where(log => log.SessionToken == normalizedSessionToken);
            joinQuery = joinQuery.Where(row => row.SessionToken == normalizedSessionToken);
            behaviorQuery = behaviorQuery.Where(row => row.SessionToken == normalizedSessionToken);
        }

        var totalDevices = (await joinQuery
                .Select(row => row.DeviceId)
                .Union(playbackQuery.Select(log => log.DeviceId))
                .Union(behaviorQuery.Select(row => row.DeviceId))
                .Distinct()
                .ToListAsync(cancellationToken))
            .Count;

        var totalSessionJoins = await joinQuery.CountAsync(cancellationToken);
        var totalListens = await playbackQuery.CountAsync(cancellationToken);
        var totalBehaviorEvents = await behaviorQuery.CountAsync(cancellationToken);
        var averageDurationSeconds = await playbackQuery.AnyAsync(cancellationToken)
            ? (int)Math.Round(await playbackQuery.AverageAsync(log => log.DurationSeconds, cancellationToken))
            : 0;

        var topPoiStats = await playbackQuery
            .GroupBy(log => log.PoiId)
            .Select(group => new
            {
                PoiId = group.Key,
                ListenCount = group.Count(),
                AverageDurationSeconds = (int)Math.Round(group.Average(x => x.DurationSeconds))
            })
            .OrderByDescending(row => row.ListenCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        var poiIds = topPoiStats.Select(row => row.PoiId).ToHashSet();
        var playbackLogs = await playbackQuery
            .OrderBy(row => row.PlayedAt)
            .ToListAsync(cancellationToken);
        var behaviorEvents = await behaviorQuery
            .OrderBy(row => row.OccurredAt)
            .ToListAsync(cancellationToken);
        foreach (var poiId in behaviorEvents.Where(row => row.PoiId.HasValue).Select(row => row.PoiId!.Value))
        {
            poiIds.Add(poiId);
        }

        var poiLite = await dbContext.Pois
            .IgnoreQueryFilters()
            .Where(poi => poiIds.Contains(poi.Id))
            .Select(poi => new PoiLite
            {
                Id = poi.Id,
                Name = poi.Name,
                Latitude = poi.Latitude,
                Longitude = poi.Longitude,
                TriggerRadiusMeters = poi.TriggerRadiusMeters
            })
            .ToListAsync(cancellationToken);
        var poiLookup = poiLite.ToDictionary(poi => poi.Id);

        var topPois = topPoiStats
            .Select(row => new AnalyticsTopPoiRow
            {
                PoiId = row.PoiId,
                Name = poiLookup.TryGetValue(row.PoiId, out var poi) ? poi.Name : "POI không xác định",
                ListenCount = row.ListenCount,
                AverageDurationSeconds = row.AverageDurationSeconds
            })
            .OrderByDescending(row => row.ListenCount)
            .ThenBy(row => row.Name)
            .ToList();

        var positionEvents = behaviorEvents
            .Where(row => row.EventType == "position" && row.Latitude.HasValue && row.Longitude.HasValue)
            .ToList();

        var heatmap = BuildHeatmap(positionEvents, playbackLogs, poiLookup);
        var deviceJourneyAggregates = BuildDeviceJourneys(
            await joinQuery.OrderBy(row => row.JoinedAt).ToListAsync(cancellationToken),
            playbackLogs,
            behaviorEvents,
            poiLite);

        var stopRows = deviceJourneyAggregates
            .SelectMany(row => row.StopSummaries)
            .Where(row => row.PoiId.HasValue)
            .GroupBy(row => row.PoiId!.Value)
            .Select(group =>
            {
                poiLookup.TryGetValue(group.Key, out var poi);
                return new AnalyticsVisitPoiRow
                {
                    PoiId = group.Key,
                    Name = poi?.Name ?? "POI không xác định",
                    VisitCount = group.Count(),
                    AverageDwellSeconds = (int)Math.Round(group.Average(x => x.DwellSeconds))
                };
            })
            .OrderByDescending(row => row.VisitCount)
            .ThenByDescending(row => row.AverageDwellSeconds)
            .Take(10)
            .ToList();

        var allStops = deviceJourneyAggregates.SelectMany(row => row.StopSummaries).ToList();
        var averageStopDurationSeconds = allStops.Count == 0
            ? 0
            : (int)Math.Round(allStops.Average(row => row.DwellSeconds));

        return new AnalyticsSnapshot
        {
            StartDate = range.StartUtc,
            EndDate = range.EndExclusiveUtc.AddTicks(-1),
            SessionToken = normalizedSessionToken ?? string.Empty,
            TotalDevices = totalDevices,
            TotalSessionJoins = totalSessionJoins,
            TotalListens = totalListens,
            AverageDurationSeconds = averageDurationSeconds,
            TotalBehaviorEvents = totalBehaviorEvents,
            TotalStops = allStops.Count,
            AverageStopDurationSeconds = averageStopDurationSeconds,
            TopPois = topPois,
            TopVisitedPois = stopRows,
            HeatmapPoints = heatmap,
            SessionDevices = deviceJourneyAggregates.Select(row => row.DeviceRow).ToList()
        };
    }

    public AnalyticsRange NormalizeRange(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        var localToday = DateTime.Today;
        var startLocalDate = startDate?.Date ?? localToday.AddDays(-30);
        var endLocalDate = endDate?.Date ?? localToday;

        if (startLocalDate > endLocalDate)
        {
            (startLocalDate, endLocalDate) = (endLocalDate, startLocalDate);
        }

        var startLocal = new DateTimeOffset(startLocalDate, TimeZoneInfo.Local.GetUtcOffset(startLocalDate));
        var endExclusiveLocalDate = endLocalDate.AddDays(1);
        var endExclusiveLocal = new DateTimeOffset(endExclusiveLocalDate, TimeZoneInfo.Local.GetUtcOffset(endExclusiveLocalDate));

        return new AnalyticsRange
        {
            StartUtc = startLocal.ToUniversalTime(),
            EndExclusiveUtc = endExclusiveLocal.ToUniversalTime(),
            StartDateInput = startLocalDate.ToString("yyyy-MM-dd"),
            EndDateInput = endLocalDate.ToString("yyyy-MM-dd")
        };
    }

    public static string? NormalizeSessionToken(string? sessionToken)
    {
        return string.IsNullOrWhiteSpace(sessionToken) ? null : sessionToken.Trim();
    }

    private static IReadOnlyList<AnalyticsHeatmapRow> BuildHeatmap(
        IReadOnlyList<BehaviorEvent> positionEvents,
        IReadOnlyList<PlaybackLog> playbackLogs,
        IReadOnlyDictionary<Guid, PoiLite> poiLookup)
    {
        if (positionEvents.Count > 0)
        {
            return positionEvents
                .GroupBy(row => new
                {
                    Lat = Math.Round(row.Latitude!.Value, 4),
                    Lng = Math.Round(row.Longitude!.Value, 4)
                })
                .Select(group => new AnalyticsHeatmapRow
                {
                    Lat = group.Key.Lat,
                    Lng = group.Key.Lng,
                    Weight = group.Count()
                })
                .OrderByDescending(row => row.Weight)
                .Take(500)
                .ToList();
        }

        return playbackLogs
            .Where(log => poiLookup.ContainsKey(log.PoiId))
            .GroupBy(log => new
            {
                Lat = Math.Round(poiLookup[log.PoiId].Latitude, 4),
                Lng = Math.Round(poiLookup[log.PoiId].Longitude, 4)
            })
            .Select(group => new AnalyticsHeatmapRow
            {
                Lat = group.Key.Lat,
                Lng = group.Key.Lng,
                Weight = group.Count()
            })
            .OrderByDescending(row => row.Weight)
            .Take(500)
            .ToList();
    }

    private static IReadOnlyList<DeviceJourneyAggregate> BuildDeviceJourneys(
        IReadOnlyList<DeviceSessionJoin> joins,
        IReadOnlyList<PlaybackLog> playbackLogs,
        IReadOnlyList<BehaviorEvent> behaviorEvents,
        IReadOnlyList<PoiLite> pois)
    {
        var playbackByDevice = playbackLogs.GroupBy(row => row.DeviceId).ToDictionary(group => group.Key, group => group.ToList());
        var behaviorByDevice = behaviorEvents.GroupBy(row => row.DeviceId).ToDictionary(group => group.Key, group => group.ToList());
        var devices = joins
            .Select(row => row.DeviceId)
            .Union(playbackLogs.Select(row => row.DeviceId))
            .Union(behaviorEvents.Select(row => row.DeviceId))
            .Distinct()
            .OrderBy(row => row, StringComparer.Ordinal)
            .ToList();

        return devices
            .Select(deviceId =>
            {
                var join = joins.FirstOrDefault(row => row.DeviceId == deviceId);
                var devicePlayback = playbackByDevice.TryGetValue(deviceId, out var playbackRows)
                    ? playbackRows.OrderByDescending(row => row.PlayedAt).ToList()
                    : [];
                var deviceBehavior = behaviorByDevice.TryGetValue(deviceId, out var behaviorRows)
                    ? behaviorRows.OrderBy(row => row.OccurredAt).ToList()
                    : [];
                var stops = BuildStops(deviceBehavior, pois);
                var orderedRouteNames = stops
                    .Where(row => !string.IsNullOrWhiteSpace(row.PoiName))
                    .Select(row => row.PoiName!)
                    .Concat(devicePlayback
                        .OrderBy(row => row.PlayedAt)
                        .Select(row => pois.FirstOrDefault(poi => poi.Id == row.PoiId)?.Name)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Select(name => name!))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(6)
                    .ToList();
                var lastPosition = deviceBehavior
                    .Where(row => row.EventType == "position" && row.Latitude.HasValue && row.Longitude.HasValue)
                    .OrderByDescending(row => row.OccurredAt)
                    .FirstOrDefault();
                var lastPoiName = devicePlayback.Count == 0
                    ? string.Empty
                    : pois.FirstOrDefault(poi => poi.Id == devicePlayback[0].PoiId)?.Name ?? string.Empty;

                return new DeviceJourneyAggregate
                {
                    DeviceRow = new AnalyticsSessionDeviceRow
                    {
                        DeviceId = deviceId,
                        ClientType = join?.ClientType ?? deviceBehavior.LastOrDefault()?.ClientType ?? devicePlayback.FirstOrDefault()?.ClientType ?? "mobile",
                        AccessMode = join?.AccessMode ?? "trial",
                        JoinedAt = join?.JoinedAt ?? deviceBehavior.FirstOrDefault()?.OccurredAt ?? devicePlayback.LastOrDefault()?.PlayedAt ?? DateTimeOffset.MinValue,
                        LastSeenAt = join?.LastSeenAt
                            ?? deviceBehavior.LastOrDefault()?.OccurredAt
                            ?? devicePlayback.FirstOrDefault()?.PlayedAt
                            ?? DateTimeOffset.MinValue,
                        ListenCount = devicePlayback.Count,
                        TotalDurationSeconds = devicePlayback.Sum(row => row.DurationSeconds),
                        LastPoiName = lastPoiName,
                        BehaviorEventCount = deviceBehavior.Count,
                        StopCount = stops.Count,
                        LongestStopSeconds = stops.Count == 0 ? 0 : stops.Max(row => row.DwellSeconds),
                        RouteSummary = orderedRouteNames.Count == 0 ? "Chưa đủ dữ liệu hành trình" : string.Join(" → ", orderedRouteNames),
                        LastKnownLatitude = lastPosition?.Latitude,
                        LastKnownLongitude = lastPosition?.Longitude
                    },
                    StopSummaries = stops
                };
            })
            .OrderByDescending(row => row.DeviceRow.LastSeenAt)
            .ToList();
    }

    private static IReadOnlyList<DeviceStopSummary> BuildStops(IReadOnlyList<BehaviorEvent> deviceBehavior, IReadOnlyList<PoiLite> pois)
    {
        var positions = deviceBehavior
            .Where(row => row.EventType == "position" && row.Latitude.HasValue && row.Longitude.HasValue)
            .OrderBy(row => row.OccurredAt)
            .ToList();
        if (positions.Count < 2)
        {
            return [];
        }

        var stops = new List<DeviceStopSummary>();
        var cluster = new List<BehaviorEvent> { positions[0] };

        for (var index = 1; index < positions.Count; index++)
        {
            var current = positions[index];
            var previous = positions[index - 1];
            var gap = current.OccurredAt - previous.OccurredAt;
            var distance = CalculateDistanceMeters(
                previous.Latitude!.Value,
                previous.Longitude!.Value,
                current.Latitude!.Value,
                current.Longitude!.Value);

            if (gap <= TimeSpan.FromMinutes(StopClusterGapMinutes) && distance <= StopClusterDistanceMeters)
            {
                cluster.Add(current);
                continue;
            }

            AppendStopIfEligible(cluster, stops, pois);
            cluster = [current];
        }

        AppendStopIfEligible(cluster, stops, pois);
        return stops;
    }

    private static void AppendStopIfEligible(
        IReadOnlyList<BehaviorEvent> cluster,
        ICollection<DeviceStopSummary> stops,
        IReadOnlyList<PoiLite> pois)
    {
        if (cluster.Count < 2)
        {
            return;
        }

        var first = cluster[0];
        var last = cluster[^1];
        var dwellSeconds = (int)Math.Round((last.OccurredAt - first.OccurredAt).TotalSeconds);
        if (dwellSeconds < LongStopThresholdSeconds)
        {
            return;
        }

        var averageLat = cluster.Average(row => row.Latitude ?? 0);
        var averageLng = cluster.Average(row => row.Longitude ?? 0);
        var nearestPoi = pois
            .Select(poi => new
            {
                Poi = poi,
                Distance = CalculateDistanceMeters(averageLat, averageLng, poi.Latitude, poi.Longitude)
            })
            .Where(row => row.Distance <= Math.Max(row.Poi.TriggerRadiusMeters, 40))
            .OrderBy(row => row.Distance)
            .FirstOrDefault();

        stops.Add(new DeviceStopSummary
        {
            PoiId = nearestPoi?.Poi.Id,
            PoiName = nearestPoi?.Poi.Name,
            DwellSeconds = dwellSeconds
        });
    }

    private static double CalculateDistanceMeters(double startLatitude, double startLongitude, double endLatitude, double endLongitude)
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

    public sealed class AnalyticsRange
    {
        public required DateTimeOffset StartUtc { get; init; }
        public required DateTimeOffset EndExclusiveUtc { get; init; }
        public required string StartDateInput { get; init; }
        public required string EndDateInput { get; init; }
    }

    private sealed class PoiLite
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public double TriggerRadiusMeters { get; init; }
    }

    private sealed class DeviceJourneyAggregate
    {
        public required AnalyticsSessionDeviceRow DeviceRow { get; init; }
        public required IReadOnlyList<DeviceStopSummary> StopSummaries { get; init; }
    }

    private sealed class DeviceStopSummary
    {
        public Guid? PoiId { get; init; }
        public string? PoiName { get; init; }
        public int DwellSeconds { get; init; }
    }
}
