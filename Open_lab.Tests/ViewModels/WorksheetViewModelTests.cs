using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class WorkSheetByPatientViewModelTests : IDisposable
    {
        private readonly Mock<IWorksheetService> _worksheetServiceMock = new();
        private readonly Mock<IPrintService> _printServiceMock = new();
        private readonly WorkSheetByPatientViewModel _viewModel;

        public WorkSheetByPatientViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _viewModel = new WorkSheetByPatientViewModel(_worksheetServiceMock.Object, _printServiceMock.Object);
        }

        [Fact]
        public async Task LoadAsync_Should_Populate_Patient_Worksheet_Rows()
        {
            // Function: X.X — To Be Determined
            // Arrange
            // Act
            _worksheetServiceMock.Setup(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow>
                {
                    new() { VisitId = 1, PatientName = "P1", VisitDate = DateTime.Today, TestsCount = 2 }
                });

            await _viewModel.InvokePrivateAsync("LoadAsync");

            // Assert
            _viewModel.Rows.Should().ContainSingle();
            _viewModel.Rows[0].PatientName.Should().Be("P1");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 زيارة");
        }

        [Fact]
        public async Task PrintAsync_Should_Send_Patient_Worksheet_To_Print_Service()
        {
            // Function: X.X — To Be Determined
            // Arrange
            // Act
            _viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 1, PatientName = "P1", VisitDate = DateTime.Today, TestsCount = 1 });

            await _viewModel.InvokePrivateAsync("PrintAsync");

            _printServiceMock.Verify(x => x.PrintWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetPatientRow>>()), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadCommand_When_Executed_Should_Load_Patient_Rows_Success()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _worksheetServiceMock
                .Setup(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow> { new() { VisitId = 2, PatientName = "P2", TestsCount = 1 } });

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Rows.Should().ContainSingle(x => x.VisitId == 2);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task LoadCommand_When_PatientService_Fails_Should_Set_Error_Message_Failure()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _worksheetServiceMock
                .Setup(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("patient-load-failed"));

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("patient-load-failed");
        }

        [Fact]
        public async Task LoadCommand_When_PatientService_Returns_Empty_Should_Set_Zero_Status_Edge()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _worksheetServiceMock
                .Setup(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow>());

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Rows.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("0 زيارة");
        }

        [Fact]
        public async Task PrintCommand_When_PatientPrintService_Fails_Should_Set_Error_Message_Failure()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 3, PatientName = "P3", TestsCount = 1 });
            _printServiceMock
                .Setup(x => x.PrintWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetPatientRow>>()))
                .ThrowsAsync(new Exception("patient-print-failed"));

            // Act
            _viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("patient-print-failed");
        }

        [Fact]
        public void PrintCommand_When_PatientRows_Empty_Should_Be_Disabled_Edge()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _viewModel.Rows.Clear();

            // Act
            var canExecute = _viewModel.PrintCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }

    public class WorkSheetByTestViewModelTests : IDisposable
    {
        private readonly Mock<IWorksheetService> _worksheetServiceMock = new();
        private readonly Mock<IPrintService> _printServiceMock = new();
        private readonly WorkSheetByTestViewModel _viewModel;

        public WorkSheetByTestViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _viewModel = new WorkSheetByTestViewModel(_worksheetServiceMock.Object, _printServiceMock.Object);
        }

        [Fact]
        public async Task LoadAsync_Should_Populate_Test_Worksheet_Rows()
        {
            // Function: X.X — To Be Determined
            // Arrange
            // Act
            _worksheetServiceMock.Setup(x => x.GetWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetTestRow>
                {
                    new() { TestName = "CBC", Count = 3 }
                });

            await _viewModel.InvokePrivateAsync("LoadAsync");

            // Assert
            _viewModel.Rows.Should().ContainSingle();
            _viewModel.Rows[0].TestName.Should().Be("CBC");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 تحليل");
        }

        [Fact]
        public async Task PrintAsync_Should_Send_Test_Worksheet_To_Print_Service()
        {
            // Function: X.X — To Be Determined
            // Arrange
            // Act
            _viewModel.Rows.Add(new WorkSheetTestRow { TestName = "CBC", Count = 3 });

            await _viewModel.InvokePrivateAsync("PrintAsync");

            _printServiceMock.Verify(x => x.PrintWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetTestRow>>()), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadCommand_When_Executed_Should_Load_Test_Rows_Success()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _worksheetServiceMock
                .Setup(x => x.GetWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetTestRow> { new() { TestName = "ALT", Count = 2 } });

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Rows.Should().ContainSingle(x => x.TestName == "ALT" && x.Count == 2);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task LoadCommand_When_TestService_Fails_Should_Set_Error_Message_Failure()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _worksheetServiceMock
                .Setup(x => x.GetWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("test-load-failed"));

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("test-load-failed");
        }

        [Fact]
        public async Task LoadCommand_When_TestService_Returns_Empty_Should_Set_Zero_Status_Edge()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _worksheetServiceMock
                .Setup(x => x.GetWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetTestRow>());

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Rows.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("0 تحليل");
        }

        [Fact]
        public async Task PrintCommand_When_TestPrintService_Fails_Should_Set_Error_Message_Failure()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _viewModel.Rows.Add(new WorkSheetTestRow { TestName = "AST", Count = 1 });
            _printServiceMock
                .Setup(x => x.PrintWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetTestRow>>()))
                .ThrowsAsync(new Exception("test-print-failed"));

            // Act
            _viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("test-print-failed");
        }

        [Fact]
        public void PrintCommand_When_TestRows_Empty_Should_Be_Disabled_Edge()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _viewModel.Rows.Clear();

            // Act
            var canExecute = _viewModel.PrintCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
