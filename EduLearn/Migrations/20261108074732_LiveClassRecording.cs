using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduLearn.Migrations
{
    /// <inheritdoc />
    public partial class LiveClassRecording : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecordingAddedAt",
                table: "LiveClasses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecordingPath",
                table: "LiveClasses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecordingUrl",
                table: "LiveClasses",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordingAddedAt",
                table: "LiveClasses");

            migrationBuilder.DropColumn(
                name: "RecordingPath",
                table: "LiveClasses");

            migrationBuilder.DropColumn(
                name: "RecordingUrl",
                table: "LiveClasses");
        }
    }
}
