using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class ExternalSettlementService : IExternalSettlementService
    {
        private readonly OpenLabDbContext _db;

        public ExternalSettlementService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<decimal> GetPendingBalanceAsync(int referralId)
        {
            // Total cost is sum of CostPrice of all tests sent to this lab that have been shipped/received
            var totalCost = await _db.ExternalLabQueues
                .Include(q => q.VisitTest)
                .ThenInclude(vt => vt.Test)
                .Where(q => q.ReferralId == referralId && (q.Status == "Shipped" || q.Status == "Received"))
                .SumAsync(q => q.VisitTest.Test.CostPrice ?? 0);

            var totalPaid = await _db.ExternalLabSettlements
                .Where(s => s.ReferralId == referralId)
                .SumAsync(s => s.AmountPaid);

            return totalCost - totalPaid;
        }

        public async Task<ExternalLabSettlement> CreateSettlementAsync(int referralId, decimal amountPaid, string? note = null)
        {
            var pendingBalance = await GetPendingBalanceAsync(referralId);
            
            // Note: In a real system, we'd also calculate 'TotalCost' as of this moment to store snapshots
            var settlement = new ExternalLabSettlement
            {
                ReferralId = referralId,
                AmountPaid = amountPaid,
                TotalCost = pendingBalance, // This is current balance before this payment
                Balance = pendingBalance - amountPaid,
                SettlementDate = DateTime.Now,
                Note = note
            };

            _db.ExternalLabSettlements.Add(settlement);
            await _db.SaveChangesAsync();
            return settlement;
        }

        public Task<List<ExternalLabSettlement>> GetSettlementHistoryAsync(int referralId)
        {
            return _db.ExternalLabSettlements
                .Where(s => s.ReferralId == referralId)
                .OrderByDescending(s => s.SettlementDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalProfitAsync(int referralId)
        {
            var tests = await _db.ExternalLabQueues
                .Include(q => q.VisitTest)
                .ThenInclude(vt => vt.Test)
                .Where(q => q.ReferralId == referralId && (q.Status == "Shipped" || q.Status == "Received"))
                .ToListAsync();

            // Profit = PatientPrice (what patient paid) - CostPrice (what we pay external lab)
            return tests.Sum(q => (q.VisitTest.Price) - (q.VisitTest.Test.CostPrice ?? 0));
        }
    }
}
