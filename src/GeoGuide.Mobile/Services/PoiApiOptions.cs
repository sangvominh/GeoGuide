namespace MauiApp1.Services;

public class PoiApiOptions
{
    public string BaseUrl { get; set; } = "http://10.0.2.2:5005";
    public int TimeoutSeconds { get; set; } = 10;
    public bool EnablePlaybackLogs { get; set; } = true;
}
