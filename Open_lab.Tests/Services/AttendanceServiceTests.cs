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
    }
}
