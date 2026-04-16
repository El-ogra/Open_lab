using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.ViewModels
{
    public class AccountsTreasuryViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private decimal _totalInvoiced;
        private decimal _totalPaid;
        private decimal _totalBalance;
        private string _statusMessage = string.Empty;

        public AccountsTreasuryViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
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
                using var db = _dbFactory();
                var from = DateFrom.Date;
                var to = DateTo.Date.AddDays(1).AddSeconds(-1);

                var invoices = await db.Invoices
                    .Include(i => i.Visit)
                    .ThenInclude(v => v.Patient)
                    .Where(i => i.Visit.VisitDate >= from && i.Visit.VisitDate <= to)
                    .ToListAsync();

                TotalInvoiced = invoices.Sum(i => i.NetTotal);
                TotalPaid = invoices.Sum(i => i.Paid);
                TotalBalance = invoices.Sum(i => i.Balance);

                var payments = await db.Payments
                    .Include(p => p.Invoice)
                    .ThenInclude(i => i.Visit)
                    .ThenInclude(v => v.Patient)
                    .Include(p => p.User)
                    .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                Payments.Clear();
                foreach (var p in payments)
                {
                    Payments.Add(new AccountsPaymentRow
                    {
                        PaymentId = p.PaymentId,
                        PatientName = p.Invoice.Visit.Patient.FullName,
                        ReferralName = p.Invoice.Visit.Referral?.Name,
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        Username = p.User?.Username
                    });
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
