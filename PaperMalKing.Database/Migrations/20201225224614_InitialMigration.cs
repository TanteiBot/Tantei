#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using Microsoft.EntityFrameworkCore.Migrations;

namespace PaperMalKing.Database.Migrations
{
    public partial class InitialMigration : Migration
    {
		#pragma warning disable MA0051
        protected override void Up(MigrationBuilder migrationBuilder)
		#pragma warning restore MA0051
		{
            migrationBuilder.CreateTable(
                name: "BotUsers",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotUsers", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "DiscordGuilds",
                columns: table => new
                {
                    DiscordGuildId = table.Column<long>(type: "INTEGER", nullable: false),
                    PostingChannelId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGuilds", x => x.DiscordGuildId);
                });

            migrationBuilder.CreateTable(
                name: "DiscordUsers",
                columns: table => new
                {
                    DiscordUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BotUserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUsers", x => x.DiscordUserId);
                    table.ForeignKey(
                        name: "FK_DiscordUsers_BotUsers_BotUserId",
                        column: x => x.BotUserId,
                        principalTable: "BotUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordGuildDiscordUser",
                columns: table => new
                {
                    GuildsDiscordGuildId = table.Column<long>(type: "INTEGER", nullable: false),
                    UsersDiscordUserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGuildDiscordUser", x => new { x.GuildsDiscordGuildId, x.UsersDiscordUserId });
                    table.ForeignKey(
                        name: "FK_DiscordGuildDiscordUser_DiscordGuilds_GuildsDiscordGuildId",
                        column: x => x.GuildsDiscordGuildId,
                        principalTable: "DiscordGuilds",
                        principalColumn: "DiscordGuildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordGuildDiscordUser_DiscordUsers_UsersDiscordUserId",
                        column: x => x.UsersDiscordUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "DiscordUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MalUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscordUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    LastUpdatedAnimeListTimestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    LastUpdatedMangaListTimestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    LastAnimeUpdateHash = table.Column<string>(type: "TEXT", nullable: false),
                    LastMangaUpdateHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalUsers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_MalUsers_DiscordUsers_DiscordUserId",
                        column: x => x.DiscordUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "DiscordUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MalFavoriteAnimes",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    StartYear = table.Column<int>(type: "INTEGER", nullable: false)
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
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
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
                name: "MalFavoriteMangas",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    StartYear = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "MalFavoritePersons",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NameUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalFavoritePersons", x => new { x.Id, x.UserId });
                    table.ForeignKey(
                        name: "FK_MalFavoritePersons_MalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MalUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordGuildDiscordUser_UsersDiscordUserId",
                table: "DiscordGuildDiscordUser",
                column: "UsersDiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_BotUserId",
                table: "DiscordUsers",
                column: "BotUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MalFavoriteAnimes_UserId",
                table: "MalFavoriteAnimes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MalFavoriteCharacters_UserId",
                table: "MalFavoriteCharacters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MalFavoriteMangas_UserId",
                table: "MalFavoriteMangas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MalFavoritePersons_UserId",
                table: "MalFavoritePersons",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MalUsers_DiscordUserId",
                table: "MalUsers",
                column: "DiscordUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordGuildDiscordUser");

            migrationBuilder.DropTable(
                name: "MalFavoriteAnimes");

            migrationBuilder.DropTable(
                name: "MalFavoriteCharacters");

            migrationBuilder.DropTable(
                name: "MalFavoriteMangas");

            migrationBuilder.DropTable(
                name: "MalFavoritePersons");

            migrationBuilder.DropTable(
                name: "DiscordGuilds");

            migrationBuilder.DropTable(
                name: "MalUsers");

            migrationBuilder.DropTable(
                name: "DiscordUsers");

            migrationBuilder.DropTable(
                name: "BotUsers");
        }
    }
}
