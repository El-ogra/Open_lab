using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class DeliveryViewModel : BaseViewModel
    {
        private readonly IDeliveryService _deliveryService;
        private readonly IInvoiceService? _invoiceService;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private string _keyword = string.Empty;
        private DeliveryVisitRow? _selectedVisit;
        private decimal _paymentAmount;
        private string _statusMessage = string.Empty;

        private bool _isVIP;
        private bool _isLab;
        private bool _isPat;
        private int _patientAge;
        private string _patientGender = string.Empty;
        private string _patientCode = string.Empty;
        private string _patientReferral = string.Empty;
        private decimal _totalRequired;
        private decimal _totalAmount;

        public DeliveryViewModel(IDeliveryService deliveryService)
            : this(deliveryService, null)
        {
        }

        public DeliveryViewModel(IDeliveryService deliveryService, IInvoiceService? invoiceService)
        {
            _deliveryService = deliveryService;
            _invoiceService = invoiceService;
            Visits = new ObservableCollection<DeliveryVisitRow>();

            SearchCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.DeliveryView));
            DeliverCommand = new RelayCommand(async _ => await DeliverAsync(), _ => CanDeliver());
            ReopenCommand = new RelayCommand(async _ => await ReopenAsync(), _ => CanReopen());
            PayCommand = new RelayCommand(async _ => await PayAsync(), _ => CanPay());

            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            PatientAccountCommand = new RelayCommand(_ => { /* navigate */ });
            FilterAllCommand = new RelayCommand(_ => { IsVIP = false; IsLab = false; IsPat = false; _ = LoadAsync(); });
        }

        public ObservableCollection<DeliveryVisitRow> Visits { get; }
        public ObservableCollection<VisitTestRow> SelectedVisitTests { get; } = new ObservableCollection<VisitTestRow>();

        public bool IsVIP { get => _isVIP; set => SetProperty(ref _isVIP, value); }
        public bool IsLab { get => _isLab; set => SetProperty(ref _isLab, value); }
        public bool IsPat { get => _isPat; set => SetProperty(ref _isPat, value); }
        public int PatientAge { get => _patientAge; set => SetProperty(ref _patientAge, value); }
        public string PatientGender { get => _patientGender; set => SetProperty(ref _patientGender, value); }
        public string PatientCode { get => _patientCode; set => SetProperty(ref _patientCode, value); }
        public string PatientReferral { get => _patientReferral; set => SetProperty(ref _patientReferral, value); }
        public decimal TotalRequired { get => _totalRequired; set => SetProperty(ref _totalRequired, value); }
        public decimal TotalAmount { get => _totalAmount; set => SetProperty(ref _totalAmount, value); }

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

        public string Keyword
        {
            get => _keyword;
            set => SetProperty(ref _keyword, value);
        }

        public DeliveryVisitRow? SelectedVisit
        {
            get => _selectedVisit;
            set
            {
                if (SetProperty(ref _selectedVisit, value))
                {
                    _ = LoadSelectedVisitDetailsAsync();
                    RaiseActionsState();
                }
            }
        }

        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                if (SetProperty(ref _paymentAmount, value))
                {
                    RaiseActionsState();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand DeliverCommand { get; }
        public ICommand ReopenCommand { get; }
        public ICommand PayCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand PatientAccountCommand { get; }
        public ICommand FilterAllCommand { get; }

        private async Task LoadSelectedVisitDetailsAsync()
        {
            SelectedVisitTests.Clear();
            if (SelectedVisit == null) return;

            try
            {
                var tests = await _deliveryService.GetVisitTestsAsync(SelectedVisit.VisitId);
                foreach (var t in tests)
                {
                    SelectedVisitTests.Add(t);
                }

                if (tests.Any())
                {
                    PatientAge = tests.First().PatientAge;
                    PatientGender = tests.First().PatientGender;
                    PatientCode = SelectedVisit.PatientName; 
                    PatientReferral = "Self"; 
                    TotalAmount = tests.Sum(x => x.Price);
                    TotalRequired = SelectedVisit.Balance;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                var rows = await _deliveryService.SearchAsync(DateFrom, DateTo.AddDays(1).AddSeconds(-1), Keyword);
                Visits.Clear();
                foreach (var row in rows)
                {
                    Visits.Add(row);
                }

                StatusMessage = $"تم تحميل {Visits.Count} زيارة.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
            finally
            {
                RaiseActionsState();
            }
        }

        private async Task DeliverAsync()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                await _deliveryService.DeliverAsync(SelectedVisit.VisitId, AppSession.UserId > 0 ? AppSession.UserId : 1);
                await LoadAsync();
                StatusMessage = "تم تسليم الزيارة بنجاح.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task ReopenAsync()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                await _deliveryService.ReopenDeliveryAsync(SelectedVisit.VisitId);
                await LoadAsync();
                StatusMessage = "تم إعادة فتح التسليم.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task PayAsync()
        {
            if (SelectedVisit == null || _invoiceService == null)
            {
                StatusMessage = "لا توجد زيارة محددة أو خدمة الفواتير غير متاحة.";
                return;
            }

            try
            {
                var invoice = await _invoiceService.GetByVisitIdAsync(SelectedVisit.VisitId);
                if (invoice == null)
                {
                    StatusMessage = "لا توجد فاتورة لهذه الزيارة.";
                    return;
                }

                await _invoiceService.AddPaymentAsync(
                    invoice.InvoiceId,
                    PaymentAmount,
                    "Cash",
                    AppSession.UserId > 0 ? AppSession.UserId : 1);

                PaymentAmount = 0;
                await LoadAsync();
                StatusMessage = "تم تسجيل الدفع بنجاح.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private bool CanDeliver()
        {
            return AppSession.HasPermission(PermissionCodes.DeliveryEdit)
                && SelectedVisit != null
                && SelectedVisit.IsReadyForDelivery
                && !SelectedVisit.IsDelivered;
        }

        private bool CanReopen()
        {
            return AppSession.HasPermission(PermissionCodes.DeliveryEdit)
                && SelectedVisit != null
                && SelectedVisit.IsDelivered;
        }

        private bool CanPay()
        {
            return AppSession.HasPermission(PermissionCodes.DeliveryEdit)
                && _invoiceService != null
                && SelectedVisit != null
                && PaymentAmount > 0;
        }

        private void RaiseActionsState()
        {
            (SearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeliverCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ReopenCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PayCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
