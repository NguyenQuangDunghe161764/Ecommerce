using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace demoWebAPI.Migrations
{
    public partial class AddZaloPayAppTransId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZaloPayAppTransId",
                table: "orders",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZaloPayAppTransId",
                table: "orders");
        }
    }
}
