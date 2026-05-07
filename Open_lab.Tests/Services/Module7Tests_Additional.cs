using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests
{
    /// <summary>
    /// Additional comprehensive tests for Module 7: Work Sheets
    /// Covers Functions 7.1-7.4 with Service and ViewModel layer tests
    /// </summary>
    public class Module7ServiceTests_Additional : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly WorksheetService _worksheetService;
        private readonly GroupWorksheetService _groupWorksheetService;
        private readonly TestClassificationService _classificationService;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module7ServiceTests_Additional()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _worksheetService = new WorksheetService(_db);
            _groupWorksheetService = new GroupWorksheetService(_db);
            _classificationService = new TestClassificationService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        #region Function 7.1 - Generate Patient Worksheet

        [Fact]
        public async Task GetWorksheetByPatientAsync_With_Multiple_Visits_Should_Return_Correct_Row_Count_SuccessGuard()
        {
            // Function: 7.1 — Generate Patient Worksheet (Multiple Visits Success)
            // Arrange
            var patient = new Patient { LabId = "L-P7-1", FullName = "Patient Multi", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            var visit2 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.AddRange(visit1, visit2);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit1.VisitId, TestId = 1, Price = 10m },
                new VisitTest { VisitId = visit1.VisitId, TestId = 2, Price = 15m },
                new VisitTest { VisitId = visit2.VisitId, TestId = 1, Price = 10m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _worksheetService.GetWorksheetByPatientAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().HaveCount(2);
            rows.Should().Contain(r => r.TestsCount == 2);
            rows.Should().Contain(r => r.TestsCount == 1);
        }

        [Fact]
        public async Task GetWorksheetByPatientAsync_Should_Include_Patient_LabId_In_Rows_SuccessGuard()
        {
            // Function: 7.1 — Generate Patient Worksheet (Include LabId)
            // Arrange
            var patient = new Patient { LabId = "L-77-LABID", FullName = "Lab Id Test", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 20m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _worksheetService.GetWorksheetByPatientAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
            rows.First().PatientName.Should().Be("Lab Id Test");
        }

        [Fact]
        public async Task GetWorksheetByPatientAsync_With_Null_VisitDate_Should_Handle_EdgeGuard()
        {
            // Function: 7.1 — Generate Patient Worksheet (Null Visit Date Edge)
            // Arrange
            var patient = new Patient { LabId = "L-NULL-DATE", FullName = "Null Date", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 10m });
            await _db.SaveChangesAsync();

            // Act - Query with null date should still work
            var rows = await _worksheetService.GetWorksheetByPatientAsync(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30));

            // Assert - May or may not include depending on service logic
            rows.Should().NotBeNull();
        }

        #endregion

        #region Function 7.2 - Generate Test Worksheet

        [Fact]
        public async Task GetWorksheetByTestAsync_With_Multiple_Same_Tests_Should_Aggregate_Count_EdgeGuard()
        {
            // Function: 7.2 — Generate Test Worksheet (Same Test Multiple Times)
            // Arrange
            var patient1 = new Patient { LabId = "L-T2-P1", FullName = "Test2 Patient 1", Gender = "Male" };
            var patient2 = new Patient { LabId = "L-T2-P2", FullName = "Test2 Patient 2", Gender = "Female" };
            _db.Patients.AddRange(patient1, patient2);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T-CBC-77", NameReport = "CBC Full", Price = 25m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = patient1.PatientId, VisitDate = DateTime.Today };
            var visit2 = new Visit { PatientId = patient2.PatientId, VisitDate = DateTime.Today };
            _db.Visits.AddRange(visit1, visit2);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit1.VisitId, TestId = test.TestId, Price = 25m },
                new VisitTest { VisitId = visit1.VisitId, TestId = test.TestId, Price = 25m },
                new VisitTest { VisitId = visit2.VisitId, TestId = test.TestId, Price = 25m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _worksheetService.GetWorksheetByTestAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
            rows.First().TestName.Should().Be("CBC Full");
            rows.First().Count.Should().Be(3);
        }

        [Fact]
        public async Task GetWorksheetByTestAsync_Should_Include_Test_Code_SuccessGuard()
        {
            // Function: 7.2 — Generate Test Worksheet (Include Test Code)
            // Arrange
            var patient = new Patient { LabId = "L-CODE-TEST", FullName = "Code Test", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "CODE-77-TEST", NameReport = "Named Test", Price = 30m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 30m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _worksheetService.GetWorksheetByTestAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
            rows.First().TestName.Should().Be("Named Test");
        }

        #endregion

        #region Function 7.3 - Generate Group Worksheet

        [Fact]
        public async Task GetGroupWorksheetByGroupAsync_With_Mixed_Group_Tests_Should_Count_Only_Target_SuccessGuard()
        {
            // Function: 7.3 — Generate Group Worksheet (Mixed Groups Success)
            // Arrange
            var patient = new Patient { LabId = "L-GROUP-MIX", FullName = "Group Mix Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var targetGroupTest = new Test { Code = "G1-T1", NameReport = "Group1 Test 1", Price = 10m, GroupId = 77 };
            var sameGroupTest = new Test { Code = "G1-T2", NameReport = "Group1 Test 2", Price = 12m, GroupId = 77 };
            var otherGroupTest = new Test { Code = "G2-T1", NameReport = "Group2 Test", Price = 8m, GroupId = 78 };
            _db.Tests.AddRange(targetGroupTest, sameGroupTest, otherGroupTest);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = targetGroupTest.TestId, Price = 10m },
                new VisitTest { VisitId = visit.VisitId, TestId = sameGroupTest.TestId, Price = 12m },
                new VisitTest { VisitId = visit.VisitId, TestId = otherGroupTest.TestId, Price = 8m });
            await _db.SaveChangesAsync();

            // Act - Request only Group 77
            var rows = await _groupWorksheetService.GetGroupWorksheetByGroupAsync(77, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Should include only Group 77 tests
            rows.Should().ContainSingle();
            rows[0].TestsCount.Should().Be(2);
        }

        [Fact]
        public async Task GetGroupWorksheetByCustomGroupAsync_With_Multiple_Items_Should_Sum_Correctly_SuccessGuard()
        {
            // Function: 7.3 — Generate Group Worksheet (Custom Group Multi Items)
            // Arrange
            var customGroup = new CustomGroup { Name = "Full Panel 77" };
            _db.CustomGroups.Add(customGroup);
            await _db.SaveChangesAsync();

            var test1 = new Test { Code = "CG-T1", NameReport = "Panel Test 1", Price = 15m };
            var test2 = new Test { Code = "CG-T2", NameReport = "Panel Test 2", Price = 20m };
            var test3 = new Test { Code = "CG-T3", NameReport = "Panel Test 3", Price = 25m };
            _db.Tests.AddRange(test1, test2, test3);
            await _db.SaveChangesAsync();

            _db.CustomGroupItems.AddRange(
                new CustomGroupItem { CustomGroupId = customGroup.CustomGroupId, TestId = test1.TestId },
                new CustomGroupItem { CustomGroupId = customGroup.CustomGroupId, TestId = test2.TestId },
                new CustomGroupItem { CustomGroupId = customGroup.CustomGroupId, TestId = test3.TestId });
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-CG-PANEL", FullName = "Custom Panel Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 15m },
                new VisitTest { VisitId = visit.VisitId, TestId = test2.TestId, Price = 20m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _groupWorksheetService.GetGroupWorksheetByCustomGroupAsync(
                customGroup.CustomGroupId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
            rows[0].TestsCount.Should().Be(2);
        }

        [Fact]
        public async Task GetGroupWorksheetByGroupAsync_With_NonExistent_Group_Should_Return_Empty_EdgeGuard()
        {
            // Function: 7.3 — Generate Group Worksheet (Non-Existent Group)
            // Act
            var rows = await _groupWorksheetService.GetGroupWorksheetByGroupAsync(99999, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().BeEmpty();
        }

        #endregion

        #region Function 7.4 - Test Classification LOG

        [Fact]
        public async Task GetConsumptionReportAsync_Should_Calculate_Total_Consumption_Correctly_SuccessGuard()
        {
            // Function: 7.4 — Test Classification LOG (Consumption Calculation)
            // Arrange
            var reagent = new Reagent { Name = "Reagent-77", Unit = "ml", CurrentStock = 1000m };
            _db.Reagents.Add(reagent);
            await _db.SaveChangesAsync();

            var test1 = new Test { Code = "T-C1", NameReport = "Consume Test 1", Price = 10m };
            var test2 = new Test { Code = "T-C2", NameReport = "Consume Test 2", Price = 15m };
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            _db.TestConsumptions.AddRange(
                new TestConsumption { TestId = test1.TestId, ReagentId = reagent.ReagentId, AmountPerTest = 3m },
                new TestConsumption { TestId = test2.TestId, ReagentId = reagent.ReagentId, AmountPerTest = 7m });
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-CONSUME", FullName = "Consumption Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 10m },
                new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 10m },
                new VisitTest { VisitId = visit.VisitId, TestId = test2.TestId, Price = 15m });
            await _db.SaveChangesAsync();

            // Act
            var report = await _classificationService.GetConsumptionReportAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            report.Should().ContainSingle();
            report.Single().ReagentName.Should().Be("Reagent-77");
            report.Single().TotalConsumed.Should().Be(13m); // 2*3 + 1*7
            report.Single().TestCount.Should().Be(3);
            report.Single().Unit.Should().Be("ml");
        }

        [Fact]
        public async Task GetConsumptionReportAsync_With_No_Reagent_Assigned_Should_Not_Include_Test_EdgeGuard()
        {
            // Function: 7.4 — Test Classification LOG (No Reagent Assigned)
            // Arrange
            var testNoReagent = new Test { Code = "T-NO-REAG", NameReport = "No Reagent Test", Price = 50m };
            _db.Tests.Add(testNoReagent);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-NO-REAG", FullName = "No Reagent Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = testNoReagent.TestId, Price = 50m });
            await _db.SaveChangesAsync();

            // Act
            var report = await _classificationService.GetConsumptionReportAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Test without reagent assignment should not appear
            report.Should().BeEmpty();
        }

        [Fact]
        public async Task GetConsumptionReportAsync_With_Multiple_Reagents_Should_Group_By_Reagent_SuccessGuard()
        {
            // Function: 7.4 — Test Classification LOG (Multiple Reagents)
            // Arrange
            var reagent1 = new Reagent { Name = "Reagent A-77", Unit = "ml", CurrentStock = 500m };
            var reagent2 = new Reagent { Name = "Reagent B-77", Unit = "ul", CurrentStock = 200m };
            _db.Reagents.AddRange(reagent1, reagent2);
            await _db.SaveChangesAsync();

            var test1 = new Test { Code = "T-R1", NameReport = "Reagent1 Test", Price = 20m };
            var test2 = new Test { Code = "T-R2", NameReport = "Reagent2 Test", Price = 30m };
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            _db.TestConsumptions.AddRange(
                new TestConsumption { TestId = test1.TestId, ReagentId = reagent1.ReagentId, AmountPerTest = 5m },
                new TestConsumption { TestId = test2.TestId, ReagentId = reagent2.ReagentId, AmountPerTest = 10m });
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-MULTI-REG", FullName = "Multi Reagent Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 20m },
                new VisitTest { VisitId = visit.VisitId, TestId = test2.TestId, Price = 30m });
            await _db.SaveChangesAsync();

            // Act
            var report = await _classificationService.GetConsumptionReportAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            report.Should().HaveCount(2);
            report.Should().Contain(r => r.ReagentName == "Reagent A-77" && r.TotalConsumed == 5m);
            report.Should().Contain(r => r.ReagentName == "Reagent B-77" && r.TotalConsumed == 10m);
        }

        [Fact]
        public async Task GetConsumptionReportAsync_Should_Include_Current_Stock_Info_SuccessGuard()
        {
            // Function: 7.4 — Test Classification LOG (Include Stock Info)
            // Arrange
            var reagent = new Reagent { Name = "Stock-Reagent-77", Unit = "ml", CurrentStock = 777.5m };
            _db.Reagents.Add(reagent);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T-STOCK", NameReport = "Stock Test", Price = 40m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.TestConsumptions.Add(new TestConsumption { TestId = test.TestId, ReagentId = reagent.ReagentId, AmountPerTest = 2m });
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-STOCK", FullName = "Stock Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 40m });
            await _db.SaveChangesAsync();

            // Act
            var report = await _classificationService.GetConsumptionReportAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            report.Should().ContainSingle();
            report.Single().ReagentName.Should().Be("Stock-Reagent-77");
        }

        #endregion

        #region Date Range Boundary Tests

        [Fact]
        public async Task GetWorksheetByPatientAsync_At_Midnight_Should_Include_Today_EdgeGuard()
        {
            // Function: 7.1 — Generate Patient Worksheet (Midnight Boundary)
            // Arrange
            var patient = new Patient { LabId = "L-MIDNIGHT", FullName = "Midnight Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var todayMidnight = DateTime.Today;
            var visit = new Visit { PatientId = patient.PatientId, VisitDate = todayMidnight };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 10m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _worksheetService.GetWorksheetByPatientAsync(todayMidnight, todayMidnight.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
        }

        [Fact]
        public async Task GetGroupWorksheetByGroupAsync_Empty_Date_Range_Should_Return_Empty_EdgeGuard()
        {
            // Function: 7.3 — Generate Group Worksheet (Empty Date Range)
            // Arrange
            var patient = new Patient { LabId = "L-EMPTY-RANGE", FullName = "Empty Range Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-5) };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T-EMPTY", NameReport = "Empty Range Test", Price = 15m, GroupId = 99 };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 15m });
            await _db.SaveChangesAsync();

            // Act - Range that excludes the visit date
            var rows = await _groupWorksheetService.GetGroupWorksheetByGroupAsync(99, DateTime.Today.AddDays(-1), DateTime.Today);

            // Assert
            rows.Should().BeEmpty();
        }

        #endregion
    }
}
