using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public class BulkClaimRow
    {
        public int InvoiceId { get; set; }
        public string LabId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
    }

    public interface IContractInvoiceService
    {
        Task<List<BulkClaimRow>> GetPendingInvoicesAsync(int referralId, DateTime from, DateTime to);
        Task<int> CreateContractInvoiceAsync(int referralId, string invoiceNumber, DateTime from, DateTime to);
        Task<List<Models.ContractInvoice>> GetContractInvoicesAsync(int referralId);
        Task<Models.ContractInvoice> SettleContractInvoiceAsync(int contractInvoiceId);
    }
}
