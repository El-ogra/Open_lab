using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IPatientSearchService
    {
        Task<List<Patient>> SearchPatientsAsync(string? name, string? phone, string? labId);
        Task<List<Visit>> GetPatientVisitsAsync(int patientId);
    }
}
