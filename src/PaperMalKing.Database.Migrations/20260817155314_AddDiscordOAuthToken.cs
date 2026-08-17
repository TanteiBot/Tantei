using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperMalKing.Database.Migrations;

/// <inheritdoc />
public partial class _20260817155314_AddDiscordOAuthToken : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DiscordOAuthTokens",
            columns: table => new
            {
                DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                AccessToken = table.Column<string>(type: "TEXT", nullable: false),
                RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                ExpiresAt = table.Column<long>(type: "INTEGER", nullable: false),
                LastUsedAt = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DiscordOAuthTokens", x => x.DiscordUserId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DiscordOAuthTokens_LastUsedAt",
            table: "DiscordOAuthTokens",
            column: "LastUsedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DiscordOAuthTokens");
    }
}
