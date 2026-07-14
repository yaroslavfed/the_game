using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGame.Infrastructure.Persistence.Users.Migrations
{
    /// <inheritdoc />
    public partial class InitialUserSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Login = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NormalizedLogin = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "BLOB", nullable: false),
                    HashIterations = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.CheckConstraint("ck_accounts_hash_iterations", "HashIterations > 0");
                });

            migrationBuilder.CreateTable(
                name: "data_migrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_migrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    AccountId = table.Column<string>(type: "TEXT", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Experience = table.Column<int>(type: "INTEGER", nullable: false),
                    Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Revision = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.Id);
                    table.CheckConstraint("ck_players_experience", "experience >= 0");
                    table.CheckConstraint("ck_players_level", "level >= 1");
                    table.CheckConstraint("ck_players_money", "money >= 0");
                    table.CheckConstraint("ck_players_revision", "revision >= 0");
                    table.ForeignKey(
                        name: "FK_players_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_items",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    ItemId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    AcquiredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_items", x => new { x.PlayerId, x.ItemId });
                    table.CheckConstraint("ck_player_items_kind", "Kind IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_player_items_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_loadouts",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    WeaponItemId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ArmorItemId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_loadouts", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_player_loadouts_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_NormalizedLogin",
                table: "accounts",
                column: "NormalizedLogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_AccountId",
                table: "players",
                column: "AccountId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_migrations");

            migrationBuilder.DropTable(
                name: "player_items");

            migrationBuilder.DropTable(
                name: "player_loadouts");

            migrationBuilder.DropTable(
                name: "players");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
