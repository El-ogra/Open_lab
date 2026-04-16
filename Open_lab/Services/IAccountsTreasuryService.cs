using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public sealed class AccountsTreasurySnapshot
    {
        public decimal TotalInvoiced { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal TotalBalance { get; init; }
        public List<AccountsPaymentRow> Payments { get; init; } = new();
    }

    public interface IAccountsTreasuryService
    {
        Task<AccountsTreasurySnapshot> GetSnapshotAsync(DateTime from, DateTime to);
    }
}
