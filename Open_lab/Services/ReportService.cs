using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class ReportService : IReportService
    {
        private readonly OpenLabDbContext _db;

        public ReportService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<VisitReportData?> GetVisitReportAsync(int visitId)
        {
            var visit = await _db.Visits
                .AsNoTracking()
                .Include(v => v.Patient)
                .Include(v => v.VisitTests)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);

            if (visit == null)
            {
                return null;
            }

            var invoice = await _db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.VisitId == visitId);

            var report = new VisitReportData
            {
                Visit = visit,
                Patient = visit.Patient,
                Invoice = invoice
            };

            var visitTests = await _db.VisitTests
                .AsNoTracking()
                .Include(vt => vt.Test)
                .Where(vt => vt.VisitId == visitId)
                .OrderBy(vt => vt.Test.ReportOrder)
                .ThenBy(vt => vt.Test.NameReport)
                .ToListAsync();

            foreach (var vt in visitTests)
            {
                var results = await _db.ResultValues
                    .AsNoTracking()
                    .Include(rv => rv.Parameter)
                    .Where(rv => rv.VisitTestId == vt.VisitTestId)
                    .ToListAsync();

                report.Tests.Add(new VisitTestReportItem
                {
                    VisitTest = vt,
                    Test = vt.Test,
                    Results = results
                });
            }

            return report;
        }

        public async Task<VisitReportData?> GetCompositeReportAsync(int visitId, IReadOnlyCollection<int>? orderedVisitTestIds = null)
        {
            var report = await GetVisitReportAsync(visitId);
            if (report == null)
            {
                return null;
            }

            if (orderedVisitTestIds == null || orderedVisitTestIds.Count == 0)
            {
                return report;
            }

            var ordered = orderedVisitTestIds.ToList();
            var lookup = report.Tests.ToDictionary(t => t.VisitTest.VisitTestId, t => t);
            var composite = new List<VisitTestReportItem>();

            foreach (var visitTestId in ordered)
            {
                if (lookup.TryGetValue(visitTestId, out var item))
                {
                    composite.Add(item);
                }
            }

            foreach (var item in report.Tests)
            {
                if (!composite.Any(c => c.VisitTest.VisitTestId == item.VisitTest.VisitTestId))
                {
                    composite.Add(item);
                }
            }

            report.Tests = composite;
            return report;
        }

        public async Task<PatientHistoryReportData> GetPatientHistoryAsync(int patientId, DateTime? from, DateTime? to)
        {
            var patient = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == patientId);
            if (patient == null)
            {
                throw new InvalidOperationException("Patient not found.");
            }

            var query = _db.Visits.AsNoTracking().Where(v => v.PatientId == patientId);

            if (from.HasValue)
            {
                query = query.Where(v => v.VisitDate >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(v => v.VisitDate <= to.Value);
            }

            var visits = await query.OrderByDescending(v => v.VisitDate).ToListAsync();

            var history = new PatientHistoryReportData
            {
                Patient = patient
            };

            foreach (var visit in visits)
            {
                var report = await GetVisitReportAsync(visit.VisitId);
                if (report != null)
                {
                    history.Visits.Add(report);
                }
            }

            return history;
        }
    }
}
