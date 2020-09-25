using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaperMalKing.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordGuilds",
                columns: table => new
                {
                    DiscordGuildId = table.Column<long>(nullable: false),
                    PostingChannelId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGuilds", x => x.DiscordGuildId);
                });

            migrationBuilder.CreateTable(
                name: "DiscordUsers",
                columns: table => new
                {
                    DiscordUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUsers", x => x.DiscordUserId);
                });

            migrationBuilder.CreateTable(
                name: "MyAnimeListUsers",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    DiscordUserId = table.Column<long>(nullable: false),
                    Username = table.Column<string>(nullable: false),
                    LastUpdatedAnimeListTimestamp = table.Column<DateTime>(nullable: false),
                    LastUpdatedMangaListTimestamp = table.Column<DateTime>(nullable: false),
                    FeaturesEnabled = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyAnimeListUsers", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "DiscordGuildUser",
                columns: table => new
                {
                    DiscordUserId = table.Column<long>(nullable: false),
                    DiscordGuildId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGuildUser", x => new { x.DiscordGuildId, x.DiscordUserId });
                    table.ForeignKey(
                        name: "FK_DiscordGuildUser_DiscordGuilds_DiscordGuildId",
                        column: x => x.DiscordGuildId,
                        principalTable: "DiscordGuilds",
                        principalColumn: "DiscordGuildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordGuildUser_DiscordUsers_DiscordUserId",
                        column: x => x.DiscordUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "DiscordUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MyAnimeListUserAnimeListColors",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    WatchingColor = table.Column<int>(nullable: false),
                    CompletedColor = table.Column<int>(nullable: false),
                    OnHoldColor = table.Column<int>(nullable: false),
                    DroppedColor = table.Column<int>(nullable: false),
                    PlanToWatchColor = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyAnimeListUserAnimeListColors", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_MyAnimeListUserAnimeListColors_MyAnimeListUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MyAnimeListUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MyAnimeListUserFavoriteAnimes",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    StartYear = table.Column<int>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    MALUserUserId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyAnimeListUserFavoriteAnimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyAnimeListUserFavoriteAnimes_MyAnimeListUsers_MALUserUserId",
                        column: x => x.MALUserUserId,
                        principalTable: "MyAnimeListUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MyAnimeListUserFavoriteCharacters",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: false),
                    FromEntryName = table.Column<string>(nullable: false),
                    FromEntryUrl = table.Column<string>(nullable: false),
                    MALUserUserId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyAnimeListUserFavoriteCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyAnimeListUserFavoriteCharacters_MyAnimeListUsers_MALUserUserId",
                        column: x => x.MALUserUserId,
                        principalTable: "MyAnimeListUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MyAnimeListUserFavoriteMangas",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    StartYear = table.Column<int>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    MALUserUserId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyAnimeListUserFavoriteMangas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyAnimeListUserFavoriteMangas_MyAnimeListUsers_MALUserUserId",
                        column: x => x.MALUserUserId,
                        principalTable: "MyAnimeListUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MyAnimeListUserFavoritePersons",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    MALUserUserId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyAnimeListUserFavoritePersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyAnimeListUserFavoritePersons_MyAnimeListUsers_MALUserUserId",
                        column: x => x.MALUserUserId,
                        principalTable: "MyAnimeListUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MyAnimeListUserMangaListColors",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    ReadingColor = table.Column<int>(nullable: false),
                    CompletedColor = table.Column<int>(nullable: false),
                    OnHoldColor = table.Column<int>(nullable: false),
                    DroppedColor = table.Column<int>(nullable: false),
                    PlanToReadColor = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyAnimeListUserMangaListColors", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_MyAnimeListUserMangaListColors_MyAnimeListUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MyAnimeListUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordGuildUser_DiscordUserId",
                table: "DiscordGuildUser",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MyAnimeListUserFavoriteAnimes_MALUserUserId",
                table: "MyAnimeListUserFavoriteAnimes",
                column: "MALUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MyAnimeListUserFavoriteCharacters_MALUserUserId",
                table: "MyAnimeListUserFavoriteCharacters",
                column: "MALUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MyAnimeListUserFavoriteMangas_MALUserUserId",
                table: "MyAnimeListUserFavoriteMangas",
                column: "MALUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MyAnimeListUserFavoritePersons_MALUserUserId",
                table: "MyAnimeListUserFavoritePersons",
                column: "MALUserUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordGuildUser");

            migrationBuilder.DropTable(
                name: "MyAnimeListUserAnimeListColors");

            migrationBuilder.DropTable(
                name: "MyAnimeListUserFavoriteAnimes");

            migrationBuilder.DropTable(
                name: "MyAnimeListUserFavoriteCharacters");

            migrationBuilder.DropTable(
                name: "MyAnimeListUserFavoriteMangas");

            migrationBuilder.DropTable(
                name: "MyAnimeListUserFavoritePersons");

            migrationBuilder.DropTable(
                name: "MyAnimeListUserMangaListColors");

            migrationBuilder.DropTable(
                name: "DiscordGuilds");

            migrationBuilder.DropTable(
                name: "DiscordUsers");

            migrationBuilder.DropTable(
                name: "MyAnimeListUsers");
        }
    }
}
