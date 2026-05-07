using System;
using System.Collections.Generic;
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
    public class GroupWorksheetViewModelTests : IDisposable
    {
        private readonly Mock<IGroupWorksheetService> _groupWorksheetServiceMock = new();
        private readonly Mock<ITestCatalogService> _testCatalogServiceMock = new();
        private readonly Mock<IPrintService> _printServiceMock = new();
        private readonly GroupWorksheetViewModel _viewModel;

        public GroupWorksheetViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync())
                .ReturnsAsync(new List<TestGroup> { new() { GroupId = 1, GroupName = "G1" } });
            _testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync())
                .ReturnsAsync(new List<CustomGroup> { new() { CustomGroupId = 2, Name = "CG1" } });

            _viewModel = new GroupWorksheetViewModel(
                _groupWorksheetServiceMock.Object,
                _testCatalogServiceMock.Object,
                _printServiceMock.Object);
        }

        [Fact]
        public async Task LoadWorksheetAsync_Should_Load_Group_Worksheet_Rows()
        {
            _groupWorksheetServiceMock.Setup(x => x.GetGroupWorksheetByGroupAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow> { new() { VisitId = 1, PatientName = "P1", TestsCount = 2 } });

            _viewModel.SelectedGroupId = 1;
            await _viewModel.InvokePrivateAsync("LoadWorksheetAsync");

            _viewModel.Rows.Should().ContainSingle();
            _viewModel.Rows[0].PatientName.Should().Be("P1");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 زيارة");
        }

        [Fact]
        public async Task LoadGroupsCommand_When_Executed_Should_Load_Groups_Success()
        {
            // Act
            _viewModel.LoadGroupsCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Groups.Should().NotBeEmpty();
            _viewModel.CustomGroups.Should().NotBeEmpty();
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task LoadGroupsCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Arrange
            _testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ThrowsAsync(new InvalidOperationException("groups-failed"));

            // Act
            _viewModel.LoadGroupsCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("groups-failed");
        }

        [Fact]
        public async Task LoadGroupsCommand_With_Empty_Lists_Should_Keep_Collections_Empty_Edge()
        {
            // Arrange
            _testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ReturnsAsync(new List<TestGroup>());
            _testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync()).ReturnsAsync(new List<CustomGroup>());

            // Act
            _viewModel.LoadGroupsCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Groups.Should().BeEmpty();
            _viewModel.CustomGroups.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("تم تحميل 0 مجموعة");
        }

        [Fact]
        public async Task PrintAsync_Should_Send_Group_Worksheet_To_Print_Service()
        {
            _viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 1, PatientName = "P1", TestsCount = 1 });

            await _viewModel.InvokePrivateAsync("PrintAsync");

            _printServiceMock.Verify(x => x.PrintWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetPatientRow>>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadWorksheetCommand_When_No_Selection_Should_Not_Call_Service_Edge()
        {
            // Arrange
            _viewModel.SelectedGroupId = null;
            _viewModel.SelectedCustomGroupId = null;
            _viewModel.IsCustomGroup = false;

            // Act
            _viewModel.LoadWorksheetCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _groupWorksheetServiceMock.Verify(x => x.GetGroupWorksheetByGroupAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
            _groupWorksheetServiceMock.Verify(x => x.GetGroupWorksheetByCustomGroupAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadWorksheetAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Arrange
            _groupWorksheetServiceMock
                .Setup(x => x.GetGroupWorksheetByGroupAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("worksheet-failed"));
            _viewModel.IsCustomGroup = false;
            _viewModel.SelectedGroupId = 1;

            // Act
            await _viewModel.InvokePrivateAsync("LoadWorksheetAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("worksheet-failed");
        }

        [Fact]
        public void PrintCommand_When_RowsEmpty_Should_Be_Disabled_EdgeGuard()
        {
            // Arrange
            _viewModel.Rows.Clear();

            // Act
            var canExecute = _viewModel.PrintCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public async Task PrintCommand_When_PrintService_Fails_Should_Set_Error_Message_Failure()
        {
            // Arrange
            _viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 2, PatientName = "P2", TestsCount = 1 });
            _printServiceMock
                .Setup(x => x.PrintWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetPatientRow>>()))
                .ThrowsAsync(new Exception("print-failed"));

            // Act
            _viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("print-failed");
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
