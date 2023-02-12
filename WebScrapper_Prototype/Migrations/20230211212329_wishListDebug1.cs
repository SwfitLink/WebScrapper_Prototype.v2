using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations
{
    public partial class wishListDebug1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "UserWishList");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "UserWishList",
                newName: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserWishList",
                newName: "ID");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "UserWishList",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
