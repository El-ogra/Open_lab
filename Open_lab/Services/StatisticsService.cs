using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly OpenLabDbContext _db;

        public StatisticsService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<StatisticsSummary> GetSummaryAsync(DateTime from, DateTime to)
        {
            return new StatisticsSummary
            {
                VisitCount = await _db.Visits.CountAsync(v => v.VisitDate >= from && v.VisitDate <= to),
                PatientCount = await _db.Patients.CountAsync(),
                TestCount = await _db.VisitTests.CountAsync(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to),
                TotalRevenue = await _db.Invoices.Where(i => i.Visit.VisitDate >= from && i.Visit.VisitDate <= to).SumAsync(i => (decimal?)i.NetTotal) ?? 0,
                TotalPaid = await _db.Invoices.Where(i => i.Visit.VisitDate >= from && i.Visit.VisitDate <= to).SumAsync(i => (decimal?)i.Paid) ?? 0
            };
        }
    }
}
