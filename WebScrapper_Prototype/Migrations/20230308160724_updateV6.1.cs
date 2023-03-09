using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations
{
    public partial class updateV61 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.RenameColumn(
                name: "BasketId",
                table: "ShopingBasket",
                newName: "UserId");
			

		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
