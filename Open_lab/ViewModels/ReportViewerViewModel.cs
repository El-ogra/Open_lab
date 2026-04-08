using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ReportViewerViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private int _visitId;
        private string _statusMessage = string.Empty;
        private VisitReportData? _report;

        public ReportViewerViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            Tests = new ObservableCollection<VisitTestReportItem>();
            LoadReportCommand = new RelayCommand(async _ => await LoadReportAsync());
        }

        public int VisitId
        {
            get => _visitId;
            set => SetProperty(ref _visitId, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public VisitReportData? Report
        {
            get => _report;
            private set => SetProperty(ref _report, value);
        }

        public ObservableCollection<VisitTestReportItem> Tests { get; }

        public ICommand LoadReportCommand { get; }

        private async Task LoadReportAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                using var db = _dbFactory();
                var service = new ReportService(db);
                var report = await service.GetVisitReportAsync(VisitId);
                if (report == null)
                {
                    StatusMessage = "لم يتم العثور على تقرير.";
                    return;
                }

                Report = report;
                Tests.Clear();
                foreach (var item in report.Tests)
                {
                    Tests.Add(item);
                }

                StatusMessage = "تم تحميل التقرير.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
