using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Open_lab.Migrations
{
    /// <inheritdoc />
    public partial class Fix6_LabIdSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabIdSequences",
                columns: table => new
                {
                    SequenceDate = table.Column<DateTime>(type: "date", nullable: false),
                    LastSequence = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabIdSequences", x => x.SequenceDate);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO LabIdSequences (SequenceDate, LastSequence, CreatedAt, UpdatedAt)
                SELECT
                    Parsed.SequenceDate,
                    MAX(Parsed.SequenceNumber) AS LastSequence,
                    SYSUTCDATETIME() AS CreatedAt,
                    SYSUTCDATETIME() AS UpdatedAt
                FROM (
                    SELECT
                        TRY_CONVERT(date, LEFT(LabId, 8), 112) AS SequenceDate,
                        TRY_CONVERT(int, SUBSTRING(LabId, 9, LEN(LabId) - 8)) AS SequenceNumber
                    FROM Patients
                    WHERE LEN(LabId) > 8
                ) AS Parsed
                WHERE Parsed.SequenceDate IS NOT NULL
                    AND Parsed.SequenceNumber IS NOT NULL
                GROUP BY Parsed.SequenceDate
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabIdSequences");
        }
    }
}
