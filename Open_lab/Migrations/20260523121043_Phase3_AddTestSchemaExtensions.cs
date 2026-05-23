using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_AddTestSchemaExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgeGroupId",
                table: "TestReferenceRanges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParameterId",
                table: "TestReferenceRanges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParameterId",
                table: "TestComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AgeGroups",
                columns: table => new
                {
                    AgeGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgeFromMonths = table.Column<int>(type: "int", nullable: false),
                    AgeToMonths = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgeGroups", x => x.AgeGroupId);
                });

            // G2.3 Seed: 6 standard age groups (decided in Phase 3 planning).
            migrationBuilder.InsertData(
                table: "AgeGroups",
                columns: new[] { "Name", "AgeFromMonths", "AgeToMonths", "DisplayOrder" },
                values: new object[,]
                {
                    { "رضيع", 0, 12, 1 },
                    { "طفل صغير", 12, 72, 2 },
                    { "طفل", 72, 144, 3 },
                    { "مراهق", 144, 216, 4 },
                    { "بالغ", 216, 720, 5 },
                    { "كبير في السن", 720, 1500, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestReferenceRanges_AgeGroupId",
                table: "TestReferenceRanges",
                column: "AgeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TestReferenceRanges_ParameterId",
                table: "TestReferenceRanges",
                column: "ParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_TestComments_ParameterId",
                table: "TestComments",
                column: "ParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_AgeGroups_DisplayOrder",
                table: "AgeGroups",
                column: "DisplayOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_TestComments_TestParameters_ParameterId",
                table: "TestComments",
                column: "ParameterId",
                principalTable: "TestParameters",
                principalColumn: "ParameterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestReferenceRanges_AgeGroups_AgeGroupId",
                table: "TestReferenceRanges",
                column: "AgeGroupId",
                principalTable: "AgeGroups",
                principalColumn: "AgeGroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestReferenceRanges_TestParameters_ParameterId",
                table: "TestReferenceRanges",
                column: "ParameterId",
                principalTable: "TestParameters",
                principalColumn: "ParameterId",
                onDelete: ReferentialAction.Restrict);

            // G2.1 Data Migration: auto-link existing ranges/comments to the single
            // parameter of simple tests (tests with exactly one TestParameter). Ranges
            // belonging to multi-component tests stay null until manually re-mapped.
            migrationBuilder.Sql(@"
                UPDATE r SET r.ParameterId = (
                    SELECT TOP 1 p.ParameterId FROM TestParameters p WHERE p.TestId = r.TestId
                )
                FROM TestReferenceRanges r
                WHERE r.ParameterId IS NULL
                  AND (SELECT COUNT(*) FROM TestParameters WHERE TestId = r.TestId) = 1;

                UPDATE c SET c.ParameterId = (
                    SELECT TOP 1 p.ParameterId FROM TestParameters p WHERE p.TestId = c.TestId
                )
                FROM TestComments c
                WHERE c.ParameterId IS NULL
                  AND (SELECT COUNT(*) FROM TestParameters WHERE TestId = c.TestId) = 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestComments_TestParameters_ParameterId",
                table: "TestComments");

            migrationBuilder.DropForeignKey(
                name: "FK_TestReferenceRanges_AgeGroups_AgeGroupId",
                table: "TestReferenceRanges");

            migrationBuilder.DropForeignKey(
                name: "FK_TestReferenceRanges_TestParameters_ParameterId",
                table: "TestReferenceRanges");

            migrationBuilder.DropTable(
                name: "AgeGroups");

            migrationBuilder.DropIndex(
                name: "IX_TestReferenceRanges_AgeGroupId",
                table: "TestReferenceRanges");

            migrationBuilder.DropIndex(
                name: "IX_TestReferenceRanges_ParameterId",
                table: "TestReferenceRanges");

            migrationBuilder.DropIndex(
                name: "IX_TestComments_ParameterId",
                table: "TestComments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "AgeGroupId",
                table: "TestReferenceRanges");

            migrationBuilder.DropColumn(
                name: "ParameterId",
                table: "TestReferenceRanges");

            migrationBuilder.DropColumn(
                name: "ParameterId",
                table: "TestComments");
        }
    }
}
