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

        [Fact]
        public async Task SimplifyAuditLogAsync_Should_Describe_Field_Changes()
        {
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

            var text = await _service.SimplifyAuditLogAsync(1);

            text.Should().Contain("تعديل");
            (text.Contains("Patients") || text.Contains("بيانات مريض")).Should().BeTrue();
        }

        [Fact]
        public async Task SimplifyAuditLogAsync_When_LogNotFound_Should_Return_NotFoundMessage_FailureGuard()
        {
            // Act
            var text = await _service.SimplifyAuditLogAsync(9999);

            // Assert
            text.Should().Be("السجل غير موجود");
        }

        [Fact]
        public async Task GetRecentActivitiesAsync_With_UserFilter_Should_Return_Only_SelectedUserRows_EdgeGuard()
        {
            // Arrange
            var user1 = new User { Username = "u1" };
            var user2 = new User { Username = "u2" };
            _db.Users.AddRange(user1, user2);
            await _db.SaveChangesAsync();

            _db.AuditLogs.AddRange(
                new AuditLog { UserId = user1.UserId, Action = "Insert", TableName = "Patients", RecordId = "1", Timestamp = DateTime.Now },
                new AuditLog { UserId = user2.UserId, Action = "Insert", TableName = "Visits", RecordId = "2", Timestamp = DateTime.Now.AddMinutes(-1) });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetRecentActivitiesAsync(user1.UserId, 100);

            // Assert
            rows.Should().ContainSingle();
            rows[0].Username.Should().Be("u1");
            rows[0].TableName.Should().Be("Patients");
        }

        [Fact]
        public async Task GetRecentActivitiesAsync_Should_Respect_Count_Limit_Edge()
        {
            // Arrange
            var user = new User { Username = "u-limit" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.AuditLogs.AddRange(
                new AuditLog { UserId = user.UserId, Action = "Insert", TableName = "Patients", RecordId = "1", Timestamp = DateTime.Now },
                new AuditLog { UserId = user.UserId, Action = "Update", TableName = "Patients", RecordId = "2", Timestamp = DateTime.Now.AddSeconds(-1) });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetRecentActivitiesAsync(user.UserId, 1);

            // Assert
            rows.Should().HaveCount(1);
        }
    }
}
