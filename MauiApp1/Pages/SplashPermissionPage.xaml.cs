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
        private string _selectedLanguageCode = "en-US";
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

            if (!_isFirstRun)
            {
                _ = AutoNavigateReturningUserAsync();
            }
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
            LogoIcon.Opacity = 0;
            LogoIcon.Scale = 0.5;
            AppTitle.Opacity = 0;
            AppTitle.TranslationY = 20;
            AppSubtitle.Opacity = 0;
            AppSubtitle.TranslationY = 20;
            ReturningUserPanel.Opacity = 0;
            ReturningUserPanel.TranslationY = 20;
            LanguageSelectionPanel.Opacity = 0;
            LanguageSelectionPanel.TranslationY = 20;
            ContinueButton.Opacity = 0;
            ProgressLabel.Opacity = 0;

            await Task.WhenAll(
                LogoIcon.FadeToAsync(1, 400, Easing.CubicOut),
                LogoIcon.ScaleToAsync(1, 500, Easing.SpringOut)
            );

            await Task.WhenAll(
                AppTitle.FadeToAsync(1, 300, Easing.CubicOut),
                AppTitle.TranslateToAsync(0, 0, 300, Easing.CubicOut)
            );

            await Task.WhenAll(
                AppSubtitle.FadeToAsync(1, 300, Easing.CubicOut),
                AppSubtitle.TranslateToAsync(0, 0, 300, Easing.CubicOut)
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

        private async Task AutoNavigateReturningUserAsync()
        {
            // Keep returning-user splash visible briefly while warmup starts.
            await Task.Delay(900);

            if (_isFirstRun || _isNavigating)
            {
                return;
            }

            await NavigateToMainMapAsync();
        }

        private async void OnContinueClicked(object? sender, EventArgs e)
        {
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

            ContinueButton.IsEnabled = false;

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
                ContinueButton.IsEnabled = true;
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
            AppSubtitle.Text = _isFirstRun
                ? GetText("SubtitleFirstRun")
                : GetText("SubtitleReturning");

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

            _selectedLanguageCode = languageCode == "vi-VN" ? "vi-VN" : "en-US";

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

            if (Shell.Current == null)
            {
                return;
            }

            await Shell.Current.GoToAsync(AppShell.MainMapNavigationRoute);
        }

        private void LoadLanguagePreference()
        {
            _selectedLanguageCode = Preferences.Default.Get(LanguagePreferenceKey, "en-US");
            if (_selectedLanguageCode != "vi-VN")
            {
                _selectedLanguageCode = "en-US";
            }
        }

        private void ApplyLanguageSelection()
        {
            var isVietnamese = _selectedLanguageCode == "vi-VN";

            ContinueButton.Text = isVietnamese ? "Tiếp tục" : "Continue";
            LanguageSelectorTitle.Text = isVietnamese ? "Chon ngon ngu" : "Choose language";
            EnglishOptionButton.BackgroundColor = isVietnamese
                ? Color.FromArgb("#E9E7ED")
                : Color.FromArgb("#0058BC");
            EnglishOptionButton.TextColor = isVietnamese ? Color.FromArgb("#414755") : Colors.White;
            VietnameseOptionButton.BackgroundColor = isVietnamese
                ? Color.FromArgb("#0058BC")
                : Color.FromArgb("#E9E7ED");
            VietnameseOptionButton.TextColor = isVietnamese ? Colors.White : Color.FromArgb("#414755");
            FooterLabel.Text = isVietnamese
                ? "DANG KHOI DONG HE THONG KE CHUYEN"
                : "INITIALIZING NARRATIVE ENGINE";
        }

        private string GetText(string key)
        {
            var isVietnamese = _selectedLanguageCode == "vi-VN";

            return key switch
            {
                "LanguagePickerTitle" => isVietnamese ? "Chon ngon ngu" : "Choose language",
                "LanguageTitle" => isVietnamese ? "Ngon ngu" : "Language",
                "LanguageRequiredMessage" => isVietnamese
                    ? "Vui long chon ngon ngu truoc khi tiep tuc."
                    : "Please choose a language before continuing.",
                "Cancel" => isVietnamese ? "Huy" : "Cancel",
                "LocationTitle" => isVietnamese ? "Vi tri" : "Location",
                "LocationDeniedMessage" => isVietnamese
                    ? "Quyen truy cap vi tri bi tu choi. Ban co the bat lai trong Cai dat."
                    : "Location access was denied. You can enable it again in Settings.",
                "PreparingLocalData" => isVietnamese
                    ? "Dang chuan bi du lieu dia danh cuc bo..."
                    : "Preparing local POI data...",
                "UpdatingNearbyData" => isVietnamese
                    ? "Dang dong bo va tai san am thanh gan ban..."
                    : "Syncing updates and preloading nearby audio...",
                "WarmupCompleted" => isVietnamese
                    ? "San sang trai nghiem tren ban do"
                    : "Ready to explore the map",
                "WarmupOfflineFallback" => isVietnamese
                    ? "Mang yeu. Dang uu tien du lieu ngoai tuyen"
                    : "Weak connection. Running in offline-first mode",
                "WarmupAwaitPermission" => isVietnamese
                    ? "Can cap quyen vi tri de kich hoat tai san am thanh gan ban"
                    : "Grant location to preload nearby audio",
                "WarmupFailed" => isVietnamese
                    ? "Khoi dong co ban hoan tat"
                    : "Basic startup completed",
                "ContinueAndAllowLocation" => isVietnamese ? "Tiep tuc va cap quyen vi tri" : "Continue and allow location",
                "OpenMap" => isVietnamese ? "Mo ban do" : "Open map",
                "SubtitleFirstRun" => isVietnamese
                    ? "Chon ngon ngu va cap quyen de bat dau"
                    : "Pick language and location to get started",
                "SubtitleReturning" => isVietnamese
                    ? "Cap nhat moi se duoc tai nen trong giay lat"
                    : "Latest updates are loading in the background",
                "ReturningTitle" => isVietnamese ? "Co gi moi" : "What's new",
                "ReturningMessage" => isVietnamese
                    ? "Da cap nhat dia diem va am thanh moi gan ban"
                    : "Nearby places and audio stories have been refreshed",
                "ErrorTitle" => isVietnamese ? "Loi" : "Error",
                "GenericErrorMessage" => isVietnamese
                    ? "Da xay ra loi khi khoi dong. Ung dung se tiep tuc voi che do co ban."
                    : "An error occurred during startup. The app will continue in basic mode.",
                "Ok" => "OK",
                _ => string.Empty
            };
        }
    }
}
