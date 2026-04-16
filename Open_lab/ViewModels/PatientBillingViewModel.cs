using System;
using System.Collections.ObjectModel;
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
        private InvoicePaymentRow? _selectedPayment;

        public PatientBillingViewModel(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
            Payments = new ObservableCollection<InvoicePaymentRow>();

            LoadVisitCommand = new RelayCommand(async _ => await LoadVisitAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsView));
            SaveInvoiceCommand = new RelayCommand(async _ => await SaveInvoiceAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsEdit));
            AddPaymentCommand = new RelayCommand(async _ => await AddPaymentAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsEdit) && VisitId > 0);
            DeletePaymentCommand = new RelayCommand(async _ => await DeletePaymentAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsEdit) && SelectedPayment != null);
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

        public InvoicePaymentRow? SelectedPayment
        {
            get => _selectedPayment;
            set
            {
                if (SetProperty(ref _selectedPayment, value))
                {
                    (DeletePaymentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<InvoicePaymentRow> Payments { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadVisitCommand { get; }
        public ICommand SaveInvoiceCommand { get; }
        public ICommand AddPaymentCommand { get; }
        public ICommand DeletePaymentCommand { get; }

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
                    await LoadPaymentsAsync(invoice.InvoiceId);
                }
                else
                {
                    Discount = 0;
                    Paid = 0;
                    NetTotal = Total;
                    Balance = Total;
                    Payments.Clear();
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
                var invoice = await _invoiceService.CreateOrUpdateInvoiceAsync(VisitId, Discount, 0);
                Total = invoice.Total;
                Paid = invoice.Paid;
                NetTotal = invoice.NetTotal;
                Balance = invoice.Balance;
                await LoadPaymentsAsync(invoice.InvoiceId);
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
                Paid = 0;
                await LoadVisitAsync();
                StatusMessage = "تم تسجيل الدفعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task DeletePaymentAsync()
        {
            if (SelectedPayment == null)
            {
                return;
            }

            try
            {
                await _invoiceService.DeletePaymentAsync(SelectedPayment.PaymentId);
                await LoadVisitAsync();
                StatusMessage = "تم حذف الدفعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadPaymentsAsync(int invoiceId)
        {
            var items = await _invoiceService.GetPaymentsAsync(invoiceId);
            Payments.Clear();
            foreach (var payment in items)
            {
                Payments.Add(new InvoicePaymentRow
                {
                    PaymentId = payment.PaymentId,
                    Amount = payment.Amount,
                    PaymentDate = payment.PaymentDate,
                    UserId = payment.UserId
                });
            }
        }
    }
}
