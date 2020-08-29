using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleService.Data.SqlServer.Migrations
{
    public partial class Add_fileds_on_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailCount",
                table: "Users",
                nullable: false,
                defaultValue: 0,
                comment: "인증 실패수");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Users",
                nullable: false,
                defaultValue: true,
                comment: "사용여부");

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Users",
                nullable: false,
                defaultValue: false,
                comment: "계정 잠금 여부");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Users");
        }
    }
}
