using System.Collections.Generic;
using System.Linq;

namespace Open_lab.Services
{
    public sealed class SessionContext : IMutableSessionContext
    {
        private readonly HashSet<string> _grantedPermissions = new(StringComparer.OrdinalIgnoreCase);

        public static SessionContext Current { get; } = new();

        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsAdmin => HasPermission(PermissionCodes.FullAccess);
        public int AttendanceLogId { get; set; }

        public void BeginSession(int userId, string username, IReadOnlyCollection<string> permissionCodes, int attendanceLogId = 0)
        {
            UserId = userId;
            Username = username;
            AttendanceLogId = attendanceLogId;
            _grantedPermissions.Clear();

            foreach (var permissionCode in permissionCodes.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                _grantedPermissions.Add(permissionCode);
            }
        }

        public void UpdateAttendanceLog(int attendanceLogId)
        {
            AttendanceLogId = attendanceLogId;
        }

        public bool HasPermission(string permissionCode)
        {
            return _grantedPermissions.Contains(PermissionCodes.FullAccess) ||
                   _grantedPermissions.Contains(permissionCode);
        }

        public void EndSession()
        {
            UserId = 0;
            Username = string.Empty;
            AttendanceLogId = 0;
            _grantedPermissions.Clear();
        }
    }
}
