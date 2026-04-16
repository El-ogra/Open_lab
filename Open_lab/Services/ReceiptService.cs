using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly OpenLabDbContext _db;

        public ReceiptService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<ReceiptData?> GetReceiptDataAsync(int visitId)
        {
            var visit = await _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);

            if (visit == null)
            {
                return null;
            }

            var visitTests = await _db.VisitTests
                .AsNoTracking()
                .Include(vt => vt.Test)
                .Where(vt => vt.VisitId == visitId)
                .OrderBy(vt => vt.VisitTestId)
                .ToListAsync();

            var invoice = await _db.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.VisitId == visitId);

            return new ReceiptData
            {
                Visit = visit,
                Patient = visit.Patient,
                VisitTests = visitTests,
                Invoice = invoice
            };
        }
    }
}
