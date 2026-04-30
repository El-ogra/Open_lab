using System;
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
    /// Tests for Module 10: Function 10.7 (Monitor System Usage)
    /// Tracks active user sessions via AttendanceLogs
    /// </summary>
    public class SystemMonitorServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly SystemMonitorService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public SystemMonitorServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new SystemMonitorService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.7 — Monitor System Usage
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task MonitorSystemUsage_GetActiveSessions_ShouldReturnOnlyOpenSessions()
        {
            // Function: 10.7 — Monitor System Usage
            // Arrange
            var user = new User { Username = "open-user", FullName = "Open User" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.AttendanceLogs.AddRange(
                new AttendanceLog
                {
                    UserId = user.UserId,
                    LoginAt = DateTime.Today.AddHours(9),
                    LastActivityAt = DateTime.Today.AddHours(10)
                    // LogoutAt = null → open session
                },
                new AttendanceLog
                {
                    UserId = user.UserId,
                    LoginAt = DateTime.Today.AddHours(7),
                    LogoutAt = DateTime.Today.AddHours(8),
                    LastActivityAt = DateTime.Today.AddHours(8)
                    // closed session — should be excluded
                });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetActiveSessionsAsync();

            // Assert
            rows.Should().ContainSingle();
            rows[0].Username.Should().Be("open-user");
            rows[0].Status.Should().Be("متصل");
        }

        [Fact]
        public async Task MonitorSystemUsage_WhenNoOpenSessions_ShouldReturnEmpty()
        {
            // Function: 10.7 — Monitor System Usage (failure: all sessions closed)
            // Arrange
            var user = new User { Username = "closed" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = DateTime.Today.AddHours(8),
                LogoutAt = DateTime.Today.AddHours(16),
                LastActivityAt = DateTime.Today.AddHours(16)
            });
            await _db.SaveChangesAsync();

            // Act
            var active = await _service.GetActiveSessionsAsync();

            // Assert
            active.Should().BeEmpty();
        }

        [Fact]
        public async Task MonitorSystemUsage_MarkActivity_ShouldUpdateMostRecentOpenSession()
        {
            // Function: 10.7 — Monitor System Usage (edge: activity heartbeat)
            // Arrange
            var user = new User { Username = "active" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var oldLog = new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = DateTime.Today.AddDays(-1).AddHours(8),
                LastActivityAt = DateTime.Today.AddDays(-1).AddHours(8)
            };
            var latestLog = new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = DateTime.Today.AddDays(-1).AddHours(9),
                LastActivityAt = DateTime.Today.AddDays(-1).AddHours(9)
            };
            _db.AttendanceLogs.AddRange(oldLog, latestLog);
            await _db.SaveChangesAsync();

            // Act
            await _service.MarkActivityAsync(user.UserId);

            // Assert
            var refreshedOld = await _db.AttendanceLogs
                .SingleAsync(x => x.AttendanceLogId == oldLog.AttendanceLogId);
            var refreshedLatest = await _db.AttendanceLogs
                .SingleAsync(x => x.AttendanceLogId == latestLog.AttendanceLogId);

            refreshedLatest.LastActivityAt.Should().BeAfter(refreshedOld.LastActivityAt);
            refreshedOld.LastActivityAt.Should().Be(oldLog.LastActivityAt);
        }

        [Fact]
        public async Task MonitorSystemUsage_MarkActivity_WhenNoOpenSession_ShouldNotModifyClosedSession()
        {
            // Function: 10.7 — Monitor System Usage (edge: mark activity on closed session)
            // Arrange
            var user = new User { Username = "none-open" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var closed = new AttendanceLog
            {
                UserId = user.UserId,
                LoginAt = DateTime.Today.AddHours(7),
                LogoutAt = DateTime.Today.AddHours(8),
                LastActivityAt = DateTime.Today.AddHours(8)
            };
            _db.AttendanceLogs.Add(closed);
            await _db.SaveChangesAsync();

            // Act
            await _service.MarkActivityAsync(user.UserId);

            // Assert
            var refreshed = await _db.AttendanceLogs
                .SingleAsync(x => x.AttendanceLogId == closed.AttendanceLogId);
            refreshed.LastActivityAt.Should().Be(closed.LastActivityAt);
        }
    }
}
