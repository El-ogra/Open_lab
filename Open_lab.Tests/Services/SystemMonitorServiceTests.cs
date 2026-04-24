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

        [Fact]
        public async Task MarkActivityAsync_Should_Update_Most_Recent_Open_Session()
        {
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

            await _service.MarkActivityAsync(user.UserId);

            var refreshedOld = await _db.AttendanceLogs.SingleAsync(x => x.AttendanceLogId == oldLog.AttendanceLogId);
            var refreshedLatest = await _db.AttendanceLogs.SingleAsync(x => x.AttendanceLogId == latestLog.AttendanceLogId);

            refreshedLatest.LastActivityAt.Should().BeAfter(refreshedOld.LastActivityAt);
            refreshedOld.LastActivityAt.Should().Be(oldLog.LastActivityAt);
        }
    }
}
