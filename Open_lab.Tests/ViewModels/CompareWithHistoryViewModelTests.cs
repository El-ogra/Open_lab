using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class CompareWithHistoryViewModelTests
    {
        private readonly Mock<ICompareWithHistoryService> _compareServiceMock;
        private readonly Mock<IPatientService> _patientServiceMock;
        private readonly Mock<ITestCatalogService> _catalogServiceMock;
        private readonly CompareWithHistoryViewModel _viewModel;

        public CompareWithHistoryViewModelTests()
        {
            _compareServiceMock = new Mock<ICompareWithHistoryService>();
            _patientServiceMock = new Mock<IPatientService>();
            _catalogServiceMock = new Mock<ITestCatalogService>();

            _viewModel = new CompareWithHistoryViewModel(
                _compareServiceMock.Object,
                _patientServiceMock.Object,
                _catalogServiceMock.Object);
        }

        [Fact]
        public async Task LoadPatientCommand_When_LabId_NotFound_Should_Set_Status_FailureGuard()
        {
            _viewModel.LabId = "UNKNOWN";
            _patientServiceMock.Setup(s => s.GetByLabIdAsync("UNKNOWN")).ReturnsAsync((Patient?)null);

            _viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.PatientId.Should().BeNull();
            _viewModel.StatusMessage.Should().Be("لم يتم العثور على المريض.");
        }

        [Fact]
        public async Task LoadPatientCommand_When_Patient_Exists_Should_Load_Test_List_SuccessGuard()
        {
            _viewModel.LabId = "L-100";
            _patientServiceMock.Setup(s => s.GetByLabIdAsync("L-100"))
                .ReturnsAsync(new Patient { PatientId = 10, FullName = "Ali", LabId = "L-100", Gender = "Male" });
            _catalogServiceMock.Setup(s => s.GetAllTestsAsync())
                .ReturnsAsync(new List<Test>
                {
                    new() { TestId = 1, NameReport = "CBC", NameReceipt = "CBC", Code = "CBC", Price = 120m },
                    new() { TestId = 2, NameReport = "Glucose", NameReceipt = "GLU", Code = "GLU", Price = 80m }
                });

            _viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.PatientId.Should().Be(10);
            _viewModel.Tests.Should().HaveCount(2);
            _viewModel.StatusMessage.Should().Contain("تم تحميل بيانات المريض");
        }

        [Fact]
        public async Task LoadHistoryCommand_Should_Group_And_Load_HistoryRows_EdgeGuard()
        {
            _viewModel.LabId = "L-200";
            _patientServiceMock.Setup(s => s.GetByLabIdAsync("L-200"))
                .ReturnsAsync(new Patient { PatientId = 11, FullName = "Mona", LabId = "L-200", Gender = "Female" });
            _catalogServiceMock.Setup(s => s.GetAllTestsAsync())
                .ReturnsAsync(new List<Test> { new() { TestId = 5, NameReport = "TSH", NameReceipt = "TSH", Code = "TSH", Price = 100m } });
            _compareServiceMock.Setup(s => s.GetLastResultsAsync(11, 5, 3))
                .ReturnsAsync(new List<HistoricalResult>
                {
                    new() { VisitId = 1, VisitDate = new DateTime(2026, 4, 10), ParameterName = "TSH", Value = "3.1", Flag = "N" },
                    new() { VisitId = 1, VisitDate = new DateTime(2026, 4, 10), ParameterName = "FT4", Value = "1.2", Flag = "N" },
                    new() { VisitId = 2, VisitDate = new DateTime(2026, 3, 1), ParameterName = "TSH", Value = "5.4", Flag = "H" }
                });

            _viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(50);
            _viewModel.SelectedTestId = 5;
            _viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.HistoryResults.Should().HaveCount(3);
            _viewModel.HistoryResults[0].VisitId.Should().Be(1);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }
    }
}
