using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class UserProductivityService : IUserProductivityService
    {
        private readonly OpenLabDbContext _db;

        public UserProductivityService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserPerformanceRow>> GetUserPerformanceAsync(DateTime from, DateTime to)
        {
            // Calculate productivity based on number of tests that have a verified result by the user
            // We group by VisitTestId to count each test only once even if it has multiple parameters
            
            var verifications = await _db.ResultValues
                .AsNoTracking()
                .Where(rv => rv.VerifiedAt >= from && rv.VerifiedAt <= to && rv.VerifiedBy != null)
                .Select(rv => new { rv.VerifiedBy, rv.VisitTestId })
                .Distinct()
                .ToListAsync();

            var users = await _db.Users.AsNoTracking().ToListAsync();

            var performance = verifications
                .GroupBy(v => v.VerifiedBy)
                .Select(g => new UserPerformanceRow
                {
                    UserId = g.Key!.Value,
                    Username = users.FirstOrDefault(u => u.UserId == g.Key)?.Username ?? "Unknown",
                    CompletedTestsCount = g.Count()
                })
                .OrderByDescending(p => p.CompletedTestsCount)
                .ToList();

            return performance;
        }
    }
}
