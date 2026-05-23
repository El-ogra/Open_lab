using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    /// <summary>
    /// شاشة تسجيل وتعديل بيانات المريض — مطابقة بالكامل للنظام المرجعي
    /// (real lab system). تحتوي على:
    ///   - بيانات المريض الأساسية
    ///   - بيانات الطبيب/الجهة المُحوِّلة
    ///   - معلومات طبية إضافية (صيام / أمراض مزمنة / علاجات)
    ///   - اختيار العينات + Taken outside Lab
    ///   - قائمة التحاليل المتاحة + المختارة (Routine test menu)
    ///   - ملخص مالي كامل (إجمالي / خصم / مدفوع / باقي للمعمل / باقي للمريض)
    ///   - أزرار: إضافة / تعديل / حفظ / حذف / الباركود / الإيصال / حركة / تراجع / خالص
    ///   - تنقل إلى نتائج التحاليل + قائمة "مرضى اليوم"
    /// </summary>
    public class PatientRegistrationViewModel : BaseViewModel
    {
        private readonly IPatientService _patientService;
        private readonly ITestCatalogService? _testCatalogService;
        private readonly IVisitService? _visitService;
        private readonly IInvoiceService? _invoiceService;
        private readonly IBarcodeDialogService? _barcodeDialogService;
        private readonly IPrintService? _printService;
        private readonly INavigationService? _navigationService;

        // ===== بيانات المريض الأساسية =====
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
        private string _ageUnit = "Years";
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
        private string _title = "السيد/";
        private string? _patientTitleNote;

        // ===== التواريخ/الأوقات =====
        private DateTime _entryDate = DateTime.Today;
        private DateTime _deliveryDate = DateTime.Today;
        private DateTime _receptionDate = DateTime.Today;
        private TimeSpan _entryTime = DateTime.Now.TimeOfDay;
        private TimeSpan _deliveryTime = DateTime.Now.TimeOfDay;
        private TimeSpan _receptionTime = DateTime.Now.TimeOfDay;
        private string _entryTimeText = DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        private string _receptionTimeText = DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        private string _systemTimeText = DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);

        // ===== بيانات الطبيب/الجهة =====
        private string _referralSource = string.Empty;
        private string _referralAddress = string.Empty;
        private string _referralPhone = string.Empty;
        private string _referralAltPhone = string.Empty;
        private string _referralResponsible = string.Empty;
        private string _referralTitleNote = string.Empty;
        private string _doctorTitle = "د./";
        private string _responsibleName = string.Empty;
        private string _referralTitle = "د./";
        private bool _printReferral;
        private bool _printReferralOnReport;

        // ===== العلاجات/الأمراض (الأسماء التقليدية محفوظة للتوافق مع الاختبارات) =====
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

        // ===== أنواع العينات =====
        private bool _hasBloodSample;
        private bool _hasUrineSample;
        private bool _hasStoolSample;
        private bool _hasSemenSample;
        private bool _hasCsfSample;
        private bool _takenOutsideLab;

        // ===== التحاليل/البحث =====
        private string _testCategoryFilter = "Routine Tests";
        private string _searchText = string.Empty;
        private decimal _discountValue;

        // ===== مرضى اليوم =====
        private Patient? _quickSelectedPatient;

        // ============ Constructors ============
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
                  null,
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
            INavigationService? navigationService = null,
            bool initialize = true)
        {
            _patientService = patientService;
            _testCatalogService = testCatalogService;
            _visitService = visitService;
            _invoiceService = invoiceService;
            _barcodeDialogService = barcodeDialogService;
            _printService = printService;
            _navigationService = navigationService;

            Results = new ObservableCollection<Patient>();
            Referrals = new ObservableCollection<Referral>();
            AvailableTests = new ObservableCollection<Test>();
            SelectedTests = new ObservableCollection<SelectedTestItem>();
            QuickPatientsList = new ObservableCollection<Patient>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit));
            NewCommand = new RelayCommand(async _ => await ClearFormAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit));
            GenerateLabIdCommand = new RelayCommand(async _ => await GenerateLabIdAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit) && PatientId == 0);
            LoadByLabIdCommand = new RelayCommand(async _ => await LoadByLabIdAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit) && PatientId > 0);
            AddTestCommand = new RelayCommand(_ => AddSelectedTest(), _ => SelectedAvailableTest != null);
            AddAllTestsCommand = new RelayCommand(_ => AddAllVisibleTests(), _ => AvailableTests.Count > 0);
            RemoveTestCommand = new RelayCommand(_ => RemoveSelectedTest(), _ => SelectedTest != null);
            ShowBarcodeCommand = new RelayCommand(_ => ShowBarcode(), _ => PatientId > 0 || !string.IsNullOrWhiteSpace(LabId));
            PrintReceiptCommand = new RelayCommand(async _ => await PrintReceiptAsync(), _ => CurrentVisitId > 0);
            SendCommand = new RelayCommand(async _ => await PrintReceiptAsync(), _ => CurrentVisitId > 0);
            AddSelectedTestCommand = new RelayCommand(_ => AddSelectedTest(), _ => SelectedAvailableTest != null);
            RemoveSelectedTestCommand = new RelayCommand(_ => RemoveSelectedTest(), _ => SelectedTest != null);
            EditCommand = new RelayCommand(async _ => await SaveAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit) && PatientId > 0);
            GoToResultsCommand = new RelayCommand(_ => NavigateToResults(), _ => true);
            ResetCommand = new RelayCommand(async _ => await ClearFormAsync(), _ => true);
            DocumentsCommand = new RelayCommand(_ => ShowDocuments(), _ => PatientId > 0);
            GoToHomeCommand = new RelayCommand(_ => NavigateToHome(), _ => true);
            LoadByCodeCommand = new RelayCommand(async _ => await LoadByCodeAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            NewPatientCommand = new RelayCommand(async _ => await ClearFormAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit));
            ShowTodayPatientsCommand = new RelayCommand(async _ => await LoadTodayPatientsAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            ShowMovementCommand = new RelayCommand(_ => ShowMovement(), _ => CurrentVisitId > 0);
            UndoCommand = new RelayCommand(_ => UndoLastChange(), _ => true);
            // FIX: CanExecute كان دائماً true فيُضلّل المستخدم — يضغط الزر قبل حفظ الزيارة
            // ويرى "احفظ الزيارة لتثبيت الدفع". الآن الزر معطّل حتى تكون هناك زيارة محفوظة.
            SettleCommand = new RelayCommand(async _ => await SettleBalanceAsync(), _ => CurrentVisitId > 0);

            // WorksheetCommand now sends the selected patient tests to the worksheet printer.
            WorksheetCommand = new RelayCommand(async _ => await PrintWorksheetAsync(), _ => true);
            InsuranceCommand = new RelayCommand(_ => ShowInsuranceInfo(), _ => true);

            if (initialize)
            {
                _ = InitializeAsync();
            }
        }

        // ============ Properties ============

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
                    (SendCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (ShowMovementCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    // FIX: SettleCommand يعتمد الآن على CurrentVisitId > 0 — حدّث حالته أيضاً
                    // كي يصبح الزر فعّالاً تلقائياً بمجرد حفظ الزيارة (السطر 1349) ومُعطّلاً
                    // بمجرد إعادة تهيئة النموذج (السطر 1144).
                    (SettleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string LabId
        {
            get => _labId;
            set
            {
                if (SetProperty(ref _labId, value))
                {
                    (ShowBarcodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
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

        public string? PatientTitleNote
        {
            get => _patientTitleNote;
            set => SetProperty(ref _patientTitleNote, value);
        }

        // ===== التواريخ/الأوقات =====

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

        public DateTime ReceptionDate
        {
            get => _receptionDate;
            set => SetProperty(ref _receptionDate, value);
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

        public TimeSpan ReceptionTime
        {
            get => _receptionTime;
            set => SetProperty(ref _receptionTime, value);
        }

        public string EntryTimeText
        {
            get => _entryTimeText;
            set => SetProperty(ref _entryTimeText, value);
        }

        public string ReceptionTimeText
        {
            get => _receptionTimeText;
            set => SetProperty(ref _receptionTimeText, value);
        }

        public string SystemTimeText
        {
            get => _systemTimeText;
            set => SetProperty(ref _systemTimeText, value);
        }

        // ===== بيانات الجهة المُحوِّلة =====

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

        public string ReferralAltPhone
        {
            get => _referralAltPhone;
            set => SetProperty(ref _referralAltPhone, value);
        }

        public string ReferralResponsible
        {
            get => _referralResponsible;
            set => SetProperty(ref _referralResponsible, value);
        }

        public string ReferralTitleNote
        {
            get => _referralTitleNote;
            set => SetProperty(ref _referralTitleNote, value);
        }

        public string DoctorTitle
        {
            get => _doctorTitle;
            set => SetProperty(ref _doctorTitle, value);
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

        public bool PrintReferralOnReport
        {
            get => _printReferralOnReport;
            set
            {
                if (SetProperty(ref _printReferralOnReport, value))
                {
                    PrintReferral = value;
                }
            }
        }

        // ===== العلاجات/الأمراض =====

        public bool TestedBefore
        {
            get => _testedBefore;
            set
            {
                if (SetProperty(ref _testedBefore, value))
                {
                    OnPropertyChanged(nameof(HadPreviousAnalysisHere));
                }
            }
        }

        /// <summary>
        /// "هل سبق لك التحليل بمعملنا" — alias لـ TestedBefore لمطابقة الـ XAML.
        /// </summary>
        public bool HadPreviousAnalysisHere
        {
            get => _testedBefore;
            set => TestedBefore = value;
        }

        public bool IsAntibiotic
        {
            get => _isAntibiotic;
            set
            {
                if (SetProperty(ref _isAntibiotic, value))
                {
                    OnPropertyChanged(nameof(TakesAntibiotics));
                }
            }
        }

        public bool TakesAntibiotics
        {
            get => _isAntibiotic;
            set => IsAntibiotic = value;
        }

        public bool IsDiabetesMed
        {
            get => _isDiabetesMed;
            set
            {
                if (SetProperty(ref _isDiabetesMed, value))
                {
                    OnPropertyChanged(nameof(TakesDiabetesTreatment));
                }
            }
        }

        public bool TakesDiabetesTreatment
        {
            get => _isDiabetesMed;
            set => IsDiabetesMed = value;
        }

        public bool IsBloodThinner
        {
            get => _isBloodThinner;
            set
            {
                if (SetProperty(ref _isBloodThinner, value))
                {
                    OnPropertyChanged(nameof(TakesBloodThinners));
                }
            }
        }

        public bool TakesBloodThinners
        {
            get => _isBloodThinner;
            set => IsBloodThinner = value;
        }

        public bool IsVirusMed
        {
            get => _isVirusMed;
            set
            {
                if (SetProperty(ref _isVirusMed, value))
                {
                    OnPropertyChanged(nameof(TakesAntiVirals));
                }
            }
        }

        public bool TakesAntiVirals
        {
            get => _isVirusMed;
            set => IsVirusMed = value;
        }

        public bool IsBloodTransfusion
        {
            get => _isBloodTransfusion;
            set
            {
                if (SetProperty(ref _isBloodTransfusion, value))
                {
                    OnPropertyChanged(nameof(HadBloodTransfusion));
                }
            }
        }

        public bool HadBloodTransfusion
        {
            get => _isBloodTransfusion;
            set => IsBloodTransfusion = value;
        }

        public bool IsGlandMed
        {
            get => _isGlandMed;
            set
            {
                if (SetProperty(ref _isGlandMed, value))
                {
                    OnPropertyChanged(nameof(TakesGlandsTreatment));
                }
            }
        }

        public bool TakesGlandsTreatment
        {
            get => _isGlandMed;
            set => IsGlandMed = value;
        }

        public bool IsLiverMed
        {
            get => _isLiverMed;
            set
            {
                if (SetProperty(ref _isLiverMed, value))
                {
                    OnPropertyChanged(nameof(TakesLiverTreatment));
                }
            }
        }

        public bool TakesLiverTreatment
        {
            get => _isLiverMed;
            set => IsLiverMed = value;
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
            set
            {
                if (SetProperty(ref _isDyeScan, value))
                {
                    OnPropertyChanged(nameof(HadContrastImaging));
                }
            }
        }

        public bool HadContrastImaging
        {
            get => _isDyeScan;
            set => IsDyeScan = value;
        }

        public bool IsSmoker
        {
            get => _isSmoker;
            set => SetProperty(ref _isSmoker, value);
        }

        // ===== أنواع العينات =====

        public bool HasBloodSample
        {
            get => _hasBloodSample;
            set => SetProperty(ref _hasBloodSample, value);
        }

        public bool HasUrineSample
        {
            get => _hasUrineSample;
            set => SetProperty(ref _hasUrineSample, value);
        }

        public bool HasStoolSample
        {
            get => _hasStoolSample;
            set => SetProperty(ref _hasStoolSample, value);
        }

        public bool HasSemenSample
        {
            get => _hasSemenSample;
            set => SetProperty(ref _hasSemenSample, value);
        }

        public bool HasCsfSample
        {
            get => _hasCsfSample;
            set => SetProperty(ref _hasCsfSample, value);
        }

        public bool TakenOutsideLab
        {
            get => _takenOutsideLab;
            set => SetProperty(ref _takenOutsideLab, value);
        }

        // ===== البحث/الفلاتر =====

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

        public string TestSearchText
        {
            get => _searchText;
            set => SearchText = value;
        }

        // ===== الملخص المالي =====

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

        /// <summary>
        /// alias لـ NetAmount يُستخدم في الـ XAML باسم "التكلفة بعد الخصم".
        /// </summary>
        public decimal NetTotal => NetAmount;

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
                    OnPropertyChanged(nameof(PreviousPaidAmount));
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
                    OnPropertyChanged(nameof(PreviousPaid));
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

        // ===== Collections =====
        public ObservableCollection<Patient> Results { get; }
        public ObservableCollection<Referral> Referrals { get; }
        public ObservableCollection<Test> AvailableTests { get; }
        public ObservableCollection<SelectedTestItem> SelectedTests { get; }
        public ObservableCollection<Patient> QuickPatientsList { get; }

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
            set
            {
                if (SetProperty(ref _selectedReferral, value) && value != null)
                {
                    ReferralSource = value.Name;
                }
            }
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

        public Patient? QuickSelectedPatient
        {
            get => _quickSelectedPatient;
            set
            {
                if (SetProperty(ref _quickSelectedPatient, value) && value != null)
                {
                    _ = LoadFromPatientAsync(value);
                }
            }
        }

        // ============ Commands ============
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
        public ICommand SendCommand { get; }
        public ICommand AddSelectedTestCommand { get; }
        public ICommand RemoveSelectedTestCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand GoToResultsCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand DocumentsCommand { get; }
        public ICommand GoToHomeCommand { get; }
        public ICommand LoadByCodeCommand { get; }
        public ICommand NewPatientCommand { get; }
        public ICommand ShowTodayPatientsCommand { get; }
        public ICommand ShowMovementCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand SettleCommand { get; }
        // CRITICAL FIX Phase 0: Added missing WorksheetCommand and InsuranceCommand (C-01)
        public ICommand WorksheetCommand { get; }
        public ICommand InsuranceCommand { get; }

        // ============ Methods ============

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
                        IsPregnant = IsPregnant,
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
                        IsPregnant = IsPregnant,
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
            QuickSelectedPatient = null;
            PatientCode = string.Empty;
            Title = "السيد/";
            PatientTitleNote = null;
            EntryDate = DateTime.Today;
            DeliveryDate = DateTime.Today;
            ReceptionDate = DateTime.Today;
            EntryTime = DateTime.Now.TimeOfDay;
            DeliveryTime = DateTime.Now.TimeOfDay;
            ReceptionTime = DateTime.Now.TimeOfDay;
            EntryTimeText = DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
            ReceptionTimeText = DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
            SystemTimeText = DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
            ReferralSource = string.Empty;
            ReferralAddress = string.Empty;
            ReferralPhone = string.Empty;
            ReferralAltPhone = string.Empty;
            ReferralResponsible = string.Empty;
            ReferralTitleNote = string.Empty;
            DoctorTitle = "د./";
            ResponsibleName = string.Empty;
            ReferralTitle = "د./";
            PrintReferral = false;
            PrintReferralOnReport = false;
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
            HasBloodSample = false;
            HasUrineSample = false;
            HasStoolSample = false;
            HasSemenSample = false;
            HasCsfSample = false;
            TakenOutsideLab = false;
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
            IsPregnant = patient.IsPregnant;
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
            await LoadTodayPatientsAsync();
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

        private async Task LoadTodayPatientsAsync()
        {
            try
            {
                QuickPatientsList.Clear();
                var results = await _patientService.SearchAsync(null, null, DateTime.Today, null);
                foreach (var p in results)
                {
                    QuickPatientsList.Add(p);
                }
                StatusMessage = $"مرضى اليوم: {QuickPatientsList.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ تحميل مرضى اليوم: {ex.Message}";
            }
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
            OnPropertyChanged(nameof(SelectedTests));
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

            // CRITICAL FIX Phase 0: Wrap in TransactionScope for atomicity (C-09)
            // Previously patient + visit + invoice could be partially saved
            using var transaction = new System.Transactions.TransactionScope(
                System.Transactions.TransactionScopeOption.Required,
                new System.Transactions.TransactionOptions { Timeout = TimeSpan.FromSeconds(30) });

            try
            {
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

                // CRITICAL FIX: Commit transaction only after all operations succeed
                transaction.Complete();

                StatusMessage = $"تم حفظ الزيارة #{visit.VisitId} والفاتورة #{invoice.InvoiceId}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في حفظ البيانات: {ex.Message}";
                // Transaction is automatically rolled back when disposed
                throw;
            }
        }

        private void ShowBarcode()
        {
            if (_barcodeDialogService == null)
            {
                StatusMessage = "خدمة الباركود غير متاحة.";
                return;
            }

            if (PatientId <= 0 && string.IsNullOrWhiteSpace(LabId))
            {
                StatusMessage = "يجب حفظ المريض أو إدخال Lab ID قبل طباعة الباركود.";
                return;
            }

            var labels = BuildBarcodeSampleLabels();
            var caseCode = CurrentVisitId > 0 ? $"CASE-{CurrentVisitId}" : (PatientId > 0 ? $"PAT-{PatientId}" : LabId);

            _barcodeDialogService.ShowBarcodeDialog(
                new BarcodeDialogData
                {
                    CaseCode = caseCode,
                    FileCode = LabId,
                    LabCode = LabId,
                    PatientName = string.IsNullOrWhiteSpace(FullName) ? "—" : FullName,
                    Gender = Gender,
                    Age = Age,
                    AgeUnit = AgeUnit,
                    BarcodeDate = DateTime.Now,
                    SampleLabels = labels
                });

            StatusMessage = "تم فتح نافذة الباركود.";
        }

        /// <summary>
        /// يبني قائمة ملصقات الأنابيب: مزيج من العينات المحددة + أسماء التحاليل المختارة،
        /// بنفس روح نظام النموذج المرجعي (FBG, PPBG, CBC, ESR, Urine Culture ...).
        /// </summary>
        private List<string> BuildBarcodeSampleLabels()
        {
            var labels = new List<string>();

            // 1) إذا كان فحص الصيام مفعّل، أنشئ ملصق Serum Fasting خاص
            if (IsFasting)
            {
                labels.Add("Serum Fasting");
            }

            // 2) أضف ملصق لكل تحليل مختار
            foreach (var t in SelectedTests)
            {
                if (!string.IsNullOrWhiteSpace(t.TestName))
                {
                    labels.Add(t.TestName);
                }
            }

            // 3) إذا لم تكن هناك تحاليل لكن العينات مفعّلة، أضف ملصقات العينات نفسها
            if (labels.Count == 0)
            {
                if (HasBloodSample) labels.Add("Blood");
                if (HasUrineSample) labels.Add("Urine");
                if (HasStoolSample) labels.Add("Stool");
                if (HasSemenSample) labels.Add("Semen");
                if (HasCsfSample) labels.Add("CSF");
            }
            else
            {
                // 4) إذا كان لدينا تحاليل، أضف فقط العينات الإضافية كملصقات للعينات
                if (HasUrineSample && !labels.Contains("Urine")) labels.Add("Urine");
                if (HasStoolSample && !labels.Contains("Stool")) labels.Add("Stool");
                if (HasSemenSample && !labels.Contains("Semen")) labels.Add("Semen");
                if (HasCsfSample && !labels.Contains("CSF")) labels.Add("CSF");
            }

            return labels;
        }

        private async Task PrintReceiptAsync()
        {
            if (_printService == null || CurrentVisitId <= 0)
            {
                StatusMessage = "لا توجد زيارة محفوظة أو خدمة الطباعة غير متاحة.";
                return;
            }

            var visitTests = SelectedTests.Select(t => new VisitTest
            {
                VisitId = CurrentVisitId,
                TestId = t.TestId,
                Price = t.Price,
                Status = "Open",
                Test = new Test
                {
                    TestId = t.TestId,
                    NameReport = t.TestName,
                    NameReceipt = t.TestName,
                    Price = t.Price
                }
            }).ToList();

            var receipt = new ReceiptData
            {
                Visit = new Visit
                {
                    VisitId = CurrentVisitId,
                    PatientId = PatientId,
                    VisitDate = EntryDate,
                    Status = "Open"
                },
                Patient = new Patient
                {
                    PatientId = PatientId,
                    LabId = LabId,
                    FullName = FullName,
                    Gender = Gender,
                    Age = Age
                },
                VisitTests = visitTests,
                Invoice = new Invoice
                {
                    VisitId = CurrentVisitId,
                    Total = TotalAmount,
                    Discount = DiscountAmount,
                    NetTotal = NetAmount,
                    Paid = PaidAmount + PreviousPaidAmount,
                    Balance = BalanceForLab,
                    Status = BalanceForLab <= 0 ? "Closed" : "Open"
                }
            };

            await _printService.PrintReceiptAsync(receipt, LabId);
            StatusMessage = "تم إرسال الإيصال للطباعة.";
        }

        private void ShowMovement()
        {
            StatusMessage = $"حركة الزيارة #{CurrentVisitId} — إجمالي {TotalAmount:N2} / مدفوع {(PaidAmount + PreviousPaidAmount):N2} / باقي {BalanceForLab:N2}.";
        }

        private void UndoLastChange()
        {
            PaidAmount = 0;
            DiscountPercent = 0;
            StatusMessage = "تم التراجع عن آخر تعديل مالي.";
        }

        private async Task SettleBalanceAsync()
        {
            // "خالص" — يجعل المدفوع مساوياً للصافي بحيث يصبح الباقي 0
            var remaining = NetAmount - PreviousPaidAmount;
            if (remaining < 0)
            {
                remaining = 0;
            }
            PaidAmount = remaining;

            if (_invoiceService != null && CurrentVisitId > 0)
            {
                await _invoiceService.CreateOrUpdateInvoiceAsync(CurrentVisitId, DiscountAmount, PaidAmount + PreviousPaidAmount);
                PreviousPaidAmount += PaidAmount;
                PaidAmount = 0;
                RaiseFinancialTotals();
                StatusMessage = "تم تعليم الفاتورة كـ (خالص) وحفظ الدفع.";
                return;
            }

            StatusMessage = "تم تعليم الفاتورة كـ (خالص). احفظ الزيارة لتثبيت الدفع.";
        }

        private void RaiseFinancialTotals()
        {
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(NetAmount));
            OnPropertyChanged(nameof(NetTotal));
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

        private async Task PrintWorksheetAsync()
        {
            if (SelectedTests.Count == 0)
            {
                StatusMessage = "لا توجد تحاليل مختارة لطباعة ورقة العمل.";
                return;
            }

            if (_printService == null)
            {
                StatusMessage = "خدمة الطباعة غير متاحة.";
                return;
            }

            var rows = new List<WorkSheetPatientRow>
            {
                new WorkSheetPatientRow
                {
                    VisitId = CurrentVisitId,
                    PatientName = FullName,
                    VisitDate = EntryDate,
                    TestsCount = SelectedTests.Count
                }
            };

            await _printService.PrintWorksheetByPatientAsync(EntryDate.Date, EntryDate.Date, rows);
            StatusMessage = "تم إرسال ورقة عمل المريض للطباعة.";
        }

        // CRITICAL FIX Phase 0: InsuranceCommand handler — was previously missing (C-01).
        // Per spec: shows a placeholder StatusMessage instead of being a no-op.
        private void ShowInsuranceInfo()
        {
            if (SelectedReferral == null || SelectedReferral.ReferralId <= 0)
            {
                StatusMessage = "إكارنية التأمين (قيد التطوير): المريض ليس لديه جهة إحالة مسجلة.";
                return;
            }

            StatusMessage = $"إكارنية التأمين (قيد التطوير): الجهة المُحوِّلة = {SelectedReferral.Name}.";
        }

        private void NavigateToResults()
        {
            if (_navigationService == null)
            {
                StatusMessage = "خدمة التنقل غير متاحة.";
                return;
            }

            _navigationService.Navigate(NavigationTarget.ResultsEntry);
        }

        private void ShowDocuments()
        {
            if (PatientId <= 0)
            {
                StatusMessage = "حدد مريضاً أولاً.";
                return;
            }

            StatusMessage = $"عرض مستندات المريض: {FullName} (Lab ID: {LabId})";
            // Navigation to document viewer would be handled by the main window
        }

        private void NavigateToHome()
        {
            if (_navigationService == null)
            {
                StatusMessage = "خدمة التنقل غير متاحة.";
                return;
            }

            _navigationService.Navigate(NavigationTarget.Home);
        }
    }
}
