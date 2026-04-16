using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ReportViewerViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IPrintService _printService;
        private int _visitId;
        private string _statusMessage = string.Empty;
        private VisitReportData? _report;

        public ReportViewerViewModel(IReportService reportService, IPrintService printService)
        {
            _reportService = reportService;
            _printService = printService;
            Tests = new ObservableCollection<VisitTestReportItem>();
            LoadReportCommand = new RelayCommand(async _ => await LoadReportAsync());
            PrintCommand = new RelayCommand(async _ => await PrintAsync(false), _ => Report != null);
            ReprintCommand = new RelayCommand(async _ => await PrintAsync(true), _ => Report != null);
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
            private set
            {
                if (SetProperty(ref _report, value))
                {
                    (PrintCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (ReprintCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<VisitTestReportItem> Tests { get; }

        public ICommand LoadReportCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand ReprintCommand { get; }

        private async Task LoadReportAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                var report = await _reportService.GetVisitReportAsync(VisitId);
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

        private async Task PrintAsync(bool isReprint)
        {
            if (Report == null)
            {
                return;
            }

            try
            {
                await _printService.PrintVisitReportAsync(Report, isReprint);
                StatusMessage = isReprint
                    ? "تم إرسال إعادة الطباعة إلى Microsoft Print to PDF."
                    : "تم إرسال التقرير للطباعة عبر Microsoft Print to PDF.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ طباعة: {ex.Message}";
            }
        }
    }
}
