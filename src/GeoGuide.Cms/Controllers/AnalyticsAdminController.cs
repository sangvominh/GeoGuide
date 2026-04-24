using GeoGuide.Cms.Data;
using GeoGuide.Cms.Models;
using GeoGuide.Cms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoGuide.Cms.Controllers;

[Authorize]
public class AnalyticsAdminController(AnalyticsQueryService analyticsQueryService) : Controller
{
    public async Task<IActionResult> Index(
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        string? sessionToken = null)
    {
        var snapshot = await analyticsQueryService.BuildSnapshotAsync(startDate, endDate, sessionToken);
        var range = analyticsQueryService.NormalizeRange(startDate, endDate);

        var vm = new AnalyticsDashboardViewModel
        {
            StartDate = snapshot.StartDate,
            EndDate = snapshot.EndDate,
            StartDateInput = range.StartDateInput,
            EndDateInput = range.EndDateInput,
            SessionTokenInput = snapshot.SessionToken,
            TotalDevices = snapshot.TotalDevices,
            TotalSessionJoins = snapshot.TotalSessionJoins,
            TotalListens = snapshot.TotalListens,
            AverageDurationSeconds = snapshot.AverageDurationSeconds,
            TotalBehaviorEvents = snapshot.TotalBehaviorEvents,
            TotalStops = snapshot.TotalStops,
            AverageStopDurationSeconds = snapshot.AverageStopDurationSeconds,
            TopPois = snapshot.TopPois,
            TopVisitedPois = snapshot.TopVisitedPois,
            HeatmapPoints = snapshot.HeatmapPoints,
            SessionDevices = snapshot.SessionDevices
        };

        return View(vm);
    }
}
