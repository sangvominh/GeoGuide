using Mapsui;
using Mapsui.Extensions;
using Mapsui.Features;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Extensions.DependencyInjection;
using MapsuiStyleBrush = Mapsui.Styles.Brush;
using MapsuiStyleColor = Mapsui.Styles.Color;
using MapsuiStylePen = Mapsui.Styles.Pen;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace MauiApp1.Pages;

public partial class MainMapPage : ContentPage
{
    private const string LanguagePreferenceKey = "app_language";
    private const string LanguageSelectedKey = "app_language_selected";
    private const double DefaultLatitude = 10.8231;
    private const double DefaultLongitude = 106.6297;
    private static readonly TimeSpan PoiRefreshInterval = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan AutoTriggerCooldown = TimeSpan.FromMinutes(5);

    private readonly LocationService _locationService;
    private readonly PoiRepository _poiRepository;
    private readonly NarrationService _narrationService;
    private readonly List<PointOfInterest> _allPois = [];
    private readonly Dictionary<string, DateTimeOffset> _lastPlaybackByPoiId = [];
    private readonly List<(string Key, string Label)> _categoryFilters =
    [
        ("all", "Tất cả"),
        ("food", "Ẩm thực"),
        ("cafe", "Cafe"),
        ("park", "Công viên"),
        ("play", "Vui chơi"),
        ("theatre", "Sân khấu"),
        ("attraction", "Tham quan")
    ];

    private readonly IDispatcherTimer _locationTimer;
    private MemoryLayer? _userLocationLayer;
    private MemoryLayer? _poiLayer;
    private Location? _currentLocation;
    private PointOfInterest? _nearestPoi;
    private bool _isInitialized;
    private bool _isLoadingPois;
    private bool _isMapFullScreen;
    private bool _isNarrationRunning;
    private DateTimeOffset _lastPoiRefreshUtc = DateTimeOffset.MinValue;
    private string _selectedCategoryKey = "all";
    private string _searchKeyword = string.Empty;

    public MainMapPage()
    {
        InitializeComponent();
        Mapsui.Widgets.InfoWidgets.LoggingWidget.ShowLoggingInMap = Mapsui.Widgets.ActiveMode.No;

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider is not available.");

        _locationService = services.GetRequiredService<LocationService>();
        _poiRepository = services.GetRequiredService<PoiRepository>();
        _narrationService = services.GetRequiredService<NarrationService>();

        _locationTimer = Dispatcher.CreateTimer();
        _locationTimer.Interval = TimeSpan.FromSeconds(20);
        _locationTimer.Tick += async (_, _) => await PollLocationAsync();

        InitializeMap();
        BuildCategoryChips();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            _isInitialized = true;
            await InitializeAsync();
        }

        if (!_locationTimer.IsRunning)
        {
            _locationTimer.Start();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_locationTimer.IsRunning)
        {
            _locationTimer.Stop();
        }
    }

    private async Task InitializeAsync()
    {
        await RefreshCurrentLocationAsync(requestIfMissing: false, recenterMap: true);
        await LoadPoisAsync(force: true);
        await EvaluateAutoTriggerAsync();
    }

    private void InitializeMap()
    {
        var map = MapControl.Map;
        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        _userLocationLayer = new MemoryLayer
        {
            Name = "UserLocationLayer",
            Style = new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                Fill = new MapsuiStyleBrush(MapsuiStyleColor.Blue),
                Outline = new MapsuiStylePen
                {
                    Color = MapsuiStyleColor.White,
                    Width = 3
                },
                SymbolScale = 0.8
            }
        };

        _poiLayer = new MemoryLayer
        {
            Name = "PoiLayer"
        };

        map.Layers.Add(_poiLayer);
        map.Layers.Add(_userLocationLayer);

        var defaultCoordinate = SphericalMercator.FromLonLat(DefaultLongitude, DefaultLatitude);
        map.Navigator.CenterOnAndZoomTo(defaultCoordinate.ToMPoint(), map.Navigator.Resolutions[15]);
    }

    private async Task RefreshCurrentLocationAsync(bool requestIfMissing, bool recenterMap)
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
                UpdateNearestPoiStatus();
                return;
            }

            _currentLocation = await _locationService.GetCurrentLocationAsync()
                ?? await _locationService.GetLastKnownLocationAsync();

            if (_currentLocation == null)
            {
                UpdateNearestPoiStatus();
                return;
            }

            UpdateUserLocationMarker(_currentLocation.Latitude, _currentLocation.Longitude);
            if (recenterMap)
            {
                CenterMapOnLocation(_currentLocation.Latitude, _currentLocation.Longitude);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
        }
    }

    private void CenterMapOnLocation(double latitude, double longitude)
    {
        var coordinate = SphericalMercator.FromLonLat(longitude, latitude);
        MapControl.Map.Navigator.CenterOnAndZoomTo(
            coordinate.ToMPoint(),
            MapControl.Map.Navigator.Resolutions[16]);
    }

    private void UpdateUserLocationMarker(double latitude, double longitude)
    {
        if (_userLocationLayer == null)
        {
            return;
        }

        var projected = SphericalMercator.FromLonLat(longitude, latitude).ToMPoint();
        _userLocationLayer.Features = [new PointFeature(projected)];
        _userLocationLayer.DataHasChanged();
    }

    private async Task LoadPoisAsync(bool force = false)
    {
        if (_isLoadingPois)
        {
            return;
        }

        if (!force && _allPois.Count > 0 && DateTimeOffset.UtcNow - _lastPoiRefreshUtc < PoiRefreshInterval)
        {
            RecalculatePoiDistances();
            ApplyFilters();
            return;
        }

        _isLoadingPois = true;
        NearbyLoadingIndicator.IsVisible = true;
        NearbyLoadingIndicator.IsRunning = true;
        NearbyStatusLabel.IsVisible = false;

        try
        {
            var result = await _poiRepository.GetPoisAsync();
            _allPois.Clear();
            _allPois.AddRange(result.Pois.Where(static poi => poi.IsActive));
            _lastPoiRefreshUtc = DateTimeOffset.UtcNow;
            DataSourceLabel.Text = result.DataSource switch
            {
                PoiDataSource.Api => "Nguồn: API",
                PoiDataSource.Cache => "Nguồn: cache",
                _ => "Nguồn: fallback"
            };

            RecalculatePoiDistances();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"POI load error: {ex.Message}");
            NearbyStatusLabel.IsVisible = true;
            NearbyStatusLabel.Text = "Không tải được danh sách địa điểm.";
            DiscoveryList.Children.Clear();
            UpdatePoiMarkers([]);
        }
        finally
        {
            NearbyLoadingIndicator.IsRunning = false;
            NearbyLoadingIndicator.IsVisible = false;
            _isLoadingPois = false;
        }
    }

    private void RecalculatePoiDistances()
    {
        if (_currentLocation == null)
        {
            foreach (var poi in _allPois)
            {
                poi.DistanceMeters = double.MaxValue;
            }
        }
        else
        {
            foreach (var poi in _allPois)
            {
                poi.UpdateDistanceFrom(_currentLocation);
            }
        }

        _allPois.Sort(static (left, right) =>
        {
            var distanceComparison = left.DistanceMeters.CompareTo(right.DistanceMeters);
            return distanceComparison != 0
                ? distanceComparison
                : right.Priority.CompareTo(left.Priority);
        });

        _nearestPoi = _allPois.FirstOrDefault();
        UpdateNearestPoiStatus();
    }

    private void UpdateNearestPoiStatus()
    {
        if (_currentLocation == null)
        {
            NearestPoiLabel.Text = "Chưa có vị trí hiện tại. Cấp quyền để gợi ý địa điểm gần nhất.";
            GpsStatusLabel.Text = "GPS đang chờ";
            GpsStatusDot.Color = MauiColor.FromArgb("#F78A44");
            LocationStateTitleLabel.Text = "Vị trí hiện tại";
            LocationStateDetailLabel.Text = "Cần quyền GPS để xác định khu vực bạn đang đứng";
            MiniPlayerPoiLabel.Text = "Chưa sẵn sàng thuyết minh";
            MiniPlayerStatusLabel.Text = "Bật vị trí để app gợi ý nội dung theo địa điểm gần bạn";
            return;
        }

        if (_nearestPoi == null || _nearestPoi.DistanceMeters == double.MaxValue)
        {
            NearestPoiLabel.Text = "Chưa có địa điểm phù hợp gần bạn.";
            GpsStatusLabel.Text = "GPS sẵn sàng";
            GpsStatusDot.Color = MauiColor.FromArgb("#22A35A");
            LocationStateTitleLabel.Text = "Vị trí hiện tại";
            LocationStateDetailLabel.Text = "Đã có vị trí, đang chờ dữ liệu địa điểm phù hợp";
            MiniPlayerPoiLabel.Text = "Chưa có địa điểm gần bạn";
            MiniPlayerStatusLabel.Text = "Mini player sẽ hiện nội dung khi có POI nằm trong tầm theo dõi";
            return;
        }

        NearestPoiLabel.Text = $"Gần nhất: {_nearestPoi.Name} ({_nearestPoi.DistanceDisplay})";
        GpsStatusLabel.Text = "GPS sẵn sàng";
        GpsStatusDot.Color = MauiColor.FromArgb("#22A35A");
        LocationStateTitleLabel.Text = "Vị trí hiện tại";
        LocationStateDetailLabel.Text = $"Gần {_nearestPoi.Name} • {_nearestPoi.DistanceDisplay}";
        MiniPlayerPoiLabel.Text = _nearestPoi.Name;
        MiniPlayerStatusLabel.Text = $"Sẵn sàng phát thuyết minh khi bạn vào bán kính {Math.Round(_nearestPoi.TriggerRadiusMeters)}m";
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
            BackgroundColor = MauiColor.FromArgb("#FFFFFF"),
            Padding = new Thickness(16),
            Stroke = MauiColor.FromArgb("#E5EAF2"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(24) }
        };

        var cardGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(64)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        var icon = new Border
        {
            WidthRequest = 64,
            HeightRequest = 64,
                BackgroundColor = MauiColor.FromArgb("#EAF1FF"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(14) },
            Content = new Label
            {
                Text = poi.IconGlyph,
                FontFamily = "MaterialIcons",
                FontSize = 28,
                    TextColor = MauiColor.FromArgb("#0058BC"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        cardGrid.SetColumn(icon, 0);
        cardGrid.Children.Add(icon);

        var infoStack = new VerticalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };
        var titleText = _nearestPoi?.Id == poi.Id ? $"{poi.Name} • Gần nhất" : poi.Name;

        infoStack.Children.Add(new Label
        {
            Text = titleText,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
                TextColor = MauiColor.FromArgb("#1A1B1F"),
            LineBreakMode = LineBreakMode.TailTruncation
        });

        infoStack.Children.Add(new Label
        {
            Text = poi.CategoryLabel,
            FontSize = 12,
            TextColor = MauiColor.FromArgb("#6B7382")
        });

        infoStack.Children.Add(new Label
        {
            Text = $"Cách {poi.DistanceDisplay} • Kích hoạt trong {Math.Round(poi.TriggerRadiusMeters)}m",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = MauiColor.FromArgb("#414755")
        });

        if (!string.IsNullOrWhiteSpace(poi.Description))
        {
            infoStack.Children.Add(new Label
            {
                Text = poi.Description,
                FontSize = 11,
                TextColor = MauiColor.FromArgb("#6A6F7D"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 2
            });
        }

        cardGrid.SetColumn(infoStack, 1);
        cardGrid.Children.Add(infoStack);

        var playButton = new Button
        {
            Text = "Phát",
            FontSize = 13,
            Padding = new Thickness(14, 8),
            CornerRadius = 18,
            BackgroundColor = MauiColor.FromArgb("#F78A44"),
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };
        playButton.Clicked += async (_, _) => await PlayNarrationAsync(poi, "manual", userInitiated: true);

        cardGrid.SetColumn(playButton, 2);
        cardGrid.Children.Add(playButton);

        card.Content = cardGrid;
        return card;
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
            Command = new Command(() => OnCategoryChipTapped(categoryKey))
        });

        return chip;
    }

    private void OnCategoryChipTapped(string categoryKey)
    {
        _selectedCategoryKey = categoryKey;
        RefreshCategoryChipStyles();
        ApplyFilters();
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
            chip.BackgroundColor = isActive ? MauiColor.FromArgb("#0058BC") : MauiColor.FromArgb("#E9E7ED");
            label.TextColor = isActive ? Colors.White : MauiColor.FromArgb("#414755");
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<PointOfInterest> query = _allPois;

        if (_selectedCategoryKey != "all")
        {
            query = query.Where(p => p.CategoryKey == _selectedCategoryKey);
        }

        if (!string.IsNullOrWhiteSpace(_searchKeyword))
        {
            query = query.Where(p =>
                p.Name.Contains(_searchKeyword, StringComparison.OrdinalIgnoreCase)
                || p.CategoryLabel.Contains(_searchKeyword, StringComparison.OrdinalIgnoreCase)
                || p.Description.Contains(_searchKeyword, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query
            .OrderBy(p => p.DistanceMeters)
            .ThenByDescending(p => p.Priority)
            .ToList();

        BindNearbyCards(filtered);
        UpdatePoiMarkers(filtered);

        NearbyStatusLabel.IsVisible = filtered.Count == 0;
        NearbyStatusLabel.Text = filtered.Count == 0
            ? "Không tìm thấy địa điểm phù hợp."
            : string.Empty;
    }

    private void UpdatePoiMarkers(IEnumerable<PointOfInterest> pois)
    {
        if (_poiLayer == null)
        {
            return;
        }

        var features = new List<IFeature>();
        foreach (var poi in pois)
        {
            var point = SphericalMercator.FromLonLat(poi.Longitude, poi.Latitude).ToMPoint();
            var feature = new PointFeature(point);
            feature.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                Fill = new MapsuiStyleBrush(_nearestPoi?.Id == poi.Id ? MapsuiStyleColor.Orange : MapsuiStyleColor.Red),
                Outline = new MapsuiStylePen
                {
                    Color = MapsuiStyleColor.White,
                    Width = 2
                },
                SymbolScale = _nearestPoi?.Id == poi.Id ? 0.9 : 0.7
            });
            features.Add(feature);
        }

        _poiLayer.Features = features;
        _poiLayer.DataHasChanged();
    }

    private async Task PlayNarrationAsync(PointOfInterest poi, string triggerType, bool userInitiated)
    {
        if (_isNarrationRunning)
        {
            return;
        }

        _isNarrationRunning = true;
        NearbyStatusLabel.IsVisible = true;
        NearbyStatusLabel.Text = userInitiated
            ? $"Đang phát thuyết minh: {poi.Name}"
            : $"Đang tự động phát theo vị trí: {poi.Name}";
        MiniPlayerPoiLabel.Text = poi.Name;
        MiniPlayerStatusLabel.Text = userInitiated
            ? "Đang phát thủ công từ mini player"
            : "Đang phát tự động theo vị trí hiện tại";

        try
        {
            await _narrationService.PlayAsync(poi, triggerType);
            _lastPlaybackByPoiId[poi.Id] = DateTimeOffset.UtcNow;
            NearbyStatusLabel.Text = $"Đã phát xong: {poi.Name}";
            MiniPlayerStatusLabel.Text = $"Đã phát xong. Sẵn sàng cho lần kích hoạt tiếp theo quanh {poi.Name}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Narration error: {ex.Message}");
            NearbyStatusLabel.Text = "Không thể phát thuyết minh lúc này.";
            MiniPlayerStatusLabel.Text = "Chưa thể phát thuyết minh lúc này";
        }
        finally
        {
            _isNarrationRunning = false;
        }
    }

    private async Task EvaluateAutoTriggerAsync()
    {
        if (_currentLocation == null || _isNarrationRunning)
        {
            return;
        }

        var candidate = _allPois
            .Where(poi => poi.DistanceMeters <= poi.TriggerRadiusMeters)
            .OrderBy(poi => poi.DistanceMeters)
            .ThenByDescending(poi => poi.Priority)
            .FirstOrDefault();

        if (candidate == null)
        {
            return;
        }

        if (_lastPlaybackByPoiId.TryGetValue(candidate.Id, out var lastPlayedAt)
            && DateTimeOffset.UtcNow - lastPlayedAt < AutoTriggerCooldown)
        {
            return;
        }

        await PlayNarrationAsync(candidate, "gps", userInitiated: false);
    }

    private async Task PollLocationAsync()
    {
        await RefreshCurrentLocationAsync(requestIfMissing: false, recenterMap: false);
        RecalculatePoiDistances();
        ApplyFilters();

        if (DateTimeOffset.UtcNow - _lastPoiRefreshUtc >= PoiRefreshInterval)
        {
            await LoadPoisAsync(force: true);
        }

        await EvaluateAutoTriggerAsync();
    }

    private async Task FilterNearbyPoiAsync(string keyword)
    {
        _searchKeyword = keyword.Trim();
        ApplyFilters();
        await Task.CompletedTask;
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
            ApplyFilters();
        }
    }

    private void OnSearchCloseTapped(object? sender, EventArgs e)
    {
        SearchOverlay.IsVisible = false;
        SearchEntry.Text = string.Empty;
        NearbyStatusLabel.IsVisible = false;
        ApplyFilters();
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
        var action = await DisplayActionSheetAsync("Cài đặt", "Đóng", null, "Ngôn ngữ", "Vị trí");

        if (action == "Ngôn ngữ")
        {
            await ChangeLanguageAsync();
            return;
        }

        if (action == "Vị trí")
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
                "Quyền vị trí",
                "Ứng dụng cần quyền vị trí để tìm POI gần bạn và phát audio theo ngữ cảnh.",
                "Cấp quyền",
                "Hủy");

            if (!grant)
            {
                return;
            }

            status = await _locationService.RequestLocationPermissionAsync();
        }

        if (status == PermissionStatus.Granted)
        {
            await RefreshCurrentLocationAsync(requestIfMissing: false, recenterMap: true);
            await LoadPoisAsync(force: true);
            await EvaluateAutoTriggerAsync();
            return;
        }

        var openSettings = await DisplayAlertAsync(
            "Quyền vị trí",
            "Bạn đã từ chối quyền vị trí. Hãy mở cài đặt hệ thống để cấp quyền.",
            "Mở cài đặt",
            "Để sau");

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

        await RefreshCurrentLocationAsync(requestIfMissing: true, recenterMap: true);
        await LoadPoisAsync(force: true);
        await EvaluateAutoTriggerAsync();
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
