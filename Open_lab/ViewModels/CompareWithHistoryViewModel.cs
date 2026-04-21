using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class CompareWithHistoryViewModel : BaseViewModel
    {
        private readonly ICompareWithHistoryService _compareService;
        private readonly IPatientService _patientService;
        private readonly ITestCatalogService _testCatalogService;
        private string _labId = string.Empty;
        private int? _selectedTestId;
        private int _historyCount = 3;
        private string _statusMessage = string.Empty;
        private int? _patientId;

        public CompareWithHistoryViewModel(ICompareWithHistoryService compareService, IPatientService patientService, ITestCatalogService testCatalogService)
        {
            _compareService = compareService;
            _patientService = patientService;
            _testCatalogService = testCatalogService;
            Tests = new ObservableCollection<TestItem>();
            HistoryResults = new ObservableCollection<HistoricalResultRow>();

            LoadPatientCommand = new RelayCommand(async _ => await LoadPatientAsync(), _ => !string.IsNullOrWhiteSpace(LabId));
            LoadHistoryCommand = new RelayCommand(async _ => await LoadHistoryAsync(), _ => PatientId.HasValue && SelectedTestId.HasValue);
            ClearCommand = new RelayCommand(_ => Clear());
        }

        public string LabId
        {
            get => _labId;
            set
            {
                if (SetProperty(ref _labId, value))
                {
                    (LoadPatientCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int? PatientId
        {
            get => _patientId;
            private set
            {
                if (SetProperty(ref _patientId, value))
                {
                    (LoadHistoryCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int? SelectedTestId
        {
            get => _selectedTestId;
            set
            {
                if (SetProperty(ref _selectedTestId, value))
                {
                    (LoadHistoryCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int HistoryCount
        {
            get => _historyCount;
            set => SetProperty(ref _historyCount, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<TestItem> Tests { get; }
        public ObservableCollection<HistoricalResultRow> HistoryResults { get; }

        public ICommand LoadPatientCommand { get; }
        public ICommand LoadHistoryCommand { get; }
        public ICommand ClearCommand { get; }

        private async Task LoadPatientAsync()
        {
            try
            {
                var patient = await _patientService.GetByLabIdAsync(LabId);
                if (patient == null)
                {
                    StatusMessage = "لم يتم العثور على المريض.";
                    PatientId = null;
                    return;
                }

                PatientId = patient.PatientId;
                Tests.Clear();
                var tests = await _testCatalogService.GetAllTestsAsync();
                foreach (var test in tests)
                {
                    Tests.Add(new TestItem { TestId = test.TestId, TestName = test.NameReport });
                }

                StatusMessage = $"تم تحميل بيانات المريض: {patient.FullName}. اختر التحليل لعرض التاريخ.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadHistoryAsync()
        {
            if (!PatientId.HasValue || !SelectedTestId.HasValue)
            {
                return;
            }

            try
            {
                var history = await _compareService.GetLastResultsAsync(PatientId.Value, SelectedTestId.Value, HistoryCount);
                HistoryResults.Clear();

                var groupedByVisit = history.GroupBy(h => new { h.VisitId, h.VisitDate });
                foreach (var visitGroup in groupedByVisit.OrderByDescending(g => g.Key.VisitDate))
                {
                    foreach (var result in visitGroup)
                    {
                        HistoryResults.Add(new HistoricalResultRow
                        {
                            VisitDate = visitGroup.Key.VisitDate,
                            VisitId = visitGroup.Key.VisitId,
                            ParameterName = result.ParameterName,
                            Value = result.Value,
                            Flag = result.Flag
                        });
                    }
                }

                StatusMessage = $"تم تحميل {HistoryResults.Count} نتيجة من {groupedByVisit.Count()} زيارات سابقة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private void Clear()
        {
            LabId = string.Empty;
            PatientId = null;
            SelectedTestId = null;
            Tests.Clear();
            HistoryResults.Clear();
            StatusMessage = string.Empty;
        }
    }

    public class TestItem
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
    }

    public class HistoricalResultRow
    {
        public DateTime VisitDate { get; set; }
        public int VisitId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Flag { get; set; }
    }
}
