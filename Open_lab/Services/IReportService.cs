using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IReportService
    {
        Task<VisitReportData?> GetVisitReportAsync(int visitId);
        Task<VisitReportData?> GetCompositeReportAsync(int visitId, IReadOnlyCollection<int>? orderedVisitTestIds = null);
        Task<PatientHistoryReportData> GetPatientHistoryAsync(int patientId, DateTime? from, DateTime? to);
    }
}
