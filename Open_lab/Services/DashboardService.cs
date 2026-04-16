using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly OpenLabDbContext _db;

        public DashboardService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<(int PatientCount, int VisitCount, int TestCount)> GetCountsAsync()
        {
            var patients = await _db.Patients.CountAsync();
            var visits = await _db.Visits.CountAsync();
            var tests = await _db.Tests.CountAsync();
            return (patients, visits, tests);
        }
    }
}
