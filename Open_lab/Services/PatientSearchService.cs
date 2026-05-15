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
            return await SearchPatientsAsync(new PatientSearchCriteria
            {
                Name = name,
                Phone = phone,
                LabId = labId,
                Date = date
            });
        }

        public async Task<List<Patient>> SearchPatientsAsync(PatientSearchCriteria criteria)
        {
            var query = _db.Patients.AsNoTracking().AsQueryable();
            criteria ??= new PatientSearchCriteria();

            if (!string.IsNullOrWhiteSpace(criteria.Name))
            {
                query = query.Where(p => p.FullName.Contains(criteria.Name));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Phone))
            {
                query = query.Where(p =>
                    (p.Phone != null && p.Phone.Contains(criteria.Phone)) ||
                    (p.HomePhone != null && p.HomePhone.Contains(criteria.Phone)));
            }

            if (!string.IsNullOrWhiteSpace(criteria.LabId))
            {
                query = query.Where(p => p.LabId == criteria.LabId);
            }

            if (!string.IsNullOrWhiteSpace(criteria.NationalId))
            {
                query = query.Where(p => p.NationalId != null && p.NationalId.Contains(criteria.NationalId));
            }

            if (criteria.Date.HasValue)
            {
                var searchDate = criteria.Date.Value.Date;
                query = query.Where(p => p.Visits.Any(v => v.VisitDate.Date == searchDate));
            }

            if (criteria.DateFrom.HasValue || criteria.DateTo.HasValue)
            {
                var from = (criteria.DateFrom ?? DateTime.MinValue).Date;
                var to = (criteria.DateTo ?? DateTime.MaxValue).Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.Visits.Any(v => v.VisitDate >= from && v.VisitDate <= to));
            }

            if (!string.IsNullOrWhiteSpace(criteria.AgeGroup) && !string.Equals(criteria.AgeGroup, "الكل", StringComparison.OrdinalIgnoreCase))
            {
                query = criteria.AgeGroup switch
                {
                    "أطفال" => query.Where(p => p.Age.HasValue && p.Age.Value < 18),
                    "بالغين" => query.Where(p => p.Age.HasValue && p.Age.Value >= 18 && p.Age.Value < 60),
                    "كبار سن" => query.Where(p => p.Age.HasValue && p.Age.Value >= 60),
                    _ => query
                };
            }

            return await query.OrderBy(p => p.FullName).Take(100).ToListAsync();
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
