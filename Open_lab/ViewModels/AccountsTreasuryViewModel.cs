using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class AccountsTreasuryViewModel : BaseViewModel
    {
        private readonly IAccountsTreasuryService _accountsTreasuryService;
        private readonly IPrintService _printService;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private decimal _totalInvoiced;
        private decimal _totalPaid;
        private decimal _totalBalance;
        private string _statusMessage = string.Empty;

        public AccountsTreasuryViewModel(IAccountsTreasuryService accountsTreasuryService, IPrintService printService)
        {
            _accountsTreasuryService = accountsTreasuryService;
            _printService = printService;
            Payments = new ObservableCollection<AccountsPaymentRow>();
            ByUser = new ObservableCollection<TreasuryByUserRow>();
            ByReferral = new ObservableCollection<TreasuryByReferralRow>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsView));
            DailyCommand = new RelayCommand(async _ => await LoadDailyAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsView));
            WeeklyCommand = new RelayCommand(async _ => await LoadWeeklyAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsView));
            MonthlyCommand = new RelayCommand(async _ => await LoadMonthlyAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsView));
            PrintCommand = new RelayCommand(async _ => await PrintAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsView));
            _ = LoadAsync();
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

        public decimal TotalInvoiced
        {
            get => _totalInvoiced;
            private set => SetProperty(ref _totalInvoiced, value);
        }

        public decimal TotalPaid
        {
            get => _totalPaid;
            private set => SetProperty(ref _totalPaid, value);
        }

        public decimal TotalBalance
        {
            get => _totalBalance;
            private set => SetProperty(ref _totalBalance, value);
        }

        public ObservableCollection<AccountsPaymentRow> Payments { get; }
        public ObservableCollection<TreasuryByUserRow> ByUser { get; }
        public ObservableCollection<TreasuryByReferralRow> ByReferral { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand DailyCommand { get; }
        public ICommand WeeklyCommand { get; }
        public ICommand MonthlyCommand { get; }
        public ICommand PrintCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var from = DateFrom.Date;
                var to = DateTo.Date.AddDays(1).AddSeconds(-1);
                var snapshot = await _accountsTreasuryService.GetSnapshotAsync(from, to);

                TotalInvoiced = snapshot.TotalInvoiced;
                TotalPaid = snapshot.TotalPaid;
                TotalBalance = snapshot.TotalBalance;

                Payments.Clear();
                foreach (var payment in snapshot.Payments)
                {
                    Payments.Add(payment);
                }

                ByUser.Clear();
                foreach (var row in snapshot.ByUser)
                {
                    ByUser.Add(row);
                }

                ByReferral.Clear();
                foreach (var row in snapshot.ByReferral)
                {
                    ByReferral.Add(row);
                }

                StatusMessage = "تم تحميل بيانات الخزينة.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task LoadDailyAsync()
        {
            DateFrom = DateTime.Today;
            DateTo = DateTime.Today;
            await LoadAsync();
        }

        private async Task LoadWeeklyAsync()
        {
            var today = DateTime.Today;
            var delta = ((int)today.DayOfWeek + 6) % 7;
            DateFrom = today.AddDays(-delta);
            DateTo = DateFrom.AddDays(6);
            await LoadAsync();
        }

        private async Task LoadMonthlyAsync()
        {
            var today = DateTime.Today;
            DateFrom = new DateTime(today.Year, today.Month, 1);
            DateTo = DateFrom.AddMonths(1).AddDays(-1);
            await LoadAsync();
        }

        private async Task PrintAsync()
        {
            try
            {
                var lines = new ObservableCollection<string>
                {
                    $"الفترة: {DateFrom:yyyy-MM-dd} إلى {DateTo:yyyy-MM-dd}",
                    $"إجمالي الفواتير: {TotalInvoiced.ToString("N2", CultureInfo.CurrentCulture)}",
                    $"إجمالي المدفوع: {TotalPaid.ToString("N2", CultureInfo.CurrentCulture)}",
                    $"إجمالي المتبقي: {TotalBalance.ToString("N2", CultureInfo.CurrentCulture)}",
                    "",
                    "تفصيل حسب المستخدم:"
                };

                foreach (var row in ByUser)
                {
                    lines.Add($"- {row.Username}: عمليات {row.PaymentsCount}, إجمالي {row.TotalAmount:N2}");
                }

                lines.Add("");
                lines.Add("تفصيل حسب الجهة:");
                foreach (var row in ByReferral)
                {
                    lines.Add($"- {row.ReferralName}: زيارات {row.VisitsCount}, فواتير {row.TotalInvoiced:N2}, مدفوع {row.TotalPaid:N2}, متبقي {row.TotalBalance:N2}");
                }

                await _printService.PrintTextReportAsync("تقرير الخزينة", lines, "TreasuryReport");
                StatusMessage = "تم إرسال تقرير الخزينة للطباعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }
    }
}
