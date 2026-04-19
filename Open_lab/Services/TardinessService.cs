using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class TardinessService : ITardinessService
    {
        private readonly OpenLabDbContext _db;

        public TardinessService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<AttendanceReportRow>> GetPunctualityReportAsync(DateTime from, DateTime to, int? userId = null)
        {
            var query = _db.AttendanceLogs
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Shift)
                .Where(a => a.LoginAt >= from && a.LoginAt <= to);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            var logs = await query.OrderBy(a => a.LoginAt).ToListAsync();
            var report = new List<AttendanceReportRow>();

            foreach (var log in logs)
            {
                double totalHours = 0;
                if (log.LogoutAt.HasValue)
                {
                    totalHours = (log.LogoutAt.Value - log.LoginAt).TotalHours;
                }

                double delayMins = 0;
                double overtimeMins = 0;

                // Compare with shift if manually selected
                if (log.Shift != null)
                {
                    // Delay calculation: Compare Login time with Shift StartTime
                    var loginTime = log.LoginAt.TimeOfDay;
                    if (loginTime > log.Shift.StartTime)
                    {
                        var diff = (loginTime - log.Shift.StartTime).TotalMinutes;
                        if (diff > log.Shift.GracePeriodMinutes)
                        {
                            delayMins = diff;
                        }
                    }

                    // Overtime calculation: Compare Logout time with Shift EndTime
                    if (log.LogoutAt.HasValue)
                    {
                        var logoutTime = log.LogoutAt.Value.TimeOfDay;
                        if (logoutTime > log.Shift.EndTime)
                        {
                            overtimeMins = (logoutTime - log.Shift.EndTime).TotalMinutes;
                        }
                    }
                }

                report.Add(new AttendanceReportRow
                {
                    Username = log.User.Username,
                    LoginAt = log.LoginAt,
                    LogoutAt = log.LogoutAt,
                    ShiftName = log.Shift?.Name ?? "مرنة",
                    TotalHours = Math.Round(totalHours, 2),
                    DelayMinutes = Math.Round(delayMins, 2),
                    OvertimeMinutes = Math.Round(overtimeMins, 2)
                });
            }

            return report;
        }
    }
}
