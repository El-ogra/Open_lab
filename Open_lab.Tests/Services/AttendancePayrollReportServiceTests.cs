using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class AttendancePayrollReportServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly AttendancePayrollReportService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public AttendancePayrollReportServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new AttendancePayrollReportService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GeneratePayrollSummaryAsync_Should_Calculate_Time_And_Day_Statuses_For_Module11_5()
        {
            // Function: 11.5 — `Generate Attendance Report`
            var user = new User { Username = "u-payroll", FullName = "Payroll User" };
            _db.Users.Add(user);

            var shift = new ShiftSchedule
            {
                Name = "Morning",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                GracePeriodMinutes = 5
            };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();

            var log = new AttendanceLog
            {
                UserId = user.UserId,
                ShiftId = shift.ShiftId,
                LoginAt = DateTime.Today.AddHours(8).AddMinutes(20),
                LogoutAt = DateTime.Today.AddHours(17),
                LastActivityAt = DateTime.Today.AddHours(17)
            };
            _db.AttendanceLogs.Add(log);
            await _db.SaveChangesAsync();

            _db.AttendanceBreaks.Add(new AttendanceBreak
            {
                AttendanceLogId = log.AttendanceLogId,
                StartAt = DateTime.Today.AddHours(12),
                EndAt = DateTime.Today.AddHours(12).AddMinutes(30),
                Type = "Rest"
            });

            _db.AttendanceDayStatuses.Add(new AttendanceDayStatus
            {
                UserId = user.UserId,
                Date = DateTime.Today.AddDays(1),
                Status = "Vacation"
            });
            await _db.SaveChangesAsync();

            var rows = await _service.GeneratePayrollSummaryAsync(DateTime.Today, DateTime.Today.AddDays(1), user.UserId);

            rows.Should().ContainSingle();
            var row = rows[0];
            row.UserId.Should().Be(user.UserId);
            row.GrossMinutes.Should().Be(520);
            row.BreakMinutes.Should().Be(30);
            row.NetMinutes.Should().Be(490);
            row.DelayMinutes.Should().Be(20);
            row.OvertimeMinutes.Should().Be(60);
            row.PresentDays.Should().Be(1);
            row.VacationDays.Should().Be(1);
            row.AbsentDays.Should().Be(0);
        }

        [Fact]
        public async Task GeneratePayrollSummaryAsync_When_UserId_NotFound_Should_Return_EmptyList_FailureGuard()
        {
            // Arrange
            var user = new User { Username = "u-any", FullName = "Any User" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GeneratePayrollSummaryAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), userId: 99999);

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GeneratePayrollSummaryAsync_When_ToBeforeFrom_Should_SwapRange_And_Count_UnknownStatus_AsOther_EdgeGuard()
        {
            // Arrange
            var user = new User { Username = "u-edge", FullName = "Edge User" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var targetDay = DateTime.Today;
            _db.AttendanceDayStatuses.Add(new AttendanceDayStatus
            {
                UserId = user.UserId,
                Date = targetDay,
                Status = "Training"
            });
            await _db.SaveChangesAsync();

            // Act (reversed range on purpose)
            var rows = await _service.GeneratePayrollSummaryAsync(targetDay.AddDays(1), targetDay.AddDays(-1), user.UserId);

            // Assert
            rows.Should().ContainSingle();
            var row = rows[0];
            row.OtherStatusDays.Should().BeGreaterThan(0);
            row.From.Date.Should().Be(targetDay.AddDays(-1).Date);
            row.To.Date.Should().Be(targetDay.AddDays(1).Date);
        }
    }
}
