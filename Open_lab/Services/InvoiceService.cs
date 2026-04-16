using System;
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
            var visit = await _db.Visits.FirstOrDefaultAsync(v => v.VisitId == visitId);
            if (visit == null)
            {
                throw new InvalidOperationException("Visit not found.");
            }

            var total = await GetVisitTotalAsync(visitId);
            var netTotal = total - discount;
            if (netTotal < 0)
            {
                netTotal = 0;
            }

            var balance = netTotal - paid;
            if (balance < 0)
            {
                balance = 0;
            }

            var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.VisitId == visitId);
            if (invoice == null)
            {
                invoice = new Invoice
                {
                    VisitId = visitId,
                    Total = total,
                    Discount = discount,
                    NetTotal = netTotal,
                    Paid = paid,
                    Balance = balance,
                    Status = balance == 0 ? "Paid" : "Partial"
                };
                _db.Invoices.Add(invoice);
            }
            else
            {
                invoice.Total = total;
                invoice.Discount = discount;
                invoice.NetTotal = netTotal;
                invoice.Paid = paid;
                invoice.Balance = balance;
                invoice.Status = balance == 0 ? "Paid" : "Partial";
            }

            await _db.SaveChangesAsync();
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

            invoice.Paid += amount;
            invoice.Balance = invoice.NetTotal - invoice.Paid;
            if (invoice.Balance < 0)
            {
                invoice.Balance = 0;
            }
            invoice.Status = invoice.Balance == 0 ? "Paid" : "Partial";

            await _db.SaveChangesAsync();
            return payment;
        }

        public Task<Invoice?> GetByVisitIdAsync(int visitId)
        {
            return _db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.VisitId == visitId);
        }

        public async Task<decimal> GetVisitTotalAsync(int visitId)
        {
            return await _db.VisitTests.Where(vt => vt.VisitId == visitId).SumAsync(vt => vt.Price);
        }
    }
}
