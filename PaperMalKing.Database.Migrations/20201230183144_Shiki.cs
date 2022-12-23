// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaperMalKing.Database.Migrations;

public partial class Shiki : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "ShikiUsers",
			columns: table => new
			{
				Id = table.Column<long>(type: "INTEGER", nullable: false),
				LastHistoryEntryId = table.Column<long>(type: "INTEGER", nullable: false),
				DiscordUserId = table.Column<long>(type: "INTEGER", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_ShikiUsers", x => x.Id);
				table.ForeignKey(
					name: "FK_ShikiUsers_DiscordUsers_DiscordUserId",
					column: x => x.DiscordUserId,
					principalTable: "DiscordUsers",
					principalColumn: "DiscordUserId",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateTable(
			name: "ShikiFavourites",
			columns: table => new
			{
				Id = table.Column<long>(type: "INTEGER", nullable: false),
				FavType = table.Column<string>(type: "TEXT", nullable: false),
				UserId = table.Column<long>(type: "INTEGER", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_ShikiFavourites", x => new { x.Id, x.FavType });
				table.ForeignKey(
					name: "FK_ShikiFavourites_ShikiUsers_UserId",
					column: x => x.UserId,
					principalTable: "ShikiUsers",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_ShikiFavourites_UserId",
			table: "ShikiFavourites",
			column: "UserId");

		migrationBuilder.CreateIndex(
			name: "IX_ShikiUsers_DiscordUserId",
			table: "ShikiUsers",
			column: "DiscordUserId");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "ShikiFavourites");

		migrationBuilder.DropTable(
			name: "ShikiUsers");
	}
}