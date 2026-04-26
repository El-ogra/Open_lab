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
    public class AttendanceLogViewModelTests : IDisposable
    {
        private readonly Mock<IAttendanceService> _attendanceServiceMock = new();
        private readonly AttendanceLogViewModel _viewModel;

        public AttendanceLogViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _viewModel = new AttendanceLogViewModel(_attendanceServiceMock.Object);
        }

        [Fact]
        public async Task LoadLogsAsync_Should_Map_Attendance_Log_Rows_With_Duration()
        {
            _attendanceServiceMock.Setup(x => x.GetLogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<AttendanceLog>
                {
                    new()
                    {
                        AttendanceLogId = 5,
                        LoginAt = DateTime.Today.AddHours(8),
                        LogoutAt = DateTime.Today.AddHours(16).AddMinutes(30),
                        User = new User { Username = "tech1", FullName = "Tech One" },
                        Breaks = new List<AttendanceBreak>()
                    }
                });

            await _viewModel.InvokePrivateAsync("LoadLogsAsync");

            _viewModel.Logs.Should().ContainSingle();
            _viewModel.Logs[0].Username.Should().Be("tech1");
            _viewModel.Logs[0].Duration.Should().Be("08:30");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 سجل");
        }

        [Fact]
        public async Task LoadLogsAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Arrange
            _attendanceServiceMock
                .Setup(x => x.GetLogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("load-failed"));

            // Act
            await _viewModel.InvokePrivateAsync("LoadLogsAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("load-failed");
        }

        [Fact]
        public async Task ClockOutAsync_When_NoOpenLog_Should_Set_NotFoundMessage_EdgeGuard()
        {
            // Arrange
            AppSession.UserId = 11;
            _attendanceServiceMock
                .Setup(x => x.ClockOutAsync(AppSession.UserId, It.IsAny<DateTime?>()))
                .ReturnsAsync((AttendanceLog?)null);

            // Act
            await _viewModel.InvokePrivateAsync("ClockOutAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("لا يوجد سجل حضور مفتوح");
        }

        [Fact]
        public async Task ClockInCommand_When_Service_Returns_Log_Should_Set_AppSession_And_Status_Success()
        {
            // Arrange
            AppSession.UserId = 7;
            _attendanceServiceMock.Setup(x => x.ClockInAsync(AppSession.UserId, It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync(new AttendanceLog { AttendanceLogId = 11, UserId = 7, LoginAt = DateTime.Now });
            _attendanceServiceMock.Setup(x => x.GetOpenLogAsync(AppSession.UserId)).ReturnsAsync((AttendanceLog?)null);

            // Act
            _viewModel.ClockInCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            AppSession.AttendanceLogId.Should().Be(11);
            _attendanceServiceMock.Verify(x => x.ClockInAsync(AppSession.UserId, It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task StartBreakCommand_When_No_Open_Log_Should_Set_User_Message_Failure()
        {
            // Arrange
            AppSession.UserId = 22;
            _attendanceServiceMock.Setup(x => x.StartBreakAsync(AppSession.UserId, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string?>()))
                .ReturnsAsync((AttendanceBreak?)null);

            // Act
            _viewModel.StartBreakCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("لا يمكن بدء راحة");
        }

        [Fact]
        public async Task EndBreakCommand_When_Open_Break_Exists_Should_Set_Success_Message()
        {
            // Arrange
            AppSession.UserId = 30;
            _attendanceServiceMock.Setup(x => x.EndBreakAsync(AppSession.UserId, It.IsAny<DateTime?>()))
                .ReturnsAsync(new AttendanceBreak { BreakId = 5, AttendanceLogId = 10, StartAt = DateTime.Now.AddMinutes(-10), EndAt = DateTime.Now });
            _attendanceServiceMock.Setup(x => x.GetOpenLogAsync(AppSession.UserId)).ReturnsAsync((AttendanceLog?)null);

            // Act
            _viewModel.EndBreakCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _attendanceServiceMock.Verify(x => x.EndBreakAsync(AppSession.UserId, It.IsAny<DateTime?>()), Times.Once);
        }

        [Fact]
        public async Task LoadDailySummaryCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Arrange
            AppSession.UserId = 41;
            _attendanceServiceMock
                .Setup(x => x.GetDailyWorkingSummaryAsync(AppSession.UserId, It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
                .ThrowsAsync(new InvalidOperationException("summary-failed"));

            // Act
            _viewModel.LoadDailySummaryCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("summary-failed");
        }

        [Fact]
        public async Task RefreshOpenLogCommand_When_Open_Log_Has_Breaks_Should_Map_Breaks_Edge()
        {
            // Arrange
            AppSession.UserId = 52;
            _attendanceServiceMock.Setup(x => x.GetOpenLogAsync(AppSession.UserId))
                .ReturnsAsync(new AttendanceLog
                {
                    AttendanceLogId = 3,
                    UserId = 52,
                    LoginAt = DateTime.Today.AddHours(8),
                    Breaks = new List<AttendanceBreak>
                    {
                        new() { BreakId = 1, StartAt = DateTime.Today.AddHours(10), EndAt = null, Type = "Rest" }
                    }
                });

            // Act
            _viewModel.RefreshOpenLogCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Breaks.Should().ContainSingle(b => b.BreakId == 1 && b.Duration == "مفتوحة");
            _viewModel.StatusMessage.Should().Contain("سجل حضور مفتوح");
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
