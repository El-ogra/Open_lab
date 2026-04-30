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
    /// Tests for Module 10: Function 10.7 (Monitor System Usage)
    /// ViewModel layer for SystemUsageMonitorViewModel
    /// </summary>
    public class SystemUsageMonitorViewModelTests : IDisposable
    {
        private readonly Mock<ISystemMonitorService> _systemMonitorServiceMock = new();
        private readonly SystemUsageMonitorViewModel _viewModel;

        public SystemUsageMonitorViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _systemMonitorServiceMock
                .Setup(x => x.GetActiveSessionsAsync())
                .ReturnsAsync(new List<ActiveSessionRow>
                {
                    new()
                    {
                        UserId = 1,
                        Username = "user1",
                        LoginAt = DateTime.Now,
                        LastActivityAt = DateTime.Now,
                        Status = "متصل"
                    }
                });

            _viewModel = new SystemUsageMonitorViewModel(_systemMonitorServiceMock.Object);
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.7 — Monitor System Usage
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RefreshCommand_WhenActiveSessions_ShouldPopulateSessionsAndShowCount()
        {
            // Function: 10.7 — Monitor System Usage
            // Arrange
            _systemMonitorServiceMock
                .Setup(x => x.GetActiveSessionsAsync())
                .ReturnsAsync(new List<ActiveSessionRow>
                {
                    new()
                    {
                        UserId = 5,
                        Username = "active_user",
                        FullName = "Active Employee",
                        LoginAt = DateTime.Now.AddHours(-2),
                        LastActivityAt = DateTime.Now.AddMinutes(-5),
                        Status = "متصل"
                    }
                });

            // Act
            _viewModel.RefreshCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Sessions.Should().ContainSingle();
            _viewModel.Sessions[0].Username.Should().Be("active_user");
            _viewModel.Sessions[0].Status.Should().Be("متصل");
            _viewModel.StatusMessage.Should().Contain("1");
        }

        [Fact]
        public async Task RefreshCommand_WhenServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.7 — Monitor System Usage (failure: service error)
            // Arrange
            _systemMonitorServiceMock
                .Setup(x => x.GetActiveSessionsAsync())
                .ThrowsAsync(new InvalidOperationException("monitor-failed"));

            // Act
            _viewModel.RefreshCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("monitor-failed");
        }

        [Fact]
        public async Task RefreshCommand_WhenNoActiveSessions_ShouldShowZeroCount()
        {
            // Function: 10.7 — Monitor System Usage (edge: no active users)
            // Arrange
            _systemMonitorServiceMock
                .Setup(x => x.GetActiveSessionsAsync())
                .ReturnsAsync(new List<ActiveSessionRow>());

            // Act
            _viewModel.RefreshCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Sessions.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("0");
        }

        [Fact]
        public async Task RefreshAsync_WhenCalled_ShouldPopulateSessionsCorrectly()
        {
            // Function: 10.7 — Monitor System Usage (internal load via InvokePrivateAsync)
            // Arrange
            _systemMonitorServiceMock
                .Setup(x => x.GetActiveSessionsAsync())
                .ReturnsAsync(new List<ActiveSessionRow>
                {
                    new() { UserId = 1, Username = "user1", LoginAt = DateTime.Now, LastActivityAt = DateTime.Now, Status = "متصل" }
                });

            // Act
            await _viewModel.InvokePrivateAsync("RefreshAsync");

            // Assert
            _viewModel.Sessions.Should().ContainSingle();
            _viewModel.Sessions[0].Username.Should().Be("user1");
            _viewModel.StatusMessage.Should().Contain("1");
        }

        [Fact]
        public async Task RefreshCommand_WithMultipleActiveSessions_ShouldDisplayAllSessions()
        {
            // Function: 10.7 — Monitor System Usage (edge: multiple concurrent users)
            // Arrange
            _systemMonitorServiceMock
                .Setup(x => x.GetActiveSessionsAsync())
                .ReturnsAsync(new List<ActiveSessionRow>
                {
                    new() { UserId = 1, Username = "admin", Status = "متصل", LoginAt = DateTime.Now, LastActivityAt = DateTime.Now },
                    new() { UserId = 2, Username = "tech1", Status = "متصل", LoginAt = DateTime.Now, LastActivityAt = DateTime.Now },
                    new() { UserId = 3, Username = "reception", Status = "متصل", LoginAt = DateTime.Now, LastActivityAt = DateTime.Now }
                });

            // Act
            _viewModel.RefreshCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Sessions.Should().HaveCount(3);
            _viewModel.StatusMessage.Should().Contain("3");
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
