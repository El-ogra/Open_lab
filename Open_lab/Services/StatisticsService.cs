using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly OpenLabDbContext _db;

        public StatisticsService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<List<StatisticsReferralLookup>> GetReferralsAsync()
        {
            return _db.Referrals
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new StatisticsReferralLookup
                {
                    ReferralId = r.ReferralId,
                    Name = r.Name
                })
                .ToListAsync();
        }

        public async Task<StatisticsSnapshot> GetSnapshotAsync(DateTime from, DateTime to, string? gender, int? referralId)
        {
            var filteredVisits = BuildFilteredVisitsQuery(from, to, gender, referralId);
            var filteredVisitIds = filteredVisits.Select(v => v.VisitId);

            var summary = new StatisticsSummary
            {
                VisitCount = await filteredVisits.CountAsync(),
                PatientCount = await filteredVisits.Select(v => v.PatientId).Distinct().CountAsync(),
                TestCount = await _db.VisitTests.CountAsync(vt => filteredVisitIds.Contains(vt.VisitId)),
                TotalRevenue = await _db.Invoices.Where(i => filteredVisitIds.Contains(i.VisitId)).SumAsync(i => (decimal?)i.NetTotal) ?? 0,
                TotalPaid = await _db.Invoices.Where(i => filteredVisitIds.Contains(i.VisitId)).SumAsync(i => (decimal?)i.Paid) ?? 0
            };

            var byGenderVisits = await filteredVisits
                .GroupBy(v => string.IsNullOrWhiteSpace(v.Patient.Gender) ? "غير محدد" : v.Patient.Gender)
                .Select(g => new { Key = g.Key, Visits = g.Count() })
                .ToListAsync();

            var byGenderTests = await (
                from vt in _db.VisitTests
                join v in filteredVisits on vt.VisitId equals v.VisitId
                group vt by (string.IsNullOrWhiteSpace(v.Patient.Gender) ? "غير محدد" : v.Patient.Gender)
                into g
                select new { Key = g.Key, Tests = g.Count() })
                .ToListAsync();

            var byGenderRevenue = await (
                from i in _db.Invoices
                join v in filteredVisits on i.VisitId equals v.VisitId
                group i by (string.IsNullOrWhiteSpace(v.Patient.Gender) ? "غير محدد" : v.Patient.Gender)
                into g
                select new { Key = g.Key, Revenue = g.Sum(x => x.NetTotal) })
                .ToListAsync();

            var genderKeys = byGenderVisits.Select(x => x.Key)
                .Union(byGenderTests.Select(x => x.Key))
                .Union(byGenderRevenue.Select(x => x.Key))
                .ToList();

            var byGender = genderKeys
                .Select(key => new StatisticsGenderRow
                {
                    Gender = key,
                    VisitsCount = byGenderVisits.FirstOrDefault(x => x.Key == key)?.Visits ?? 0,
                    TestsCount = byGenderTests.FirstOrDefault(x => x.Key == key)?.Tests ?? 0,
                    Revenue = byGenderRevenue.FirstOrDefault(x => x.Key == key)?.Revenue ?? 0
                })
                .OrderByDescending(r => r.VisitsCount)
                .ToList();

            var byReferralVisits = await filteredVisits
                .GroupBy(v => v.Referral != null ? v.Referral.Name : "بدون جهة")
                .Select(g => new { Key = g.Key, Visits = g.Count() })
                .ToListAsync();

            var byReferralTests = await (
                from vt in _db.VisitTests
                join v in filteredVisits on vt.VisitId equals v.VisitId
                group vt by (v.Referral != null ? v.Referral.Name : "بدون جهة")
                into g
                select new { Key = g.Key, Tests = g.Count() })
                .ToListAsync();

            var byReferralFinance = await (
                from i in _db.Invoices
                join v in filteredVisits on i.VisitId equals v.VisitId
                group i by (v.Referral != null ? v.Referral.Name : "بدون جهة")
                into g
                select new { Key = g.Key, Revenue = g.Sum(x => x.NetTotal), Paid = g.Sum(x => x.Paid) })
                .ToListAsync();

            var referralKeys = byReferralVisits.Select(x => x.Key)
                .Union(byReferralTests.Select(x => x.Key))
                .Union(byReferralFinance.Select(x => x.Key))
                .ToList();

            var byReferral = referralKeys
                .Select(key => new StatisticsReferralRow
                {
                    ReferralName = key,
                    VisitsCount = byReferralVisits.FirstOrDefault(x => x.Key == key)?.Visits ?? 0,
                    TestsCount = byReferralTests.FirstOrDefault(x => x.Key == key)?.Tests ?? 0,
                    Revenue = byReferralFinance.FirstOrDefault(x => x.Key == key)?.Revenue ?? 0,
                    Paid = byReferralFinance.FirstOrDefault(x => x.Key == key)?.Paid ?? 0
                })
                .OrderByDescending(r => r.Revenue)
                .ToList();

            return new StatisticsSnapshot
            {
                Summary = summary,
                ByGender = byGender,
                ByReferral = byReferral
            };
        }

        private IQueryable<Models.Visit> BuildFilteredVisitsQuery(DateTime from, DateTime to, string? gender, int? referralId)
        {
            var visits = _db.Visits
                .AsNoTracking()
                .Where(v => v.VisitDate >= from && v.VisitDate <= to);

            if (!string.IsNullOrWhiteSpace(gender) && !string.Equals(gender, "الكل", StringComparison.OrdinalIgnoreCase))
            {
                visits = visits.Where(v => v.Patient.Gender == gender);
            }

            if (referralId.HasValue)
            {
                visits = visits.Where(v => v.ReferralId == referralId.Value);
            }

            return visits;
        }
    }
}
