using System;
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
    public class AttendanceServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly AttendanceService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public AttendanceServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new AttendanceService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateLoginAsync_Should_Create_Log_LogicGuard()
        {
            // Refactored to Logic Guard - verifies complete log creation and side effects
            // Arrange
            var user = new User { Username = "testuser", FullName = "Test User" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var log = await _service.CreateLoginAsync(user.UserId, "Test login note");

            // Assert - Logic Guard: Verify complete log data
            log.AttendanceLogId.Should().BeGreaterThan(0);
            log.UserId.Should().Be(user.UserId);
            log.Note.Should().Be("Test login note");
            log.LoginAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
            log.LastActivityAt.Should().BeCloseTo(log.LoginAt, TimeSpan.FromSeconds(1));
            log.LogoutAt.Should().BeNull("Logout should be null for new login");
            
            // Assert - Logic Guard: Verify log is persisted in database
            var savedLog = await _db.AttendanceLogs.FindAsync(log.AttendanceLogId);
            savedLog.Should().NotBeNull();
            savedLog!.UserId.Should().Be(user.UserId);
            savedLog.LastActivityAt.Should().BeCloseTo(savedLog.LoginAt, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CloseAsync_Should_Set_LogoutAt()
        {
            var created = await _service.CreateLoginAsync(6, null);
            await _service.CloseAsync(created.AttendanceLogId);
            var updated = await _db.AttendanceLogs.FindAsync(created.AttendanceLogId);
            updated.Should().NotBeNull();
            updated!.LogoutAt.Should().NotBeNull();
            updated.LastActivityAt.Should().BeCloseTo(updated.LogoutAt!.Value, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GetLogsAsync_Should_Filter_By_Date()
        {
            var user = new User { Username = "u" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var log = new AttendanceLog { UserId = user.UserId, LoginAt = DateTime.Today };
            _db.AttendanceLogs.Add(log);
            await _db.SaveChangesAsync();

            var rows = await _service.GetLogsAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            rows.Should().ContainSingle();
        }

        // 11.3, 11.5 Time Calculation Tests - NEW TEST

        [Fact]
        public async Task CalculateWorkHours_Should_Validate_Regular_Overtime_Delay_LogicGuard()
        {
            // 11.3 Calculate Work Hours - Logic Guard: Verify regular, overtime, and delay calculation
            // Arrange
            var user = new User { Username = "timecalc", FullName = "Time Calc User" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var shift = new ShiftSchedule 
            { 
                Name = "Standard Shift", 
                StartTime = new TimeSpan(8, 0, 0), 
                EndTime = new TimeSpan(16, 0, 0), 
                GracePeriodMinutes = 15 
            };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();

            // Scenario: Login at 8:30 (30 min late), Logout at 17:00 (1 hour overtime)
            var loginTime = DateTime.Today.AddHours(8).AddMinutes(30);
            var logoutTime = DateTime.Today.AddHours(17);

            var log = await _service.CreateLoginAsync(user.UserId, "Test work hours calculation");
            log.LoginAt = loginTime;
            log.ShiftId = shift.ShiftId;
            await _db.SaveChangesAsync();

            await _service.CloseAsync(log.AttendanceLogId);
            var updatedLog = await _db.AttendanceLogs.FindAsync(log.AttendanceLogId);
            updatedLog!.LogoutAt = logoutTime;
            await _db.SaveChangesAsync();

            // Act - Calculate work hours
            var workHours = updatedLog.LogoutAt.Value - updatedLog.LoginAt;
            var expectedWorkHours = TimeSpan.FromHours(8.5); // 8:30 to 17:00
            var shiftDuration = shift.EndTime - shift.StartTime; // 8 hours
            var delayMinutes = (int)(loginTime - DateTime.Today.Add(shift.StartTime)).TotalMinutes;
            var overtimeMinutes = (int)(logoutTime - DateTime.Today.Add(shift.EndTime)).TotalMinutes;

            // Assert - Logic Guard: Verify work hours, delay, and overtime calculations
            workHours.Should().BeCloseTo(expectedWorkHours, TimeSpan.FromMinutes(1));
            delayMinutes.Should().Be(30, "Should be 30 minutes late");
            overtimeMinutes.Should().Be(60, "Should be 60 minutes overtime");
            updatedLog.UserId.Should().Be(user.UserId);
            updatedLog.ShiftId.Should().Be(shift.ShiftId);
        }

        [Fact]
        public async Task ClockOutAsync_When_No_Open_Log_Should_Return_Null()
        {
            var result = await _service.ClockOutAsync(userId: 9999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ClockInAsync_When_Open_Log_Exists_Should_Return_Existing_Log_Edge()
        {
            // Arrange
            var first = await _service.ClockInAsync(50, DateTime.Today.AddHours(8));

            // Act
            var second = await _service.ClockInAsync(50, DateTime.Today.AddHours(9));

            // Assert
            second.AttendanceLogId.Should().Be(first.AttendanceLogId);
            (await _db.AttendanceLogs.CountAsync(l => l.UserId == 50)).Should().Be(1);
        }

        [Fact]
        public async Task GetOpenLogAsync_When_Open_Log_Exists_Should_Return_It_Success()
        {
            // Arrange
            var created = await _service.ClockInAsync(88, DateTime.Today.AddHours(7));

            // Act
            var open = await _service.GetOpenLogAsync(88);

            // Assert
            open.Should().NotBeNull();
            open!.AttendanceLogId.Should().Be(created.AttendanceLogId);
        }

        [Fact]
        public async Task StartBreakAsync_When_No_Open_Log_Should_Return_Null_Failure()
        {
            // Act
            var br = await _service.StartBreakAsync(123);

            // Assert
            br.Should().BeNull();
        }

        [Fact]
        public async Task StartBreakAsync_When_Open_Break_Already_Exists_Should_Return_Same_Break_Edge()
        {
            // Arrange
            await _service.ClockInAsync(77, DateTime.Today.AddHours(8));
            var firstBreak = await _service.StartBreakAsync(77, "Rest", DateTime.Today.AddHours(10));

            // Act
            var secondBreak = await _service.StartBreakAsync(77, "Meal", DateTime.Today.AddHours(11));

            // Assert
            secondBreak.Should().NotBeNull();
            secondBreak!.BreakId.Should().Be(firstBreak!.BreakId);
        }

        [Fact]
        public async Task EndBreakAsync_When_No_Open_Break_Should_Return_Null_Failure()
        {
            // Arrange
            await _service.ClockInAsync(66, DateTime.Today.AddHours(8));

            // Act
            var br = await _service.EndBreakAsync(66);

            // Assert
            br.Should().BeNull();
        }

        [Fact]
        public async Task GetDailyWorkingSummaryAsync_When_No_Logs_Should_Return_Zeroes_Edge()
        {
            // Act
            var summary = await _service.GetDailyWorkingSummaryAsync(999, DateTime.Today, DateTime.Today.AddHours(12));

            // Assert
            summary.UserId.Should().Be(999);
            summary.GrossMinutes.Should().Be(0);
            summary.BreakMinutes.Should().Be(0);
            summary.NetMinutes.Should().Be(0);
            summary.HasOpenLog.Should().BeFalse();
        }

        [Fact]
        public async Task GetDailyWorkingSummaryAsync_Should_Subtract_Break_Minutes()
        {
            var user = new User { Username = "summary-user", FullName = "Summary User" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var loginAt = DateTime.Today.AddHours(8);
            var logoutAt = DateTime.Today.AddHours(16);

            var log = await _service.ClockInAsync(user.UserId, loginAt);
            await _service.StartBreakAsync(user.UserId, at: DateTime.Today.AddHours(12));
            await _service.EndBreakAsync(user.UserId, at: DateTime.Today.AddHours(12).AddMinutes(30));
            await _service.ClockOutAsync(user.UserId, logoutAt);

            var summary = await _service.GetDailyWorkingSummaryAsync(user.UserId, DateTime.Today, now: DateTime.Today.AddHours(18));

            summary.UserId.Should().Be(user.UserId);
            summary.GrossMinutes.Should().Be(480);
            summary.BreakMinutes.Should().Be(30);
            summary.NetMinutes.Should().Be(450);
            summary.HasOpenLog.Should().BeFalse();
            summary.FirstLoginAt.Should().Be(loginAt);
            summary.LastLogoutAt.Should().Be(logoutAt);
        }
    }
}
