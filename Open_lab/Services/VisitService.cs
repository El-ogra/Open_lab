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

            var patient = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == visit.PatientId);
            if (patient == null)
            {
                throw new InvalidOperationException("Patient not found.");
            }

            if (visit.VisitDate == default)
            {
                visit.VisitDate = DateTime.Now;
            }

            if (!visit.ReferralId.HasValue && patient.ReferralId.HasValue)
            {
                visit.ReferralId = patient.ReferralId.Value;
            }

            visit.AccountType = string.IsNullOrWhiteSpace(visit.AccountType) ? "Cash" : visit.AccountType.Trim();
            if (visit.ReferralId.HasValue && !string.Equals(visit.AccountType, "Referral", StringComparison.OrdinalIgnoreCase))
            {
                visit.AccountType = "Referral";
            }

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

            if (test.IsSendOut)
            {
                await EnsureExternalQueueRegistrationAsync(visitTest.VisitTestId, visit.ReferralId);
            }

            return visitTest;
        }

        public async Task<List<VisitTest>> AddCustomGroupToVisitAsync(int visitId, int customGroupId)
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

            var customGroup = await _db.CustomGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.CustomGroupId == customGroupId);
            if (customGroup == null)
            {
                throw new InvalidOperationException("Custom group not found.");
            }

            var groupTestIds = await _db.CustomGroupItems
                .AsNoTracking()
                .Where(i => i.CustomGroupId == customGroupId)
                .Select(i => i.TestId)
                .Distinct()
                .ToListAsync();

            if (groupTestIds.Count == 0)
            {
                throw new InvalidOperationException("Custom group has no tests.");
            }

            var added = new List<VisitTest>();
            foreach (var testId in groupTestIds)
            {
                var existsInVisit = await _db.VisitTests.AnyAsync(vt => vt.VisitId == visitId && vt.TestId == testId);
                if (existsInVisit)
                {
                    continue;
                }

                var visitTest = await AddTestToVisitAsync(visitId, testId);
                added.Add(visitTest);
            }

            return added;
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

            var hasEnteredResult = await _db.ResultValues.AnyAsync(r =>
                r.VisitTestId == visitTestId &&
                (!string.IsNullOrWhiteSpace(r.Value) ||
                 !string.IsNullOrWhiteSpace(r.Flag) ||
                 !string.IsNullOrWhiteSpace(r.Comment)));
            if (hasEnteredResult)
            {
                throw new InvalidOperationException("Tests cannot be removed after result entry.");
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

        private async Task EnsureExternalQueueRegistrationAsync(int visitTestId, int? referralId)
        {
            var exists = await _db.ExternalLabQueues.AnyAsync(q => q.VisitTestId == visitTestId);
            if (exists)
            {
                return;
            }

            _db.ExternalLabQueues.Add(new ExternalLabQueue
            {
                VisitTestId = visitTestId,
                ReferralId = referralId,
                Status = "Pending",
                DateQueued = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }
    }
}
