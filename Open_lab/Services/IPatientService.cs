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
        Task<List<Patient>> SearchAsync(string? name, string? phone);
        Task<string> GenerateNextLabIdAsync(DateTime? forDate = null);
        Task<Patient> CreateAsync(Patient patient);
        Task UpdateAsync(Patient patient);
        Task DeleteAsync(int patientId);
    }
}
