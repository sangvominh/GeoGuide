namespace GeoGuide.Cms.Models;

public class QrSessionAdminViewModel
{
    public string SessionToken { get; init; } = string.Empty;
    public string AccessMode { get; init; } = "full";
    public string PublicBaseUrl { get; init; } = string.Empty;
    public string JoinUrl { get; init; } = string.Empty;
    public string DeepLinkUrl { get; init; } = string.Empty;
    public string AndroidApkUrl { get; init; } = string.Empty;
    public bool HasAndroidApk => !string.IsNullOrWhiteSpace(AndroidApkUrl);
    public string QrImageUrl { get; init; } = string.Empty;
    public IReadOnlyList<AnalyticsSessionDeviceRow> Devices { get; init; } = [];
}
