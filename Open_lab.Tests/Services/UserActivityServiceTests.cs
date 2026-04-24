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
    }
}
