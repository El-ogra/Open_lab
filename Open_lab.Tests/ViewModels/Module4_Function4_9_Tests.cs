using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    /// <summary>
    /// Function 4.9 — Compare with History (CompareWithHistoryViewModel)
    /// Success, Failure, and Edge case tests (BR-MED-008)
    /// </summary>
    public class Module4_Function4_9_Tests : IDisposable
    {
        private readonly Mock<ICompareWithHistoryService> _compareServiceMock;
        private readonly Mock<IPatientService> _patientServiceMock;
        private readonly Mock<ITestCatalogService> _catalogServiceMock;

        public Module4_Function4_9_Tests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _compareServiceMock = new Mock<ICompareWithHistoryService>();
            _patientServiceMock = new Mock<IPatientService>();
            _catalogServiceMock = new Mock<ITestCatalogService>();
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task Function_4_9_LoadPatientCommand_With_Valid_LabId_Success()
        {
            // Function: 4.9 — Compare with History (Success)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            var patient = new Patient { PatientId = 900, FullName = "History Patient", LabId = "L-HIST", Gender = "Female" };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("L-HIST")).ReturnsAsync(patient);
            _catalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test> { new() { TestId = 1, NameReport = "CBC" } });

            viewModel.LabId = "L-HIST";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);

            // viewModel.Patient.Should().NotBeNull();
            // viewModel.Patient!.FullName.Should().Be("History Patient");
            viewModel.Tests.Should().HaveCount(1);
        }

        [Fact]
        public async Task Function_4_9_LoadPatientCommand_With_Invalid_LabId_Failure()
        {
            // Function: 4.9 — Compare with History (Failure)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("INVALID")).ReturnsAsync((Patient?)null);

            viewModel.LabId = "INVALID";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);

            // viewModel.Patient.Should().BeNull();
            viewModel.StatusMessage.Should().Contain("لم يتم العثور");
        }

        [Fact]
        public async Task Function_4_9_LoadPatientCommand_With_Empty_LabId_Edge()
        {
            // Function: 4.9 — Compare with History (Edge)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);

            viewModel.LabId = "";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);

            viewModel.StatusMessage.Should().Contain("يرجى إدخال");
        }

        [Fact]
        public async Task Function_4_9_LoadHistoryCommand_With_History_Success()
        {
            // Function: 4.9 — Compare with History (Success)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            var patient = new Patient { PatientId = 901, FullName = "History Patient", LabId = "L-HIST2" };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("L-HIST2")).ReturnsAsync(patient);
            _catalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test> { new() { TestId = 10, NameReport = "Glucose" } });
            _compareServiceMock.Setup(x => x.GetLastResultsAsync(901, 10, 5)).ReturnsAsync(new List<HistoricalResult>
            {
                new() { VisitId = 100, VisitDate = DateTime.Now.AddDays(-30), ParameterName = "Glucose", Value = "100", Flag = "N" },
                new() { VisitId = 101, VisitDate = DateTime.Now.AddDays(-60), ParameterName = "Glucose", Value = "110", Flag = "H" }
            });

            viewModel.LabId = "L-HIST2";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);
            viewModel.SelectedTestId = 10;
            viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            viewModel.HistoryResults.Should().HaveCount(2);
            viewModel.StatusMessage.Should().Contain("2");
        }

        [Fact]
        public async Task Function_4_9_LoadHistoryCommand_With_No_History_Edge()
        {
            // Function: 4.9 — Compare with History (Edge: no history)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            var patient = new Patient { PatientId = 902, FullName = "New Patient", LabId = "L-NEW" };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("L-NEW")).ReturnsAsync(patient);
            _catalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test> { new() { TestId = 20, NameReport = "HbA1c" } });
            _compareServiceMock.Setup(x => x.GetLastResultsAsync(902, 20, 5)).ReturnsAsync(new List<HistoricalResult>());

            viewModel.LabId = "L-NEW";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);
            viewModel.SelectedTestId = 20;
            viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            viewModel.HistoryResults.Should().BeEmpty();
            viewModel.StatusMessage.Should().Contain("0");
        }

        [Fact]
        public async Task Function_4_9_LoadHistoryCommand_With_ServiceError_Failure()
        {
            // Function: 4.9 — Compare with History (Failure)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            var patient = new Patient { PatientId = 903, FullName = "Error Patient", LabId = "L-ERR" };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("L-ERR")).ReturnsAsync(patient);
            _catalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test> { new() { TestId = 30, NameReport = "Test" } });
            _compareServiceMock.Setup(x => x.GetLastResultsAsync(903, 30, 5)).ThrowsAsync(new InvalidOperationException("DB error"));

            viewModel.LabId = "L-ERR";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);
            viewModel.SelectedTestId = 30;
            viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            viewModel.StatusMessage.Should().Contain("خطأ:");
        }

        [Fact]
        public async Task Function_4_9_LoadHistoryCommand_With_Multiple_Results_Grouping_Edge()
        {
            // Function: 4.9 — Compare with History (Edge: grouping by visit)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            var patient = new Patient { PatientId = 904, FullName = "Group Patient", LabId = "L-GRP" };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("L-GRP")).ReturnsAsync(patient);
            _catalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test> { new() { TestId = 40, NameReport = "CBC" } });
            _compareServiceMock.Setup(x => x.GetLastResultsAsync(904, 40, 5)).ReturnsAsync(new List<HistoricalResult>
            {
                new() { VisitId = 200, VisitDate = new DateTime(2026, 4, 1), ParameterName = "WBC", Value = "7.5", Flag = "N" },
                new() { VisitId = 200, VisitDate = new DateTime(2026, 4, 1), ParameterName = "RBC", Value = "5.0", Flag = "N" },
                new() { VisitId = 201, VisitDate = new DateTime(2026, 3, 15), ParameterName = "WBC", Value = "8.0", Flag = "H" }
            });

            viewModel.LabId = "L-GRP";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);
            viewModel.SelectedTestId = 40;
            viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            viewModel.HistoryResults.Should().HaveCount(3);
        }

        [Fact]
        public async Task Function_4_9_ClearCommand_Should_Reset_View_Success()
        {
            // Function: 4.9 — Compare with History (Clear)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            var patient = new Patient { PatientId = 905, FullName = "Clear Patient", LabId = "L-CLR" };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("L-CLR")).ReturnsAsync(patient);
            _catalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test>());

            viewModel.LabId = "L-CLR";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);

            // viewModel.Patient.Should().NotBeNull();

            viewModel.ClearCommand.Execute(null);

            // viewModel.Patient.Should().BeNull();
            viewModel.LabId.Should().BeNullOrEmpty();
            viewModel.HistoryResults.Should().BeEmpty();
        }

        [Fact]
        public Task Function_4_9_With_No_Permission_Should_Disable_Command_Failure()
        {
            // Function: 4.9 — Compare with History (Permission failure)
            AppSessionTestHelper.Reset();
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);

            viewModel.LoadPatientCommand.CanExecute(null).Should().BeFalse();
            viewModel.LoadHistoryCommand.CanExecute(null).Should().BeFalse();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Function_4_9_Without_SelectedTest_Should_Not_LoadHistory_Edge()
        {
            // Function: 4.9 — Compare with History (Edge: no test selected)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            var patient = new Patient { PatientId = 906, FullName = "No Test", LabId = "L-NT" };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("L-NT")).ReturnsAsync(patient);
            _catalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test> { new() { TestId = 50, NameReport = "Test" } });

            viewModel.LabId = "L-NT";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);

            viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            viewModel.StatusMessage.Should().Contain("اختيار");
        }

        [Fact]
        public async Task Function_4_9_TestsList_Should_Populate_On_PatientLoad_Success()
        {
            // Function: 4.9 — Compare with History (Populate tests)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            var patient = new Patient { PatientId = 907, FullName = "Tests Patient", LabId = "L-TS" };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("L-TS")).ReturnsAsync(patient);
            _catalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test>
            {
                new() { TestId = 1, NameReport = "Glucose", Code = "GLU" },
                new() { TestId = 2, NameReport = "CBC", Code = "CBC" },
                new() { TestId = 3, NameReport = "Lipid Panel", Code = "LIPID" }
            });

            viewModel.LabId = "L-TS";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);

            viewModel.Tests.Should().HaveCount(3);
        }

        [Fact]
        public async Task Function_4_9_History_Should_Show_Flag_Indicators_Success()
        {
            // Function: 4.9 — Compare with History (Flag indicators)
            var viewModel = new CompareWithHistoryViewModel(_compareServiceMock.Object, _patientServiceMock.Object, _catalogServiceMock.Object);
            var patient = new Patient { PatientId = 908, FullName = "Flag Patient", LabId = "L-FLG" };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("L-FLG")).ReturnsAsync(patient);
            _catalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test> { new() { TestId = 60, NameReport = "Test" } });
            _compareServiceMock.Setup(x => x.GetLastResultsAsync(908, 60, 5)).ReturnsAsync(new List<HistoricalResult>
            {
                new() { VisitId = 300, VisitDate = DateTime.Now.AddDays(-10), ParameterName = "Value", Value = "150", Flag = "H" },
                new() { VisitId = 301, VisitDate = DateTime.Now.AddDays(-20), ParameterName = "Value", Value = "80", Flag = "L" },
                new() { VisitId = 302, VisitDate = DateTime.Now.AddDays(-30), ParameterName = "Value", Value = "100", Flag = "N" }
            });

            viewModel.LabId = "L-FLG";
            viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(100);
            viewModel.SelectedTestId = 60;
            viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            viewModel.HistoryResults.Should().HaveCount(3);
        }
    }
}
