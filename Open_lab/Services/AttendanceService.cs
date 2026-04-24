using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly OpenLabDbContext _db;

        public AttendanceService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<AttendanceLog> CreateLoginAsync(int userId, string? note)
        {
            return await ClockInAsync(userId, at: DateTime.Now, shiftId: null, note: note);
        }

        public async Task<AttendanceLog> ClockInAsync(int userId, DateTime? at = null, int? shiftId = null, string? note = null)
        {
            var existingOpen = await GetOpenLogAsync(userId);
            if (existingOpen != null)
            {
                return existingOpen;
            }

            var now = at ?? DateTime.Now;
            var log = new AttendanceLog
            {
                UserId = userId,
                ShiftId = shiftId,
                LoginAt = now,
                LastActivityAt = now,
                Note = note
            };

            _db.AttendanceLogs.Add(log);
            await _db.SaveChangesAsync();
            return log;
        }

        public async Task<AttendanceLog?> ClockOutAsync(int userId, DateTime? at = null)
        {
            var log = await GetOpenLogAsync(userId);
            if (log == null)
            {
                return null;
            }

            if (log.LogoutAt != null)
            {
                return log;
            }

            var now = at ?? DateTime.Now;
            log.LogoutAt = now;
            log.LastActivityAt = now;
            await _db.SaveChangesAsync();
            return log;
        }

        public Task<AttendanceLog?> GetOpenLogAsync(int userId)
        {
            return _db.AttendanceLogs
                .Include(l => l.Breaks)
                .FirstOrDefaultAsync(l => l.UserId == userId && l.LogoutAt == null);
        }

        public async Task<AttendanceBreak?> StartBreakAsync(int userId, string type = "Rest", DateTime? at = null, string? note = null)
        {
            var log = await GetOpenLogAsync(userId);
            if (log == null)
            {
                return null;
            }

            var now = at ?? DateTime.Now;
            if (now < log.LoginAt)
            {
                now = log.LoginAt;
            }

            var openBreak = log.Breaks
                .OrderByDescending(b => b.StartAt)
                .FirstOrDefault(b => b.EndAt == null);

            if (openBreak != null)
            {
                // لا نسمح بأكثر من راحة مفتوحة لنفس سجل الحضور
                return openBreak;
            }

            var br = new AttendanceBreak
            {
                AttendanceLogId = log.AttendanceLogId,
                StartAt = now,
                EndAt = null,
                Type = string.IsNullOrWhiteSpace(type) ? "Rest" : type.Trim(),
                Note = note
            };

            _db.AttendanceBreaks.Add(br);
            log.LastActivityAt = now;
            await _db.SaveChangesAsync();
            return br;
        }

        public async Task<AttendanceBreak?> EndBreakAsync(int userId, DateTime? at = null)
        {
            var log = await GetOpenLogAsync(userId);
            if (log == null)
            {
                return null;
            }

            var openBreak = log.Breaks
                .OrderByDescending(b => b.StartAt)
                .FirstOrDefault(b => b.EndAt == null);

            if (openBreak == null)
            {
                // لا يمكن إنهاء راحة بدون بدء راحة
                return null;
            }

            var now = at ?? DateTime.Now;
            if (now < openBreak.StartAt)
            {
                now = openBreak.StartAt;
            }

            openBreak.EndAt = now;
            log.LastActivityAt = now;
            await _db.SaveChangesAsync();
            return openBreak;
        }

        public async Task CloseAsync(int attendanceLogId)
        {
            var log = await _db.AttendanceLogs.FirstOrDefaultAsync(l => l.AttendanceLogId == attendanceLogId);
            if (log == null || log.LogoutAt != null)
            {
                return;
            }

            var now = DateTime.Now;
            log.LogoutAt = now;
            log.LastActivityAt = now;
            await _db.SaveChangesAsync();
        }

        public Task<List<AttendanceLog>> GetLogsAsync(DateTime from, DateTime to)
        {
            return _db.AttendanceLogs
                .AsNoTracking()
                .Include(l => l.User)
                .Include(l => l.Breaks)
                .Where(l => l.LoginAt >= from && l.LoginAt <= to)
                .OrderByDescending(l => l.LoginAt)
                .ToListAsync();
        }

        public async Task<DailyWorkingSummary> GetDailyWorkingSummaryAsync(int userId, DateTime date, DateTime? now = null)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);
            var effectiveNow = now ?? DateTime.Now;

            var logs = await _db.AttendanceLogs
                .AsNoTracking()
                .Include(l => l.Breaks)
                .Where(l => l.UserId == userId && l.LoginAt >= dayStart && l.LoginAt < dayEnd)
                .OrderBy(l => l.LoginAt)
                .ToListAsync();

            var summary = new DailyWorkingSummary
            {
                UserId = userId,
                Date = dayStart,
                HasOpenLog = logs.Any(l => l.LogoutAt == null),
                FirstLoginAt = logs.FirstOrDefault()?.LoginAt,
                LastLogoutAt = logs.Where(l => l.LogoutAt.HasValue).OrderByDescending(l => l.LogoutAt).FirstOrDefault()?.LogoutAt
            };

            var totalGrossMinutes = 0;
            var totalBreakMinutes = 0;

            foreach (var log in logs)
            {
                var loginAt = log.LoginAt;
                var logoutAt = log.LogoutAt ?? effectiveNow;
                if (logoutAt <= loginAt)
                {
                    continue;
                }

                totalGrossMinutes += (int)Math.Round((logoutAt - loginAt).TotalMinutes, 0, MidpointRounding.AwayFromZero);

                foreach (var br in log.Breaks)
                {
                    var bStart = br.StartAt;
                    var bEnd = br.EndAt ?? logoutAt;
                    if (bEnd <= bStart)
                    {
                        continue;
                    }

                    // Clip break to the working interval [loginAt, logoutAt]
                    if (bStart < loginAt) bStart = loginAt;
                    if (bEnd > logoutAt) bEnd = logoutAt;

                    if (bEnd > bStart)
                    {
                        totalBreakMinutes += (int)Math.Round((bEnd - bStart).TotalMinutes, 0, MidpointRounding.AwayFromZero);
                    }
                }
            }

            summary.GrossMinutes = Math.Max(0, totalGrossMinutes);
            summary.BreakMinutes = Math.Max(0, totalBreakMinutes);
            summary.NetMinutes = Math.Max(0, summary.GrossMinutes - summary.BreakMinutes);
            return summary;
        }
    }
}
