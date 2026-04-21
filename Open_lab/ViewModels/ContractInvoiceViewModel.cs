using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ContractInvoiceViewModel : BaseViewModel
    {
        private readonly IContractInvoiceService _contractInvoiceService;
        private readonly ITestCatalogService _testCatalogService;
        private DateTime _from = DateTime.Today.AddMonths(-1);
        private DateTime _to = DateTime.Today;
        private int? _selectedReferralId;
        private string _invoiceNumber = string.Empty;
        private string _statusMessage = string.Empty;
        private decimal _totalPending;
        private ContractInvoice? _selectedContractInvoice;

        public ContractInvoiceViewModel(IContractInvoiceService contractInvoiceService, ITestCatalogService testCatalogService)
        {
            _contractInvoiceService = contractInvoiceService;
            _testCatalogService = testCatalogService;
            Referrals = new ObservableCollection<ReferralItem>();
            PendingInvoices = new ObservableCollection<BulkClaimRow>();
            ContractInvoices = new ObservableCollection<ContractInvoice>();

            LoadReferralsCommand = new RelayCommand(async _ => await LoadReferralsAsync());
            LoadPendingCommand = new RelayCommand(async _ => await LoadPendingAsync(), _ => SelectedReferralId.HasValue);
            CreateInvoiceCommand = new RelayCommand(async _ => await CreateInvoiceAsync(), _ => CanCreateInvoice());
            LoadHistoryCommand = new RelayCommand(async _ => await LoadHistoryAsync(), _ => SelectedReferralId.HasValue);

            _ = LoadReferralsAsync();
        }

        public DateTime From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public DateTime To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public int? SelectedReferralId
        {
            get => _selectedReferralId;
            set
            {
                if (SetProperty(ref _selectedReferralId, value))
                {
                    (LoadPendingCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (CreateInvoiceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (LoadHistoryCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set
            {
                if (SetProperty(ref _invoiceNumber, value))
                {
                    (CreateInvoiceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal TotalPending
        {
            get => _totalPending;
            private set => SetProperty(ref _totalPending, value);
        }

        public ContractInvoice? SelectedContractInvoice
        {
            get => _selectedContractInvoice;
            set => SetProperty(ref _selectedContractInvoice, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<ReferralItem> Referrals { get; }
        public ObservableCollection<BulkClaimRow> PendingInvoices { get; }
        public ObservableCollection<ContractInvoice> ContractInvoices { get; }

        public ICommand LoadReferralsCommand { get; }
        public ICommand LoadPendingCommand { get; }
        public ICommand CreateInvoiceCommand { get; }
        public ICommand LoadHistoryCommand { get; }

        private bool CanCreateInvoice()
        {
            return SelectedReferralId.HasValue && !string.IsNullOrWhiteSpace(InvoiceNumber) && PendingInvoices.Count > 0;
        }

        private async Task LoadReferralsAsync()
        {
            try
            {
                var referrals = await _testCatalogService.GetReferralsAsync();
                Referrals.Clear();
                foreach (var referral in referrals.Where(r => r.ReferralType == "Company" || r.ReferralType == "Insurance" || r.ReferralType == "Hospital"))
                {
                    Referrals.Add(new ReferralItem { ReferralId = referral.ReferralId, Name = referral.Name });
                }
                StatusMessage = $"تم تحميل {Referrals.Count} جهة تعاقد.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadPendingAsync()
        {
            if (!SelectedReferralId.HasValue)
                return;

            try
            {
                var pending = await _contractInvoiceService.GetPendingInvoicesAsync(SelectedReferralId.Value, From.Date, To.Date.AddDays(1).AddSeconds(-1));
                PendingInvoices.Clear();
                foreach (var invoice in pending)
                {
                    PendingInvoices.Add(invoice);
                }

                TotalPending = pending.Sum(p => p.NetTotal);
                (CreateInvoiceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                StatusMessage = $"تم تحميل {PendingInvoices.Count} فاتورة معلقة | الإجمالي: {TotalPending:N2}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task CreateInvoiceAsync()
        {
            if (!SelectedReferralId.HasValue || string.IsNullOrWhiteSpace(InvoiceNumber))
                return;

            try
            {
                var contractInvoiceId = await _contractInvoiceService.CreateContractInvoiceAsync(
                    SelectedReferralId.Value, InvoiceNumber, From.Date, To.Date.AddDays(1).AddSeconds(-1));

                StatusMessage = $"تم إنشاء فاتورة التعاقد رقم: {contractInvoiceId} للفواتير المحددة.";
                InvoiceNumber = string.Empty;
                await LoadPendingAsync();
                await LoadHistoryAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadHistoryAsync()
        {
            if (!SelectedReferralId.HasValue)
                return;

            try
            {
                var history = await _contractInvoiceService.GetContractInvoicesAsync(SelectedReferralId.Value);
                ContractInvoices.Clear();
                foreach (var invoice in history)
                {
                    ContractInvoices.Add(invoice);
                }
                StatusMessage = $"تم تحميل {ContractInvoices.Count} فاتورة تعاقد سابقة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }

}
