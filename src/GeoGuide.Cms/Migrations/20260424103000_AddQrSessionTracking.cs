using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoGuide.Cms.Migrations;

public partial class AddQrSessionTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "client_type",
            table: "playback_logs",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "session_token",
            table: "playback_logs",
            type: "character varying(120)",
            maxLength: 120,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "device_session_joins",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                session_token = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                client_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                access_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_device_session_joins", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_device_session_joins_session_token",
            table: "device_session_joins",
            column: "session_token");

        migrationBuilder.CreateIndex(
            name: "IX_device_session_joins_session_token_device_id",
            table: "device_session_joins",
            columns: new[] { "session_token", "device_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_playback_logs_session_token",
            table: "playback_logs",
            column: "session_token");

        migrationBuilder.CreateIndex(
            name: "IX_playback_logs_session_token_device_id",
            table: "playback_logs",
            columns: new[] { "session_token", "device_id" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "device_session_joins");

        migrationBuilder.DropIndex(
            name: "IX_playback_logs_session_token",
            table: "playback_logs");

        migrationBuilder.DropIndex(
            name: "IX_playback_logs_session_token_device_id",
            table: "playback_logs");

        migrationBuilder.DropColumn(
            name: "client_type",
            table: "playback_logs");

        migrationBuilder.DropColumn(
            name: "session_token",
            table: "playback_logs");
    }
}
