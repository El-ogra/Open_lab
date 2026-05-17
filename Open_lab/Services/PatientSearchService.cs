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
                var labId = criteria.LabId.Trim();
                query = query.Where(p => p.LabId.Contains(labId));
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

            if (criteria.AgeFrom.HasValue)
            {
                query = query.Where(p => p.Age.HasValue && p.Age.Value >= criteria.AgeFrom.Value);
            }

            if (criteria.AgeTo.HasValue)
            {
                query = query.Where(p => p.Age.HasValue && p.Age.Value <= criteria.AgeTo.Value);
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
                .Include(v => v.Invoice)
                .Include(v => v.VisitTests)
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
        }

        public Task<List<VisitTest>> GetPatientVisitTestsAsync(int patientId)
        {
            return _db.VisitTests.AsNoTracking()
                .Include(vt => vt.Test)
                .Include(vt => vt.Visit)
                .Where(vt => vt.Visit.PatientId == patientId)
                .OrderByDescending(vt => vt.Visit.VisitDate)
                .ThenBy(vt => vt.Test.NameReport)
                .ToListAsync();
        }

        public async Task DeletePatientAsync(int patientId)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);
            if (patient == null)
            {
                return;
            }

            var hasVisits = await _db.Visits.AnyAsync(v => v.PatientId == patientId);
            if (hasVisits)
            {
                throw new InvalidOperationException("لا يمكن حذف مريض لديه زيارات مسجلة.");
            }

            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Patient>> GetUnenteredResultsPatientsAsync(DateTime from, DateTime to)
        {
            var visits = await _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .Include(v => v.VisitTests)
                .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                .ToListAsync();

            var patientIds = visits
                .Where(v => v.VisitTests.Any(t => string.IsNullOrEmpty(t.Status) || t.Status == "InProgress"))
                .Select(v => v.PatientId)
                .Distinct()
                .ToList();

            return await _db.Patients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.PatientId))
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<List<Patient>> GetUnreviewedResultsPatientsAsync(DateTime from, DateTime to)
        {
            var visits = await _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .Include(v => v.VisitTests)
                .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                .ToListAsync();

            var patientIds = visits
                .Where(v => v.VisitTests.Any(t => string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase)))
                .Select(v => v.PatientId)
                .Distinct()
                .ToList();

            return await _db.Patients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.PatientId))
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<List<Patient>> GetUnprintedResultsPatientsAsync(DateTime from, DateTime to)
        {
            var visits = await _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .Include(v => v.VisitTests)
                .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                .ToListAsync();

            var patientIds = visits
                .Where(v => v.VisitTests.Any(t =>
                    (string.Equals(t.Status, "Verified", StringComparison.OrdinalIgnoreCase)) &&
                    !string.Equals(t.Status, "Printed", StringComparison.OrdinalIgnoreCase)))
                .Select(v => v.PatientId)
                .Distinct()
                .ToList();

            return await _db.Patients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.PatientId))
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<List<Patient>> GetUndeliveredResultsPatientsAsync(DateTime from, DateTime to)
        {
            var visits = await _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .Include(v => v.VisitTests)
                .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                .ToListAsync();

            var patientIds = visits
                .Where(v => v.VisitTests.Any(t =>
                    !string.Equals(t.Status, "Delivered", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(t.Status, "Verified", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(t.Status, "Printed", StringComparison.OrdinalIgnoreCase))))
                .Select(v => v.PatientId)
                .Distinct()
                .ToList();

            return await _db.Patients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.PatientId))
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<List<Patient>> GetOpenAccountPatientsAsync(DateTime from, DateTime to)
        {
            var visits = await _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .Include(v => v.Invoice)
                .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                .ToListAsync();

            var patientIds = visits
                .Where(v => v.Invoice != null && v.Invoice.Balance > 0)
                .Select(v => v.PatientId)
                .Distinct()
                .ToList();

            return await _db.Patients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.PatientId))
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<List<Patient>> GetGroupedResultsPatientsAsync(DateTime from, DateTime to)
        {
            var visits = await _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .Include(v => v.VisitTests)
                .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                .ToListAsync();

            var patientIds = visits
                .Where(v => v.VisitTests.Count > 3)
                .Select(v => v.PatientId)
                .Distinct()
                .ToList();

            return await _db.Patients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.PatientId))
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }
    }
}
