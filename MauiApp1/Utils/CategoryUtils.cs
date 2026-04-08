namespace MauiApp1.Utils;

public static class CategoryUtils
{
    public static string ToCategoryKey(string? amenity, string? leisure, string? tourism)
    {
        if (amenity is "restaurant" or "fast_food" or "food_court")
        {
            return "food";
        }

        if (amenity is "cafe" or "bar")
        {
            return "cafe";
        }

        if (amenity is "theatre" or "cinema" or "arts_centre")
        {
            return "theatre";
        }

        if (leisure is "park" or "garden" or "nature_reserve")
        {
            return "park";
        }

        if (leisure is "playground" or "sports_centre" || tourism == "theme_park")
        {
            return "play";
        }

        if (tourism is "attraction" or "museum" or "zoo")
        {
            return "attraction";
        }

        return "other";
    }

    public static string ToCategoryLabel(string categoryKey)
    {
        return categoryKey switch
        {
            "food" => "Quan an",
            "cafe" => "Cafe",
            "park" => "Cong vien",
            "play" => "Khu vui choi",
            "theatre" => "Nha hat",
            "attraction" => "Tham quan",
            _ => "Dia diem khac"
        };
    }

    public static string ToIconGlyph(string categoryKey, bool useMaterialIcons = false)
    {
        if (useMaterialIcons)
        {
            return categoryKey switch
            {
                "food" => "restaurant",
                "cafe" => "local_cafe",
                "park" => "park",
                "play" => "attractions",
                "theatre" => "theaters",
                "attraction" => "tour",
                _ => "place"
            };
        }

        return categoryKey switch
        {
            "food" => "\ue56c",
            "cafe" => "\ue541",
            "park" => "\ueb2f",
            "play" => "\uea44",
            "theatre" => "\ue03d",
            "attraction" => "\ue56b",
            _ => "\ue55f"
        };
    }
}
