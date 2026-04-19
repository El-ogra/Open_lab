using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_MicrobiologyAdvanced : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSafeForChildren",
                table: "Antibiotics",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSafeForPregnancy",
                table: "Antibiotics",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSafeForChildren",
                table: "Antibiotics");

            migrationBuilder.DropColumn(
                name: "IsSafeForPregnancy",
                table: "Antibiotics");
        }
    }
}
