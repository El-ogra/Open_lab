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
            // Function: 10.4 — Record Attendance
            // Arrange
            // Act
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

            // Assert
            _viewModel.Logs.Should().ContainSingle();
            _viewModel.Logs[0].Username.Should().Be("tech1");
            _viewModel.Logs[0].Duration.Should().Be("08:30");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 سجل");
        }

        [Fact]
        public async Task LoadLogsAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Function: 10.4 — Record Attendance
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
        public async Task LoadLogsCommand_When_Service_Returns_Empty_Should_Set_Zero_Status_Edge()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            // Act
            _attendanceServiceMock.Setup(x => x.GetLogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<AttendanceLog>());

            _viewModel.LoadLogsCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Logs.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("تم تحميل 0 سجل");
        }

        [Fact]
        public async Task ClockOutAsync_When_NoOpenLog_Should_Set_NotFoundMessage_EdgeGuard()
        {
            // Function: 10.5 — Record Departure
            // Arrange
            SessionContext.Current.UserId = 11;
            _attendanceServiceMock
                .Setup(x => x.ClockOutAsync(SessionContext.Current.UserId, It.IsAny<DateTime?>()))
                .ReturnsAsync((AttendanceLog?)null);

            // Act
            await _viewModel.InvokePrivateAsync("ClockOutAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("لا يوجد سجل حضور مفتوح");
        }

        [Fact]
        public async Task ClockOutCommand_When_OpenLog_Exists_Should_Reset_AttendanceLogId_Success()
        {
            // Function: 10.5 — Record Departure
            // Arrange
            // Act
            SessionContext.Current.UserId = 11;
            SessionContext.Current.AttendanceLogId = 55;
            _attendanceServiceMock
                .Setup(x => x.ClockOutAsync(SessionContext.Current.UserId, It.IsAny<DateTime?>()))
                .ReturnsAsync(new AttendanceLog { AttendanceLogId = 55, UserId = 11, LogoutAt = DateTime.Now });
            _attendanceServiceMock.Setup(x => x.GetOpenLogAsync(SessionContext.Current.UserId)).ReturnsAsync((AttendanceLog?)null);

            _viewModel.ClockOutCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            SessionContext.Current.AttendanceLogId.Should().Be(0);
            _attendanceServiceMock.Verify(x => x.ClockOutAsync(SessionContext.Current.UserId, It.IsAny<DateTime?>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RecordDeparture_WhenClockOutServiceThrows_ShouldSetArabicErrorStatusMessage()
        {
            // Function: 10.5 — Record Departure
            // Arrange
            SessionContext.Current.UserId = 12;
            SessionContext.Current.AttendanceLogId = 77;
            _attendanceServiceMock
                .Setup(x => x.ClockOutAsync(SessionContext.Current.UserId, It.IsAny<DateTime?>()))
                .ThrowsAsync(new InvalidOperationException("clock-out-failed"));

            // Act
            _viewModel.ClockOutCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("clock-out-failed");
            SessionContext.Current.AttendanceLogId.Should().Be(77);
        }

        [Fact]
        public async Task ClockInCommand_When_Service_Returns_Log_Should_Set_AppSession_And_Status_Success()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            SessionContext.Current.UserId = 7;
            _attendanceServiceMock.Setup(x => x.ClockInAsync(SessionContext.Current.UserId, It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync(new AttendanceLog { AttendanceLogId = 11, UserId = 7, LoginAt = DateTime.Now });
            _attendanceServiceMock.Setup(x => x.GetOpenLogAsync(SessionContext.Current.UserId)).ReturnsAsync((AttendanceLog?)null);

            // Act
            _viewModel.ClockInCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            SessionContext.Current.AttendanceLogId.Should().Be(11);
            _attendanceServiceMock.Verify(x => x.ClockInAsync(SessionContext.Current.UserId, It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<string?>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task StartBreakCommand_When_No_Open_Log_Should_Set_User_Message_Failure()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            SessionContext.Current.UserId = 22;
            _attendanceServiceMock.Setup(x => x.StartBreakAsync(SessionContext.Current.UserId, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string?>()))
                .ReturnsAsync((AttendanceBreak?)null);

            // Act
            _viewModel.StartBreakCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("لا يمكن بدء راحة");
        }

        [Fact]
        public async Task StartBreakCommand_When_Open_Log_Exists_Should_Set_Success_Message()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            // Act
            SessionContext.Current.UserId = 22;
            _viewModel.BreakNote = "break note";
            _attendanceServiceMock.Setup(x => x.StartBreakAsync(SessionContext.Current.UserId, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string?>()))
                .ReturnsAsync(new AttendanceBreak { BreakId = 3, AttendanceLogId = 1, StartAt = DateTime.Now });
            _attendanceServiceMock.Setup(x => x.GetOpenLogAsync(SessionContext.Current.UserId)).ReturnsAsync((AttendanceLog?)null);

            _viewModel.StartBreakCommand.Execute(null);
            await Task.Delay(50);

            _attendanceServiceMock.Verify(x => x.StartBreakAsync(SessionContext.Current.UserId, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string?>()), Times.Once);
            // Assert
            _viewModel.BreakNote.Should().BeEmpty();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task EndBreakCommand_When_Open_Break_Exists_Should_Set_Success_Message()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            SessionContext.Current.UserId = 30;
            _attendanceServiceMock.Setup(x => x.EndBreakAsync(SessionContext.Current.UserId, It.IsAny<DateTime?>()))
                .ReturnsAsync(new AttendanceBreak { BreakId = 5, AttendanceLogId = 10, StartAt = DateTime.Now.AddMinutes(-10), EndAt = DateTime.Now });
            _attendanceServiceMock.Setup(x => x.GetOpenLogAsync(SessionContext.Current.UserId)).ReturnsAsync((AttendanceLog?)null);

            // Act
            _viewModel.EndBreakCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _attendanceServiceMock.Verify(x => x.EndBreakAsync(SessionContext.Current.UserId, It.IsAny<DateTime?>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task EndBreakCommand_When_No_Open_Break_Should_Set_Failure_Message()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            // Act
            SessionContext.Current.UserId = 30;
            _attendanceServiceMock.Setup(x => x.EndBreakAsync(SessionContext.Current.UserId, It.IsAny<DateTime?>()))
                .ReturnsAsync((AttendanceBreak?)null);

            _viewModel.EndBreakCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("لا توجد راحة مفتوحة");
        }

        [Fact]
        public async Task LoadDailySummaryCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            SessionContext.Current.UserId = 41;
            _attendanceServiceMock
                .Setup(x => x.GetDailyWorkingSummaryAsync(SessionContext.Current.UserId, It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
                .ThrowsAsync(new InvalidOperationException("summary-failed"));

            // Act
            _viewModel.LoadDailySummaryCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("summary-failed");
        }

        [Fact]
        public async Task LoadDailySummaryCommand_When_Service_Returns_Data_Should_Set_Summary_Message_Success()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            // Act
            SessionContext.Current.UserId = 41;
            _attendanceServiceMock
                .Setup(x => x.GetDailyWorkingSummaryAsync(SessionContext.Current.UserId, It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(new DailyWorkingSummary { UserId = 41, GrossMinutes = 480, BreakMinutes = 30, NetMinutes = 450 });

            _viewModel.LoadDailySummaryCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.DailySummary.Should().NotBeNull();
            _viewModel.DailySummary!.NetMinutes.Should().Be(450);
            _viewModel.StatusMessage.Should().Contain("صافي 450 دقيقة");
        }

        [Fact]
        public async Task RefreshOpenLogCommand_When_Open_Log_Has_Breaks_Should_Map_Breaks_Edge()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            SessionContext.Current.UserId = 52;
            _attendanceServiceMock.Setup(x => x.GetOpenLogAsync(SessionContext.Current.UserId))
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

        [Fact]
        public async Task RefreshOpenLogCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            // Act
            SessionContext.Current.UserId = 52;
            _attendanceServiceMock.Setup(x => x.GetOpenLogAsync(SessionContext.Current.UserId))
                .ThrowsAsync(new InvalidOperationException("refresh-open-failed"));

            _viewModel.RefreshOpenLogCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("refresh-open-failed");
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
