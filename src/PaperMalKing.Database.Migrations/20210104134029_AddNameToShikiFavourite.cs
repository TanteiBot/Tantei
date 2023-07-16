// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaperMalKing.Database.Migrations;

public partial class AddNameToShikiFavourite : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "Name",
			table: "ShikiFavourites",
			type: "TEXT",
			nullable: false,
			defaultValue: "");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "Name",
			table: "ShikiFavourites");
	}
}