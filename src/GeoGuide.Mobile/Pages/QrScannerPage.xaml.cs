using ZXing.Net.Maui;

namespace MauiApp1.Pages;

public partial class QrScannerPage : ContentPage
{
    private readonly Services.AccessModeService _accessModeService;
    private readonly Services.OfflineAnalyticsLogService _offlineAnalyticsLogService;
    private bool _isHandlingScan;

    public QrScannerPage(
        Services.AccessModeService accessModeService,
        Services.OfflineAnalyticsLogService offlineAnalyticsLogService)
    {
        InitializeComponent();
        _accessModeService = accessModeService;
        _offlineAnalyticsLogService = offlineAnalyticsLogService;

        CameraView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };

        UpdateAccessStateChip();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            await DisplayAlertAsync("Quét QR", "Thiết bị này chưa hỗ trợ quét camera. Hãy dùng mục nhập payload thủ công.", "OK");
            return;
        }

        var permission = await Permissions.RequestAsync<Permissions.Camera>();
        if (permission != PermissionStatus.Granted)
        {
            await DisplayAlertAsync("Quyền camera", "Cần quyền camera để quét mã QR.", "OK");
            await CloseAsync();
            return;
        }

        CameraView.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        CameraView.IsDetecting = false;
        base.OnDisappearing();
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var payload = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(payload) || _isHandlingScan)
        {
            return;
        }

        await HandlePayloadAsync(payload);
    }

    private async void OnManualEntryTapped(object? sender, EventArgs e)
    {
        var payload = await DisplayPromptAsync(
            "Nhập payload QR",
            "Ví dụ: GEOGUIDE:TRIAL:DEMO hoặc GEOGUIDE:FULL:DEMO",
            "Kích hoạt",
            "Hủy",
            maxLength: 300,
            initialValue: "GEOGUIDE:TRIAL:DEMO");

        if (string.IsNullOrWhiteSpace(payload))
        {
            return;
        }

        await HandlePayloadAsync(payload);
    }

    private async void OnCloseTapped(object? sender, EventArgs e)
    {
        await CloseAsync();
    }

    private async Task HandlePayloadAsync(string payload)
    {
        if (_isHandlingScan)
        {
            return;
        }

        _isHandlingScan = true;
        CameraView.IsDetecting = false;
        LoadingOverlay.IsVisible = true;
        LoadingLabel.Text = "Đang xử lý mã QR...";

        try
        {
            var success = _accessModeService.TryActivateFromQrPayload(payload, out var message);
            _ = _offlineAnalyticsLogService.LogQrScannedAsync(payload);

            UpdateAccessStateChip();
            await DisplayAlertAsync(success ? "Kích hoạt thành công" : "Kích hoạt thất bại", message, "OK");

            if (success)
            {
                await CloseAsync();
                return;
            }
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
            _isHandlingScan = false;
            CameraView.IsDetecting = true;
        }
    }

    private void UpdateAccessStateChip()
    {
        var state = _accessModeService.GetState();
        AccessStateLabel.Text = state.IsFullAccess ? "FULL" : "TRIAL";
        AccessStateLabel.TextColor = state.IsFullAccess
            ? Color.FromArgb("#C7FFD8")
            : Color.FromArgb("#FFD7BF");
    }

    private Task CloseAsync()
    {
        return Navigation.PopModalAsync();
    }
}
