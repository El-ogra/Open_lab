using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.ViewModels
{
    public class AttendanceLogViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private string _statusMessage = string.Empty;

        public AttendanceLogViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            Logs = new ObservableCollection<AttendanceLogRow>();
            LoadLogsCommand = new RelayCommand(async _ => await LoadLogsAsync());
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
                using var db = _dbFactory();
                var from = DateFrom.Date;
                var to = DateTo.Date.AddDays(1).AddSeconds(-1);

                var logs = await db.AttendanceLogs
                    .Include(l => l.User)
                    .Where(l => l.LoginAt >= from && l.LoginAt <= to)
                    .OrderByDescending(l => l.LoginAt)
                    .ToListAsync();

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
