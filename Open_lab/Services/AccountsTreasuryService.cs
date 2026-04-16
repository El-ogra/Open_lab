using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public class AccountsTreasuryService : IAccountsTreasuryService
    {
        private readonly OpenLabDbContext _db;

        public AccountsTreasuryService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<AccountsTreasurySnapshot> GetSnapshotAsync(DateTime from, DateTime to)
        {
            var invoices = await _db.Invoices
                .Include(i => i.Visit)
                .ThenInclude(v => v.Patient)
                .Where(i => i.Visit.VisitDate >= from && i.Visit.VisitDate <= to)
                .ToListAsync();

            var payments = await _db.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Visit)
                .ThenInclude(v => v.Patient)
                .Include(p => p.User)
                .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return new AccountsTreasurySnapshot
            {
                TotalInvoiced = invoices.Sum(i => i.NetTotal),
                TotalPaid = invoices.Sum(i => i.Paid),
                TotalBalance = invoices.Sum(i => i.Balance),
                Payments = payments.Select(p => new AccountsPaymentRow
                {
                    PaymentId = p.PaymentId,
                    PatientName = p.Invoice.Visit.Patient.FullName,
                    ReferralName = p.Invoice.Visit.Referral?.Name,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    Username = p.User?.Username
                }).ToList()
            };
        }
    }
}
