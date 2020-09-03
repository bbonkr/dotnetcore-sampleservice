using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleService.Data.SqlServer.Migrations
{
    public partial class Add_AuthorizationLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "Users",
                comment: "사용자");

            migrationBuilder.AlterTable(
                name: "RefreshToken",
                comment: "리프레시 토큰");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "RefreshToken",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(2020, 9, 3, 8, 8, 20, 103, DateTimeKind.Unspecified).AddTicks(837), new TimeSpan(0, 0, 0, 0, 0)),
                comment: "작성시각",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldComment: "작성시각");

            migrationBuilder.CreateTable(
                name: "AuthorizationLogs",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 36, nullable: false, comment: "식별자"),
                    Username = table.Column<string>(maxLength: 100, nullable: false, comment: "사용자 계정이름"),
                    IsSuccess = table.Column<bool>(nullable: false, defaultValue: false, comment: "성공여부"),
                    IpAddress = table.Column<string>(maxLength: 100, nullable: true, comment: "아이피 주소"),
                    Hostname = table.Column<string>(maxLength: 100, nullable: true, comment: "기기명칭"),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false, defaultValue: new DateTimeOffset(new DateTime(2020, 9, 3, 8, 8, 20, 111, DateTimeKind.Unspecified).AddTicks(3985), new TimeSpan(0, 0, 0, 0, 0)), comment: "작성시각")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationLogs", x => x.Id);
                },
                comment: "인증로그");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizationLogs");

            migrationBuilder.AlterTable(
                name: "Users",
                oldComment: "사용자");

            migrationBuilder.AlterTable(
                name: "RefreshToken",
                oldComment: "리프레시 토큰");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "RefreshToken",
                type: "datetimeoffset",
                nullable: false,
                comment: "작성시각",
                oldClrType: typeof(DateTimeOffset),
                oldDefaultValue: new DateTimeOffset(new DateTime(2020, 9, 3, 8, 8, 20, 103, DateTimeKind.Unspecified).AddTicks(837), new TimeSpan(0, 0, 0, 0, 0)),
                oldComment: "작성시각");
        }
    }
}
