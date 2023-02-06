using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations.WebScrapper_Prototype
{
    public partial class ShoppingCart2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ShopingBasket_ShoppingBasketId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ShoppingBasketId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShoppingBasketId",
                table: "Products");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShoppingBasketId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ShoppingBasketId",
                table: "Products",
                column: "ShoppingBasketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ShopingBasket_ShoppingBasketId",
                table: "Products",
                column: "ShoppingBasketId",
                principalTable: "ShopingBasket",
                principalColumn: "Id");
        }
    }
}
