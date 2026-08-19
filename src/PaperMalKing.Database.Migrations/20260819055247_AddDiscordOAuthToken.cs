using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperMalKing.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscordOAuthToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Colors",
                table: "ShikiUsers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Achievements",
                table: "ShikiUsers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Colors",
                table: "MalUsers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Colors",
                table: "AniListUsers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Colors",
                table: "ShikiUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Achievements",
                table: "ShikiUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Colors",
                table: "MalUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Colors",
                table: "AniListUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
