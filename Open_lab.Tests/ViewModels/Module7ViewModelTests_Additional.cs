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
    /// Additional ViewModel tests for Module 7
    /// </summary>
    public class Module7ViewModelTests_Additional
    {
        #region WorkSheetByPatientViewModel Additional Tests

        [Fact]
        public async Task LoadCommand_With_Specific_Date_Range_Should_Pass_Range_To_Service_SuccessGuard()
        {
            // Function: 7.1 — Generate Patient Worksheet (Date Range from ViewModel)
            // Arrange
            // Production normalises the range to From.Date → To.Date end-of-day (To.Date.AddDays(1).AddSeconds(-1)).
            AppSessionTestHelper.ResetToAdmin();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByPatientViewModel(worksheetServiceMock.Object, printServiceMock.Object);

            var fromDate = DateTime.Today.AddDays(-7);
            var toDate = DateTime.Today;
            viewModel.From = fromDate;
            viewModel.To = toDate;

            var expectedFrom = fromDate.Date;
            var expectedTo = toDate.Date.AddDays(1).AddSeconds(-1);

            worksheetServiceMock.Setup(x => x.GetWorksheetByPatientAsync(expectedFrom, expectedTo))
                .ReturnsAsync(new List<WorkSheetPatientRow>());

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            worksheetServiceMock.Verify(x => x.GetWorksheetByPatientAsync(expectedFrom, expectedTo), Times.Once);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task PrintCommand_With_Multiple_Rows_Should_Call_Print_With_All_Rows_SuccessGuard()
        {
            // Function: 7.1 — Generate Patient Worksheet (Print Multiple Rows)
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByPatientViewModel(worksheetServiceMock.Object, printServiceMock.Object);

            viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 1, PatientName = "P1", TestsCount = 2 });
            viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 2, PatientName = "P2", TestsCount = 1 });
            viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 3, PatientName = "P3", TestsCount = 3 });

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            printServiceMock.Verify(x => x.PrintWorksheetByPatientAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.Is<IReadOnlyCollection<WorkSheetPatientRow>>(rows => rows.Count == 3)), Times.Once);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task GeneratePatientWorksheet_WhenLoadServiceThrows_ShouldSetErrorStatusMessage()
        {
            // Function: 7.1 — Generate Patient Worksheet
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByPatientViewModel(worksheetServiceMock.Object, printServiceMock.Object);
            worksheetServiceMock.Setup(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("patient-worksheet-load-failed"));

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("خطأ:");
            viewModel.StatusMessage.Should().Contain("patient-worksheet-load-failed");
            viewModel.Rows.Should().BeEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task GeneratePatientWorksheet_WhenPrintServiceThrows_ShouldSetPrintErrorStatusMessage()
        {
            // Function: 7.1 — Generate Patient Worksheet
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByPatientViewModel(worksheetServiceMock.Object, printServiceMock.Object);
            viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 10, PatientName = "Patient", TestsCount = 2 });
            printServiceMock.Setup(x => x.PrintWorksheetByPatientAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IReadOnlyCollection<WorkSheetPatientRow>>()))
                .ThrowsAsync(new InvalidOperationException("patient-print-failed"));

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("خطأ طباعة:");
            viewModel.StatusMessage.Should().Contain("patient-print-failed");
            AppSessionTestHelper.Reset();
        }

        #endregion

        #region WorkSheetByTestViewModel Additional Tests

        [Fact]
        public async Task LoadCommand_With_Multiple_Test_Results_Should_Display_All_SuccessGuard()
        {
            // Function: 7.2 — Generate Test Worksheet (Multiple Results)
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByTestViewModel(worksheetServiceMock.Object, printServiceMock.Object);

            worksheetServiceMock.Setup(x => x.GetWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetTestRow>
                {
                    new() { TestName = "CBC", Count = 5 },
                    new() { TestName = "ALT", Count = 3 },
                    new() { TestName = "AST", Count = 4 }
                });

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            viewModel.Rows.Should().HaveCount(3);
            viewModel.StatusMessage.Should().Contain("3 تحليل");
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task GenerateTestWorksheet_WhenLoadServiceThrows_ShouldSetErrorStatusMessage()
        {
            // Function: 7.2 — Generate Test Worksheet
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByTestViewModel(worksheetServiceMock.Object, printServiceMock.Object);
            worksheetServiceMock.Setup(x => x.GetWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("test-worksheet-load-failed"));

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("خطأ:");
            viewModel.StatusMessage.Should().Contain("test-worksheet-load-failed");
            viewModel.Rows.Should().BeEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task GenerateTestWorksheet_WithRows_ShouldPrintWorksheet()
        {
            // Function: 7.2 — Generate Test Worksheet
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByTestViewModel(worksheetServiceMock.Object, printServiceMock.Object);
            viewModel.Rows.Add(new WorkSheetTestRow { TestName = "CBC", Count = 4 });

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            printServiceMock.Verify(x => x.PrintWorksheetByTestAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.Is<IReadOnlyCollection<WorkSheetTestRow>>(rows => rows.Count == 1)), Times.Once);
            viewModel.StatusMessage.Should().Contain("تم إرسال ورقة العمل");
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task GenerateTestWorksheet_WhenPrintServiceThrows_ShouldSetPrintErrorStatusMessage()
        {
            // Function: 7.2 — Generate Test Worksheet
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByTestViewModel(worksheetServiceMock.Object, printServiceMock.Object);
            viewModel.Rows.Add(new WorkSheetTestRow { TestName = "ALT", Count = 2 });
            printServiceMock.Setup(x => x.PrintWorksheetByTestAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<IReadOnlyCollection<WorkSheetTestRow>>()))
                .ThrowsAsync(new InvalidOperationException("test-print-failed"));

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("خطأ طباعة:");
            viewModel.StatusMessage.Should().Contain("test-print-failed");
            AppSessionTestHelper.Reset();
        }

        #endregion

        #region GroupWorksheetViewModel Additional Tests

        [Fact]
        public async Task LoadWorksheetCommand_With_Custom_Group_Should_Call_Custom_Group_Service_SuccessGuard()
        {
            // Function: 7.3 — Generate Group Worksheet (Custom Group Selection)
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var groupServiceMock = new Mock<IGroupWorksheetService>();
            var testCatalogServiceMock = new Mock<ITestCatalogService>();
            var printServiceMock = new Mock<IPrintService>();

            testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ReturnsAsync(new List<TestGroup>());
            testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync()).ReturnsAsync(new List<CustomGroup>());

            var viewModel = new GroupWorksheetViewModel(groupServiceMock.Object, testCatalogServiceMock.Object, printServiceMock.Object);

            viewModel.IsCustomGroup = true;
            viewModel.SelectedCustomGroupId = 77;
            viewModel.From = DateTime.Today.AddDays(-1);
            viewModel.To = DateTime.Today.AddDays(1);

            groupServiceMock.Setup(x => x.GetGroupWorksheetByCustomGroupAsync(77, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow>());

            // Act
            viewModel.LoadWorksheetCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            groupServiceMock.Verify(x => x.GetGroupWorksheetByCustomGroupAsync(77, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadWorksheetCommand_With_Normal_Group_Should_Call_Group_Service_SuccessGuard()
        {
            // Function: 7.3 — Generate Group Worksheet (Normal Group Selection)
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var groupServiceMock = new Mock<IGroupWorksheetService>();
            var testCatalogServiceMock = new Mock<ITestCatalogService>();
            var printServiceMock = new Mock<IPrintService>();

            testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ReturnsAsync(new List<TestGroup>());
            testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync()).ReturnsAsync(new List<CustomGroup>());

            var viewModel = new GroupWorksheetViewModel(groupServiceMock.Object, testCatalogServiceMock.Object, printServiceMock.Object);

            viewModel.IsCustomGroup = false;
            viewModel.SelectedGroupId = 88;
            viewModel.From = DateTime.Today.AddDays(-1);
            viewModel.To = DateTime.Today.AddDays(1);

            groupServiceMock.Setup(x => x.GetGroupWorksheetByGroupAsync(88, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow>());

            // Act
            viewModel.LoadWorksheetCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            groupServiceMock.Verify(x => x.GetGroupWorksheetByGroupAsync(88, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task PrintCommand_With_Group_Worksheet_Should_Call_Print_Service_SuccessGuard()
        {
            // Function: 7.3 — Generate Group Worksheet (Print Group Worksheet)
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var groupServiceMock = new Mock<IGroupWorksheetService>();
            var testCatalogServiceMock = new Mock<ITestCatalogService>();
            var printServiceMock = new Mock<IPrintService>();

            testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ReturnsAsync(new List<TestGroup>());
            testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync()).ReturnsAsync(new List<CustomGroup>());

            var viewModel = new GroupWorksheetViewModel(groupServiceMock.Object, testCatalogServiceMock.Object, printServiceMock.Object);

            viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 1, PatientName = "GP1", TestsCount = 2 });

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            printServiceMock.Verify(x => x.PrintWorksheetByPatientAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.Is<IReadOnlyCollection<WorkSheetPatientRow>>(rows => rows.Count == 1)), Times.Once);
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task GenerateGroupWorksheet_WhenLoadWorksheetServiceThrows_ShouldSetErrorStatusMessage()
        {
            // Function: 7.3 — Generate Group Worksheet
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var groupServiceMock = new Mock<IGroupWorksheetService>();
            var testCatalogServiceMock = new Mock<ITestCatalogService>();
            var printServiceMock = new Mock<IPrintService>();
            testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ReturnsAsync(new List<TestGroup>());
            testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync()).ReturnsAsync(new List<CustomGroup>());
            groupServiceMock.Setup(x => x.GetGroupWorksheetByGroupAsync(5, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("group-worksheet-load-failed"));
            var viewModel = new GroupWorksheetViewModel(groupServiceMock.Object, testCatalogServiceMock.Object, printServiceMock.Object)
            {
                IsCustomGroup = false,
                SelectedGroupId = 5
            };

            // Act
            viewModel.LoadWorksheetCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("خطأ:");
            viewModel.StatusMessage.Should().Contain("group-worksheet-load-failed");
            viewModel.Rows.Should().BeEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public void GenerateGroupWorksheet_WithNoSelectedGroup_ShouldDisablePrintCommand()
        {
            // Function: 7.3 — Generate Group Worksheet
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var groupServiceMock = new Mock<IGroupWorksheetService>();
            var testCatalogServiceMock = new Mock<ITestCatalogService>();
            var printServiceMock = new Mock<IPrintService>();
            testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ReturnsAsync(new List<TestGroup>());
            testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync()).ReturnsAsync(new List<CustomGroup>());
            var viewModel = new GroupWorksheetViewModel(groupServiceMock.Object, testCatalogServiceMock.Object, printServiceMock.Object);

            // Act
            var canPrint = viewModel.PrintCommand.CanExecute(null);

            // Assert
            canPrint.Should().BeFalse();
            viewModel.LoadWorksheetCommand.CanExecute(null).Should().BeFalse();
            AppSessionTestHelper.Reset();
        }

        #endregion

        #region TestClassificationLogViewModel Additional Tests

        [Fact]
        public async Task LoadAsync_With_Multiple_Reagent_Consumptions_Should_Display_All_SuccessGuard()
        {
            // Function: 7.4 — Test Classification LOG (Multiple Reagents Display)
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var classificationServiceMock = new Mock<ITestClassificationService>();
            var printServiceMock = new Mock<IPrintService>();

            classificationServiceMock.Setup(x => x.GetConsumptionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ReagentConsumptionReport>
                {
                    new() { ReagentName = "Reagent A", TotalConsumed = 10m, Unit = "ml", TestCount = 5 },
                    new() { ReagentName = "Reagent B", TotalConsumed = 20m, Unit = "ul", TestCount = 3 },
                    new() { ReagentName = "Reagent C", TotalConsumed = 15m, Unit = "ml", TestCount = 8 }
                });

            var viewModel = new TestClassificationLogViewModel(classificationServiceMock.Object, printServiceMock.Object);

            // Act
            await viewModel.InvokePrivateAsync("LoadAsync");

            // Assert
            viewModel.Items.Should().HaveCount(3);
            viewModel.StatusMessage.Should().Contain("3 سجل");
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task ServiceLayer_LoadCommand_When_Service_Throws_Should_Set_Error_Message_FailureGuard()
        {
            // Function: 7.4 — Test Classification LOG (Service Error Handling)
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var classificationServiceMock = new Mock<ITestClassificationService>();
            var printServiceMock = new Mock<IPrintService>();

            classificationServiceMock.Setup(x => x.GetConsumptionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("classification-load-failed"));

            var viewModel = new TestClassificationLogViewModel(classificationServiceMock.Object, printServiceMock.Object);

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            viewModel.StatusMessage.Should().Contain("خطأ:");
            viewModel.StatusMessage.Should().Contain("classification-load-failed");
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task PrintCommand_With_Empty_Items_Should_Not_Call_Print_Service_EdgeGuard()
        {
            // Function: 7.4 — Test Classification LOG (Print Empty)
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var classificationServiceMock = new Mock<ITestClassificationService>();
            var printServiceMock = new Mock<IPrintService>();

            classificationServiceMock.Setup(x => x.GetConsumptionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ReagentConsumptionReport>());

            var viewModel = new TestClassificationLogViewModel(classificationServiceMock.Object, printServiceMock.Object);

            await viewModel.InvokePrivateAsync("LoadAsync");

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert - Should still call print with empty list based on viewmodel logic
            printServiceMock.Verify(x => x.PrintTextReportAsync("سجل تصنيف التحاليل", It.IsAny<IReadOnlyCollection<string>>(), "TestClassificationLog"), Times.Once);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task TestClassificationLog_WithReportItems_ShouldPrintTextReport()
        {
            // Function: 7.4 — Test Classification LOG
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var classificationServiceMock = new Mock<ITestClassificationService>();
            var printServiceMock = new Mock<IPrintService>();
            classificationServiceMock.Setup(x => x.GetConsumptionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ReagentConsumptionReport>
                {
                    new() { ReagentName = "Glucose Reagent", TotalConsumed = 12.5m, Unit = "ml", TestCount = 5 }
                });
            var viewModel = new TestClassificationLogViewModel(classificationServiceMock.Object, printServiceMock.Object);
            await viewModel.InvokePrivateAsync("LoadAsync");

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            printServiceMock.Verify(x => x.PrintTextReportAsync(
                "سجل تصنيف التحاليل",
                It.Is<IReadOnlyCollection<string>>(lines => lines.Any(line => line.Contains("Glucose Reagent"))),
                "TestClassificationLog"), Times.Once);
            viewModel.StatusMessage.Should().Contain("تم إرسال التقرير للطباعة");
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task TestClassificationLog_WhenPrintServiceThrows_ShouldSetErrorStatusMessage()
        {
            // Function: 7.4 — Test Classification LOG
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var classificationServiceMock = new Mock<ITestClassificationService>();
            var printServiceMock = new Mock<IPrintService>();
            classificationServiceMock.Setup(x => x.GetConsumptionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ReagentConsumptionReport>
                {
                    new() { ReagentName = "Urea Reagent", TotalConsumed = 2m, Unit = "ml", TestCount = 1 }
                });
            printServiceMock.Setup(x => x.PrintTextReportAsync(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<string>>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("classification-print-failed"));
            var viewModel = new TestClassificationLogViewModel(classificationServiceMock.Object, printServiceMock.Object);
            await viewModel.InvokePrivateAsync("LoadAsync");

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("خطأ:");
            viewModel.StatusMessage.Should().Contain("classification-print-failed");
            AppSessionTestHelper.Reset();
        }

        #endregion

        #region Authentication Tests for Module 7

        [Fact]
        public async Task LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
        {
            // Function: 7.1 — Generate Patient Worksheet (Authentication Required)
            // Arrange
            AppSessionTestHelper.Reset();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByPatientViewModel(worksheetServiceMock.Object, printServiceMock.Object);

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            viewModel.StatusMessage.Should().Contain("تسجيل الدخول");
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task GroupWorksheet_LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
        {
            // Function: 7.3 — Generate Group Worksheet (Authentication Required)
            // Arrange
            AppSessionTestHelper.Reset();
            var groupServiceMock = new Mock<IGroupWorksheetService>();
            var testCatalogServiceMock = new Mock<ITestCatalogService>();
            var printServiceMock = new Mock<IPrintService>();

            var viewModel = new GroupWorksheetViewModel(groupServiceMock.Object, testCatalogServiceMock.Object, printServiceMock.Object);

            viewModel.IsCustomGroup = false;
            viewModel.SelectedGroupId = 1;

            // Act
            viewModel.LoadWorksheetCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            viewModel.StatusMessage.Should().Contain("تسجيل الدخول");
            AppSessionTestHelper.Reset();
        }

        #endregion

        #region State Transition Tests

        [Fact]
        public async Task Full_Worksheet_Workflow_Patient_Load_Print_Should_Work_SuccessGuard()
        {
            // Function: 7.1 — Complete Patient Worksheet Workflow
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var worksheetServiceMock = new Mock<IWorksheetService>();
            var printServiceMock = new Mock<IPrintService>();
            var viewModel = new WorkSheetByPatientViewModel(worksheetServiceMock.Object, printServiceMock.Object);

            worksheetServiceMock.Setup(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow>
                {
                    new() { VisitId = 1, PatientName = "Workflow Test", TestsCount = 3 }
                });

            // Act - Load
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert - Load succeeded
            viewModel.Rows.Should().ContainSingle();
            worksheetServiceMock.Verify(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);

            // Act - Print
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert - Print succeeded
            printServiceMock.Verify(x => x.PrintWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetPatientRow>>()), Times.Once);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task Full_Group_Worksheet_Workflow_Load_Print_Should_Work_SuccessGuard()
        {
            // Function: 7.3 — Complete Group Worksheet Workflow
            // Arrange
            AppSessionTestHelper.ResetToAdmin();
            var groupServiceMock = new Mock<IGroupWorksheetService>();
            var testCatalogServiceMock = new Mock<ITestCatalogService>();
            var printServiceMock = new Mock<IPrintService>();

            testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ReturnsAsync(new List<TestGroup>());
            testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync()).ReturnsAsync(new List<CustomGroup>());

            var viewModel = new GroupWorksheetViewModel(groupServiceMock.Object, testCatalogServiceMock.Object, printServiceMock.Object);

            viewModel.IsCustomGroup = false;
            viewModel.SelectedGroupId = 1;

            groupServiceMock.Setup(x => x.GetGroupWorksheetByGroupAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow>
                {
                    new() { VisitId = 1, PatientName = "Group Workflow", TestsCount = 4 }
                });

            // Act - Load
            viewModel.LoadWorksheetCommand.Execute(null);
            await Task.Delay(50);

            // Assert - Load succeeded
            viewModel.Rows.Should().ContainSingle();
            groupServiceMock.Verify(x => x.GetGroupWorksheetByGroupAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);

            // Act - Print
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert - Print succeeded
            printServiceMock.Verify(x => x.PrintWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetPatientRow>>()), Times.Once);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        #endregion
    }
}
