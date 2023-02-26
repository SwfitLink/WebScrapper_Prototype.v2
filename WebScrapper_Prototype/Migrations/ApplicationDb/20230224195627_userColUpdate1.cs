using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations.ApplicationDb
{
    public partial class userColUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsSubscribed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IsSubscribed",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
