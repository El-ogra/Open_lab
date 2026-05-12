using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Gap 4.5 — Arrange Report Order Persistence.
    /// Adds a ReportOrder column to VisitTests so the order in which a visit's
    /// tests appear in the printed report can be persisted, not just reordered
    /// in-memory inside the ViewModel.
    /// </remarks>
    public partial class AddVisitTestReportOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportOrder",
                table: "VisitTests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportOrder",
                table: "VisitTests");
        }
    }
}
