using Microsoft.EntityFrameworkCore.Migrations;

namespace PaperMalKing.Database.Migrations;

public partial class UpdateMalFavoriteCompany : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_MalFavoriteCompany_MalUsers_UserId",
			table: "MalFavoriteCompany");

		migrationBuilder.DropPrimaryKey(
			name: "PK_MalFavoriteCompany",
			table: "MalFavoriteCompany");

		migrationBuilder.RenameTable(
			name: "MalFavoriteCompany",
			newName: "MalFavoriteCompanies");

		migrationBuilder.RenameIndex(
			name: "IX_MalFavoriteCompany_UserId",
			table: "MalFavoriteCompanies",
			newName: "IX_MalFavoriteCompanies_UserId");

		migrationBuilder.AlterColumn<int>(
							name: "Id",
							table: "MalFavoriteCompanies",
							type: "INTEGER",
							nullable: false,
							oldClrType: typeof(int),
							oldType: "INTEGER")
						.OldAnnotation("Sqlite:Autoincrement", true);

		migrationBuilder.AddPrimaryKey(
			name: "PK_MalFavoriteCompanies",
			table: "MalFavoriteCompanies",
			columns: new[] { "Id", "UserId" });

		migrationBuilder.AddForeignKey(
			name: "FK_MalFavoriteCompanies_MalUsers_UserId",
			table: "MalFavoriteCompanies",
			column: "UserId",
			principalTable: "MalUsers",
			principalColumn: "UserId",
			onDelete: ReferentialAction.Cascade);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_MalFavoriteCompanies_MalUsers_UserId",
			table: "MalFavoriteCompanies");

		migrationBuilder.DropPrimaryKey(
			name: "PK_MalFavoriteCompanies",
			table: "MalFavoriteCompanies");

		migrationBuilder.RenameTable(
			name: "MalFavoriteCompanies",
			newName: "MalFavoriteCompany");

		migrationBuilder.RenameIndex(
			name: "IX_MalFavoriteCompanies_UserId",
			table: "MalFavoriteCompany",
			newName: "IX_MalFavoriteCompany_UserId");

		migrationBuilder.AlterColumn<int>(
							name: "Id",
							table: "MalFavoriteCompany",
							type: "INTEGER",
							nullable: false,
							oldClrType: typeof(int),
							oldType: "INTEGER")
						.Annotation("Sqlite:Autoincrement", true);

		migrationBuilder.AddPrimaryKey(
			name: "PK_MalFavoriteCompany",
			table: "MalFavoriteCompany",
			column: "Id");

		migrationBuilder.AddForeignKey(
			name: "FK_MalFavoriteCompany_MalUsers_UserId",
			table: "MalFavoriteCompany",
			column: "UserId",
			principalTable: "MalUsers",
			principalColumn: "UserId",
			onDelete: ReferentialAction.Cascade);
	}
}