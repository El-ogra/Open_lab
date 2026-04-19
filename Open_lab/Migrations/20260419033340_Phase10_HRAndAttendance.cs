using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase10_HRAndAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShiftId",
                table: "AttendanceLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShiftSchedules",
                columns: table => new
                {
                    ShiftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    GracePeriodMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftSchedules", x => x.ShiftId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_ShiftId",
                table: "AttendanceLogs",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceLogs_ShiftSchedules_ShiftId",
                table: "AttendanceLogs",
                column: "ShiftId",
                principalTable: "ShiftSchedules",
                principalColumn: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceLogs_ShiftSchedules_ShiftId",
                table: "AttendanceLogs");

            migrationBuilder.DropTable(
                name: "ShiftSchedules");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceLogs_ShiftId",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "AttendanceLogs");
        }
    }
}
