using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class SystemMonitorService : ISystemMonitorService
    {
        private readonly OpenLabDbContext _db;

        public SystemMonitorService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<ActiveSessionRow>> GetActiveSessionsAsync()
        {
            // Sessions with no LogoutAt are considered active (Open state)
            return await _db.AttendanceLogs
                .AsNoTracking()
                .Include(a => a.User)
                .Where(a => a.LogoutAt == null)
                .OrderByDescending(a => a.LastActivityAt)
                .Select(a => new ActiveSessionRow
                {
                    UserId = a.UserId,
                    Username = a.User.Username,
                    FullName = a.User.FullName,
                    LoginAt = a.LoginAt,
                    LastActivityAt = a.LastActivityAt,
                    Status = "متصل"
                })
                .ToListAsync();
        }

        public async Task MarkActivityAsync(int userId)
        {
            // Update the most recent active session's activity timestamp
            var session = await _db.AttendanceLogs
                .Where(a => a.UserId == userId && a.LogoutAt == null)
                .OrderByDescending(a => a.LoginAt)
                .FirstOrDefaultAsync();

            if (session != null)
            {
                session.LastActivityAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }
    }
}
