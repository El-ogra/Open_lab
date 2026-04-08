using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ResultsEntryViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private VisitTestRow? _selectedVisitTest;
        private string _statusMessage = string.Empty;

        public ResultsEntryViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            VisitTests = new ObservableCollection<VisitTestRow>();
            ResultItems = new ObservableCollection<ResultEntryItem>();

            LoadVisitTestsCommand = new RelayCommand(async _ => await LoadVisitTestsAsync());
            SaveResultsCommand = new RelayCommand(async _ => await SaveResultsAsync(), _ => SelectedVisitTest != null);
            VerifyResultsCommand = new RelayCommand(async _ => await VerifyResultsAsync(), _ => SelectedVisitTest != null);
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
                    (SaveResultsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (VerifyResultsCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

        private async Task LoadVisitTestsAsync()
        {
            try
            {
                using var db = _dbFactory();
                var service = new ResultsService(db);
                var visitTests = await service.GetVisitTestsByDateAsync(DateFrom, DateTo.AddDays(1).AddSeconds(-1));

                VisitTests.Clear();
                foreach (var vt in visitTests)
                {
                    VisitTests.Add(new VisitTestRow
                    {
                        VisitTestId = vt.VisitTestId,
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
                using var db = _dbFactory();
                var service = new ResultsService(db);

                var visitTestId = SelectedVisitTest.VisitTestId;
                var visitTest = await db.VisitTests.AsNoTracking().FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);
                if (visitTest == null)
                {
                    StatusMessage = "التحليل غير موجود.";
                    return;
                }

                var parameters = await service.GetParametersForTestAsync(visitTest.TestId);
                var results = await service.GetResultsForVisitTestAsync(visitTestId);

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
                using var db = _dbFactory();
                var service = new ResultsService(db);

                foreach (var item in ResultItems)
                {
                    await service.SaveResultAsync(SelectedVisitTest.VisitTestId, item.ParameterId, item.Value, item.Flag, item.Comment);
                }

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
                using var db = _dbFactory();
                var service = new ResultsService(db);
                await service.VerifyVisitTestAsync(SelectedVisitTest.VisitTestId, 1);
                StatusMessage = "تم اعتماد النتائج.";
                await LoadVisitTestsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
