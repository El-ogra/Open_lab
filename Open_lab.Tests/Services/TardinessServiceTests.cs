using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class TardinessServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly TardinessService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public TardinessServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new TardinessService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetPunctualityReportAsync_Should_Calculate_Delay_And_Overtime()
        {
            var shift = new ShiftSchedule { Name = "Day", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(16, 0, 0), GracePeriodMinutes = 5 };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();

            var user = new User { Username = "u1" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var login = DateTime.Today.AddHours(8).AddMinutes(10); // 10 minutes late
            var logout = DateTime.Today.AddHours(17); // 1 hour overtime

            _db.AttendanceLogs.Add(new AttendanceLog { UserId = user.UserId, LoginAt = login, LogoutAt = logout, ShiftId = shift.ShiftId });
            await _db.SaveChangesAsync();

            var report = await _service.GetPunctualityReportAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            report.Should().ContainSingle();
            var row = report[0];
            row.DelayMinutes.Should().BeGreaterThan(0);
            row.OvertimeMinutes.Should().BeGreaterThan(0);
        }
    }
}
