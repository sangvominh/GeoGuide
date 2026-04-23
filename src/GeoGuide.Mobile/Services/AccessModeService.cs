namespace MauiApp1.Services;

public enum AccessMode
{
    Trial,
    Full
}

public sealed class AccessModeState
{
    public AccessMode Mode { get; init; } = AccessMode.Trial;
    public DateTimeOffset ActivatedAt { get; init; }
    public string Token { get; init; } = string.Empty;
    public string SessionToken { get; init; } = string.Empty;
    public bool IsFullAccess => Mode == AccessMode.Full;
}

public sealed class AccessModeService
{
    private const string ModeKey = "access_mode";
    private const string ActivatedAtKey = "access_mode_activated_at";
    private const string TokenKey = "access_mode_token";
    private const string SessionTokenKey = "access_mode_session_token";

    public AccessModeState GetState()
    {
        var modeRaw = Preferences.Default.Get(ModeKey, "trial");
        var activatedAtRaw = Preferences.Default.Get(ActivatedAtKey, string.Empty);
        var token = Preferences.Default.Get(TokenKey, string.Empty);
        DateTimeOffset.TryParse(activatedAtRaw, out var activatedAt);

        return new AccessModeState
        {
            Mode = string.Equals(modeRaw, "full", StringComparison.OrdinalIgnoreCase)
                ? AccessMode.Full
                : AccessMode.Trial,
            ActivatedAt = activatedAt,
            Token = token,
            SessionToken = Preferences.Default.Get(SessionTokenKey, string.Empty)
        };
    }

    public bool TryActivateFromQrPayload(string payload, out string message)
    {
        message = "Mã QR không hợp lệ.";
        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        var normalized = payload.Trim();
        if (TryParsePayload(normalized, out var mode, out var token, out var sessionToken))
        {
            Save(mode, token, sessionToken);
            message = mode == AccessMode.Full
                ? $"Đã kích hoạt Full Access cho session {sessionToken}."
                : $"Đã kích hoạt Trial Mode cho session {sessionToken}.";
            return true;
        }

        return false;
    }

    private static bool TryParsePayload(string payload, out AccessMode mode, out string token, out string sessionToken)
    {
        mode = AccessMode.Trial;
        token = string.Empty;
        sessionToken = string.Empty;

        if (TryParseJoinUrl(payload, out mode, out sessionToken))
        {
            token = sessionToken;
            return true;
        }

        if (TryParseJoinPayload(payload, out mode, out sessionToken))
        {
            token = sessionToken;
            return true;
        }

        var parts = payload.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 3)
        {
            return false;
        }

        if (!string.Equals(parts[0], "GEOGUIDE", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        mode = string.Equals(parts[1], "FULL", StringComparison.OrdinalIgnoreCase)
            ? AccessMode.Full
            : string.Equals(parts[1], "TRIAL", StringComparison.OrdinalIgnoreCase)
                ? AccessMode.Trial
                : AccessMode.Trial;

        if (parts[1] is not ("FULL" or "full" or "TRIAL" or "trial"))
        {
            return false;
        }

        token = string.Join(":", parts.Skip(2));
        sessionToken = token;
        return !string.IsNullOrWhiteSpace(token);
    }

    private static bool TryParseJoinPayload(string payload, out AccessMode mode, out string sessionToken)
    {
        mode = AccessMode.Trial;
        sessionToken = string.Empty;

        var parts = payload.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!string.Equals(parts[0], "GEOGUIDE", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(parts[1], "JOIN", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        mode = string.Equals(parts[3], "FULL", StringComparison.OrdinalIgnoreCase)
            ? AccessMode.Full
            : string.Equals(parts[3], "TRIAL", StringComparison.OrdinalIgnoreCase)
                ? AccessMode.Trial
                : AccessMode.Trial;

        if (parts[3] is not ("FULL" or "full" or "TRIAL" or "trial"))
        {
            return false;
        }

        sessionToken = parts[2];
        return !string.IsNullOrWhiteSpace(sessionToken);
    }

    private static bool TryParseJoinUrl(string payload, out AccessMode mode, out string sessionToken)
    {
        mode = AccessMode.Trial;
        sessionToken = string.Empty;

        if (!Uri.TryCreate(payload, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var queryValues = ParseQuery(uri.Query);
        if (!queryValues.TryGetValue("session", out var parsedSessionToken) || string.IsNullOrWhiteSpace(parsedSessionToken))
        {
            return false;
        }

        sessionToken = parsedSessionToken;

        mode = queryValues.TryGetValue("mode", out var modeRaw)
            && string.Equals(modeRaw, "full", StringComparison.OrdinalIgnoreCase)
                ? AccessMode.Full
                : AccessMode.Trial;

        return true;
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

    private static void Save(AccessMode mode, string token, string sessionToken)
    {
        Preferences.Default.Set(ModeKey, mode == AccessMode.Full ? "full" : "trial");
        Preferences.Default.Set(ActivatedAtKey, DateTimeOffset.UtcNow.ToString("O"));
        Preferences.Default.Set(TokenKey, token);
        Preferences.Default.Set(SessionTokenKey, sessionToken);
    }
}
