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
    /// <summary>
    /// Tests for Module 10: Function 10.6 (View User Activity Log)
    /// ViewModel layer for UserActivityLogViewModel
    /// BR-SEC-002: Audit trail — who did what and when
    /// </summary>
    public class UserActivityLogViewModelTests : IDisposable
    {
        private readonly Mock<IUserActivityService> _userActivityServiceMock = new();
        private readonly UserActivityLogViewModel _viewModel;

        public UserActivityLogViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _userActivityServiceMock
                .Setup(x => x.GetRecentActivitiesAsync(It.IsAny<int?>(), It.IsAny<int>()))
                .ReturnsAsync(new List<UserActivityRow>
                {
                    new()
                    {
                        AuditLogId = 1,
                        Username = "admin",
                        ActivityDescription = "تعديل في بيانات مريض"
                    }
                });

            _viewModel = new UserActivityLogViewModel(_userActivityServiceMock.Object);
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.6 — View User Activity Log (BR-SEC-002)
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoadCommand_WithValidMaxCount_ShouldPopulateItemsAndShowCount()
        {
            // Function: 10.6 — View User Activity Log (BR-SEC-002: audit trail display)
            // Arrange
            _userActivityServiceMock
                .Setup(x => x.GetRecentActivitiesAsync(null, 50))
                .ReturnsAsync(new List<UserActivityRow>
                {
                    new()
                    {
                        AuditLogId = 10,
                        Username = "tech1",
                        ActivityDescription = "إدخال نتيجة فحص"
                    }
                });
            _viewModel.MaxCount = 50;

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Items.Should().ContainSingle();
            _viewModel.Items[0].Username.Should().Be("tech1");
            _viewModel.StatusMessage.Should().Contain("1");
            _userActivityServiceMock.Verify(
                x => x.GetRecentActivitiesAsync(null, 50), Times.AtLeastOnce);
        }

        [Fact]
        public async Task LoadCommand_WhenMaxCountIsInvalid_ShouldRequestDefault100()
        {
            // Function: 10.6 — View User Activity Log (edge: invalid count defaults to 100)
            // Arrange
            _viewModel.MaxCount = 0;
            _userActivityServiceMock.Invocations.Clear();

            // Act
            await _viewModel.InvokePrivateAsync("LoadAsync");

            // Assert
            _userActivityServiceMock.Verify(
                x => x.GetRecentActivitiesAsync(null, 100), Times.Once);
            _viewModel.Items.Should().ContainSingle();
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 سجل");
        }

        [Fact]
        public async Task LoadCommand_WhenServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.6 — View User Activity Log (failure: service error)
            // Arrange
            _userActivityServiceMock
                .Setup(x => x.GetRecentActivitiesAsync(It.IsAny<int?>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("activity-load-failed"));

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("activity-load-failed");
        }

        [Fact]
        public async Task LoadCommand_WhenServiceReturnsEmpty_ShouldShowZeroCount()
        {
            // Function: 10.6 — View User Activity Log (edge: no activity records)
            // Arrange
            _userActivityServiceMock
                .Setup(x => x.GetRecentActivitiesAsync(It.IsAny<int?>(), It.IsAny<int>()))
                .ReturnsAsync(new List<UserActivityRow>());

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Items.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("تم تحميل 0 سجل");
        }

        [Fact]
        public async Task LoadCommand_WithMultipleRecords_ShouldDisplayAllRecordsWithCorrectData()
        {
            // Function: 10.6 — View User Activity Log (BR-SEC-002: full audit trail)
            // Arrange
            _userActivityServiceMock
                .Setup(x => x.GetRecentActivitiesAsync(It.IsAny<int?>(), It.IsAny<int>()))
                .ReturnsAsync(new List<UserActivityRow>
                {
                    new() { AuditLogId = 1, Username = "admin", ActivityDescription = "إضافة مريض" },
                    new() { AuditLogId = 2, Username = "tech1", ActivityDescription = "تعديل فاتورة" },
                    new() { AuditLogId = 3, Username = "reception", ActivityDescription = "حجب صلاحية" }
                });

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Items.Should().HaveCount(3);
            _viewModel.Items.Should().Contain(i => i.Username == "admin");
            _viewModel.Items.Should().Contain(i => i.Username == "tech1");
            _viewModel.StatusMessage.Should().Contain("3");
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
