using System.Collections.Generic;

namespace Open_lab.Services
{
    public static class PermissionCodes
    {
        public const string FullAccess = "ALL";
        public const string PatientsView = "Patients.View";
        public const string PatientsEdit = "Patients.Edit";
        public const string VisitsView = "Visits.View";
        public const string VisitsEdit = "Visits.Edit";
        public const string TestsView = "Tests.View";
        public const string TestsEdit = "Tests.Edit";
        public const string ResultsView = "Results.View";
        public const string ResultsEdit = "Results.Edit";
        public const string ReportsView = "Reports.View";
        public const string AccountsView = "Accounts.View";
        public const string AccountsEdit = "Accounts.Edit";
        public const string SettingsView = "Settings.View";
        public const string SettingsEdit = "Settings.Edit";
        public const string UsersView = "Users.View";
        public const string UsersEdit = "Users.Edit";
        public const string StatisticsView = "Statistics.View";
        public const string BackupRestore = "Backup.Restore";
        public const string DeliveryView = "Delivery.View";
        public const string DeliveryEdit = "Delivery.Edit";
        public const string ConstantsView = "Constants.View";
        public const string ConstantsEdit = "Constants.Edit";

        public static readonly IReadOnlyList<string> All = new List<string>
        {
            FullAccess,
            PatientsView,
            PatientsEdit,
            VisitsView,
            VisitsEdit,
            TestsView,
            TestsEdit,
            ResultsView,
            ResultsEdit,
            ReportsView,
            AccountsView,
            AccountsEdit,
            SettingsView,
            SettingsEdit,
            UsersView,
            UsersEdit,
            StatisticsView,
            BackupRestore,
            DeliveryView,
            DeliveryEdit,
            ConstantsView,
            ConstantsEdit
        };
    }
}
