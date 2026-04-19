using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class TestClassificationService : ITestClassificationService
    {
        private readonly OpenLabDbContext _db;

        public TestClassificationService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<ReagentConsumptionReport>> GetConsumptionReportAsync(DateTime from, DateTime to)
        {
            // Get all tests performed in the period and group by TestId
            var performedTests = await _db.VisitTests
                .AsNoTracking()
                .Where(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to)
                .GroupBy(vt => vt.TestId)
                .Select(g => new { TestId = g.Key, Count = g.Count() })
                .ToListAsync();

            var reports = new List<ReagentConsumptionReport>();

            foreach (var testStat in performedTests)
            {
                // Find what reagents this test consumes
                var consumptions = await _db.TestConsumptions
                    .Include(tc => tc.Reagent)
                    .Where(tc => tc.TestId == testStat.TestId)
                    .ToListAsync();

                foreach (var cons in consumptions)
                {
                    var existing = reports.FirstOrDefault(r => r.ReagentName == cons.Reagent.Name);
                    if (existing == null)
                    {
                        reports.Add(new ReagentConsumptionReport
                        {
                            ReagentName = cons.Reagent.Name,
                            Unit = cons.Reagent.Unit,
                            TotalConsumed = cons.AmountPerTest * testStat.Count,
                            TestCount = testStat.Count
                        });
                    }
                    else
                    {
                        existing.TotalConsumed += (cons.AmountPerTest * testStat.Count);
                        existing.TestCount += testStat.Count;
                    }
                }
            }

            return reports.OrderBy(r => r.ReagentName).ToList();
        }
    }
}
