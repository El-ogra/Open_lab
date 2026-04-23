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

        public async Task<AccountsTreasurySnapshot> GetSnapshotAsync(DateTime from, DateTime to, int? branchId = null)
        {
            var invoicesQuery = _db.Invoices
                .AsNoTracking()
                .Include(i => i.Visit)
                .ThenInclude(v => v.Patient)
                .Include(i => i.Visit)
                .ThenInclude(v => v.Referral)
                .Include(i => i.Visit)
                .ThenInclude(v => v.Branch)
                .Include(i => i.Visit)
                .ThenInclude(v => v.Physician)
                .Where(i => i.Visit.VisitDate >= from && i.Visit.VisitDate <= to);

            if (branchId.HasValue)
            {
                invoicesQuery = invoicesQuery.Where(i => i.Visit.BranchId == branchId.Value);
            }

            var invoices = await invoicesQuery.ToListAsync();

            var paymentsQuery = _db.Payments
                .AsNoTracking()
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Visit)
                .ThenInclude(v => v.Patient)
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Visit)
                .ThenInclude(v => v.Referral)
                .Include(p => p.User)
                .Where(p => p.PaymentDate >= from && p.PaymentDate <= to);

            if (branchId.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.Invoice.Visit.BranchId == branchId.Value);
            }

            var payments = await paymentsQuery.OrderByDescending(p => p.PaymentDate).ToListAsync();

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

            var byBranch = invoices
                .GroupBy(i => i.Visit.Branch?.Name ?? "الفرع الرئيسي")
                .Select(g => new TreasuryByBranchRow
                {
                    BranchName = g.Key,
                    VisitsCount = g.Select(i => i.VisitId).Distinct().Count(),
                    TotalInvoiced = g.Sum(i => i.NetTotal),
                    TotalPaid = g.Sum(i => i.Paid),
                    TotalBalance = g.Sum(i => i.Balance)
                })
                .OrderByDescending(r => r.TotalInvoiced)
                .ToList();

            var byDoctor = invoices
                .GroupBy(i => new
                {
                    DoctorName = i.Visit.Physician?.FullName ?? "بدون طبيب",
                    CommissionPercentage = i.Visit.Physician?.CommissionPercentage ?? 0m
                })
                .Select(g => new TreasuryByDoctorRow
                {
                    DoctorName = g.Key.DoctorName,
                    VisitsCount = g.Select(i => i.VisitId).Distinct().Count(),
                    TotalInvoiced = g.Sum(i => i.NetTotal),
                    CommissionAmount = g.Sum(i => i.NetTotal * (g.Key.CommissionPercentage / 100m))
                })
                .OrderByDescending(r => r.TotalInvoiced)
                .ToList();

            var totalDiscount = invoices.Sum(i => i.Discount);
            var expensesQuery = _db.Expenses
                .AsNoTracking()
                .Where(e => e.Date >= from && e.Date <= to);
            
            // In a real system, expenses might also be branch-specific if the Expense model had a BranchId.
            // Currently, we'll keep them overall or assume they are branch-neutral for now.
            
            var expenses = await expensesQuery.ToListAsync();
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
                ByReferral = byReferral,
                ByBranch = byBranch,
                ByDoctor = byDoctor
            };
        }
    }
}
