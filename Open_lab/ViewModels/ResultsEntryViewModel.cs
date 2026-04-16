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
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private VisitTestRow? _selectedVisitTest;
        private string _statusMessage = string.Empty;

        public ResultsEntryViewModel(IResultsService resultsService)
        {
            _resultsService = resultsService;
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

        public ICommand LoadVisitTestsCommand { get; }
        public ICommand SaveResultsCommand { get; }
        public ICommand VerifyResultsCommand { get; }
        public ICommand ReopenResultsCommand { get; }

        private async Task LoadVisitTestsAsync()
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
                        TestId = vt.TestId,
                        PatientName = vt.Visit.Patient.FullName,
                        TestName = vt.Test.NameReport,
                        VisitDate = vt.Visit.VisitDate,
                        Status = vt.Status
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
            if (SelectedVisitTest == null)
            {
                return;
            }

            try
            {
                var parameters = await _resultsService.GetParametersForTestAsync(SelectedVisitTest.TestId);
                var results = await _resultsService.GetResultsForVisitTestAsync(SelectedVisitTest.VisitTestId);

                foreach (var param in parameters)
                {
                    var existing = results.FirstOrDefault(r => r.ParameterId == param.ParameterId);
                    ResultItems.Add(new ResultEntryItem
                    {
                        ParameterId = param.ParameterId,
                        ParameterName = param.Name,
                        Value = existing?.Value,
                        Flag = existing?.Flag,
                        Comment = existing?.Comment
                    });
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task SaveResultsAsync()
        {
            if (SelectedVisitTest == null)
            {
                return;
            }

            try
            {
                foreach (var item in ResultItems)
                {
                    await _resultsService.SaveResultAsync(SelectedVisitTest.VisitTestId, item.ParameterId, item.Value, item.Flag, item.Comment);
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
                return;
            }

            try
            {
                await _resultsService.ReopenVisitTestAsync(SelectedVisitTest.VisitTestId);
                SelectedVisitTest.Status = "InProgress";
                RaiseCommandStates();
                StatusMessage = "تم إعادة فتح النتائج للتعديل.";
                await LoadVisitTestsAsync();
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
    }
}
