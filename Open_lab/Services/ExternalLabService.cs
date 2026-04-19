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
    }
}
