using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class ResultsService : IResultsService
    {
        private readonly OpenLabDbContext _db;

        public ResultsService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<List<VisitTest>> GetVisitTestsByDateAsync(DateTime from, DateTime to)
        {
            return _db.VisitTests
                .AsNoTracking()
                .Include(vt => vt.Visit)
                .Include(vt => vt.Test)
                .Where(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to)
                .OrderBy(vt => vt.Visit.VisitDate)
                .ToListAsync();
        }

        public Task<List<ResultValue>> GetResultsForVisitTestAsync(int visitTestId)
        {
            return _db.ResultValues
                .AsNoTracking()
                .Include(rv => rv.Parameter)
                .Where(rv => rv.VisitTestId == visitTestId)
                .ToListAsync();
        }

        public async Task SaveResultAsync(int visitTestId, int parameterId, string? value, string? flag, string? comment)
        {
            var existing = await _db.ResultValues
                .FirstOrDefaultAsync(rv => rv.VisitTestId == visitTestId && rv.ParameterId == parameterId);

            if (existing == null)
            {
                existing = new ResultValue
                {
                    VisitTestId = visitTestId,
                    ParameterId = parameterId,
                    Value = value,
                    Flag = flag,
                    Comment = comment
                };
                _db.ResultValues.Add(existing);
            }
            else
            {
                existing.Value = value;
                existing.Flag = flag;
                existing.Comment = comment;
            }

            await _db.SaveChangesAsync();
        }

        public async Task VerifyVisitTestAsync(int visitTestId, int verifiedByUserId)
        {
            var results = await _db.ResultValues.Where(rv => rv.VisitTestId == visitTestId).ToListAsync();
            if (results.Count == 0)
            {
                throw new InvalidOperationException("No results to verify.");
            }

            var now = DateTime.Now;
            foreach (var result in results)
            {
                result.VerifiedBy = verifiedByUserId;
                result.VerifiedAt = now;
            }

            var visitTest = await _db.VisitTests.FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);
            if (visitTest != null)
            {
                visitTest.Status = "Verified";
            }

            await _db.SaveChangesAsync();
        }
    }
}
