using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game2048Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGameStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGameStats",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    HighScore = table.Column<int>(type: "INTEGER", nullable: false),
                    GamesPlayed = table.Column<int>(type: "INTEGER", nullable: false),
                    HighestTile = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGameStats", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGameStats");
        }
    }
}
