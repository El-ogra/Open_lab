using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
        public async Task GetPendingInvoicesAsync_Should_Return_Pending_With_Correct_Fields()
        {
            // Refactored to Logic Guard - verifies all returned field values
            var patient = new Patient { LabId = "L1", FullName = "P1", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var referral = new Referral { ReferralId = 1, Name = "R1" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 100m, Discount = 10m, NetTotal = 90m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            var list = await _service.GetPendingInvoicesAsync(referral.ReferralId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Logic Guard: Verify all field values are correct
            list.Should().ContainSingle();
            var row = list[0];
            row.InvoiceId.Should().Be(invoice.InvoiceId);
            row.LabId.Should().Be("L1");
            row.PatientName.Should().Be("P1");
            row.VisitDate.Should().Be(visit.VisitDate);
            row.Total.Should().Be(100m);
            row.Discount.Should().Be(10m);
            row.NetTotal.Should().Be(90m);
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
        public async Task CreateContractInvoiceAsync_Should_Create_And_Assign_With_Correct_Aggregates()
        {
            // Refactored to Logic Guard - verifies all calculated fields and side effects
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

            // Act
            var id = await _service.CreateContractInvoiceAsync(referral.ReferralId, "CN-1", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Logic Guard: Verify all fields and side effects
            id.Should().BeGreaterThan(0);

            var contract = await _db.ContractInvoices.FindAsync(id);
            contract.Should().NotBeNull();
            contract!.ReferralId.Should().Be(referral.ReferralId);
            contract.InvoiceNumber.Should().Be("CN-1");
            contract.TotalAmount.Should().Be(200m);
            contract.DiscountAmount.Should().Be(10m);
            contract.NetAmount.Should().Be(190m);
            contract.IsPaid.Should().BeFalse();
            contract.DateFrom.Should().Be(DateTime.Today.AddDays(-1));
            contract.DateTo.Should().Be(DateTime.Today.AddDays(1));

            // Side effect: Invoice should be assigned to contract
            var updatedInvoice = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updatedInvoice.Should().NotBeNull();
            updatedInvoice!.ContractInvoiceId.Should().Be(id);
        }

        [Fact]
        public async Task SettleContractInvoiceAsync_Should_Mark_Invoice_As_Paid()
        {
            // Arrange
            var referral = new Referral { Name = "R4" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var contract = new ContractInvoice
            {
                ReferralId = referral.ReferralId,
                InvoiceNumber = "C-1",
                DateFrom = DateTime.Today.AddDays(-7),
                DateTo = DateTime.Today,
                TotalAmount = 300m,
                DiscountAmount = 20m,
                NetAmount = 280m,
                IsPaid = false,
                CreatedAt = DateTime.Now
            };
            _db.ContractInvoices.Add(contract);
            await _db.SaveChangesAsync();

            // Act
            var settled = await _service.SettleContractInvoiceAsync(contract.ContractInvoiceId);

            // Assert
            settled.IsPaid.Should().BeTrue();
            var saved = await _db.ContractInvoices.FindAsync(contract.ContractInvoiceId);
            saved.Should().NotBeNull();
            saved!.IsPaid.Should().BeTrue();
        }

        // 2.4 - Account Settlement - Financial Integrity Tests
        [Fact]
        public async Task SettleContractInvoiceAsync_Should_Update_Payment_Status_Only()
        {
            // Arrange
            var referral = new Referral { Name = "R5" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var contract = new ContractInvoice
            {
                ReferralId = referral.ReferralId,
                InvoiceNumber = "C-2",
                DateFrom = DateTime.Today.AddDays(-7),
                DateTo = DateTime.Today,
                TotalAmount = 500m,
                DiscountAmount = 50m,
                NetAmount = 450m,
                IsPaid = false,
                CreatedAt = DateTime.Now
            };
            _db.ContractInvoices.Add(contract);
            await _db.SaveChangesAsync();

            // Act
            var settled = await _service.SettleContractInvoiceAsync(contract.ContractInvoiceId);

            // Assert - Only IsPaid should change, other fields should remain unchanged
            settled.IsPaid.Should().BeTrue();
            settled.TotalAmount.Should().Be(500m);
            settled.NetAmount.Should().Be(450m);
            settled.InvoiceNumber.Should().Be("C-2");
        }

        [Fact]
        public async Task SettleContractInvoiceAsync_NotFound_Should_Throw()
        {
            // Act
            Func<Task> act = async () => await _service.SettleContractInvoiceAsync(99999);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*فاتورة التعاقد غير موجودة*");
        }

        // 12.1 - Contract Creation - Contract Binding Logic Tests
        [Fact]
        public async Task CreateContractInvoiceAsync_Should_Calculate_Correct_Aggregates()
        {
            // Arrange
            var referral = new Referral { ReferralId = 10, Name = "ContractRef" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "LC1", FullName = "PC", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            var visit2 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(1), ReferralId = referral.ReferralId };
            _db.Visits.AddRange(visit1, visit2);
            await _db.SaveChangesAsync();

            var invoice1 = new Invoice { VisitId = visit1.VisitId, Total = 100m, Discount = 10m, NetTotal = 90m };
            var invoice2 = new Invoice { VisitId = visit2.VisitId, Total = 200m, Discount = 20m, NetTotal = 180m };
            _db.Invoices.AddRange(invoice1, invoice2);
            await _db.SaveChangesAsync();

            // Act
            var contractId = await _service.CreateContractInvoiceAsync(referral.ReferralId, "CN-100", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(2));

            // Assert
            var contract = await _db.ContractInvoices.FindAsync(contractId);
            contract.Should().NotBeNull();
            contract!.TotalAmount.Should().Be(300m); // 100 + 200
            contract.DiscountAmount.Should().Be(30m); // 10 + 20
            contract.NetAmount.Should().Be(270m); // 90 + 180
            contract.InvoiceNumber.Should().Be("CN-100");
        }

        [Fact]
        public async Task CreateContractInvoiceAsync_ReferralNotFound_Should_Throw()
        {
            // Act
            Func<Task> act = async () => await _service.CreateContractInvoiceAsync(99999, "CN-X", DateTime.Today, DateTime.Today);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("*الجهة غير موجودة*");
        }

        [Fact]
        public async Task CreateContractInvoiceAsync_Should_Assign_Invoices_To_Contract()
        {
            // Arrange
            var referral = new Referral { ReferralId = 11, Name = "RefBind" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "LB1", FullName = "PB", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 150m, Discount = 0m, NetTotal = 150m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            var contractId = await _service.CreateContractInvoiceAsync(referral.ReferralId, "CN-BIND", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            var updatedInvoice = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updatedInvoice.Should().NotBeNull();
            updatedInvoice!.ContractInvoiceId.Should().Be(contractId);
        }

        [Fact]
        public async Task GetContractInvoicesAsync_Should_Return_Invoices_Ordered_By_DateTo_Desc_Success()
        {
            // Arrange
            var referral = new Referral { Name = "HistoryRef" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            _db.ContractInvoices.AddRange(
                new ContractInvoice { ReferralId = referral.ReferralId, InvoiceNumber = "A", DateFrom = DateTime.Today.AddDays(-5), DateTo = DateTime.Today.AddDays(-1), TotalAmount = 10m, NetAmount = 10m, CreatedAt = DateTime.Now },
                new ContractInvoice { ReferralId = referral.ReferralId, InvoiceNumber = "B", DateFrom = DateTime.Today.AddDays(-2), DateTo = DateTime.Today, TotalAmount = 20m, NetAmount = 20m, CreatedAt = DateTime.Now });
            await _db.SaveChangesAsync();

            // Act
            var history = await _service.GetContractInvoicesAsync(referral.ReferralId);

            // Assert
            history.Should().HaveCount(2);
            history[0].InvoiceNumber.Should().Be("B");
            history[1].InvoiceNumber.Should().Be("A");
        }

        [Fact]
        public async Task GetPendingInvoicesAsync_When_Referral_Has_No_Pending_Should_Return_Empty_Edge()
        {
            // Arrange
            var referral = new Referral { Name = "NoPending" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetPendingInvoicesAsync(referral.ReferralId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().BeEmpty();
        }

        // 12.3 - Contract Discount Logic Tests
        [Fact]
        public async Task Referral_DiscountPercentage_Should_Be_Applied_Correctly()
        {
            // Arrange
            var referral = new Referral { Name = "DiscountRef", DiscountPercentage = 10m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "LD1", FullName = "PD", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 100m, Discount = 10m, NetTotal = 90m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            var contractId = await _service.CreateContractInvoiceAsync(referral.ReferralId, "CN-DISC", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Logic Guard: Verify discount applied correctly
            var contract = await _db.ContractInvoices.FindAsync(contractId);
            contract.Should().NotBeNull();
            contract!.DiscountAmount.Should().Be(10m);
            contract.NetAmount.Should().Be(90m);
        }

        [Fact]
        public async Task Referral_ZeroDiscount_Should_Not_Affect_NetTotal()
        {
            // Arrange
            var referral = new Referral { Name = "NoDiscountRef", DiscountPercentage = 0m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "LD2", FullName = "PD2", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 100m, Discount = 0m, NetTotal = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            var contractId = await _service.CreateContractInvoiceAsync(referral.ReferralId, "CN-ZERO", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Logic Guard: Verify zero discount applied
            var contract = await _db.ContractInvoices.FindAsync(contractId);
            contract.Should().NotBeNull();
            contract!.DiscountAmount.Should().Be(0m);
            contract.NetAmount.Should().Be(100m);
        }

        // 12.5 - Patient Contract Binding Tests
        [Fact]
        public async Task Patient_ReferralBinding_Should_Persist_Correctly()
        {
            // Arrange
            var referral = new Referral { Name = "BindRef" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "LPB1", FullName = "Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var retrievedVisit = await _db.Visits.Include(v => v.Patient).Include(v => v.Referral).FirstOrDefaultAsync(v => v.VisitId == visit.VisitId);

            // Assert - Logic Guard: Verify binding persisted
            retrievedVisit.Should().NotBeNull();
            retrievedVisit!.PatientId.Should().Be(patient.PatientId);
            retrievedVisit.ReferralId.Should().Be(referral.ReferralId);
            retrievedVisit.Patient.Should().NotBeNull();
            retrievedVisit.Patient!.LabId.Should().Be("LPB1");
            retrievedVisit.Referral.Should().NotBeNull();
            retrievedVisit.Referral!.Name.Should().Be("BindRef");
        }

        [Fact]
        public async Task Patient_DuplicateReferralBinding_Should_Allow_Multiple_Visits()
        {
            // Arrange
            var referral = new Referral { Name = "MultiRef" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "LPB2", FullName = "Patient2", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            var visit2 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(1), ReferralId = referral.ReferralId };
            _db.Visits.AddRange(visit1, visit2);
            await _db.SaveChangesAsync();

            // Act
            var visits = await _db.Visits.Where(v => v.PatientId == patient.PatientId).ToListAsync();

            // Assert - Logic Guard: Verify both visits bound to same referral
            visits.Should().HaveCount(2);
            visits.Should().OnlyContain(v => v.ReferralId == referral.ReferralId);
        }
    }
}
