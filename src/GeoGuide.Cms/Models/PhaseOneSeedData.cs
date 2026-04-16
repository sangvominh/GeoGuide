namespace GeoGuide.Cms.Models;

public static class PhaseOneSeedData
{
    public static readonly Tour[] Tours =
    [
        new()
        {
            Id = Guid.Parse("a91b4ac0-0b3d-425f-8cd7-6b7f7ec6d501"),
            Name = "Sai Gon Diem Den Trung Tam",
            Description = "Tuyen tham quan nhanh gom cho, nha tho va bao tang trung tam thanh pho.",
            ThumbnailUrl = "https://example.com/images/tour-sai-gon-central.jpg",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z"),
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        },
        new()
        {
            Id = Guid.Parse("d5a4a947-56fc-4671-a10f-d3cad0bdb8a2"),
            Name = "Vinh Khanh Food Walk",
            Description = "Tuyen am thuc dem cho khu Vinh Khanh va cac diem an uong gan ky tuc xa.",
            ThumbnailUrl = "https://example.com/images/tour-vinh-khanh-food-walk.jpg",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z"),
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        }
    ];

    public static readonly TourPoiMapping[] TourPoiMappings =
    [
        new()
        {
            TourId = Guid.Parse("a91b4ac0-0b3d-425f-8cd7-6b7f7ec6d501"),
            PoiId = Guid.Parse("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"),
            OrderIndex = 1
        },
        new()
        {
            TourId = Guid.Parse("a91b4ac0-0b3d-425f-8cd7-6b7f7ec6d501"),
            PoiId = Guid.Parse("458a1a0e-b524-4e95-92f4-684e80ea7b97"),
            OrderIndex = 2
        },
        new()
        {
            TourId = Guid.Parse("a91b4ac0-0b3d-425f-8cd7-6b7f7ec6d501"),
            PoiId = Guid.Parse("a76542f8-e34f-45ee-96c9-0e3961c4ca30"),
            OrderIndex = 3
        },
        new()
        {
            TourId = Guid.Parse("d5a4a947-56fc-4671-a10f-d3cad0bdb8a2"),
            PoiId = Guid.Parse("347dca58-717a-44cc-a470-0d12d2f039d1"),
            OrderIndex = 1
        },
        new()
        {
            TourId = Guid.Parse("d5a4a947-56fc-4671-a10f-d3cad0bdb8a2"),
            PoiId = Guid.Parse("15fc7351-ee99-461d-8c56-8f7e181b5ee1"),
            OrderIndex = 2
        }
    ];

    public static readonly PoiAudio[] PoiAudios =
    [
        new()
        {
            Id = Guid.Parse("0f7fb649-d0ba-4bb7-9c05-d5db40cbdf01"),
            PoiId = Guid.Parse("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"),
            LanguageCode = "vi",
            ContentType = PoiContentType.AudioFile,
            AudioUrl = "https://example.com/audio/ben-thanh-market.mp3",
            TtsContent = null,
            CreatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z"),
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        },
        new()
        {
            Id = Guid.Parse("4ebc7203-0ca9-4312-b8f8-6f49169417e1"),
            PoiId = Guid.Parse("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"),
            LanguageCode = "en",
            ContentType = PoiContentType.TtsScript,
            AudioUrl = null,
            TtsContent = "Ben Thanh Market is one of the city's most recognizable cultural and commercial landmarks.",
            CreatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z"),
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        },
        new()
        {
            Id = Guid.Parse("5b5b6512-88af-4d0b-9f7b-85f3587b3d9e"),
            PoiId = Guid.Parse("458a1a0e-b524-4e95-92f4-684e80ea7b97"),
            LanguageCode = "vi",
            ContentType = PoiContentType.AudioFile,
            AudioUrl = "https://example.com/audio/notre-dame-cathedral.mp3",
            TtsContent = null,
            CreatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z"),
            UpdatedAt = DateTimeOffset.Parse("2026-04-09T10:00:00Z")
        },
        new()
        {
            Id = Guid.Parse("04fb45c2-dc26-476c-bcb9-b9695f0271da"),
            PoiId = Guid.Parse("347dca58-717a-44cc-a470-0d12d2f039d1"),
            LanguageCode = "vi",
            ContentType = PoiContentType.TtsScript,
            AudioUrl = null,
            TtsContent = "Diem dung chan dau tien cua tuyen am thuc, noi bat voi hai san binh dan va khong khi nhon nhip ve dem.",
            CreatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z"),
            UpdatedAt = DateTimeOffset.Parse("2026-04-16T03:30:00Z")
        }
    ];
}
