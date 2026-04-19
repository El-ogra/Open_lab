using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IExternalLabService
    {
        Task<ExternalLabQueue> AddToQueueAsync(int visitTestId, int? referralId = null);
        Task<List<ExternalLabQueue>> GetPendingQueueAsync();
        Task<ShipmentManifest> CreateManifestAsync(int referralId, List<int> queueIds, string? courierNotes = null);
        Task<List<ShipmentManifest>> GetAllManifestsAsync();
        Task UpdateQueueStatusAsync(int queueId, string status, string? externalRef = null);
    }
}
