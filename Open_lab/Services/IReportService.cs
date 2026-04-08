using System;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IReportService
    {
        Task<VisitReportData?> GetVisitReportAsync(int visitId);
        Task<PatientHistoryReportData> GetPatientHistoryAsync(int patientId, DateTime? from, DateTime? to);
    }
}
