namespace MauiApp1.Services;

public sealed class ApiBaseUrlService
{
    private const string ApiBaseUrlKey = "poi_api_base_url";
    private readonly PoiApiOptions _options;

    public ApiBaseUrlService(PoiApiOptions options)
    {
        _options = options;
    }

    public string GetBaseUrl()
    {
        var stored = Preferences.Default.Get(ApiBaseUrlKey, string.Empty);
        return NormalizeBaseUrl(string.IsNullOrWhiteSpace(stored) ? _options.BaseUrl : stored);
    }

    public void SetBaseUrl(string baseUrl)
    {
        Preferences.Default.Set(ApiBaseUrlKey, NormalizeBaseUrl(baseUrl));
    }

    public bool TryUpdateFromPayload(string payload)
    {
        if (!Uri.TryCreate(payload, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            return false;
        }

        var apiBaseUrl = ParseQuery(uri.Query).TryGetValue("apiBaseUrl", out var explicitBaseUrl)
            && !string.IsNullOrWhiteSpace(explicitBaseUrl)
                ? explicitBaseUrl
                : $"{uri.Scheme}://{uri.Authority}/";

        SetBaseUrl(apiBaseUrl);
        return true;
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var normalized = (baseUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "http://10.0.2.2:5005/";
        }

        return normalized.EndsWith("/") ? normalized : $"{normalized}/";
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var segment in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = segment.Split('=', 2);
            var key = Uri.UnescapeDataString(pair[0]);
            var value = pair.Length == 2 ? Uri.UnescapeDataString(pair[1]) : string.Empty;
            result[key] = value;
        }

        return result;
    }
}
