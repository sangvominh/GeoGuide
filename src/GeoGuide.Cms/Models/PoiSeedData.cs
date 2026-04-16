namespace GeoGuide.Cms.Models;

public static class PoiSeedData
{
    public static readonly Poi[] All =
    [
        new()
        {
            Id = Guid.Parse("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"),
            TenantId = PoiTenant.DemoTenantId,
            Name = "Ben Thanh Market",
            Description = "Khu cho noi tieng o trung tam TP.HCM.",
            Latitude = 10.7720,
            Longitude = 106.6983,
            TriggerRadiusMeters = 80,
            Priority = 1,
            CategoryKey = "attraction",
            CategoryLabel = "Tham quan",
            ImageUrl = "https://images.unsplash.com/photo-1555921015-5532091f6026",
            MapUrl = "https://maps.google.com/?q=10.7720,106.6983",
            AudioUrl = "https://example.com/audio/ben-thanh-market.mp3",
            TtsScript = "Day la cho Ben Thanh, mot bieu tuong van hoa va du lich cua thanh pho.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        },
        new()
        {
            Id = Guid.Parse("458a1a0e-b524-4e95-92f4-684e80ea7b97"),
            TenantId = PoiTenant.DemoTenantId,
            Name = "Notre-Dame Cathedral",
            Description = "Nha tho Duc Ba Sai Gon voi kien truc Phap tieu bieu.",
            Latitude = 10.7798,
            Longitude = 106.6990,
            TriggerRadiusMeters = 90,
            Priority = 2,
            CategoryKey = "attraction",
            CategoryLabel = "Tham quan",
            ImageUrl = "https://images.unsplash.com/photo-1583417267826-aebc4d1542e1",
            MapUrl = "https://maps.google.com/?q=10.7798,106.6990",
            AudioUrl = "https://example.com/audio/notre-dame-cathedral.mp3",
            TtsScript = "Nha tho Duc Ba la mot diem nhan kien truc va lich su ngay giua trung tam thanh pho.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        },
        new()
        {
            Id = Guid.Parse("5807657e-240d-42cb-b6e8-b46fd35652ce"),
            TenantId = PoiTenant.DemoTenantId,
            Name = "Tao Dan Park",
            Description = "Cong vien xanh phu hop cho di bo va thu gian.",
            Latitude = 10.7778,
            Longitude = 106.6927,
            TriggerRadiusMeters = 100,
            Priority = 3,
            CategoryKey = "park",
            CategoryLabel = "Cong vien",
            ImageUrl = "https://images.unsplash.com/photo-1506744038136-46273834b3fb",
            MapUrl = "https://maps.google.com/?q=10.7778,106.6927",
            AudioUrl = "https://example.com/audio/tao-dan-park.mp3",
            TtsScript = "Cong vien Tao Dan la khoang xanh hien hoi, noi nguoi dan dia phuong thuong tap the duc vao sang som.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        },
        new()
        {
            Id = Guid.Parse("a76542f8-e34f-45ee-96c9-0e3961c4ca30"),
            TenantId = PoiTenant.DemoTenantId,
            Name = "Ho Chi Minh City Museum",
            Description = "Bao tang gioi thieu lich su va van hoa thanh pho.",
            Latitude = 10.7765,
            Longitude = 106.7010,
            TriggerRadiusMeters = 75,
            Priority = 4,
            CategoryKey = "museum",
            CategoryLabel = "Bao tang",
            ImageUrl = "https://images.unsplash.com/photo-1518998053901-5348d3961a04",
            MapUrl = "https://maps.google.com/?q=10.7765,106.7010",
            AudioUrl = "https://example.com/audio/hcm-city-museum.mp3",
            TtsScript = "Bao tang Thanh pho Ho Chi Minh luu giu nhieu tu lieu va hien vat quan trong.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        }
    ];

    public static readonly PoiTenant[] Tenants =
    [
        new()
        {
            Id = PoiTenant.DemoTenantId,
            Name = "Demo POI Tenant",
            Slug = "demo-poi-tenant",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        }
    ];
}
