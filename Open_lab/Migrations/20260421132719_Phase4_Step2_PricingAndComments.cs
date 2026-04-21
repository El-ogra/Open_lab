using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_Step2_PricingAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhysicianId",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighComment",
                table: "TestComments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LowComment",
                table: "TestComments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Physicians",
                columns: table => new
                {
                    PhysicianId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specialty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PriceListId = table.Column<int>(type: "int", nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Physicians", x => x.PhysicianId);
                    table.ForeignKey(
                        name: "FK_Physicians_PriceLists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "PriceLists",
                        principalColumn: "PriceListId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_PhysicianId",
                table: "Visits",
                column: "PhysicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Physicians_PriceListId",
                table: "Physicians",
                column: "PriceListId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Physicians_PhysicianId",
                table: "Visits",
                column: "PhysicianId",
                principalTable: "Physicians",
                principalColumn: "PhysicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Physicians_PhysicianId",
                table: "Visits");

            migrationBuilder.DropTable(
                name: "Physicians");

            migrationBuilder.DropIndex(
                name: "IX_Visits_PhysicianId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "PhysicianId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "HighComment",
                table: "TestComments");

            migrationBuilder.DropColumn(
                name: "LowComment",
                table: "TestComments");
        }
    }
}
