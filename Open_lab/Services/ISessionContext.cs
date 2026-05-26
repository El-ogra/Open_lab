using System.Collections.Generic;

namespace Open_lab.Services
{
    public interface ISessionContext
    {
        int UserId { get; }
        string Username { get; }
        bool IsAdmin { get; }
        int AttendanceLogId { get; }
        bool HasPermission(string permissionCode);
    }

    public interface IMutableSessionContext : ISessionContext
    {
        void BeginSession(int userId, string username, IReadOnlyCollection<string> permissionCodes, int attendanceLogId = 0);
        void UpdateAttendanceLog(int attendanceLogId);
        void EndSession();
    }
}
