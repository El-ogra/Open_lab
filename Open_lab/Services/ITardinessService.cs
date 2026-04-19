using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public class AttendanceReportRow
    {
        public string Username { get; set; } = string.Empty;
        public DateTime LoginAt { get; set; }
        public DateTime? LogoutAt { get; set; }
        public string? ShiftName { get; set; }
        public double TotalHours { get; set; }
        public double DelayMinutes { get; set; }
        public double OvertimeMinutes { get; set; }
    }

    public interface ITardinessService
    {
        Task<List<AttendanceReportRow>> GetPunctualityReportAsync(DateTime from, DateTime to, int? userId = null);
    }
}
