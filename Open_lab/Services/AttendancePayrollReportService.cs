using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class AttendancePayrollReportService : IAttendancePayrollReportService
    {
        private readonly OpenLabDbContext _db;

        public AttendancePayrollReportService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<AttendancePayrollSummaryRow>> GeneratePayrollSummaryAsync(DateTime from, DateTime to, int? userId = null)
        {
            var rangeFrom = from;
            var rangeTo = to;
            if (rangeTo < rangeFrom)
            {
                (rangeFrom, rangeTo) = (rangeTo, rangeFrom);
            }

            var dayFrom = rangeFrom.Date;
            var dayTo = rangeTo.Date;

            var usersQuery = _db.Users.AsNoTracking();
            if (userId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.UserId == userId.Value);
            }

            var users = await usersQuery
                .OrderBy(u => u.Username)
                .ToListAsync();

            var logsQuery = _db.AttendanceLogs
                .AsNoTracking()
                .Include(l => l.User)
                .Include(l => l.Shift)
                .Include(l => l.Breaks)
                .Where(l => l.LoginAt >= rangeFrom && l.LoginAt <= rangeTo);

            if (userId.HasValue)
            {
                logsQuery = logsQuery.Where(l => l.UserId == userId.Value);
            }

            var logs = await logsQuery
                .OrderBy(l => l.LoginAt)
                .ToListAsync();

            var statusesQuery = _db.AttendanceDayStatuses
                .AsNoTracking()
                .Where(s => s.Date >= dayFrom && s.Date <= dayTo);

            if (userId.HasValue)
            {
                statusesQuery = statusesQuery.Where(s => s.UserId == userId.Value);
            }

            var statuses = await statusesQuery.ToListAsync();

            var logsByUser = logs.GroupBy(l => l.UserId).ToDictionary(g => g.Key, g => g.ToList());
            var statusByUser = statuses
                .GroupBy(s => s.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(x => x.Date.Date, x => x.Status));

            var rows = new List<AttendancePayrollSummaryRow>();

            foreach (var user in users)
            {
                logsByUser.TryGetValue(user.UserId, out var userLogs);
                userLogs ??= new List<AttendanceLog>();

                var (grossMinutes, breakMinutes, netMinutes, delayMinutes, overtimeMinutes) = SumLogMinutes(userLogs);

                var dict = statusByUser.TryGetValue(user.UserId, out var map)
                    ? map
                    : new Dictionary<DateTime, string>();

                var (presentDays, absentDays, vacationDays, holidayDays, sickLeaveDays, otherStatusDays) =
                    CountDays(dayFrom, dayTo, dict, userLogs);

                rows.Add(new AttendancePayrollSummaryRow
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    From = rangeFrom,
                    To = rangeTo,
                    GrossMinutes = grossMinutes,
                    BreakMinutes = breakMinutes,
                    NetMinutes = netMinutes,
                    DelayMinutes = delayMinutes,
                    OvertimeMinutes = overtimeMinutes,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    VacationDays = vacationDays,
                    HolidayDays = holidayDays,
                    SickLeaveDays = sickLeaveDays,
                    OtherStatusDays = otherStatusDays
                });
            }

            return rows;
        }

        private static (int grossMinutes, int breakMinutes, int netMinutes, int delayMinutes, int overtimeMinutes) SumLogMinutes(List<AttendanceLog> logs)
        {
            var grossMinutes = 0;
            var breakMinutes = 0;
            var delayMinutes = 0;
            var overtimeMinutes = 0;

            foreach (var log in logs)
            {
                if (!log.LogoutAt.HasValue || log.LogoutAt.Value <= log.LoginAt)
                {
                    continue;
                }

                var loginAt = log.LoginAt;
                var logoutAt = log.LogoutAt.Value;

                grossMinutes += (int)Math.Round((logoutAt - loginAt).TotalMinutes, 0, MidpointRounding.AwayFromZero);
                breakMinutes += CalculateBreakMinutes(log, loginAt, logoutAt);

                delayMinutes += CalculateDelayMinutes(log);
                overtimeMinutes += CalculateOvertimeMinutes(log);
            }

            grossMinutes = Math.Max(0, grossMinutes);
            breakMinutes = Math.Max(0, breakMinutes);
            var netMinutes = Math.Max(0, grossMinutes - breakMinutes);

            delayMinutes = Math.Max(0, delayMinutes);
            overtimeMinutes = Math.Max(0, overtimeMinutes);

            return (grossMinutes, breakMinutes, netMinutes, delayMinutes, overtimeMinutes);
        }

        private static int CalculateBreakMinutes(AttendanceLog log, DateTime loginAt, DateTime logoutAt)
        {
            var total = 0;
            foreach (var br in log.Breaks)
            {
                var start = br.StartAt;
                var end = br.EndAt ?? logoutAt;
                if (end <= start)
                {
                    continue;
                }

                if (start < loginAt) start = loginAt;
                if (end > logoutAt) end = logoutAt;

                if (end > start)
                {
                    total += (int)Math.Round((end - start).TotalMinutes, 0, MidpointRounding.AwayFromZero);
                }
            }
            return Math.Max(0, total);
        }

        private static int CalculateDelayMinutes(AttendanceLog log)
        {
            if (log.Shift == null)
            {
                return 0;
            }

            var loginTime = log.LoginAt.TimeOfDay;
            if (loginTime <= log.Shift.StartTime)
            {
                return 0;
            }

            var diff = (loginTime - log.Shift.StartTime).TotalMinutes;
            return diff > log.Shift.GracePeriodMinutes
                ? (int)Math.Round(diff, 0, MidpointRounding.AwayFromZero)
                : 0;
        }

        private static int CalculateOvertimeMinutes(AttendanceLog log)
        {
            if (log.Shift == null || !log.LogoutAt.HasValue)
            {
                return 0;
            }

            var logoutTime = log.LogoutAt.Value.TimeOfDay;
            if (logoutTime <= log.Shift.EndTime)
            {
                return 0;
            }

            var mins = (logoutTime - log.Shift.EndTime).TotalMinutes;
            return (int)Math.Round(mins, 0, MidpointRounding.AwayFromZero);
        }

        private static (int present, int absent, int vacation, int holiday, int sick, int other) CountDays(
            DateTime dayFrom,
            DateTime dayTo,
            Dictionary<DateTime, string> statusesByDay,
            List<AttendanceLog> logs)
        {
            var present = 0;
            var absent = 0;
            var vacation = 0;
            var holiday = 0;
            var sick = 0;
            var other = 0;

            var logsByDay = logs
                .GroupBy(l => l.LoginAt.Date)
                .ToDictionary(g => g.Key, g => g.Any());

            for (var d = dayFrom.Date; d <= dayTo.Date; d = d.AddDays(1))
            {
                if (statusesByDay.TryGetValue(d, out var status))
                {
                    switch ((status ?? string.Empty).Trim().ToLowerInvariant())
                    {
                        case "present":
                        case "حضور":
                            present++;
                            break;
                        case "absent":
                        case "غياب":
                            absent++;
                            break;
                        case "vacation":
                        case "إجازة":
                            vacation++;
                            break;
                        case "holiday":
                        case "عطلة":
                            holiday++;
                            break;
                        case "sickleave":
                        case "sick":
                        case "مرضية":
                            sick++;
                            break;
                        default:
                            other++;
                            break;
                    }

                    continue;
                }

                if (logsByDay.TryGetValue(d, out var hasLog) && hasLog)
                {
                    present++;
                }
                else
                {
                    absent++;
                }
            }

            return (present, absent, vacation, holiday, sick, other);
        }
    }
}

