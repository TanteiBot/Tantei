using Microsoft.EntityFrameworkCore.Migrations;

namespace PaperMalKing.Database.Migrations
{
	public partial class AddAniListUserFeatures : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<ulong>(
				name: "Features",
				table: "AniListUsers",
				type: "INTEGER",
				nullable: false,
				defaultValue: 127ul);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Features",
				table: "AniListUsers");
		}
	}
}
