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

        public async Task<Payment> EditPaymentAsync(int paymentId, decimal newAmount, int userId)
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

        public async Task DeletePaymentAsync(int paymentId)
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

        public Task<Invoice?> GetByVisitIdAsync(int visitId)
        {
            return _db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.VisitId == visitId);
        }

        public async Task<decimal> GetVisitTotalAsync(int visitId)
        {
            return await _db.VisitTests.Where(vt => vt.VisitId == visitId).SumAsync(vt => vt.Price);
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
    }
}
