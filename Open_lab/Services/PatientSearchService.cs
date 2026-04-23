using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class PatientSearchService : IPatientSearchService
    {
        private readonly OpenLabDbContext _db;

        public PatientSearchService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<Patient>> SearchPatientsAsync(string? name, string? phone, string? labId, DateTime? date = null)
        {
            var query = _db.Patients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(p => p.FullName.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                query = query.Where(p => p.Phone != null && p.Phone.Contains(phone));
            }

            if (!string.IsNullOrWhiteSpace(labId))
            {
                query = query.Where(p => p.LabId == labId);
            }

            if (date.HasValue)
            {
                var searchDate = date.Value.Date;
                query = query.Where(p => p.Visits.Any(v => v.VisitDate.Date == searchDate));
            }

            return await query.OrderBy(p => p.FullName).ToListAsync();
        }

        public Task<List<Visit>> GetPatientVisitsAsync(int patientId)
        {
            return _db.Visits.AsNoTracking()
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
        }
    }
}
