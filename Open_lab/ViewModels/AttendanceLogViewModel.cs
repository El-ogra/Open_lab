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
        private bool _isBusy;
        private DailyWorkingSummary? _dailySummary;
        private string _breakType = "Rest";
        private string _breakNote = string.Empty;

        public AttendanceLogViewModel(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
            Logs = new ObservableCollection<AttendanceLogRow>();
            Breaks = new ObservableCollection<BreakRow>();
            LoadLogsCommand = new RelayCommand(async _ => await LoadLogsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.UsersView));

            ClockInCommand = new RelayCommand(async _ => await ClockInAsync(), _ => CanExecuteAttendanceAction());
            ClockOutCommand = new RelayCommand(async _ => await ClockOutAsync(), _ => CanExecuteAttendanceAction());
            StartBreakCommand = new RelayCommand(async _ => await StartBreakAsync(), _ => CanExecuteAttendanceAction());
            EndBreakCommand = new RelayCommand(async _ => await EndBreakAsync(), _ => CanExecuteAttendanceAction());
            LoadDailySummaryCommand = new RelayCommand(async _ => await LoadDailySummaryAsync(), _ => CanExecuteAttendanceAction());
            RefreshOpenLogCommand = new RelayCommand(async _ => await RefreshOpenLogAsync(), _ => CanExecuteAttendanceAction());

            _ = RefreshOpenLogAsync();
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

        public ObservableCollection<BreakRow> Breaks { get; }

        public DailyWorkingSummary? DailySummary
        {
            get => _dailySummary;
            private set => SetProperty(ref _dailySummary, value);
        }

        public string BreakType
        {
            get => _breakType;
            set => SetProperty(ref _breakType, value);
        }

        public string BreakNote
        {
            get => _breakNote;
            set => SetProperty(ref _breakNote, value);
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
                    RaiseCommandsCanExecuteChanged();
                }
            }
        }

        public ICommand LoadLogsCommand { get; }
        public ICommand ClockInCommand { get; }
        public ICommand ClockOutCommand { get; }
        public ICommand StartBreakCommand { get; }
        public ICommand EndBreakCommand { get; }
        public ICommand LoadDailySummaryCommand { get; }
        public ICommand RefreshOpenLogCommand { get; }

        private bool CanExecuteAttendanceAction()
        {
            return !IsBusy && SessionContext.Current.UserId > 0;
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            (LoadLogsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ClockInCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ClockOutCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (StartBreakCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (EndBreakCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LoadDailySummaryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RefreshOpenLogCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task RefreshOpenLogAsync()
        {
            try
            {
                IsBusy = true;
                Breaks.Clear();

                var openLog = await _attendanceService.GetOpenLogAsync(SessionContext.Current.UserId);
                if (openLog == null)
                {
                    StatusMessage = "لا يوجد سجل حضور مفتوح.";
                    DailySummary = null;
                    return;
                }

                foreach (var br in openLog.Breaks.OrderByDescending(b => b.StartAt))
                {
                    Breaks.Add(new BreakRow
                    {
                        BreakId = br.BreakId,
                        StartAt = br.StartAt,
                        EndAt = br.EndAt,
                        Type = br.Type,
                        Note = br.Note,
                        Duration = br.EndAt.HasValue
                            ? (br.EndAt.Value - br.StartAt).ToString(@"hh\:mm")
                            : "مفتوحة"
                    });
                }

                StatusMessage = $"سجل حضور مفتوح رقم: {openLog.AttendanceLogId} | عدد فترات الراحة: {Breaks.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ClockInAsync()
        {
            try
            {
                IsBusy = true;
                var log = await _attendanceService.ClockInAsync(SessionContext.Current.UserId, note: "Clock In");
                SessionContext.Current.AttendanceLogId = log.AttendanceLogId;
                StatusMessage = $"تم تسجيل الحضور. رقم السجل: {log.AttendanceLogId}";
                await RefreshOpenLogAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ClockOutAsync()
        {
            try
            {
                IsBusy = true;
                var log = await _attendanceService.ClockOutAsync(SessionContext.Current.UserId);
                if (log == null)
                {
                    StatusMessage = "لا يوجد سجل حضور مفتوح لإغلاقه.";
                    return;
                }

                StatusMessage = $"تم تسجيل الانصراف. رقم السجل: {log.AttendanceLogId}";
                SessionContext.Current.AttendanceLogId = 0;
                await RefreshOpenLogAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task StartBreakAsync()
        {
            try
            {
                IsBusy = true;
                var br = await _attendanceService.StartBreakAsync(SessionContext.Current.UserId, BreakType, note: string.IsNullOrWhiteSpace(BreakNote) ? null : BreakNote);
                if (br == null)
                {
                    StatusMessage = "لا يمكن بدء راحة بدون سجل حضور مفتوح.";
                    return;
                }

                StatusMessage = $"تم بدء الراحة. رقم الراحة: {br.BreakId}";
                BreakNote = string.Empty;
                await RefreshOpenLogAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task EndBreakAsync()
        {
            try
            {
                IsBusy = true;
                var br = await _attendanceService.EndBreakAsync(SessionContext.Current.UserId);
                if (br == null)
                {
                    StatusMessage = "لا توجد راحة مفتوحة لإنهائها.";
                    return;
                }

                StatusMessage = $"تم إنهاء الراحة. رقم الراحة: {br.BreakId}";
                await RefreshOpenLogAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadDailySummaryAsync()
        {
            try
            {
                IsBusy = true;
                DailySummary = await _attendanceService.GetDailyWorkingSummaryAsync(SessionContext.Current.UserId, DateTime.Today);
                StatusMessage = $"ملخص اليوم: صافي {DailySummary.NetMinutes} دقيقة | راحة {DailySummary.BreakMinutes} دقيقة";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadLogsAsync()
        {
            try
            {
                IsBusy = true;
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
            finally
            {
                IsBusy = false;
            }
        }
    }

    public class BreakRow
    {
        public int BreakId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string Duration { get; set; } = "-";
    }
}
