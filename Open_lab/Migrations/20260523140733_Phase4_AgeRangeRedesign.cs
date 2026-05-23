using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_AgeRangeRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestReferenceRanges_AgeGroups_AgeGroupId",
                table: "TestReferenceRanges");

            migrationBuilder.DropTable(
                name: "AgeGroups");

            migrationBuilder.RenameColumn(
                name: "AgeTo",
                table: "TestReferenceRanges",
                newName: "AgeToValue");

            migrationBuilder.RenameColumn(
                name: "AgeGroupId",
                table: "TestReferenceRanges",
                newName: "AgeToDays");

            migrationBuilder.RenameColumn(
                name: "AgeFrom",
                table: "TestReferenceRanges",
                newName: "AgeFromValue");

            migrationBuilder.RenameIndex(
                name: "IX_TestReferenceRanges_AgeGroupId",
                table: "TestReferenceRanges",
                newName: "IX_TestReferenceRanges_AgeToDays");

            migrationBuilder.AddColumn<int>(
                name: "AgeFromDays",
                table: "TestReferenceRanges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgeFromUnit",
                table: "TestReferenceRanges",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgeToUnit",
                table: "TestReferenceRanges",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgeUnit",
                table: "Patients",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestReferenceRanges_AgeFromDays",
                table: "TestReferenceRanges",
                column: "AgeFromDays");

            // Phase 4 Data Migration: assume any legacy AgeFrom/AgeTo numbers were years
            // (that was the original schema's implicit unit), then derive the *Days projection.
            // Also wipe AgeToDays since it was inherited from AgeGroupId values during rename.
            migrationBuilder.Sql(@"
                -- Step 1: AgeToDays was renamed from AgeGroupId — its values are FK ids, not days.
                -- Clear it so we can recompute properly.
                UPDATE TestReferenceRanges SET AgeToDays = NULL;

                -- Step 2: report how many legacy rows need conversion.
                DECLARE @rowCount INT = (
                    SELECT COUNT(*) FROM TestReferenceRanges
                    WHERE AgeFromValue IS NOT NULL OR AgeToValue IS NOT NULL
                );
                PRINT N'Phase 4 — TestReferenceRanges with legacy AgeFrom/AgeTo: ' + CAST(@rowCount AS NVARCHAR(10));

                -- Step 3: convert assuming Year units (matches the pre-Phase-4 implicit contract).
                UPDATE TestReferenceRanges
                SET AgeFromUnit = N'Year',
                    AgeFromDays = AgeFromValue * 365
                WHERE AgeFromValue IS NOT NULL;

                UPDATE TestReferenceRanges
                SET AgeToUnit = N'Year',
                    AgeToDays = AgeToValue * 365
                WHERE AgeToValue IS NOT NULL;

                -- Step 4: backfill Patient.AgeUnit for existing rows that already carry an Age.
                UPDATE Patients SET AgeUnit = N'Year' WHERE Age IS NOT NULL AND AgeUnit IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestReferenceRanges_AgeFromDays",
                table: "TestReferenceRanges");

            migrationBuilder.DropColumn(
                name: "AgeFromDays",
                table: "TestReferenceRanges");

            migrationBuilder.DropColumn(
                name: "AgeFromUnit",
                table: "TestReferenceRanges");

            migrationBuilder.DropColumn(
                name: "AgeToUnit",
                table: "TestReferenceRanges");

            migrationBuilder.DropColumn(
                name: "AgeUnit",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "AgeToValue",
                table: "TestReferenceRanges",
                newName: "AgeTo");

            migrationBuilder.RenameColumn(
                name: "AgeToDays",
                table: "TestReferenceRanges",
                newName: "AgeGroupId");

            migrationBuilder.RenameColumn(
                name: "AgeFromValue",
                table: "TestReferenceRanges",
                newName: "AgeFrom");

            migrationBuilder.RenameIndex(
                name: "IX_TestReferenceRanges_AgeToDays",
                table: "TestReferenceRanges",
                newName: "IX_TestReferenceRanges_AgeGroupId");

            migrationBuilder.CreateTable(
                name: "AgeGroups",
                columns: table => new
                {
                    AgeGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgeFromMonths = table.Column<int>(type: "int", nullable: false),
                    AgeToMonths = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgeGroups", x => x.AgeGroupId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgeGroups_DisplayOrder",
                table: "AgeGroups",
                column: "DisplayOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_TestReferenceRanges_AgeGroups_AgeGroupId",
                table: "TestReferenceRanges",
                column: "AgeGroupId",
                principalTable: "AgeGroups",
                principalColumn: "AgeGroupId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
