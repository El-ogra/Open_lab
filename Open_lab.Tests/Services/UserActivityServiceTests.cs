using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    /// <summary>
    /// Tests for Module 10: Function 10.6 (View User Activity Log)
    /// BR-SEC-002: Audit trail — records who did what and when
    /// </summary>
    public class UserActivityServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly UserActivityService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public UserActivityServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new UserActivityService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.6 — View User Activity Log (BR-SEC-002)
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetUserActivityLog_WithValidRequest_ShouldReturnRecentActivities()
        {
            // Function: 10.6 — View User Activity Log (BR-SEC-002: audit trail)
            // Arrange
            var user = new User { Username = "u-success" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.AuditLogs.Add(new AuditLog
            {
                UserId = user.UserId,
                Action = "Insert",
                TableName = "Patients",
                RecordId = "15",
                Timestamp = DateTime.Now
            });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetRecentActivitiesAsync();

            // Assert
            rows.Should().ContainSingle();
            rows[0].Username.Should().Be("u-success");
            rows[0].ActivityDescription.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GetUserActivityLog_WhenServiceError_ShouldReturnEmptyOrThrow()
        {
            // Function: 10.6 — View User Activity Log (failure: no records exist)
            // Arrange — no audit logs in DB

            // Act
            var rows = await _service.GetRecentActivitiesAsync();

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserActivityLog_WithUserFilter_ShouldReturnOnlySelectedUserRows()
        {
            // Function: 10.6 — View User Activity Log (edge: filter by user — BR-SEC-002)
            // Arrange
            var user1 = new User { Username = "u1" };
            var user2 = new User { Username = "u2" };
            _db.Users.AddRange(user1, user2);
            await _db.SaveChangesAsync();

            _db.AuditLogs.AddRange(
                new AuditLog
                {
                    UserId = user1.UserId,
                    Action = "Insert",
                    TableName = "Patients",
                    RecordId = "1",
                    Timestamp = DateTime.Now
                },
                new AuditLog
                {
                    UserId = user2.UserId,
                    Action = "Insert",
                    TableName = "Visits",
                    RecordId = "2",
                    Timestamp = DateTime.Now.AddMinutes(-1)
                });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetRecentActivitiesAsync(user1.UserId, 100);

            // Assert
            rows.Should().ContainSingle();
            rows[0].Username.Should().Be("u1");
            rows[0].TableName.Should().Be("Patients");
        }

        [Fact]
        public async Task GetUserActivityLog_ShouldRespectCountLimit()
        {
            // Function: 10.6 — View User Activity Log (edge: count limit respected)
            // Arrange
            var user = new User { Username = "u-limit" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.AuditLogs.AddRange(
                new AuditLog
                {
                    UserId = user.UserId,
                    Action = "Insert",
                    TableName = "Patients",
                    RecordId = "1",
                    Timestamp = DateTime.Now
                },
                new AuditLog
                {
                    UserId = user.UserId,
                    Action = "Update",
                    TableName = "Patients",
                    RecordId = "2",
                    Timestamp = DateTime.Now.AddSeconds(-1)
                });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetRecentActivitiesAsync(user.UserId, 1);

            // Assert
            rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task SimplifyAuditLog_WithFieldChanges_ShouldDescribeChanges()
        {
            // Function: 10.6 — View User Activity Log (BR-SEC-002: human-readable description)
            // Arrange
            var user = new User { Username = "admin" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.AuditLogs.Add(new AuditLog
            {
                UserId = user.UserId,
                Action = "Update",
                TableName = "Patients",
                RecordId = "10",
                OldValues = "{\"FullName\":\"Old\"}",
                NewValues = "{\"FullName\":\"New\"}",
                Timestamp = DateTime.Now
            });
            await _db.SaveChangesAsync();

            // Act
            var text = await _service.SimplifyAuditLogAsync(1);

            // Assert
            text.Should().Contain("تعديل");
            (text.Contains("Patients") || text.Contains("بيانات مريض")).Should().BeTrue();
        }

        [Fact]
        public async Task SimplifyAuditLog_WhenLogNotFound_ShouldReturnNotFoundMessage()
        {
            // Function: 10.6 — View User Activity Log (failure: log ID not found)
            // Arrange & Act
            // Act
            var text = await _service.SimplifyAuditLogAsync(9999);

            // Assert
            text.Should().Be("السجل غير موجود");
        }
    }
}
