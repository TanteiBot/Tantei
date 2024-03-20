using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperMalKing.Database.Migrations
{
    /// <inheritdoc />
    public partial class MalUpdateColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Colors",
                table: "MalUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colors",
                table: "MalUsers");
        }
    }
}
