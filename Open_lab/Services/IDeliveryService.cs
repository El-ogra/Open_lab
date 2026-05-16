using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public interface IDeliveryService
    {
        Task<List<DeliveryVisitRow>> SearchAsync(DateTime from, DateTime to, string? keyword = null);
        Task<List<DeliveryVisitRow>> SearchAsync(DateTime from, DateTime to, string? keyword, DeliverySearchFilter filter);
        Task<List<VisitTestRow>> GetVisitTestsAsync(int visitId);
        Task DeliverAsync(int visitId, int userId);
        Task ReopenDeliveryAsync(int visitId);
    }

    public class DeliverySearchFilter
    {
        public bool VipOnly { get; set; }
        public bool PatientOnly { get; set; }
        public bool LabOnly { get; set; }
        public bool UndeliveredOnly { get; set; }
    }
}
