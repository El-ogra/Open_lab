using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class PatientHistoryViewModelTests : IDisposable
    {
        private readonly Mock<IPatientService> _patientServiceMock;
        private readonly Mock<IReportService> _reportServiceMock;
        private readonly Mock<IPrintService> _printServiceMock;
        private readonly PatientHistoryViewModel _viewModel;

        public PatientHistoryViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _patientServiceMock = new Mock<IPatientService>();
            _reportServiceMock = new Mock<IReportService>();
            _printServiceMock = new Mock<IPrintService>();
            _viewModel = new PatientHistoryViewModel(_patientServiceMock.Object, _reportServiceMock.Object, _printServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadHistoryCommand_Should_Populate_Visits_LogicGuard()
        {
            // Arrange - 1.6 View History
            _viewModel.LabId = "LAB-001";
            var patient = new Patient { PatientId = 1, FullName = "John", LabId = "LAB-001" };
            var historyData = new PatientHistoryReportData { Patient = patient };
            historyData.Visits.Add(new VisitReportData { Visit = new Visit { VisitId = 10, VisitDate = DateTime.Now } });

            _patientServiceMock.Setup(s => s.GetByLabIdAsync("LAB-001")).ReturnsAsync(patient);
            _reportServiceMock.Setup(r => r.GetPatientHistoryAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(historyData);

            // Act
            _viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Visits.Should().HaveCount(1);
            _viewModel.History.Should().NotBeNull();
            _viewModel.History!.Patient.FullName.Should().Be("John");
        }

        [Fact]
        public async Task LoadHistoryAsync_When_Patient_Not_Found_Should_Set_Error()
        {
            // Arrange
            _viewModel.LabId = "LAB-NONE";
            _patientServiceMock.Setup(s => s.GetByLabIdAsync("LAB-NONE")).ReturnsAsync((Patient?)null);

            // Act
            _viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("لم يتم العثور");
            _viewModel.Visits.Should().BeEmpty();
        }
    }
}
