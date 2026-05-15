using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class PatientRegistrationViewModel : BaseViewModel
    {
        private readonly IPatientService _patientService;
        private readonly ITestCatalogService? _testCatalogService;
        private readonly IVisitService? _visitService;
        private readonly IInvoiceService? _invoiceService;
        private readonly IBarcodeDialogService? _barcodeDialogService;
        private readonly IPrintService? _printService;
        private string _labId = string.Empty;
        private string _fullName = string.Empty;
        private string _gender = string.Empty;
        private DateTime? _birthDate;
        private string? _phone;
        private string? _address;
        private string? _chronicDiseases;
        private string? _allergies;
        private string? _medications;
        private string? _medicalNotes;
        private string? _mobilePhone;
        private string? _homePhone;
        private string? _nationalId;
        private string? _email;
        private string _ageUnit = "سنوات";
        private int? _age;
        private bool _isVip;
        private bool _isFasting;
        private string? _fastingHours;
        private string _accountType = "Individual";
        private Test? _selectedAvailableTest;
        private SelectedTestItem? _selectedTest;
        private decimal _discountPercent;
        private decimal _paidAmount;
        private decimal _previousPaidAmount;
        private int _currentVisitId;
        private int _patientId;
        private string _statusMessage = string.Empty;
        private Patient? _selectedPatient;
        private Referral? _selectedReferral;
        private string _patientCode = string.Empty;
        private string _title = "السيد";
        private DateTime _entryDate = DateTime.Today;
        private DateTime _deliveryDate = DateTime.Today;
        private TimeSpan _entryTime = DateTime.Now.TimeOfDay;
        private TimeSpan _deliveryTime = DateTime.Now.TimeOfDay;
        private string _referralSource = string.Empty;
        private string _referralAddress = string.Empty;
        private string _referralPhone = string.Empty;
        private string _responsibleName = string.Empty;
        private string _referralTitle = "د./";
        private bool _printReferral;
        private bool _testedBefore;
        private bool _isAntibiotic;
        private bool _isDiabetesMed;
        private bool _isBloodThinner;
        private bool _isVirusMed;
        private bool _isBloodTransfusion;
        private bool _isGlandMed;
        private bool _isLiverMed;
        private bool _isPregnant;
        private bool _isPregnantFemale;
        private bool _isDyeScan;
        private bool _isSmoker;
        private string _testCategoryFilter = "Routine Tests";
        private string _searchText = string.Empty;
        private decimal _discountValue;

        public PatientRegistrationViewModel(IPatientService patientService)
            : this(patientService, null)
        {
        }

        public PatientRegistrationViewModel(IPatientService patientService, ITestCatalogService? testCatalogService)
            : this(patientService, testCatalogService, null, null, null, null)
        {
        }

        public PatientRegistrationViewModel(
            IPatientService patientService,
            ITestCatalogService? testCatalogService,
            IVisitService? visitService,
            IInvoiceService? invoiceService,
            IBarcodeService? barcodeService,
            IPrintService? printService)
            : this(
                  patientService,
                  testCatalogService,
                  visitService,
                  invoiceService,
                  barcodeService == null ? null : new BarcodeDialogService(barcodeService, printService),
                  printService,
                  true)
        {
        }

        [ActivatorUtilitiesConstructor]
        public PatientRegistrationViewModel(
            IPatientService patientService,
            ITestCatalogService? testCatalogService,
            IVisitService? visitService,
            IInvoiceService? invoiceService,
            IBarcodeDialogService? barcodeDialogService,
            IPrintService? printService,
            bool initialize = true)
        {
            _patientService = patientService;
            _testCatalogService = testCatalogService;
            _visitService = visitService;
            _invoiceService = invoiceService;
            _barcodeDialogService = barcodeDialogService;
            _printService = printService;
            Results = new ObservableCollection<Patient>();
            Referrals = new ObservableCollection<Referral>();
            AvailableTests = new ObservableCollection<Test>();
            SelectedTests = new ObservableCollection<SelectedTestItem>();
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit));
            NewCommand = new RelayCommand(async _ => await ClearFormAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit));
            GenerateLabIdCommand = new RelayCommand(async _ => await GenerateLabIdAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit) && PatientId == 0);
            LoadByLabIdCommand = new RelayCommand(async _ => await LoadByLabIdAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit) && PatientId > 0);
            AddTestCommand = new RelayCommand(_ => AddSelectedTest(), _ => SelectedAvailableTest != null);
            AddAllTestsCommand = new RelayCommand(_ => AddAllVisibleTests(), _ => AvailableTests.Count > 0);
            RemoveTestCommand = new RelayCommand(_ => RemoveSelectedTest(), _ => SelectedTest != null);
            ShowBarcodeCommand = new RelayCommand(_ => ShowBarcode(), _ => PatientId > 0);
            PrintReceiptCommand = new RelayCommand(async _ => await PrintReceiptAsync(), _ => CurrentVisitId > 0);
            AddSelectedTestCommand = new RelayCommand(_ => AddSelectedTest(), _ => SelectedAvailableTest != null);
            RemoveSelectedTestCommand = new RelayCommand(_ => RemoveSelectedTest(), _ => SelectedTest != null);
            EditCommand = new RelayCommand(async _ => await SaveAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit) && PatientId > 0);
            GoToResultsCommand = new RelayCommand(_ => { }, _ => true);
            ResetCommand = new RelayCommand(async _ => await ClearFormAsync(), _ => true);
            DocumentsCommand = new RelayCommand(_ => { }, _ => true);
            GoToHomeCommand = new RelayCommand(_ => { }, _ => true);
            LoadByCodeCommand = new RelayCommand(async _ => await LoadByCodeAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            NewPatientCommand = new RelayCommand(async _ => await ClearFormAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit));

            if (initialize)
            {
                _ = InitializeAsync();
            }
        }

        public int PatientId
        {
            get => _patientId;
            private set
            {
                if (SetProperty(ref _patientId, value))
                {
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (GenerateLabIdCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (ShowBarcodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int CurrentVisitId
        {
            get => _currentVisitId;
            private set
            {
                if (SetProperty(ref _currentVisitId, value))
                {
                    (PrintReceiptCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string LabId
        {
            get => _labId;
            set => SetProperty(ref _labId, value);
        }

        public string PatientCode
        {
            get => _patientCode;
            set => SetProperty(ref _patientCode, value);
        }

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        public DateTime? BirthDate
        {
            get => _birthDate;
            set => SetProperty(ref _birthDate, value);
        }

        public string? Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string? ChronicDiseases
        {
            get => _chronicDiseases;
            set => SetProperty(ref _chronicDiseases, value);
        }

        public string? Allergies
        {
            get => _allergies;
            set => SetProperty(ref _allergies, value);
        }

        public string? Medications
        {
            get => _medications;
            set => SetProperty(ref _medications, value);
        }

        public string? MedicalNotes
        {
            get => _medicalNotes;
            set => SetProperty(ref _medicalNotes, value);
        }

        public string? MobilePhone
        {
            get => _mobilePhone;
            set
            {
                if (SetProperty(ref _mobilePhone, value))
                {
                    Phone = value;
                }
            }
        }

        public string? HomePhone
        {
            get => _homePhone;
            set => SetProperty(ref _homePhone, value);
        }

        public string? NationalId
        {
            get => _nationalId;
            set => SetProperty(ref _nationalId, value);
        }

        public string? Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public int? Age
        {
            get => _age;
            set => SetProperty(ref _age, value);
        }

        public string AgeUnit
        {
            get => _ageUnit;
            set => SetProperty(ref _ageUnit, value);
        }

        public bool IsVip
        {
            get => _isVip;
            set => SetProperty(ref _isVip, value);
        }

        public bool IsFasting
        {
            get => _isFasting;
            set => SetProperty(ref _isFasting, value);
        }

        public string? FastingHours
        {
            get => _fastingHours;
            set => SetProperty(ref _fastingHours, value);
        }

        public string AccountType
        {
            get => _accountType;
            set => SetProperty(ref _accountType, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DateTime EntryDate
        {
            get => _entryDate;
            set => SetProperty(ref _entryDate, value);
        }

        public DateTime DeliveryDate
        {
            get => _deliveryDate;
            set => SetProperty(ref _deliveryDate, value);
        }

        public TimeSpan EntryTime
        {
            get => _entryTime;
            set => SetProperty(ref _entryTime, value);
        }

        public TimeSpan DeliveryTime
        {
            get => _deliveryTime;
            set => SetProperty(ref _deliveryTime, value);
        }

        public string ReferralSource
        {
            get => _referralSource;
            set => SetProperty(ref _referralSource, value);
        }

        public string ReferralAddress
        {
            get => _referralAddress;
            set => SetProperty(ref _referralAddress, value);
        }

        public string ReferralPhone
        {
            get => _referralPhone;
            set => SetProperty(ref _referralPhone, value);
        }

        public string ResponsibleName
        {
            get => _responsibleName;
            set => SetProperty(ref _responsibleName, value);
        }

        public string ReferralTitle
        {
            get => _referralTitle;
            set => SetProperty(ref _referralTitle, value);
        }

        public bool PrintReferral
        {
            get => _printReferral;
            set => SetProperty(ref _printReferral, value);
        }

        public bool TestedBefore
        {
            get => _testedBefore;
            set => SetProperty(ref _testedBefore, value);
        }

        public bool IsAntibiotic
        {
            get => _isAntibiotic;
            set => SetProperty(ref _isAntibiotic, value);
        }

        public bool IsDiabetesMed
        {
            get => _isDiabetesMed;
            set => SetProperty(ref _isDiabetesMed, value);
        }

        public bool IsBloodThinner
        {
            get => _isBloodThinner;
            set => SetProperty(ref _isBloodThinner, value);
        }

        public bool IsVirusMed
        {
            get => _isVirusMed;
            set => SetProperty(ref _isVirusMed, value);
        }

        public bool IsBloodTransfusion
        {
            get => _isBloodTransfusion;
            set => SetProperty(ref _isBloodTransfusion, value);
        }

        public bool IsGlandMed
        {
            get => _isGlandMed;
            set => SetProperty(ref _isGlandMed, value);
        }

        public bool IsLiverMed
        {
            get => _isLiverMed;
            set => SetProperty(ref _isLiverMed, value);
        }

        public bool IsPregnant
        {
            get => _isPregnant;
            set => SetProperty(ref _isPregnant, value);
        }

        public bool IsPregnantFemale
        {
            get => _isPregnantFemale;
            set => SetProperty(ref _isPregnantFemale, value);
        }

        public bool IsDyeScan
        {
            get => _isDyeScan;
            set => SetProperty(ref _isDyeScan, value);
        }

        public bool IsSmoker
        {
            get => _isSmoker;
            set => SetProperty(ref _isSmoker, value);
        }

        public string TestCategoryFilter
        {
            get => _testCategoryFilter;
            set => SetProperty(ref _testCategoryFilter, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public decimal TotalAmount => SelectedTests.Sum(t => t.Price);

        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (SetProperty(ref _discountPercent, value))
                {
                    RaiseFinancialTotals();
                }
            }
        }

        public decimal DiscountValue
        {
            get => _discountValue;
            set
            {
                if (SetProperty(ref _discountValue, value))
                {
                    RaiseFinancialTotals();
                }
            }
        }

        public decimal DiscountAmount => Math.Round(TotalAmount * DiscountPercent / 100m, 2);

        public decimal NetAmount => TotalAmount - DiscountAmount;

        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                if (SetProperty(ref _paidAmount, value))
                {
                    RaiseFinancialTotals();
                }
            }
        }

        public decimal PreviousPaid
        {
            get => _previousPaidAmount;
            set
            {
                if (SetProperty(ref _previousPaidAmount, value))
                {
                    RaiseFinancialTotals();
                }
            }
        }

        public decimal PreviousPaidAmount
        {
            get => _previousPaidAmount;
            set
            {
                if (SetProperty(ref _previousPaidAmount, value))
                {
                    RaiseFinancialTotals();
                }
            }
        }

        public decimal BalanceForLab => Math.Max(NetAmount - PaidAmount - PreviousPaidAmount, 0);

        public decimal BalanceForPatient => Math.Max(PaidAmount + PreviousPaidAmount - NetAmount, 0);

        public decimal LabRemainder => BalanceForLab;

        public decimal PatientRemainder => BalanceForPatient;

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<Patient> Results { get; }
        public ObservableCollection<Referral> Referrals { get; }
        public ObservableCollection<Test> AvailableTests { get; }
        public ObservableCollection<SelectedTestItem> SelectedTests { get; }

        public Test? SelectedAvailableTest
        {
            get => _selectedAvailableTest;
            set
            {
                if (SetProperty(ref _selectedAvailableTest, value))
                {
                    (AddTestCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (AddSelectedTestCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public SelectedTestItem? SelectedTest
        {
            get => _selectedTest;
            set
            {
                if (SetProperty(ref _selectedTest, value))
                {
                    (RemoveTestCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (RemoveSelectedTestCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public Referral? SelectedReferral
        {
            get => _selectedReferral;
            set => SetProperty(ref _selectedReferral, value);
        }

        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value) && value != null)
                {
                    _ = LoadFromPatientAsync(value);
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand GenerateLabIdCommand { get; }
        public ICommand LoadByLabIdCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddTestCommand { get; }
        public ICommand AddAllTestsCommand { get; }
        public ICommand RemoveTestCommand { get; }
        public ICommand ShowBarcodeCommand { get; }
        public ICommand PrintReceiptCommand { get; }
        public ICommand AddSelectedTestCommand { get; }
        public ICommand RemoveSelectedTestCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand GoToResultsCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand DocumentsCommand { get; }
        public ICommand GoToHomeCommand { get; }
        public ICommand LoadByCodeCommand { get; }
        public ICommand NewPatientCommand { get; }

        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FullName))
                {
                    StatusMessage = "خطأ: يرجى إدخال اسم المريض.";
                    return;
                }

                if (PatientId == 0)
                {
                    var created = await _patientService.CreateAsync(new Patient
                    {
                        LabId = LabId,
                        FullName = FullName,
                        Gender = Gender,
                        BirthDate = BirthDate,
                        Age = Age,
                        IsVip = IsVip,
                        Phone = Phone,
                        HomePhone = HomePhone,
                        NationalId = NationalId,
                        Email = Email,
                        Address = Address,
                        ReferralId = NormalizeReferralId(SelectedReferral)
                    });

                    PatientId = created.PatientId;
                    LabId = created.LabId;
                    await SaveMedicalHistoryAsync();
                    await SaveVisitAndInvoiceAsync();
                    StatusMessage = "تم إنشاء المريض بنجاح.";
                }
                else
                {
                    await _patientService.UpdateAsync(new Patient
                    {
                        PatientId = PatientId,
                        LabId = LabId,
                        FullName = FullName,
                        Gender = Gender,
                        BirthDate = BirthDate,
                        Age = Age,
                        IsVip = IsVip,
                        Phone = Phone,
                        HomePhone = HomePhone,
                        NationalId = NationalId,
                        Email = Email,
                        Address = Address,
                        ReferralId = NormalizeReferralId(SelectedReferral)
                    }, AppSession.UserId > 0 ? AppSession.UserId : 1);

                    await SaveMedicalHistoryAsync();
                    await SaveVisitAndInvoiceAsync();
                    StatusMessage = "تم تحديث بيانات المريض.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task GenerateLabIdAsync()
        {
            if (PatientId > 0)
            {
                return;
            }

            try
            {
                LabId = await _patientService.GenerateNextLabIdAsync(DateTime.Today);
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ توليد Lab ID: {ex.Message}";
            }
        }

        private async Task LoadByLabIdAsync()
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

                await LoadFromPatientAsync(patient);
                StatusMessage = "تم تحميل بيانات المريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadByCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(PatientCode))
            {
                StatusMessage = "يرجى إدخال كود المريض.";
                return;
            }

            try
            {
                var patient = await _patientService.GetByLabIdAsync(PatientCode);
                if (patient == null)
                {
                    StatusMessage = "لم يتم العثور على المريض.";
                    return;
                }

                await LoadFromPatientAsync(patient);
                StatusMessage = "تم تحميل بيانات المريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                var results = await _patientService.SearchAsync(FullName, Phone);
                Results.Clear();
                foreach (var patient in results)
                {
                    Results.Add(patient);
                }
                StatusMessage = $"تم العثور على {Results.Count} نتيجة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task DeleteAsync()
        {
            if (PatientId == 0)
            {
                return;
            }

            try
            {
                await _patientService.DeleteAsync(PatientId);
                StatusMessage = "تم حذف المريض.";
                await ClearFormAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task ClearFormAsync()
        {
            PatientId = 0;
            FullName = string.Empty;
            Gender = string.Empty;
            BirthDate = null;
            Age = null;
            MobilePhone = null;
            HomePhone = null;
            NationalId = null;
            Email = null;
            IsVip = false;
            IsFasting = false;
            FastingHours = null;
            AccountType = "Individual";
            Phone = null;
            Address = null;
            ChronicDiseases = null;
            Allergies = null;
            Medications = null;
            MedicalNotes = null;
            SelectedTests.Clear();
            DiscountPercent = 0;
            PaidAmount = 0;
            PreviousPaidAmount = 0;
            CurrentVisitId = 0;
            SelectedReferral = Referrals.FirstOrDefault(r => r.ReferralId == 0);
            SelectedPatient = null;
            PatientCode = string.Empty;
            Title = "السيد";
            EntryDate = DateTime.Today;
            DeliveryDate = DateTime.Today;
            EntryTime = DateTime.Now.TimeOfDay;
            DeliveryTime = DateTime.Now.TimeOfDay;
            ReferralSource = string.Empty;
            ReferralAddress = string.Empty;
            ReferralPhone = string.Empty;
            ResponsibleName = string.Empty;
            ReferralTitle = "د./";
            PrintReferral = false;
            TestedBefore = false;
            IsAntibiotic = false;
            IsDiabetesMed = false;
            IsBloodThinner = false;
            IsVirusMed = false;
            IsBloodTransfusion = false;
            IsGlandMed = false;
            IsLiverMed = false;
            IsPregnant = false;
            IsPregnantFemale = false;
            IsDyeScan = false;
            IsSmoker = false;
            RaiseFinancialTotals();
            await GenerateLabIdAsync();
        }

        private async Task LoadFromPatientAsync(Patient patient)
        {
            PatientId = patient.PatientId;
            LabId = patient.LabId;
            FullName = patient.FullName;
            Gender = patient.Gender;
            BirthDate = patient.BirthDate;
            Age = patient.Age;
            IsVip = patient.IsVip;
            Phone = patient.Phone;
            MobilePhone = patient.Phone;
            HomePhone = patient.HomePhone;
            NationalId = patient.NationalId;
            Email = patient.Email;
            Address = patient.Address;
            SelectedReferral = FindReferral(patient.ReferralId);
            await LoadMedicalHistoryAsync(patient.PatientId);
        }

        private async Task InitializeAsync()
        {
            await LoadReferralsAsync();
            await LoadAvailableTestsAsync();
            await GenerateLabIdAsync();
        }

        private async Task LoadReferralsAsync()
        {
            Referrals.Clear();
            Referrals.Add(new Referral { ReferralId = 0, Name = "بدون عقد", ReferralType = "None" });
            if (_testCatalogService != null)
            {
                var referrals = await _testCatalogService.GetReferralsAsync();
                foreach (var referral in referrals)
                {
                    Referrals.Add(referral);
                }
            }

            SelectedReferral = Referrals.FirstOrDefault(r => r.ReferralId == 0);
        }

        private async Task LoadAvailableTestsAsync()
        {
            AvailableTests.Clear();
            if (_testCatalogService == null)
            {
                return;
            }

            var tests = await _testCatalogService.GetAllTestsAsync();
            foreach (var test in tests.OrderBy(t => t.NameReport))
            {
                AvailableTests.Add(test);
            }

            (AddAllTestsCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void AddSelectedTest()
        {
            if (SelectedAvailableTest == null)
            {
                return;
            }

            AddTest(SelectedAvailableTest);
        }

        private void AddAllVisibleTests()
        {
            foreach (var test in AvailableTests.ToList())
            {
                AddTest(test);
            }
        }

        private void AddTest(Test test)
        {
            if (SelectedTests.Any(t => t.TestId == test.TestId))
            {
                return;
            }

            SelectedTests.Add(new SelectedTestItem
            {
                TestId = test.TestId,
                TestName = string.IsNullOrWhiteSpace(test.NameReceipt) ? test.NameReport : test.NameReceipt,
                Price = test.PatientPrice ?? test.Price
            });

            RaiseFinancialTotals();
        }

        private void RemoveSelectedTest()
        {
            if (SelectedTest == null)
            {
                return;
            }

            SelectedTests.Remove(SelectedTest);
            SelectedTest = null;
            RaiseFinancialTotals();
        }

        private async Task SaveVisitAndInvoiceAsync()
        {
            if (_visitService == null || _invoiceService == null || PatientId <= 0 || SelectedTests.Count == 0)
            {
                return;
            }

            var visit = CurrentVisitId > 0
                ? await _visitService.GetByIdAsync(CurrentVisitId)
                : null;

            if (visit == null)
            {
                visit = await _visitService.CreateAsync(new Visit
                {
                    PatientId = PatientId,
                    VisitDate = DateTime.Now,
                    AccountType = string.Equals(AccountType, "Lab to Lab", StringComparison.OrdinalIgnoreCase) ? "Referral" : "Cash",
                    ReferralId = NormalizeReferralId(SelectedReferral),
                    Status = "Open"
                });
                CurrentVisitId = visit.VisitId;
            }

            var existingTests = await _visitService.GetVisitTestsAsync(visit.VisitId);
            foreach (var selected in SelectedTests)
            {
                if (existingTests.Any(t => t.TestId == selected.TestId))
                {
                    continue;
                }

                var added = await _visitService.AddTestToVisitAsync(visit.VisitId, selected.TestId, selected.Price);
                selected.VisitTestId = added.VisitTestId;
            }

            var discount = DiscountAmount;
            var invoice = await _invoiceService.CreateOrUpdateInvoiceAsync(visit.VisitId, discount, PaidAmount + PreviousPaidAmount);
            StatusMessage = $"تم حفظ الزيارة #{visit.VisitId} والفاتورة #{invoice.InvoiceId}.";
        }

        private void ShowBarcode()
        {
            if (_barcodeDialogService == null || PatientId <= 0)
            {
                StatusMessage = "خدمة الباركود غير متاحة أو لم يتم حفظ المريض بعد.";
                return;
            }

            _barcodeDialogService.ShowBarcodeDialog(
                new BarcodeDialogData
                {
                    CaseCode = CurrentVisitId > 0 ? $"CASE-{CurrentVisitId}" : $"PAT-{PatientId}",
                    FileCode = LabId,
                    LabCode = LabId,
                    PatientName = FullName,
                    SampleLabels = SelectedTests.Select(t => t.TestName).ToList()
                });
        }

        private async Task PrintReceiptAsync()
        {
            if (_printService == null || CurrentVisitId <= 0)
            {
                StatusMessage = "لا توجد زيارة محفوظة أو خدمة الطباعة غير متاحة.";
                return;
            }

            var lines = new List<string>
            {
                $"المريض: {FullName}",
                $"Lab ID: {LabId}",
                $"الزيارة: {CurrentVisitId}",
                $"الإجمالي: {TotalAmount:N2}",
                $"الخصم: {DiscountAmount:N2}",
                $"الصافي: {NetAmount:N2}",
                $"المدفوع: {(PaidAmount + PreviousPaidAmount):N2}",
                $"الباقي: {BalanceForLab:N2}"
            };

            await _printService.PrintTextReportAsync("إيصال زيارة", lines, $"Receipt_{CurrentVisitId}");
            StatusMessage = "تم إرسال الإيصال للطباعة.";
        }

        private void RaiseFinancialTotals()
        {
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(NetAmount));
            OnPropertyChanged(nameof(BalanceForLab));
            OnPropertyChanged(nameof(BalanceForPatient));
            OnPropertyChanged(nameof(LabRemainder));
            OnPropertyChanged(nameof(PatientRemainder));
        }

        private Referral? FindReferral(int? referralId)
        {
            if (!referralId.HasValue)
            {
                return Referrals.FirstOrDefault(r => r.ReferralId == 0);
            }

            return Referrals.FirstOrDefault(r => r.ReferralId == referralId.Value)
                ?? Referrals.FirstOrDefault(r => r.ReferralId == 0);
        }

        private static int? NormalizeReferralId(Referral? referral)
        {
            return referral == null || referral.ReferralId <= 0 ? null : referral.ReferralId;
        }

        private async Task LoadMedicalHistoryAsync(int patientId)
        {
            var history = await _patientService.GetMedicalHistoryAsync(patientId);
            ChronicDiseases = history?.ChronicDiseases;
            Allergies = history?.Allergies;
            Medications = history?.Medications;
            MedicalNotes = history?.Notes;
        }

        private async Task SaveMedicalHistoryAsync()
        {
            if (PatientId <= 0)
            {
                return;
            }

            await _patientService.SaveMedicalHistoryAsync(PatientId, new MedicalHistory
            {
                ChronicDiseases = ChronicDiseases,
                Allergies = Allergies,
                Medications = Medications,
                Notes = MedicalNotes
            });
        }
    }
}
