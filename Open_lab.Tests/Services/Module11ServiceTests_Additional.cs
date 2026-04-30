using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    /// <summary>
    /// Module 11 — HR & Attendance | موديول الحضور والانصراف
    /// Service-layer unit tests covering the five documented functions:
    ///  11.1 Clock In
    ///  11.2 Clock Out
    ///  11.3 Calculate Working Hours
    ///  11.4 Calculate Tardiness
    ///  11.5 Generate Attendance Report
    ///
    /// Each test follows AAA structure and contains the function-number comment as the first line,
    /// per the UnitTest_Skill.md mandatory rules. All dependencies use either an in-memory EF context
    /// (for service tests that exercise real persistence logic) or Moq for orchestrating components.
    /// </summary>
    public class Module11ServiceTests_Additional : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly AttendanceService _attendanceService;
        private readonly TardinessService _tardinessService;
        private readonly AttendancePayrollReportService _payrollService;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module11ServiceTests_Additional()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _attendanceService = new AttendanceService(_db);
            _tardinessService = new TardinessService(_db);
            _payrollService = new AttendancePayrollReportService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.1 — Clock In
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ClockIn_WithValidUserId_ShouldCreateLogWithLoginTimestamp_SuccessGuard()
        {
            // Function: 11.1 — Clock In
            // Arrange
            var user = new User { Username = "emp_clockin", FullName = "Employee A", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var clockInTime = DateTime.Today.AddHours(8);

            // Act
            var log = await _attendanceService.ClockInAsync(user.UserId, clockInTime, shiftId: null, note: "بداية الدوام");

            // Assert
            log.Should().NotBeNull();
            log.AttendanceLogId.Should().BeGreaterThan(0);
            log.UserId.Should().Be(user.UserId);
            log.LoginAt.Should().Be(clockInTime);
            log.LastActivityAt.Should().Be(clockInTime);
            log.LogoutAt.Should().BeNull();
            log.Note.Should().Be("بداية الدوام");
        }

        [Fact]
        public async Task ClockIn_WhenAlreadyClockedIn_ShouldReturnExistingLog_FailureGuard()
        {
            // Function: 11.1 — Clock In (لا يُسمح بسجلين مفتوحين لنفس الموظف)
            // Arrange
            var user = new User { Username = "emp_double", FullName = "Double Punch", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var first = await _attendanceService.ClockInAsync(user.UserId, DateTime.Today.AddHours(8));

            // Act
            var second = await _attendanceService.ClockInAsync(user.UserId, DateTime.Today.AddHours(9));

            // Assert
            second.Should().NotBeNull();
            second.AttendanceLogId.Should().Be(first.AttendanceLogId);
            (await _db.AttendanceLogs.CountAsync(l => l.UserId == user.UserId)).Should().Be(1);
        }

        [Fact]
        public async Task ClockIn_WithNullTime_ShouldUseCurrentDateTime_EdgeGuard()
        {
            // Function: 11.1 — Clock In (الحالة الحدية: الوقت غير مُمرّر)
            // Arrange
            var user = new User { Username = "emp_now", FullName = "Now User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var beforeCall = DateTime.Now;

            // Act
            var log = await _attendanceService.ClockInAsync(user.UserId, at: null);

            // Assert
            log.LoginAt.Should().BeOnOrAfter(beforeCall.AddSeconds(-1));
            log.LoginAt.Should().BeOnOrBefore(DateTime.Now.AddSeconds(1));
            log.LastActivityAt.Should().Be(log.LoginAt);
        }

        [Fact]
        public async Task ClockIn_WithShiftId_ShouldPersistShiftLink_SuccessGuard()
        {
            // Function: 11.1 — Clock In (ربط السجل بوردية محددة)
            // Arrange
            var user = new User { Username = "emp_shift", FullName = "Shift User", IsActive = true };
            _db.Users.Add(user);
            var shift = new ShiftSchedule
            {
                Name = "Morning",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                GracePeriodMinutes = 5
            };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();

            // Act
            var log = await _attendanceService.ClockInAsync(user.UserId, DateTime.Today.AddHours(8), shiftId: shift.ShiftId);

            // Assert
            log.ShiftId.Should().Be(shift.ShiftId);
            var fromDb = await _db.AttendanceLogs.FindAsync(log.AttendanceLogId);
            fromDb!.ShiftId.Should().Be(shift.ShiftId);
        }

        [Fact]
        public async Task ClockIn_ShouldPersistRecordInDatabase_SuccessGuard()
        {
            // Function: 11.1 — Clock In (التحقق من الحفظ في قاعدة البيانات)
            // Arrange
            var user = new User { Username = "emp_persist", FullName = "Persist User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var log = await _attendanceService.ClockInAsync(user.UserId, DateTime.Today.AddHours(7));

            // Assert
            var saved = await _db.AttendanceLogs.AsNoTracking().FirstOrDefaultAsync(l => l.AttendanceLogId == log.AttendanceLogId);
            saved.Should().NotBeNull();
            saved!.UserId.Should().Be(user.UserId);
            saved.LoginAt.Should().Be(DateTime.Today.AddHours(7));
            saved.LogoutAt.Should().BeNull();
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.2 — Clock Out
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ClockOut_WithOpenLog_ShouldCloseLogAndSetLogoutAt_SuccessGuard()
        {
            // Function: 11.2 — Clock Out
            // Arrange
            var user = new User { Username = "emp_co", FullName = "Out User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var login = DateTime.Today.AddHours(8);
            var logout = DateTime.Today.AddHours(16);
            await _attendanceService.ClockInAsync(user.UserId, login);

            // Act
            var closed = await _attendanceService.ClockOutAsync(user.UserId, logout);

            // Assert
            closed.Should().NotBeNull();
            closed!.LogoutAt.Should().Be(logout);
            closed.LastActivityAt.Should().Be(logout);
            (await _db.AttendanceLogs.FindAsync(closed.AttendanceLogId))!.LogoutAt.Should().Be(logout);
        }

        [Fact]
        public async Task ClockOut_WithoutOpenLog_ShouldReturnNull_FailureGuard()
        {
            // Function: 11.2 — Clock Out (محاولة انصراف بدون حضور سابق)
            // Arrange
            var user = new User { Username = "emp_noopen", FullName = "No Open", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var result = await _attendanceService.ClockOutAsync(user.UserId, DateTime.Today.AddHours(16));

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ClockOut_WithNullTime_ShouldUseCurrentDateTime_EdgeGuard()
        {
            // Function: 11.2 — Clock Out (الحالة الحدية: عدم تمرير وقت الانصراف)
            // Arrange
            var user = new User { Username = "emp_nulltime", FullName = "Null Time", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await _attendanceService.ClockInAsync(user.UserId, DateTime.Now.AddHours(-1));
            var beforeCall = DateTime.Now;

            // Act
            var closed = await _attendanceService.ClockOutAsync(user.UserId, at: null);

            // Assert
            closed.Should().NotBeNull();
            closed!.LogoutAt.Should().NotBeNull();
            closed.LogoutAt!.Value.Should().BeOnOrAfter(beforeCall.AddSeconds(-1));
            closed.LogoutAt.Value.Should().BeOnOrBefore(DateTime.Now.AddSeconds(1));
        }

        [Fact]
        public async Task ClockOut_TwiceOnSameLog_ShouldNotChangeLogoutTime_EdgeGuard()
        {
            // Function: 11.2 — Clock Out (محاولة انصراف مكرر على نفس السجل)
            // Arrange
            var user = new User { Username = "emp_twice", FullName = "Twice", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await _attendanceService.ClockInAsync(user.UserId, DateTime.Today.AddHours(8));
            var firstLogout = DateTime.Today.AddHours(16);
            var firstClose = await _attendanceService.ClockOutAsync(user.UserId, firstLogout);

            // Act
            var secondClose = await _attendanceService.ClockOutAsync(user.UserId, DateTime.Today.AddHours(18));

            // Assert
            // After the first close, GetOpenLogAsync returns null, so a second ClockOut returns null
            // and the original logout time stays intact.
            secondClose.Should().BeNull();
            (await _db.AttendanceLogs.FindAsync(firstClose!.AttendanceLogId))!.LogoutAt.Should().Be(firstLogout);
        }

        [Fact]
        public async Task ClockOut_ShouldOnlyCloseLogForRequestingUser_SuccessGuard()
        {
            // Function: 11.2 — Clock Out (لا يجب أن يؤثر انصراف موظف على آخر)
            // Arrange
            var user1 = new User { Username = "emp1", FullName = "E One", IsActive = true };
            var user2 = new User { Username = "emp2", FullName = "E Two", IsActive = true };
            _db.Users.AddRange(user1, user2);
            await _db.SaveChangesAsync();
            await _attendanceService.ClockInAsync(user1.UserId, DateTime.Today.AddHours(8));
            var open2 = await _attendanceService.ClockInAsync(user2.UserId, DateTime.Today.AddHours(8));

            // Act
            var closed1 = await _attendanceService.ClockOutAsync(user1.UserId, DateTime.Today.AddHours(16));

            // Assert
            closed1.Should().NotBeNull();
            closed1!.UserId.Should().Be(user1.UserId);
            (await _db.AttendanceLogs.FindAsync(open2.AttendanceLogId))!.LogoutAt.Should().BeNull();
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.3 — Calculate Working Hours
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CalculateWorkingHours_WithCompletedShift_ShouldReturnCorrectGrossAndNet_SuccessGuard()
        {
            // Function: 11.3 — Calculate Working Hours
            // Arrange
            var user = new User { Username = "emp_hours", FullName = "Hours User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var login = DateTime.Today.AddHours(8);
            var logout = DateTime.Today.AddHours(16);
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = login,
                LogoutAt = logout,
                LastActivityAt = logout
            });
            await _db.SaveChangesAsync();

            // Act
            var summary = await _attendanceService.GetDailyWorkingSummaryAsync(user.UserId, DateTime.Today);

            // Assert
            summary.GrossMinutes.Should().Be(480, "8 hours = 480 minutes");
            summary.BreakMinutes.Should().Be(0);
            summary.NetMinutes.Should().Be(480);
            summary.HasOpenLog.Should().BeFalse();
            summary.FirstLoginAt.Should().Be(login);
            summary.LastLogoutAt.Should().Be(logout);
        }

        [Fact]
        public async Task CalculateWorkingHours_WithBreaks_ShouldDeductBreakMinutes_SuccessGuard()
        {
            // Function: 11.3 — Calculate Working Hours (استقطاع فترات الراحة)
            // Arrange
            var user = new User { Username = "emp_break", FullName = "Break User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var login = DateTime.Today.AddHours(8);
            var logout = DateTime.Today.AddHours(17);
            var log = new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = login,
                LogoutAt = logout,
                LastActivityAt = logout
            };
            _db.AttendanceLogs.Add(log);
            await _db.SaveChangesAsync();
            _db.AttendanceBreaks.Add(new AttendanceBreak
            {
                AttendanceLogId = log.AttendanceLogId,
                StartAt = DateTime.Today.AddHours(12),
                EndAt = DateTime.Today.AddHours(12).AddMinutes(45),
                Type = "Rest"
            });
            await _db.SaveChangesAsync();

            // Act
            var summary = await _attendanceService.GetDailyWorkingSummaryAsync(user.UserId, DateTime.Today);

            // Assert
            summary.GrossMinutes.Should().Be(540, "9 hours = 540 minutes");
            summary.BreakMinutes.Should().Be(45);
            summary.NetMinutes.Should().Be(495);
        }

        [Fact]
        public async Task CalculateWorkingHours_WithNoLogsForDate_ShouldReturnZeroMinutes_FailureGuard()
        {
            // Function: 11.3 — Calculate Working Hours (لا توجد سجلات لليوم المطلوب)
            // Arrange
            var user = new User { Username = "emp_zero", FullName = "Zero User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var summary = await _attendanceService.GetDailyWorkingSummaryAsync(user.UserId, DateTime.Today);

            // Assert
            summary.UserId.Should().Be(user.UserId);
            summary.GrossMinutes.Should().Be(0);
            summary.BreakMinutes.Should().Be(0);
            summary.NetMinutes.Should().Be(0);
            summary.HasOpenLog.Should().BeFalse();
            summary.FirstLoginAt.Should().BeNull();
            summary.LastLogoutAt.Should().BeNull();
        }

        [Fact]
        public async Task CalculateWorkingHours_WithOpenLog_ShouldComputeAgainstNow_EdgeGuard()
        {
            // Function: 11.3 — Calculate Working Hours (سجل مفتوح، يُحسب حتى لحظة الاستعلام)
            // Arrange
            var user = new User { Username = "emp_open", FullName = "Open User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var login = DateTime.Today.AddHours(8);
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = login,
                LogoutAt = null,
                LastActivityAt = login
            });
            await _db.SaveChangesAsync();
            var fakeNow = DateTime.Today.AddHours(12);

            // Act
            var summary = await _attendanceService.GetDailyWorkingSummaryAsync(user.UserId, DateTime.Today, now: fakeNow);

            // Assert
            summary.HasOpenLog.Should().BeTrue();
            summary.GrossMinutes.Should().Be(240, "12:00 - 08:00 = 4 hours = 240 minutes");
            summary.NetMinutes.Should().Be(240);
            summary.FirstLoginAt.Should().Be(login);
            summary.LastLogoutAt.Should().BeNull();
        }

        [Fact]
        public async Task CalculateWorkingHours_WhenLogoutEqualsLogin_ShouldReturnZeroNetMinutes_EdgeGuard()
        {
            // Function: 11.3 — Calculate Working Hours (الحدية: مدة صفرية)
            // Arrange
            var user = new User { Username = "emp_eq", FullName = "Eq User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var moment = DateTime.Today.AddHours(9);
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = moment,
                LogoutAt = moment,
                LastActivityAt = moment
            });
            await _db.SaveChangesAsync();

            // Act
            var summary = await _attendanceService.GetDailyWorkingSummaryAsync(user.UserId, DateTime.Today);

            // Assert
            summary.GrossMinutes.Should().Be(0);
            summary.NetMinutes.Should().Be(0);
        }

        [Fact]
        public async Task CalculateWorkingHours_ShouldNotIncludeOtherDates_SuccessGuard()
        {
            // Function: 11.3 — Calculate Working Hours (تصفية حسب اليوم المطلوب فقط)
            // Arrange
            var user = new User { Username = "emp_filter", FullName = "Filter User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            // Yesterday log
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = DateTime.Today.AddDays(-1).AddHours(8),
                LogoutAt = DateTime.Today.AddDays(-1).AddHours(16),
                LastActivityAt = DateTime.Today.AddDays(-1).AddHours(16)
            });
            // Today log
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = DateTime.Today.AddHours(9),
                LogoutAt = DateTime.Today.AddHours(13),
                LastActivityAt = DateTime.Today.AddHours(13)
            });
            await _db.SaveChangesAsync();

            // Act
            var summary = await _attendanceService.GetDailyWorkingSummaryAsync(user.UserId, DateTime.Today);

            // Assert
            summary.GrossMinutes.Should().Be(240, "Only today's 4-hour log should be counted");
            summary.NetMinutes.Should().Be(240);
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.4 — Calculate Tardiness
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CalculateTardiness_WhenLoginAfterShiftAndOutsideGrace_ShouldRecordDelay_SuccessGuard()
        {
            // Function: 11.4 — Calculate Tardiness
            // Arrange
            var user = new User { Username = "emp_late", FullName = "Late User", IsActive = true };
            _db.Users.Add(user);
            var shift = new ShiftSchedule
            {
                Name = "Morning",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                GracePeriodMinutes = 5
            };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                ShiftId = shift.ShiftId,
                LoginAt = DateTime.Today.AddHours(8).AddMinutes(20),
                LogoutAt = DateTime.Today.AddHours(16),
                LastActivityAt = DateTime.Today.AddHours(16)
            });
            await _db.SaveChangesAsync();

            // Act
            var report = await _tardinessService.GetPunctualityReportAsync(
                DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), user.UserId);

            // Assert
            report.Should().ContainSingle();
            report[0].DelayMinutes.Should().Be(20);
            report[0].OvertimeMinutes.Should().Be(0);
            report[0].ShiftName.Should().Be("Morning");
        }

        [Fact]
        public async Task CalculateTardiness_WhenLoginWithinGracePeriod_ShouldRecordZeroDelay_EdgeGuard()
        {
            // Function: 11.4 — Calculate Tardiness (التأخير ضمن فترة السماح)
            // Arrange
            var user = new User { Username = "emp_grace", FullName = "Grace User", IsActive = true };
            _db.Users.Add(user);
            var shift = new ShiftSchedule
            {
                Name = "Morning",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                GracePeriodMinutes = 10
            };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                ShiftId = shift.ShiftId,
                LoginAt = DateTime.Today.AddHours(8).AddMinutes(7),
                LogoutAt = DateTime.Today.AddHours(16),
                LastActivityAt = DateTime.Today.AddHours(16)
            });
            await _db.SaveChangesAsync();

            // Act
            var report = await _tardinessService.GetPunctualityReportAsync(
                DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), user.UserId);

            // Assert
            report.Should().ContainSingle();
            report[0].DelayMinutes.Should().Be(0, "Login is within grace period");
        }

        [Fact]
        public async Task CalculateTardiness_WhenLoginBeforeShiftStart_ShouldRecordZeroDelay_EdgeGuard()
        {
            // Function: 11.4 — Calculate Tardiness (الموظف بكر قبل بداية الوردية)
            // Arrange
            var user = new User { Username = "emp_early", FullName = "Early User", IsActive = true };
            _db.Users.Add(user);
            var shift = new ShiftSchedule
            {
                Name = "Morning",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                GracePeriodMinutes = 5
            };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                ShiftId = shift.ShiftId,
                LoginAt = DateTime.Today.AddHours(7).AddMinutes(45),
                LogoutAt = DateTime.Today.AddHours(16),
                LastActivityAt = DateTime.Today.AddHours(16)
            });
            await _db.SaveChangesAsync();

            // Act
            var report = await _tardinessService.GetPunctualityReportAsync(
                DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), user.UserId);

            // Assert
            report.Should().ContainSingle();
            report[0].DelayMinutes.Should().Be(0);
        }

        [Fact]
        public async Task CalculateTardiness_WhenLogoutAfterShiftEnd_ShouldRecordOvertime_SuccessGuard()
        {
            // Function: 11.4 — Calculate Tardiness (تسجيل ساعات إضافية)
            // Arrange
            var user = new User { Username = "emp_ot", FullName = "OT User", IsActive = true };
            _db.Users.Add(user);
            var shift = new ShiftSchedule
            {
                Name = "Morning",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                GracePeriodMinutes = 5
            };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                ShiftId = shift.ShiftId,
                LoginAt = DateTime.Today.AddHours(8),
                LogoutAt = DateTime.Today.AddHours(18),
                LastActivityAt = DateTime.Today.AddHours(18)
            });
            await _db.SaveChangesAsync();

            // Act
            var report = await _tardinessService.GetPunctualityReportAsync(
                DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), user.UserId);

            // Assert
            report.Should().ContainSingle();
            report[0].OvertimeMinutes.Should().Be(120);
            report[0].DelayMinutes.Should().Be(0);
        }

        [Fact]
        public async Task CalculateTardiness_WhenNoShiftAssigned_ShouldRecordZeroDelayAndShowFlexible_EdgeGuard()
        {
            // Function: 11.4 — Calculate Tardiness (وردية مرنة بدون شِفت رسمي)
            // Arrange
            var user = new User { Username = "emp_flex", FullName = "Flex User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                ShiftId = null,
                LoginAt = DateTime.Today.AddHours(10).AddMinutes(30),
                LogoutAt = DateTime.Today.AddHours(18),
                LastActivityAt = DateTime.Today.AddHours(18)
            });
            await _db.SaveChangesAsync();

            // Act
            var report = await _tardinessService.GetPunctualityReportAsync(
                DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), user.UserId);

            // Assert
            report.Should().ContainSingle();
            report[0].DelayMinutes.Should().Be(0);
            report[0].OvertimeMinutes.Should().Be(0);
            report[0].ShiftName.Should().Be("مرنة");
        }

        [Fact]
        public async Task CalculateTardiness_FilterByUserId_ShouldOnlyReturnRequestedUser_SuccessGuard()
        {
            // Function: 11.4 — Calculate Tardiness (تصفية حسب الموظف)
            // Arrange
            var user1 = new User { Username = "u-a", FullName = "A", IsActive = true };
            var user2 = new User { Username = "u-b", FullName = "B", IsActive = true };
            _db.Users.AddRange(user1, user2);
            await _db.SaveChangesAsync();
            _db.AttendanceLogs.AddRange(
                new AttendanceLog { UserId = user1.UserId, LoginAt = DateTime.Today.AddHours(8), LogoutAt = DateTime.Today.AddHours(16) },
                new AttendanceLog { UserId = user2.UserId, LoginAt = DateTime.Today.AddHours(9), LogoutAt = DateTime.Today.AddHours(17) });
            await _db.SaveChangesAsync();

            // Act
            var report = await _tardinessService.GetPunctualityReportAsync(
                DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), user1.UserId);

            // Assert
            report.Should().ContainSingle();
            report[0].Username.Should().Be("u-a");
        }

        [Fact]
        public async Task CalculateTardiness_WhenNoLogsInPeriod_ShouldReturnEmpty_FailureGuard()
        {
            // Function: 11.4 — Calculate Tardiness (لا توجد بيانات في الفترة)
            // Arrange & Act
            var report = await _tardinessService.GetPunctualityReportAsync(
                DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-29));

            // Assert
            report.Should().BeEmpty();
        }

        // ────────────────────────────────────────────────────────────────────
        // 11.5 — Generate Attendance Report
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GenerateAttendanceReport_WithMonthlyData_ShouldAggregateAllMetrics_SuccessGuard()
        {
            // Function: 11.5 — Generate Attendance Report (تقرير شهري شامل)
            // Arrange
            var user = new User { Username = "emp_report", FullName = "Report User", IsActive = true };
            _db.Users.Add(user);
            var shift = new ShiftSchedule
            {
                Name = "Morning",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                GracePeriodMinutes = 5
            };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();
            var log = new AttendanceLog
            {
                UserId = user.UserId,
                ShiftId = shift.ShiftId,
                LoginAt = DateTime.Today.AddHours(8).AddMinutes(20),
                LogoutAt = DateTime.Today.AddHours(17),
                LastActivityAt = DateTime.Today.AddHours(17)
            };
            _db.AttendanceLogs.Add(log);
            await _db.SaveChangesAsync();
            _db.AttendanceBreaks.Add(new AttendanceBreak
            {
                AttendanceLogId = log.AttendanceLogId,
                StartAt = DateTime.Today.AddHours(12),
                EndAt = DateTime.Today.AddHours(12).AddMinutes(30),
                Type = "Rest"
            });
            _db.AttendanceDayStatuses.Add(new AttendanceDayStatus
            {
                UserId = user.UserId,
                Date = DateTime.Today.AddDays(1),
                Status = "Vacation"
            });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _payrollService.GeneratePayrollSummaryAsync(
                DateTime.Today, DateTime.Today.AddDays(1), user.UserId);

            // Assert
            rows.Should().ContainSingle();
            var row = rows[0];
            row.UserId.Should().Be(user.UserId);
            row.Username.Should().Be("emp_report");
            row.GrossMinutes.Should().Be(520, "8h 40m = 520 minutes");
            row.BreakMinutes.Should().Be(30);
            row.NetMinutes.Should().Be(490);
            row.DelayMinutes.Should().Be(20);
            row.OvertimeMinutes.Should().Be(60);
            row.PresentDays.Should().Be(1);
            row.VacationDays.Should().Be(1);
            row.AbsentDays.Should().Be(0);
        }

        [Fact]
        public async Task GenerateAttendanceReport_WithReversedDateRange_ShouldNormalizeRange_EdgeGuard()
        {
            // Function: 11.5 — Generate Attendance Report (نطاق تواريخ معكوس)
            // Arrange
            var user = new User { Username = "emp_rev", FullName = "Rev User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act (To قبل From عمداً)
            var rows = await _payrollService.GeneratePayrollSummaryAsync(
                DateTime.Today.AddDays(2), DateTime.Today.AddDays(-2), user.UserId);

            // Assert
            rows.Should().ContainSingle();
            rows[0].From.Date.Should().Be(DateTime.Today.AddDays(-2).Date);
            rows[0].To.Date.Should().Be(DateTime.Today.AddDays(2).Date);
        }

        [Fact]
        public async Task GenerateAttendanceReport_WithUserNotFound_ShouldReturnEmptyList_FailureGuard()
        {
            // Function: 11.5 — Generate Attendance Report (موظف غير موجود)
            // Arrange
            var user = new User { Username = "ignored", FullName = "Ignored", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var rows = await _payrollService.GeneratePayrollSummaryAsync(
                DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), userId: 99999);

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateAttendanceReport_WithMultipleStatusTypes_ShouldClassifyCorrectly_SuccessGuard()
        {
            // Function: 11.5 — Generate Attendance Report (تصنيف الحالات: حضور، غياب، إجازة، عطلة، مرضية)
            // Arrange
            var user = new User { Username = "emp_status", FullName = "Status User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var baseDate = DateTime.Today;
            _db.AttendanceDayStatuses.AddRange(
                new AttendanceDayStatus { UserId = user.UserId, Date = baseDate, Status = "Present" },
                new AttendanceDayStatus { UserId = user.UserId, Date = baseDate.AddDays(1), Status = "Vacation" },
                new AttendanceDayStatus { UserId = user.UserId, Date = baseDate.AddDays(2), Status = "Holiday" },
                new AttendanceDayStatus { UserId = user.UserId, Date = baseDate.AddDays(3), Status = "SickLeave" },
                new AttendanceDayStatus { UserId = user.UserId, Date = baseDate.AddDays(4), Status = "Absent" });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _payrollService.GeneratePayrollSummaryAsync(baseDate, baseDate.AddDays(4), user.UserId);

            // Assert
            rows.Should().ContainSingle();
            var row = rows[0];
            row.PresentDays.Should().Be(1);
            row.VacationDays.Should().Be(1);
            row.HolidayDays.Should().Be(1);
            row.SickLeaveDays.Should().Be(1);
            row.AbsentDays.Should().Be(1);
        }

        [Fact]
        public async Task GenerateAttendanceReport_WithUnknownStatus_ShouldCountAsOther_EdgeGuard()
        {
            // Function: 11.5 — Generate Attendance Report (حالة غير معروفة)
            // Arrange
            var user = new User { Username = "emp_other", FullName = "Other User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            _db.AttendanceDayStatuses.Add(new AttendanceDayStatus
            {
                UserId = user.UserId,
                Date = DateTime.Today,
                Status = "Training"
            });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _payrollService.GeneratePayrollSummaryAsync(
                DateTime.Today, DateTime.Today, user.UserId);

            // Assert
            rows.Should().ContainSingle();
            rows[0].OtherStatusDays.Should().Be(1);
            rows[0].PresentDays.Should().Be(0);
        }

        [Fact]
        public async Task GenerateAttendanceReport_WithoutUserFilter_ShouldReturnRowsForAllUsers_SuccessGuard()
        {
            // Function: 11.5 — Generate Attendance Report (تقرير لكل المستخدمين)
            // Arrange
            var u1 = new User { Username = "user-a", FullName = "A", IsActive = true };
            var u2 = new User { Username = "user-b", FullName = "B", IsActive = true };
            _db.Users.AddRange(u1, u2);
            await _db.SaveChangesAsync();

            // Act
            var rows = await _payrollService.GeneratePayrollSummaryAsync(
                DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), userId: null);

            // Assert
            rows.Should().HaveCount(2);
            rows.Should().Contain(r => r.Username == "user-a");
            rows.Should().Contain(r => r.Username == "user-b");
        }

        [Fact]
        public async Task GenerateAttendanceReport_WithLogsButNoStatuses_ShouldCountDaysWithLogsAsPresent_SuccessGuard()
        {
            // Function: 11.5 — Generate Attendance Report (احتساب الحضور تلقائياً من سجلات الدخول)
            // The production query filters logs by `LoginAt >= rangeFrom && LoginAt <= rangeTo`, so the
            // upper bound must extend to the end of the day for an 08:00 login to be included.
            // Arrange
            var user = new User { Username = "emp_auto", FullName = "Auto User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            var login = DateTime.Today.AddHours(8);
            var logout = DateTime.Today.AddHours(16);
            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = login,
                LogoutAt = logout,
                LastActivityAt = logout
            });
            await _db.SaveChangesAsync();

            // Act
            var rangeFrom = DateTime.Today;
            var rangeTo = DateTime.Today.AddDays(1).AddSeconds(-1);
            var rows = await _payrollService.GeneratePayrollSummaryAsync(
                rangeFrom, rangeTo, user.UserId);

            // Assert
            rows.Should().ContainSingle();
            rows[0].PresentDays.Should().Be(1, "Day with login record but no explicit status should count as present");
            rows[0].AbsentDays.Should().Be(0);
            rows[0].GrossMinutes.Should().Be(480);
        }

        [Fact]
        public async Task GenerateAttendanceReport_WithoutLogsAndStatuses_ShouldCountAllDaysAsAbsent_EdgeGuard()
        {
            // Function: 11.5 — Generate Attendance Report (لا سجلات ولا حالات => غياب)
            // Arrange
            var user = new User { Username = "emp_abs", FullName = "Abs User", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var rows = await _payrollService.GeneratePayrollSummaryAsync(
                DateTime.Today.AddDays(-2), DateTime.Today, user.UserId);

            // Assert
            rows.Should().ContainSingle();
            rows[0].AbsentDays.Should().Be(3, "Three consecutive days with no logs and no statuses");
            rows[0].PresentDays.Should().Be(0);
        }
    }
}
