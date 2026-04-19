using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public class ActiveSessionRow
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public DateTime LoginAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public string Status { get; set; } = "Active";
    }

    public interface ISystemMonitorService
    {
        Task<List<ActiveSessionRow>> GetActiveSessionsAsync();
        Task MarkActivityAsync(int userId);
    }
}
