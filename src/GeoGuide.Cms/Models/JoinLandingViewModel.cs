namespace GeoGuide.Cms.Models;

public class JoinLandingViewModel
{
    public string SessionToken { get; init; } = string.Empty;
    public string AccessMode { get; init; } = "full";
    public string JoinUrl { get; init; } = string.Empty;
    public string DeepLinkUrl { get; init; } = string.Empty;
    public string AndroidApkUrl { get; init; } = string.Empty;
    public bool HasAndroidApk => !string.IsNullOrWhiteSpace(AndroidApkUrl);
}
