using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class PatientTestsSelectionViewModel : BaseViewModel
    {
        private readonly IPatientService _patientService;
        private readonly IVisitService _visitService;
        private readonly ITestCatalogService _testCatalogService;
        private readonly IInvoiceService _invoiceService;
        private string _labId = string.Empty;
        private string _patientName = string.Empty;
        private int _patientId;
        private int _visitId;
        private string _statusMessage = string.Empty;
        private Test? _selectedAvailableTest;
        private SelectedTestItem? _selectedVisitTest;
        private string _selectedAccountType = "Cash";
        private Referral? _selectedReferral;
        private string _searchText = string.Empty;
        private CustomGroup? _selectedCustomGroup;
        private decimal _totalAmount;

        public PatientTestsSelectionViewModel(
            IPatientService patientService,
            IVisitService visitService,
            ITestCatalogService testCatalogService,
            IInvoiceService invoiceService)
        {
            _patientService = patientService;
            _visitService = visitService;
            _testCatalogService = testCatalogService;
            _invoiceService = invoiceService;
            AvailableTests = new ObservableCollection<Test>();
            FilteredAvailableTests = new ObservableCollection<Test>();
            SelectedTests = new ObservableCollection<SelectedTestItem>();
            Referrals = new ObservableCollection<Referral>();
            AccountTypes = new ObservableCollection<string> { "Cash", "Referral" };
            CustomGroups = new ObservableCollection<CustomGroup>();

            LoadPatientCommand = new RelayCommand(async _ => await LoadPatientAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            CreateVisitCommand = new RelayCommand(async _ => await CreateVisitAsync(), _ => AppSession.HasPermission(PermissionCodes.VisitsEdit) && PatientId > 0);
            AddTestCommand = new RelayCommand(async _ => await AddTestAsync(), _ => AppSession.HasPermission(PermissionCodes.VisitsEdit) && VisitId > 0 && SelectedAvailableTest != null);
            AddCustomGroupCommand = new RelayCommand(async _ => await AddCustomGroupAsync(), _ => AppSession.HasPermission(PermissionCodes.VisitsEdit) && VisitId > 0 && SelectedCustomGroup != null);
            RemoveTestCommand = new RelayCommand(async _ => await RemoveTestAsync(), _ => AppSession.HasPermission(PermissionCodes.VisitsEdit) && SelectedVisitTest != null);
            RefreshTestsCommand = new RelayCommand(async _ => await LoadAvailableTestsAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsView));

            _ = InitializeAsync();
        }

        public string LabId
        {
            get => _labId;
            set => SetProperty(ref _labId, value);
        }

        public string PatientName
        {
            get => _patientName;
            private set => SetProperty(ref _patientName, value);
        }

        public int PatientId
        {
            get => _patientId;
            private set
            {
                if (SetProperty(ref _patientId, value))
                {
                    (CreateVisitCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int VisitId
        {
            get => _visitId;
            private set
            {
                if (SetProperty(ref _visitId, value))
                {
                    (AddTestCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddCustomGroupCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string SelectedAccountType
        {
            get => _selectedAccountType;
            set => SetProperty(ref _selectedAccountType, value);
        }

        public Referral? SelectedReferral
        {
            get => _selectedReferral;
            set => SetProperty(ref _selectedReferral, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyTestFilter();
                }
            }
        }

        public CustomGroup? SelectedCustomGroup
        {
            get => _selectedCustomGroup;
            set
            {
                if (SetProperty(ref _selectedCustomGroup, value))
                {
                    (AddCustomGroupCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            private set => SetProperty(ref _totalAmount, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<Test> AvailableTests { get; }
        public ObservableCollection<Test> FilteredAvailableTests { get; }
        public ObservableCollection<SelectedTestItem> SelectedTests { get; }
        public ObservableCollection<Referral> Referrals { get; }
        public ObservableCollection<string> AccountTypes { get; }
        public ObservableCollection<CustomGroup> CustomGroups { get; }

        public Test? SelectedAvailableTest
        {
            get => _selectedAvailableTest;
            set
            {
                if (SetProperty(ref _selectedAvailableTest, value))
                {
                    (AddTestCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public SelectedTestItem? SelectedVisitTest
        {
            get => _selectedVisitTest;
            set
            {
                if (SetProperty(ref _selectedVisitTest, value))
                {
                    (RemoveTestCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand LoadPatientCommand { get; }
        public ICommand CreateVisitCommand { get; }
        public ICommand AddTestCommand { get; }
        public ICommand AddCustomGroupCommand { get; }
        public ICommand RemoveTestCommand { get; }
        public ICommand RefreshTestsCommand { get; }

        private async Task InitializeAsync()
        {
            await LoadAvailableTestsAsync();
            await LoadReferralsAsync();
            await LoadCustomGroupsAsync();
        }

        private async Task LoadPatientAsync()
        {
            if (string.IsNullOrWhiteSpace(LabId))
            {
                StatusMessage = "يرجى إدخال Lab ID.";
                return;
            }

            try
            {
                var patient = await _patientService.GetByLabIdAsync(LabId);
                if (patient == null)
                {
                    StatusMessage = "لم يتم العثور على المريض.";
                    return;
                }

                PatientId = patient.PatientId;
                PatientName = patient.FullName;

                var existingVisits = await _visitService.GetByPatientIdAsync(PatientId);
                var openVisit = existingVisits.FirstOrDefault(v => string.Equals(v.Status, "Open", StringComparison.OrdinalIgnoreCase));
                VisitId = openVisit?.VisitId ?? 0;
                SelectedAccountType = openVisit?.AccountType ?? "Cash";
                SelectedReferral = openVisit?.ReferralId.HasValue == true
                    ? Referrals.FirstOrDefault(r => r.ReferralId == openVisit.ReferralId.Value)
                    : null;

                if (VisitId > 0)
                {
                    await LoadVisitTestsAsync();
                    await SyncInvoiceAsync();
                    StatusMessage = $"تم تحميل المريض. الزيارة المفتوحة: {VisitId}.";
                }
                else
                {
                    SelectedTests.Clear();
                    TotalAmount = 0;
                    StatusMessage = "تم تحميل بيانات المريض. أنشئ زيارة جديدة للمتابعة.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task CreateVisitAsync()
        {
            try
            {
                if (string.Equals(SelectedAccountType, "Referral", StringComparison.OrdinalIgnoreCase) && SelectedReferral == null)
                {
                    StatusMessage = "يرجى اختيار جهة إحالة لنوع حساب التحويل.";
                    return;
                }

                var visit = await _visitService.CreateAsync(new Visit
                {
                    PatientId = PatientId,
                    VisitDate = DateTime.Now,
                    AccountType = SelectedAccountType,
                    ReferralId = string.Equals(SelectedAccountType, "Referral", StringComparison.OrdinalIgnoreCase) ? SelectedReferral?.ReferralId : null,
                    Status = "Open"
                });

                VisitId = visit.VisitId;
                SelectedTests.Clear();
                TotalAmount = 0;
                await SyncInvoiceAsync();
                StatusMessage = $"تم إنشاء زيارة رقم {VisitId}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadAvailableTestsAsync()
        {
            try
            {
                var tests = await _testCatalogService.GetAllTestsAsync();
                AvailableTests.Clear();
                foreach (var test in tests)
                {
                    AvailableTests.Add(test);
                }

                ApplyTestFilter();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private void ApplyTestFilter()
        {
            var term = SearchText?.Trim();
            var filtered = string.IsNullOrWhiteSpace(term)
                ? AvailableTests
                : new ObservableCollection<Test>(AvailableTests.Where(t =>
                    t.NameReport.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || t.NameReceipt.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || t.Code.Contains(term, StringComparison.OrdinalIgnoreCase)));

            FilteredAvailableTests.Clear();
            foreach (var test in filtered)
            {
                FilteredAvailableTests.Add(test);
            }
        }

        private async Task LoadReferralsAsync()
        {
            try
            {
                var referrals = await _testCatalogService.GetReferralsAsync();
                Referrals.Clear();
                foreach (var referral in referrals)
                {
                    Referrals.Add(referral);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadCustomGroupsAsync()
        {
            try
            {
                var groups = await _testCatalogService.GetCustomGroupsAsync();
                CustomGroups.Clear();
                foreach (var group in groups)
                {
                    CustomGroups.Add(group);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task AddTestAsync()
        {
            if (SelectedAvailableTest == null)
            {
                return;
            }

            try
            {
                await _visitService.AddTestToVisitAsync(VisitId, SelectedAvailableTest.TestId);
                await LoadVisitTestsAsync();
                await SyncInvoiceAsync();
                StatusMessage = "تمت إضافة التحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task AddCustomGroupAsync()
        {
            if (SelectedCustomGroup == null)
            {
                return;
            }

            try
            {
                var groupItems = await _testCatalogService.GetCustomGroupItemsAsync(SelectedCustomGroup.CustomGroupId);
                if (groupItems.Count == 0)
                {
                    StatusMessage = "المجموعة المختارة لا تحتوي تحاليل.";
                    return;
                }

                var added = 0;
                foreach (var item in groupItems)
                {
                    try
                    {
                        await _visitService.AddTestToVisitAsync(VisitId, item.TestId);
                        added++;
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }

                await LoadVisitTestsAsync();
                await SyncInvoiceAsync();
                StatusMessage = $"تمت إضافة {added} تحليل من المجموعة \"{SelectedCustomGroup.Name}\".";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task RemoveTestAsync()
        {
            if (SelectedVisitTest == null)
            {
                return;
            }

            try
            {
                await _visitService.RemoveVisitTestAsync(SelectedVisitTest.VisitTestId);
                await LoadVisitTestsAsync();
                await SyncInvoiceAsync();
                StatusMessage = "تم حذف التحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadVisitTestsAsync()
        {
            var visitTests = await _visitService.GetVisitTestsAsync(VisitId);
            SelectedTests.Clear();
            foreach (var visitTest in visitTests)
            {
                SelectedTests.Add(new SelectedTestItem
                {
                    VisitTestId = visitTest.VisitTestId,
                    TestId = visitTest.TestId,
                    TestName = visitTest.Test?.NameReport ?? $"Test#{visitTest.TestId}",
                    Price = visitTest.Price
                });
            }

            TotalAmount = SelectedTests.Sum(t => t.Price);
        }

        private async Task SyncInvoiceAsync()
        {
            if (VisitId <= 0)
            {
                return;
            }

            var existingInvoice = await _invoiceService.GetByVisitIdAsync(VisitId);
            var discount = existingInvoice?.Discount ?? 0;
            var invoice = await _invoiceService.CreateOrUpdateInvoiceAsync(VisitId, discount, 0);
            TotalAmount = invoice.Total;
        }
    }
}
