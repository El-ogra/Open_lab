using System;
using System.Collections.ObjectModel;
using System.Text;
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
        private int _visitId;
        private string _statusMessage = string.Empty;
        private VisitReportData? _report;
        private string _previewContent = string.Empty;

        public ReportViewerViewModel(IReportService reportService, IPrintService printService, IResultsService resultsService)
        {
            _reportService = reportService;
            _printService = printService;
            _resultsService = resultsService;
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

        public string PreviewContent
        {
            get => _previewContent;
            private set => SetProperty(ref _previewContent, value);
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
                PreviewContent = BuildPreviewContent(report);

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
                await _resultsService.LogVisitReportPrintedAsync(Report.Visit.VisitId, AppSession.UserId > 0 ? AppSession.UserId : 1);
                
                StatusMessage = isReprint
                    ? "تم إرسال إعادة الطباعة إلى Microsoft Print to PDF وتوثيقها."
                    : "تم إرسال التقرير للطباعة وتوثيقها في سجل المريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ طباعة: {ex.Message}";
            }
        }

        private static string BuildPreviewContent(VisitReportData report)
        {
            var builder = new StringBuilder();
            builder.AppendLine("معاينة التقرير");
            builder.AppendLine($"المريض: {report.Patient.FullName}");
            builder.AppendLine($"رقم الزيارة: {report.Visit.VisitId} | التاريخ: {report.Visit.VisitDate:yyyy-MM-dd HH:mm}");
            builder.AppendLine();

            foreach (var test in report.Tests)
            {
                builder.AppendLine(test.Test.NameReport);

                foreach (var resultItem in test.Results)
                {
                    var result = resultItem.Result;
                    var line = $"{result.Parameter.Name}: {result.Value ?? "-"}";
                    if (!string.IsNullOrWhiteSpace(result.Flag))
                    {
                        line += $" ({result.Flag})";
                    }

                    if (!string.IsNullOrWhiteSpace(resultItem.PreviousValue))
                    {
                        line += $" [السابق: {resultItem.PreviousValue} بتاريخ {resultItem.PreviousDate:yyyy-MM-dd}]";
                    }

                    builder.AppendLine($"  {line}");
                }
            }

            return builder.ToString();
        }
    }
}
