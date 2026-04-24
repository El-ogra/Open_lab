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
            var now = DateTime.Now;
            var log = new AttendanceLog
            {
                UserId = userId,
                LoginAt = now,
                LastActivityAt = now,
                Note = note
            };

            _db.AttendanceLogs.Add(log);
            await _db.SaveChangesAsync();
            return log;
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
                .Where(l => l.LoginAt >= from && l.LoginAt <= to)
                .OrderByDescending(l => l.LoginAt)
                .ToListAsync();
        }
    }
}
