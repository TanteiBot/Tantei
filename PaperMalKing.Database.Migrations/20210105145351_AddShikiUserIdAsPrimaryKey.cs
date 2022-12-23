// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaperMalKing.Database.Migrations;

public partial class AddShikiUserIdAsPrimaryKey : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropPrimaryKey(
			name: "PK_ShikiFavourites",
			table: "ShikiFavourites");

		migrationBuilder.AddPrimaryKey(
			name: "PK_ShikiFavourites",
			table: "ShikiFavourites",
			columns: new[] { "Id", "FavType", "UserId" });
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropPrimaryKey(
			name: "PK_ShikiFavourites",
			table: "ShikiFavourites");

		migrationBuilder.AddPrimaryKey(
			name: "PK_ShikiFavourites",
			table: "ShikiFavourites",
			columns: new[] { "Id", "FavType" });
	}
}