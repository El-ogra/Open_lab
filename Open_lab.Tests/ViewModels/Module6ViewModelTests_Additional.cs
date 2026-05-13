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
    /// Additional ViewModel tests for Module 6
    /// </summary>
    public class Module6ViewModelTests_Additional : IDisposable
    {
        private readonly Mock<ISampleCollectionService> _collectionServiceMock;
        private readonly Mock<ISampleTrackingService> _trackingServiceMock;
        private readonly SampleCollectionViewModel _viewModel;

        public Module6ViewModelTests_Additional()
        {
            AppSessionTestHelper.ResetToAdmin();
            _collectionServiceMock = new Mock<ISampleCollectionService>();
            _trackingServiceMock = new Mock<ISampleTrackingService>();

            _collectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SampleCollectionRow>());
            _trackingServiceMock.Setup(x => x.GetSampleStatusAsync(It.IsAny<int>()))
                .ReturnsAsync((SampleCollection?)null);

            _viewModel = new SampleCollectionViewModel(_collectionServiceMock.Object, _trackingServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        #region Function 6.1 ViewModel Tests

        [Fact]
        public async Task MarkCollectedAsync_With_IsExternal_Flag_Should_Call_Service_With_External_True_SuccessGuard()
        {
            // Function: 6.4 — Mark Taken Outside Lab (External Flag from ViewModel)
            // Arrange
            // Act
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 100 };
            _collectionServiceMock.Setup(x => x.MarkCollectedAsync(100, It.IsAny<int>(), true, It.IsAny<int?>()))
                .Returns(Task.CompletedTask);

            _viewModel.MarkExternalCollectedCommand.Execute(null);
            await Task.Delay(50);

            _collectionServiceMock.Verify(x => x.MarkCollectedAsync(100, It.IsAny<int>(), true, It.IsAny<int?>()), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void MarkCollectedCommand_CanExecute_When_Row_Null_Should_Return_False_EdgeGuard()
        {
            // Function: 6.1 — Register Sample Collection (Null Selection Guard)
            // Arrange
            // Act
            _viewModel.SelectedRow = null;

            // Assert
            _viewModel.MarkCollectedCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public async Task LoadCommand_With_Empty_Rows_Should_Set_Empty_Message_EdgeGuard()
        {
            // Function: 6.1 — Register Sample Collection (Empty Result Edge Case)
            // Arrange
            // Act
            _collectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SampleCollectionRow>());

            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Items.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task RegisterSampleCollection_WithSelectedRow_ShouldMarkCollectedAndReloadRows()
        {
            // Function: 6.1 — Register Sample Collection
            // Arrange
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 101 };
            _collectionServiceMock.Setup(x => x.MarkCollectedAsync(101, It.IsAny<int>(), false, It.IsAny<int?>()))
                .Returns(Task.CompletedTask);
            _collectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SampleCollectionRow>
                {
                    new() { VisitTestId = 101, PatientName = "Patient 101" }
                });

            // Act
            _viewModel.MarkCollectedCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _collectionServiceMock.Verify(x => x.MarkCollectedAsync(101, It.IsAny<int>(), false, It.IsAny<int?>()), Times.Once);
            _collectionServiceMock.Verify(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce);
            _viewModel.Items.Should().ContainSingle(row => row.VisitTestId == 101);
            _viewModel.StatusMessage.Should().Contain("تم تحديث حالة العينة");
        }

        [Fact]
        public async Task RegisterSampleCollection_WhenMarkCollectedThrows_ShouldSetErrorStatusMessage()
        {
            // Function: 6.1 — Register Sample Collection
            // Arrange
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 102 };
            _collectionServiceMock.Setup(x => x.MarkCollectedAsync(102, It.IsAny<int>(), false, It.IsAny<int?>()))
                .ThrowsAsync(new InvalidOperationException("mark-collected-failed"));

            // Act
            _viewModel.MarkCollectedCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("mark-collected-failed");
            _collectionServiceMock.Verify(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        }

        #endregion

        #region Function 6.2 ViewModel Tests

        [Fact]
        public async Task MarkSeparatedAsync_With_Null_SeparationType_Should_Set_Warning_EdgeGuard()
        {
            // Function: 6.2 — Record Sample Separation (Null Separation Type)
            // Arrange
            // Act
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 200 };
            _viewModel.SeparationType = string.Empty;

            _collectionServiceMock.Setup(x => x.MarkSeparatedAsync(200, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _viewModel.MarkSeparatedCommand.Execute(null);
            await Task.Delay(50);

            // Separation type can be null - service handles it
            _collectionServiceMock.Verify(x => x.MarkSeparatedAsync(200, It.IsAny<string>()), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void MarkSeparatedCommand_CanExecute_When_No_Permission_Should_Return_False_EdgeGuard()
        {
            // Function: 6.2 — Record Sample Separation (No Permission Edge Case)
            // Arrange
            AppSessionTestHelper.Reset(); // No permissions
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 201 };
            _viewModel.SeparationType = string.Empty;

            // Act
            var canExecute = _viewModel.MarkSeparatedCommand.CanExecute(null);

            // Assert
            Assert.False(canExecute);
        }

        [Fact]
        public async Task RecordSampleSeparation_WithValidSeparationType_ShouldCallServiceAndReloadRows()
        {
            // Function: 6.2 — Record Sample Separation
            // Arrange
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 202 };
            _viewModel.SeparationType = "Centrifuge";
            _collectionServiceMock.Setup(x => x.MarkSeparatedAsync(202, "Centrifuge"))
                .Returns(Task.CompletedTask);
            _collectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SampleCollectionRow>
                {
                    new() { VisitTestId = 202, PatientName = "Separated Patient" }
                });

            // Act
            _viewModel.MarkSeparatedCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _collectionServiceMock.Verify(x => x.MarkSeparatedAsync(202, "Centrifuge"), Times.Once);
            _viewModel.Items.Should().ContainSingle(row => row.VisitTestId == 202);
            _viewModel.StatusMessage.Should().Contain("تم تسجيل فصل العينة");
        }

        [Fact]
        public async Task RecordSampleSeparation_WhenMarkSeparatedThrows_ShouldSetErrorStatusMessage()
        {
            // Function: 6.2 — Record Sample Separation
            // Arrange
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 203 };
            _viewModel.SeparationType = "Centrifuge";
            _collectionServiceMock.Setup(x => x.MarkSeparatedAsync(203, "Centrifuge"))
                .ThrowsAsync(new InvalidOperationException("mark-separated-failed"));

            // Act
            _viewModel.MarkSeparatedCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("mark-separated-failed");
            _collectionServiceMock.Verify(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        }

        #endregion

        #region Function 6.3 ViewModel Tests

        [Fact]
        public async Task RefreshSampleStatusAsync_With_Null_Tracking_Data_Should_Set_NotFound_Message_EdgeGuard()
        {
            // Function: 6.3 — Track Sample Status (No Tracking Data)
            // Arrange
            // Act
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 300 };
            _trackingServiceMock.Setup(x => x.GetSampleStatusAsync(300)).ReturnsAsync((SampleCollection?)null);

            _viewModel.RefreshSampleStatusCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.SelectedSampleStatus.Should().Contain("لا يوجد سجل تتبع");
        }

        [Fact]
        public async Task RefreshSampleStatusAsync_With_Valid_Data_Should_Update_Status_SuccessGuard()
        {
            // Function: 6.3 — Track Sample Status (Valid Status Update)
            // Arrange
            // Act
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 301 };

            var sampleData = new SampleCollection
            {
                VisitTestId = 301,
                Status = "مسحوبة",
                IsSeparated = false
            };
            _trackingServiceMock.Setup(x => x.GetSampleStatusAsync(301)).ReturnsAsync(sampleData);

            _viewModel.RefreshSampleStatusCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.SelectedSampleStatus.Should().Contain("مسحوبة");
        }

        [Fact]
        public async Task RefreshSampleStatusAsync_With_Separated_Status_Should_Show_Separated_Message_SuccessGuard()
        {
            // Function: 6.3 — Track Sample Status (Separated Status)
            // Arrange
            // Act
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 302 };

            var sampleData = new SampleCollection
            {
                VisitTestId = 302,
                Status = "مفصولة - Centrifuge",
                IsSeparated = true
            };
            _trackingServiceMock.Setup(x => x.GetSampleStatusAsync(302)).ReturnsAsync(sampleData);

            _viewModel.RefreshSampleStatusCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.SelectedSampleStatus.Should().Contain("مفصولة");
        }

        [Fact]
        public async Task TrackSampleStatus_WhenTrackingServiceThrows_ShouldSetTrackingErrorMessage()
        {
            // Function: 6.3 — Track Sample Status
            // Arrange
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 303 };
            _trackingServiceMock.Setup(x => x.GetSampleStatusAsync(303))
                .ThrowsAsync(new InvalidOperationException("tracking-failed"));

            // Act
            _viewModel.RefreshSampleStatusCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.SelectedSampleStatus.Should().Contain("خطأ تتبع:");
            _viewModel.SelectedSampleStatus.Should().Contain("tracking-failed");
        }

        #endregion

        #region Function 6.4 ViewModel Tests

        [Fact]
        public async Task MarkExternalCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing_EdgeGuard()
        {
            // Function: 6.4 — Mark Taken Outside Lab (Null Selection)
            // Arrange
            // Act
            _viewModel.SelectedRow = null;

            _viewModel.MarkExternalCollectedCommand.Execute(null);
            await Task.Delay(50);

            _collectionServiceMock.Verify(x => x.MarkCollectedAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<int?>()), Times.Never);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkExternalCollectedAsync_When_Service_Succeeds_Should_Reload_Data_SuccessGuard()
        {
            // Function: 6.4 — Mark Taken Outside Lab (Success + Reload)
            // Arrange
            // Act
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 400 };

            _collectionServiceMock.Setup(x => x.MarkCollectedAsync(400, It.IsAny<int>(), true, It.IsAny<int?>()))
                .Returns(Task.CompletedTask);
            _collectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SampleCollectionRow>());

            _viewModel.MarkExternalCollectedCommand.Execute(null);
            await Task.Delay(50);

            _collectionServiceMock.Verify(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeast(1));
            // Assert
            _viewModel.StatusMessage.Should().Contain("تم تعليم العينة كخارجية");
        }

        [Fact]
        public async Task MarkExternalCollectedCommand_Without_Login_Should_Reject_FailureGuard()
        {
            // Function: 6.4 — Mark Taken Outside Lab (Authentication Required)
            // Arrange
            // Act
            AppSessionTestHelper.Reset();
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 401 };

            _viewModel.MarkExternalCollectedCommand.Execute(null);
            await Task.Delay(50);

            _collectionServiceMock.Verify(x => x.MarkCollectedAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<int?>()), Times.Never);
            // Assert
            _viewModel.StatusMessage.Should().Contain("يجب تسجيل الدخول");
        }

        [Fact]
        public async Task MarkTakenOutsideLab_WhenExternalMarkThrows_ShouldSetErrorStatusMessage()
        {
            // Function: 6.4 — Mark Taken Outside Lab
            // Arrange
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 402 };
            _collectionServiceMock.Setup(x => x.MarkCollectedAsync(402, It.IsAny<int>(), true, It.IsAny<int?>()))
                .ThrowsAsync(new InvalidOperationException("external-mark-failed"));

            // Act
            _viewModel.MarkExternalCollectedCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("external-mark-failed");
            _collectionServiceMock.Verify(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        }

        #endregion

        #region State Transition Tests

        [Fact]
        public async Task MarkCollected_Followed_By_MarkSeparated_Should_Update_Status_SuccessGuard()
        {
            // Function: 6.2 — Record Sample Separation
            // Arrange
            // Act
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 500 };
            _viewModel.SeparationType = "Centrifuge";

            _collectionServiceMock.Setup(x => x.MarkCollectedAsync(500, It.IsAny<int>(), false, It.IsAny<int?>()))
                .Returns(Task.CompletedTask);
            _collectionServiceMock.Setup(x => x.MarkSeparatedAsync(500, "Centrifuge"))
                .Returns(Task.CompletedTask);
            _collectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SampleCollectionRow>());

            _viewModel.MarkCollectedCommand.Execute(null);
            await Task.Delay(50);
            _viewModel.MarkSeparatedCommand.Execute(null);
            await Task.Delay(50);

            _collectionServiceMock.Verify(x => x.MarkCollectedAsync(500, It.IsAny<int>(), false, It.IsAny<int?>()), Times.Once);
            _collectionServiceMock.Verify(x => x.MarkSeparatedAsync(500, "Centrifuge"), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkNotCollectedAsync_Should_Call_Service_SuccessGuard()
        {
            // Function: 6.1 — Register Sample Collection (Cancel Collection)
            // Arrange
            // Act
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 600 };

            _collectionServiceMock.Setup(x => x.MarkNotCollectedAsync(600)).Returns(Task.CompletedTask);
            _collectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SampleCollectionRow>());

            _viewModel.MarkNotCollectedCommand.Execute(null);
            await Task.Delay(50);

            _collectionServiceMock.Verify(x => x.MarkNotCollectedAsync(600), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        #endregion
    }
}
