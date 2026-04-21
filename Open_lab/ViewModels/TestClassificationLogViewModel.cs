using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class TestClassificationLogViewModel : BaseViewModel
    {
        private readonly ITestClassificationService _testClassificationService;
        private readonly IPrintService _printService;
        private DateTime _from = DateTime.Today.AddDays(-30);
        private DateTime _to = DateTime.Today;
        private string _statusMessage = string.Empty;

        public TestClassificationLogViewModel(ITestClassificationService testClassificationService, IPrintService printService)
        {
            _testClassificationService = testClassificationService;
            _printService = printService;
            Items = new ObservableCollection<ReagentConsumptionReport>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsView));
            PrintCommand = new RelayCommand(async _ => await PrintAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsView));
            _ = LoadAsync();
        }

        public DateTime From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public DateTime To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<ReagentConsumptionReport> Items { get; }

        public ICommand LoadCommand { get; }
        public ICommand PrintCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var from = From.Date;
                var to = To.Date.AddDays(1).AddSeconds(-1);
                var rows = await _testClassificationService.GetConsumptionReportAsync(from, to);

                Items.Clear();
                foreach (var row in rows)
                {
                    Items.Add(row);
                }

                StatusMessage = $"تم تحميل {Items.Count} سجل تصنيف.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task PrintAsync()
        {
            try
            {
                var lines = new ObservableCollection<string>
                {
                    $"الفترة: {From:yyyy-MM-dd} إلى {To:yyyy-MM-dd}",
                    string.Empty
                };

                foreach (var item in Items)
                {
                    lines.Add($"- {item.ReagentName}: استهلاك {item.TotalConsumed:N2} {item.Unit} | عدد التحاليل {item.TestCount}");
                }

                await _printService.PrintTextReportAsync("سجل تصنيف التحاليل", lines, "TestClassificationLog");
                StatusMessage = "تم إرسال التقرير للطباعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
