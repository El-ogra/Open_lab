using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class PatientBillingByDateViewModel : BaseViewModel
    {
        private readonly IInvoiceService _invoiceService;
        private bool _isBusy;

        public PatientBillingByDateViewModel(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            FromDate = DateTime.Today.AddMonths(-1);
            ToDate = DateTime.Today;
        }

        private int _patientId;
        public int PatientId
        {
            get => _patientId;
            set { _patientId = value; OnPropertyChanged(); }
        }

        private DateTime _fromDate;
        public DateTime FromDate
        {
            get => _fromDate;
            set { _fromDate = value; OnPropertyChanged(); }
        }

        private DateTime _toDate;
        public DateTime ToDate
        {
            get => _toDate;
            set { _toDate = value; OnPropertyChanged(); }
        }

        private decimal _totalInvoiced;
        public decimal TotalInvoiced
        {
            get => _totalInvoiced;
            set { _totalInvoiced = value; OnPropertyChanged(); }
        }

        private decimal _totalPaid;
        public decimal TotalPaid
        {
            get => _totalPaid;
            set { _totalPaid = value; OnPropertyChanged(); }
        }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Invoice> Invoices { get; set; } = new ObservableCollection<Invoice>();
        public ObservableCollection<Payment> Payments { get; set; } = new ObservableCollection<Payment>();

        public ICommand SearchCommand { get; }

        private async Task SearchAsync()
        {
            if (PatientId <= 0) return;

            IsBusy = true;
            try
            {
                var invoicesList = await _invoiceService.GetPatientInvoicesByDateAsync(PatientId, FromDate, ToDate);
                var paymentsList = await _invoiceService.GetPatientPaymentsByDateAsync(PatientId, FromDate, ToDate);

                Invoices.Clear();
                foreach (var i in invoicesList)
                {
                    Invoices.Add(i);
                }

                Payments.Clear();
                foreach (var p in paymentsList)
                {
                    Payments.Add(p);
                }

                TotalInvoiced = invoicesList.Sum(i => i.NetTotal);
                TotalPaid = paymentsList.Sum(p => p.Amount);
                Balance = TotalInvoiced - TotalPaid;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
