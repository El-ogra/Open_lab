using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase5_SampleTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExternalSample",
                table: "SampleCollections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeparated",
                table: "SampleCollections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReceivedBy",
                table: "SampleCollections",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SampleCollections_ReceivedBy",
                table: "SampleCollections",
                column: "ReceivedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_SampleCollections_Users_ReceivedBy",
                table: "SampleCollections",
                column: "ReceivedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SampleCollections_Users_ReceivedBy",
                table: "SampleCollections");

            migrationBuilder.DropIndex(
                name: "IX_SampleCollections_ReceivedBy",
                table: "SampleCollections");

            migrationBuilder.DropColumn(
                name: "IsExternalSample",
                table: "SampleCollections");

            migrationBuilder.DropColumn(
                name: "IsSeparated",
                table: "SampleCollections");

            migrationBuilder.DropColumn(
                name: "ReceivedBy",
                table: "SampleCollections");
        }
    }
}
