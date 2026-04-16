using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class AttendanceLogViewModel : BaseViewModel
    {
        private readonly IAttendanceService _attendanceService;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private string _statusMessage = string.Empty;

        public AttendanceLogViewModel(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
            Logs = new ObservableCollection<AttendanceLogRow>();
            LoadLogsCommand = new RelayCommand(async _ => await LoadLogsAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersView));
        }

        public DateTime DateFrom
        {
            get => _dateFrom;
            set => SetProperty(ref _dateFrom, value);
        }

        public DateTime DateTo
        {
            get => _dateTo;
            set => SetProperty(ref _dateTo, value);
        }

        public ObservableCollection<AttendanceLogRow> Logs { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadLogsCommand { get; }

        private async Task LoadLogsAsync()
        {
            try
            {
                var from = DateFrom.Date;
                var to = DateTo.Date.AddDays(1).AddSeconds(-1);
                var logs = await _attendanceService.GetLogsAsync(from, to);

                Logs.Clear();
                foreach (var log in logs)
                {
                    var duration = log.LogoutAt.HasValue
                        ? (log.LogoutAt.Value - log.LoginAt)
                        : (TimeSpan?)null;

                    Logs.Add(new AttendanceLogRow
                    {
                        AttendanceLogId = log.AttendanceLogId,
                        Username = log.User.Username,
                        FullName = log.User.FullName,
                        LoginAt = log.LoginAt,
                        LogoutAt = log.LogoutAt,
                        Duration = duration.HasValue ? duration.Value.ToString(@"hh\:mm") : "-"
                    });
                }

                StatusMessage = "تم تحميل " + Logs.Count + " سجل.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }
    }
}
