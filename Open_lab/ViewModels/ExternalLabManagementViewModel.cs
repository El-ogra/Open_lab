using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Specialized;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ExternalLabManagementViewModel : BaseViewModel
    {
        private readonly IExternalLabService _externalLabService;
        private readonly IExternalSettlementService _externalSettlementService;
        private readonly ITestCatalogService _testCatalogService;
        private readonly IPrintService _printService;
        private readonly IReportService _reportService;
        private string _statusMessage = string.Empty;
        private ExternalLabQueue? _selectedQueueItem;
        private ShipmentManifest? _selectedManifest;
        private int? _selectedReferralId;
        private decimal _settlementAmount;
        private string? _settlementNote;
        private decimal _totalProfit;
        private string _externalResultValue = string.Empty;
        private string? _externalResultComment;
        private string? _externalReference;

        public ExternalLabManagementViewModel(
            IExternalLabService externalLabService,
            IExternalSettlementService externalSettlementService,
            ITestCatalogService testCatalogService,
            IPrintService printService,
            IReportService reportService)
        {
            _externalLabService = externalLabService;
            _externalSettlementService = externalSettlementService;
            _testCatalogService = testCatalogService;
            _printService = printService;
            _reportService = reportService;
            PendingQueue = new ObservableCollection<ExternalLabQueueRow>();
            Manifests = new ObservableCollection<ShipmentManifest>();
            Referrals = new ObservableCollection<ReferralItem>();
            SettlementHistory = new ObservableCollection<ExternalLabSettlement>();
            SelectedQueueIds = new ObservableCollection<int>();
            SelectedQueueIds.CollectionChanged += OnSelectedQueueIdsChanged;

            LoadReferralsCommand = new RelayCommand(async _ => await LoadReferralsAsync());
            LoadQueueCommand = new RelayCommand(async _ => await LoadQueueAsync());
            LoadManifestsCommand = new RelayCommand(async _ => await LoadManifestsAsync());
            CreateManifestCommand = new RelayCommand(async _ => await CreateManifestAsync(), _ => SelectedQueueIds.Count > 0 && SelectedReferralId.HasValue);
            UpdateStatusCommand = new RelayCommand(async _ => await UpdateStatusAsync(), _ => SelectedQueueItem != null);
            LoadSettlementCommand = new RelayCommand(async _ => await LoadSettlementAsync(), _ => SelectedReferralId.HasValue);
            CreateSettlementCommand = new RelayCommand(async _ => await CreateSettlementAsync(), _ => SelectedReferralId.HasValue && SettlementAmount > 0);
            EnterExternalResultCommand = new RelayCommand(async _ => await EnterExternalResultAsync(), _ => SelectedQueueItem != null && !string.IsNullOrWhiteSpace(ExternalResultValue));
            PrintExternalReportCommand = new RelayCommand(async _ => await PrintExternalReportAsync(), _ => SelectedQueueItem != null);

            _ = LoadReferralsAsync();
            _ = LoadQueueAsync();
            _ = LoadManifestsAsync();
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public int? SelectedReferralId
        {
            get => _selectedReferralId;
            set
            {
                if (SetProperty(ref _selectedReferralId, value))
                {
                    (CreateManifestCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (LoadSettlementCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (CreateSettlementCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal SettlementAmount
        {
            get => _settlementAmount;
            set
            {
                if (SetProperty(ref _settlementAmount, value))
                {
                    (CreateSettlementCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string? SettlementNote
        {
            get => _settlementNote;
            set => SetProperty(ref _settlementNote, value);
        }

        public string ExternalResultValue
        {
            get => _externalResultValue;
            set
            {
                if (SetProperty(ref _externalResultValue, value))
                {
                    (EnterExternalResultCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string? ExternalResultComment
        {
            get => _externalResultComment;
            set => SetProperty(ref _externalResultComment, value);
        }

        public string? ExternalReference
        {
            get => _externalReference;
            set => SetProperty(ref _externalReference, value);
        }

        public decimal TotalProfit
        {
            get => _totalProfit;
            private set => SetProperty(ref _totalProfit, value);
        }

        public ExternalLabQueue? SelectedQueueItem
        {
            get => _selectedQueueItem;
            set
            {
                if (SetProperty(ref _selectedQueueItem, value))
                {
                    ExternalReference = value?.ExternalReference;
                    (UpdateStatusCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (EnterExternalResultCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (PrintExternalReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ShipmentManifest? SelectedManifest
        {
            get => _selectedManifest;
            set => SetProperty(ref _selectedManifest, value);
        }

        public ObservableCollection<ReferralItem> Referrals { get; }
        public ObservableCollection<ExternalLabQueueRow> PendingQueue { get; }
        public ObservableCollection<ShipmentManifest> Manifests { get; }
        public ObservableCollection<ExternalLabSettlement> SettlementHistory { get; }
        public ObservableCollection<int> SelectedQueueIds { get; }

        public ICommand LoadReferralsCommand { get; }
        public ICommand LoadQueueCommand { get; }
        public ICommand LoadManifestsCommand { get; }
        public ICommand CreateManifestCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand LoadSettlementCommand { get; }
        public ICommand CreateSettlementCommand { get; }
        public ICommand EnterExternalResultCommand { get; }
        public ICommand PrintExternalReportCommand { get; }

        private async Task LoadReferralsAsync()
        {
            try
            {
                var referrals = await _testCatalogService.GetReferralsAsync();
                Referrals.Clear();
                foreach (var referral in referrals.Where(r => r.ReferralType == "ExternalLab"))
                {
                    Referrals.Add(new ReferralItem { ReferralId = referral.ReferralId, Name = referral.Name });
                }
                StatusMessage = $"تم تحميل {Referrals.Count} معامل خارجية.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadQueueAsync()
        {
            try
            {
                var queue = await _externalLabService.GetPendingQueueAsync();
                PendingQueue.Clear();
                foreach (var item in queue)
                {
                    PendingQueue.Add(new ExternalLabQueueRow
                    {
                        QueueId = item.QueueId,
                        VisitTestId = item.VisitTestId,
                        VisitTest = item.VisitTest!,
                        Referral = item.Referral,
                        PatientName = item.VisitTest?.Visit?.Patient?.FullName ?? "-",
                        TestName = item.VisitTest?.Test?.NameReport ?? "-",
                        Status = item.Status,
                        DateQueued = item.DateQueued,
                        ExternalReference = item.ExternalReference
                    });
                }
                StatusMessage = $"تم تحميل {PendingQueue.Count} عناصر في قائمة الانتظار.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadManifestsAsync()
        {
            try
            {
                var manifests = await _externalLabService.GetAllManifestsAsync();
                Manifests.Clear();
                foreach (var manifest in manifests)
                {
                    Manifests.Add(manifest);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task CreateManifestAsync()
        {
            if (!SelectedReferralId.HasValue || SelectedQueueIds.Count == 0)
                return;

            try
            {
                var queueIds = SelectedQueueIds.ToList();
                var manifest = await _externalLabService.CreateManifestAsync(SelectedReferralId.Value, queueIds);
                await LoadQueueAsync();
                await LoadManifestsAsync();
                SelectedQueueIds.Clear();
                StatusMessage = $"تم إنشاء بوليصة الشحن رقم: {manifest.ManifestNumber}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task UpdateStatusAsync()
        {
            if (SelectedQueueItem == null)
                return;

            try
            {
                // Cycle through statuses: Pending -> InManifest -> Shipped -> Received
                string newStatus = SelectedQueueItem.Status switch
                {
                    "Pending" => "InManifest",
                    "InManifest" => "Shipped",
                    "Shipped" => "Received",
                    _ => "Received"
                };

                await _externalLabService.UpdateQueueStatusAsync(SelectedQueueItem.QueueId, newStatus);
                await LoadQueueAsync();
                StatusMessage = $"تم تحديث الحالة إلى: {newStatus}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadSettlementAsync()
        {
            if (!SelectedReferralId.HasValue)
                return;

            try
            {
                var pendingBalance = await _externalSettlementService.GetPendingBalanceAsync(SelectedReferralId.Value);
                var history = await _externalSettlementService.GetSettlementHistoryAsync(SelectedReferralId.Value);
                
                SettlementHistory.Clear();
                foreach (var settlement in history)
                {
                    SettlementHistory.Add(settlement);
                }

                TotalProfit = await _externalSettlementService.GetTotalProfitAsync(SelectedReferralId.Value);

                StatusMessage = $"الرصيد المعلق: {pendingBalance:N2} | الربح المحقق: {TotalProfit:N2} | عدد التسويات: {SettlementHistory.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task CreateSettlementAsync()
        {
            if (!SelectedReferralId.HasValue || SettlementAmount <= 0)
                return;

            try
            {
                var settlement = await _externalSettlementService.CreateSettlementAsync(SelectedReferralId.Value, SettlementAmount, SettlementNote);
                SettlementAmount = 0;
                SettlementNote = null;
                await LoadSettlementAsync();
                StatusMessage = $"تم إنشاء تسوية بمبلغ: {settlement.AmountPaid:N2} | الرصيد المتبقي: {settlement.Balance:N2}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task EnterExternalResultAsync()
        {
            if (SelectedQueueItem == null || string.IsNullOrWhiteSpace(ExternalResultValue))
            {
                return;
            }

            try
            {
                await _externalLabService.EnterExternalLabResultAsync(
                    SelectedQueueItem.QueueId,
                    ExternalResultValue,
                    ExternalResultComment,
                    ExternalReference);

                ExternalResultValue = string.Empty;
                ExternalResultComment = null;
                await LoadQueueAsync();
                StatusMessage = "تم إدخال نتيجة المعمل الخارجي بنجاح.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task PrintExternalReportAsync()
        {
            if (SelectedQueueItem == null)
            {
                return;
            }

            try
            {
                var visitId = SelectedQueueItem.VisitTest?.VisitId ?? 0;
                if (visitId <= 0)
                {
                    StatusMessage = "لا يمكن تحديد الزيارة المرتبطة.";
                    return;
                }

                var report = await _reportService.GetVisitReportAsync(visitId);
                if (report == null)
                {
                    StatusMessage = "لم يتم العثور على تقرير للطباعة.";
                    return;
                }

                await _printService.PrintVisitReportAsync(report, true);
                StatusMessage = "تم إرسال تقرير المعمل الخارجي للطباعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private void OnSelectedQueueIdsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            (CreateManifestCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public class ExternalLabQueueRow : ExternalLabQueue
    {
        public string PatientName { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
    }

}
