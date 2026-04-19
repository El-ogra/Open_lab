using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public class GroupWorksheetService : IGroupWorksheetService
    {
        private readonly OpenLabDbContext _db;

        public GroupWorksheetService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<WorkSheetPatientRow>> GetGroupWorksheetByGroupAsync(int groupId, DateTime from, DateTime to)
        {
            var testsInGroup = await _db.Tests
                .Where(t => t.GroupId == groupId)
                .Select(t => t.TestId)
                .ToListAsync();

            return await _db.Visits
                .AsNoTracking()
                .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                .Where(v => v.VisitTests.Any(vt => testsInGroup.Contains(vt.TestId)))
                .Select(v => new WorkSheetPatientRow
                {
                    VisitId = v.VisitId,
                    PatientName = v.Patient.FullName,
                    VisitDate = v.VisitDate,
                    TestsCount = v.VisitTests.Count(vt => testsInGroup.Contains(vt.TestId))
                })
                .OrderBy(v => v.VisitDate)
                .ToListAsync();
        }

        public async Task<List<WorkSheetPatientRow>> GetGroupWorksheetByCustomGroupAsync(int customGroupId, DateTime from, DateTime to)
        {
            var testsInProfile = await _db.CustomGroupItems
                .Where(cgi => cgi.CustomGroupId == customGroupId)
                .Select(cgi => cgi.TestId)
                .ToListAsync();

            return await _db.Visits
                .AsNoTracking()
                .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                .Where(v => v.VisitTests.Any(vt => testsInProfile.Contains(vt.TestId)))
                .Select(v => new WorkSheetPatientRow
                {
                    VisitId = v.VisitId,
                    PatientName = v.Patient.FullName,
                    VisitDate = v.VisitDate,
                    TestsCount = v.VisitTests.Count(vt => testsInProfile.Contains(vt.TestId))
                })
                .OrderBy(v => v.VisitDate)
                .ToListAsync();
        }
    }
}
