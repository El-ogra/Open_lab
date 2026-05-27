using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IPatientService
    {
        Task<Patient?> GetByIdAsync(int patientId);
        Task<Patient?> GetByLabIdAsync(string labId);
        Task<MedicalHistory?> GetMedicalHistoryAsync(int patientId);
        Task SaveMedicalHistoryAsync(int patientId, MedicalHistory history);
        Task<List<Patient>> SearchAsync(string? name, string? phone, DateTime? date = null, string? labId = null);
        Task<string> GenerateNextLabIdAsync(DateTime? forDate = null);
        Task<Patient> CreateAsync(Patient patient);
        Task UpdateAsync(Patient patient);
        Task UpdateAsync(Patient patient, int userId);
        Task AssignReferralAsync(int patientId, int? referralId, int userId);
        Task DeleteAsync(int patientId);

        // Fix #1 (ambient-TX/SQLite mismatch): callers that need to bundle
        // multi-service writes (Visit + VisitTests + Invoice) into a single
        // commit must use this helper instead of System.Transactions.TransactionScope,
        // which the SQLite EF Core provider does not support.
        Task RunInTransactionAsync(Func<Task> action);
    }
}
