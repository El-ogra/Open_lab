using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class ContractInvoiceServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly ContractInvoiceService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public ContractInvoiceServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new ContractInvoiceService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetPendingInvoicesAsync_Should_Return_Pending()
        {
            var patient = new Patient { LabId = "L1", FullName = "P1", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var referral = new Referral { ReferralId = 1, Name = "R1" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 100m, Discount = 0m, NetTotal = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var list = await _service.GetPendingInvoicesAsync(referral.ReferralId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            list.Should().ContainSingle();
        }

        [Fact]
        public async Task CreateContractInvoiceAsync_NoPending_Should_Throw()
        {
            _db.Referrals.Add(new Referral { ReferralId = 2, Name = "R2" });
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.CreateContractInvoiceAsync(2, "INV-1", DateTime.Today.AddDays(-1), DateTime.Today);
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task CreateContractInvoiceAsync_Should_Create_And_Assign()
        {
            var referral = new Referral { ReferralId = 3, Name = "R3" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L2", FullName = "P2", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 200m, Discount = 10m, NetTotal = 190m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var id = await _service.CreateContractInvoiceAsync(referral.ReferralId, "CN-1", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            id.Should().BeGreaterThan(0);

            var contract = await _db.ContractInvoices.FindAsync(id);
            contract.Should().NotBeNull();
            var updatedInvoice = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updatedInvoice.Should().NotBeNull();
            updatedInvoice!.ContractInvoiceId.Should().Be(id);
        }
    }
}
