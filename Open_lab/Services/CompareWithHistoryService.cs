using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class CompareWithHistoryService : ICompareWithHistoryService
    {
        private readonly OpenLabDbContext _db;

        public CompareWithHistoryService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<HistoricalResult>> GetLastResultsAsync(int patientId, int testId, int count = 3)
        {
            // Get last N visits for this patient that contained this test
            var visitTests = await _db.VisitTests
                .AsNoTracking()
                .Include(vt => vt.Visit)
                .Include(vt => vt.ResultValues)
                .ThenInclude(rv => rv.Parameter)
                .Where(vt => vt.Visit.PatientId == patientId && vt.TestId == testId)
                .OrderByDescending(vt => vt.Visit.VisitDate)
                .Take(count)
                .ToListAsync();

            var history = new List<HistoricalResult>();

            foreach (var vt in visitTests)
            {
                foreach (var rv in vt.ResultValues)
                {
                    history.Add(new HistoricalResult
                    {
                        VisitDate = vt.Visit.VisitDate,
                        VisitId = vt.VisitId,
                        ParameterName = rv.Parameter.Name,
                        Value = rv.Value,
                        Flag = rv.Flag
                    });
                }
            }

            return history;
        }
    }
}
