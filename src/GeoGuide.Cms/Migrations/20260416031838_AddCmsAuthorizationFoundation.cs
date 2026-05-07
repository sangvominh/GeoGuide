using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GeoGuide.Cms.Migrations
{
    /// <inheritdoc />
    public partial class AddCmsAuthorizationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "pois",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cms_roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cms_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "poi_tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poi_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cms_role_claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cms_role_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cms_role_claims_cms_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "cms_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cms_users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cms_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cms_users_poi_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "poi_tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cms_user_claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cms_user_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cms_user_claims_cms_users_UserId",
                        column: x => x.UserId,
                        principalTable: "cms_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cms_user_logins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cms_user_logins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_cms_user_logins_cms_users_UserId",
                        column: x => x.UserId,
                        principalTable: "cms_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cms_user_roles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cms_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_cms_user_roles_cms_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "cms_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cms_user_roles_cms_users_UserId",
                        column: x => x.UserId,
                        principalTable: "cms_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cms_user_tokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cms_user_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_cms_user_tokens_cms_users_UserId",
                        column: x => x.UserId,
                        principalTable: "cms_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "poi_tenants",
                columns: new[] { "id", "is_active", "name", "slug", "updated_at" },
                values: new object[] { new Guid("6eb3f509-c1a5-4d8d-9bb4-e307db6d0a75"), true, "Demo POI Tenant", "demo-poi-tenant", new DateTimeOffset(new DateTime(2026, 4, 9, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("458a1a0e-b524-4e95-92f4-684e80ea7b97"),
                column: "tenant_id",
                value: new Guid("6eb3f509-c1a5-4d8d-9bb4-e307db6d0a75"));

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("5807657e-240d-42cb-b6e8-b46fd35652ce"),
                column: "tenant_id",
                value: new Guid("6eb3f509-c1a5-4d8d-9bb4-e307db6d0a75"));

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01"),
                column: "tenant_id",
                value: new Guid("6eb3f509-c1a5-4d8d-9bb4-e307db6d0a75"));

            migrationBuilder.UpdateData(
                table: "pois",
                keyColumn: "id",
                keyValue: new Guid("a76542f8-e34f-45ee-96c9-0e3961c4ca30"),
                column: "tenant_id",
                value: new Guid("6eb3f509-c1a5-4d8d-9bb4-e307db6d0a75"));

            migrationBuilder.CreateIndex(
                name: "IX_pois_tenant_id",
                table: "pois",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_cms_role_claims_RoleId",
                table: "cms_role_claims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "cms_roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cms_user_claims_UserId",
                table: "cms_user_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_cms_user_logins_UserId",
                table: "cms_user_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_cms_user_roles_RoleId",
                table: "cms_user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "cms_users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_cms_users_tenant_id",
                table: "cms_users",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "cms_users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_poi_tenants_slug",
                table: "poi_tenants",
                column: "slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_pois_poi_tenants_tenant_id",
                table: "pois",
                column: "tenant_id",
                principalTable: "poi_tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pois_poi_tenants_tenant_id",
                table: "pois");

            migrationBuilder.DropTable(
                name: "cms_role_claims");

            migrationBuilder.DropTable(
                name: "cms_user_claims");

            migrationBuilder.DropTable(
                name: "cms_user_logins");

            migrationBuilder.DropTable(
                name: "cms_user_roles");

            migrationBuilder.DropTable(
                name: "cms_user_tokens");

            migrationBuilder.DropTable(
                name: "cms_roles");

            migrationBuilder.DropTable(
                name: "cms_users");

            migrationBuilder.DropTable(
                name: "poi_tenants");

            migrationBuilder.DropIndex(
                name: "IX_pois_tenant_id",
                table: "pois");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "pois");
        }
    }
}
