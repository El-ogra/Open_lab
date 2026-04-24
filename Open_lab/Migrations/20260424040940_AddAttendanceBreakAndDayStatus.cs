using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceBreakAndDayStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ColonyCount",
                table: "Cultures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GrowthConditions",
                table: "Cultures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IsolatedOrganism",
                table: "Cultures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SampleType",
                table: "Cultures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AttendanceBreaks",
                columns: table => new
                {
                    BreakId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttendanceLogId = table.Column<int>(type: "int", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceBreaks", x => x.BreakId);
                    table.ForeignKey(
                        name: "FK_AttendanceBreaks_AttendanceLogs_AttendanceLogId",
                        column: x => x.AttendanceLogId,
                        principalTable: "AttendanceLogs",
                        principalColumn: "AttendanceLogId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceDayStatuses",
                columns: table => new
                {
                    DayStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceDayStatuses", x => x.DayStatusId);
                    table.ForeignKey(
                        name: "FK_AttendanceDayStatuses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceBreaks_AttendanceLogId_StartAt",
                table: "AttendanceBreaks",
                columns: new[] { "AttendanceLogId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceDayStatuses_UserId_Date",
                table: "AttendanceDayStatuses",
                columns: new[] { "UserId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceBreaks");

            migrationBuilder.DropTable(
                name: "AttendanceDayStatuses");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ColonyCount",
                table: "Cultures");

            migrationBuilder.DropColumn(
                name: "GrowthConditions",
                table: "Cultures");

            migrationBuilder.DropColumn(
                name: "IsolatedOrganism",
                table: "Cultures");

            migrationBuilder.DropColumn(
                name: "SampleType",
                table: "Cultures");
        }
    }
}
