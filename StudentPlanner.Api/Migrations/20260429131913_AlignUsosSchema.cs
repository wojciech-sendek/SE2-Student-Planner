using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class AlignUsosSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsosTokens");

            migrationBuilder.DropIndex(
                name: "IX_UsosEvents_UserId",
                table: "UsosEvents");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "UsosEvents");

            migrationBuilder.RenameColumn(
                name: "LecturerName",
                table: "UsosEvents",
                newName: "Teacher");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "UsosEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedAtUtc",
                table: "UsosEvents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsosConnectedAtUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsosRefreshTokenProtected",
                table: "AspNetUsers",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsosScheduleSyncedAtUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsosEvents_UserId_ExternalId",
                table: "UsosEvents",
                columns: new[] { "UserId", "ExternalId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsosEvents_UserId_ExternalId",
                table: "UsosEvents");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "UsosEvents");

            migrationBuilder.DropColumn(
                name: "SyncedAtUtc",
                table: "UsosEvents");

            migrationBuilder.DropColumn(
                name: "UsosConnectedAtUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UsosRefreshTokenProtected",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UsosScheduleSyncedAtUtc",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Teacher",
                table: "UsosEvents",
                newName: "LecturerName");

            migrationBuilder.AddColumn<string>(
                name: "CourseId",
                table: "UsosEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UsosTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AccessTokenSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsosTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsosTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsosEvents_UserId",
                table: "UsosEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsosTokens_UserId",
                table: "UsosTokens",
                column: "UserId",
                unique: true);
        }
    }
}
