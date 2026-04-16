using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GeoGuide.Cms.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pois",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    trigger_radius_meters = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    category_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: false),
                    map_url = table.Column<string>(type: "text", nullable: false),
                    audio_url = table.Column<string>(type: "text", nullable: false),
                    tts_script = table.Column<string>(type: "text", nullable: false),
                    language_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pois", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "playback_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    poi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    played_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    trigger_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_playback_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_playback_logs_pois_poi_id",
                        column: x => x.poi_id,
                        principalTable: "pois",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "pois",
                columns: new[] { "id", "audio_url", "category_key", "category_label", "description", "image_url", "is_active", "language_code", "latitude", "longitude", "map_url", "name", "priority", "trigger_radius_meters", "tts_script", "updated_at" },
                values: new object[,]
                {
                    { new Guid("458a1a0e-b524-4e95-92f4-684e80ea7b97"), "https://example.com/audio/notre-dame-cathedral.mp3", "attraction", "Tham quan", "Nha tho Duc Ba Sai Gon voi kien truc Phap tieu bieu.", "https://images.unsplash.com/photo-1583417267826-aebc4d1542e1", true, "vi-VN", 10.7798, 106.699, "https://maps.google.com/?q=10.7798,106.6990", "Notre-Dame Cathedral", 2, 90, "Nha tho Duc Ba la mot diem nhan kien truc va lich su ngay giua trung tam thanh pho.", new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("5807657e-240d-42cb-b6e8-b46fd35652ce"), "https://example.com/audio/tao-dan-park.mp3", "park", "Cong vien", "Cong vien xanh phu hop cho di bo va thu gian.", "https://images.unsplash.com/photo-1506744038136-46273834b3fb", true, "vi-VN", 10.777799999999999, 106.6927, "https://maps.google.com/?q=10.7778,106.6927", "Tao Dan Park", 3, 100, "Cong vien Tao Dan la khoang xanh hien hoi, noi nguoi dan dia phuong thuong tap the duc vao sang som.", new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"), "https://example.com/audio/ben-thanh-market.mp3", "attraction", "Tham quan", "Khu cho noi tieng o trung tam TP.HCM.", "https://images.unsplash.com/photo-1555921015-5532091f6026", true, "vi-VN", 10.772, 106.6983, "https://maps.google.com/?q=10.7720,106.6983", "Ben Thanh Market", 1, 80, "Day la cho Ben Thanh, mot bieu tuong van hoa va du lich cua thanh pho.", new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("a76542f8-e34f-45ee-96c9-0e3961c4ca30"), "https://example.com/audio/hcm-city-museum.mp3", "museum", "Bao tang", "Bao tang gioi thieu lich su va van hoa thanh pho.", "https://images.unsplash.com/photo-1518998053901-5348d3961a04", true, "vi-VN", 10.7765, 106.70099999999999, "https://maps.google.com/?q=10.7765,106.7010", "Ho Chi Minh City Museum", 4, 75, "Bao tang Thanh pho Ho Chi Minh luu giu nhieu tu lieu va hien vat quan trong.", new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_playback_logs_poi_id",
                table: "playback_logs",
                column: "poi_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "playback_logs");

            migrationBuilder.DropTable(
                name: "pois");
        }
    }
}
