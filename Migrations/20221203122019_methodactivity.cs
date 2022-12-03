using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthProject.Migrations
{
    public partial class methodactivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MethodType",
                table: "UserActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MethodType",
                table: "UserActivities");
        }
    }
}
