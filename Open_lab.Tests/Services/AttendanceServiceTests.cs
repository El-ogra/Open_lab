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
    /// <summary>
    /// Tests for Module 10: Functions 10.4 (Record Attendance) and 10.5 (Record Departure)
    /// These functions relate to the system login/logout attendance tracking.
    /// BR-SEC-004: Every login creates a timestamped attendance record; logout closes it.
    /// </summary>
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

        // ──────────────────────────────────────────────────────────────────
        // 10.4 — Record Attendance (BR-SEC-004)
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RecordAttendance_WithValidUserId_ShouldCreateLogWithTimestamp()
        {
            // Function: 10.4 — Record Attendance
            // Arrange
            var user = new User { Username = "testuser", FullName = "Test User" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var log = await _service.CreateLoginAsync(user.UserId, "تسجيل دخول");

            // Assert
            log.AttendanceLogId.Should().BeGreaterThan(0);
            log.UserId.Should().Be(user.UserId);
            log.Note.Should().Be("تسجيل دخول");
            log.LoginAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
            log.LastActivityAt.Should().BeCloseTo(log.LoginAt, TimeSpan.FromSeconds(1));
            log.LogoutAt.Should().BeNull("Logout should be null when attendance is first recorded");
        }

        [Fact]
        public async Task RecordAttendance_WhenOpenLogAlreadyExists_ShouldReturnExistingLog()
        {
            // Function: 10.4 — Record Attendance (BR-SEC-004: one open session per user)
            // Arrange
            var first = await _service.ClockInAsync(50, DateTime.Today.AddHours(8));

            // Act
            var second = await _service.ClockInAsync(50, DateTime.Today.AddHours(9));

            // Assert
            second.AttendanceLogId.Should().Be(first.AttendanceLogId);
            (await _db.AttendanceLogs.CountAsync(l => l.UserId == 50)).Should().Be(1);
        }

        [Fact]
        public async Task RecordAttendance_ShouldPersistLogInDatabase()
        {
            // Function: 10.4 — Record Attendance (persistence check)
            // Arrange
            var user = new User { Username = "persist_user" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var log = await _service.CreateLoginAsync(user.UserId, null);

            // Assert
            var savedLog = await _db.AttendanceLogs.FindAsync(log.AttendanceLogId);
            savedLog.Should().NotBeNull();
            savedLog!.UserId.Should().Be(user.UserId);
        }

        [Fact]
        public async Task RecordAttendance_GetOpenLog_WhenOpenLogExists_ShouldReturnIt()
        {
            // Function: 10.4 — Record Attendance (edge: retrieve open session)
            // Arrange
            var created = await _service.ClockInAsync(88, DateTime.Today.AddHours(7));

            // Act
            var open = await _service.GetOpenLogAsync(88);

            // Assert
            open.Should().NotBeNull();
            open!.AttendanceLogId.Should().Be(created.AttendanceLogId);
        }

        [Fact]
        public async Task RecordAttendance_GetOpenLog_WhenNoOpenLog_ShouldReturnNull()
        {
            // Function: 10.4 — Record Attendance (edge: no open session)
            // Arrange & Act
            // Act
            var open = await _service.GetOpenLogAsync(9999);

            // Assert
            open.Should().BeNull();
        }

        [Fact]
        public async Task RecordAttendance_WithCustomTime_ShouldUseProvidedTime()
        {
            // Function: 10.4 — Record Attendance (BR-SEC-004: timestamp integrity)
            // Arrange
            var loginTime = DateTime.Today.AddHours(8).AddMinutes(30);

            // Act
            var log = await _service.ClockInAsync(101, loginTime);

            // Assert
            log.LoginAt.Should().Be(loginTime);
            log.LastActivityAt.Should().Be(loginTime);
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.5 — Record Departure (BR-SEC-004)
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RecordDeparture_WhenOpenLogExists_ShouldSetLogoutAt()
        {
            // Function: 10.5 — Record Departure
            // Arrange
            var created = await _service.CreateLoginAsync(6, null);

            // Act
            await _service.CloseAsync(created.AttendanceLogId);

            // Assert
            var updated = await _db.AttendanceLogs.FindAsync(created.AttendanceLogId);
            updated.Should().NotBeNull();
            updated!.LogoutAt.Should().NotBeNull();
            updated.LastActivityAt.Should().BeCloseTo(
                updated.LogoutAt!.Value, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task RecordDeparture_WhenNoOpenLog_ShouldReturnNull()
        {
            // Function: 10.5 — Record Departure (failure: no open session to close)
            // Arrange & Act
            // Act
            var result = await _service.ClockOutAsync(userId: 9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RecordDeparture_WhenOpenLogExists_ShouldCloseCorrectLog()
        {
            // Function: 10.5 — Record Departure (BR-SEC-004: exact logout time)
            // Arrange
            var open = await _service.ClockInAsync(444, DateTime.Today.AddHours(8));

            // Act
            var closed = await _service.ClockOutAsync(444, DateTime.Today.AddHours(16));

            // Assert
            closed.Should().NotBeNull();
            closed!.AttendanceLogId.Should().Be(open.AttendanceLogId);
            closed.LogoutAt.Should().Be(DateTime.Today.AddHours(16));
        }

        [Fact]
        public async Task RecordDeparture_WithCustomLogoutTime_ShouldUseProvidedTime()
        {
            // Function: 10.5 — Record Departure (BR-SEC-004: timestamp integrity)
            // Arrange
            var logoutTime = DateTime.Today.AddHours(17).AddMinutes(15);
            await _service.ClockInAsync(200, DateTime.Today.AddHours(8));

            // Act
            var closed = await _service.ClockOutAsync(200, logoutTime);

            // Assert
            closed!.LogoutAt.Should().Be(logoutTime);
            closed.LastActivityAt.Should().Be(logoutTime);
        }

        [Fact]
        public async Task RecordDeparture_GetLogsAsync_ShouldFilterByDateRange()
        {
            // Function: 10.5 — Record Departure (edge: log retrieval by date)
            // Arrange
            var user = new User { Username = "u" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var log = new AttendanceLog { UserId = user.UserId, LoginAt = DateTime.Today };
            _db.AttendanceLogs.Add(log);
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetLogsAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
        }

        // ──────────────────────────────────────────────────────────────────
        // Break handling — extends 10.4 / 10.5
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task StartBreak_WhenNoOpenLog_ShouldReturnNull()
        {
            // Function: 10.4 — Record Attendance (edge: break requires open session)
            // Arrange & Act
            // Act
            var br = await _service.StartBreakAsync(123);

            // Assert
            br.Should().BeNull();
        }

        [Fact]
        public async Task StartBreak_WhenOpenBreakAlreadyExists_ShouldReturnSameBreak()
        {
            // Function: 10.4 — Record Attendance (edge: only one break at a time)
            // Arrange
            await _service.ClockInAsync(77, DateTime.Today.AddHours(8));
            var firstBreak = await _service.StartBreakAsync(77, "Rest", DateTime.Today.AddHours(10));

            // Act
            var secondBreak = await _service.StartBreakAsync(77, "Rest", DateTime.Today.AddHours(10).AddMinutes(5));

            // Assert
            secondBreak.Should().NotBeNull();
            secondBreak!.BreakId.Should().Be(firstBreak!.BreakId);
        }
    }
}
