using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public class UserActivityRow
    {
        public int AuditLogId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string ActivityDescription { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public interface IUserActivityService
    {
        Task<List<UserActivityRow>> GetRecentActivitiesAsync(int? userId = null, int count = 100);
        Task<string> SimplifyAuditLogAsync(int auditLogId);
    }
}
