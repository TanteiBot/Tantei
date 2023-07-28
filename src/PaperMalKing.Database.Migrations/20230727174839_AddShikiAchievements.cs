using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperMalKing.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddShikiAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Achievements",
                table: "ShikiUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Achievements",
                table: "ShikiUsers");
        }
    }
}
