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
    public class SampleCollectionViewModelTests : IDisposable
    {
        private readonly Mock<ISampleCollectionService> _sampleCollectionServiceMock;
        private readonly Mock<ISampleTrackingService> _sampleTrackingServiceMock;
        private readonly SampleCollectionViewModel _viewModel;

        public SampleCollectionViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _sampleCollectionServiceMock = new Mock<ISampleCollectionService>();
            _sampleTrackingServiceMock = new Mock<ISampleTrackingService>();
            _viewModel = new SampleCollectionViewModel(_sampleCollectionServiceMock.Object, _sampleTrackingServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public void SampleCollection_Commands_When_Admin_Should_Be_Enabled()
        {
            // Function: X.X — To Be Determined
            _viewModel.LoadCommand.CanExecute(null).Should().BeTrue();
            _viewModel.MarkCollectedCommand.CanExecute(null).Should().BeFalse(); // SelectedRow == null
        }

        [Fact]
        public void MarkCollectedCommand_CanExecute_When_Row_Selected_Should_Return_True()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _viewModel.MarkCollectedCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public async Task LoadAsync_Should_Load_Sample_Collection_Rows()
        {
            // Function: X.X — To Be Determined
            var rows = new List<SampleCollectionRow> { new SampleCollectionRow { VisitTestId = 1 } };
            _sampleCollectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(rows);

            await _viewModel.InvokePrivateAsync("LoadAsync");

            _viewModel.Items.Should().HaveCount(1);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task LoadCommand_Execute_With_Valid_Data_Should_Load_Items_Success()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _sampleCollectionServiceMock
                .Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SampleCollectionRow> { new SampleCollectionRow { VisitTestId = 101 } });

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Items.Should().ContainSingle(x => x.VisitTestId == 101);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task LoadCommand_Execute_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _sampleCollectionServiceMock
                .Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("load failed"));

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("load failed");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task MarkCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = null;
            await _viewModel.InvokePrivateAsync("MarkCollectedAsync");
            _sampleCollectionServiceMock.Verify(x => x.MarkCollectedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int?>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkCollectedAsync_With_Valid_Row_Should_Call_Service()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _sampleCollectionServiceMock.Setup(x => x.MarkCollectedAsync(10, It.IsAny<int>(), false, It.IsAny<int?>())).Returns(Task.CompletedTask);

            _viewModel.MarkCollectedCommand.Execute(null);
            await Task.Delay(50); // wait for async void

            _sampleCollectionServiceMock.Verify(x => x.MarkCollectedAsync(10, It.IsAny<int>(), false, It.IsAny<int?>()), Times.Once);
            _viewModel.IsLoading.Should().BeFalse();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkCollectedCommand_When_User_Not_LoggedIn_Should_Reject_Request_Failure()
        {
            // Function: X.X — To Be Determined
            // Arrange
            AppSessionTestHelper.Reset();
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };

            // Act
            _viewModel.MarkCollectedCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _sampleCollectionServiceMock.Verify(x => x.MarkCollectedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int?>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("يجب تسجيل الدخول");
        }
        
        [Fact]
        public async Task MarkCollected_Failure_Should_HandleException_FailureGuard()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _sampleCollectionServiceMock.Setup(x => x.MarkCollectedAsync(10, It.IsAny<int>(), false, It.IsAny<int?>()))
                .ThrowsAsync(new Exception("DB ERROR"));

            _viewModel.MarkCollectedCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.StatusMessage.Should().Contain("DB ERROR");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task MarkSeparatedAsync_With_Valid_Row_Should_Call_Service()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _viewModel.SeparationType = "Centrifuge";
            _sampleCollectionServiceMock.Setup(x => x.MarkSeparatedAsync(10, "Centrifuge")).Returns(Task.CompletedTask);

            _viewModel.MarkSeparatedCommand.Execute(null);
            await Task.Delay(50);

            _sampleCollectionServiceMock.Verify(x => x.MarkSeparatedAsync(10, "Centrifuge"), Times.Once);
            _viewModel.IsLoading.Should().BeFalse();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkSeparated_Failure_Should_HandleException_FailureGuard()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _viewModel.SeparationType = "Centrifuge";
            _sampleCollectionServiceMock.Setup(x => x.MarkSeparatedAsync(10, "Centrifuge"))
                .ThrowsAsync(new Exception("Separation failed"));

            _viewModel.MarkSeparatedCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.StatusMessage.Should().Contain("Separation failed");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _sampleCollectionServiceMock.Setup(x => x.MarkNotCollectedAsync(10)).Returns(Task.CompletedTask);

            _viewModel.MarkNotCollectedCommand.Execute(null);
            await Task.Delay(50);

            _sampleCollectionServiceMock.Verify(x => x.MarkNotCollectedAsync(10), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم تعليم العينة كغير مجمعة");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task MarkNotCollectedCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _sampleCollectionServiceMock
                .Setup(x => x.MarkNotCollectedAsync(10))
                .ThrowsAsync(new Exception("not collected failure"));

            // Act
            _viewModel.MarkNotCollectedCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("not collected failure");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task MarkExternalCollectedAsync_With_Valid_Row_Should_Call_Service_SuccessGuard()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 20 };
            _sampleCollectionServiceMock.Setup(x => x.MarkCollectedAsync(20, It.IsAny<int>(), true, It.IsAny<int?>())).Returns(Task.CompletedTask);
            _sampleCollectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<SampleCollectionRow>());

            _viewModel.MarkExternalCollectedCommand.Execute(null);
            await Task.Delay(50); 

            _sampleCollectionServiceMock.Verify(x => x.MarkCollectedAsync(20, It.IsAny<int>(), true, It.IsAny<int?>()), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم تعليم العينة كخارجية");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task MarkExternalCollected_Failure_Should_HandleException_FailureGuard()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 20 };
            _sampleCollectionServiceMock.Setup(x => x.MarkCollectedAsync(20, It.IsAny<int>(), true, It.IsAny<int?>()))
                .ThrowsAsync(new Exception("External mark failed"));

            _viewModel.MarkExternalCollectedCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.StatusMessage.Should().Contain("External mark failed");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task MarkExternalCollectedCommand_When_User_Not_LoggedIn_Should_Reject_Request_Failure()
        {
            // Function: X.X — To Be Determined
            // Arrange
            AppSessionTestHelper.Reset();
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 20 };

            // Act
            _viewModel.MarkExternalCollectedCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _sampleCollectionServiceMock.Verify(x => x.MarkCollectedAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<int?>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("يجب تسجيل الدخول");
        }

        [Fact]
        public async Task RefreshSampleStatusAsync_Should_UpdateStatus_SuccessGuard()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 30 };
            var sampleData = new SampleCollection { VisitTestId = 30, Status = "Laboratory processing", IsSeparated = true };
            _sampleTrackingServiceMock.Setup(x => x.GetSampleStatusAsync(30)).ReturnsAsync(sampleData);

            _viewModel.RefreshSampleStatusCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.SelectedSampleStatus.Should().Contain("Laboratory processing");
            _viewModel.SelectedSampleStatus.Should().Contain("مفصولة");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task RefreshSampleStatusAsync_Failure_Should_HandleException_FailureGuard()
        {
            // Function: X.X — To Be Determined
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 30 };
            _sampleTrackingServiceMock.Setup(x => x.GetSampleStatusAsync(30))
                .ThrowsAsync(new Exception("Tracking system offline"));

            _viewModel.RefreshSampleStatusCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.SelectedSampleStatus.Should().Contain("Tracking system offline");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task RefreshSampleStatusCommand_When_No_Tracking_Record_Should_Show_NotFound_Message_Edge()
        {
            // Function: X.X — To Be Determined
            // Arrange
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 77 };
            _sampleTrackingServiceMock.Setup(x => x.GetSampleStatusAsync(77)).ReturnsAsync((SampleCollection?)null);

            // Act
            _viewModel.RefreshSampleStatusCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.SelectedSampleStatus.Should().Contain("لا يوجد سجل تتبع");
            _viewModel.IsLoading.Should().BeFalse();
        }
    }
}

