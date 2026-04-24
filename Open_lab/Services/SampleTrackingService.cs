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
            if (sample != null)
            {
                sample.IsSeparated = isSeparated;
                sample.Status = isSeparated ? "مفصولة" : "مسحوبة";
                await _db.SaveChangesAsync();
            }
        }

        public Task<List<SampleCollection>> GetPendingTrackingSamplesAsync()
        {
            return _db.SampleCollections
                .Include(sc => sc.VisitTest)
                .ThenInclude(vt => vt.Visit)
                .ThenInclude(v => v.Patient)
                .Include(sc => sc.VisitTest)
                .ThenInclude(vt => vt.Test)
                .Where(sc => !sc.IsSeparated || sc.Status != "Verified") // Sample is still in process
                .OrderByDescending(sc => sc.CollectedAt)
                .ToListAsync();
        }
    }
}
