namespace GeoGuide.Cms.Models;

public class QrSessionAdminViewModel
{
    public string SessionToken { get; init; } = string.Empty;
    public string AccessMode { get; init; } = "full";
    public string PayloadText { get; init; } = string.Empty;
    public string QrImageUrl { get; init; } = string.Empty;
    public IReadOnlyList<AnalyticsSessionDeviceRow> Devices { get; init; } = [];
}
