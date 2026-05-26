using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class Sprint5BackendModerationSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademicEventSubscriptions",
                columns: table => new
                {
                    AcademicEventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicEventSubscriptions", x => new { x.AcademicEventId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AcademicEventSubscriptions_AcademicEvents_AcademicEventId",
                        column: x => x.AcademicEventId,
                        principalTable: "AcademicEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcademicEventSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicEventSubscriptions_UserId",
                table: "AcademicEventSubscriptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicEventSubscriptions");
        }
    }
}
