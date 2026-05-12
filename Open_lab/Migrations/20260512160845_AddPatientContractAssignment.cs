using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientContractAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReferralId",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ReferralId",
                table: "Patients",
                column: "ReferralId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Referrals_ReferralId",
                table: "Patients",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "ReferralId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Referrals_ReferralId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_ReferralId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ReferralId",
                table: "Patients");
        }
    }
}
