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
        private readonly OpenStreetMapService _openStreetMapService;
        private readonly PoiSyncCacheService _poiSyncCacheService;
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
        private bool _isAutoAudioEnabled;
        private DateTime _lastNearbyLoadUtc = DateTime.MinValue;
        private string _selectedCategoryKey = "all";
        private string _searchKeyword = string.Empty;

        private const double DefaultLatitude = 10.8231;
        private const double DefaultLongitude = 106.6297;

        public MainMapPage()
        {
            InitializeComponent();
            _locationService = new LocationService();
            _openStreetMapService = new OpenStreetMapService();
            _poiSyncCacheService = new PoiSyncCacheService();
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
            await LoadCachedPoiAsync();
            await LoadNearbyPoiAsync(force: true, applyImmediately: _nearbyPois.Count == 0);
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

        private async Task<PermissionStatus> TryGetLocationAndCenterMapAsync(bool requestIfMissing)
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
                    UpdateAutoAudioReliability(permission, null);
                    return permission;
                }

                _currentLocation = await _locationService.GetCurrentLocationAsync()
                    ?? await _locationService.GetLastKnownLocationAsync();

                if (_currentLocation != null)
                {
                    CenterMapOnLocation(_currentLocation.Latitude, _currentLocation.Longitude);
                    UpdateUserLocationMarker(_currentLocation.Latitude, _currentLocation.Longitude);
                }

                UpdateAutoAudioReliability(permission, _currentLocation);
                return permission;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
                ShowSystemStatus("Khong the xac dinh vi tri hien tai. Vui long thu lai hoac quet QR de nghe.");
                _isAutoAudioEnabled = false;
                return PermissionStatus.Unknown;
            }
        }

        private async Task LoadCachedPoiAsync()
        {
            var cached = await _poiSyncCacheService.GetCachedPoiAsync();
            if (cached.Count > 0)
            {
                _nearbyPois.Clear();
                _nearbyPois.AddRange(cached);
                ApplyFilters();
            }

            if (_poiSyncCacheService.HasPendingRefresh())
            {
                ShowPoiUpdateBanner();
            }
        }

        private async Task LoadNearbyPoiAsync(bool force = false, bool applyImmediately = false)
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
                var nearby = await _openStreetMapService.GetNearbyPlacesAsync(centerLat, centerLon, maxItems: 8);

                if (nearby.Count == 0)
                {
                    if (_nearbyPois.Count == 0)
                    {
                        NearbyStatusLabel.IsVisible = true;
                        NearbyStatusLabel.Text = "Khong co du lieu truc tuyen. Ung dung dang dung che do ngoai tuyen.";
                    }
                }
                else if (applyImmediately)
                {
                    _nearbyPois.Clear();
                    _nearbyPois.AddRange(nearby);
                    ApplyFilters();
                    await _poiSyncCacheService.UpsertCachedPoiAsync(nearby);
                    _poiSyncCacheService.ClearPendingRefresh();
                    HidePoiUpdateBanner();
                }
                else
                {
                    var hasPending = await _poiSyncCacheService.StageIncomingRefreshAsync(nearby);
                    if (hasPending)
                    {
                        ShowPoiUpdateBanner();
                    }
                }

                _lastNearbyLoadUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OSM load error: {ex.Message}");

                if (_nearbyPois.Count == 0)
                {
                    NearbyStatusLabel.IsVisible = true;
                    NearbyStatusLabel.Text = "Khong tai duoc du lieu dia diem. Dang uu tien du lieu ngoai tuyen.";
                    DiscoveryList.Children.Clear();
                }
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
                DiscoveryList.Children.Add(CreateDiscoveryCard(poi));
            }
        }

        private View CreateDiscoveryCard(PointOfInterest poi)
        {
            var card = new Border
            {
                BackgroundColor = Color.FromArgb("#FFFFFF"),
                Padding = new Thickness(16),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(24) }
            };

            var cardGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(64)),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 12
            };

            var icon = new Border
            {
                WidthRequest = 64,
                HeightRequest = 64,
                BackgroundColor = Color.FromArgb("#EAF1FF"),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(14) },
                Content = new Label
                {
                    Text = poi.IconGlyph,
                    FontFamily = "MaterialIcons",
                    FontSize = 28,
                    TextColor = Color.FromArgb("#0058BC"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
            cardGrid.SetColumn(icon, 0);
            cardGrid.Children.Add(icon);

            var infoStack = new VerticalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };

            infoStack.Children.Add(new Label
            {
                Text = poi.Name,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1A1B1F"),
                LineBreakMode = LineBreakMode.TailTruncation
            });

            infoStack.Children.Add(new Label
            {
                Text = poi.Category,
                FontSize = 12,
                TextColor = Color.FromArgb("#414755")
            });

            var detail = new HorizontalStackLayout { Spacing = 8 };
            detail.Children.Add(new Label
            {
                Text = $"cach {poi.Distance}",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#414755")
            });

            if (!string.IsNullOrWhiteSpace(poi.Description))
            {
                detail.Children.Add(new Label
                {
                    Text = poi.Description,
                    FontSize = 11,
                    TextColor = Color.FromArgb("#6A6F7D"),
                    LineBreakMode = LineBreakMode.TailTruncation,
                    MaxLines = 1,
                    VerticalOptions = LayoutOptions.Center
                });
            }

            infoStack.Children.Add(detail);

            cardGrid.SetColumn(infoStack, 1);
            cardGrid.Children.Add(infoStack);

            card.Content = cardGrid;
            return card;
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
                var chip = CreateCategoryChip(filter.Key, filter.Label);
                CategoryChipContainer.Children.Add(chip);
            }

            RefreshCategoryChipStyles();
        }

        private Border CreateCategoryChip(string categoryKey, string label)
        {
            var chipLabel = new Label
            {
                Text = label,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            };

            var chip = new Border
            {
                StrokeThickness = 0,
                Padding = new Thickness(14, 8),
                BindingContext = categoryKey,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
                Content = chipLabel
            };

            chip.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await OnCategoryChipTappedAsync(categoryKey))
            });

            return chip;
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
            foreach (var child in CategoryChipContainer.Children)
            {
                if (child is not Border chip || chip.Content is not Label label || chip.BindingContext is not string key)
                {
                    continue;
                }

                var isActive = key == _selectedCategoryKey;
                chip.BackgroundColor = isActive ? Color.FromArgb("#0058BC") : Color.FromArgb("#E9E7ED");
                label.TextColor = isActive ? Colors.White : Color.FromArgb("#414755");
            }
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
            var choice = await DisplayActionSheetAsync("Chon ngon ngu", "Huy", null, "Tieng Viet", "English (US)");
            if (choice == "Huy" || string.IsNullOrWhiteSpace(choice))
            {
                return;
            }

            var selectedCode = choice == "Tieng Viet" ? "vi-VN" : "en-US";
            Preferences.Default.Set(LanguagePreferenceKey, selectedCode);
            Preferences.Default.Set(LanguageSelectedKey, true);

            await DisplayAlertAsync("Ngon ngu", "Da cap nhat ngon ngu. Mo lai Splash de thay doi toan bo giao dien.", "OK");
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
                await LoadNearbyPoiAsync(force: true, applyImmediately: true);
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

            var status = await TryGetLocationAndCenterMapAsync(requestIfMissing: true);
            if (status != PermissionStatus.Granted)
            {
                return;
            }

            await LoadNearbyPoiAsync(force: true, applyImmediately: true);
        }

        private async void OnApplyRefreshTapped(object? sender, EventArgs e)
        {
            var refreshed = await _poiSyncCacheService.ApplyPendingRefreshAsync();
            _nearbyPois.Clear();
            _nearbyPois.AddRange(refreshed);
            ApplyFilters();
            HidePoiUpdateBanner();

            NearbyStatusLabel.IsVisible = true;
            NearbyStatusLabel.Text = "Da lam moi danh sach dia diem moi nhat.";
        }

        private void ShowPoiUpdateBanner()
        {
            PoiUpdateBannerLabel.Text = "Da co ban cap nhat moi. Cham de lam moi";
            PoiUpdateBanner.IsVisible = true;
            RepositionPoiBanner();
        }

        private void HidePoiUpdateBanner()
        {
            PoiUpdateBanner.IsVisible = false;
        }

        private void UpdateAutoAudioReliability(PermissionStatus permission, Location? location)
        {
            if (permission != PermissionStatus.Granted)
            {
                _isAutoAudioEnabled = false;
                ShowSystemStatus("Da tat GPS. Phat audio tu dong se tam dung, ban van co the chon tren ban do hoac quet QR.");
                return;
            }

            if (location == null)
            {
                _isAutoAudioEnabled = false;
                ShowSystemStatus("Khong lay duoc toa do hien tai. Vui long thu lai hoac quet QR de nghe.");
                return;
            }

            if (location.Accuracy is > 50)
            {
                _isAutoAudioEnabled = false;
                ShowSystemStatus("Tin hieu dinh vi yeu (sai so > 50m). Vui long quet QR de nghe chinh xac.");
                return;
            }

            _isAutoAudioEnabled = true;
            HideSystemStatus();
        }

        private void ShowSystemStatus(string message)
        {
            SystemStatusLabel.Text = message;
            SystemStatusBanner.IsVisible = true;
            RepositionPoiBanner();
        }

        private void HideSystemStatus()
        {
            SystemStatusBanner.IsVisible = false;
            RepositionPoiBanner();
        }

        private void RepositionPoiBanner()
        {
            PoiUpdateBanner.Margin = SystemStatusBanner.IsVisible
                ? new Thickness(20, 72, 20, 0)
                : new Thickness(20, 14, 20, 0);
        }
    }
}
