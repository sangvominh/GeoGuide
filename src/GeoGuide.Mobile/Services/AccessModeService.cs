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
    public bool IsFullAccess => Mode == AccessMode.Full;
}

public sealed class AccessModeService
{
    private const string ModeKey = "access_mode";
    private const string ActivatedAtKey = "access_mode_activated_at";
    private const string TokenKey = "access_mode_token";

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
            Token = token
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
        if (TryParseSimplePayload(normalized, out var mode, out var token))
        {
            Save(mode, token);
            message = mode == AccessMode.Full
                ? "Đã kích hoạt Full Access thành công."
                : "Đã kích hoạt Trial Mode thành công.";
            return true;
        }

        return false;
    }

    private bool TryParseSimplePayload(string payload, out AccessMode mode, out string token)
    {
        mode = AccessMode.Trial;
        token = string.Empty;

        // Expected: GEOGUIDE:TRIAL:<token> or GEOGUIDE:FULL:<token>
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
        return !string.IsNullOrWhiteSpace(token);
    }

    private static void Save(AccessMode mode, string token)
    {
        Preferences.Default.Set(ModeKey, mode == AccessMode.Full ? "full" : "trial");
        Preferences.Default.Set(ActivatedAtKey, DateTimeOffset.UtcNow.ToString("O"));
        Preferences.Default.Set(TokenKey, token);
    }
}
