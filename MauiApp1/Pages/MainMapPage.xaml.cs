using MauiApp1.Models;
using MauiApp1.Services;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Features;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using MapsuiStyleBrush = Mapsui.Styles.Brush;
using MapsuiStyleColor = Mapsui.Styles.Color;
using MapsuiStylePen = Mapsui.Styles.Pen;
using MapsuiSymbolStyle = Mapsui.Styles.SymbolStyle;
using MapsuiSymbolType = Mapsui.Styles.SymbolType;

namespace MauiApp1.Pages
{
    public partial class MainMapPage : ContentPage
    {
        private const string LanguagePreferenceKey = "app_language";
        private const string LanguageSelectedKey = "app_language_selected";
        private readonly LocationService _locationService;
        private readonly PostgresPoiService _postgresPoiService;
        private readonly List<PointOfInterest> _nearbyPois = new();
        private readonly List<(string Key, string Label)> _categoryFilters =
        [
            ("all", "Tat ca"),
            ("food", "Quan an"),
            ("cafe", "Cafe"),
            ("park", "Cong vien"),
            ("play", "Khu vui choi"),
            ("theatre", "Nha hat"),
            ("attraction", "Tham quan")
        ];
        private MemoryLayer? _userLocationLayer;
        private Location? _currentLocation;
        private bool _isInitialized;
        private bool _isLoadingNearby;
        private bool _isMapFullScreen;
        private DateTime _lastNearbyLoadUtc = DateTime.MinValue;
        private string _selectedCategoryKey = "all";
        private string _searchKeyword = string.Empty;

        private const double DefaultLatitude = 10.8231;
        private const double DefaultLongitude = 106.6297;

        public MainMapPage(LocationService locationService, PostgresPoiService postgresPoiService)
        {
            InitializeComponent();
            _locationService = locationService;
            _postgresPoiService = postgresPoiService;
            InitializeMap();
            BuildCategoryChips();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            await TryGetLocationAndCenterMapAsync(requestIfMissing: false);
            await LoadNearbyPoiAsync(force: true);
        }

        private void InitializeMap()
        {
            var map = MapControl.Map;
            map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
            _userLocationLayer = new MemoryLayer
            {
                Name = "UserLocationLayer",
                Style = BuildUserLocationStyle()
            };
            map.Layers.Add(_userLocationLayer);

            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(
                DefaultLongitude, DefaultLatitude);
            map.Navigator.CenterOnAndZoomTo(
                sphericalMercatorCoordinate.ToMPoint(), map.Navigator.Resolutions[15]);
        }

        private void CenterMapOnLocation(double latitude, double longitude)
        {
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(longitude, latitude);
            MapControl.Map.Navigator.CenterOnAndZoomTo(
                sphericalMercatorCoordinate.ToMPoint(),
                MapControl.Map.Navigator.Resolutions[16]);
        }

        private static MapsuiSymbolStyle BuildUserLocationStyle()
        {
            return new MapsuiSymbolStyle
            {
                SymbolType = MapsuiSymbolType.Ellipse,
                Fill = new MapsuiStyleBrush(MapsuiStyleColor.Blue),
                Outline = new MapsuiStylePen
                {
                    Color = MapsuiStyleColor.White,
                    Width = 3
                },
                SymbolScale = 0.8
            };
        }

        private void UpdateUserLocationMarker(double latitude, double longitude)
        {
            if (_userLocationLayer == null)
            {
                return;
            }

            var projected = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
            var feature = new PointFeature(projected);
            _userLocationLayer.Features = new[] { feature };
            _userLocationLayer.DataHasChanged();
        }

        private async Task TryGetLocationAndCenterMapAsync(bool requestIfMissing)
        {
            try
            {
                var permission = await _locationService.CheckLocationPermissionAsync();
                if (permission != PermissionStatus.Granted && requestIfMissing)
                {
                    permission = await _locationService.RequestLocationPermissionAsync();
                }

                if (permission != PermissionStatus.Granted)
                {
                    return;
                }

                _currentLocation = await _locationService.GetCurrentLocationAsync();
                if (_currentLocation != null)
                {
                    CenterMapOnLocation(_currentLocation.Latitude, _currentLocation.Longitude);
                    UpdateUserLocationMarker(_currentLocation.Latitude, _currentLocation.Longitude);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
            }
        }

        private async Task LoadNearbyPoiAsync(bool force = false)
        {
            if (_isLoadingNearby)
            {
                return;
            }

            if (!force && _nearbyPois.Count > 0 && DateTime.UtcNow - _lastNearbyLoadUtc < TimeSpan.FromSeconds(8))
            {
                return;
            }

            _isLoadingNearby = true;
            NearbyLoadingIndicator.IsVisible = true;
            NearbyLoadingIndicator.IsRunning = true;
            NearbyStatusLabel.IsVisible = false;

            try
            {
                var centerLat = _currentLocation?.Latitude ?? DefaultLatitude;
                var centerLon = _currentLocation?.Longitude ?? DefaultLongitude;
                var nearby = await _postgresPoiService.GetNearbyRegisteredPoisAsync(centerLat, centerLon, maxItems: 8);

                _nearbyPois.Clear();
                _nearbyPois.AddRange(nearby);
                ApplyFilters();
                _lastNearbyLoadUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostgreSQL load error: {ex.Message}");
                NearbyStatusLabel.IsVisible = true;
                NearbyStatusLabel.Text = "Khong tai duoc du lieu dia diem. Vui long thu lai.";
                DiscoveryList.Children.Clear();
            }
            finally
            {
                _isLoadingNearby = false;
                NearbyLoadingIndicator.IsRunning = false;
                NearbyLoadingIndicator.IsVisible = false;
            }
        }

        private void BindNearbyCards(IEnumerable<PointOfInterest> pois)
        {
            DiscoveryList.Children.Clear();
            foreach (var poi in pois)
            {
                DiscoveryList.Children.Add(MauiApp1.Utils.UIHelper.CreateNearbyCard(poi));
            }
        }

        private async Task FilterNearbyPoiAsync(string keyword)
        {
            _searchKeyword = keyword.Trim();
            ApplyFilters();
            await Task.CompletedTask;
        }

        private void BuildCategoryChips()
        {
            CategoryChipContainer.Children.Clear();

            foreach (var filter in _categoryFilters)
            {
                var chip = MauiApp1.Utils.UIHelper.CreateCategoryChip(filter.Key, filter.Label, OnCategoryChipTappedAsync);
                CategoryChipContainer.Children.Add(chip);
            }

            RefreshCategoryChipStyles();
        }

        private async Task OnCategoryChipTappedAsync(string categoryKey)
        {
            _selectedCategoryKey = categoryKey;
            RefreshCategoryChipStyles();
            ApplyFilters();
            await Task.CompletedTask;
        }

        private void RefreshCategoryChipStyles()
        {
            MauiApp1.Utils.UIHelper.RefreshCategoryChipStyles(CategoryChipContainer, _selectedCategoryKey);
        }

        private void ApplyFilters()
        {
            if (_nearbyPois.Count == 0)
            {
                DiscoveryList.Children.Clear();
                NearbyStatusLabel.IsVisible = true;
                NearbyStatusLabel.Text = "Khong tim thay dia diem phu hop trong ban kinh 1.5km.";
                return;
            }

            IEnumerable<PointOfInterest> query = _nearbyPois;

            if (_selectedCategoryKey != "all")
            {
                query = query.Where(p => p.CategoryKey == _selectedCategoryKey);
            }

            if (!string.IsNullOrWhiteSpace(_searchKeyword))
            {
                query = query.Where(p =>
                    p.Name.Contains(_searchKeyword, StringComparison.OrdinalIgnoreCase)
                    || p.Category.Contains(_searchKeyword, StringComparison.OrdinalIgnoreCase)
                    || p.Description.Contains(_searchKeyword, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query.ToList();

            if (filtered.Count == 0 && _selectedCategoryKey != "all")
            {
                _selectedCategoryKey = "all";
                RefreshCategoryChipStyles();
                ApplyFilters();
                NearbyStatusLabel.IsVisible = true;
                NearbyStatusLabel.Text = "Khong co ket qua theo tag da chon. Dang hien Tat ca.";
                return;
            }

            BindNearbyCards(filtered);
            NearbyStatusLabel.IsVisible = filtered.Count == 0;
            NearbyStatusLabel.Text = filtered.Count == 0
                ? "Khong tim thay ket qua phu hop."
                : string.Empty;
        }

        private async void OnSearchToggleTapped(object? sender, EventArgs e)
        {
            SearchOverlay.IsVisible = !SearchOverlay.IsVisible;
            if (SearchOverlay.IsVisible)
            {
                await SearchOverlay.FadeToAsync(1, 120);
                SearchEntry.Focus();
            }
            else
            {
                SearchEntry.Text = string.Empty;
                NearbyStatusLabel.IsVisible = false;
                BindNearbyCards(_nearbyPois);
            }
        }

        private void OnSearchCloseTapped(object? sender, EventArgs e)
        {
            SearchOverlay.IsVisible = false;
            SearchEntry.Text = string.Empty;
            NearbyStatusLabel.IsVisible = false;
            BindNearbyCards(_nearbyPois);
        }

        private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            await FilterNearbyPoiAsync(e.NewTextValue ?? string.Empty);
        }

        private async void OnSearchCompleted(object? sender, EventArgs e)
        {
            await FilterNearbyPoiAsync(SearchEntry.Text ?? string.Empty);
        }

        private async void OnSettingsTapped(object? sender, EventArgs e)
        {
            var action = await DisplayActionSheetAsync("Cai dat", "Dong", null, "Ngon ngu", "Vi tri");

            if (action == "Ngon ngu")
            {
                await ChangeLanguageAsync();
                return;
            }

            if (action == "Vi tri")
            {
                await HandleLocationSettingsAsync();
            }
        }

        private async Task ChangeLanguageAsync()
        {
            var choice = await DisplayActionSheetAsync("Chọn ngôn ngữ", "Hủy", null, "Tiếng Việt", "Tiếng Anh (Mỹ)");
            if (choice == "Hủy" || string.IsNullOrWhiteSpace(choice))
            {
                return;
            }

            var selectedCode = choice == "Tiếng Việt" ? "vi-VN" : "en-US";
            Preferences.Default.Set(LanguagePreferenceKey, selectedCode);
            Preferences.Default.Set(LanguageSelectedKey, true);

            await DisplayAlertAsync("Ngôn ngữ", "Đã cập nhật ngôn ngữ. Mở lại màn hình khởi động để áp dụng toàn bộ giao diện.", "OK");
        }

        private async Task HandleLocationSettingsAsync()
        {
            var status = await _locationService.CheckLocationPermissionAsync();
            if (status != PermissionStatus.Granted)
            {
                var grant = await DisplayAlertAsync(
                    "Quyen vi tri",
                    "Ung dung can quyen vi tri de tim POI gan ban va phat audio theo ngu canh.",
                    "Cap quyen",
                    "Huy");

                if (!grant)
                {
                    return;
                }

                status = await _locationService.RequestLocationPermissionAsync();
            }

            if (status == PermissionStatus.Granted)
            {
                await TryGetLocationAndCenterMapAsync(requestIfMissing: false);
                await LoadNearbyPoiAsync(force: true);
                return;
            }

            var openSettings = await DisplayAlertAsync(
                "Quyen vi tri",
                "Ban da tu choi quyen vi tri. Hay mo cai dat he thong de cap quyen.",
                "Mo cai dat",
                "De sau");

            if (openSettings)
            {
                AppInfo.ShowSettingsUI();
            }
        }

        private async void OnRecenterTapped(object? sender, EventArgs e)
        {
            if (FabRecenter != null)
            {
                await FabRecenter.ScaleToAsync(0.9, 100);
                await FabRecenter.ScaleToAsync(1.0, 100, Easing.SpringOut);
            }

            await TryGetLocationAndCenterMapAsync(requestIfMissing: true);
            await LoadNearbyPoiAsync(force: true);
        }

        private void OnExpandMapTapped(object? sender, EventArgs e)
        {
            EnterMapFullScreen();
        }

        private void OnExitFullscreenTapped(object? sender, EventArgs e)
        {
            ExitMapFullScreen();
        }

        private void EnterMapFullScreen()
        {
            if (_isMapFullScreen)
            {
                return;
            }

            CompactMapHost.Content = null;
            FullScreenMapHost.Content = MapControl;

            HeaderBar.IsVisible = false;
            MainContentGrid.IsVisible = false;
            BottomNavBar.IsVisible = false;
            FullScreenOverlay.IsVisible = true;

            _isMapFullScreen = true;
        }

        private void ExitMapFullScreen()
        {
            if (!_isMapFullScreen)
            {
                return;
            }

            FullScreenMapHost.Content = null;
            CompactMapHost.Content = MapControl;

            HeaderBar.IsVisible = true;
            MainContentGrid.IsVisible = true;
            BottomNavBar.IsVisible = true;
            FullScreenOverlay.IsVisible = false;

            _isMapFullScreen = false;
        }
    }
}
