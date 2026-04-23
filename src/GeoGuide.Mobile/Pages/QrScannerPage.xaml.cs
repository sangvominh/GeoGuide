using ZXing.Net.Maui;

namespace MauiApp1.Pages;

public partial class QrScannerPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _resultSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _isCompleted;

    public QrScannerPage()
    {
        InitializeComponent();

        ScannerView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
        ScannerView.BarcodesDetected += OnBarcodesDetected;
    }

    public Task<string?> WaitForResultAsync() => _resultSource.Task;

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var payload = e.Results.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(payload))
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_isCompleted)
            {
                return;
            }

            _isCompleted = true;
            ScannerView.IsDetecting = false;
            ScannerStatusLabel.Text = "Đã đọc được mã QR, đang đóng màn hình quét.";
            _resultSource.TrySetResult(payload);
            await Navigation.PopModalAsync();
        });
    }

    private async void OnCancelTapped(object? sender, EventArgs e)
    {
        if (_isCompleted)
        {
            return;
        }

        _isCompleted = true;
        ScannerView.IsDetecting = false;
        _resultSource.TrySetResult(null);
        await Navigation.PopModalAsync();
    }
}
