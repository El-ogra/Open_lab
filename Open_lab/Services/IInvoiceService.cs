using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateOrUpdateInvoiceAsync(int visitId, decimal discount, decimal paid);
        Task<Payment> AddPaymentAsync(int invoiceId, decimal amount, int userId);
        Task<Payment> EditPaymentAsync(int paymentId, decimal newAmount, int userId);
        Task DeletePaymentAsync(int paymentId);
        Task<List<Payment>> GetPaymentsAsync(int invoiceId);
        Task<Invoice?> GetByVisitIdAsync(int visitId);
        Task<decimal> GetVisitTotalAsync(int visitId);
        Task<List<Invoice>> GetPatientInvoicesByDateAsync(int patientId, DateTime from, DateTime to);
        Task<List<Payment>> GetPatientPaymentsByDateAsync(int patientId, DateTime from, DateTime to);

        /// <summary>
        /// Calculates the discount amount based on referral discount percentage.
        /// </summary>
        Task<decimal> CalculateReferralDiscountAsync(int referralId, decimal totalAmount);

        /// <summary>
        /// Calculates the commission amount based on referral commission percentage.
        /// </summary>
        Task<decimal> CalculateReferralCommissionAsync(int referralId, decimal totalAmount);
    }
}
