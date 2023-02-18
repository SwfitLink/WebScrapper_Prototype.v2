using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations.ApplicationDb
{
    public partial class Subscribed : Migration
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
