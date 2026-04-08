using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class PatientTestsSelectionViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private string _labId = string.Empty;
        private string _patientName = string.Empty;
        private int _patientId;
        private int _visitId;
        private string _statusMessage = string.Empty;
        private Test? _selectedAvailableTest;
        private SelectedTestItem? _selectedVisitTest;

        public PatientTestsSelectionViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            AvailableTests = new ObservableCollection<Test>();
            SelectedTests = new ObservableCollection<SelectedTestItem>();

            LoadPatientCommand = new RelayCommand(async _ => await LoadPatientAsync());
            CreateVisitCommand = new RelayCommand(async _ => await CreateVisitAsync(), _ => PatientId > 0);
            AddTestCommand = new RelayCommand(async _ => await AddTestAsync(), _ => VisitId > 0 && SelectedAvailableTest != null);
            RemoveTestCommand = new RelayCommand(async _ => await RemoveTestAsync(), _ => SelectedVisitTest != null);
            RefreshTestsCommand = new RelayCommand(async _ => await LoadAvailableTestsAsync());

            _ = LoadAvailableTestsAsync();
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
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

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
        public ICommand RemoveTestCommand { get; }
        public ICommand RefreshTestsCommand { get; }

        private async Task LoadPatientAsync()
        {
            if (string.IsNullOrWhiteSpace(LabId))
            {
                StatusMessage = "يرجى إدخال Lab ID.";
                return;
            }

            try
            {
                using var db = _dbFactory();
                var patientService = new PatientService(db);
                var patient = await patientService.GetByLabIdAsync(LabId);
                if (patient == null)
                {
                    StatusMessage = "لم يتم العثور على المريض.";
                    return;
                }

                PatientId = patient.PatientId;
                PatientName = patient.FullName;
                VisitId = 0;
                SelectedTests.Clear();
                StatusMessage = "تم تحميل بيانات المريض.";
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
                using var db = _dbFactory();
                var visitService = new VisitService(db);
                var visit = await visitService.CreateAsync(new Visit
                {
                    PatientId = PatientId,
                    VisitDate = DateTime.Now,
                    Status = "Open"
                });

                VisitId = visit.VisitId;
                SelectedTests.Clear();
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
                using var db = _dbFactory();
                var catalog = new TestCatalogService(db);
                var tests = await catalog.GetAllTestsAsync();
                AvailableTests.Clear();
                foreach (var test in tests)
                {
                    AvailableTests.Add(test);
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
                using var db = _dbFactory();
                var visitService = new VisitService(db);
                var visitTest = await visitService.AddTestToVisitAsync(VisitId, SelectedAvailableTest.TestId);
                SelectedTests.Add(new SelectedTestItem
                {
                    VisitTestId = visitTest.VisitTestId,
                    TestId = SelectedAvailableTest.TestId,
                    TestName = SelectedAvailableTest.NameReport,
                    Price = visitTest.Price
                });
                StatusMessage = "تمت إضافة التحليل.";
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
                using var db = _dbFactory();
                var visitService = new VisitService(db);
                await visitService.RemoveVisitTestAsync(SelectedVisitTest.VisitTestId);
                SelectedTests.Remove(SelectedVisitTest);
                StatusMessage = "تم حذف التحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
