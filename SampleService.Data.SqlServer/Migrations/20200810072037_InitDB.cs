using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleService.Data.SqlServer.Migrations
{
    public partial class InitDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 36, nullable: false, comment: "식별자"),
                    FirstName = table.Column<string>(maxLength: 100, nullable: false, comment: "성"),
                    LastName = table.Column<string>(maxLength: 100, nullable: false, comment: "이름"),
                    UserName = table.Column<string>(maxLength: 100, nullable: false, comment: "계정이름"),
                    Password = table.Column<string>(maxLength: 4000, nullable: true, comment: "비밀번호")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 36, nullable: false, comment: "식별자"),
                    UserId = table.Column<string>(maxLength: 36, nullable: false, comment: "사용자 식별자"),
                    Token = table.Column<string>(maxLength: 4000, nullable: false, comment: "리프레시 토큰"),
                    Expires = table.Column<DateTimeOffset>(nullable: false, comment: "만료시각"),
                    Created = table.Column<DateTimeOffset>(nullable: false, comment: "작성시각"),
                    CreatedByIp = table.Column<string>(maxLength: 100, nullable: false, comment: "작성 요청 아이피 주소"),
                    Revoked = table.Column<DateTimeOffset>(nullable: true, comment: "취소시각"),
                    RevokedByIp = table.Column<string>(maxLength: 100, nullable: true, comment: "취소 요청 아이피 주소"),
                    ReplacedByToken = table.Column<string>(maxLength: 4000, nullable: true, comment: "취소 요청 토큰")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserId",
                table: "RefreshToken",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
