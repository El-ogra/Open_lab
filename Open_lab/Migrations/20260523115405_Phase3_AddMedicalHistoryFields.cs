using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_AddMedicalHistoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAnemia",
                table: "MedicalHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasChronicDisease",
                table: "MedicalHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasHypertension",
                table: "MedicalHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasJointInflammation",
                table: "MedicalHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasKidneyFailure",
                table: "MedicalHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPregnancyComplication",
                table: "MedicalHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAnemia",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "HasChronicDisease",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "HasHypertension",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "HasJointInflammation",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "HasKidneyFailure",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "HasPregnancyComplication",
                table: "MedicalHistories");
        }
    }
}
