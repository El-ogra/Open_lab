using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public class SampleCollectionService : ISampleCollectionService
    {
        private readonly OpenLabDbContext _db;

        public SampleCollectionService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<SampleCollectionRow>> GetRowsAsync(DateTime from, DateTime to)
        {
            var visitTests = await _db.VisitTests
                .Include(vt => vt.Visit)
                .ThenInclude(v => v.Patient)
                .Include(vt => vt.Test)
                .Include(vt => vt.SampleCollection)
                .ThenInclude(sc => sc!.CollectedByUser)
                .Where(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to)
                .OrderByDescending(vt => vt.Visit.VisitDate)
                .ToListAsync();

            return visitTests.Select(vt =>
            {
                var sc = vt.SampleCollection;
                return new SampleCollectionRow
                {
                    VisitTestId = vt.VisitTestId,
                    PatientName = vt.Visit.Patient.FullName,
                    TestName = vt.Test.NameReport,
                    VisitDate = vt.Visit.VisitDate,
                    Status = sc?.Status ?? "غير مسحوبة",
                    CollectedAt = sc?.CollectedAt,
                    CollectedBy = sc?.CollectedByUser?.Username
                };
            }).ToList();
        }

        public async Task MarkCollectedAsync(int visitTestId, int userId, bool isExternal = false, int? receivedBy = null)
        {
            var vt = await _db.VisitTests.Include(v => v.SampleCollection).FirstOrDefaultAsync(v => v.VisitTestId == visitTestId);
            if (vt == null)
            {
                throw new InvalidOperationException("العنصر غير موجود.");
            }

            if (vt.SampleCollection == null)
            {
                vt.SampleCollection = new SampleCollection
                {
                    VisitTestId = vt.VisitTestId,
                    CollectedBy = userId,
                    CollectedAt = DateTime.Now,
                    Status = "مسحوبة",
                    IsExternalSample = isExternal,
                    ReceivedBy = receivedBy
                };
                _db.SampleCollections.Add(vt.SampleCollection);
            }
            else
            {
                vt.SampleCollection.CollectedBy = userId;
                vt.SampleCollection.CollectedAt = DateTime.Now;
                vt.SampleCollection.Status = "مسحوبة";
                vt.SampleCollection.IsExternalSample = isExternal;
                vt.SampleCollection.ReceivedBy = receivedBy;
            }

            await _db.SaveChangesAsync();
        }

        public async Task MarkSeparatedAsync(int visitTestId, string? separationType = null)
        {
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == visitTestId);
            if (sample == null)
            {
                throw new InvalidOperationException("يجب تسجيل السحب أولاً قبل الفصل.");
            }

            sample.IsSeparated = true;
            sample.Status = string.IsNullOrWhiteSpace(separationType)
                ? "مفصولة"
                : $"مفصولة - {separationType.Trim()}";
            await _db.SaveChangesAsync();
        }

        public async Task MarkNotCollectedAsync(int visitTestId)
        {
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == visitTestId);
            if (sample == null)
            {
                return;
            }

            _db.SampleCollections.Remove(sample);
            await _db.SaveChangesAsync();
        }
    }
}
