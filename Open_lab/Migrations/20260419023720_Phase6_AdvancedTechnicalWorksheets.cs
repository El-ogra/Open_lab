using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase6_AdvancedTechnicalWorksheets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reagents",
                columns: table => new
                {
                    ReagentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reagents", x => x.ReagentId);
                });

            migrationBuilder.CreateTable(
                name: "TestConsumptions",
                columns: table => new
                {
                    ConsumptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    ReagentId = table.Column<int>(type: "int", nullable: false),
                    AmountPerTest = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestConsumptions", x => x.ConsumptionId);
                    table.ForeignKey(
                        name: "FK_TestConsumptions_Reagents_ReagentId",
                        column: x => x.ReagentId,
                        principalTable: "Reagents",
                        principalColumn: "ReagentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestConsumptions_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "TestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestConsumptions_ReagentId",
                table: "TestConsumptions",
                column: "ReagentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestConsumptions_TestId",
                table: "TestConsumptions",
                column: "TestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestConsumptions");

            migrationBuilder.DropTable(
                name: "Reagents");
        }
    }
}
