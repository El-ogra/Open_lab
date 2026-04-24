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
                        User = new User { Username = "tech1", FullName = "Tech One" }
                    }
                });

            await _viewModel.InvokePrivateAsync("LoadLogsAsync");

            _viewModel.Logs.Should().ContainSingle();
            _viewModel.Logs[0].Username.Should().Be("tech1");
            _viewModel.Logs[0].Duration.Should().Be("08:30");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 سجل");
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
