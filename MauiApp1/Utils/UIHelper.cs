using MauiApp1.Models;

namespace MauiApp1.Utils;

public static class UIHelper
{
    public static Border CreateNearbyCard(PointOfInterest poi)
    {
        var card = new Border
        {
            Stroke = Color.FromArgb("#E0E0E0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 12),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) }
        };

        var cardGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(40) },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 12
        };

        var iconFrame = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#F5F7FA"),
            WidthRequest = 40,
            HeightRequest = 40,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) },
            VerticalOptions = LayoutOptions.Start
        };

        var iconLabel = new Label
        {
            Text = poi.IconGlyph,
            FontFamily = "MaterialIcons",
            FontSize = 20,
            TextColor = Color.FromArgb("#0058BC"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        iconFrame.Content = iconLabel;
        cardGrid.Children.Add(iconFrame);

        var infoStack = new VerticalStackLayout { Spacing = 4 };

        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        headerGrid.Children.Add(new Label
        {
            Text = poi.Name,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#1C1F23"),
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1
        });

        if (poi.HasAudio)
        {
            var audioIcon = new Label
            {
                Text = "\ue050",
                FontFamily = "MaterialIcons",
                FontSize = 16,
                TextColor = Color.FromArgb("#0058BC"),
                VerticalOptions = LayoutOptions.Center
            };
            headerGrid.SetColumn(audioIcon, 1);
            headerGrid.Children.Add(audioIcon);
        }

        infoStack.Children.Add(headerGrid);

        var detail = new HorizontalStackLayout { Spacing = 8 };

        detail.Children.Add(new Label
        {
            Text = poi.Category,
            FontSize = 12,
            TextColor = Color.FromArgb("#6A6F7D")
        });

        detail.Children.Add(new Label
        {
            Text = "•",
            FontSize = 12,
            TextColor = Color.FromArgb("#D1D5DB"),
            VerticalOptions = LayoutOptions.Center
        });

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

    public static Border CreateCategoryChip(string categoryKey, string label, Func<string, Task> onTapped)
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
            Command = new Command(async () => await onTapped(categoryKey))
        });

        return chip;
    }

    public static void RefreshCategoryChipStyles(Layout container, string selectedCategoryKey)
    {
        foreach (var child in container.Children)
        {
            if (child is not Border chip || chip.Content is not Label label || chip.BindingContext is not string key)
            {
                continue;
            }

            var isActive = key == selectedCategoryKey;
            chip.BackgroundColor = isActive ? Color.FromArgb("#0058BC") : Color.FromArgb("#E9E7ED");
            label.TextColor = isActive ? Colors.White : Color.FromArgb("#414755");
        }
    }
}
