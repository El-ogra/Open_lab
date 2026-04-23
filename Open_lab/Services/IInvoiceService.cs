using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateOrUpdateInvoiceAsync(int visitId, decimal discount, decimal paid);
        Task<Payment> AddPaymentAsync(int invoiceId, decimal amount, string paymentMethod, int userId);
        Task<Payment> EditPaymentAsync(int paymentId, decimal newAmount, int userId, string reason);
        Task DeletePaymentAsync(int paymentId, int userId, string reason);
        Task<List<Payment>> GetPaymentsAsync(int invoiceId);
        Task<AdditionalCharge> AddAdditionalChargeAsync(int invoiceId, string description, decimal amount);
        Task<List<AdditionalCharge>> GetAdditionalChargesAsync(int invoiceId);
        Task<Invoice?> GetByVisitIdAsync(int visitId);
        Task<decimal> GetVisitTotalAsync(int visitId);
        Task<List<Invoice>> GetPatientInvoicesByDateAsync(int patientId, DateTime from, DateTime to);
        Task<List<Payment>> GetPatientPaymentsByDateAsync(int patientId, DateTime from, DateTime to);
        Task<Invoice> SettleAccountAsync(int visitId);

        /// <summary>
        /// Calculates the discount amount based on referral discount percentage.
        /// </summary>
        Task<decimal> CalculateReferralDiscountAsync(int referralId, decimal totalAmount);

        /// <summary>
        /// Calculates the commission amount based on referral commission percentage.
        /// </summary>
        Task<decimal> CalculateReferralCommissionAsync(int referralId, decimal totalAmount);
        
        /// <summary>
        /// Logs that an invoice has been printed to the audit trail.
        /// </summary>
        Task LogInvoicePrintedAsync(int invoiceId, int userId);
    }
}
