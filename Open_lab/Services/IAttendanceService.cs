using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceLog> CreateLoginAsync(int userId, string? note);
        Task CloseAsync(int attendanceLogId);
        Task<List<AttendanceLog>> GetLogsAsync(DateTime from, DateTime to);
    }
}
