using MauiApp1.Services;

namespace MauiApp1.Pages
{
    public partial class SplashPermissionPage : ContentPage
    {
        private readonly LocationService _locationService;
        private readonly StartupWarmupService _startupWarmupService;

        private const string LanguagePreferenceKey = "app_language";
        private const string LanguageSelectedKey = "app_language_selected";
        private const string LocationPromptedKey = "location_prompted";
        private const string OnboardingCompletedKey = "onboarding_completed";

        private bool _isFirstRun;
        private bool _hasAppeared;
        private bool _isNavigating;
        private bool _languageChosen;
        private string _selectedLanguageCode = "vi-VN";
        private CancellationTokenSource? _warmupCts;

        public SplashPermissionPage()
        {
            InitializeComponent();
            _locationService = new LocationService();
            _startupWarmupService = new StartupWarmupService(_locationService);
            LoadLanguagePreference();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_hasAppeared)
            {
                return;
            }

            _hasAppeared = true;
            _isFirstRun = IsFirstRunUser();
            ConfigureFlowUi();
            ApplyLanguageSelection();

            _ = RunSplashAnimationsAsync();

            // Warmup runs in the background and never blocks first interaction.
            _warmupCts = new CancellationTokenSource();
            _ = ExecuteWarmupAsync(_warmupCts.Token);

            _ = InitializeContinueStateAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (_warmupCts != null)
            {
                _warmupCts.Cancel();
                _warmupCts.Dispose();
                _warmupCts = null;
            }
        }

        private async Task RunSplashAnimationsAsync()
        {
            AppTitle.Opacity = 0;
            AppTitle.TranslationY = 20;
            ReturningUserPanel.Opacity = 0;
            ReturningUserPanel.TranslationY = 20;
            LanguageSelectionPanel.Opacity = 0;
            LanguageSelectionPanel.TranslationY = 20;
            ContinueButton.Opacity = 0;
            ProgressLabel.Opacity = 0;

            await Task.WhenAll(
                AppTitle.FadeToAsync(1, 300, Easing.CubicOut),
                AppTitle.TranslateToAsync(0, 0, 300, Easing.CubicOut)
            );

            if (_isFirstRun)
            {
                await Task.WhenAll(
                    LanguageSelectionPanel.FadeToAsync(1, 250, Easing.CubicOut),
                    LanguageSelectionPanel.TranslateToAsync(0, 0, 250, Easing.CubicOut)
                );
            }
            else
            {
                await Task.WhenAll(
                    ReturningUserPanel.FadeToAsync(1, 250, Easing.CubicOut),
                    ReturningUserPanel.TranslateToAsync(0, 0, 250, Easing.CubicOut)
                );
            }

            await Task.WhenAll(
                ContinueButton.FadeToAsync(1, 200, Easing.CubicOut),
                ProgressLabel.FadeToAsync(1, 200, Easing.CubicOut)
            );
        }

        private async Task InitializeContinueStateAsync()
        {
            try
            {
                var status = await _locationService.CheckLocationPermissionAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_isFirstRun)
                    {
                        ContinueButton.Text = status == PermissionStatus.Granted
                            ? GetText("Continue")
                            : GetText("ContinueAndAllowLocation");
                        ContinueButton.IsEnabled = _languageChosen;
                        return;
                    }

                    ContinueButton.Text = GetText("OpenMap");
                    ContinueButton.IsEnabled = true;
                });
            }
            catch
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ContinueButton.Text = _isFirstRun ? GetText("Continue") : GetText("OpenMap");
                    ContinueButton.IsEnabled = _isFirstRun ? _languageChosen : true;
                });
            }
        }

        private async void OnContinueClicked(object? sender, EventArgs e)
        {
            if (_isNavigating)
            {
                return;
            }

            await PlayContinueButtonPressAnimationAsync();

            if (!_isFirstRun)
            {
                await NavigateToMainMapAsync();
                return;
            }

            if (!_languageChosen)
            {
                await DisplayAlertAsync(GetText("LanguageTitle"), GetText("LanguageRequiredMessage"), GetText("Ok"));
                ContinueButton.IsEnabled = false;
                return;
            }

            SetNavigationLoading(true);

            try
            {
                Preferences.Default.Set(LanguageSelectedKey, true);

                var currentStatus = await _locationService.CheckLocationPermissionAsync();
                Preferences.Default.Set(LocationPromptedKey, true);

                if (currentStatus != PermissionStatus.Granted)
                {
                    var requestStatus = await _locationService.RequestLocationPermissionAsync();
                    if (requestStatus != PermissionStatus.Granted)
                    {
                        await DisplayAlertAsync(
                            GetText("LocationTitle"),
                            GetText("LocationDeniedMessage"),
                            GetText("Ok"));
                    }
                }

                Preferences.Default.Set(OnboardingCompletedKey, true);

                await NavigateToMainMapAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Splash flow error: {ex}");
                await DisplayAlertAsync(GetText("ErrorTitle"), GetText("GenericErrorMessage"), GetText("Ok"));
                await NavigateToMainMapAsync();
            }
            finally
            {
                if (!_isNavigating)
                {
                    SetNavigationLoading(false);
                }
            }
        }

        private async Task ExecuteWarmupAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_isFirstRun)
                {
                    SkeletonPanel.IsVisible = true;
                    await SkeletonPanel.FadeToAsync(1, 220, Easing.CubicOut);
                }

                SetProgress(GetText("PreparingLocalData"));
                await _startupWarmupService.PrepareLocalPoiAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var permission = await _locationService.CheckLocationPermissionAsync();
                if (permission == PermissionStatus.Granted)
                {
                    SetProgress(GetText("UpdatingNearbyData"));
                    var result = await _startupWarmupService.SyncAndPrefetchNearbyAsync(cancellationToken);
                    SetProgress(result.IsFreshDataAvailable
                        ? GetText("WarmupCompleted")
                        : GetText("WarmupOfflineFallback"));
                }
                else
                {
                    SetProgress(GetText("WarmupAwaitPermission"));
                }
            }
            catch (OperationCanceledException)
            {
                // Splash was closed before warmup completed.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warmup error: {ex}");
                SetProgress(GetText("WarmupFailed"));
            }
        }

        private void ConfigureFlowUi()
        {
            ReturningUserPanel.IsVisible = !_isFirstRun;
            LanguageSelectionPanel.IsVisible = _isFirstRun;
            SkeletonPanel.IsVisible = false;
            _languageChosen = Preferences.Default.Get(LanguageSelectedKey, false);

            ContinueButton.Text = _isFirstRun ? GetText("Continue") : GetText("OpenMap");
            ContinueButton.IsEnabled = _isFirstRun ? _languageChosen : true;

            if (!_isFirstRun)
            {
                ReturningTitle.Text = GetText("ReturningTitle");
                ReturningMessage.Text = GetText("ReturningMessage");
            }
        }

        private bool IsFirstRunUser()
        {
            var onboardingCompleted = Preferences.Default.Get(OnboardingCompletedKey, false);
            var languageSelected = Preferences.Default.Get(LanguageSelectedKey, false);
            var locationPrompted = Preferences.Default.Get(LocationPromptedKey, false);
            return !(onboardingCompleted && languageSelected && locationPrompted);
        }

        private void SetProgress(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProgressLabel.Text = message;
            });
        }

        private void OnLanguageOptionClicked(object? sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not string languageCode)
            {
                return;
            }

            _selectedLanguageCode = languageCode == "en-US" ? "en-US" : "vi-VN";

            Preferences.Default.Set(LanguagePreferenceKey, _selectedLanguageCode);
            Preferences.Default.Set(LanguageSelectedKey, true);
            _languageChosen = true;
            ContinueButton.IsEnabled = true;
            ApplyLanguageSelection();
        }

        private async Task NavigateToMainMapAsync()
        {
            if (_isNavigating)
            {
                return;
            }

            _isNavigating = true;
            SetNavigationLoading(true);

            if (Shell.Current == null)
            {
                _isNavigating = false;
                SetNavigationLoading(false);
                return;
            }

            try
            {
                await Shell.Current.GoToAsync(AppShell.MainMapNavigationRoute);
            }
            catch
            {
                _isNavigating = false;
                SetNavigationLoading(false);
                throw;
            }
        }

        private void SetNavigationLoading(bool isLoading)
        {
            NavigationLoadingPanel.IsVisible = isLoading;
            NavigationLoadingIndicator.IsRunning = isLoading;
            ContinueButton.IsEnabled = !isLoading && (_isFirstRun ? _languageChosen : true);

            if (isLoading)
            {
                ContinueButton.Text = GetText("OpeningMapLoading");
                return;
            }

            ContinueButton.Text = _isFirstRun ? GetText("Continue") : GetText("OpenMap");
        }

        private async Task PlayContinueButtonPressAnimationAsync()
        {
            if (ContinueButton == null || !ContinueButton.IsEnabled)
            {
                return;
            }

            await ContinueButton.ScaleToAsync(0.98, 70, Easing.CubicOut);
            await ContinueButton.ScaleToAsync(1, 90, Easing.CubicIn);
        }

        private void LoadLanguagePreference()
        {
            _selectedLanguageCode = Preferences.Default.Get(LanguagePreferenceKey, "vi-VN");
            if (_selectedLanguageCode != "vi-VN" && _selectedLanguageCode != "en-US")
            {
                _selectedLanguageCode = "vi-VN";
            }
        }

        private void ApplyLanguageSelection()
        {
            var isVietnamese = _selectedLanguageCode == "vi-VN";

            ContinueButton.Text = "Tiếp tục";
            LanguageSelectorTitle.Text = "Chọn ngôn ngữ";
            EnglishOptionButton.BackgroundColor = isVietnamese
                ? Color.FromArgb("#E9E7ED")
                : Color.FromArgb("#0058BC");
            EnglishOptionButton.TextColor = isVietnamese ? Color.FromArgb("#414755") : Colors.White;
            VietnameseOptionButton.BackgroundColor = isVietnamese
                ? Color.FromArgb("#0058BC")
                : Color.FromArgb("#E9E7ED");
            VietnameseOptionButton.TextColor = isVietnamese ? Colors.White : Color.FromArgb("#414755");
            FooterLabel.Text = "ĐANG KHỞI ĐỘNG HỆ THỐNG KỂ CHUYỆN";
        }

        private string GetText(string key)
        {
            return key switch
            {
                "LanguagePickerTitle" => "Chọn ngôn ngữ",
                "LanguageTitle" => "Ngôn ngữ",
                "LanguageRequiredMessage" => "Vui lòng chọn ngôn ngữ trước khi tiếp tục.",
                "Cancel" => "Hủy",
                "LocationTitle" => "Vị trí",
                "LocationDeniedMessage" => "Quyền truy cập vị trí bị từ chối. Bạn có thể bật lại trong Cài đặt.",
                "PreparingLocalData" => "Đang chuẩn bị dữ liệu địa danh cục bộ...",
                "UpdatingNearbyData" => "Đang đồng bộ và tải sẵn âm thanh gần bạn...",
                "WarmupCompleted" => "Sẵn sàng trải nghiệm trên bản đồ",
                "WarmupOfflineFallback" => "Mạng yếu. Đang ưu tiên dữ liệu ngoại tuyến",
                "WarmupAwaitPermission" => "Cần cấp quyền vị trí để kích hoạt âm thanh gần bạn",
                "WarmupFailed" => "Khởi động cơ bản hoàn tất",
                "Continue" => "Tiếp tục",
                "ContinueAndAllowLocation" => "Tiếp tục và cấp quyền vị trí",
                "OpeningMapLoading" => "Đang mở...",
                "OpenMap" => "Mở bản đồ",
                "ReturningTitle" => "Có gì mới",
                "ReturningMessage" => "Nội dung cập nhật cho phiên bản mới sẽ hiển thị tại đây.",
                "ErrorTitle" => "Lỗi",
                "GenericErrorMessage" => "Đã xảy ra lỗi khi khởi động. Ứng dụng sẽ tiếp tục với chế độ cơ bản.",
                "Ok" => "OK",
                _ => string.Empty
            };
        }
    }
}
