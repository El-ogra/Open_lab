using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    /// <summary>
    /// Gap 4.8 — Print Blank Report Architecture Fix.
    /// Implements the construction and dispatch of a blank report. All business
    /// formatting logic that previously lived inside BlankReportViewModel was
    /// moved here so the ViewModel becomes a thin coordinator:
    ///     ViewModel  ->  IBlankReportService  ->  IPrintService
    /// </summary>
    public class BlankReportService : IBlankReportService
    {
        private readonly IReportService _reportService;
        private readonly IPrintService? _printService;

        public BlankReportService(IReportService reportService, IPrintService? printService = null)
        {
            _reportService = reportService;
            _printService = printService;
        }

        public async Task<IReadOnlyList<string>?> BuildBlankReportAsync(int visitId)
        {
            if (visitId <= 0)
            {
                return null;
            }

            var reportData = await _reportService.GetVisitReportAsync(visitId);
            if (reportData == null)
            {
                return null;
            }

            return BuildLines(reportData);
        }

        public async Task<bool> PrintBlankReportAsync(int visitId)
        {
            if (_printService == null || visitId <= 0)
            {
                return false;
            }

            var reportData = await _reportService.GetVisitReportAsync(visitId);
            if (reportData == null)
            {
                return false;
            }

            var lines = BuildLines(reportData);
            var collection = new ObservableCollection<string>(lines);

            await _printService.PrintTextReportAsync("تقرير فارغ", collection, $"BlankReport_{visitId}");
            return true;
        }

        private static List<string> BuildLines(VisitReportData reportData)
        {
            var lines = new List<string>
            {
                $"المريض: {reportData.Patient.FullName}",
                $"Lab ID: {reportData.Patient.LabId}",
                $"التاريخ: {reportData.Visit.VisitDate:yyyy-MM-dd}",
                string.Empty,
                "تحاليل مطلوبة (بدون نتائج):"
            };

            foreach (var test in reportData.Tests)
            {
                lines.Add($"- {test.Test.NameReport}");
            }

            return lines;
        }
    }
}
