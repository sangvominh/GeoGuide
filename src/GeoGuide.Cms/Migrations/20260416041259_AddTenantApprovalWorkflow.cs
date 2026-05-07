using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GeoGuide.Cms.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "approval_status",
                table: "pois",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "poi_tenants",
                columns: new[] { "id", "is_active", "name", "slug", "updated_at" },
                values: new object[] { new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), true, "Vinh Khanh Food Street", "vinh-khanh-food-street", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("458a1a0e-b524-4e95-92f4-684e80ea7b97"),
                column: "approval_status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("5807657e-240d-42cb-b6e8-b46fd35652ce"),
                column: "approval_status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"),
                column: "approval_status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("a76542f8-e34f-45ee-96c9-0e3961c4ca30"),
                column: "approval_status",
                value: 1);

            migrationBuilder.InsertData(
                table: "pois",
                columns: new[] { "id", "approval_status", "audio_url", "category_key", "category_label", "description", "image_url", "is_active", "language_code", "latitude", "longitude", "map_url", "name", "priority", "tenant_id", "trigger_radius_meters", "tts_script", "updated_at" },
                values: new object[,]
                {
                    { new Guid("15fc7351-ee99-461d-8c56-8f7e181b5ee1"), 1, "https://example.com/audio/bo-la-lot-co-lan.mp3", "food-street", "Am thuc", "Gian hang bo la lot va nuong vi than, thich hop cho nhom sinh vien di an toi.", "https://example.com/images/bo-la-lot-co-lan.jpg", true, "vi-VN", 10.760400000000001, 106.6818, "https://maps.google.com/?q=10.7604,106.6818", "Bo La Lot Co Lan", 2, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 40, "Mui bo nuong thoang ra tu bep than giup diem nay tro thanh mot diem nhan tren pho am thuc mo phong.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("31ab5dfa-4db0-4f37-b280-d82d01f5c34c"), 1, "https://example.com/audio/kem-cuon-dem-sai-gon.mp3", "food-street", "Trang mieng", "Gian kem cuon va mon trang mieng phuc vu du khach sau khi tham quan loat diem am thuc.", "https://example.com/images/kem-cuon-dem-sai-gon.jpg", true, "vi-VN", 10.7592, 106.6819, "https://maps.google.com/?q=10.7592,106.6819", "Kem Cuon Dem Sai Gon", 8, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 30, "Diem kem cuon giup hanh trinh am thuc co them phan ket nhe va hop voi khach tre.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("347dca58-717a-44cc-a470-0d12d2f039d1"), 1, "https://example.com/audio/oc-vinh-khanh-198.mp3", "food-street", "Am thuc", "Quan oc mo cua toi, phuc vu cac mon oc nuong, hap va xao ngay gan khu ky tuc xa.", "https://example.com/images/oc-vinh-khanh-198.jpg", true, "vi-VN", 10.7599, 106.68129999999999, "https://maps.google.com/?q=10.7599,106.6813", "Oc Vinh Khanh 198", 1, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 45, "Diem dung chan dau tien cua tuyen am thuc, noi bat voi hai san binh dan va khong khi nhon nhip ve dem.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("6aa7b67f-5bf0-4efd-8d20-c4c2648c63d8"), 1, "https://example.com/audio/mi-tron-pho-dem.mp3", "food-street", "Mon an dac san", "Quan mi tron va hoanh thanh la diem ket noi giua khach du lich va sinh vien khu vuc.", "https://example.com/images/mi-tron-pho-dem.jpg", true, "vi-VN", 10.7606, 106.6828, "https://maps.google.com/?q=10.7606,106.6828", "Mi Tron Pho Dem", 10, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 35, "Diem mi tron dem bo sung them mot diem dung chan trong cum poi am thuc gan ky tuc xa.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("99d5b24a-8ef9-48cf-ad63-c4b5107f8b25"), 1, "https://example.com/audio/pha-lau-hem-99.mp3", "food-street", "Mon an dac san", "Hang pha lau mang phong vi duong pho, phu hop de mo phong noi dung am thuc dac trung.", "https://example.com/images/pha-lau-hem-99.jpg", true, "vi-VN", 10.7615, 106.6803, "https://maps.google.com/?q=10.7615,106.6803", "Pha Lau Hem 99", 9, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 40, "Pha lau la mot trong nhung mon an de xay dung cau chuyen audio ve pho am thuc thanh pho.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("a4134f4e-0e41-447d-b791-403743213e0d"), 1, "https://example.com/audio/chao-suon-dem-phu-dinh.mp3", "food-street", "Mon nuoc", "Quan chao suon mo som va ban muon, huong toi khach dia phuong va sinh vien o tro.", "https://example.com/images/chao-suon-dem-phu-dinh.jpg", true, "vi-VN", 10.7613, 106.6812, "https://maps.google.com/?q=10.7613,106.6812", "Chao Suon Dem Phu Dinh", 5, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 35, "Bat chao nong la lua chon thu vi cho nguoi di bo khuya quanh cum am thuc mo phong.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("ba80c3f5-fbbf-4586-b70c-55f70b2787de"), 1, "https://example.com/audio/nuong-da-toi-24h.mp3", "food-street", "Do nuong", "Xe nuong dem voi cac mon thit xien, bach tuoc va rau cu nuong da toi.", "https://example.com/images/nuong-da-toi-24h.jpg", true, "vi-VN", 10.759499999999999, 106.68040000000001, "https://maps.google.com/?q=10.7595,106.6804", "Nuong Da Toi 24h", 4, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 30, "Quan nuong da toi giai lap khong khi cho dem, phu hop cho ban do tham quan am thuc ve toi.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("c2bfa8d1-331d-4a6d-809f-01c1b248699c"), 1, "https://example.com/audio/bun-thai-hai-san-co-may.mp3", "food-street", "Mon nuoc", "To bun thai vi chua cay la diem dung pho bien cua tuyen tham quan am thuc.", "https://example.com/images/bun-thai-hai-san-co-may.jpg", true, "vi-VN", 10.760999999999999, 106.68219999999999, "https://maps.google.com/?q=10.7610,106.6822", "Bun Thai Hai San Co May", 7, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 45, "Mon bun thai lam phong phu them trai nghiem noi dung audio cho tuyen am thuc theo chu de.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("daeb6942-24d9-4d7e-89ec-b4836afefdae"), 1, "https://example.com/audio/banh-trang-nuong-ktx.mp3", "food-street", "An vat", "Xe banh trang nuong phong cach Da Lat dat ngay lo vao khu tro sinh vien.", "https://example.com/images/banh-trang-nuong-ktx.jpg", true, "vi-VN", 10.7601, 106.6799, "https://maps.google.com/?q=10.7601,106.6799", "Banh Trang Nuong KTX", 6, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 25, "Banh trang nuong giai lap diem an vat rong rai duoc gioi tre ua chuong quanh ky tuc xa.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("f2ff8acf-f2ab-43b5-b967-1ac32b8c9a13"), 1, "https://example.com/audio/tra-chanh-sinh-vien-99.mp3", "food-street", "An vat", "Quan nuoc va an vat nham toi uu cho luong khach tre quanh ky tuc xa.", "https://example.com/images/tra-chanh-sinh-vien-99.jpg", true, "vi-VN", 10.7608, 106.6808, "https://maps.google.com/?q=10.7608,106.6808", "Tra Chanh Sinh Vien 99", 3, new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"), 35, "Khu tra chanh va an vat la diem hen pho bien cho sinh vien sau gio hoc chieu.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("15fc7351-ee99-461d-8c56-8f7e181b5ee1"));

            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("31ab5dfa-4db0-4f37-b280-d82d01f5c34c"));

            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("347dca58-717a-44cc-a470-0d12d2f039d1"));

            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("6aa7b67f-5bf0-4efd-8d20-c4c2648c63d8"));

            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("99d5b24a-8ef9-48cf-ad63-c4b5107f8b25"));

            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("a4134f4e-0e41-447d-b791-403743213e0d"));

            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("ba80c3f5-fbbf-4586-b70c-55f70b2787de"));

            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("c2bfa8d1-331d-4a6d-809f-01c1b248699c"));

            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("daeb6942-24d9-4d7e-89ec-b4836afefdae"));

            migrationBuilder.DeleteData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("f2ff8acf-f2ab-43b5-b967-1ac32b8c9a13"));

            migrationBuilder.DeleteData(
                table: "poi_tenants",
                keyColumn: "id",
                keyValue: new Guid("f57c20b5-e865-40fb-bdbe-6fbf1a6fe6d0"));

            migrationBuilder.DropColumn(
                name: "approval_status",
                table: "pois");
        }
    }
}
