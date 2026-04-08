using System.Collections.Generic;

namespace Open_lab.Services
{
    public static class PermissionCodes
    {
        public const string FullAccess = "ALL";

        public static readonly IReadOnlyList<string> All = new List<string>
        {
            FullAccess,
            "Patients.View",
            "Patients.Edit",
            "Visits.View",
            "Visits.Edit",
            "Tests.View",
            "Tests.Edit",
            "Results.View",
            "Results.Edit",
            "Reports.View",
            "Accounts.View",
            "Accounts.Edit",
            "Settings.View",
            "Settings.Edit",
            "Users.View",
            "Users.Edit",
            "Statistics.View",
            "Backup.Restore"
        };
    }
}
