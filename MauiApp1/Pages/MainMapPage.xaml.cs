using MauiApp1.Models;
using MauiApp1.Services;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;

namespace MauiApp1.Pages
{
    public partial class MainMapPage : ContentPage
    {
        private readonly LocationService _locationService;
        private Location? _currentLocation;

        // Default location: Ho Chi Minh City
        private const double DefaultLatitude = 10.8231;
        private const double DefaultLongitude = 106.6297;

        public MainMapPage()
        {
            InitializeComponent();
            _locationService = new LocationService();
            InitializeMap();
            LoadDiscoveryCards();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            StartPulseAnimation();
            await TryGetLocationAndCenterMapAsync();
        }

        /// <summary>
        /// Initialize the Mapsui map with OpenStreetMap tiles.
        /// </summary>
        private void InitializeMap()
        {
            var map = MapControl.Map;

            // Add OpenStreetMap tile layer
            map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            // Set default center to Ho Chi Minh City
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(
                DefaultLongitude, DefaultLatitude);
            map.Navigator.CenterOnAndZoomTo(
                sphericalMercatorCoordinate.ToMPoint(), map.Navigator.Resolutions[15]);
        }

        /// <summary>
        /// Center the map on the user's current GPS location.
        /// </summary>
        private void CenterMapOnLocation(double latitude, double longitude)
        {
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(longitude, latitude);
            MapControl.Map.Navigator.CenterOnAndZoomTo(
                sphericalMercatorCoordinate.ToMPoint(),
                MapControl.Map.Navigator.Resolutions[16]);
        }

        /// <summary>
        /// Starts the pulse animation on the user location marker.
        /// </summary>
        private void StartPulseAnimation()
        {
            var pulseAnimation = new Animation();

            pulseAnimation.Add(0, 0.5, new Animation(v => PulseRing.Scale = v, 1, 1.8, Easing.CubicOut));
            pulseAnimation.Add(0, 0.5, new Animation(v => PulseRing.Opacity = v, 0.6, 0, Easing.CubicOut));
            pulseAnimation.Add(0.5, 1, new Animation(v => PulseRing.Scale = v, 1, 1, Easing.Linear));
            pulseAnimation.Add(0.5, 1, new Animation(v => PulseRing.Opacity = v, 0.6, 0.6, Easing.Linear));

            pulseAnimation.Commit(this, "PulseAnimation", length: 2000, repeat: () => true);
        }

        /// <summary>
        /// Attempt to get the user's current location and center the map.
        /// </summary>
        private async Task TryGetLocationAndCenterMapAsync()
        {
            try
            {
                _currentLocation = await _locationService.GetCurrentLocationAsync();
                if (_currentLocation != null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Location: {_currentLocation.Latitude}, {_currentLocation.Longitude}");
                    CenterMapOnLocation(_currentLocation.Latitude, _currentLocation.Longitude);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
            }
        }

        /// <summary>
        /// Load demo discovery cards into the bottom sheet.
        /// </summary>
        private void LoadDiscoveryCards()
        {
            var discoveries = new List<PointOfInterest>
            {
                new PointOfInterest
                {
                    Name = "Artisan Hearth",
                    Category = "Nhà hàng",
                    Rating = 4.9,
                    Distance = "250m",
                    HasAudio = true,
                    AudioStatus = "SMART AUDIO",
                    IconGlyph = "\ue56c",
                    Description = "Pizza thủ công tại chợ ngoài trời"
                },
                new PointOfInterest
                {
                    Name = "Silk Road Spices",
                    Category = "Quán ăn đường phố",
                    Rating = 4.7,
                    Distance = "400m",
                    HasAudio = false,
                    AudioStatus = "ĐỢI: 5P",
                    IconGlyph = "\ue56c",
                    Description = "Kebab đường phố đầy màu sắc"
                },
                new PointOfInterest
                {
                    Name = "The Old Mill Bakery",
                    Category = "Tiệm bánh",
                    Rating = 5.0,
                    Distance = "650m",
                    HasAudio = true,
                    AudioStatus = "SMART AUDIO",
                    IconGlyph = "\uea53",
                    Description = "Tiệm bánh truyền thống"
                }
            };

            foreach (var poi in discoveries)
            {
                DiscoveryList.Children.Add(CreateDiscoveryCard(poi));
            }
        }

        /// <summary>
        /// Creates a styled discovery card for a POI.
        /// </summary>
        private View CreateDiscoveryCard(PointOfInterest poi)
        {
            var card = new Border
            {
                BackgroundColor = Color.FromArgb("#FFFFFF"),
                Padding = new Thickness(16),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(24) },
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Colors.Black),
                    Offset = new Point(0, 2),
                    Radius = 8,
                    Opacity = 0.06f
                }
            };

            var cardGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(80)),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 16
            };

            // Image placeholder (colored box with icon)
            var imageBorder = new Border
            {
                WidthRequest = 80,
                HeightRequest = 80,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
            };

            var imageGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            imageGradient.GradientStops.Add(new GradientStop(Color.FromArgb("#D8E2FF"), 0.0f));
            imageGradient.GradientStops.Add(new GradientStop(Color.FromArgb("#E2DFFF"), 1.0f));
            imageBorder.Background = imageGradient;

            var imageIcon = new Label
            {
                Text = poi.IconGlyph,
                FontFamily = "MaterialIcons",
                FontSize = 32,
                TextColor = Color.FromArgb("#0058BC"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            imageBorder.Content = imageIcon;

            // If it has audio, add a play badge
            if (poi.HasAudio)
            {
                var imageStack = new Grid();
                imageStack.Children.Add(imageBorder);

                var playBadge = new Border
                {
                    WidthRequest = 24,
                    HeightRequest = 24,
                    BackgroundColor = Color.FromArgb("#0058BC"),
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, -4, -4, 0),
                    Shadow = new Shadow
                    {
                        Brush = new SolidColorBrush(Colors.Black),
                        Offset = new Point(0, 2),
                        Radius = 4,
                        Opacity = 0.2f
                    },
                    Content = new Label
                    {
                        Text = "\ue037",
                        FontFamily = "MaterialIcons",
                        FontSize = 16,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };
                imageStack.Children.Add(playBadge);
                cardGrid.SetColumn(imageStack, 0);
                cardGrid.Children.Add(imageStack);
            }
            else
            {
                cardGrid.SetColumn(imageBorder, 0);
                cardGrid.Children.Add(imageBorder);
            }

            // Right side: info
            var infoStack = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };

            // Name + Badge row
            var topRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };

            var nameLabel = new Label
            {
                Text = poi.Name,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1A1B1F"),
                LineBreakMode = LineBreakMode.TailTruncation
            };
            topRow.SetColumn(nameLabel, 0);
            topRow.Children.Add(nameLabel);

            var statusBadge = new Border
            {
                BackgroundColor = poi.HasAudio ? Color.FromArgb("#1A0058BC") : Color.FromArgb("#E9E7ED"),
                StrokeThickness = 0,
                Padding = new Thickness(8, 4),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) },
                Content = new Label
                {
                    Text = poi.AudioStatus,
                    FontSize = 9,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = poi.HasAudio ? Color.FromArgb("#0058BC") : Color.FromArgb("#414755"),
                    CharacterSpacing = 1
                }
            };
            topRow.SetColumn(statusBadge, 1);
            topRow.Children.Add(statusBadge);

            infoStack.Children.Add(topRow);

            // Rating + Distance row
            var detailRow = new HorizontalStackLayout { Spacing = 8 };
            detailRow.Children.Add(new Label
            {
                Text = "\ue838",
                FontFamily = "MaterialIcons",
                FontSize = 14,
                TextColor = Color.FromArgb("#414755"),
                VerticalOptions = LayoutOptions.Center
            });
            detailRow.Children.Add(new Label
            {
                Text = poi.Rating.ToString("0.0"),
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#414755"),
                VerticalOptions = LayoutOptions.Center
            });
            detailRow.Children.Add(new Border
            {
                WidthRequest = 4,
                HeightRequest = 4,
                BackgroundColor = Color.FromArgb("#C1C6D7"),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(2) },
                VerticalOptions = LayoutOptions.Center
            });
            detailRow.Children.Add(new Label
            {
                Text = $"cách {poi.Distance}",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#414755"),
                VerticalOptions = LayoutOptions.Center
            });
            infoStack.Children.Add(detailRow);

            cardGrid.SetColumn(infoStack, 1);
            cardGrid.Children.Add(infoStack);

            card.Content = cardGrid;
            return card;
        }

        private async void OnRecenterTapped(object? sender, EventArgs e)
        {
            // Animate the FAB
            if (FabRecenter != null)
            {
                await FabRecenter.ScaleToAsync(0.9, 100);
                await FabRecenter.ScaleToAsync(1.0, 100, Easing.SpringOut);
            }

            // Re-request location
            await TryGetLocationAndCenterMapAsync();

            if (_currentLocation != null)
            {
                await DisplayAlertAsync("Vị trí hiện tại",
                    $"Vĩ độ: {_currentLocation.Latitude:F6}\nKinh độ: {_currentLocation.Longitude:F6}",
                    "OK");
            }
            else
            {
                await DisplayAlertAsync("Vị trí", "Không thể lấy vị trí hiện tại.", "OK");
            }
        }
    }
}
