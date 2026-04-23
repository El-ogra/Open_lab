using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public sealed class AccountsTreasurySnapshot
    {
        public decimal TotalInvoiced { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public List<AccountsPaymentRow> Payments { get; set; } = new();
        public List<TreasuryByUserRow> ByUser { get; set; } = new();
        public List<TreasuryByReferralRow> ByReferral { get; set; } = new();
        public List<TreasuryByBranchRow> ByBranch { get; set; } = new();
        public List<TreasuryByDoctorRow> ByDoctor { get; set; } = new();
    }

    public interface IAccountsTreasuryService
    {
        Task<AccountsTreasurySnapshot> GetSnapshotAsync(DateTime from, DateTime to, int? branchId = null);
    }
}
