using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceLog> CreateLoginAsync(int userId, string? note);
        Task<AttendanceLog> ClockInAsync(int userId, DateTime? at = null, int? shiftId = null, string? note = null);
        Task<AttendanceLog?> ClockOutAsync(int userId, DateTime? at = null);
        Task<AttendanceLog?> GetOpenLogAsync(int userId);
        Task<AttendanceBreak?> StartBreakAsync(int userId, string type = "Rest", DateTime? at = null, string? note = null);
        Task<AttendanceBreak?> EndBreakAsync(int userId, DateTime? at = null);
        Task CloseAsync(int attendanceLogId);
        Task<List<AttendanceLog>> GetLogsAsync(DateTime from, DateTime to);
        Task<DailyWorkingSummary> GetDailyWorkingSummaryAsync(int userId, DateTime date, DateTime? now = null);
    }
}
