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
        private readonly IAttendancePayrollReportService _payrollReportService;
        private DateTime _from = DateTime.Today.AddDays(-7);
        private DateTime _to = DateTime.Today;
        private int? _selectedUserId;
        private string _statusMessage = string.Empty;
        private bool _isBusy;

        public AttendanceReportViewModel(
            ITardinessService tardinessService,
            IUserAdminService userAdminService,
            IAttendancePayrollReportService payrollReportService)
        {
            _tardinessService = tardinessService;
            _userAdminService = userAdminService;
            _payrollReportService = payrollReportService;
            Users = new ObservableCollection<UserItem>();
            ReportRows = new ObservableCollection<AttendanceReportRow>();
            PayrollRows = new ObservableCollection<AttendancePayrollSummaryRow>();

            LoadUsersCommand = new RelayCommand(async _ => await LoadUsersAsync());
            GenerateReportCommand = new RelayCommand(async _ => await GenerateReportAsync());
            GeneratePayrollSummaryCommand = new RelayCommand(async _ => await GeneratePayrollSummaryAsync());
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

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    (LoadUsersCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (GeneratePayrollSummaryCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (ClearFilterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<UserItem> Users { get; }
        public ObservableCollection<AttendanceReportRow> ReportRows { get; }
        public ObservableCollection<AttendancePayrollSummaryRow> PayrollRows { get; }

        public ICommand LoadUsersCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand GeneratePayrollSummaryCommand { get; }
        public ICommand ClearFilterCommand { get; }

        private async Task LoadUsersAsync()
        {
            try
            {
                IsBusy = true;
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
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GenerateReportAsync()
        {
            try
            {
                IsBusy = true;
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
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GeneratePayrollSummaryAsync()
        {
            try
            {
                IsBusy = true;
                var rows = await _payrollReportService.GeneratePayrollSummaryAsync(From.Date, To.Date.AddDays(1).AddSeconds(-1), SelectedUserId);
                PayrollRows.Clear();
                foreach (var row in rows)
                {
                    PayrollRows.Add(row);
                }

                var netMinutes = rows.Sum(r => r.NetMinutes);
                var delayMinutes = rows.Sum(r => r.DelayMinutes);
                var overtimeMinutes = rows.Sum(r => r.OvertimeMinutes);
                var absentDays = rows.Sum(r => r.AbsentDays);

                StatusMessage =
                    $"ملخص الرواتب: {rows.Count} موظف | صافي {netMinutes} دقيقة | تأخير {delayMinutes} دقيقة | إضافي {overtimeMinutes} دقيقة | غياب {absentDays} يوم";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
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
