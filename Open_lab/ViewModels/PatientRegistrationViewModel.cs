using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class PatientRegistrationViewModel : BaseViewModel
    {
        private readonly IPatientService _patientService;
        private string _labId = string.Empty;
        private string _fullName = string.Empty;
        private string _gender = string.Empty;
        private DateTime? _birthDate;
        private string? _phone;
        private string? _address;
        private int _patientId;
        private string _statusMessage = string.Empty;
        private Patient? _selectedPatient;

        public PatientRegistrationViewModel(IPatientService patientService)
        {
            _patientService = patientService;
            Results = new ObservableCollection<Patient>();
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit));
            NewCommand = new RelayCommand(async _ => await ClearFormAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit));
            GenerateLabIdCommand = new RelayCommand(async _ => await GenerateLabIdAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit) && PatientId == 0);
            LoadByLabIdCommand = new RelayCommand(async _ => await LoadByLabIdAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsView));
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => AppSession.HasPermission(PermissionCodes.PatientsEdit) && PatientId > 0);

            _ = GenerateLabIdAsync();
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
                }
            }
        }

        public string LabId
        {
            get => _labId;
            set => SetProperty(ref _labId, value);
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

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<Patient> Results { get; }

        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value) && value != null)
                {
                    LoadFromPatient(value);
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand GenerateLabIdCommand { get; }
        public ICommand LoadByLabIdCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand DeleteCommand { get; }

        private async Task SaveAsync()
        {
            try
            {
                if (PatientId == 0)
                {
                    var created = await _patientService.CreateAsync(new Patient
                    {
                        LabId = LabId,
                        FullName = FullName,
                        Gender = Gender,
                        BirthDate = BirthDate,
                        Phone = Phone,
                        Address = Address
                    });

                    PatientId = created.PatientId;
                    LabId = created.LabId;
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
                        Phone = Phone,
                        Address = Address
                    });

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

                LoadFromPatient(patient);
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
            Phone = null;
            Address = null;
            SelectedPatient = null;
            await GenerateLabIdAsync();
        }

        private void LoadFromPatient(Patient patient)
        {
            PatientId = patient.PatientId;
            LabId = patient.LabId;
            FullName = patient.FullName;
            Gender = patient.Gender;
            BirthDate = patient.BirthDate;
            Phone = patient.Phone;
            Address = patient.Address;
        }
    }
}
