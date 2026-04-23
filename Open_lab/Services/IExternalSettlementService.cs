using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IExternalSettlementService
    {
        Task<decimal> GetPendingBalanceAsync(int referralId);
        Task<ExternalLabSettlement> CreateSettlementAsync(int referralId, decimal amountPaid, string? note = null);
        Task<List<ExternalLabSettlement>> GetSettlementHistoryAsync(int referralId);
        
        /// <summary>
        /// Calculates Revenue = Patient Price - Cost Price for all tests sent to this lab.
        /// </summary>
        Task<decimal> GetTotalProfitAsync(int referralId);
    }
}
