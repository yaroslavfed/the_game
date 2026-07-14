using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheGame.Infrastructure.Persistence.Users.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerHighestWave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HighestWave",
                table: "players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "ck_players_highest_wave",
                table: "players",
                sql: "HighestWave >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_players_highest_wave",
                table: "players");

            migrationBuilder.DropColumn(
                name: "HighestWave",
                table: "players");
        }
    }
}
