using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ResultsEntryViewModel : BaseViewModel
    {
        private readonly IResultsService _resultsService;
        private readonly IPatientService? _patientService;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private VisitTestRow? _selectedVisitTest;
        private string _statusMessage = string.Empty;
        private string _medicalHistorySummary = string.Empty;
        private bool _hasMedicalAlerts;

        public ResultsEntryViewModel(IResultsService resultsService)
            : this(resultsService, null)
        {
        }

        public ResultsEntryViewModel(IResultsService resultsService, IPatientService? patientService)
        {
            _resultsService = resultsService;
            _patientService = patientService;
            VisitTests = new ObservableCollection<VisitTestRow>();
            ResultItems = new ObservableCollection<ResultEntryItem>();

            LoadVisitTestsCommand = new RelayCommand(async _ => await LoadVisitTestsAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsView));
            SaveResultsCommand = new RelayCommand(async _ => await SaveResultsAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null && !IsSelectedVerified());
            VerifyResultsCommand = new RelayCommand(async _ => await VerifyResultsAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null);
            ReopenResultsCommand = new RelayCommand(async _ => await ReopenResultsAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null && IsSelectedVerified());
        }

        public DateTime DateFrom
        {
            get => _dateFrom;
            set => SetProperty(ref _dateFrom, value);
        }

        public DateTime DateTo
        {
            get => _dateTo;
            set => SetProperty(ref _dateTo, value);
        }

        public ObservableCollection<VisitTestRow> VisitTests { get; }
        public ObservableCollection<ResultEntryItem> ResultItems { get; }

        public VisitTestRow? SelectedVisitTest
        {
            get => _selectedVisitTest;
            set
            {
                if (SetProperty(ref _selectedVisitTest, value))
                {
                    RaiseCommandStates();
                    _ = LoadResultsAsync();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public string MedicalHistorySummary
        {
            get => _medicalHistorySummary;
            private set => SetProperty(ref _medicalHistorySummary, value);
        }

        public bool HasMedicalAlerts
        {
            get => _hasMedicalAlerts;
            private set => SetProperty(ref _hasMedicalAlerts, value);
        }

        public ICommand LoadVisitTestsCommand { get; }
        public ICommand SaveResultsCommand { get; }
        public ICommand VerifyResultsCommand { get; }
        public ICommand ReopenResultsCommand { get; }

        private async Task LoadVisitTestsAsync()
        {
            await LoadVisitTestsCoreAsync(true);
        }

        private async Task LoadVisitTestsCoreAsync(bool updateStatus)
        {
            try
            {
                var visitTests = await _resultsService.GetVisitTestsByDateAsync(DateFrom, DateTo.AddDays(1).AddSeconds(-1));

                VisitTests.Clear();
                foreach (var vt in visitTests)
                {
                    VisitTests.Add(new VisitTestRow
                    {
                        VisitTestId = vt.VisitTestId,
                        PatientId = vt.Visit?.PatientId ?? 0,
                        TestId = vt.TestId,
                        PatientName = vt.Visit?.Patient?.FullName ?? string.Empty,
                        PatientGender = vt.Visit?.Patient?.Gender ?? string.Empty,
                        PatientAge = vt.Visit?.Patient?.Age ?? 0,
                        TestName = vt.Test?.NameReport ?? string.Empty,
                        VisitDate = vt.Visit?.VisitDate ?? DateTime.MinValue,
                        Status = vt.Status
                    });
                }

                if (updateStatus)
                {
                    StatusMessage = $"تم تحميل {VisitTests.Count} تحليل.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadResultsAsync()
        {
            ResultItems.Clear();
            MedicalHistorySummary = string.Empty;
            HasMedicalAlerts = false;
            if (SelectedVisitTest == null)
            {
                return;
            }

            try
            {
                var parameters = await _resultsService.GetParametersForTestAsync(SelectedVisitTest.TestId);
                var results = await _resultsService.GetResultsForVisitTestAsync(SelectedVisitTest.VisitTestId);
                await LoadMedicalHistoryForSelectedPatientAsync();

                foreach (var param in parameters)
                {
                    var existing = results.FirstOrDefault(r => r.ParameterId == param.ParameterId);
                    var item = new ResultEntryItem
                    {
                        ParameterId = param.ParameterId,
                        ParameterName = param.Name,
                        Value = existing?.Value,
                        Flag = existing?.Flag,
                        Comment = existing?.Comment,
                        OnValueChanged = async i => await AutoValidateResultAsync(i)
                    };
                    ResultItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task AutoValidateResultAsync(ResultEntryItem item)
        {
            if (SelectedVisitTest == null || string.IsNullOrWhiteSpace(item.Value))
            {
                return;
            }

            try
            {
                var validation = await _resultsService.ValidateResultAsync(
                    SelectedVisitTest.TestId,
                    item.Value,
                    SelectedVisitTest.PatientGender,
                    SelectedVisitTest.PatientAge);

                item.Flag = validation.Flag;
                item.Comment = validation.RecommendedComment;
            }
            catch
            {
                StatusMessage = "تعذر تطبيق التحقق التلقائي للنتيجة.";
            }
        }

        private async Task SaveResultsAsync()
        {
            if (SelectedVisitTest == null)
            {
                StatusMessage = "لم يتم تحديد اختبار لحفظ نتائجه.";
                return;
            }

            try
            {
                foreach (var item in ResultItems)
                {
                    await _resultsService.SaveResultAsync(
                        SelectedVisitTest.VisitTestId,
                        item.ParameterId,
                        item.Value,
                        item.Flag,
                        item.Comment,
                        AppSession.UserId > 0 ? AppSession.UserId : 1);
                }

                SelectedVisitTest.Status = "InProgress";
                RaiseCommandStates();
                StatusMessage = "تم حفظ النتائج.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task VerifyResultsAsync()
        {
            if (SelectedVisitTest == null)
            {
                StatusMessage = "لم يتم تحديد اختبار لاعتماده.";
                return;
            }

            try
            {
                await _resultsService.VerifyVisitTestAsync(SelectedVisitTest.VisitTestId, AppSession.UserId > 0 ? AppSession.UserId : 1);
                SelectedVisitTest.Status = "Verified";
                RaiseCommandStates();
                StatusMessage = "تم اعتماد النتائج وقفلها.";
                await LoadVisitTestsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task ReopenResultsAsync()
        {
            if (SelectedVisitTest == null)
            {
                StatusMessage = "لم يتم تحديد اختبار لإعادة فتحه.";
                return;
            }

            try
            {
                await _resultsService.ReopenVisitTestAsync(SelectedVisitTest.VisitTestId);
                SelectedVisitTest.Status = "InProgress";
                RaiseCommandStates();
                StatusMessage = "تم إعادة فتح النتائج للتعديل.";
                await LoadVisitTestsCoreAsync(false);
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private bool IsSelectedVerified()
        {
            return string.Equals(SelectedVisitTest?.Status, "Verified", StringComparison.OrdinalIgnoreCase);
        }

        private void RaiseCommandStates()
        {
            (SaveResultsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (VerifyResultsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ReopenResultsCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task LoadMedicalHistoryForSelectedPatientAsync()
        {
            if (_patientService == null || SelectedVisitTest == null || SelectedVisitTest.PatientId <= 0)
            {
                return;
            }

            var history = await _patientService.GetMedicalHistoryAsync(SelectedVisitTest.PatientId);
            if (history == null)
            {
                return;
            }

            var parts = new[]
            {
                string.IsNullOrWhiteSpace(history.ChronicDiseases) ? null : $"الأمراض المزمنة: {history.ChronicDiseases}",
                string.IsNullOrWhiteSpace(history.Allergies) ? null : $"الحساسيات: {history.Allergies}",
                string.IsNullOrWhiteSpace(history.Medications) ? null : $"الأدوية الحالية: {history.Medications}",
                string.IsNullOrWhiteSpace(history.Notes) ? null : $"ملاحظات: {history.Notes}"
            }.Where(p => p != null);

            MedicalHistorySummary = string.Join(Environment.NewLine, parts);
            HasMedicalAlerts = !string.IsNullOrWhiteSpace(MedicalHistorySummary);
        }
    }
}
