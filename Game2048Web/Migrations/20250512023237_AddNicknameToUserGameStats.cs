using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game2048Web.Migrations
{
    /// <inheritdoc />
    public partial class AddNicknameToUserGameStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "UserGameStats",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShowInLeaderboard",
                table: "UserGameStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "UserGameStats");

            migrationBuilder.DropColumn(
                name: "ShowInLeaderboard",
                table: "UserGameStats");
        }
    }
}
