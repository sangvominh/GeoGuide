using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models.Api;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/analytics")]
public class AnalyticsV1Controller(AnalyticsQueryService analyticsQueryService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? sessionToken = null)
    {
        var snapshot = await analyticsQueryService.BuildSnapshotAsync(startDate, endDate, sessionToken);

        var response = new AnalyticsDashboardDto
        {
            StartDate = snapshot.StartDate,
            EndDate = snapshot.EndDate,
            SessionToken = snapshot.SessionToken,
            TotalDevices = snapshot.TotalDevices,
            TotalSessionJoins = snapshot.TotalSessionJoins,
            TotalListens = snapshot.TotalListens,
            AverageDurationSeconds = snapshot.AverageDurationSeconds,
            TotalBehaviorEvents = snapshot.TotalBehaviorEvents,
            TotalStops = snapshot.TotalStops,
            AverageStopDurationSeconds = snapshot.AverageStopDurationSeconds,
            TopPois = snapshot.TopPois.Select(row => new TopPoiAnalyticsDto
            {
                PoiId = row.PoiId,
                Name = row.Name,
                ListenCount = row.ListenCount,
                AverageDurationSeconds = row.AverageDurationSeconds
            }).ToList(),
            TopVisitedPois = snapshot.TopVisitedPois.Select(row => new VisitedPoiAnalyticsDto
            {
                PoiId = row.PoiId,
                Name = row.Name,
                VisitCount = row.VisitCount,
                AverageDwellSeconds = row.AverageDwellSeconds
            }).ToList(),
            Devices = snapshot.SessionDevices.Select(row => new SessionDeviceDto
            {
                DeviceId = row.DeviceId,
                ClientType = row.ClientType,
                AccessMode = row.AccessMode,
                JoinedAt = row.JoinedAt,
                LastSeenAt = row.LastSeenAt,
                ListenCount = row.ListenCount,
                TotalDurationSeconds = row.TotalDurationSeconds,
                LastPoiName = row.LastPoiName,
                BehaviorEventCount = row.BehaviorEventCount,
                StopCount = row.StopCount,
                LongestStopSeconds = row.LongestStopSeconds,
                RouteSummary = row.RouteSummary,
                LastKnownLatitude = row.LastKnownLatitude,
                LastKnownLongitude = row.LastKnownLongitude
            }).ToList()
        };

        return Ok(response);
    }

    [HttpGet("heatmap")]
    public async Task<IActionResult> Heatmap(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? sessionToken = null)
    {
        var snapshot = await analyticsQueryService.BuildSnapshotAsync(startDate, endDate, sessionToken);
        var heatmap = snapshot.HeatmapPoints
            .Select(row => new HeatmapPointDto
            {
                Lat = row.Lat,
                Lng = row.Lng,
                Weight = row.Weight
            })
            .ToList();

        return Ok(heatmap);
    }

    [HttpGet("sessions/{sessionToken}/devices")]
    public async Task<IActionResult> SessionDevices([FromRoute] string sessionToken)
    {
        var normalizedSessionToken = AnalyticsQueryService.NormalizeSessionToken(sessionToken);
        if (string.IsNullOrWhiteSpace(normalizedSessionToken))
        {
            return BadRequest();
        }

        var snapshot = await analyticsQueryService.BuildSnapshotAsync(sessionToken: normalizedSessionToken);
        var devices = snapshot.SessionDevices.Select(row => new SessionDeviceDto
        {
            DeviceId = row.DeviceId,
            ClientType = row.ClientType,
            AccessMode = row.AccessMode,
            JoinedAt = row.JoinedAt,
            LastSeenAt = row.LastSeenAt,
            ListenCount = row.ListenCount,
            TotalDurationSeconds = row.TotalDurationSeconds,
            LastPoiName = row.LastPoiName,
            BehaviorEventCount = row.BehaviorEventCount,
            StopCount = row.StopCount,
            LongestStopSeconds = row.LongestStopSeconds,
            RouteSummary = row.RouteSummary,
            LastKnownLatitude = row.LastKnownLatitude,
            LastKnownLongitude = row.LastKnownLongitude
        }).ToList();
        return Ok(devices);
    }
}
