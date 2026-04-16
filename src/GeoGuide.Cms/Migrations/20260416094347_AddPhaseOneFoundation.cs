using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GeoGuide.Cms.Migrations
{
    /// <inheritdoc />
    public partial class AddPhaseOneFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cooldown_minutes",
                table: "pois",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "pois",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "pois",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "poi_audios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    poi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    content_type = table.Column<int>(type: "integer", nullable: false),
                    audio_url = table.Column<string>(type: "text", nullable: true),
                    tts_content = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poi_audios", x => x.id);
                    table.ForeignKey(
                        name: "FK_poi_audios_pois_poi_id",
                        column: x => x.poi_id,
                        principalTable: "pois",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tours",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tours", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tour_poi_mappings",
                columns: table => new
                {
                    tour_id = table.Column<Guid>(type: "uuid", nullable: false),
                    poi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_poi_mappings", x => new { x.tour_id, x.poi_id });
                    table.ForeignKey(
                        name: "FK_tour_poi_mappings_pois_poi_id",
                        column: x => x.poi_id,
                        principalTable: "pois",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tour_poi_mappings_tours_tour_id",
                        column: x => x.tour_id,
                        principalTable: "tours",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "poi_audios",
                columns: new[] { "id", "audio_url", "content_type", "created_at", "is_deleted", "language_code", "poi_id", "tts_content", "updated_at" },
                values: new object[,]
                {
                    { new Guid("04fb45c2-dc26-476c-bcb9-b9695f0271da"), null, 2, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "vi", new Guid("347dca58-717a-44cc-a470-0d12d2f039d1"), "Diem dung chan dau tien cua tuyen am thuc, noi bat voi hai san binh dan va khong khi nhon nhip ve dem.", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("0f7fb649-d0ba-4bb7-9c05-d5db40cbdf01"), "https://example.com/audio/ben-thanh-market.mp3", 1, new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "vi", new Guid("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"), null, new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("4ebc7203-0ca9-4312-b8f8-6f49169417e1"), null, 2, new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "en", new Guid("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"), "Ben Thanh Market is one of the city's most recognizable cultural and commercial landmarks.", new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("5b5b6512-88af-4d0b-9f7b-85f3587b3d9e"), "https://example.com/audio/notre-dame-cathedral.mp3", 1, new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "vi", new Guid("458a1a0e-b524-4e95-92f4-684e80ea7b97"), null, new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("15fc7351-ee99-461d-8c56-8f7e181b5ee1"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("31ab5dfa-4db0-4f37-b280-d82d01f5c34c"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("347dca58-717a-44cc-a470-0d12d2f039d1"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("458a1a0e-b524-4e95-92f4-684e80ea7b97"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("5807657e-240d-42cb-b6e8-b46fd35652ce"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("6aa7b67f-5bf0-4efd-8d20-c4c2648c63d8"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("99d5b24a-8ef9-48cf-ad63-c4b5107f8b25"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("a4134f4e-0e41-447d-b791-403743213e0d"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("a76542f8-e34f-45ee-96c9-0e3961c4ca30"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("ba80c3f5-fbbf-4586-b70c-55f70b2787de"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("c2bfa8d1-331d-4a6d-809f-01c1b248699c"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("daeb6942-24d9-4d7e-89ec-b4836afefdae"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("f2ff8acf-f2ab-43b5-b967-1ac32b8c9a13"),
                columns: new[] { "cooldown_minutes", "created_at", "is_deleted" },
                values: new object[] { 15, new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false });

            migrationBuilder.InsertData(
                table: "tours",
                columns: new[] { "id", "created_at", "description", "is_active", "is_deleted", "name", "thumbnail_url", "updated_at" },
                values: new object[,]
                {
                    { new Guid("a91b4ac0-0b3d-425f-8cd7-6b7f7ec6d501"), new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tuyen tham quan nhanh gom cho, nha tho va bao tang trung tam thanh pho.", true, false, "Sai Gon Diem Den Trung Tam", "https://example.com/images/tour-sai-gon-central.jpg", new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("d5a4a947-56fc-4671-a10f-d3cad0bdb8a2"), new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tuyen am thuc dem cho khu Vinh Khanh va cac diem an uong gan ky tuc xa.", true, false, "Vinh Khanh Food Walk", "https://example.com/images/tour-vinh-khanh-food-walk.jpg", new DateTimeOffset(new DateTime(2026, 4, 16, 3, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "tour_poi_mappings",
                columns: new[] { "poi_id", "tour_id", "order_index" },
                values: new object[,]
                {
                    { new Guid("458a1a0e-b524-4e95-92f4-684e80ea7b97"), new Guid("a91b4ac0-0b3d-425f-8cd7-6b7f7ec6d501"), 2 },
                    { new Guid("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"), new Guid("a91b4ac0-0b3d-425f-8cd7-6b7f7ec6d501"), 1 },
                    { new Guid("a76542f8-e34f-45ee-96c9-0e3961c4ca30"), new Guid("a91b4ac0-0b3d-425f-8cd7-6b7f7ec6d501"), 3 },
                    { new Guid("15fc7351-ee99-461d-8c56-8f7e181b5ee1"), new Guid("d5a4a947-56fc-4671-a10f-d3cad0bdb8a2"), 2 },
                    { new Guid("347dca58-717a-44cc-a470-0d12d2f039d1"), new Guid("d5a4a947-56fc-4671-a10f-d3cad0bdb8a2"), 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_pois_latitude_longitude",
                table: "pois",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_poi_audios_poi_id",
                table: "poi_audios",
                column: "poi_id");

            migrationBuilder.CreateIndex(
                name: "IX_tour_poi_mappings_poi_id",
                table: "tour_poi_mappings",
                column: "poi_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "poi_audios");

            migrationBuilder.DropTable(
                name: "tour_poi_mappings");

            migrationBuilder.DropTable(
                name: "tours");

            migrationBuilder.DropIndex(
                name: "IX_pois_latitude_longitude",
                table: "pois");

            migrationBuilder.DropColumn(
                name: "cooldown_minutes",
                table: "pois");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "pois");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "pois");
        }
    }
}
