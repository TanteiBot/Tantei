using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaperMalKing.Database.Migrations
{
    /// <inheritdoc />
    public partial class MoveMalFavoritesToSingleTableAddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MalFavoritePersons_MalUsers_UserId",
                table: "MalFavoritePersons");

            migrationBuilder.DropTable(
                name: "MalFavoriteAnimes");

            migrationBuilder.DropTable(
                name: "MalFavoriteCharacters");

            migrationBuilder.DropTable(
                name: "MalFavoriteCompanies");

            migrationBuilder.DropTable(
                name: "MalFavoriteMangas");

            migrationBuilder.DropIndex(
                name: "IX_ShikiUsers_DiscordUserId",
                table: "ShikiUsers");

            migrationBuilder.DropIndex(
                name: "IX_MalUsers_DiscordUserId",
                table: "MalUsers");

            migrationBuilder.DropIndex(
                name: "IX_AniListUsers_DiscordUserId",
                table: "AniListUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MalFavoritePersons",
                table: "MalFavoritePersons");

            migrationBuilder.RenameTable(
                name: "MalFavoritePersons",
                newName: "MalFavorites");

            migrationBuilder.RenameIndex(
                name: "IX_MalFavoritePersons_UserId",
                table: "MalFavorites",
                newName: "IX_MalFavorites_UserId");

            migrationBuilder.AddColumn<byte>(
                name: "FavoriteType",
                table: "MalFavorites",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "FromTitleName",
                table: "MalFavorites",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<ushort>(
                name: "StartYear",
                table: "MalFavorites",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "MalFavorites",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MalFavorites",
                table: "MalFavorites",
                columns: new[] { "Id", "UserId", "FavoriteType" });

            migrationBuilder.CreateIndex(
                name: "IX_ShikiUsers_DiscordUserId",
                table: "ShikiUsers",
                column: "DiscordUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShikiUsers_Features",
                table: "ShikiUsers",
                column: "Features");

            migrationBuilder.CreateIndex(
                name: "IX_MalUsers_DiscordUserId",
                table: "MalUsers",
                column: "DiscordUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MalUsers_Features",
                table: "MalUsers",
                column: "Features");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_DiscordUserId",
                table: "DiscordUsers",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordGuilds_DiscordGuildId",
                table: "DiscordGuilds",
                column: "DiscordGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_AniListUsers_DiscordUserId",
                table: "AniListUsers",
                column: "DiscordUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AniListUsers_Features",
                table: "AniListUsers",
                column: "Features");

            migrationBuilder.CreateIndex(
                name: "IX_MalFavorites_FavoriteType",
                table: "MalFavorites",
                column: "FavoriteType");

            migrationBuilder.CreateIndex(
                name: "IX_MalFavorites_Id",
                table: "MalFavorites",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MalFavorites_MalUsers_UserId",
                table: "MalFavorites",
                column: "UserId",
                principalTable: "MalUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("DELETE FROM MalFavorites");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MalFavorites_MalUsers_UserId",
                table: "MalFavorites");

            migrationBuilder.DropIndex(
                name: "IX_ShikiUsers_DiscordUserId",
                table: "ShikiUsers");

            migrationBuilder.DropIndex(
                name: "IX_ShikiUsers_Features",
                table: "ShikiUsers");

            migrationBuilder.DropIndex(
                name: "IX_MalUsers_DiscordUserId",
                table: "MalUsers");

            migrationBuilder.DropIndex(
                name: "IX_MalUsers_Features",
                table: "MalUsers");

            migrationBuilder.DropIndex(
                name: "IX_DiscordUsers_DiscordUserId",
                table: "DiscordUsers");

            migrationBuilder.DropIndex(
                name: "IX_DiscordGuilds_DiscordGuildId",
                table: "DiscordGuilds");

            migrationBuilder.DropIndex(
                name: "IX_AniListUsers_DiscordUserId",
                table: "AniListUsers");

            migrationBuilder.DropIndex(
                name: "IX_AniListUsers_Features",
                table: "AniListUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MalFavorites",
                table: "MalFavorites");

            migrationBuilder.DropIndex(
                name: "IX_MalFavorites_FavoriteType",
                table: "MalFavorites");

            migrationBuilder.DropIndex(
                name: "IX_MalFavorites_Id",
                table: "MalFavorites");

            migrationBuilder.DropColumn(
                name: "FavoriteType",
                table: "MalFavorites");

            migrationBuilder.DropColumn(
                name: "FromTitleName",
                table: "MalFavorites");

            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "MalFavorites");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "MalFavorites");

            migrationBuilder.RenameTable(
                name: "MalFavorites",
                newName: "MalFavoritePersons");

            migrationBuilder.RenameIndex(
                name: "IX_MalFavorites_UserId",
                table: "MalFavoritePersons",
                newName: "IX_MalFavoritePersons_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MalFavoritePersons",
                table: "MalFavoritePersons",
                columns: new[] { "Id", "UserId" });

            migrationBuilder.CreateTable(
                name: "MalFavoriteAnimes",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false),
                    UserId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameUrl = table.Column<string>(type: "TEXT", nullable: false),
                    StartYear = table.Column<uint>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalFavoriteAnimes", x => new { x.Id, x.UserId });
                    table.ForeignKey(
                        name: "FK_MalFavoriteAnimes_MalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MalUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MalFavoriteCharacters",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false),
                    UserId = table.Column<uint>(type: "INTEGER", nullable: false),
                    FromTitleName = table.Column<string>(type: "TEXT", nullable: false),
                    FromTitleUrl = table.Column<string>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalFavoriteCharacters", x => new { x.Id, x.UserId });
                    table.ForeignKey(
                        name: "FK_MalFavoriteCharacters_MalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MalUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MalFavoriteCompanies",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false),
                    UserId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalFavoriteCompanies", x => new { x.Id, x.UserId });
                    table.ForeignKey(
                        name: "FK_MalFavoriteCompanies_MalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MalUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MalFavoriteMangas",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false),
                    UserId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameUrl = table.Column<string>(type: "TEXT", nullable: false),
                    StartYear = table.Column<uint>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalFavoriteMangas", x => new { x.Id, x.UserId });
                    table.ForeignKey(
                        name: "FK_MalFavoriteMangas_MalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MalUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShikiUsers_DiscordUserId",
                table: "ShikiUsers",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MalUsers_DiscordUserId",
                table: "MalUsers",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AniListUsers_DiscordUserId",
                table: "AniListUsers",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MalFavoriteAnimes_UserId",
                table: "MalFavoriteAnimes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MalFavoriteCharacters_UserId",
                table: "MalFavoriteCharacters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MalFavoriteCompanies_UserId",
                table: "MalFavoriteCompanies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MalFavoriteMangas_UserId",
                table: "MalFavoriteMangas",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MalFavoritePersons_MalUsers_UserId",
                table: "MalFavoritePersons",
                column: "UserId",
                principalTable: "MalUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
