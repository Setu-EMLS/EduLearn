using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduLearn.Migrations
{
    /// <inheritdoc />
    public partial class Batches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "LiveClasses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "Enrollments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "Assignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "Announcements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Schedule = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Seats = table.Column<int>(type: "int", nullable: true),
                    IsAdmissionOpen = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batches_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Batches_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveClasses_BatchId",
                table: "LiveClasses",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_BatchId",
                table: "Enrollments",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_BatchId",
                table: "Assignments",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_BatchId",
                table: "Announcements",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_CourseId_Name",
                table: "Batches",
                columns: new[] { "CourseId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Batches_TeacherId",
                table: "Batches",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Batches_BatchId",
                table: "Announcements",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Batches_BatchId",
                table: "Assignments",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Batches_BatchId",
                table: "Enrollments",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LiveClasses_Batches_BatchId",
                table: "LiveClasses",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Batches_BatchId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Batches_BatchId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Batches_BatchId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_LiveClasses_Batches_BatchId",
                table: "LiveClasses");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_LiveClasses_BatchId",
                table: "LiveClasses");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_BatchId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_BatchId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_BatchId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "LiveClasses");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Announcements");
        }
    }
}
