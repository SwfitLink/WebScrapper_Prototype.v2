using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations
{
    public partial class addTBLOrders1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderProduct_NavProducts_ProductId",
                table: "OrderProduct");

            migrationBuilder.DropTable(
                name: "NavProducts");

            migrationBuilder.DropIndex(
                name: "IX_OrderProduct_ProductId",
                table: "OrderProduct");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NavProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavProducts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderProduct_ProductId",
                table: "OrderProduct",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProduct_NavProducts_ProductId",
                table: "OrderProduct",
                column: "ProductId",
                principalTable: "NavProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
