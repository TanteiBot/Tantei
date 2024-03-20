using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperMalKing.Database.Migrations
{
    /// <inheritdoc />
    public partial class AniListUpdateColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Colors",
                table: "AniListUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colors",
                table: "AniListUsers");
        }
    }
}
