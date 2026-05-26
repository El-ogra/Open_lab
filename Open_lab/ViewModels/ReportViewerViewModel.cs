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
        private readonly IResultsService _resultsService;
        private readonly IReportPdfService? _reportPdfService;
        private int _visitId;
        private string _statusMessage = string.Empty;
        private VisitReportData? _report;
        private string _previewPdfPath = string.Empty;
        private Uri? _previewPdfUri;

        public ReportViewerViewModel(IReportService reportService, IPrintService printService, IResultsService resultsService)
            : this(reportService, printService, resultsService, null)
        {
        }

        public ReportViewerViewModel(IReportService reportService, IPrintService printService, IResultsService resultsService, IReportPdfService? reportPdfService)
        {
            _reportService = reportService;
            _printService = printService;
            _resultsService = resultsService;
            _reportPdfService = reportPdfService;
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

        public string PreviewPdfPath
        {
            get => _previewPdfPath;
            private set => SetProperty(ref _previewPdfPath, value);
        }

        public string PreviewContent => PreviewPdfPath;

        public Uri? PreviewPdfUri
        {
            get => _previewPdfUri;
            private set => SetProperty(ref _previewPdfUri, value);
        }

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
                if (_reportPdfService != null)
                {
                    PreviewPdfPath = await _reportPdfService.GenerateVisitReportPdfAsync(report);
                    OnPropertyChanged(nameof(PreviewContent));
                    PreviewPdfUri = new Uri(PreviewPdfPath);
                }

                StatusMessage = string.IsNullOrWhiteSpace(PreviewPdfPath)
                    ? "تم تحميل التقرير بدون معاينة PDF."
                    : "تم تحميل التقرير وإنشاء معاينة PDF.";
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
                StatusMessage = "لا يوجد تقرير للطباعة.";
                return;
            }

            try
            {
                await _printService.PrintVisitReportAsync(Report, isReprint);
                await _resultsService.LogVisitReportPrintedAsync(Report.Visit.VisitId, SessionContext.Current.UserId > 0 ? SessionContext.Current.UserId : 1);
                
                StatusMessage = isReprint
                    ? "تم إرسال إعادة الطباعة إلى Microsoft Print to PDF وتوثيقها."
                    : "تم إرسال التقرير للطباعة وتوثيقها في سجل المريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ طباعة: {ex.Message}";
            }
        }

    }
}
