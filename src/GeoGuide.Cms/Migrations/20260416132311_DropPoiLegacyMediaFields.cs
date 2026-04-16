using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoGuide.Cms.Migrations
{
    /// <inheritdoc />
    public partial class DropPoiLegacyMediaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "audio_url",
                table: "pois");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "pois");

            migrationBuilder.DropColumn(
                name: "tts_script",
                table: "pois");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "audio_url",
                table: "pois",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "pois",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tts_script",
                table: "pois",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("15fc7351-ee99-461d-8c56-8f7e181b5ee1"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/bo-la-lot-co-lan.mp3", "vi-VN", "Mui bo nuong thoang ra tu bep than giup diem nay tro thanh mot diem nhan tren pho am thuc mo phong." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("31ab5dfa-4db0-4f37-b280-d82d01f5c34c"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/kem-cuon-dem-sai-gon.mp3", "vi-VN", "Diem kem cuon giup hanh trinh am thuc co them phan ket nhe va hop voi khach tre." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("347dca58-717a-44cc-a470-0d12d2f039d1"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/oc-vinh-khanh-198.mp3", "vi-VN", "Diem dung chan dau tien cua tuyen am thuc, noi bat voi hai san binh dan va khong khi nhon nhip ve dem." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("458a1a0e-b524-4e95-92f4-684e80ea7b97"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/notre-dame-cathedral.mp3", "vi-VN", "Nha tho Duc Ba la mot diem nhan kien truc va lich su ngay giua trung tam thanh pho." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("5807657e-240d-42cb-b6e8-b46fd35652ce"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/tao-dan-park.mp3", "vi-VN", "Cong vien Tao Dan la khoang xanh hien hoi, noi nguoi dan dia phuong thuong tap the duc vao sang som." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("6aa7b67f-5bf0-4efd-8d20-c4c2648c63d8"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/mi-tron-pho-dem.mp3", "vi-VN", "Diem mi tron dem bo sung them mot diem dung chan trong cum poi am thuc gan ky tuc xa." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("99d5b24a-8ef9-48cf-ad63-c4b5107f8b25"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/pha-lau-hem-99.mp3", "vi-VN", "Pha lau la mot trong nhung mon an de xay dung cau chuyen audio ve pho am thuc thanh pho." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/ben-thanh-market.mp3", "vi-VN", "Day la cho Ben Thanh, mot bieu tuong van hoa va du lich cua thanh pho." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("a4134f4e-0e41-447d-b791-403743213e0d"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/chao-suon-dem-phu-dinh.mp3", "vi-VN", "Bat chao nong la lua chon thu vi cho nguoi di bo khuya quanh cum am thuc mo phong." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("a76542f8-e34f-45ee-96c9-0e3961c4ca30"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/hcm-city-museum.mp3", "vi-VN", "Bao tang Thanh pho Ho Chi Minh luu giu nhieu tu lieu va hien vat quan trong." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("ba80c3f5-fbbf-4586-b70c-55f70b2787de"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/nuong-da-toi-24h.mp3", "vi-VN", "Quan nuong da toi giai lap khong khi cho dem, phu hop cho ban do tham quan am thuc ve toi." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("c2bfa8d1-331d-4a6d-809f-01c1b248699c"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/bun-thai-hai-san-co-may.mp3", "vi-VN", "Mon bun thai lam phong phu them trai nghiem noi dung audio cho tuyen am thuc theo chu de." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("daeb6942-24d9-4d7e-89ec-b4836afefdae"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/banh-trang-nuong-ktx.mp3", "vi-VN", "Banh trang nuong giai lap diem an vat rong rai duoc gioi tre ua chuong quanh ky tuc xa." });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("f2ff8acf-f2ab-43b5-b967-1ac32b8c9a13"),
                columns: new[] { "audio_url", "language_code", "tts_script" },
                values: new object[] { "https://example.com/audio/tra-chanh-sinh-vien-99.mp3", "vi-VN", "Khu tra chanh va an vat la diem hen pho bien cho sinh vien sau gio hoc chieu." });
        }
    }
}
