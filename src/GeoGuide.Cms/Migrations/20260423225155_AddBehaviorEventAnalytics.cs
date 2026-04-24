using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoGuide.Cms.Migrations
{
    /// <inheritdoc />
    public partial class AddBehaviorEventAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "behavior_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    poi_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    session_token = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    client_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_behavior_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_behavior_events_pois_poi_id",
                        column: x => x.poi_id,
                        principalTable: "pois",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_behavior_events_device_id_occurred_at",
                table: "behavior_events",
                columns: new[] { "device_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_behavior_events_poi_id",
                table: "behavior_events",
                column: "poi_id");

            migrationBuilder.CreateIndex(
                name: "IX_behavior_events_session_token",
                table: "behavior_events",
                column: "session_token");

            migrationBuilder.CreateIndex(
                name: "IX_behavior_events_session_token_device_id_occurred_at",
                table: "behavior_events",
                columns: new[] { "session_token", "device_id", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "behavior_events");
        }
    }
}
