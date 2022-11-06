using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthProject.Migrations
{
    public partial class zajecia2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstLogin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastPasswChange",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FirstLogin",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswChange",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }
    }
}
