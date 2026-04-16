using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class AccountsTreasuryViewModel : BaseViewModel
    {
        private readonly IAccountsTreasuryService _accountsTreasuryService;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private decimal _totalInvoiced;
        private decimal _totalPaid;
        private decimal _totalBalance;
        private string _statusMessage = string.Empty;

        public AccountsTreasuryViewModel(IAccountsTreasuryService accountsTreasuryService)
        {
            _accountsTreasuryService = accountsTreasuryService;
            Payments = new ObservableCollection<AccountsPaymentRow>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
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

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }

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

                StatusMessage = "تم تحميل " + Payments.Count + " دفعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }
    }
}
