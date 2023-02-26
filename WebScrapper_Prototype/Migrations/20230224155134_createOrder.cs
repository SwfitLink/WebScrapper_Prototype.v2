using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations
{
    public partial class createOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckoutAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    locationCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    locationProvince = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    addressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    addressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutAccount", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckoutAccount");
        }
    }
}
