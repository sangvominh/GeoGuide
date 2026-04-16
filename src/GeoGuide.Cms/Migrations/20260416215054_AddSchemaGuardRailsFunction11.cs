using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoGuide.Cms.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaGuardRailsFunction11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_poi_audios_poi_id",
                table: "poi_audios");

            migrationBuilder.AddCheckConstraint(
                name: "CK_pois_cooldown_minutes_non_negative",
                table: "pois",
                sql: "cooldown_minutes >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_pois_latitude_range",
                table: "pois",
                sql: "latitude >= -90 AND latitude <= 90");

            migrationBuilder.AddCheckConstraint(
                name: "CK_pois_longitude_range",
                table: "pois",
                sql: "longitude >= -180 AND longitude <= 180");

            migrationBuilder.AddCheckConstraint(
                name: "CK_pois_priority_non_negative",
                table: "pois",
                sql: "priority >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_pois_trigger_radius_positive",
                table: "pois",
                sql: "trigger_radius_meters > 0");

            migrationBuilder.CreateIndex(
                name: "IX_poi_audios_poi_id_language_code_content_type_is_deleted",
                table: "poi_audios",
                columns: new[] { "poi_id", "language_code", "content_type", "is_deleted" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_poi_audios_content_payload",
                table: "poi_audios",
                sql: "(content_type = 1 AND audio_url IS NOT NULL) OR (content_type = 2 AND tts_content IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_playback_logs_played_at",
                table: "playback_logs",
                column: "played_at");

            migrationBuilder.AddCheckConstraint(
                name: "CK_playback_logs_duration_non_negative",
                table: "playback_logs",
                sql: "duration_seconds >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_playback_logs_trigger_type",
                table: "playback_logs",
                sql: "trigger_type IN ('gps', 'qr', 'manual')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_pois_cooldown_minutes_non_negative",
                table: "pois");

            migrationBuilder.DropCheckConstraint(
                name: "CK_pois_latitude_range",
                table: "pois");

            migrationBuilder.DropCheckConstraint(
                name: "CK_pois_longitude_range",
                table: "pois");

            migrationBuilder.DropCheckConstraint(
                name: "CK_pois_priority_non_negative",
                table: "pois");

            migrationBuilder.DropCheckConstraint(
                name: "CK_pois_trigger_radius_positive",
                table: "pois");

            migrationBuilder.DropIndex(
                name: "IX_poi_audios_poi_id_language_code_content_type_is_deleted",
                table: "poi_audios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_poi_audios_content_payload",
                table: "poi_audios");

            migrationBuilder.DropIndex(
                name: "IX_playback_logs_played_at",
                table: "playback_logs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_playback_logs_duration_non_negative",
                table: "playback_logs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_playback_logs_trigger_type",
                table: "playback_logs");

            migrationBuilder.CreateIndex(
                name: "IX_poi_audios_poi_id",
                table: "poi_audios",
                column: "poi_id");
        }
    }
}
