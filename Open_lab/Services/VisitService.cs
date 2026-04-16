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
        private const string VisitStatusOpen = "Open";
        private const string VisitStatusClosed = "Closed";
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

        public Task<List<VisitTest>> GetVisitTestsAsync(int visitId)
        {
            return _db.VisitTests
                .AsNoTracking()
                .Include(vt => vt.Test)
                .Where(vt => vt.VisitId == visitId)
                .OrderBy(vt => vt.VisitTestId)
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

            var patientExists = await _db.Patients.AnyAsync(p => p.PatientId == visit.PatientId);
            if (!patientExists)
            {
                throw new InvalidOperationException("Patient not found.");
            }

            if (visit.VisitDate == default)
            {
                visit.VisitDate = DateTime.Now;
            }

            visit.AccountType = string.IsNullOrWhiteSpace(visit.AccountType) ? "Cash" : visit.AccountType.Trim();
            if (string.Equals(visit.AccountType, "Referral", StringComparison.OrdinalIgnoreCase) && !visit.ReferralId.HasValue)
            {
                throw new InvalidOperationException("Referral account type requires a referral.");
            }

            visit.Status = string.IsNullOrWhiteSpace(visit.Status) ? VisitStatusOpen : visit.Status;
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

            if (string.Equals(visit.Status, VisitStatusClosed, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Closed visits cannot be edited.");
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

            if (string.Equals(visit.Status, VisitStatusClosed, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Closed visits cannot be deleted.");
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

            if (string.Equals(visit.Status, VisitStatusClosed, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Closed visits cannot accept new tests.");
            }

            var hasDuplicate = await _db.VisitTests.AnyAsync(vt => vt.VisitId == visitId && vt.TestId == testId);
            if (hasDuplicate)
            {
                throw new InvalidOperationException("Test already exists in this visit.");
            }

            var test = await _db.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                throw new InvalidOperationException("Test not found.");
            }

            var price = overridePrice ?? await ResolveTestPriceAsync(visit, test);
            var visitTest = new VisitTest
            {
                VisitId = visitId,
                TestId = testId,
                Price = price,
                Status = "Pending"
            };

            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();
            return visitTest;
        }

        public async Task RemoveVisitTestAsync(int visitTestId)
        {
            var visitTest = await _db.VisitTests
                .Include(vt => vt.Visit)
                .FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);
            if (visitTest == null)
            {
                return;
            }

            if (string.Equals(visitTest.Status, "Verified", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Verified tests cannot be removed.");
            }

            if (visitTest.Visit != null && string.Equals(visitTest.Visit.Status, VisitStatusClosed, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Closed visits cannot remove tests.");
            }

            _db.VisitTests.Remove(visitTest);
            await _db.SaveChangesAsync();
        }

        private async Task<decimal> ResolveTestPriceAsync(Visit visit, Test test)
        {
            PriceList? priceList = null;

            if (visit.ReferralId.HasValue)
            {
                priceList = await _db.PriceLists
                    .AsNoTracking()
                    .Where(p => p.ReferralId == visit.ReferralId)
                    .OrderByDescending(p => p.IsDefault)
                    .ThenBy(p => p.PriceListId)
                    .FirstOrDefaultAsync();
            }

            priceList ??= await _db.PriceLists
                .AsNoTracking()
                .Where(p => p.IsDefault)
                .OrderBy(p => p.PriceListId)
                .FirstOrDefaultAsync();

            if (priceList == null)
            {
                return test.Price;
            }

            var item = await _db.PriceListItems
                .AsNoTracking()
                .Where(i => i.PriceListId == priceList.PriceListId && i.TestId == test.TestId)
                .Select(i => new { i.Price })
                .FirstOrDefaultAsync();

            return item?.Price ?? test.Price;
        }
    }
}
