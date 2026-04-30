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
    /// <summary>
    /// Module 11 — HR & Attendance | موديول الحضور والانصراف
    /// ViewModel-layer unit tests covering the five documented functions:
    ///  11.1 Clock In       (AttendanceLogViewModel.ClockInCommand)
    ///  11.2 Clock Out      (AttendanceLogViewModel.ClockOutCommand)
    ///  11.3 Calculate Working Hours (AttendanceLogViewModel.LoadDailySummaryCommand)
    ///  11.4 Calculate Tardiness     (AttendanceReportViewModel.GenerateReportCommand)
    ///  11.5 Generate Attendance Report (AttendanceReportViewModel.GeneratePayrollSummaryCommand)
    ///
    /// All tests follow AAA structure with the function-number comment as the first line,
    /// per the UnitTest_Skill.md mandatory rules. All Service dependencies are mocked with Moq.
    /// Verify is used for write/command operations and assertions check ViewModel property changes,
    /// not just service-method invocation.
    /// </summary>
    public class Module11ViewModelTests_Additional : IDisposable
    {
        private readonly Mock<IAttendanceService> _attendanceServiceMock = new();
        private readonly Mock<ITardinessService> _tardinessServiceMock = new();
        private readonly Mock<IUserAdminService> _userAdminServiceMock = new();
        private readonly Mock<IAttendancePayrollReportService> _payrollServiceMock = new();

        public Module11ViewModelTests_Additional()
        {
            AppSessionTestHelper.ResetToAdmin();
            // Default empty results for the report VM constructor's initial LoadUsers call.
            _userAdminServiceMock
                .Setup(x => x.GetUsersAsync())
                .ReturnsAsync(new List<User>());
            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendanceReportRow>());
            _payrollServiceMock
                .Setup(x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendancePayrollSummaryRow>());
        }

        public void Dispose() => AppSessionTestHelper.Reset();

        private AttendanceLogViewModel CreateLogViewModel()
        {
            // Default open-log = null so the auto refresh in the constructor doesn't fail.
            _attendanceServiceMock
                .Setup(x => x.GetOpenLogAsync(It.IsAny<int>()))
                .ReturnsAsync((AttendanceLog?)null);
            return new AttendanceLogViewModel(_attendanceServiceMock.Object);
        }

        private AttendanceReportViewModel CreateReportViewModel()
        {
            return new AttendanceReportViewModel(
                _tardinessServiceMock.Object,
                _userAdminServiceMock.Object,
                _payrollServiceMock.Object);
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.1 — Clock In (ViewModel)
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ClockInCommand_WithValidUser_ShouldUpdateAppSessionAndStatusMessage_SuccessGuard()
        {
            // Function: 11.1 — Clock In
            // Arrange
            AppSession.UserId = 7;
            AppSession.AttendanceLogId = 0;
            _attendanceServiceMock
                .Setup(x => x.ClockInAsync(7, It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync(new AttendanceLog { AttendanceLogId = 42, UserId = 7, LoginAt = DateTime.Now });
            var vm = CreateLogViewModel();

            // Act
            vm.ClockInCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            AppSession.AttendanceLogId.Should().Be(42);
            vm.StatusMessage.Should().Contain("تم تسجيل الحضور");
            vm.StatusMessage.Should().Contain("42");
            _attendanceServiceMock.Verify(
                x => x.ClockInAsync(7, It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<string?>()),
                Times.Once);
        }

        [Fact]
        public async Task ClockInCommand_WhenServiceThrows_ShouldSetErrorStatusMessage_FailureGuard()
        {
            // Function: 11.1 — Clock In (فشل الخدمة)
            // Arrange
            AppSession.UserId = 8;
            _attendanceServiceMock
                .Setup(x => x.ClockInAsync(8, It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<string?>()))
                .ThrowsAsync(new InvalidOperationException("clockin-failed"));
            var vm = CreateLogViewModel();

            // Act
            vm.ClockInCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.StatusMessage.Should().NotBeNullOrEmpty();
            vm.StatusMessage.Should().Contain("خطأ:");
            vm.StatusMessage.Should().Contain("clockin-failed");
        }

        [Fact]
        public void ClockInCommand_WhenNoUserLoggedIn_ShouldNotInvokeService_EdgeGuard()
        {
            // Function: 11.1 — Clock In (الحالة الحدية: لا يوجد مستخدم مسجل دخول)
            // Arrange
            AppSession.Clear(); // UserId = 0 → CanExecute should be false
            var vm = CreateLogViewModel();

            // Act
            var canExecute = vm.ClockInCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse("ClockIn must require a logged-in user");
            _attendanceServiceMock.Verify(
                x => x.ClockInAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<string?>()),
                Times.Never);
        }

        [Fact]
        public async Task ClockInCommand_WhenAlreadyHasOpenLog_ShouldStillReportSuccess_EdgeGuard()
        {
            // Function: 11.1 — Clock In (السجل موجود مسبقاً، الخدمة ترجعه دون إنشاء جديد)
            // Arrange
            AppSession.UserId = 9;
            _attendanceServiceMock
                .Setup(x => x.ClockInAsync(9, It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync(new AttendanceLog { AttendanceLogId = 100, UserId = 9, LoginAt = DateTime.Today.AddHours(8) });
            var vm = CreateLogViewModel();

            // Act
            vm.ClockInCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            AppSession.AttendanceLogId.Should().Be(100);
            vm.StatusMessage.Should().Contain("100");
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.2 — Clock Out (ViewModel)
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ClockOutCommand_WithOpenLog_ShouldClearAppSessionAndShowSuccess_SuccessGuard()
        {
            // Function: 11.2 — Clock Out
            // Arrange
            AppSession.UserId = 11;
            AppSession.AttendanceLogId = 55;
            _attendanceServiceMock
                .Setup(x => x.ClockOutAsync(11, It.IsAny<DateTime?>()))
                .ReturnsAsync(new AttendanceLog { AttendanceLogId = 55, UserId = 11, LogoutAt = DateTime.Now });
            var vm = CreateLogViewModel();

            // Act
            vm.ClockOutCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            AppSession.AttendanceLogId.Should().Be(0);
            vm.StatusMessage.Should().Contain("تم تسجيل الانصراف");
            vm.StatusMessage.Should().Contain("55");
            _attendanceServiceMock.Verify(x => x.ClockOutAsync(11, It.IsAny<DateTime?>()), Times.Once);
        }

        [Fact]
        public async Task ClockOutCommand_WhenNoOpenLog_ShouldShowFailureMessage_FailureGuard()
        {
            // Function: 11.2 — Clock Out (لا يوجد سجل لإغلاقه)
            // Arrange
            AppSession.UserId = 12;
            _attendanceServiceMock
                .Setup(x => x.ClockOutAsync(12, It.IsAny<DateTime?>()))
                .ReturnsAsync((AttendanceLog?)null);
            var vm = CreateLogViewModel();

            // Act
            vm.ClockOutCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.StatusMessage.Should().NotBeNullOrEmpty();
            vm.StatusMessage.Should().Contain("لا يوجد سجل حضور مفتوح");
        }

        [Fact]
        public async Task ClockOutCommand_WhenServiceThrows_ShouldSetErrorMessage_FailureGuard()
        {
            // Function: 11.2 — Clock Out (الخدمة ترمي استثناء)
            // Arrange
            AppSession.UserId = 13;
            _attendanceServiceMock
                .Setup(x => x.ClockOutAsync(13, It.IsAny<DateTime?>()))
                .ThrowsAsync(new InvalidOperationException("clockout-failed"));
            var vm = CreateLogViewModel();

            // Act
            vm.ClockOutCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.StatusMessage.Should().Contain("خطأ:");
            vm.StatusMessage.Should().Contain("clockout-failed");
        }

        [Fact]
        public void ClockOutCommand_CanExecute_WhenNoUser_ShouldReturnFalse_EdgeGuard()
        {
            // Function: 11.2 — Clock Out (الحالة الحدية: لا يوجد مستخدم)
            // Arrange
            AppSession.Clear();
            var vm = CreateLogViewModel();

            // Act
            var canExec = vm.ClockOutCommand.CanExecute(null);

            // Assert
            canExec.Should().BeFalse();
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.3 — Calculate Working Hours (ViewModel)
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoadDailySummaryCommand_WithValidData_ShouldPopulateDailySummaryProperty_SuccessGuard()
        {
            // Function: 11.3 — Calculate Working Hours
            // Arrange
            AppSession.UserId = 21;
            _attendanceServiceMock
                .Setup(x => x.GetDailyWorkingSummaryAsync(21, It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(new DailyWorkingSummary
                {
                    UserId = 21,
                    Date = DateTime.Today,
                    GrossMinutes = 540,
                    BreakMinutes = 60,
                    NetMinutes = 480
                });
            var vm = CreateLogViewModel();

            // Act
            vm.LoadDailySummaryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.DailySummary.Should().NotBeNull();
            vm.DailySummary!.GrossMinutes.Should().Be(540);
            vm.DailySummary.BreakMinutes.Should().Be(60);
            vm.DailySummary.NetMinutes.Should().Be(480);
            vm.StatusMessage.Should().Contain("صافي 480 دقيقة");
            vm.StatusMessage.Should().Contain("راحة 60 دقيقة");
        }

        [Fact]
        public async Task LoadDailySummaryCommand_WhenServiceThrows_ShouldSetErrorStatusMessage_FailureGuard()
        {
            // Function: 11.3 — Calculate Working Hours (فشل الخدمة)
            // Arrange
            AppSession.UserId = 22;
            _attendanceServiceMock
                .Setup(x => x.GetDailyWorkingSummaryAsync(22, It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
                .ThrowsAsync(new InvalidOperationException("hours-failed"));
            var vm = CreateLogViewModel();

            // Act
            vm.LoadDailySummaryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.StatusMessage.Should().Contain("خطأ:");
            vm.StatusMessage.Should().Contain("hours-failed");
        }

        [Fact]
        public async Task LoadDailySummaryCommand_WithZeroMinutes_ShouldShowZeroValues_EdgeGuard()
        {
            // Function: 11.3 — Calculate Working Hours (الحدية: لا توجد دقائق عمل)
            // Arrange
            AppSession.UserId = 23;
            _attendanceServiceMock
                .Setup(x => x.GetDailyWorkingSummaryAsync(23, It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(new DailyWorkingSummary
                {
                    UserId = 23,
                    Date = DateTime.Today,
                    GrossMinutes = 0,
                    BreakMinutes = 0,
                    NetMinutes = 0
                });
            var vm = CreateLogViewModel();

            // Act
            vm.LoadDailySummaryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.DailySummary.Should().NotBeNull();
            vm.DailySummary!.NetMinutes.Should().Be(0);
            vm.StatusMessage.Should().Contain("صافي 0 دقيقة");
        }

        [Fact]
        public async Task LoadLogsCommand_WithValidData_ShouldMapAttendanceLogRowsAndStatusMessage_SuccessGuard()
        {
            // Function: 11.3 — Calculate Working Hours (تحميل سجلات الحضور لعرضها)
            // Arrange
            _attendanceServiceMock
                .Setup(x => x.GetLogsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<AttendanceLog>
                {
                    new()
                    {
                        AttendanceLogId = 7,
                        LoginAt = DateTime.Today.AddHours(8),
                        LogoutAt = DateTime.Today.AddHours(16),
                        User = new User { Username = "tech1", FullName = "Tech One" },
                        Breaks = new List<AttendanceBreak>()
                    }
                });
            var vm = CreateLogViewModel();

            // Act
            vm.LoadLogsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.Logs.Should().ContainSingle();
            vm.Logs[0].Username.Should().Be("tech1");
            vm.Logs[0].Duration.Should().Be("08:00");
            vm.StatusMessage.Should().Contain("تم تحميل 1 سجل");
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.4 — Calculate Tardiness (ViewModel)
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GenerateReportCommand_WithDelayAndOvertime_ShouldPopulateReportRows_SuccessGuard()
        {
            // Function: 11.4 — Calculate Tardiness
            // Arrange
            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendanceReportRow>
                {
                    new() { Username = "u-late", TotalHours = 8.5, DelayMinutes = 25, OvertimeMinutes = 30, ShiftName = "Morning" }
                });
            var vm = CreateReportViewModel();

            // Act
            vm.GenerateReportCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.ReportRows.Should().ContainSingle();
            vm.ReportRows[0].Username.Should().Be("u-late");
            vm.ReportRows[0].DelayMinutes.Should().Be(25);
            vm.ReportRows[0].OvertimeMinutes.Should().Be(30);
            vm.StatusMessage.Should().Contain("تم إنشاء التقرير");
            vm.StatusMessage.Should().Contain("1 سجل");
        }

        [Fact]
        public async Task GenerateReportCommand_WhenServiceThrows_ShouldSetErrorMessage_FailureGuard()
        {
            // Function: 11.4 — Calculate Tardiness (فشل الخدمة)
            // Arrange
            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ThrowsAsync(new InvalidOperationException("tardiness-failed"));
            var vm = CreateReportViewModel();

            // Act
            vm.GenerateReportCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.StatusMessage.Should().Contain("خطأ:");
            vm.StatusMessage.Should().Contain("tardiness-failed");
            vm.ReportRows.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateReportCommand_WhenServiceReturnsEmpty_ShouldKeepReportRowsEmpty_EdgeGuard()
        {
            // Function: 11.4 — Calculate Tardiness (الحدية: لا بيانات في الفترة)
            // Arrange
            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendanceReportRow>());
            var vm = CreateReportViewModel();

            // Act
            vm.GenerateReportCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.ReportRows.Should().BeEmpty();
            vm.StatusMessage.Should().Contain("0 سجل");
        }

        [Fact]
        public async Task GenerateReportCommand_WithSelectedUserId_ShouldPassFilterToService_SuccessGuard()
        {
            // Function: 11.4 — Calculate Tardiness (تصفية حسب الموظف)
            // Arrange
            var vm = CreateReportViewModel();
            vm.SelectedUserId = 99;
            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 99))
                .ReturnsAsync(new List<AttendanceReportRow>
                {
                    new() { Username = "filtered-user", TotalHours = 7, DelayMinutes = 0, OvertimeMinutes = 0 }
                });

            // Act
            vm.GenerateReportCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.ReportRows.Should().ContainSingle();
            vm.ReportRows[0].Username.Should().Be("filtered-user");
            _tardinessServiceMock.Verify(
                x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 99),
                Times.Once);
        }

        [Fact]
        public async Task GenerateReportCommand_WithMultipleRows_ShouldShowAggregateInStatus_SuccessGuard()
        {
            // Function: 11.4 — Calculate Tardiness (تجميع الإحصاءات)
            // Arrange
            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendanceReportRow>
                {
                    new() { Username = "u1", TotalHours = 8, DelayMinutes = 10, OvertimeMinutes = 30 },
                    new() { Username = "u2", TotalHours = 7, DelayMinutes = 5,  OvertimeMinutes = 60 }
                });
            var vm = CreateReportViewModel();

            // Act
            vm.GenerateReportCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.ReportRows.Should().HaveCount(2);
            vm.StatusMessage.Should().Contain("2 سجل");
            // Aggregated totals appear in the formatted status message
            vm.StatusMessage.Should().Contain("التأخير: 15");
            vm.StatusMessage.Should().Contain("90");
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.5 — Generate Attendance Report (ViewModel)
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GeneratePayrollSummaryCommand_WithValidData_ShouldPopulatePayrollRows_SuccessGuard()
        {
            // Function: 11.5 — Generate Attendance Report
            // Arrange
            _payrollServiceMock
                .Setup(x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendancePayrollSummaryRow>
                {
                    new()
                    {
                        UserId = 1,
                        Username = "emp-report",
                        NetMinutes = 480,
                        DelayMinutes = 15,
                        OvertimeMinutes = 30,
                        AbsentDays = 1,
                        VacationDays = 2,
                        PresentDays = 20
                    }
                });
            var vm = CreateReportViewModel();

            // Act
            vm.GeneratePayrollSummaryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.PayrollRows.Should().ContainSingle();
            vm.PayrollRows[0].Username.Should().Be("emp-report");
            vm.PayrollRows[0].NetMinutes.Should().Be(480);
            vm.PayrollRows[0].DelayMinutes.Should().Be(15);
            vm.PayrollRows[0].OvertimeMinutes.Should().Be(30);
            vm.PayrollRows[0].AbsentDays.Should().Be(1);
            vm.StatusMessage.Should().Contain("ملخص الرواتب");
            vm.StatusMessage.Should().Contain("1 موظف");
        }

        [Fact]
        public async Task GeneratePayrollSummaryCommand_WhenServiceThrows_ShouldSetErrorMessage_FailureGuard()
        {
            // Function: 11.5 — Generate Attendance Report (فشل الخدمة)
            // Arrange
            _payrollServiceMock
                .Setup(x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ThrowsAsync(new InvalidOperationException("payroll-failed"));
            var vm = CreateReportViewModel();

            // Act
            vm.GeneratePayrollSummaryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.StatusMessage.Should().Contain("خطأ:");
            vm.StatusMessage.Should().Contain("payroll-failed");
            vm.PayrollRows.Should().BeEmpty();
        }

        [Fact]
        public async Task GeneratePayrollSummaryCommand_WhenServiceReturnsEmpty_ShouldKeepPayrollRowsEmpty_EdgeGuard()
        {
            // Function: 11.5 — Generate Attendance Report (الحدية: لا توجد بيانات)
            // Arrange
            _payrollServiceMock
                .Setup(x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendancePayrollSummaryRow>());
            var vm = CreateReportViewModel();

            // Act
            vm.GeneratePayrollSummaryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.PayrollRows.Should().BeEmpty();
            vm.StatusMessage.Should().Contain("0 موظف");
        }

        [Fact]
        public async Task GeneratePayrollSummaryCommand_WithMultipleUsers_ShouldAggregateAllRows_SuccessGuard()
        {
            // Function: 11.5 — Generate Attendance Report (تقرير لعدة موظفين)
            // Arrange
            _payrollServiceMock
                .Setup(x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendancePayrollSummaryRow>
                {
                    new() { UserId = 1, Username = "a", NetMinutes = 480, DelayMinutes = 10, OvertimeMinutes = 60, AbsentDays = 0 },
                    new() { UserId = 2, Username = "b", NetMinutes = 450, DelayMinutes = 20, OvertimeMinutes = 30, AbsentDays = 2 },
                    new() { UserId = 3, Username = "c", NetMinutes = 420, DelayMinutes =  0, OvertimeMinutes =  0, AbsentDays = 5 }
                });
            var vm = CreateReportViewModel();

            // Act
            vm.GeneratePayrollSummaryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.PayrollRows.Should().HaveCount(3);
            vm.StatusMessage.Should().Contain("3 موظف");
            vm.StatusMessage.Should().Contain("صافي 1350 دقيقة");
            vm.StatusMessage.Should().Contain("غياب 7 يوم");
        }

        [Fact]
        public async Task GeneratePayrollSummaryCommand_WithSelectedUser_ShouldPassFilterToService_SuccessGuard()
        {
            // Function: 11.5 — Generate Attendance Report (تصفية حسب الموظف)
            // Arrange
            var vm = CreateReportViewModel();
            vm.SelectedUserId = 77;
            _payrollServiceMock
                .Setup(x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 77))
                .ReturnsAsync(new List<AttendancePayrollSummaryRow>
                {
                    new() { UserId = 77, Username = "single", NetMinutes = 300, DelayMinutes = 0, OvertimeMinutes = 0, AbsentDays = 0 }
                });

            // Act
            vm.GeneratePayrollSummaryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.PayrollRows.Should().ContainSingle();
            vm.PayrollRows[0].UserId.Should().Be(77);
            _payrollServiceMock.Verify(
                x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 77),
                Times.Once);
        }

        [Fact]
        public async Task LoadUsersCommand_WithMultipleUsers_ShouldPopulateUsersDropdown_SuccessGuard()
        {
            // Function: 11.5 — Generate Attendance Report (تحميل قائمة الموظفين للفلترة)
            // Arrange
            _userAdminServiceMock.Setup(x => x.GetUsersAsync()).ReturnsAsync(new List<User>
            {
                new() { UserId = 5, Username = "alice" },
                new() { UserId = 6, Username = "bob" }
            });
            var vm = CreateReportViewModel();

            // Act
            vm.LoadUsersCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.Users.Should().Contain(u => u.UserId == null && u.Username == "(جميع المستخدمين)");
            vm.Users.Should().Contain(u => u.UserId == 5 && u.Username == "alice");
            vm.Users.Should().Contain(u => u.UserId == 6 && u.Username == "bob");
            vm.StatusMessage.Should().Contain("تم تحميل 2 مستخدم");
        }

        [Fact]
        public void ClearFilterCommand_AfterCustomization_ShouldResetUserAndDateRange_EdgeGuard()
        {
            // Function: 11.5 — Generate Attendance Report (مسح الفلتر يعيد الافتراضات)
            // Arrange
            var vm = CreateReportViewModel();
            vm.SelectedUserId = 999;
            vm.From = DateTime.Today.AddDays(-30);
            vm.To = DateTime.Today.AddDays(-15);

            // Act
            vm.ClearFilterCommand.Execute(null);

            // Assert
            vm.SelectedUserId.Should().BeNull();
            vm.From.Date.Should().Be(DateTime.Today.AddDays(-7).Date);
            vm.To.Date.Should().Be(DateTime.Today.Date);
        }
    }
}
