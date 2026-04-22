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
        public async Task CreateLoginAsync_Should_Create_Log()
        {
            var log = await _service.CreateLoginAsync(5, "note");
            log.AttendanceLogId.Should().BeGreaterThan(0);
            log.UserId.Should().Be(5);
        }

        [Fact]
        public async Task CloseAsync_Should_Set_LogoutAt()
        {
            var created = await _service.CreateLoginAsync(6, null);
            await _service.CloseAsync(created.AttendanceLogId);
            var updated = await _db.AttendanceLogs.FindAsync(created.AttendanceLogId);
            updated.Should().NotBeNull();
            updated!.LogoutAt.Should().NotBeNull();
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
    }
}
