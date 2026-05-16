using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly OpenLabDbContext _db;

        public DeliveryService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<DeliveryVisitRow>> SearchAsync(DateTime from, DateTime to, string? keyword = null)
        {
            return await SearchAsync(from, to, keyword, new DeliverySearchFilter());
        }

        public async Task<List<DeliveryVisitRow>> SearchAsync(DateTime from, DateTime to, string? keyword, DeliverySearchFilter filter)
        {
            var query = _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .Include(v => v.Referral)
                .Include(v => v.Invoice)
                .Include(v => v.VisitTests)
                .Where(v => v.VisitDate >= from && v.VisitDate <= to);

            filter ??= new DeliverySearchFilter();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                query = query.Where(v =>
                    v.Patient.FullName.Contains(term) ||
                    v.Patient.LabId.Contains(term) ||
                    v.VisitId.ToString().Contains(term));
            }

            if (filter.VipOnly)
            {
                query = query.Where(v => v.Patient.IsVip);
            }

            if (filter.PatientOnly)
            {
                query = query.Where(v => string.IsNullOrWhiteSpace(v.AccountType) ||
                    v.AccountType == "Cash" ||
                    v.AccountType == "Individual");
            }

            if (filter.LabOnly)
            {
                query = query.Where(v => v.ReferralId.HasValue ||
                    v.AccountType == "Referral" ||
                    v.AccountType == "Lab to Lab");
            }

            var visits = await query
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();

            var rows = visits.Select(v =>
            {
                var testsCount = v.VisitTests.Count;
                var verifiedCount = v.VisitTests.Count(t => string.Equals(t.Status, "Verified", StringComparison.OrdinalIgnoreCase));
                var deliveredCount = v.VisitTests.Count(t => string.Equals(t.Status, "Delivered", StringComparison.OrdinalIgnoreCase));
                var hasPending = v.VisitTests.Any(t =>
                    !string.Equals(t.Status, "Verified", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(t.Status, "Delivered", StringComparison.OrdinalIgnoreCase));

                var balance = v.Invoice?.Balance ?? 0m;
                var isDelivered = string.Equals(v.Status, "Completed", StringComparison.OrdinalIgnoreCase) || deliveredCount == testsCount;
                var isReady = testsCount > 0 && !hasPending && balance <= 0m;

                return new DeliveryVisitRow
                {
                    VisitId = v.VisitId,
                    LabId = v.Patient.LabId,
                    PatientName = v.Patient.FullName,
                    VisitDate = v.VisitDate,
                    VisitStatus = v.Status,
                    TestsCount = testsCount,
                    VerifiedCount = verifiedCount,
                    DeliveredCount = deliveredCount,
                    Balance = balance,
                    IsReadyForDelivery = isReady,
                    IsDelivered = isDelivered
                };
            }).ToList();

            if (filter.UndeliveredOnly)
            {
                rows = rows.Where(r => !r.IsDelivered).ToList();
            }

            return rows;
        }

        public async Task<List<VisitTestRow>> GetVisitTestsAsync(int visitId)
        {
            var tests = await _db.VisitTests
                .AsNoTracking()
                .Include(t => t.Test)
                .Include(t => t.Visit)
                .ThenInclude(v => v.Patient)
                .Include(t => t.ResultValues)
                .Where(t => t.VisitId == visitId)
                .ToListAsync();

            return tests.Select(t => new VisitTestRow
            {
                VisitTestId = t.VisitTestId,
                TestId = t.TestId,
                TestName = t.Test.NameReport,
                Status = t.Status,
                ResultValue = t.ResultValues.FirstOrDefault()?.Value,
                IsFinished = string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase) || string.Equals(t.Status, "Verified", StringComparison.OrdinalIgnoreCase) || string.Equals(t.Status, "Delivered", StringComparison.OrdinalIgnoreCase),
                IsVerified = string.Equals(t.Status, "Verified", StringComparison.OrdinalIgnoreCase) || string.Equals(t.Status, "Delivered", StringComparison.OrdinalIgnoreCase),
                Price = t.Price,
                PatientId = t.Visit.PatientId,
                PatientName = t.Visit.Patient.FullName,
                PatientAge = t.Visit.Patient.Age ?? 0,
                PatientGender = t.Visit.Patient.Gender,
                VisitDate = t.Visit.VisitDate,
                ShouldPrint = false,
                ShouldExport = false
            }).ToList();
        }

        public async Task DeliverAsync(int visitId, int userId)
        {
            var visit = await _db.Visits
                .Include(v => v.Invoice)
                .Include(v => v.VisitTests)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);

            if (visit == null)
            {
                throw new InvalidOperationException("الزيارة غير موجودة.");
            }

            if (visit.VisitTests.Count == 0)
            {
                throw new InvalidOperationException("لا توجد تحاليل داخل الزيارة.");
            }

            var hasNotVerified = visit.VisitTests.Any(t =>
                !string.Equals(t.Status, "Verified", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(t.Status, "Delivered", StringComparison.OrdinalIgnoreCase));
            if (hasNotVerified)
            {
                throw new InvalidOperationException("لا يمكن التسليم قبل اعتماد جميع النتائج.");
            }

            if ((visit.Invoice?.Balance ?? 0m) > 0m)
            {
                throw new InvalidOperationException("يوجد متبقي مالي. أكمل التسوية أولاً.");
            }

            foreach (var test in visit.VisitTests)
            {
                test.Status = "Delivered";
            }

            visit.Status = "Completed";
            if (visit.Invoice != null)
            {
                visit.Invoice.Status = "Closed";
            }

            await _db.SaveChangesAsync();
        }

        public async Task ReopenDeliveryAsync(int visitId)
        {
            var visit = await _db.Visits
                .Include(v => v.Invoice)
                .Include(v => v.VisitTests)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);

            if (visit == null)
            {
                throw new InvalidOperationException("الزيارة غير موجودة.");
            }

            foreach (var test in visit.VisitTests)
            {
                if (string.Equals(test.Status, "Delivered", StringComparison.OrdinalIgnoreCase))
                {
                    test.Status = "Verified";
                }
            }

            visit.Status = "Open";
            if (visit.Invoice != null && string.Equals(visit.Invoice.Status, "Closed", StringComparison.OrdinalIgnoreCase))
            {
                visit.Invoice.Status = "Open";
            }

            await _db.SaveChangesAsync();
        }
    }
}

