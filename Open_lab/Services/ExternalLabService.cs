using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class ExternalLabService : IExternalLabService
    {
        private readonly OpenLabDbContext _db;

        public ExternalLabService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<ExternalLabQueue> AddToQueueAsync(int visitTestId, int? referralId = null)
        {
            var visitTestExists = await _db.VisitTests.AnyAsync(vt => vt.VisitTestId == visitTestId);
            if (!visitTestExists)
            {
                throw new InvalidOperationException("Visit test not found.");
            }

            var existing = await _db.ExternalLabQueues.FirstOrDefaultAsync(q => q.VisitTestId == visitTestId);
            if (existing != null) return existing;

            var queueItem = new ExternalLabQueue
            {
                VisitTestId = visitTestId,
                ReferralId = referralId,
                Status = "Pending",
                DateQueued = DateTime.Now
            };

            _db.ExternalLabQueues.Add(queueItem);
            await _db.SaveChangesAsync();
            return queueItem;
        }

        public Task<List<ExternalLabQueue>> GetPendingQueueAsync()
        {
            return _db.ExternalLabQueues
                .Include(q => q.VisitTest)
                .ThenInclude(vt => vt.Test)
                .Include(q => q.VisitTest)
                .ThenInclude(vt => vt.Visit)
                .ThenInclude(v => v.Patient)
                .Include(q => q.Referral)
                .Where(q => q.Status == "Pending")
                .OrderBy(q => q.DateQueued)
                .ToListAsync();
        }

        public async Task<ShipmentManifest> CreateManifestAsync(int referralId, List<int> queueIds, string? courierNotes = null)
        {
            var manifestNumber = $"MAN-{DateTime.Now:yyyyMMddHHmmss}";
            var manifest = new ShipmentManifest
            {
                ManifestNumber = manifestNumber,
                ReferralId = referralId,
                DateCreated = DateTime.Now,
                Status = "Open",
                CourierNotes = courierNotes
            };

            _db.ShipmentManifests.Add(manifest);
            await _db.SaveChangesAsync();

            foreach (var qId in queueIds)
            {
                var item = new ShipmentItem
                {
                    ManifestId = manifest.ManifestId,
                    QueueId = qId
                };
                _db.ShipmentItems.Add(item);

                var queueItem = await _db.ExternalLabQueues.FindAsync(qId);
                if (queueItem != null)
                {
                    queueItem.Status = "InManifest";
                }
            }

            await _db.SaveChangesAsync();
            return manifest;
        }

        public Task<List<ShipmentManifest>> GetAllManifestsAsync()
        {
            return _db.ShipmentManifests
                .Include(m => m.Referral)
                .Include(m => m.Items)
                .OrderByDescending(m => m.DateCreated)
                .ToListAsync();
        }

        public async Task UpdateQueueStatusAsync(int queueId, string status, string? externalRef = null)
        {
            var item = await _db.ExternalLabQueues.FindAsync(queueId);
            if (item != null)
            {
                item.Status = status;
                if (!string.IsNullOrEmpty(externalRef))
                {
                    item.ExternalReference = externalRef;
                }
                await _db.SaveChangesAsync();
            }
        }

        public async Task EnterExternalLabResultAsync(int queueId, string resultValue, string? comment = null, string? externalRef = null)
        {
            if (string.IsNullOrWhiteSpace(resultValue))
            {
                throw new ArgumentException("Result value is required.", nameof(resultValue));
            }

            var queueItem = await _db.ExternalLabQueues
                .Include(q => q.VisitTest)
                .ThenInclude(vt => vt.Test)
                .FirstOrDefaultAsync(q => q.QueueId == queueId);

            if (queueItem == null)
            {
                throw new InvalidOperationException("External queue item not found.");
            }

            var visitTest = queueItem.VisitTest;
            var parameter = await _db.TestParameters
                .FirstOrDefaultAsync(p => p.TestId == visitTest.TestId && p.Name == "External Lab Result");

            if (parameter == null)
            {
                var maxOrder = await _db.TestParameters
                    .Where(p => p.TestId == visitTest.TestId)
                    .Select(p => (int?)p.OrderNo)
                    .MaxAsync() ?? 0;

                parameter = new TestParameter
                {
                    TestId = visitTest.TestId,
                    Name = "External Lab Result",
                    OrderNo = maxOrder + 1
                };
                _db.TestParameters.Add(parameter);
                await _db.SaveChangesAsync();
            }

            var existingResult = await _db.ResultValues
                .FirstOrDefaultAsync(r => r.VisitTestId == visitTest.VisitTestId && r.ParameterId == parameter.ParameterId);

            if (existingResult == null)
            {
                _db.ResultValues.Add(new ResultValue
                {
                    VisitTestId = visitTest.VisitTestId,
                    ParameterId = parameter.ParameterId,
                    Value = resultValue.Trim(),
                    Comment = comment
                });
            }
            else
            {
                existingResult.Value = resultValue.Trim();
                existingResult.Comment = comment;
                existingResult.Flag = null;
                existingResult.VerifiedAt = null;
                existingResult.VerifiedBy = null;
            }

            visitTest.Status = "InProgress";
            queueItem.Status = "ResultReceived";
            if (!string.IsNullOrWhiteSpace(externalRef))
            {
                queueItem.ExternalReference = externalRef.Trim();
            }

            await _db.SaveChangesAsync();
        }
    }
}
