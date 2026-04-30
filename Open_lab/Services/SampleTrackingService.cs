using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class SampleTrackingService : ISampleTrackingService
    {
        private readonly OpenLabDbContext _db;

        public SampleTrackingService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<SampleCollection?> GetSampleStatusAsync(int visitTestId)
        {
            return _db.SampleCollections
                .Include(sc => sc.CollectedByUser)
                .Include(sc => sc.ReceivedByUser)
                .FirstOrDefaultAsync(sc => sc.VisitTestId == visitTestId);
        }

        public async Task UpdateSeparationStatusAsync(int visitTestId, bool isSeparated)
        {
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(sc => sc.VisitTestId == visitTestId);
            if (sample == null)
            {
                throw new InvalidOperationException("لم يتم العثور على سجل العينة المطلوب.");
            }

            sample.IsSeparated = isSeparated;
            sample.Status = isSeparated ? "مفصولة" : "مسحوبة";
            await _db.SaveChangesAsync();
        }

        public Task<List<SampleCollection>> GetPendingTrackingSamplesAsync()
        {
            return _db.SampleCollections
                .Include(sc => sc.VisitTest)
                .ThenInclude(vt => vt.Visit)
                .ThenInclude(v => v.Patient)
                .Include(sc => sc.VisitTest)
                .ThenInclude(vt => vt.Test)
                .Where(sc => !sc.IsSeparated)
                .OrderByDescending(sc => sc.CollectedAt)
                .ToListAsync();
        }
    }
}
