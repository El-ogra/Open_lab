using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class VisitService : IVisitService
    {
        private readonly OpenLabDbContext _db;

        public VisitService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<Visit?> GetByIdAsync(int visitId)
        {
            return _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .Include(v => v.Referral)
                .Include(v => v.VisitTests)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);
        }

        public Task<List<Visit>> GetByPatientIdAsync(int patientId)
        {
            return _db.Visits
                .AsNoTracking()
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
        }

        public async Task<Visit> CreateAsync(Visit visit)
        {
            if (visit == null)
            {
                throw new ArgumentNullException(nameof(visit));
            }

            if (visit.PatientId <= 0)
            {
                throw new ArgumentException("PatientId is required.", nameof(visit));
            }

            if (visit.VisitDate == default)
            {
                visit.VisitDate = DateTime.Now;
            }

            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();
            return visit;
        }

        public async Task UpdateAsync(Visit visit)
        {
            if (visit == null)
            {
                throw new ArgumentNullException(nameof(visit));
            }

            _db.Visits.Update(visit);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int visitId)
        {
            var visit = await _db.Visits.FirstOrDefaultAsync(v => v.VisitId == visitId);
            if (visit == null)
            {
                return;
            }

            _db.Visits.Remove(visit);
            await _db.SaveChangesAsync();
        }

        public async Task<VisitTest> AddTestToVisitAsync(int visitId, int testId, decimal? overridePrice = null)
        {
            var visit = await _db.Visits.FirstOrDefaultAsync(v => v.VisitId == visitId);
            if (visit == null)
            {
                throw new InvalidOperationException("Visit not found.");
            }

            var test = await _db.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                throw new InvalidOperationException("Test not found.");
            }

            var visitTest = new VisitTest
            {
                VisitId = visitId,
                TestId = testId,
                Price = overridePrice ?? test.Price,
                Status = "Pending"
            };

            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();
            return visitTest;
        }

        public async Task RemoveVisitTestAsync(int visitTestId)
        {
            var visitTest = await _db.VisitTests.FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);
            if (visitTest == null)
            {
                return;
            }

            _db.VisitTests.Remove(visitTest);
            await _db.SaveChangesAsync();
        }
    }
}
