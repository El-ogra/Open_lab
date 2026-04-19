using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class ContractInvoiceService : IContractInvoiceService
    {
        private readonly OpenLabDbContext _db;

        public ContractInvoiceService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<List<BulkClaimRow>> GetPendingInvoicesAsync(int referralId, DateTime from, DateTime to)
        {
            return await _db.Invoices
                .AsNoTracking()
                .Include(i => i.Visit)
                .ThenInclude(v => v.Patient)
                .Where(i => i.Visit.ReferralId == referralId &&
                            i.Visit.VisitDate >= from &&
                            i.Visit.VisitDate <= to &&
                            i.ContractInvoiceId == null)
                .Select(i => new BulkClaimRow
                {
                    InvoiceId = i.InvoiceId,
                    LabId = i.Visit.Patient.LabId,
                    PatientName = i.Visit.Patient.FullName,
                    VisitDate = i.Visit.VisitDate,
                    Total = i.Total,
                    Discount = i.Discount,
                    NetTotal = i.NetTotal
                })
                .ToListAsync();
        }

        public async Task<int> CreateContractInvoiceAsync(int referralId, string invoiceNumber, DateTime from, DateTime to)
        {
            var referral = await _db.Referrals.FindAsync(referralId);
            if (referral == null) throw new Exception("الجهة غير موجودة");

            var pendingInvoices = await _db.Invoices
                .Include(i => i.Visit)
                .Where(i => i.Visit.ReferralId == referralId &&
                            i.Visit.VisitDate >= from &&
                            i.Visit.VisitDate <= to &&
                            i.ContractInvoiceId == null)
                .ToListAsync();

            if (!pendingInvoices.Any()) throw new Exception("لا توجد فواتير معلقة لهذه الفترة");

            var contractInvoice = new ContractInvoice
            {
                ReferralId = referralId,
                InvoiceNumber = invoiceNumber,
                DateFrom = from,
                DateTo = to,
                TotalAmount = pendingInvoices.Sum(i => i.Total),
                DiscountAmount = pendingInvoices.Sum(i => i.Discount),
                NetAmount = pendingInvoices.Sum(i => i.NetTotal),
                IsPaid = false,
                CreatedAt = DateTime.Now
            };

            _db.ContractInvoices.Add(contractInvoice);
            await _db.SaveChangesAsync(); // Get the ID

            foreach (var invoice in pendingInvoices)
            {
                invoice.ContractInvoiceId = contractInvoice.ContractInvoiceId;
            }

            await _db.SaveChangesAsync();
            return contractInvoice.ContractInvoiceId;
        }

        public async Task<List<ContractInvoice>> GetContractInvoicesAsync(int referralId)
        {
            return await _db.ContractInvoices
                .AsNoTracking()
                .Where(c => c.ReferralId == referralId)
                .OrderByDescending(c => c.DateTo)
                .ToListAsync();
        }
    }
}
