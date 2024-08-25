using System;
using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SharedGameFramework.Game.Armoury;
using SharedGameFramework.Game.Character;
using SharedGameFramework.Game.Kingdom.Map;

#nullable disable

namespace MM_API.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "t_armoury");

            migrationBuilder.DropTable(
                name: "t_session");

            migrationBuilder.DropTable(
                name: "t_soupkitchen");

            migrationBuilder.DropTable(
                name: "t_treasury");

            migrationBuilder.DropTable(
                name: "t_character");

            migrationBuilder.DropTable(
                name: "t_kingdom");

            migrationBuilder.DropTable(
                name: "t_user");
        }
    }
}
