using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase11_BulkInvoicingB2B : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPercentage",
                table: "Referrals",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Referrals",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ContractInvoiceId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractInvoices",
                columns: table => new
                {
                    ContractInvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferralId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractInvoices", x => x.ContractInvoiceId);
                    table.ForeignKey(
                        name: "FK_ContractInvoices_Referrals_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Referrals",
                        principalColumn: "ReferralId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ContractInvoiceId",
                table: "Invoices",
                column: "ContractInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractInvoices_ReferralId",
                table: "ContractInvoices",
                column: "ReferralId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_ContractInvoices_ContractInvoiceId",
                table: "Invoices",
                column: "ContractInvoiceId",
                principalTable: "ContractInvoices",
                principalColumn: "ContractInvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_ContractInvoices_ContractInvoiceId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "ContractInvoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ContractInvoiceId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CommissionPercentage",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ContractInvoiceId",
                table: "Invoices");
        }
    }
}
