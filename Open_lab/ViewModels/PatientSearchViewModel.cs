using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.ViewModels
{
    public class PatientSearchViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private string _name = string.Empty;
        private string _phone = string.Empty;
        private string _labId = string.Empty;
        private string _statusMessage = string.Empty;
        private Patient? _selectedPatient;

        public PatientSearchViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
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
                using var db = _dbFactory();
                var query = db.Patients.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(Name))
                {
                    query = query.Where(p => p.FullName.Contains(Name));
                }

                if (!string.IsNullOrWhiteSpace(Phone))
                {
                    query = query.Where(p => p.Phone != null && p.Phone.Contains(Phone));
                }

                if (!string.IsNullOrWhiteSpace(LabId))
                {
                    query = query.Where(p => p.LabId == LabId);
                }

                var results = await query.OrderBy(p => p.FullName).ToListAsync();
                Patients.Clear();
                foreach (var p in results)
                {
                    Patients.Add(p);
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
                using var db = _dbFactory();
                var visits = await db.Visits.AsNoTracking()
                    .Where(v => v.PatientId == SelectedPatient.PatientId)
                    .OrderByDescending(v => v.VisitDate)
                    .ToListAsync();

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
