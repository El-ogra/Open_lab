using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IPatientSearchService
    {
        Task<List<Patient>> SearchPatientsAsync(string? name, string? phone, string? labId, DateTime? date = null);
        Task<List<Patient>> SearchPatientsAsync(PatientSearchCriteria criteria);
        Task<List<Visit>> GetPatientVisitsAsync(int patientId);
    }

    public class PatientSearchCriteria
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? LabId { get; set; }
        public string? NationalId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? AgeGroup { get; set; }
    }
}
