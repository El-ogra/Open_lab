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
        private decimal _editPaymentAmount;
        private decimal _netTotal;
        private decimal _balance;
        private decimal _newChargeAmount;
        private string _newChargeDescription = string.Empty;
        private string _statusMessage = string.Empty;
        private string _reason = "Financial modification";
        private string _paymentMethod = "Cash";
        private InvoicePaymentRow? _selectedPayment;

        public PatientBillingViewModel(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
            Payments = new ObservableCollection<InvoicePaymentRow>();
            AdditionalCharges = new ObservableCollection<AdditionalChargeRow>();

            LoadVisitCommand = new RelayCommand(async _ => await LoadVisitAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsView));
            SaveInvoiceCommand = new RelayCommand(async _ => await SaveInvoiceAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsEdit));
            AddPaymentCommand = new RelayCommand(async _ => await AddPaymentAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsEdit) && VisitId > 0);
            EditPaymentCommand = new RelayCommand(async _ => await EditPaymentAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsEdit) && SelectedPayment != null && EditPaymentAmount > 0);
            DeletePaymentCommand = new RelayCommand(async _ => await DeletePaymentAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsEdit) && SelectedPayment != null);
            AddChargeCommand = new RelayCommand(async _ => await AddChargeAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsEdit) && VisitId > 0 && NewChargeAmount > 0 && !string.IsNullOrWhiteSpace(NewChargeDescription));
            SettleAccountCommand = new RelayCommand(async _ => await SettleAccountAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsEdit) && VisitId > 0);
            PrintInvoiceCommand = new RelayCommand(async _ => await PrintInvoiceAsync(), _ => AppSession.HasPermission(PermissionCodes.AccountsView) && VisitId > 0);
        }

        public int VisitId
        {
            get => _visitId;
            set
            {
                if (SetProperty(ref _visitId, value))
                {
                    (AddPaymentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddChargeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (SettleAccountCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

        public decimal EditPaymentAmount
        {
            get => _editPaymentAmount;
            set
            {
                if (SetProperty(ref _editPaymentAmount, value))
                {
                    (EditPaymentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
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

        public decimal NewChargeAmount
        {
            get => _newChargeAmount;
            set
            {
                if (SetProperty(ref _newChargeAmount, value))
                {
                    (AddChargeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string NewChargeDescription
        {
            get => _newChargeDescription;
            set
            {
                if (SetProperty(ref _newChargeDescription, value))
                {
                    (AddChargeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public InvoicePaymentRow? SelectedPayment
        {
            get => _selectedPayment;
            set
            {
                if (SetProperty(ref _selectedPayment, value))
                {
                    EditPaymentAmount = value?.Amount ?? 0;
                    (EditPaymentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DeletePaymentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<InvoicePaymentRow> Payments { get; }
        public ObservableCollection<AdditionalChargeRow> AdditionalCharges { get; }

        public string Reason
        {
            get => _reason;
            set => SetProperty(ref _reason, value);
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadVisitCommand { get; }
        public ICommand SaveInvoiceCommand { get; }
        public ICommand AddPaymentCommand { get; }
        public ICommand EditPaymentCommand { get; }
        public ICommand DeletePaymentCommand { get; }
        public ICommand AddChargeCommand { get; }
        public ICommand SettleAccountCommand { get; }
        public ICommand PrintInvoiceCommand { get; }

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
                    await LoadAdditionalChargesAsync(invoice.InvoiceId);
                }
                else
                {
                    Discount = 0;
                    Paid = 0;
                    NetTotal = Total;
                    Balance = Total;
                    Payments.Clear();
                    AdditionalCharges.Clear();
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
                await _invoiceService.AddPaymentAsync(
                    invoice.InvoiceId,
                    Paid,
                    PaymentMethod,
                    AppSession.UserId > 0 ? AppSession.UserId : 1);
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
                StatusMessage = "يرجى تحديد دفعة مالية.";
                return;
            }

            try
            {
                await _invoiceService.DeletePaymentAsync(
                    SelectedPayment.PaymentId,
                    AppSession.UserId > 0 ? AppSession.UserId : 1,
                    Reason);
                await LoadVisitAsync();
                StatusMessage = "تم حذف الدفعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task EditPaymentAsync()
        {
            if (SelectedPayment == null)
            {
                return;
            }

            if (EditPaymentAmount <= 0)
            {
                StatusMessage = "أدخل مبلغ تعديل صالح.";
                return;
            }

            try
            {
                await _invoiceService.EditPaymentAsync(
                    SelectedPayment.PaymentId,
                    EditPaymentAmount,
                    AppSession.UserId > 0 ? AppSession.UserId : 1,
                    Reason);
                await LoadVisitAsync();
                StatusMessage = "تم تعديل الدفعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task AddChargeAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "رقم الزيارة غير صالح.";
                return;
            }

            try
            {
                var invoice = await _invoiceService.CreateOrUpdateInvoiceAsync(VisitId, Discount, 0);
                await _invoiceService.AddAdditionalChargeAsync(invoice.InvoiceId, NewChargeDescription, NewChargeAmount);
                NewChargeDescription = string.Empty;
                NewChargeAmount = 0;
                await LoadVisitAsync();
                StatusMessage = "تمت إضافة الرسوم الإضافية.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task SettleAccountAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "رقم الزيارة غير صالح.";
                return;
            }

            try
            {
                var invoice = await _invoiceService.SettleAccountAsync(VisitId);
                Total = invoice.Total;
                NetTotal = invoice.NetTotal;
                Balance = invoice.Balance;
                StatusMessage = "تمت تصفية الحساب وإغلاقه بنجاح.";
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
                        PaymentMethod = payment.PaymentMethod,
                        PaymentDate = payment.PaymentDate,
                        UserId = payment.UserId
                    });
            }
        }

        private async Task LoadAdditionalChargesAsync(int invoiceId)
        {
            var charges = await _invoiceService.GetAdditionalChargesAsync(invoiceId);
            AdditionalCharges.Clear();
            foreach (var charge in charges)
            {
                AdditionalCharges.Add(new AdditionalChargeRow
                {
                    AdditionalChargeId = charge.AdditionalChargeId,
                    Description = charge.Description,
                    Amount = charge.Amount
                });
            }
        }

        private async Task PrintInvoiceAsync()
        {
            if (VisitId <= 0) return;

            try
            {
                var invoice = await _invoiceService.GetByVisitIdAsync(VisitId);
                if (invoice != null)
                {
                    await _invoiceService.LogInvoicePrintedAsync(invoice.InvoiceId, AppSession.UserId > 0 ? AppSession.UserId : 1);
                    StatusMessage = "تم تسجيل عملية الطباعة برمجياً.";
                }
                else
                {
                    StatusMessage = "الفاتورة غير موجودة.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
