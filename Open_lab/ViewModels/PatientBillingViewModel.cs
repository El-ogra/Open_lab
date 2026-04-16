using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class PatientBillingViewModel : BaseViewModel
    {
        private readonly IInvoiceService _invoiceService;
        private int _visitId;
        private decimal _total;
        private decimal _discount;
        private decimal _paid;
        private decimal _netTotal;
        private decimal _balance;
        private string _statusMessage = string.Empty;

        public PatientBillingViewModel(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
            LoadVisitCommand = new RelayCommand(async _ => await LoadVisitAsync());
            SaveInvoiceCommand = new RelayCommand(async _ => await SaveInvoiceAsync());
            AddPaymentCommand = new RelayCommand(async _ => await AddPaymentAsync(), _ => VisitId > 0);
        }

        public int VisitId
        {
            get => _visitId;
            set
            {
                if (SetProperty(ref _visitId, value))
                {
                    (AddPaymentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal Total
        {
            get => _total;
            private set => SetProperty(ref _total, value);
        }

        public decimal Discount
        {
            get => _discount;
            set => SetProperty(ref _discount, value);
        }

        public decimal Paid
        {
            get => _paid;
            set => SetProperty(ref _paid, value);
        }

        public decimal NetTotal
        {
            get => _netTotal;
            private set => SetProperty(ref _netTotal, value);
        }

        public decimal Balance
        {
            get => _balance;
            private set => SetProperty(ref _balance, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadVisitCommand { get; }
        public ICommand SaveInvoiceCommand { get; }
        public ICommand AddPaymentCommand { get; }

        private async Task LoadVisitAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                Total = await _invoiceService.GetVisitTotalAsync(VisitId);

                var invoice = await _invoiceService.GetByVisitIdAsync(VisitId);
                if (invoice != null)
                {
                    Discount = invoice.Discount;
                    Paid = invoice.Paid;
                    NetTotal = invoice.NetTotal;
                    Balance = invoice.Balance;
                }
                else
                {
                    Discount = 0;
                    Paid = 0;
                    NetTotal = Total;
                    Balance = Total;
                }

                StatusMessage = "تم تحميل بيانات الفاتورة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task SaveInvoiceAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                var invoice = await _invoiceService.CreateOrUpdateInvoiceAsync(VisitId, Discount, Paid);
                Total = invoice.Total;
                NetTotal = invoice.NetTotal;
                Balance = invoice.Balance;
                StatusMessage = "تم حفظ الفاتورة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task AddPaymentAsync()
        {
            if (VisitId <= 0)
            {
                return;
            }

            if (Paid <= 0)
            {
                StatusMessage = "أدخل قيمة المدفوع.";
                return;
            }

            try
            {
                var invoice = await _invoiceService.CreateOrUpdateInvoiceAsync(VisitId, Discount, 0);
                await _invoiceService.AddPaymentAsync(invoice.InvoiceId, Paid, AppSession.UserId > 0 ? AppSession.UserId : 1);
                await LoadVisitAsync();
                StatusMessage = "تم تسجيل الدفعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
