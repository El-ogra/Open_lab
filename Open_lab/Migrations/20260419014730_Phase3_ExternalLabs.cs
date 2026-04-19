using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_ExternalLabs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalLabQueues",
                columns: table => new
                {
                    QueueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitTestId = table.Column<int>(type: "int", nullable: false),
                    ReferralId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateQueued = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalReference = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLabQueues", x => x.QueueId);
                    table.ForeignKey(
                        name: "FK_ExternalLabQueues_Referrals_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Referrals",
                        principalColumn: "ReferralId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExternalLabQueues_VisitTests_VisitTestId",
                        column: x => x.VisitTestId,
                        principalTable: "VisitTests",
                        principalColumn: "VisitTestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalLabSettlements",
                columns: table => new
                {
                    SettlementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferralId = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLabSettlements", x => x.SettlementId);
                    table.ForeignKey(
                        name: "FK_ExternalLabSettlements_Referrals_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Referrals",
                        principalColumn: "ReferralId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentManifests",
                columns: table => new
                {
                    ManifestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManifestNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferralId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateShipped = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourierNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentManifests", x => x.ManifestId);
                    table.ForeignKey(
                        name: "FK_ShipmentManifests_Referrals_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Referrals",
                        principalColumn: "ReferralId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentItems",
                columns: table => new
                {
                    ShipmentItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManifestId = table.Column<int>(type: "int", nullable: false),
                    QueueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentItems", x => x.ShipmentItemId);
                    table.ForeignKey(
                        name: "FK_ShipmentItems_ExternalLabQueues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "ExternalLabQueues",
                        principalColumn: "QueueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipmentItems_ShipmentManifests_ManifestId",
                        column: x => x.ManifestId,
                        principalTable: "ShipmentManifests",
                        principalColumn: "ManifestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLabQueues_ReferralId",
                table: "ExternalLabQueues",
                column: "ReferralId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLabQueues_VisitTestId",
                table: "ExternalLabQueues",
                column: "VisitTestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLabSettlements_ReferralId",
                table: "ExternalLabSettlements",
                column: "ReferralId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ManifestId",
                table: "ShipmentItems",
                column: "ManifestId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_QueueId",
                table: "ShipmentItems",
                column: "QueueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentManifests_ReferralId",
                table: "ShipmentManifests",
                column: "ReferralId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalLabSettlements");

            migrationBuilder.DropTable(
                name: "ShipmentItems");

            migrationBuilder.DropTable(
                name: "ExternalLabQueues");

            migrationBuilder.DropTable(
                name: "ShipmentManifests");
        }
    }
}
