using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public interface IDeliveryService
    {
        Task<List<DeliveryVisitRow>> SearchAsync(DateTime from, DateTime to, string? keyword = null);
        Task DeliverAsync(int visitId, int userId);
        Task ReopenDeliveryAsync(int visitId);
    }
}
