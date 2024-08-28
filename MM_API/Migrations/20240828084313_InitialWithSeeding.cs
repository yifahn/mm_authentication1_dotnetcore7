using System;
using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SharedGameFramework.Game.Armoury;
using SharedGameFramework.Game.Character;
using SharedGameFramework.Game.Kingdom.Map;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MM_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "t_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_user", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
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
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
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
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_kingdom",
                columns: table => new
                {
                    kingdom_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_user_id = table.Column<int>(type: "integer", nullable: false),
                    kingdom_name = table.Column<string>(type: "text", nullable: false),
                    kingdom_map = table.Column<Map>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_kingdom", x => x.kingdom_id);
                    table.ForeignKey(
                        name: "FK_t_kingdom_t_user_fk_user_id",
                        column: x => x.fk_user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_session",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_user_id = table.Column<int>(type: "integer", nullable: false),
                    session_authtoken = table.Column<string>(type: "text", nullable: false),
                    session_refreshtoken = table.Column<string>(type: "text", nullable: false),
                    session_loggedin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    session_loggedout = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_session", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_t_session_t_user_fk_user_id",
                        column: x => x.fk_user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_armoury",
                columns: table => new
                {
                    armoury_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_kingdom_id = table.Column<int>(type: "integer", nullable: false),
                    armoury_inventory = table.Column<Armoury_Inventory>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_armoury", x => x.armoury_id);
                    table.ForeignKey(
                        name: "FK_t_armoury_t_kingdom_fk_kingdom_id",
                        column: x => x.fk_kingdom_id,
                        principalTable: "t_kingdom",
                        principalColumn: "kingdom_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_character",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_kingdom_id = table.Column<int>(type: "integer", nullable: false),
                    political_points = table.Column<int>(type: "integer", nullable: false),
                    character_name = table.Column<string>(type: "text", nullable: false),
                    character_inventory = table.Column<Character_Inventory>(type: "jsonb", nullable: false),
                    character_sheet = table.Column<CharacterSheet>(type: "jsonb", nullable: false),
                    character_state = table.Column<CharacterState>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_character", x => x.character_id);
                    table.ForeignKey(
                        name: "FK_t_character_t_kingdom_fk_kingdom_id",
                        column: x => x.fk_kingdom_id,
                        principalTable: "t_kingdom",
                        principalColumn: "kingdom_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_treasury",
                columns: table => new
                {
                    treasury_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_kingdom_id = table.Column<int>(type: "integer", nullable: false),
                    treasury_coin = table.Column<BigInteger>(type: "numeric", nullable: false),
                    treasury_gainrate = table.Column<double>(type: "double precision", nullable: false),
                    treasury_multiplier = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_treasury", x => x.treasury_id);
                    table.ForeignKey(
                        name: "FK_t_treasury_t_kingdom_fk_kingdom_id",
                        column: x => x.fk_kingdom_id,
                        principalTable: "t_kingdom",
                        principalColumn: "kingdom_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_soupkitchen",
                columns: table => new
                {
                    soupkitchen_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fk_character_id = table.Column<int>(type: "integer", nullable: false),
                    soupkitchen_cooldown_days = table.Column<int>(type: "integer", nullable: false),
                    soupkitchen_cooldown_hours = table.Column<int>(type: "integer", nullable: false),
                    soupkitchen_cooldown_minutes = table.Column<int>(type: "integer", nullable: false),
                    soupkitchen_cooldown_seconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_soupkitchen", x => x.soupkitchen_id);
                    table.ForeignKey(
                        name: "FK_t_soupkitchen_t_character_fk_character_id",
                        column: x => x.fk_character_id,
                        principalTable: "t_character",
                        principalColumn: "character_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "520d6e0e-235c-47c2-a1d8-5078f7b3fa43", null, "User", "USER" },
                    { "5de48f8c-0a70-4e21-9cc0-798ff818fdc3", null, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "5de48f8c-0a70-4e21-9cc0-798ff818fdc3", 0, "2284196f-07bd-43fa-aeeb-99912ed9a224", "yifahn@gmail.com", false, false, null, "YIFAHN@GMAIL.COM", "YIFAHNADMIN", "AQAAAAIAAYagAAAAEAAwtrT+Pnwp3z/4oHtJ+3Ryl2M8YRIpETsVvekDhtxGFKXrzLPZtjD5z6UIaVSR3Q==", null, false, "251232a3-7f8d-4e9f-9467-a65d67100070", false, "yifahnadmin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "520d6e0e-235c-47c2-a1d8-5078f7b3fa43", "5de48f8c-0a70-4e21-9cc0-798ff818fdc3" },
                    { "5de48f8c-0a70-4e21-9cc0-798ff818fdc3", "5de48f8c-0a70-4e21-9cc0-798ff818fdc3" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_armoury_fk_kingdom_id",
                table: "t_armoury",
                column: "fk_kingdom_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_character_fk_kingdom_id",
                table: "t_character",
                column: "fk_kingdom_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_kingdom_fk_user_id",
                table: "t_kingdom",
                column: "fk_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_session_fk_user_id",
                table: "t_session",
                column: "fk_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_soupkitchen_fk_character_id",
                table: "t_soupkitchen",
                column: "fk_character_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_treasury_fk_kingdom_id",
                table: "t_treasury",
                column: "fk_kingdom_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "t_armoury");

            migrationBuilder.DropTable(
                name: "t_session");

            migrationBuilder.DropTable(
                name: "t_soupkitchen");

            migrationBuilder.DropTable(
                name: "t_treasury");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "t_character");

            migrationBuilder.DropTable(
                name: "t_kingdom");

            migrationBuilder.DropTable(
                name: "t_user");
        }
    }
}
