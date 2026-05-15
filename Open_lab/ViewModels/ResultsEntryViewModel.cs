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
        private readonly IVisitService? _visitService;

        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private string _statusMessage = string.Empty;
        private string _medicalHistorySummary = string.Empty;
        private string _visitNotes = string.Empty;
        private bool _hasMedicalAlerts;
        
        private int _todayPatientsCount;
        private string _patientQuickSearch = string.Empty;
        private VisitSummary? _selectedVisit;
        private VisitSummary? _selectedVisitDetail;

        public ResultsEntryViewModel(IResultsService resultsService)
            : this(resultsService, null, null)
        {
        }

        public ResultsEntryViewModel(IResultsService resultsService, IPatientService? patientService)
            : this(resultsService, patientService, null)
        {
        }

        public ResultsEntryViewModel(IResultsService resultsService, IPatientService? patientService, IVisitService? visitService)
        {
            _resultsService = resultsService;
            _patientService = patientService;
            _visitService = visitService;

            TodayPatients = new ObservableCollection<VisitSummary>();
            VisitTests = new ObservableCollection<VisitTestRow>();
            Visits = new ObservableCollection<VisitSummary>();
            ResultItems = new ObservableCollection<ResultEntryItem>();

            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
            SearchByDateCommand = new RelayCommand(async _ => await SearchByDateAsync());
            SelectPatientCommand = new RelayCommand(async _ => await SelectPatientAsync());
            LoadVisitTestsCommand = new RelayCommand(async _ => await LoadVisitTestsAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsView));
            SaveResultsCommand = new RelayCommand(async _ => await SaveResultsAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null && !IsSelectedVerified());
            VerifyResultsCommand = new RelayCommand(async _ => await VerifyResultsAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null);
            ReopenResultsCommand = new RelayCommand(async _ => await ReopenResultsAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null && IsSelectedVerified());

            MarkFinishedAllCommand = new RelayCommand(async _ => await MarkFinishedAllAsync());
            MarkVerifiedAllCommand = new RelayCommand(async _ => await MarkVerifiedAllAsync());
            MarkPrintedAllCommand = new RelayCommand(async _ => await MarkPrintedAllAsync());

            ShowMedicalHistoryCommand = new RelayCommand(async _ => await LoadMedicalHistoryAsync());
            PrintGroupReportCommand = new RelayCommand(_ => { StatusMessage = "Printing Group Report..."; });
            PrintWorksheetCommand = new RelayCommand(_ => { StatusMessage = "Printing Worksheet..."; });
            PrintMethodsCommand = new RelayCommand(_ => { StatusMessage = "Printing Methods..."; });
            PrintBlankReportCommand = new RelayCommand(_ => { StatusMessage = "Printing Blank Report..."; });
            GoToPatientDataCommand = new RelayCommand(_ => { StatusMessage = "Navigating to Patient Data..."; });

            FilterVipCommand = new RelayCommand(_ => { StatusMessage = "Filtering VIP..."; });
            FilterAllCommand = new RelayCommand(_ => { StatusMessage = "Filtering All..."; });
            FilterLabCommand = new RelayCommand(_ => { StatusMessage = "Filtering Lab To..."; });
            
            ShowPreviousResultCommand = new RelayCommand(_ => { StatusMessage = "Showing Previous Results..."; });
            ShowNormalRangeCommand = new RelayCommand(_ => { StatusMessage = "Showing Normal Range..."; });
        }

        private async Task InitializeAsync()
        {
            await RefreshAsync();
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

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public string MedicalHistorySummary
        {
            get => _medicalHistorySummary;
            set => SetProperty(ref _medicalHistorySummary, value);
        }

        public string VisitNotes
        {
            get => _visitNotes;
            set => SetProperty(ref _visitNotes, value);
        }

        public bool HasMedicalAlerts
        {
            get => _hasMedicalAlerts;
            private set => SetProperty(ref _hasMedicalAlerts, value);
        }

        public int TodayPatientsCount
        {
            get => _todayPatientsCount;
            private set => SetProperty(ref _todayPatientsCount, value);
        }

        public string PatientQuickSearch
        {
            get => _patientQuickSearch;
            set => SetProperty(ref _patientQuickSearch, value);
        }

        public VisitSummary? SelectedVisit
        {
            get => _selectedVisit;
            set
            {
                if (SetProperty(ref _selectedVisit, value))
                {
                    _ = SelectPatientAsync();
                }
            }
        }

        private VisitTestRow? _selectedVisitTest;

        public VisitSummary? SelectedVisitDetail
        {
            get => _selectedVisitDetail;
            set
            {
                if (SetProperty(ref _selectedVisitDetail, value))
                {
                    _ = LoadVisitTestsAsync();
                }
            }
        }

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

        public ObservableCollection<VisitSummary> TodayPatients { get; }
        public ObservableCollection<VisitTestRow> VisitTests { get; }
        public ObservableCollection<VisitSummary> Visits { get; }
        public ObservableCollection<ResultEntryItem> ResultItems { get; }

        public ICommand RefreshCommand { get; }
        public ICommand SearchByDateCommand { get; }
        public ICommand SelectPatientCommand { get; }
        public ICommand LoadVisitTestsCommand { get; }
        public ICommand SaveResultsCommand { get; }
        public ICommand VerifyResultsCommand { get; }
        public ICommand ReopenResultsCommand { get; }
        
        public ICommand MarkFinishedAllCommand { get; }
        public ICommand MarkVerifiedAllCommand { get; }
        public ICommand MarkPrintedAllCommand { get; }
        
        public ICommand ShowMedicalHistoryCommand { get; }
        public ICommand PrintGroupReportCommand { get; }
        public ICommand PrintWorksheetCommand { get; }
        public ICommand PrintMethodsCommand { get; }
        public ICommand PrintBlankReportCommand { get; }
        public ICommand GoToPatientDataCommand { get; }
        
        public ICommand FilterVipCommand { get; }
        public ICommand FilterAllCommand { get; }
        public ICommand FilterLabCommand { get; }

        public ICommand ShowPreviousResultCommand { get; }
        public ICommand ShowNormalRangeCommand { get; }

        private async Task RefreshAsync()
        {
            await SearchByDateAsync();
        }

        private async Task SearchByDateAsync()
        {
            try
            {
                var visitTests = await _resultsService.GetVisitTestsByDateAsync(DateFrom, DateTo.AddDays(1).AddSeconds(-1));
                if (visitTests == null) return;
                
                var uniqueVisits = visitTests
                    .Where(vt => vt.Visit != null)
                    .Select(vt => vt.Visit)
                    .GroupBy(v => v!.VisitId)
                    .Select(g => g.First())
                    .Select(v => new VisitSummary
                    {
                        VisitId = v!.VisitId,
                        PatientId = v.PatientId,
                        PatientCode = v.Patient?.LabId ?? string.Empty,
                        PatientName = v.Patient?.FullName ?? string.Empty,
                        Gender = v.Patient?.Gender ?? string.Empty,
                        Age = v.Patient?.Age ?? 0,
                        ReferralSource = v.Referral?.Name ?? string.Empty,
                        LabId = v.Patient?.LabId ?? string.Empty,
                        VisitDate = v.VisitDate
                    })
                    .ToList();

                TodayPatients.Clear();
                foreach (var visit in uniqueVisits)
                {
                    TodayPatients.Add(visit);
                }
                TodayPatientsCount = TodayPatients.Count;
                StatusMessage = $"تم تحميل {TodayPatientsCount} مريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task SelectPatientAsync()
        {
            if (SelectedVisit == null) return;
            
            Visits.Clear();
            Visits.Add(SelectedVisit);
            SelectedVisitDetail = SelectedVisit;
            
            await LoadMedicalHistoryAsync();
        }

        private async Task LoadVisitTestsAsync()
        {
            try
            {
                var results = await _resultsService.GetVisitTestsByDateAsync(DateFrom, DateTo.AddDays(1).AddSeconds(-1));
                if (results == null) results = new System.Collections.Generic.List<Open_lab.Models.VisitTest>();

                var visitTestsToLoad = SelectedVisitDetail == null 
                    ? results 
                    : results.Where(vt => vt.VisitId == SelectedVisitDetail.VisitId).ToList();

                VisitTests.Clear();
                foreach (var vt in visitTestsToLoad)
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
                        Status = vt.Status,
                        IsFinished = vt.Status == "Verified" || vt.Status == "Finished",
                        IsVerified = vt.Status == "Verified"
                    });
                }
                
                StatusMessage = $"تم تحميل {VisitTests.Count} تحليل.";
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
                await LoadMedicalHistoryAsync();

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
                StatusMessage = "خطأ: لم يتم تحديد اختبار لحفظ نتائجه.";
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
                await LoadVisitTestsAsync();
                StatusMessage = $"تم اعتماد النتائج وقفلها. ({StatusMessage})";
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
                await LoadVisitTestsAsync();
                StatusMessage = $"تم إعادة فتح النتائج للتعديل. ({StatusMessage})";
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

        private async Task MarkFinishedAllAsync()
        {
            foreach (var test in VisitTests)
            {
                test.IsFinished = true;
            }
            StatusMessage = "تم تعيين الكل كمنتهي.";
            await Task.CompletedTask;
        }

        private async Task MarkVerifiedAllAsync()
        {
            foreach (var test in VisitTests)
            {
                test.IsVerified = true;
                test.IsFinished = true;
            }
            StatusMessage = "تم تعيين الكل كمعتمد.";
            await Task.CompletedTask;
        }

        private async Task MarkPrintedAllAsync()
        {
            foreach (var test in VisitTests)
            {
                test.ShouldPrint = true;
            }
            StatusMessage = "تم تعيين الكل كطباعة.";
            await Task.CompletedTask;
        }

        private async Task LoadMedicalHistoryAsync()
        {
            if (_patientService == null || SelectedVisit == null || SelectedVisit.PatientId <= 0)
            {
                return;
            }

            var history = await _patientService.GetMedicalHistoryAsync(SelectedVisit.PatientId);
            if (history == null)
            {
                MedicalHistorySummary = string.Empty;
                HasMedicalAlerts = false;
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
