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
    }
}
