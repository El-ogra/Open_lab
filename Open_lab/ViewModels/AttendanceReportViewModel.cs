using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class AttendanceReportViewModel : BaseViewModel
    {
        private readonly ITardinessService _tardinessService;
        private readonly IUserAdminService _userAdminService;
        private DateTime _from = DateTime.Today.AddDays(-7);
        private DateTime _to = DateTime.Today;
        private int? _selectedUserId;
        private string _statusMessage = string.Empty;

        public AttendanceReportViewModel(ITardinessService tardinessService, IUserAdminService userAdminService)
        {
            _tardinessService = tardinessService;
            _userAdminService = userAdminService;
            Users = new ObservableCollection<UserItem>();
            ReportRows = new ObservableCollection<AttendanceReportRow>();

            LoadUsersCommand = new RelayCommand(async _ => await LoadUsersAsync());
            GenerateReportCommand = new RelayCommand(async _ => await GenerateReportAsync());
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());

            _ = LoadUsersAsync();
        }

        public DateTime From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public DateTime To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public int? SelectedUserId
        {
            get => _selectedUserId;
            set => SetProperty(ref _selectedUserId, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<UserItem> Users { get; }
        public ObservableCollection<AttendanceReportRow> ReportRows { get; }

        public ICommand LoadUsersCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand ClearFilterCommand { get; }

        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _userAdminService.GetUsersAsync();
                Users.Clear();
                Users.Add(new UserItem { UserId = null, Username = "(جميع المستخدمين)" });
                foreach (var user in users)
                {
                    Users.Add(new UserItem { UserId = user.UserId, Username = user.Username });
                }
                StatusMessage = $"تم تحميل {users.Count} مستخدم.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task GenerateReportAsync()
        {
            try
            {
                var rows = await _tardinessService.GetPunctualityReportAsync(From.Date, To.Date.AddDays(1).AddSeconds(-1), SelectedUserId);
                ReportRows.Clear();
                foreach (var row in rows)
                {
                    ReportRows.Add(row);
                }

                var totalHours = rows.Sum(r => r.TotalHours);
                var totalDelay = rows.Sum(r => r.DelayMinutes);
                var totalOvertime = rows.Sum(r => r.OvertimeMinutes);

                StatusMessage = $"تم إنشاء التقرير: {rows.Count} سجل | إجمالي الساعات: {totalHours:F2} | التأخير: {totalDelay:F0} دقيقة | الovertime: {totalOvertime:F0} دقيقة";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private void ClearFilter()
        {
            SelectedUserId = null;
            From = DateTime.Today.AddDays(-7);
            To = DateTime.Today;
        }
    }

    public class UserItem
    {
        public int? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}
