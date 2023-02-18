using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations
{
    public partial class wishListDebug : Migration
    {
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "Cellphone",
				table: "AspNetUsers",
				newName: "Subscribed");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "Subscribed",
				table: "AspNetUsers",
				newName: "Cellphone");
		}
	}
}
