using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateOrUpdateInvoiceAsync(int visitId, decimal discount, decimal paid);
        Task<Payment> AddPaymentAsync(int invoiceId, decimal amount, int userId);
        Task DeletePaymentAsync(int paymentId);
        Task<List<Payment>> GetPaymentsAsync(int invoiceId);
        Task<Invoice?> GetByVisitIdAsync(int visitId);
        Task<decimal> GetVisitTotalAsync(int visitId);
    }
}
