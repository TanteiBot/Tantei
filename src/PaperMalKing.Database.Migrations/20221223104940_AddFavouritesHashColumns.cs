using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperMalKing.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFavouritesHashColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FavouritesIdHash",
                table: "ShikiUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FavoritesIdHash",
                table: "MalUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FavouritesIdHash",
                table: "AniListUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FavouritesIdHash",
                table: "ShikiUsers");

            migrationBuilder.DropColumn(
                name: "FavoritesIdHash",
                table: "MalUsers");

            migrationBuilder.DropColumn(
                name: "FavouritesIdHash",
                table: "AniListUsers");
        }
    }
}
