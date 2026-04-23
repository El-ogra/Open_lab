using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
        private string _statusMessage = string.Empty;
        private Patient? _selectedPatient;
        private DateTime? _date;

        public PatientSearchViewModel(IPatientSearchService patientSearchService)
        {
            _patientSearchService = patientSearchService;
            Patients = new ObservableCollection<Patient>();
            Visits = new ObservableCollection<Visit>();
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
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

        public DateTime? Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<Patient> Patients { get; }
        public ObservableCollection<Visit> Visits { get; }

        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    _ = LoadVisitsAsync();
                }
            }
        }

        public ICommand SearchCommand { get; }

        private async Task SearchAsync()
        {
            try
            {
                var results = await _patientSearchService.SearchPatientsAsync(Name, Phone, LabId, Date);
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
            if (SelectedPatient == null)
            {
                return;
            }

            try
            {
                var visits = await _patientSearchService.GetPatientVisitsAsync(SelectedPatient.PatientId);
                foreach (var visit in visits)
                {
                    Visits.Add(visit);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
