using MauiApp1.Services;

namespace MauiApp1.Pages
{
    public partial class SplashPermissionPage : ContentPage
    {
        private readonly LocationService _locationService;
        private string _selectedLanguageCode = "en-US";

        public SplashPermissionPage()
        {
            InitializeComponent();
            _locationService = new LocationService();
            LoadLanguagePreference();
            ApplyLanguageSelection();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Run splash entrance animations
            await RunSplashAnimationsAsync();

            // Check if permission is already granted
            var status = await _locationService.CheckLocationPermissionAsync();
            ContinueButton.Text = status == PermissionStatus.Granted ? "Continue" : "Continue and Allow Location";
        }

        private async Task RunSplashAnimationsAsync()
        {
            // Start off invisible
            LogoIcon.Opacity = 0;
            LogoIcon.Scale = 0.5;
            AppTitle.Opacity = 0;
            AppTitle.TranslationY = 20;
            AppSubtitle.Opacity = 0;
            AppSubtitle.TranslationY = 20;
            LanguageSelector.Opacity = 0;
            LanguageSelector.TranslationY = 20;

            // Animate logo
            await Task.WhenAll(
                LogoIcon.FadeToAsync(1, 400, Easing.CubicOut),
                LogoIcon.ScaleToAsync(1, 500, Easing.SpringOut)
            );

            // Animate title
            await Task.WhenAll(
                AppTitle.FadeToAsync(1, 300, Easing.CubicOut),
                AppTitle.TranslateToAsync(0, 0, 300, Easing.CubicOut)
            );

            // Animate subtitle
            await Task.WhenAll(
                AppSubtitle.FadeToAsync(1, 300, Easing.CubicOut),
                AppSubtitle.TranslateToAsync(0, 0, 300, Easing.CubicOut)
            );

            // Animate language selector
            await Task.WhenAll(
                LanguageSelector.FadeToAsync(1, 300, Easing.CubicOut),
                LanguageSelector.TranslateToAsync(0, 0, 300, Easing.CubicOut)
            );

            // Small delay before showing permission dialog
            await Task.Delay(500);
        }

        private async void OnContinueClicked(object? sender, EventArgs e)
        {
            ContinueButton.IsEnabled = false;

            try
            {
                var currentStatus = await _locationService.CheckLocationPermissionAsync();
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

        private async void OnLanguageSelectorTapped(object? sender, EventArgs e)
        {
            var selected = await DisplayActionSheet(
                GetText("LanguagePickerTitle"),
                GetText("Cancel"),
                null,
                "English (US)",
                "Tiếng Việt");

            if (selected == "English (US)")
            {
                _selectedLanguageCode = "en-US";
            }
            else if (selected == "Tiếng Việt")
            {
                _selectedLanguageCode = "vi-VN";
            }
            else
            {
                return;
            }

            Preferences.Default.Set("app_language", _selectedLanguageCode);
            ApplyLanguageSelection();
        }

        private async Task NavigateToMainMapAsync()
        {
            if (Shell.Current == null)
            {
                return;
            }

            await Shell.Current.GoToAsync(AppShell.MainMapNavigationRoute);
        }

        private void LoadLanguagePreference()
        {
            _selectedLanguageCode = Preferences.Default.Get("app_language", "en-US");
            if (_selectedLanguageCode != "vi-VN")
            {
                _selectedLanguageCode = "en-US";
            }
        }

        private void ApplyLanguageSelection()
        {
            var isVietnamese = _selectedLanguageCode == "vi-VN";

            LanguageLabel.Text = isVietnamese ? "Tiếng Việt" : "English (US)";
            ContinueButton.Text = isVietnamese ? "Tiếp tục" : "Continue";
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
                "Cancel" => isVietnamese ? "Huy" : "Cancel",
                "LocationTitle" => isVietnamese ? "Vi tri" : "Location",
                "LocationDeniedMessage" => isVietnamese
                    ? "Quyen truy cap vi tri bi tu choi. Ban co the bat lai trong Cai dat."
                    : "Location access was denied. You can enable it again in Settings.",
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
