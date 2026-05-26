using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ResultsEntryViewModel : BaseViewModel
    {
        private readonly IResultsService _resultsService;
        private readonly IPatientService? _patientService;
        private readonly IVisitService? _visitService;
        private readonly IPrintService? _printService;

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

        private ObservableCollection<VisitTestRow> _allVisitTests = new ObservableCollection<VisitTestRow>();

        public ResultsEntryViewModel(IResultsService resultsService)
            : this(resultsService, null, null)
        {
        }

        public ResultsEntryViewModel(IResultsService resultsService, IPatientService? patientService)
            : this(resultsService, patientService, null)
        {
        }

        public ResultsEntryViewModel(IResultsService resultsService, IPatientService? patientService, IVisitService? visitService)
            : this(resultsService, patientService, visitService, null)
        {
        }

        [ActivatorUtilitiesConstructor]
        public ResultsEntryViewModel(IResultsService resultsService, IPatientService? patientService, IVisitService? visitService, IPrintService? printService)
        {
            _resultsService = resultsService;
            _patientService = patientService;
            _visitService = visitService;
            _printService = printService;

            TodayPatients = new ObservableCollection<VisitSummary>();
            VisitTests = new ObservableCollection<VisitTestRow>();
            Visits = new ObservableCollection<VisitSummary>();
            ResultItems = new ObservableCollection<ResultEntryItem>();

            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
            SearchByDateCommand = new RelayCommand(async _ => await SearchByDateAsync());
            SelectPatientCommand = new RelayCommand(async _ => await SelectPatientAsync());
            LoadVisitTestsCommand = new RelayCommand(async _ => await LoadVisitTestsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsView));
            SaveResultsCommand = new RelayCommand(async _ => await SaveResultsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null && !IsSelectedVerified());
            VerifyResultsCommand = new RelayCommand(async _ => await VerifyResultsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null);
            ReopenResultsCommand = new RelayCommand(async _ => await ReopenResultsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null && IsSelectedVerified());

            MarkFinishedAllCommand = new RelayCommand(async _ => await MarkFinishedAllAsync());
            MarkVerifiedAllCommand = new RelayCommand(async _ => await MarkVerifiedAllAsync());
            MarkPrintedAllCommand = new RelayCommand(async _ => await MarkPrintedAllAsync());

            ShowMedicalHistoryCommand = new RelayCommand(async _ => await LoadMedicalHistoryAsync());
            PrintGroupReportCommand = new RelayCommand(async _ => await PrintGroupReportAsync(), _ => VisitTests.Count > 0);
            PrintWorksheetCommand = new RelayCommand(async _ => await PrintWorksheetAsync(), _ => VisitTests.Count > 0);
            PrintMethodsCommand = new RelayCommand(async _ => await PrintMethodsAsync(), _ => SelectedVisitTest != null);
            PrintBlankReportCommand = new RelayCommand(async _ => await PrintBlankReportAsync(), _ => VisitTests.Count > 0);
            GoToPatientDataCommand = new RelayCommand(_ => NavigateToPatientData(), _ => SelectedVisit != null);

            FilterVipCommand = new RelayCommand(_ => FilterByVip());
            FilterAllCommand = new RelayCommand(_ => FilterAll());
            FilterLabCommand = new RelayCommand(_ => FilterByLab());

            ShowPreviousResultCommand = new RelayCommand(async _ => await ShowPreviousResultAsync(), _ => SelectedVisitTest != null);
            ShowNormalRangeCommand = new RelayCommand(async _ => await ShowNormalRangeAsync(), _ => SelectedVisitTest != null);
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
                        VisitDate = v.VisitDate,
                        StatusMarker = BuildVisitStatusMarker(visitTests.Where(vt => vt.VisitId == v.VisitId))
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
                _allVisitTests.Clear();

                foreach (var vt in visitTestsToLoad)
                {
                    var row = new VisitTestRow
                    {
                        VisitTestId = vt.VisitTestId,
                        PatientId = vt.Visit?.PatientId ?? 0,
                        TestId = vt.TestId,
                        PatientName = vt.Visit?.Patient?.FullName ?? string.Empty,
                        PatientGender = vt.Visit?.Patient?.Gender ?? string.Empty,
                        PatientAge = vt.Visit?.Patient?.Age ?? 0,
                        TestName = vt.Test?.NameReport ?? string.Empty,
                        VisitDate = vt.Visit?.VisitDate ?? DateTime.MinValue,
                        ResultValue = vt.ResultValues?.FirstOrDefault()?.Value,
                        Status = vt.Status,
                        IsFinished = vt.Status == "Verified" || vt.Status == "Finished",
                        IsVerified = vt.Status == "Verified"
                    };
                    VisitTests.Add(row);
                    _allVisitTests.Add(row);
                }

                StatusMessage = $"تم تحميل {VisitTests.Count} تحليل.";
                (PrintWorksheetCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

                if (ResultItems.Count > 0 && string.IsNullOrWhiteSpace(SelectedVisitTest.ResultValue))
                {
                    SelectedVisitTest.ResultValue = ResultItems.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Value))?.Value;
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
                // FIX (الفجوة الخامسة): سابقاً كان هذا السطر يكتب ResultValue (الاختصار) فوق
                // المعامل الأول دائماً فيفقد المستخدم ما أدخله في Grid 2 لتحليل متعدد المعاملات.
                // الحل: الاختصار يعمل فقط إذا كان التحليل معاملاً واحداً والخانة فارغة.
                if (ResultItems.Count == 1 &&
                    string.IsNullOrWhiteSpace(ResultItems[0].Value) &&
                    !string.IsNullOrWhiteSpace(SelectedVisitTest.ResultValue))
                {
                    ResultItems[0].Value = SelectedVisitTest.ResultValue;
                }

                foreach (var item in ResultItems)
                {
                    await _resultsService.SaveResultAsync(
                        SelectedVisitTest.VisitTestId,
                        item.ParameterId,
                        item.Value,
                        item.Flag,
                        item.Comment,
                        SessionContext.Current.UserId > 0 ? SessionContext.Current.UserId : 1);
                }

                SelectedVisitTest.Status = ResultItems.Any(i => !string.IsNullOrWhiteSpace(i.Value)) ? "Completed" : "InProgress";
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
                await _resultsService.VerifyVisitTestAsync(SelectedVisitTest.VisitTestId, SessionContext.Current.UserId > 0 ? SessionContext.Current.UserId : 1);
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
            (ShowPreviousResultCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ShowNormalRangeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (GoToPatientDataCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task MarkFinishedAllAsync()
        {
            var ids = VisitTests.Select(t => t.VisitTestId).ToList();
            await _resultsService.MarkVisitTestsCompletedAsync(ids);

            foreach (var test in VisitTests)
            {
                test.IsFinished = true;
                if (!string.Equals(test.Status, "Verified", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(test.Status, "Delivered", StringComparison.OrdinalIgnoreCase))
                {
                    test.Status = "Completed";
                }
            }
            StatusMessage = "تم تعيين الكل كمنتهي.";
        }

        private async Task MarkVerifiedAllAsync()
        {
            var userId = SessionContext.Current.UserId > 0 ? SessionContext.Current.UserId : 1;
            await _resultsService.MarkVisitTestsVerifiedAsync(VisitTests.Select(t => t.VisitTestId), userId);

            foreach (var test in VisitTests)
            {
                test.IsVerified = true;
                test.IsFinished = true;
                test.Status = "Verified";
            }
            StatusMessage = "تم تعيين الكل كمعتمد.";
        }

        private async Task MarkPrintedAllAsync()
        {
            var userId = SessionContext.Current.UserId > 0 ? SessionContext.Current.UserId : 1;
            await _resultsService.MarkVisitTestsPrintedAsync(VisitTests.Select(t => t.VisitTestId), userId);

            foreach (var test in VisitTests)
            {
                test.ShouldPrint = true;
            }
            StatusMessage = "تم تعيين الكل كطباعة.";
        }

        private async Task PrintWorksheetAsync()
        {
            if (_printService == null)
            {
                StatusMessage = "خدمة الطباعة غير متاحة.";
                return;
            }

            if (VisitTests.Count == 0)
            {
                StatusMessage = "لا توجد تحاليل لطباعة ورقة العمل.";
                return;
            }

            var rows = VisitTests
                .GroupBy(t => new { t.PatientId, t.PatientName, t.VisitDate, VisitId = SelectedVisitDetail?.VisitId ?? SelectedVisit?.VisitId ?? 0 })
                .Select(g => new WorkSheetPatientRow
                {
                    VisitId = g.Key.VisitId,
                    PatientName = g.Key.PatientName,
                    VisitDate = g.Key.VisitDate,
                    TestsCount = g.Count()
                })
                .ToList();

            await _printService.PrintWorksheetByPatientAsync(DateFrom.Date, DateTo.Date, rows);
            StatusMessage = "تم إرسال ورقة العمل للطباعة.";
        }

        private async Task PrintGroupReportAsync()
        {
            if (_printService == null)
            {
                StatusMessage = "خدمة الطباعة غير متاحة.";
                return;
            }

            if (VisitTests.Count == 0)
            {
                StatusMessage = "لا توجد تحاليل لتقرير مجمع.";
                return;
            }

            var lines = VisitTests.Select(t => $"{t.TestName}: {t.ResultValue ?? "-"} ({t.Status})").ToList();
            await _printService.PrintTextReportAsync("تقرير مجمع", lines, "GroupReport");
            StatusMessage = "تم إرسال التقرير المجمع للطباعة.";
        }

        private async Task PrintMethodsAsync()
        {
            if (SelectedVisitTest == null)
            {
                StatusMessage = "لم يتم تحديد تحليل.";
                return;
            }

            if (_printService == null)
            {
                StatusMessage = "خدمة الطباعة غير متاحة.";
                return;
            }

            var lines = new List<string>
            {
                $"التحليل: {SelectedVisitTest.TestName}",
                $"المريض: {SelectedVisitTest.PatientName}",
                $"التاريخ: {SelectedVisitTest.VisitDate:yyyy-MM-dd}",
                "",
                "طرق التحليل غير محددة في النظام."
            };

            await _printService.PrintTextReportAsync("طرق التحليل", lines, "MethodsReport");
            StatusMessage = $"تم إرسال طرق التحليل لـ {SelectedVisitTest.TestName} للطباعة.";
        }

        private async Task PrintBlankReportAsync()
        {
            if (_printService == null)
            {
                StatusMessage = "خدمة الطباعة غير متاحة.";
                return;
            }

            var lines = VisitTests.Select(t => $"[ ] {t.TestName}").ToList();
            await _printService.PrintTextReportAsync("تقرير فارغ", lines, "BlankReport");
            StatusMessage = "تم إرسال التقرير الفارغ للطباعة.";
        }

        private void NavigateToPatientData()
        {
            if (SelectedVisit == null)
            {
                StatusMessage = "لم يتم تحديد مريض.";
                return;
            }

            StatusMessage = $"الانتقال لبيانات المريض: {SelectedVisit.PatientName}";
            // Navigation would be handled by the main window via Messenger or similar pattern
            // This is a placeholder that sets the status message
        }

        private void FilterByVip()
        {
            VisitTests.Clear();
            foreach (var test in _allVisitTests.Where(t => IsVipPatient(t)))
            {
                VisitTests.Add(test);
            }
            StatusMessage = $"تم تصفية VIP: {VisitTests.Count} تحليل.";
        }

        private void FilterAll()
        {
            VisitTests.Clear();
            foreach (var test in _allVisitTests)
            {
                VisitTests.Add(test);
            }
            StatusMessage = $"تم عرض الكل: {VisitTests.Count} تحليل.";
        }

        private void FilterByLab()
        {
            VisitTests.Clear();
            foreach (var test in _allVisitTests.Where(t => IsLabToPatient(t)))
            {
                VisitTests.Add(test);
            }
            StatusMessage = $"تم تصفية Lab To: {VisitTests.Count} تحليل.";
        }

        private bool IsVipPatient(VisitTestRow test)
        {
            // In the actual implementation, this would check the Patient's IsVip property
            // For now, we check the patient name or ID - actual VIP logic should be implemented
            // based on the Patient.IsVip property from the Visit object
            return true; // Placeholder - actual implementation would check patient.IsVip
        }

        private bool IsLabToPatient(VisitTestRow test)
        {
            // In the actual implementation, this would check the Visit.AccountType
            // For Lab To patients, AccountType would be "Referral" or "Lab to Lab"
            return true; // Placeholder - actual implementation would check visit.AccountType
        }

        private async Task ShowPreviousResultAsync()
        {
            if (SelectedVisitTest == null)
            {
                StatusMessage = "لم يتم تحديد تحليل.";
                return;
            }

            try
            {
                var previousResult = await _resultsService.GetPreviousResultAsync(
                    SelectedVisitTest.PatientId,
                    SelectedVisitTest.TestId,
                    SelectedVisitTest.VisitTestId);

                if (previousResult == null)
                {
                    StatusMessage = "لا توجد نتيجة سابقة لهذا التحليل.";
                    return;
                }

                var previousValue = previousResult.ResultValues?.FirstOrDefault()?.Value;
                var previousDate = previousResult.Visit?.VisitDate ?? DateTime.MinValue;

                StatusMessage = $"النتيجة السابقة ({previousDate:yyyy-MM-dd}): {previousValue ?? "-"}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task ShowNormalRangeAsync()
        {
            if (SelectedVisitTest == null)
            {
                StatusMessage = "لم يتم تحديد تحليل.";
                return;
            }

            try
            {
                var parameters = await _resultsService.GetParametersForTestAsync(SelectedVisitTest.TestId);
                if (parameters == null || !parameters.Any())
                {
                    StatusMessage = "لا توجد معايير محددة لهذا التحليل.";
                    return;
                }

                var parameterNames = string.Join(", ", parameters.Select(p => p.Name));
                StatusMessage = $"المدى الطبيعي للمعايير: {parameterNames}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private static string BuildVisitStatusMarker(IEnumerable<Open_lab.Models.VisitTest> tests)
        {
            var list = tests.ToList();
            if (list.Count == 0) return "جديد";
            if (list.Any(t => string.Equals(t.Status, "Delivered", StringComparison.OrdinalIgnoreCase))) return "تم التسليم";
            if (list.All(t => string.Equals(t.Status, "Verified", StringComparison.OrdinalIgnoreCase))) return "تمت المراجعة";
            if (list.Any(t => string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase))) return "لم تراجع";
            if (list.Any(t => string.Equals(t.Status, "InProgress", StringComparison.OrdinalIgnoreCase))) return "لم تكتمل";
            return "جديد";
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