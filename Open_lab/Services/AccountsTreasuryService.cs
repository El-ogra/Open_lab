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
                .AsNoTracking()
                .Include(i => i.Visit)
                .ThenInclude(v => v.Patient)
                .Include(i => i.Visit)
                .ThenInclude(v => v.Referral)
                .Where(i => i.Visit.VisitDate >= from && i.Visit.VisitDate <= to)
                .ToListAsync();

            var payments = await _db.Payments
                .AsNoTracking()
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Visit)
                .ThenInclude(v => v.Patient)
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Visit)
                .ThenInclude(v => v.Referral)
                .Include(p => p.User)
                .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var byUser = payments
                .GroupBy(p => string.IsNullOrWhiteSpace(p.User?.Username) ? "غير محدد" : p.User!.Username)
                .Select(g => new TreasuryByUserRow
                {
                    Username = g.Key,
                    PaymentsCount = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .OrderByDescending(r => r.TotalAmount)
                .ToList();

            var byReferral = invoices
                .GroupBy(i => i.Visit.Referral?.Name ?? "بدون جهة")
                .Select(g => new TreasuryByReferralRow
                {
                    ReferralName = g.Key,
                    VisitsCount = g.Select(i => i.VisitId).Distinct().Count(),
                    TotalInvoiced = g.Sum(i => i.NetTotal),
                    TotalPaid = g.Sum(i => i.Paid),
                    TotalBalance = g.Sum(i => i.Balance)
                })
                .OrderByDescending(r => r.TotalInvoiced)
                .ToList();

            var totalDiscount = invoices.Sum(i => i.Discount);
            var expenses = await _db.Expenses
                .AsNoTracking()
                .Where(e => e.Date >= from && e.Date <= to)
                .ToListAsync();
            var totalExpenses = expenses.Sum(e => e.Amount);

            var totalPaid = invoices.Sum(i => i.Paid);
            var netProfit = totalPaid - totalExpenses;

            return new AccountsTreasurySnapshot
            {
                TotalInvoiced = invoices.Sum(i => i.NetTotal),
                TotalPaid = totalPaid,
                TotalBalance = invoices.Sum(i => i.Balance),
                TotalDiscount = totalDiscount,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
                Payments = payments.Select(p => new AccountsPaymentRow
                {
                    PaymentId = p.PaymentId,
                    PatientName = p.Invoice.Visit.Patient.FullName,
                    ReferralName = p.Invoice.Visit.Referral?.Name,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    Username = p.User?.Username
                }).ToList(),
                ByUser = byUser,
                ByReferral = byReferral
            };
        }
    }
}
