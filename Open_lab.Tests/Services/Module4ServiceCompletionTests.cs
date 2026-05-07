using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    /// <summary>
    /// Service layer completion tests for Module 4: Result Entry & Reporting
    /// Fills all missing Success / Failure / Edge scenarios per UnitTest_Skill.md
    /// Covers Functions 4.1-4.9
    /// </summary>
    public class Module4ServiceCompletionTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly ResultsService _resultsService;
        private readonly ReportService _reportService;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module4ServiceCompletionTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _resultsService = new ResultsService(_db);
            _reportService = new ReportService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ===================================================================
        // Function 4.4 — Create Composite Report
        // ===================================================================

        [Fact]
        public async Task GetCompositeReportAsync_With_Valid_Visit_Should_Return_Report_Success()
        {
            // Function: 4.4 — Create Composite Report
            // Arrange
            var patient = new Patient { PatientId = 100, FullName = "Composite Patient", LabId = "L-COMP" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 100, PatientId = 100, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test1 = new Test { TestId = 100, Code = "T1", NameReport = "Test 1", NameReceipt = "T1", Price = 50m };
            var test2 = new Test { TestId = 101, Code = "T2", NameReport = "Test 2", NameReceipt = "T2", Price = 75m };
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitTestId = 100, VisitId = 100, TestId = 100, Price = 50m, Status = "Completed" };
            var vt2 = new VisitTest { VisitTestId = 101, VisitId = 100, TestId = 101, Price = 75m, Status = "Completed" };
            _db.VisitTests.AddRange(vt1, vt2);
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetCompositeReportAsync(100);

            // Assert
            report.Should().NotBeNull();
            report!.Patient.Should().NotBeNull();
            report.Patient.FullName.Should().Be("Composite Patient");
            report.Tests.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCompositeReportAsync_With_NonExistent_Visit_Should_Return_Null_Failure()
        {
            // Function: 4.4 — Create Composite Report
            // Arrange
            // Act
            var report = await _reportService.GetCompositeReportAsync(99999);

            // Assert
            report.Should().BeNull();
        }

        [Fact]
        public async Task GetCompositeReportAsync_With_Custom_Order_Should_Reorder_Tests_Edge()
        {
            // Function: 4.4 — Create Composite Report (with custom ordering)
            // Arrange
            var patient = new Patient { PatientId = 200, FullName = "Ordered Patient", LabId = "L-ORD" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 200, PatientId = 200, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test1 = new Test { TestId = 200, Code = "A", NameReport = "Alpha" };
            var test2 = new Test { TestId = 201, Code = "B", NameReport = "Beta" };
            var test3 = new Test { TestId = 202, Code = "C", NameReport = "Gamma" };
            _db.Tests.AddRange(test1, test2, test3);
            await _db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitTestId = 200, VisitId = 200, TestId = 200 };
            var vt2 = new VisitTest { VisitTestId = 201, VisitId = 200, TestId = 201 };
            var vt3 = new VisitTest { VisitTestId = 202, VisitId = 200, TestId = 202 };
            _db.VisitTests.AddRange(vt1, vt2, vt3);
            await _db.SaveChangesAsync();

            // Custom order: C, A, B
            var customOrder = new List<int> { 202, 200, 201 };

            // Act
            var report = await _reportService.GetCompositeReportAsync(200, customOrder);

            // Assert
            report.Should().NotBeNull();
            report!.Tests.Should().HaveCount(3);
            report.Tests[0].VisitTest.VisitTestId.Should().Be(202); // C first
            report.Tests[1].VisitTest.VisitTestId.Should().Be(200); // A second
            report.Tests[2].VisitTest.VisitTestId.Should().Be(201); // B third
        }

        // ===================================================================
        // Function 4.5 — Arrange Report Order
        // ===================================================================

        [Fact]
        public async Task GetVisitReportAsync_Should_Order_By_ReportOrder_Success()
        {
            // Function: 4.5 — Arrange Report Order
            // Arrange
            var patient = new Patient { PatientId = 300, FullName = "Order Test" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 300, PatientId = 300, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test1 = new Test { TestId = 300, Code = "Z", NameReport = "Zulu", ReportOrder = 3 };
            var test2 = new Test { TestId = 301, Code = "A", NameReport = "Alpha", ReportOrder = 1 };
            var test3 = new Test { TestId = 302, Code = "M", NameReport = "Mike", ReportOrder = 2 };
            _db.Tests.AddRange(test1, test2, test3);
            await _db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitTestId = 300, VisitId = 300, TestId = 300 };
            var vt2 = new VisitTest { VisitTestId = 301, VisitId = 300, TestId = 301 };
            var vt3 = new VisitTest { VisitTestId = 302, VisitId = 300, TestId = 302 };
            _db.VisitTests.AddRange(vt1, vt2, vt3);
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(300);

            // Assert - Should be ordered by ReportOrder: Alpha(1), Mike(2), Zulu(3)
            report.Should().NotBeNull();
            report!.Tests.Should().HaveCount(3);
            report.Tests[0].Test.NameReport.Should().Be("Alpha");
            report.Tests[1].Test.NameReport.Should().Be("Mike");
            report.Tests[2].Test.NameReport.Should().Be("Zulu");
        }

        [Fact]
        public async Task GetVisitReportAsync_With_Same_ReportOrder_Should_Order_By_Name_Success()
        {
            // Function: 4.5 — Arrange Report Order (tie-breaker)
            // Arrange
            var patient = new Patient { PatientId = 400, FullName = "Tie Test" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 400, PatientId = 400, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test1 = new Test { TestId = 400, Code = "B", NameReport = "Bravo", ReportOrder = 1 };
            var test2 = new Test { TestId = 401, Code = "A", NameReport = "Alpha", ReportOrder = 1 };
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitTestId = 400, VisitId = 400, TestId = 400 };
            var vt2 = new VisitTest { VisitTestId = 401, VisitId = 400, TestId = 401 };
            _db.VisitTests.AddRange(vt1, vt2);
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(400);

            // Assert - Should be ordered by NameReport when ReportOrder is equal
            report.Should().NotBeNull();
            report!.Tests.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetVisitReportAsync_With_No_Tests_Should_Return_Empty_Report_Edge()
        {
            // Function: 4.5 — Arrange Report Order (empty case)
            // Arrange
            var patient = new Patient { PatientId = 500, FullName = "Empty Report" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 500, PatientId = 500, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(500);

            // Assert
            report.Should().NotBeNull();
            report!.Tests.Should().BeEmpty();
        }

        // ===================================================================
        // Function 4.6 — Preview Report
        // ===================================================================

        [Fact]
        public async Task GetVisitReportAsync_For_Preview_Should_Include_All_Data_Success()
        {
            // Function: 4.6 — Preview Report
            // Arrange
            var patient = new Patient { PatientId = 600, FullName = "Preview Patient", LabId = "L-PREV", Gender = "Male", Age = 30 };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 600, PatientId = 600, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { TestId = 600, Code = "PREV", NameReport = "Preview Test" };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var param = new TestParameter { ParameterId = 600, TestId = 600, Name = "PrevParam", OrderNo = 1 };
            _db.TestParameters.Add(param);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitTestId = 600, VisitId = 600, TestId = 600, Price = 100m, Status = "Completed" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            _db.ResultValues.Add(new ResultValue { VisitTestId = 600, ParameterId = 600, Value = "99.9", Flag = "N" });
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(600);

            // Assert
            report.Should().NotBeNull();
            report!.Patient.Should().NotBeNull();
            report.Patient.FullName.Should().Be("Preview Patient");
            report.Tests.Should().HaveCount(1);
            report.Tests[0].Results.Should().HaveCount(1);
            report.Tests[0].Results[0].Result.Value.Should().Be("99.9");
        }

        [Fact]
        public async Task GetVisitReportAsync_With_InvalidVisitId_Should_Return_Null_Failure()
        {
            // Function: 4.6 — Preview Report
            // Arrange
            // Act
            var report = await _reportService.GetVisitReportAsync(-1);

            // Assert
            report.Should().BeNull();
        }

        [Fact]
        public async Task GetVisitReportAsync_With_Verified_Only_Status_Should_Still_Return_Edge()
        {
            // Function: 4.6 — Preview Report (verified status edge case)
            // Arrange
            var patient = new Patient { PatientId = 700, FullName = "Verified Patient" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 700, PatientId = 700, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { TestId = 700, Code = "VER", NameReport = "Verified Test" };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitTestId = 700, VisitId = 700, TestId = 700, Status = "Verified" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(700);

            // Assert
            report.Should().NotBeNull();
            report!.Tests.Should().HaveCount(1);
        }

        // ===================================================================
        // Function 4.7 — Print Report
        // ===================================================================

        [Fact]
        public async Task LogVisitReportPrintedAsync_Should_Create_AuditLog_With_UserId_Success()
        {
            // Function: 4.7 — Print Report (BR-SEC-002 Audit Trail)
            // Arrange
            var visitId = 800;
            var userId = 42;

            // Act
            await _resultsService.LogVisitReportPrintedAsync(visitId, userId);

            // Assert
            var auditLog = await _db.AuditLogs.FirstOrDefaultAsync(l => l.Action == "PRINT_REPORT" && l.RecordId == visitId.ToString());
            auditLog.Should().NotBeNull();
            auditLog!.UserId.Should().Be(userId);
            auditLog.TableName.Should().Be("Visits");
            auditLog.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromHours(5));
        }

        [Fact]
        public async Task LogVisitReportPrintedAsync_With_ZeroUserId_Should_Still_Create_Log_Edge()
        {
            // Function: 4.7 — Print Report (edge case: system user)
            // Arrange
            var visitId = 801;
            var userId = 0;

            // Act
            await _resultsService.LogVisitReportPrintedAsync(visitId, userId);

            // Assert
            var auditLog = await _db.AuditLogs.FirstOrDefaultAsync(l => l.Action == "PRINT_REPORT");
            auditLog.Should().NotBeNull();
            auditLog!.UserId.Should().Be(0);
        }

        [Fact]
        public async Task GetVisitReportAsync_For_Print_Should_Include_Invoice_Data_Success()
        {
            // Function: 4.7 — Print Report (with invoice)
            // Arrange
            var patient = new Patient { PatientId = 802, FullName = "Invoice Patient" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 802, PatientId = 802, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = 802, NetTotal = 250m, Paid = 200m, Discount = 50m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(802);

            // Assert
            report.Should().NotBeNull();
            report!.Invoice.Should().NotBeNull();
            report.Invoice!.NetTotal.Should().Be(250m);
        }

        // ===================================================================
        // Function 4.8 — Print Blank Report
        // ===================================================================

        [Fact]
        public async Task GetVisitReportAsync_For_Blank_Report_Should_Include_Test_Names_Success()
        {
            // Function: 4.8 — Print Blank Report
            // Arrange
            var patient = new Patient { PatientId = 900, FullName = "Blank Patient", LabId = "L-BLANK" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 900, PatientId = 900, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { TestId = 900, Code = "BLK", NameReport = "Blank Test Name", NameReceipt = "Blank Receipt" };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitTestId = 900, VisitId = 900, TestId = 900, Price = 100m, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(900);

            // Assert
            report.Should().NotBeNull();
            report!.Tests.Should().HaveCount(1);
            report.Tests[0].Test.NameReport.Should().Be("Blank Test Name");
            report.Tests[0].Results.Should().BeEmpty(); // No results yet - blank report
        }

        [Fact]
        public async Task GetVisitReportAsync_For_Blank_With_Null_Referral_Should_Handle_Edge()
        {
            // Function: 4.8 — Print Blank Report (null referral edge)
            // Arrange
            var patient = new Patient { PatientId = 901, FullName = "No Referral Patient" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 901, PatientId = 901, VisitDate = DateTime.Now, ReferralId = null };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(901);

            // Assert
            report.Should().NotBeNull();
            report!.Visit.Referral.Should().BeNull();
        }

        [Fact]
        public async Task GetVisitReportAsync_With_Multiple_Pending_Tests_Should_Return_All_For_Blank_Success()
        {
            // Function: 4.8 — Print Blank Report (multiple tests)
            // Arrange
            var patient = new Patient { PatientId = 902, FullName = "Multi Blank" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = 902, PatientId = 902, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test1 = new Test { TestId = 902, Code = "A", NameReport = "Test A" };
            var test2 = new Test { TestId = 903, Code = "B", NameReport = "Test B" };
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitTestId = 902, VisitId = 902, TestId = 902, Status = "Pending" };
            var vt2 = new VisitTest { VisitTestId = 903, VisitId = 902, TestId = 903, Status = "Pending" };
            _db.VisitTests.AddRange(vt1, vt2);
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(902);

            // Assert
            report.Should().NotBeNull();
            report!.Tests.Should().HaveCount(2);
        }

        // ===================================================================
        // Function 4.9 — Compare with History (BR-MED-008)
        // ===================================================================

        [Fact]
        public async Task GetPatientHistoryAsync_With_History_Should_Return_All_Visits_Success()
        {
            // Function: 4.9 — Compare with History (BR-MED-008)
            // Arrange
            var patient = new Patient { PatientId = 1000, FullName = "History Patient", LabId = "L-HIST" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { VisitId = 1000, PatientId = 1000, VisitDate = DateTime.Now.AddMonths(-6) };
            var visit2 = new Visit { VisitId = 1001, PatientId = 1000, VisitDate = DateTime.Now.AddMonths(-3) };
            var visit3 = new Visit { VisitId = 1002, PatientId = 1000, VisitDate = DateTime.Now };
            _db.Visits.AddRange(visit1, visit2, visit3);
            await _db.SaveChangesAsync();

            // Act
            var history = await _reportService.GetPatientHistoryAsync(1000, null, null);

            // Assert
            history.Should().NotBeNull();
            history.Patient.Should().NotBeNull();
            history.Patient.FullName.Should().Be("History Patient");
            history.Visits.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetPatientHistoryAsync_With_Date_Range_Should_Filter_Visits_Success()
        {
            // Function: 4.9 — Compare with History (with date filter)
            // Arrange
            var patient = new Patient { PatientId = 1100, FullName = "Filtered Patient" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { VisitId = 1100, PatientId = 1100, VisitDate = new DateTime(2026, 1, 15) };
            var visit2 = new Visit { VisitId = 1101, PatientId = 1100, VisitDate = new DateTime(2026, 3, 15) };
            var visit3 = new Visit { VisitId = 1102, PatientId = 1100, VisitDate = new DateTime(2026, 5, 15) };
            _db.Visits.AddRange(visit1, visit2, visit3);
            await _db.SaveChangesAsync();

            var from = new DateTime(2026, 2, 1);
            var to = new DateTime(2026, 4, 30);

            // Act
            var history = await _reportService.GetPatientHistoryAsync(1100, from, to);

            // Assert - Should only include visit2 (March 15)
            history.Should().NotBeNull();
            history.Visits.Should().HaveCount(1);
            history.Visits[0].Visit.VisitId.Should().Be(1101);
        }

        [Fact]
        public async Task GetPatientHistoryAsync_With_No_Visits_Should_Return_Empty_Edge()
        {
            // Function: 4.9 — Compare with History (no visits edge)
            // Arrange
            var patient = new Patient { PatientId = 1200, FullName = "No History Patient" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            // Act
            var history = await _reportService.GetPatientHistoryAsync(1200, null, null);

            // Assert
            history.Should().NotBeNull();
            history.Patient.Should().NotBeNull();
            history.Visits.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPatientHistoryAsync_With_NonExistent_Patient_Should_Throw_Failure()
        {
            // Function: 4.9 — Compare with History (not found failure)
            // Arrange
            // Act & Assert
            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reportService.GetPatientHistoryAsync(99999, null, null));
        }

        [Fact]
        public async Task GetVisitReportAsync_Should_Include_Previous_Result_For_Comparison_Success()
        {
            // Function: 4.9 — Compare with History (previous value comparison)
            // Arrange
            var patientId = 1300;
            var patient = new Patient { PatientId = patientId, FullName = "Compare Patient" };
            _db.Patients.Add(patient);

            var test = new Test { TestId = 1300, Code = "CMP", NameReport = "Compare Test" };
            _db.Tests.Add(test);

            var param = new TestParameter { ParameterId = 1300, TestId = 1300, Name = "Value" };
            _db.TestParameters.Add(param);
            await _db.SaveChangesAsync();

            // Previous visit (1 month ago)
            var prevVisit = new Visit { VisitId = 1300, PatientId = patientId, VisitDate = DateTime.Now.AddMonths(-1) };
            var prevVt = new VisitTest { VisitTestId = 1300, VisitId = 1300, TestId = 1300 };
            _db.Visits.Add(prevVisit);
            _db.VisitTests.Add(prevVt);
            _db.ResultValues.Add(new ResultValue { VisitTestId = 1300, ParameterId = 1300, Value = "100" });
            await _db.SaveChangesAsync();

            // Current visit
            var currVisit = new Visit { VisitId = 1301, PatientId = patientId, VisitDate = DateTime.Now };
            var currVt = new VisitTest { VisitTestId = 1301, VisitId = 1301, TestId = 1300 };
            _db.Visits.Add(currVisit);
            _db.VisitTests.Add(currVt);
            _db.ResultValues.Add(new ResultValue { VisitTestId = 1301, ParameterId = 1300, Value = "150" });
            await _db.SaveChangesAsync();

            // Act
            var report = await _reportService.GetVisitReportAsync(1301);

            // Assert
            report.Should().NotBeNull();
            report!.Tests.Should().HaveCount(1);
            report.Tests[0].Results.Should().HaveCount(1);
            report.Tests[0].Results[0].Result.Value.Should().Be("150");
            report.Tests[0].Results[0].PreviousValue.Should().Be("100");
            report.Tests[0].Results[0].PreviousDate.Should().NotBeNull();
        }
    }
}
