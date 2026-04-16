using System;
using System.Collections.Generic;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public static class AppSession
    {
        private static readonly HashSet<string> GrantedPermissions = new(StringComparer.OrdinalIgnoreCase);

        public static int UserId { get; set; }
        public static string Username { get; set; } = string.Empty;
        public static bool IsAdmin { get; set; }
        public static int AttendanceLogId { get; set; }

        public static void SetPermissions(IEnumerable<string> permissionCodes)
        {
            GrantedPermissions.Clear();
            foreach (var code in permissionCodes)
            {
                GrantedPermissions.Add(code);
            }
        }

        public static bool HasPermission(string permissionCode)
        {
            if (IsAdmin)
            {
                return true;
            }

            return GrantedPermissions.Contains(PermissionCodes.FullAccess) || GrantedPermissions.Contains(permissionCode);
        }

        public static void Clear()
        {
            UserId = 0;
            Username = string.Empty;
            IsAdmin = false;
            AttendanceLogId = 0;
            GrantedPermissions.Clear();
        }
    }
}
