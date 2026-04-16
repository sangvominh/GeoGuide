namespace GeoGuide.Cms.Models;

public static class PoiSeedData
{
    public static readonly Poi[] All = BuildPois();

    public static readonly PoiTenant[] Tenants =
    [
        new()
        {
            Id = PoiTenant.DemoTenantId,
            Name = "Demo POI Tenant",
            Slug = "demo-poi-tenant",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        },
        new()
        {
            Id = PoiTenant.VinhKhanhTenantId,
            Name = "Vinh Khanh Food Street",
            Slug = "vinh-khanh-food-street",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        }
    ];

    private static Poi[] BuildPois()
    {
        Poi[] pois =
        [
        new()
        {
            Id = Guid.Parse("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"),
            TenantId = PoiTenant.DemoTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
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
            ApprovalStatus = PoiApprovalStatus.Approved,
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
            ApprovalStatus = PoiApprovalStatus.Approved,
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
            ApprovalStatus = PoiApprovalStatus.Approved,
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
        },
        new()
        {
            Id = Guid.Parse("347dca58-717a-44cc-a470-0d12d2f039d1"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Oc Vinh Khanh 198",
            Description = "Quan oc mo cua toi, phuc vu cac mon oc nuong, hap va xao ngay gan khu ky tuc xa.",
            Latitude = 10.7599,
            Longitude = 106.6813,
            TriggerRadiusMeters = 45,
            Priority = 1,
            CategoryKey = "food-street",
            CategoryLabel = "Am thuc",
            ImageUrl = "https://example.com/images/oc-vinh-khanh-198.jpg",
            MapUrl = "https://maps.google.com/?q=10.7599,106.6813",
            AudioUrl = "https://example.com/audio/oc-vinh-khanh-198.mp3",
            TtsScript = "Diem dung chan dau tien cua tuyen am thuc, noi bat voi hai san binh dan va khong khi nhon nhip ve dem.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        },
        new()
        {
            Id = Guid.Parse("15fc7351-ee99-461d-8c56-8f7e181b5ee1"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Bo La Lot Co Lan",
            Description = "Gian hang bo la lot va nuong vi than, thich hop cho nhom sinh vien di an toi.",
            Latitude = 10.7604,
            Longitude = 106.6818,
            TriggerRadiusMeters = 40,
            Priority = 2,
            CategoryKey = "food-street",
            CategoryLabel = "Am thuc",
            ImageUrl = "https://example.com/images/bo-la-lot-co-lan.jpg",
            MapUrl = "https://maps.google.com/?q=10.7604,106.6818",
            AudioUrl = "https://example.com/audio/bo-la-lot-co-lan.mp3",
            TtsScript = "Mui bo nuong thoang ra tu bep than giup diem nay tro thanh mot diem nhan tren pho am thuc mo phong.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        },
        new()
        {
            Id = Guid.Parse("f2ff8acf-f2ab-43b5-b967-1ac32b8c9a13"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Tra Chanh Sinh Vien 99",
            Description = "Quan nuoc va an vat nham toi uu cho luong khach tre quanh ky tuc xa.",
            Latitude = 10.7608,
            Longitude = 106.6808,
            TriggerRadiusMeters = 35,
            Priority = 3,
            CategoryKey = "food-street",
            CategoryLabel = "An vat",
            ImageUrl = "https://example.com/images/tra-chanh-sinh-vien-99.jpg",
            MapUrl = "https://maps.google.com/?q=10.7608,106.6808",
            AudioUrl = "https://example.com/audio/tra-chanh-sinh-vien-99.mp3",
            TtsScript = "Khu tra chanh va an vat la diem hen pho bien cho sinh vien sau gio hoc chieu.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        },
        new()
        {
            Id = Guid.Parse("ba80c3f5-fbbf-4586-b70c-55f70b2787de"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Nuong Da Toi 24h",
            Description = "Xe nuong dem voi cac mon thit xien, bach tuoc va rau cu nuong da toi.",
            Latitude = 10.7595,
            Longitude = 106.6804,
            TriggerRadiusMeters = 30,
            Priority = 4,
            CategoryKey = "food-street",
            CategoryLabel = "Do nuong",
            ImageUrl = "https://example.com/images/nuong-da-toi-24h.jpg",
            MapUrl = "https://maps.google.com/?q=10.7595,106.6804",
            AudioUrl = "https://example.com/audio/nuong-da-toi-24h.mp3",
            TtsScript = "Quan nuong da toi giai lap khong khi cho dem, phu hop cho ban do tham quan am thuc ve toi.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        },
        new()
        {
            Id = Guid.Parse("a4134f4e-0e41-447d-b791-403743213e0d"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Chao Suon Dem Phu Dinh",
            Description = "Quan chao suon mo som va ban muon, huong toi khach dia phuong va sinh vien o tro.",
            Latitude = 10.7613,
            Longitude = 106.6812,
            TriggerRadiusMeters = 35,
            Priority = 5,
            CategoryKey = "food-street",
            CategoryLabel = "Mon nuoc",
            ImageUrl = "https://example.com/images/chao-suon-dem-phu-dinh.jpg",
            MapUrl = "https://maps.google.com/?q=10.7613,106.6812",
            AudioUrl = "https://example.com/audio/chao-suon-dem-phu-dinh.mp3",
            TtsScript = "Bat chao nong la lua chon thu vi cho nguoi di bo khuya quanh cum am thuc mo phong.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        },
        new()
        {
            Id = Guid.Parse("daeb6942-24d9-4d7e-89ec-b4836afefdae"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Banh Trang Nuong KTX",
            Description = "Xe banh trang nuong phong cach Da Lat dat ngay lo vao khu tro sinh vien.",
            Latitude = 10.7601,
            Longitude = 106.6799,
            TriggerRadiusMeters = 25,
            Priority = 6,
            CategoryKey = "food-street",
            CategoryLabel = "An vat",
            ImageUrl = "https://example.com/images/banh-trang-nuong-ktx.jpg",
            MapUrl = "https://maps.google.com/?q=10.7601,106.6799",
            AudioUrl = "https://example.com/audio/banh-trang-nuong-ktx.mp3",
            TtsScript = "Banh trang nuong giai lap diem an vat rong rai duoc gioi tre ua chuong quanh ky tuc xa.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        },
        new()
        {
            Id = Guid.Parse("c2bfa8d1-331d-4a6d-809f-01c1b248699c"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Bun Thai Hai San Co May",
            Description = "To bun thai vi chua cay la diem dung pho bien cua tuyen tham quan am thuc.",
            Latitude = 10.7610,
            Longitude = 106.6822,
            TriggerRadiusMeters = 45,
            Priority = 7,
            CategoryKey = "food-street",
            CategoryLabel = "Mon nuoc",
            ImageUrl = "https://example.com/images/bun-thai-hai-san-co-may.jpg",
            MapUrl = "https://maps.google.com/?q=10.7610,106.6822",
            AudioUrl = "https://example.com/audio/bun-thai-hai-san-co-may.mp3",
            TtsScript = "Mon bun thai lam phong phu them trai nghiem noi dung audio cho tuyen am thuc theo chu de.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        },
        new()
        {
            Id = Guid.Parse("31ab5dfa-4db0-4f37-b280-d82d01f5c34c"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Kem Cuon Dem Sai Gon",
            Description = "Gian kem cuon va mon trang mieng phuc vu du khach sau khi tham quan loat diem am thuc.",
            Latitude = 10.7592,
            Longitude = 106.6819,
            TriggerRadiusMeters = 30,
            Priority = 8,
            CategoryKey = "food-street",
            CategoryLabel = "Trang mieng",
            ImageUrl = "https://example.com/images/kem-cuon-dem-sai-gon.jpg",
            MapUrl = "https://maps.google.com/?q=10.7592,106.6819",
            AudioUrl = "https://example.com/audio/kem-cuon-dem-sai-gon.mp3",
            TtsScript = "Diem kem cuon giup hanh trinh am thuc co them phan ket nhe va hop voi khach tre.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        },
        new()
        {
            Id = Guid.Parse("99d5b24a-8ef9-48cf-ad63-c4b5107f8b25"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Pha Lau Hem 99",
            Description = "Hang pha lau mang phong vi duong pho, phu hop de mo phong noi dung am thuc dac trung.",
            Latitude = 10.7615,
            Longitude = 106.6803,
            TriggerRadiusMeters = 40,
            Priority = 9,
            CategoryKey = "food-street",
            CategoryLabel = "Mon an dac san",
            ImageUrl = "https://example.com/images/pha-lau-hem-99.jpg",
            MapUrl = "https://maps.google.com/?q=10.7615,106.6803",
            AudioUrl = "https://example.com/audio/pha-lau-hem-99.mp3",
            TtsScript = "Pha lau la mot trong nhung mon an de xay dung cau chuyen audio ve pho am thuc thanh pho.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        },
        new()
        {
            Id = Guid.Parse("6aa7b67f-5bf0-4efd-8d20-c4c2648c63d8"),
            TenantId = PoiTenant.VinhKhanhTenantId,
            ApprovalStatus = PoiApprovalStatus.Approved,
            Name = "Mi Tron Pho Dem",
            Description = "Quan mi tron va hoanh thanh la diem ket noi giua khach du lich va sinh vien khu vuc.",
            Latitude = 10.7606,
            Longitude = 106.6828,
            TriggerRadiusMeters = 35,
            Priority = 10,
            CategoryKey = "food-street",
            CategoryLabel = "Mon an dac san",
            ImageUrl = "https://example.com/images/mi-tron-pho-dem.jpg",
            MapUrl = "https://maps.google.com/?q=10.7606,106.6828",
            AudioUrl = "https://example.com/audio/mi-tron-pho-dem.mp3",
            TtsScript = "Diem mi tron dem bo sung them mot diem dung chan trong cum poi am thuc gan ky tuc xa.",
            LanguageCode = "vi-VN",
            IsActive = true,
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        }
        ];

        foreach (var poi in pois)
        {
            poi.CreatedAt = poi.UpdatedAt;
            if (poi.CooldownMinutes == 0)
            {
                poi.CooldownMinutes = 15;
            }
        }

        return pois;
    }
}
