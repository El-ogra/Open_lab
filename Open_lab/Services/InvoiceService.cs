using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly OpenLabDbContext _db;

        public InvoiceService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<Invoice> CreateOrUpdateInvoiceAsync(int visitId, decimal discount, decimal paid)
        {
            if (discount < 0)
            {
                throw new ArgumentException("Discount cannot be negative.", nameof(discount));
            }

            if (paid < 0)
            {
                throw new ArgumentException("Paid cannot be negative.", nameof(paid));
            }

            var visit = await _db.Visits.FirstOrDefaultAsync(v => v.VisitId == visitId);
            if (visit == null)
            {
                throw new InvalidOperationException("Visit not found.");
            }

            var total = await GetVisitTotalAsync(visitId);
            if (discount > total)
            {
                throw new InvalidOperationException("الخصم لا يمكن أن يتجاوز إجمالي الفاتورة (قاعدة BR-ACC-004).");
            }
            var netTotal = Math.Max(0, total - discount);

            var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.VisitId == visitId);
            if (invoice == null)
            {
                invoice = new Invoice
                {
                    VisitId = visitId
                };
                _db.Invoices.Add(invoice);
            }

            invoice.Total = total;
            invoice.Discount = discount;
            invoice.NetTotal = netTotal;

            await _db.SaveChangesAsync();
            await RecalculateInvoiceAsync(invoice.InvoiceId, paid);
            return invoice;
        }

        public async Task<Payment> AddPaymentAsync(int invoiceId, decimal amount, int userId)
        {
            var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
            if (invoice == null)
            {
                throw new InvalidOperationException("Invoice not found.");
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            }

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                Amount = amount,
                PaymentDate = DateTime.Now,
                UserId = userId
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            await RecalculateInvoiceAsync(invoiceId, 0);
            return payment;
        }

        public async Task<Payment> EditPaymentAsync(int paymentId, decimal newAmount, int userId, string? reason = null)
        {
            if (newAmount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.", nameof(newAmount));
            }

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null)
            {
                throw new InvalidOperationException("Payment not found.");
            }

            payment.Amount = newAmount;
            payment.UserId = userId;
            payment.PaymentDate = DateTime.Now;

            await _db.SaveChangesAsync();

            await RecalculateInvoiceAsync(payment.InvoiceId, 0);
            return payment;
        }

        public async Task DeletePaymentAsync(int paymentId, string? reason = null)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null)
            {
                return;
            }

            var invoiceId = payment.InvoiceId;
            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync();
            await RecalculateInvoiceAsync(invoiceId, 0);
        }

        public Task<List<Payment>> GetPaymentsAsync(int invoiceId)
        {
            return _db.Payments.AsNoTracking()
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<AdditionalCharge> AddAdditionalChargeAsync(int invoiceId, string description, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description is required.", nameof(description));
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            }

            var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
            if (invoice == null)
            {
                throw new InvalidOperationException("Invoice not found.");
            }

            var charge = new AdditionalCharge
            {
                InvoiceId = invoiceId,
                Description = description.Trim(),
                Amount = amount
            };

            _db.AdditionalCharges.Add(charge);
            await _db.SaveChangesAsync();
            await RecalculateInvoiceAsync(invoiceId, 0);
            return charge;
        }

        public Task<List<AdditionalCharge>> GetAdditionalChargesAsync(int invoiceId)
        {
            return _db.AdditionalCharges
                .AsNoTracking()
                .Where(c => c.InvoiceId == invoiceId)
                .OrderByDescending(c => c.AdditionalChargeId)
                .ToListAsync();
        }

        public Task<Invoice?> GetByVisitIdAsync(int visitId)
        {
            return _db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.VisitId == visitId);
        }

        public async Task<decimal> GetVisitTotalAsync(int visitId)
        {
            var testsTotal = await _db.VisitTests
                .Where(vt => vt.VisitId == visitId)
                .SumAsync(vt => vt.Price);

            var invoiceId = await _db.Invoices
                .Where(i => i.VisitId == visitId)
                .Select(i => (int?)i.InvoiceId)
                .FirstOrDefaultAsync();

            if (!invoiceId.HasValue)
            {
                return testsTotal;
            }

            var chargesTotal = await _db.AdditionalCharges
                .Where(c => c.InvoiceId == invoiceId.Value)
                .SumAsync(c => c.Amount);

            return testsTotal + chargesTotal;
        }

        public Task<List<Invoice>> GetPatientInvoicesByDateAsync(int patientId, DateTime from, DateTime to)
        {
            return _db.Invoices.AsNoTracking()
                .Include(i => i.Visit)
                .Where(i => i.Visit.PatientId == patientId && i.Visit.VisitDate >= from && i.Visit.VisitDate <= to)
                .OrderBy(i => i.Visit.VisitDate)
                .ToListAsync();
        }

        public Task<List<Payment>> GetPatientPaymentsByDateAsync(int patientId, DateTime from, DateTime to)
        {
            return _db.Payments.AsNoTracking()
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Visit)
                .Where(p => p.Invoice.Visit.PatientId == patientId && p.PaymentDate >= from && p.PaymentDate <= to)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Invoice> SettleAccountAsync(int visitId)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Visit)
                .FirstOrDefaultAsync(i => i.VisitId == visitId);

            if (invoice == null)
            {
                throw new InvalidOperationException("Invoice not found for this visit.");
            }

            await RecalculateInvoiceAsync(invoice.InvoiceId, 0);
            invoice = await _db.Invoices
                .Include(i => i.Visit)
                .FirstAsync(i => i.InvoiceId == invoice.InvoiceId);

            if (invoice.Balance > 0)
            {
                throw new InvalidOperationException("Cannot settle account with remaining balance.");
            }

            invoice.Status = "Settled";
            if (invoice.Visit != null)
            {
                invoice.Visit.Status = "Closed";
            }

            await _db.SaveChangesAsync();
            return invoice;
        }

        private async Task RecalculateInvoiceAsync(int invoiceId, decimal manualPaid)
        {
            var invoice = await _db.Invoices.FirstAsync(i => i.InvoiceId == invoiceId);
            var paidFromPayments = await _db.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .SumAsync(p => p.Amount);

            invoice.Paid = paidFromPayments + manualPaid;
            invoice.Balance = invoice.NetTotal - invoice.Paid;
            if (invoice.Balance < 0)
            {
                invoice.Balance = 0;
            }

            invoice.Status = invoice.Balance == 0 ? "Paid" : "Partial";
            await _db.SaveChangesAsync();
        }

        public async Task<decimal> CalculateReferralDiscountAsync(int referralId, decimal totalAmount)
        {
            if (totalAmount < 0)
            {
                throw new ArgumentException("Total amount cannot be negative.", nameof(totalAmount));
            }

            var referral = await _db.Referrals.FirstOrDefaultAsync(r => r.ReferralId == referralId);
            if (referral == null)
            {
                return 0;
            }

            var discountPercentage = referral.DiscountPercentage;
            if (discountPercentage <= 0)
            {
                return 0;
            }

            var discountAmount = totalAmount * (discountPercentage / 100);
            return Math.Round(discountAmount, 2);
        }

        public async Task<decimal> CalculateReferralCommissionAsync(int referralId, decimal totalAmount)
        {
            if (totalAmount < 0)
            {
                throw new ArgumentException("Total amount cannot be negative.", nameof(totalAmount));
            }

            var referral = await _db.Referrals.FirstOrDefaultAsync(r => r.ReferralId == referralId);
            if (referral == null)
            {
                return 0;
            }

            var commissionPercentage = referral.CommissionPercentage;
            if (commissionPercentage <= 0)
            {
                return 0;
            }

            var commissionAmount = totalAmount * (commissionPercentage / 100);
            return Math.Round(commissionAmount, 2);
        }

        public async Task LogInvoicePrintedAsync(int invoiceId, int userId)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = "Print",
                TableName = "Invoice",
                RecordId = invoiceId.ToString(),
                Timestamp = DateTime.UtcNow,
                NewValues = "Invoice printed for patient."
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
