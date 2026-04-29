using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using System.Linq;

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
            // Function: 1.6 — View Patient History
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
            await Task.Delay(100);

            // Assert
            _viewModel.Visits.Should().HaveCount(1);
            _viewModel.History.Should().NotBeNull();
            _viewModel.History!.Patient.FullName.Should().Be("John");
        }

        [Fact]
        public async Task LoadHistoryAsync_When_Patient_Not_Found_Should_Set_Error()
        {
            // Function: 1.6 — View Patient History
            // Arrange
            _viewModel.LabId = "LAB-NONE";
            _patientServiceMock.Setup(s => s.GetByLabIdAsync("LAB-NONE")).ReturnsAsync((Patient?)null);

            // Act
            _viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("لم يتم العثور");
            _viewModel.Visits.Should().BeEmpty();
        }

        [Fact]
        public async Task LoadHistoryAsync_When_ReportService_Throws_Should_Set_Error()
        {
            // Function: 1.6 — View Patient History
            // Arrange - FAILURE test for LoadHistoryCommand
            _viewModel.LabId = "LAB-001";
            var patient = new Patient { PatientId = 1, FullName = "John", LabId = "LAB-001" };
            _patientServiceMock.Setup(s => s.GetByLabIdAsync("LAB-001")).ReturnsAsync(patient);
            _reportServiceMock.Setup(r => r.GetPatientHistoryAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ThrowsAsync(new Exception("Report Error"));

            // Act
            _viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ");
        }

        [Fact]
        public async Task LoadHistoryAsync_When_LabId_Empty_Should_Set_Error()
        {
            // Function: 1.6 — View Patient History
            // Arrange - FAILURE test for LoadHistoryCommand with empty LabId
            _viewModel.LabId = "";

            // Act
            _viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("يرجى إدخال Lab ID");
        }

        [Fact]
        public async Task PrintHistoryAsync_When_HistoryIsNull_Should_NotCall_PrintService_EdgeGuard()
        {
            // Function: 1.6 — View Patient History
            // Arrange
            _viewModel.History.Should().BeNull();

            // Act
            await _viewModel.InvokePrivateAsync("PrintHistoryAsync");

            // Assert
            _printServiceMock.Verify(x => x.PrintPatientHistoryAsync(It.IsAny<PatientHistoryReportData>()), Times.Never);
        }

        [Fact]
        public async Task PrintHistoryAsync_When_PrintServiceThrows_Should_Set_PrintErrorMessage_FailureGuard()
        {
            // Function: 1.6 — View Patient History
            // Arrange
            _viewModel.LabId = "LAB-001";
            var patient = new Patient { PatientId = 1, FullName = "John", LabId = "LAB-001" };
            var historyData = new PatientHistoryReportData { Patient = patient };
            _patientServiceMock.Setup(s => s.GetByLabIdAsync("LAB-001")).ReturnsAsync(patient);
            _reportServiceMock.Setup(r => r.GetPatientHistoryAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(historyData);
            _printServiceMock.Setup(x => x.PrintPatientHistoryAsync(It.IsAny<PatientHistoryReportData>()))
                .ThrowsAsync(new Exception("print-failed"));

            await _viewModel.InvokePrivateAsync("LoadHistoryAsync");

            // Act
            await _viewModel.InvokePrivateAsync("PrintHistoryAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ طباعة");
            _viewModel.StatusMessage.Should().Contain("print-failed");
        }

        [Fact]
        public void PatientHistoryViewModel_Should_Expose_ReadOnly_Design_Without_EditCommand_DesignEnforcement()
        {
            // Function: 1.6 — View Patient History
            // Arrange
            var commandNames = typeof(PatientHistoryViewModel)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(System.Windows.Input.ICommand))
                .Select(p => p.Name)
                .ToList();

            // Act
            var hasEditOrSave = commandNames.Any(name =>
                name.Contains("Edit", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Save", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Update", StringComparison.OrdinalIgnoreCase));

            // Assert
            hasEditOrSave.Should().BeFalse("BR-MED-008 read-only enforcement is currently by design (no edit/save command exposed).");
        }
    }
}
