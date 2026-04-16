using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public class WorksheetService : IWorksheetService
    {
        private readonly OpenLabDbContext _db;

        public WorksheetService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<List<WorkSheetPatientRow>> GetWorksheetByPatientAsync(DateTime from, DateTime to)
        {
            return _db.Visits
                .AsNoTracking()
                .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                .Select(v => new WorkSheetPatientRow
                {
                    VisitId = v.VisitId,
                    PatientName = v.Patient.FullName,
                    VisitDate = v.VisitDate,
                    TestsCount = v.VisitTests.Count
                })
                .OrderBy(v => v.VisitDate)
                .ToListAsync();
        }

        public Task<List<WorkSheetTestRow>> GetWorksheetByTestAsync(DateTime from, DateTime to)
        {
            return _db.VisitTests
                .AsNoTracking()
                .Where(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to)
                .GroupBy(vt => vt.Test.NameReport)
                .Select(g => new WorkSheetTestRow
                {
                    TestName = g.Key,
                    Count = g.Count()
                })
                .OrderBy(r => r.TestName)
                .ToListAsync();
        }
    }
}
