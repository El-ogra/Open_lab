using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    /// <summary>
    /// Gap 4.5 — Arrange Report Order Persistence.
    /// Persists the report ordering using the new VisitTest.ReportOrder column so the
    /// arrangement survives across sessions instead of living only inside an
    /// ObservableCollection.Move in the ViewModel.
    /// </summary>
    public class ReportOrderService : IReportOrderService
    {
        private readonly OpenLabDbContext _db;

        public ReportOrderService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task SaveReportOrderAsync(int visitId, IReadOnlyList<int> orderedVisitTestIds)
        {
            if (orderedVisitTestIds == null || orderedVisitTestIds.Count == 0)
            {
                return;
            }

            var visitTests = await _db.VisitTests
                .Where(vt => vt.VisitId == visitId)
                .ToListAsync();

            // Assign 1-based ordering for items in the supplied list.
            for (var i = 0; i < orderedVisitTestIds.Count; i++)
            {
                var id = orderedVisitTestIds[i];
                var vt = visitTests.FirstOrDefault(x => x.VisitTestId == id);
                if (vt != null)
                {
                    vt.ReportOrder = i + 1;
                }
            }

            // Any visit test not present in the supplied order keeps its previous order
            // (or 0) — we do not mutate unrelated rows.

            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<int>> GetReportOrderAsync(int visitId)
        {
            var ordered = await _db.VisitTests
                .Where(vt => vt.VisitId == visitId && vt.ReportOrder > 0)
                .OrderBy(vt => vt.ReportOrder)
                .Select(vt => vt.VisitTestId)
                .ToListAsync();

            return ordered;
        }
    }
}
