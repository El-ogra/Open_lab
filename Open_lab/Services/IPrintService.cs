using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public interface IPrintService
    {
        Task PrintReceiptAsync(ReceiptData data, string? barcodeText = null);
        Task PrintVisitReportAsync(VisitReportData report, bool isReprint = false);
        Task PrintPatientHistoryAsync(PatientHistoryReportData history);
        Task PrintWorksheetByPatientAsync(DateTime from, DateTime to, IReadOnlyCollection<WorkSheetPatientRow> rows);
        Task PrintWorksheetByTestAsync(DateTime from, DateTime to, IReadOnlyCollection<WorkSheetTestRow> rows);
        Task PrintTextReportAsync(string title, IReadOnlyCollection<string> lines, string? jobName = null);
        Task PrintCultureReportAsync(CultureReportData data);
    }

    public class CultureReportData
    {
        public int VisitId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string LabId { get; set; } = string.Empty;
        public string CultureName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public List<CultureResultRow> Results { get; set; } = new();
    }

    public class CultureResultRow
    {
        public string AntibioticName { get; set; } = string.Empty;
        public string Sensitivity { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }
}
