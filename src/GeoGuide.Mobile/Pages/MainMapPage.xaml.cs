using CommunityToolkit.Maui.Core;
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

    private readonly LocationService _locationService;
    private readonly PoiRepository _poiRepository;
    private readonly GeofenceEngineService _geofenceEngineService;
    private readonly TriggerGuardService _triggerGuardService;
    private readonly AccessModeService _accessModeService;
    private readonly TtsSettingsService _ttsSettingsService;
    private readonly OfflineAnalyticsLogService _offlineAnalyticsLogService;
    private readonly NarrationService _narrationService;
    private readonly MediaPrefetchService _mediaPrefetchService;
    private readonly LocalDatabaseService _localDatabaseService;
    private readonly List<PointOfInterest> _allPois = [];
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
    private PointOfInterest? _selectedPoiForPlayback;
    private bool _isInitialized;
    private bool _isLoadingPois;
    private bool _isMapFullScreen;
    private bool _isAutoNarrationEnabled = true;
    private bool _isFollowUserEnabled;
    private DateTimeOffset _lastPoiRefreshUtc = DateTimeOffset.MinValue;
    private string _selectedCategoryKey = "all";
    private string _searchKeyword = string.Empty;
    private TaskCompletionSource<bool>? _audioPlaybackCompletionSource;
    private PoiDataSource _lastPoiDataSource = PoiDataSource.BundledFallback;

    public MainMapPage()
    {
        InitializeComponent();
        Mapsui.Widgets.InfoWidgets.LoggingWidget.ShowLoggingInMap = Mapsui.Widgets.ActiveMode.No;

        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Service provider is not available.");

        _locationService = services.GetRequiredService<LocationService>();
        _poiRepository = services.GetRequiredService<PoiRepository>();
        _geofenceEngineService = services.GetRequiredService<GeofenceEngineService>();
        _triggerGuardService = services.GetRequiredService<TriggerGuardService>();
        _accessModeService = services.GetRequiredService<AccessModeService>();
        _ttsSettingsService = services.GetRequiredService<TtsSettingsService>();
        _offlineAnalyticsLogService = services.GetRequiredService<OfflineAnalyticsLogService>();
        _narrationService = services.GetRequiredService<NarrationService>();
        _mediaPrefetchService = services.GetRequiredService<MediaPrefetchService>();
        _localDatabaseService = services.GetRequiredService<LocalDatabaseService>();
        _narrationService.PlaybackChanged += OnNarrationPlaybackChanged;
        _locationService.LocationUpdated += OnLocationUpdated;

        _locationTimer = Dispatcher.CreateTimer();
        _locationTimer.Interval = TimeSpan.FromSeconds(30);
        _locationTimer.Tick += async (_, _) => await PollLocationAsync();

        InitializeMap();
        BuildCategoryChips();
        UpdateAutoNarrationUiState();
        UpdateFollowUserUiState();
        UpdateAccessModeUiState();
        UpdateDemoFeaturePanelAsync().SafeFireAndForget();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            _isInitialized = true;
            await InitializeAsync();
        }

        await _locationService.StartTrackingAsync(
            interval: TimeSpan.FromSeconds(8),
            accuracy: GeolocationAccuracy.Best);

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

        _ = _locationService.StopTrackingAsync();
    }

    private async Task InitializeAsync()
    {
        await RefreshCurrentLocationAsync(requestIfMissing: false, recenterMap: true);
        await LoadPoisAsync(force: true);
        await EvaluateAutoTriggerAsync();
        await UpdateDemoFeaturePanelAsync();
    }

    private void InitializeMap()
    {
        var map = MapControl.Map;
        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        MapControl.Info += async (_, e) => await HandleMapInfoAsync(e);

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
                permission = await _locationService.RequestBackgroundLocationPermissionAsync();
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
            _lastPoiDataSource = result.DataSource;
            _lastPoiRefreshUtc = DateTimeOffset.UtcNow;
            DataSourceLabel.Text = result.DataSource switch
            {
                PoiDataSource.Api => "Nguồn: API",
                PoiDataSource.Cache => "Nguồn: cache",
                _ => "Nguồn: fallback"
            };

            RecalculatePoiDistances();
            ApplyFilters();
            await UpdateDemoFeaturePanelAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"POI load error: {ex.Message}");
            NearbyStatusLabel.IsVisible = true;
            NearbyStatusLabel.Text = "Không tải được danh sách địa điểm.";
            DiscoveryList.Children.Clear();
            UpdatePoiMarkers([]);
            await UpdateDemoFeaturePanelAsync();
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
        var ranked = _geofenceEngineService.BuildRankedSnapshot(_allPois, _currentLocation);
        _allPois.Clear();
        _allPois.AddRange(ranked);
        _nearestPoi = _geofenceEngineService.SelectNearest(_allPois);
        UpdateNearestPoiStatus();
    }

    private void CenterMapOnNearestPoi()
    {
        if (_nearestPoi == null)
        {
            return;
        }

        CenterMapOnLocation(_nearestPoi.Latitude, _nearestPoi.Longitude);
    }

    private void UpdateNearestPoiStatus()
    {
        UpdateAutoNarrationUiState();

        if (_selectedPoiForPlayback != null && _allPois.All(poi => poi.Id != _selectedPoiForPlayback.Id))
        {
            _selectedPoiForPlayback = null;
        }

        if (_currentLocation == null)
        {
            NearestPoiLabel.Text = "Chưa có vị trí hiện tại. Cấp quyền để gợi ý địa điểm gần nhất.";
            GpsStatusLabel.Text = "GPS đang chờ";
            GpsStatusDot.Color = MauiColor.FromArgb("#F78A44");
            LocationStateTitleLabel.Text = "Vị trí hiện tại";
            LocationStateDetailLabel.Text = "Cần quyền GPS để xác định khu vực bạn đang đứng";
            UpdateMiniPlayerLabels("Chưa sẵn sàng thuyết minh", "Bật vị trí để app gợi ý nội dung theo địa điểm gần bạn");
            UpdateDemoGeoFeature();
            return;
        }

        if (_nearestPoi == null || _nearestPoi.DistanceMeters == double.MaxValue)
        {
            NearestPoiLabel.Text = "Chưa có địa điểm phù hợp gần bạn.";
            GpsStatusLabel.Text = "GPS sẵn sàng";
            GpsStatusDot.Color = MauiColor.FromArgb("#22A35A");
            LocationStateTitleLabel.Text = "Vị trí hiện tại";
            LocationStateDetailLabel.Text = "Đã có vị trí, đang chờ dữ liệu địa điểm phù hợp";
            UpdateMiniPlayerLabels("Chưa có địa điểm gần bạn", "Mini player sẽ hiện nội dung khi có POI nằm trong tầm theo dõi");
            UpdateDemoGeoFeature();
            return;
        }

        NearestPoiLabel.Text = $"Gần nhất: {_nearestPoi.Name} ({_nearestPoi.DistanceDisplay})";
        GpsStatusLabel.Text = "GPS sẵn sàng";
        GpsStatusDot.Color = MauiColor.FromArgb("#22A35A");
        LocationStateTitleLabel.Text = "Vị trí hiện tại";
        LocationStateDetailLabel.Text = $"Gần {_nearestPoi.Name} • {_nearestPoi.DistanceDisplay}";

        var activePoi = GetActivePlaybackPoi();
        UpdateMiniPlayerLabels(
            activePoi?.Name ?? _nearestPoi.Name,
            activePoi == null || activePoi.Id == _nearestPoi.Id
                ? $"Sẵn sàng phát thuyết minh khi bạn vào bán kính {Math.Round(_nearestPoi.TriggerRadiusMeters)}m"
                : $"Đã chọn thủ công • Cách {activePoi.DistanceDisplay}");
        UpdateDemoGeoFeature();
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

        card.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await ShowPoiDetailAsync(poi))
        });

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
            var feature = new PointFeature(point)
            {
                Data = poi
            };
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
        _selectedPoiForPlayback = poi;
        NearbyStatusLabel.IsVisible = true;
        NearbyStatusLabel.Text = userInitiated
            ? $"Đã xếp hàng phát thủ công: {poi.Name}"
            : $"Đã xếp hàng phát tự động: {poi.Name}";
        UpdateMiniPlayerLabels(
            poi.Name,
            userInitiated
            ? "Đã thêm vào hàng đợi từ mini player"
            : "Đã thêm vào hàng đợi tự động theo vị trí");

        try
        {
            await _narrationService.EnqueueAsync(poi, triggerType, userInitiated, PlayAudioAsync);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Narration error: {ex.Message}");
            NearbyStatusLabel.Text = ex is InvalidOperationException
                ? ex.Message
                : "Không thể phát thuyết minh lúc này.";
            UpdateMiniPlayerLabels(poi.Name, NearbyStatusLabel.Text);
        }
    }

    private async Task EvaluateAutoTriggerAsync()
    {
        if (!_isAutoNarrationEnabled || _currentLocation == null || !GetAccessState().IsFullAccess)
        {
            return;
        }

        var candidate = _geofenceEngineService.SelectTriggerCandidate(_allPois);

        if (candidate == null)
        {
            return;
        }

        if (!_triggerGuardService.CanTrigger(candidate, DateTimeOffset.UtcNow))
        {
            return;
        }

        await PlayNarrationAsync(candidate, "gps", userInitiated: false);
    }

    private async Task PollLocationAsync()
    {
        if (_currentLocation == null)
        {
            await RefreshCurrentLocationAsync(requestIfMissing: false, recenterMap: false);
        }

        RecalculatePoiDistances();
        ApplyFilters();

        if (DateTimeOffset.UtcNow - _lastPoiRefreshUtc >= PoiRefreshInterval)
        {
            await LoadPoisAsync(force: true);
        }

        await EvaluateAutoTriggerAsync();
        await UpdateDemoFeaturePanelAsync();
    }

    private void OnLocationUpdated(object? sender, Location location)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _currentLocation = location;
            UpdateUserLocationMarker(location.Latitude, location.Longitude);
            _ = _offlineAnalyticsLogService.LogPositionUpdateAsync(location.Latitude, location.Longitude);
            if (_isFollowUserEnabled)
            {
                CenterMapOnLocation(location.Latitude, location.Longitude);
            }

            RecalculatePoiDistances();
            ApplyFilters();
            await EvaluateAutoTriggerAsync();
        });
    }

    private void UpdateAutoNarrationUiState()
    {
        var isFullAccess = GetAccessState().IsFullAccess;
        if (!isFullAccess)
        {
            _isAutoNarrationEnabled = false;
        }

        if (AutoNarrationSwitch.IsToggled != _isAutoNarrationEnabled)
        {
            AutoNarrationSwitch.IsToggled = _isAutoNarrationEnabled;
        }

        AutoNarrationSwitch.IsEnabled = isFullAccess;
        AutoNarrationStateLabel.Text = isFullAccess
            ? (_isAutoNarrationEnabled ? "Tự động bật" : "Tự động tắt")
            : "Tự động: Full";
        AutoNarrationStateLabel.TextColor = _isAutoNarrationEnabled
            ? MauiColor.FromArgb("#1DB954")
            : MauiColor.FromArgb("#B3B3B3");
        UpdateDemoNarrationFeature();
    }

    private AccessModeState GetAccessState() => _accessModeService.GetState();

    private void UpdateAccessModeUiState()
    {
        var state = GetAccessState();
        AccessModeLabel.Text = state.IsFullAccess ? "FULL ACCESS" : "TRIAL MODE";
        AccessModeLabel.TextColor = state.IsFullAccess
            ? MauiColor.FromArgb("#1E824C")
            : MauiColor.FromArgb("#B84A00");

        UpdateAutoNarrationUiState();
        UpdateDemoAccessFeature();
    }

    private async Task HandleMapInfoAsync(MapInfoEventArgs eventArgs)
    {
        if (_poiLayer == null)
        {
            return;
        }

        var mapInfo = eventArgs.GetMapInfo?.Invoke([_poiLayer]);
        if (mapInfo?.Feature?.Data is not PointOfInterest poi)
        {
            return;
        }

        eventArgs.Handled = true;
        await ShowPoiPopupAsync(poi);
    }

    private async Task ShowPoiPopupAsync(PointOfInterest poi)
    {
        _selectedPoiForPlayback = poi;
        UpdateMiniPlayerLabels(poi.Name, $"Đã chọn trên bản đồ • Cách {poi.DistanceDisplay}");

        var action = await DisplayActionSheetAsync(
            $"{poi.Name} • {poi.DistanceDisplay}",
            "Đóng",
            null,
            "Xem chi tiết",
            "Phát thuyết minh");

        if (action == "Xem chi tiết")
        {
            await ShowPoiDetailAsync(poi);
            return;
        }

        if (action == "Phát thuyết minh")
        {
            await PlayNarrationAsync(poi, "map-tap", userInitiated: true);
        }
    }

    private PointOfInterest? GetActivePlaybackPoi()
    {
        return _selectedPoiForPlayback ?? _nearestPoi;
    }

    private void UpdateMiniPlayerLabels(string title, string status)
    {
        MiniPlayerPoiLabel.Text = title;
        MiniPlayerStatusLabel.Text = status;
    }

    private Task PlayAudioAsync(Uri audioUri, CancellationToken cancellationToken)
    {
        var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _audioPlaybackCompletionSource = completionSource;

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                NarrationMediaElement.Stop();
                NarrationMediaElement.Source = audioUri;
                NarrationMediaElement.MetadataTitle = _selectedPoiForPlayback?.Name ?? "GeoGuide";
                NarrationMediaElement.MetadataArtist = "GeoGuide";
                NarrationMediaElement.Play();

                using var registration = cancellationToken.Register(() =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        NarrationMediaElement.Stop();
                        _audioPlaybackCompletionSource?.TrySetCanceled(cancellationToken);
                    });
                });

                await completionSource.Task;
            }
            finally
            {
                if (ReferenceEquals(_audioPlaybackCompletionSource, completionSource))
                {
                    _audioPlaybackCompletionSource = null;
                }
            }
        });
    }

    private void OnNarrationMediaEnded(object? sender, EventArgs e)
    {
        _audioPlaybackCompletionSource?.TrySetResult(true);
    }

    private void OnNarrationMediaFailed(object? sender, MediaFailedEventArgs e)
    {
        _audioPlaybackCompletionSource?.TrySetException(new InvalidOperationException($"Không phát được audio từ backend: {e.ErrorMessage}"));
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
        var action = await DisplayActionSheetAsync("Cài đặt", "Đóng", null, "Ngôn ngữ ứng dụng", "Giọng đọc TTS", "Vị trí");

        if (action == "Ngôn ngữ ứng dụng")
        {
            await ChangeLanguageAsync();
            return;
        }

        if (action == "Giọng đọc TTS")
        {
            await ConfigureTtsSettingsAsync();
            return;
        }

        if (action == "Vị trí")
        {
            await HandleLocationSettingsAsync();
        }
    }

    private async Task ConfigureTtsSettingsAsync()
    {
        var current = _ttsSettingsService.Get();

        var languageChoice = await DisplayActionSheetAsync(
            "Ngôn ngữ giọng đọc TTS",
            "Hủy",
            null,
            "Theo nội dung POI",
            "Tiếng Việt (vi-VN)",
            "Tiếng Anh (en-US)");

        if (languageChoice == "Hủy" || string.IsNullOrWhiteSpace(languageChoice))
        {
            return;
        }

        var languageCode = languageChoice switch
        {
            "Tiếng Việt (vi-VN)" => "vi-VN",
            "Tiếng Anh (en-US)" => "en-US",
            _ => string.Empty
        };

        var pitchInput = await DisplayPromptAsync(
            "Pitch TTS",
            "Nhập giá trị 0.5 - 2.0",
            "Lưu",
            "Bỏ qua",
            initialValue: current.Pitch.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));

        var volumeInput = await DisplayPromptAsync(
            "Volume TTS",
            "Nhập giá trị 0.0 - 1.0",
            "Lưu",
            "Bỏ qua",
            initialValue: current.Volume.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));

        var rateInput = await DisplayPromptAsync(
            "Tốc độ đọc TTS",
            "Nhập giá trị 0.25 - 2.0",
            "Lưu",
            "Bỏ qua",
            initialValue: current.SpeechRate.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));

        var updated = new TtsSettings
        {
            PreferredLanguageCode = languageCode,
            Pitch = TryParseFloatOrDefault(pitchInput, current.Pitch),
            Volume = TryParseFloatOrDefault(volumeInput, current.Volume),
            SpeechRate = TryParseFloatOrDefault(rateInput, current.SpeechRate)
        };

        _ttsSettingsService.Save(updated);
        await DisplayAlertAsync("TTS", "Đã lưu cấu hình giọng đọc.", "OK");
    }

    private async Task ShowPoiDetailAsync(PointOfInterest poi)
    {
        var detail = string.Join(
            Environment.NewLine,
            new[]
            {
                $"Tên: {poi.Name}",
                $"Danh mục: {poi.CategoryLabel}",
                $"Khoảng cách: {poi.DistanceDisplay}",
                $"Bán kính kích hoạt: {Math.Round(poi.TriggerRadiusMeters)}m",
                $"Cooldown: {poi.CooldownMinutes} phút",
                $"Ngôn ngữ: {poi.LanguageCode}",
                string.IsNullOrWhiteSpace(poi.AudioUrl) ? "Audio: Không có file audio" : "Audio: Có file audio",
                string.IsNullOrWhiteSpace(poi.TtsScript) ? "TTS script: Không có" : "TTS script: Có",
                string.IsNullOrWhiteSpace(poi.MapUrl) ? "Bản đồ ngoài: Không có" : $"Bản đồ ngoài: {poi.MapUrl}",
                string.IsNullOrWhiteSpace(poi.Description) ? "Mô tả: Không có" : $"Mô tả: {poi.Description}"
            });

        await DisplayAlertAsync("Chi tiết POI", detail, "Đóng");
    }

    private static float TryParseFloatOrDefault(string? value, float fallback)
    {
        return float.TryParse(
            value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : fallback;
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

            status = await _locationService.RequestBackgroundLocationPermissionAsync();
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

    private async void OnMiniPlayerPlayTapped(object? sender, EventArgs e)
    {
        var poi = GetActivePlaybackPoi();
        if (poi == null)
        {
            NearbyStatusLabel.IsVisible = true;
            NearbyStatusLabel.Text = "Chưa có POI khả dụng để phát.";
            return;
        }

        await PlayNarrationAsync(poi, "mini-player", userInitiated: true);
    }

    private async void OnAutoNarrationToggled(object? sender, ToggledEventArgs e)
    {
        if (e.Value && !GetAccessState().IsFullAccess)
        {
            _isAutoNarrationEnabled = false;
            UpdateAutoNarrationUiState();
            NearbyStatusLabel.IsVisible = true;
            NearbyStatusLabel.Text = "Tự động phát chỉ khả dụng ở Full Access.";
            return;
        }

        _isAutoNarrationEnabled = e.Value;
        UpdateAutoNarrationUiState();

        NearbyStatusLabel.IsVisible = true;
        NearbyStatusLabel.Text = _isAutoNarrationEnabled
            ? "Đã bật tự động phát theo bán kính POI."
            : "Đã tắt tự động phát. Bạn vẫn có thể bấm Play thủ công.";

        if (_isAutoNarrationEnabled)
        {
            await EvaluateAutoTriggerAsync();
        }
    }

    private async void OnQrActivateTapped(object? sender, EventArgs e)
    {
        var payload = await DisplayPromptAsync(
            "Kích hoạt bằng QR",
            "Nhập payload QR (demo: GEOGUIDE:TRIAL:DEMO hoặc GEOGUIDE:FULL:DEMO)",
            "Kích hoạt",
            "Hủy",
            maxLength: 300,
            initialValue: "GEOGUIDE:TRIAL:DEMO");

        if (string.IsNullOrWhiteSpace(payload))
        {
            return;
        }

        var success = _accessModeService.TryActivateFromQrPayload(payload, out var message);
        _ = _offlineAnalyticsLogService.LogQrScannedAsync(payload);
        await DisplayAlertAsync(success ? "Kích hoạt thành công" : "Kích hoạt thất bại", message, "OK");
        UpdateAccessModeUiState();
    }

    private void OnNarrationPlaybackChanged(object? sender, NarrationPlaybackEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            NearbyStatusLabel.IsVisible = true;
            NearbyStatusLabel.Text = e.Message;

            if (_selectedPoiForPlayback?.Id == e.PoiId || _selectedPoiForPlayback == null)
            {
                UpdateMiniPlayerLabels(e.PoiName, e.Message);
            }

            UpdateDemoNarrationFeature(e.Message);
            UpdateDemoFeaturePanelAsync().SafeFireAndForget();
        });
    }

    private void OnExpandMapTapped(object? sender, EventArgs e)
    {
        EnterMapFullScreen();
    }

    private void OnFocusNearestPoiTapped(object? sender, EventArgs e)
    {
        CenterMapOnNearestPoi();
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

    private void OnFollowUserToggled(object? sender, ToggledEventArgs e)
    {
        _isFollowUserEnabled = e.Value;
        UpdateFollowUserUiState();

        if (_isFollowUserEnabled && _currentLocation != null)
        {
            CenterMapOnLocation(_currentLocation.Latitude, _currentLocation.Longitude);
        }
    }

    private void UpdateFollowUserUiState()
    {
        FollowUserStateLabel.Text = _isFollowUserEnabled ? "Theo dõi: Bật" : "Theo dõi: Tắt";

        if (FollowUserSwitch.IsToggled != _isFollowUserEnabled)
        {
            FollowUserSwitch.IsToggled = _isFollowUserEnabled;
        }
    }

    private async Task UpdateDemoFeaturePanelAsync()
    {
        try
        {
            var localPoiCount = await _localDatabaseService.CountPoisAsync();
            var pendingLogCount = await _localDatabaseService.CountPendingOfflineLogsAsync();
            var cachedMediaCount = await _mediaPrefetchService.CountCachedMediaAsync();
            var ttsSettings = _ttsSettingsService.Get();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                OfflineModeValueLabel.Text = _lastPoiDataSource switch
                {
                    PoiDataSource.Api => "Online + offline",
                    PoiDataSource.Cache => "Offline từ cache",
                    _ => "Offline fallback"
                };
                OfflineModeHintLabel.Text = $"POI local: {localPoiCount}";

                MediaCacheValueLabel.Text = cachedMediaCount == 1 ? "1 file cache" : $"{cachedMediaCount} file cache";
                MediaCacheHintLabel.Text = cachedMediaCount > 0
                    ? "Pre-download đã sẵn sàng"
                    : "Chưa có audio được tải trước";

                LoggingValueLabel.Text = pendingLogCount == 1 ? "1 log chờ sync" : $"{pendingLogCount} log chờ sync";
                LoggingHintLabel.Text = pendingLogCount > 0
                    ? "Sẽ đẩy lên CMS khi có mạng"
                    : "Nhật ký đã đồng bộ hoặc chưa phát sinh";

                NarrationHintLabel.Text =
                    $"{(string.IsNullOrWhiteSpace(ttsSettings.PreferredLanguageCode) ? "Theo POI" : ttsSettings.PreferredLanguageCode)} • tốc độ {ttsSettings.SpeechRate:0.##}x";

                UpdateDemoGeoFeature();
                UpdateDemoNarrationFeature();
                UpdateDemoAccessFeature();
            });
        }
        catch
        {
        }
    }

    private void UpdateDemoGeoFeature()
    {
        if (_currentLocation == null)
        {
            GeofenceValueLabel.Text = "GPS chưa sẵn sàng";
            GeofenceHintLabel.Text = "Cần vị trí để tính vùng kích hoạt";
            return;
        }

        if (_nearestPoi == null || _nearestPoi.DistanceMeters == double.MaxValue)
        {
            GeofenceValueLabel.Text = "Chưa có POI gần";
            GeofenceHintLabel.Text = "Đang chờ dữ liệu hoặc di chuyển";
            return;
        }

        var isInside = _nearestPoi.DistanceMeters <= _nearestPoi.TriggerRadiusMeters;
        GeofenceValueLabel.Text = isInside ? "Đang trong vùng kích hoạt" : $"Gần {_nearestPoi.Name}";
        GeofenceHintLabel.Text = isInside
            ? $"Auto trigger trong {_nearestPoi.TriggerRadiusMeters:0}m"
            : $"{_nearestPoi.DistanceDisplay} • bán kính {_nearestPoi.TriggerRadiusMeters:0}m";
    }

    private void UpdateDemoNarrationFeature(string? playbackMessage = null)
    {
        var settings = _ttsSettingsService.Get();
        NarrationValueLabel.Text = _narrationService.IsBusy ? "Đang phát / xếp hàng" : "Sẵn sàng phát";
        NarrationHintLabel.Text = !string.IsNullOrWhiteSpace(playbackMessage)
            ? playbackMessage
            : $"{(string.IsNullOrWhiteSpace(settings.PreferredLanguageCode) ? "Theo POI" : settings.PreferredLanguageCode)} • tốc độ {settings.SpeechRate:0.##}x";
    }

    private void UpdateDemoAccessFeature()
    {
        var state = GetAccessState();
        QrAccessValueLabel.Text = state.IsFullAccess ? "Full Access" : "Trial Mode";
        QrAccessHintLabel.Text = state.ActivatedAt != default
            ? $"Kích hoạt {state.ActivatedAt.LocalDateTime:dd/MM HH:mm}"
            : "Quét QR để đổi quyền truy cập";
    }

    private async void OnQuickPoiDetailTapped(object? sender, EventArgs e)
    {
        var poi = _nearestPoi ?? _selectedPoiForPlayback ?? _allPois.FirstOrDefault();
        if (poi == null)
        {
            await DisplayAlertAsync("Chi tiết POI", "Chưa có POI nào sẵn sàng để xem chi tiết.", "Đóng");
            return;
        }

        await ShowPoiDetailAsync(poi);
    }
}

internal static class TaskExtensions
{
    public static async void SafeFireAndForget(this Task task)
    {
        try
        {
            await task;
        }
        catch
        {
        }
    }
}
