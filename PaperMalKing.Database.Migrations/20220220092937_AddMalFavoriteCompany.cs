using Microsoft.EntityFrameworkCore.Migrations;

namespace PaperMalKing.Database.Migrations;

public partial class AddMalFavoriteCompany : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "MalFavoriteCompany",
			columns: table => new
			{
				Id = table.Column<int>(type: "INTEGER", nullable: false)
						  .Annotation("Sqlite:Autoincrement", true),
				UserId = table.Column<int>(type: "INTEGER", nullable: false),
				ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
				Name = table.Column<string>(type: "TEXT", nullable: false),
				NameUrl = table.Column<string>(type: "TEXT", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_MalFavoriteCompany", x => x.Id);
				table.ForeignKey(
					name: "FK_MalFavoriteCompany_MalUsers_UserId",
					column: x => x.UserId,
					principalTable: "MalUsers",
					principalColumn: "UserId",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_MalFavoriteCompany_UserId",
			table: "MalFavoriteCompany",
			column: "UserId");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "MalFavoriteCompany");
	}
}