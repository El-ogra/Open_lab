using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class PatientSearchViewModel : BaseViewModel
    {
        private readonly IPatientSearchService _patientSearchService;
        private string _name = string.Empty;
        private string _phone = string.Empty;
        private string _labId = string.Empty;
        private string _nationalId = string.Empty;
        private string _ageGroup = "الكل";
        private string _statusMessage = string.Empty;
        private Patient? _selectedPatient;
        private DateTime? _date;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private bool _isAgeFree = true;
        private bool _isAgeRestricted;
        private string _ageUnit = "Years";
        private bool _isDateFree = true;
        private bool _isDateRestricted;
        private bool _isDatabaseSearch = true;
        private bool _isBackupSearch;

        public PatientSearchViewModel(IPatientSearchService patientSearchService)
        {
            _patientSearchService = patientSearchService;
            Patients = new ObservableCollection<Patient>();
            Visits = new ObservableCollection<Visit>();
            PatientTests = new ObservableCollection<string>();
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            DeletePatientCommand = new RelayCommand(async _ => await DeletePatientAsync(), _ => SelectedPatient != null);

            UnenteredResultsCommand = new RelayCommand(async _ => await LoadUnenteredResultsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsView));
            UnreviewedResultsCommand = new RelayCommand(async _ => await LoadUnreviewedResultsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsView));
            UnprintedResultsCommand = new RelayCommand(async _ => await LoadUnprintedResultsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsView));
            UndeliveredResultsCommand = new RelayCommand(async _ => await LoadUndeliveredResultsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsView));
            OpenAccountCommand = new RelayCommand(async _ => await LoadOpenAccountAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsView));
            GroupedResultsCommand = new RelayCommand(async _ => await LoadGroupedResultsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsView));

            NavigatePatientRegistrationCommand = new RelayCommand(_ => NavigateToPatientRegistration());
            NavigateResultsEntryCommand = new RelayCommand(_ => NavigateToResultsEntry());
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string LabId
        {
            get => _labId;
            set => SetProperty(ref _labId, value);
        }

        public string NationalId
        {
            get => _nationalId;
            set => SetProperty(ref _nationalId, value);
        }

        public string AgeGroup
        {
            get => _ageGroup;
            set => SetProperty(ref _ageGroup, value);
        }

        public bool IsAgeFree
        {
            get => _isAgeFree;
            set
            {
                if (SetProperty(ref _isAgeFree, value) && value)
                {
                    IsAgeRestricted = false;
                }
            }
        }

        public bool IsAgeRestricted
        {
            get => _isAgeRestricted;
            set
            {
                if (SetProperty(ref _isAgeRestricted, value) && value)
                {
                    IsAgeFree = false;
                }
            }
        }

        public string AgeUnit
        {
            get => _ageUnit;
            set => SetProperty(ref _ageUnit, value);
        }

        public DateTime? Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set => SetProperty(ref _dateFrom, value);
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set => SetProperty(ref _dateTo, value);
        }

        public bool IsDateFree
        {
            get => _isDateFree;
            set
            {
                if (SetProperty(ref _isDateFree, value) && value)
                {
                    IsDateRestricted = false;
                }
            }
        }

        public bool IsDateRestricted
        {
            get => _isDateRestricted;
            set
            {
                if (SetProperty(ref _isDateRestricted, value) && value)
                {
                    IsDateFree = false;
                }
            }
        }

        public bool IsDatabaseSearch
        {
            get => _isDatabaseSearch;
            set
            {
                if (SetProperty(ref _isDatabaseSearch, value) && value)
                {
                    IsBackupSearch = false;
                }
            }
        }

        public bool IsBackupSearch
        {
            get => _isBackupSearch;
            set
            {
                if (SetProperty(ref _isBackupSearch, value) && value)
                {
                    IsDatabaseSearch = false;
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<Patient> Patients { get; }
        public ObservableCollection<Visit> Visits { get; }
        public ObservableCollection<string> PatientTests { get; }

        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    (DeletePatientCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    _ = LoadVisitsAsync();
                }
            }
        }

        public ICommand SearchCommand { get; }
        public ICommand DeletePatientCommand { get; }
        public ICommand UnenteredResultsCommand { get; }
        public ICommand UnreviewedResultsCommand { get; }
        public ICommand UnprintedResultsCommand { get; }
        public ICommand UndeliveredResultsCommand { get; }
        public ICommand OpenAccountCommand { get; }
        public ICommand GroupedResultsCommand { get; }
        public ICommand NavigatePatientRegistrationCommand { get; }
        public ICommand NavigateResultsEntryCommand { get; }
        public Func<string, string, bool> ConfirmAction { get; set; } =
            (message, title) => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;

        private async Task SearchAsync()
        {
            try
            {
                var hasAdvancedFilters = !string.IsNullOrWhiteSpace(NationalId)
                    || DateFrom.HasValue
                    || DateTo.HasValue
                    || ParseNullableInt(AgeFrom).HasValue
                    || ParseNullableInt(AgeTo).HasValue
                    || (!string.IsNullOrWhiteSpace(AgeGroup) && !string.Equals(AgeGroup, "الكل", StringComparison.OrdinalIgnoreCase));

                var results = hasAdvancedFilters
                    ? await _patientSearchService.SearchPatientsAsync(new PatientSearchCriteria
                    {
                        Name = Name,
                        Phone = Phone,
                        LabId = LabId,
                        NationalId = NationalId,
                        Date = Date,
                        DateFrom = DateFrom,
                        DateTo = DateTo,
                        AgeFrom = ParseNullableInt(AgeFrom),
                        AgeTo = ParseNullableInt(AgeTo),
                        AgeGroup = AgeGroup
                    })
                    : await _patientSearchService.SearchPatientsAsync(Name, Phone, LabId, Date);
                Patients.Clear();
                foreach (var patient in results)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"تم العثور على {Patients.Count} مريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadVisitsAsync()
        {
            Visits.Clear();
            PatientTests.Clear();
            if (SelectedPatient == null)
            {
                return;
            }

            try
            {
                var visits = await _patientSearchService.GetPatientVisitsAsync(SelectedPatient.PatientId) ?? new List<Visit>();
                foreach (var visit in visits)
                {
                    Visits.Add(visit);
                }

                var tests = await _patientSearchService.GetPatientVisitTestsAsync(SelectedPatient.PatientId) ?? new List<VisitTest>();
                foreach (var test in tests)
                {
                    var name = string.IsNullOrWhiteSpace(test.Test?.NameReceipt)
                        ? test.Test?.NameReport
                        : test.Test.NameReceipt;
                    PatientTests.Add($"{test.Visit.VisitDate:yyyy-MM-dd} | {name ?? test.TestId.ToString()} | {test.Status}");
                }

                StatusMessage = $"تم تحميل {Visits.Count} زيارة و {PatientTests.Count} تحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        // New properties for the updated UI
        private string _mobilePhone = string.Empty;
        public string MobilePhone
        {
            get => _mobilePhone;
            set => SetProperty(ref _mobilePhone, value);
        }

        private string _ageFrom = string.Empty;
        public string AgeFrom
        {
            get => _ageFrom;
            set => SetProperty(ref _ageFrom, value);
        }

        private string _ageTo = string.Empty;
        public string AgeTo
        {
            get => _ageTo;
            set => SetProperty(ref _ageTo, value);
        }

        private string _nameExact = string.Empty;
        public string NameExact
        {
            get => _nameExact;
            set => SetProperty(ref _nameExact, value);
        }

        private string _doctorName = string.Empty;
        public string DoctorName
        {
            get => _doctorName;
            set => SetProperty(ref _doctorName, value);
        }

        private async Task DeletePatientAsync()
        {
            if (SelectedPatient == null)
            {
                StatusMessage = "حدد مريضاً قبل الحذف.";
                return;
            }

            if (!ConfirmAction($"هل تريد حذف المريض {SelectedPatient.FullName}؟", "تأكيد حذف المريض"))
            {
                StatusMessage = "تم إلغاء الحذف.";
                return;
            }

            try
            {
                var patient = SelectedPatient;
                await _patientSearchService.DeletePatientAsync(patient.PatientId);
                Patients.Remove(patient);
                Visits.Clear();
                PatientTests.Clear();
                SelectedPatient = null;
                StatusMessage = "تم حذف المريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private static int? ParseNullableInt(string value)
        {
            return int.TryParse(value, out var parsed) ? parsed : null;
        }

        private DateTime GetEffectiveDateFrom()
        {
            return DateFrom ?? DateTime.Today.AddDays(-30);
        }

        private DateTime GetEffectiveDateTo()
        {
            return DateTo ?? DateTime.Today;
        }

        private async Task LoadUnenteredResultsAsync()
        {
            try
            {
                var patients = await _patientSearchService.GetUnenteredResultsPatientsAsync(
                    GetEffectiveDateFrom().Date,
                    GetEffectiveDateTo().Date.AddDays(1).AddSeconds(-1));

                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"نتائج لم تدخل: تم العثور على {Patients.Count} مريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadUnreviewedResultsAsync()
        {
            try
            {
                var patients = await _patientSearchService.GetUnreviewedResultsPatientsAsync(
                    GetEffectiveDateFrom().Date,
                    GetEffectiveDateTo().Date.AddDays(1).AddSeconds(-1));

                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"نتائج لم تراجع: تم العثور على {Patients.Count} مريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadUnprintedResultsAsync()
        {
            try
            {
                var patients = await _patientSearchService.GetUnprintedResultsPatientsAsync(
                    GetEffectiveDateFrom().Date,
                    GetEffectiveDateTo().Date.AddDays(1).AddSeconds(-1));

                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"نتائج لم تطبع: تم العثور على {Patients.Count} مريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadUndeliveredResultsAsync()
        {
            try
            {
                var patients = await _patientSearchService.GetUndeliveredResultsPatientsAsync(
                    GetEffectiveDateFrom().Date,
                    GetEffectiveDateTo().Date.AddDays(1).AddSeconds(-1));

                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"نتائج لم تسلم: تم العثور على {Patients.Count} مريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadOpenAccountAsync()
        {
            try
            {
                var patients = await _patientSearchService.GetOpenAccountPatientsAsync(
                    GetEffectiveDateFrom().Date,
                    GetEffectiveDateTo().Date.AddDays(1).AddSeconds(-1));

                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"حساب مفتوح: تم العثور على {Patients.Count} مريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadGroupedResultsAsync()
        {
            try
            {
                var patients = await _patientSearchService.GetGroupedResultsPatientsAsync(
                    GetEffectiveDateFrom().Date,
                    GetEffectiveDateTo().Date.AddDays(1).AddSeconds(-1));

                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }

                StatusMessage = $"نتائج مجمعة: تم العثور على {Patients.Count} مريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private void NavigateToPatientRegistration()
        {
            if (SelectedPatient == null)
            {
                StatusMessage = "حدد مريضاً أولاً.";
                return;
            }

            StatusMessage = $"الانتقال لبيانات المريض: {SelectedPatient.FullName}";
            // Navigation would be handled by the main window via Messenger or similar pattern
        }

        private void NavigateToResultsEntry()
        {
            if (SelectedPatient == null)
            {
                StatusMessage = "حدد مريضاً أولاً.";
                return;
            }

            StatusMessage = $"الانتقال لنتائج التحاليل للمريض: {SelectedPatient.FullName}";
            // Navigation would be handled by the main window via Messenger or similar pattern
        }
    }
}
