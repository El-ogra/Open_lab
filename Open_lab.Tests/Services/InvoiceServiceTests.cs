using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class InvoiceServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly InvoiceService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public InvoiceServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new InvoiceService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_Should_Create_Invoice_With_Correct_Totals_LogicGuard()
        {
            // Refactored to Logic Guard - verifies calculation accuracy
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 200m });
            await _db.SaveChangesAsync();

            // Act
            var invoice = await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 50m, 0m);

            // Assert - Logic Guard: Verify exact calculation (200 - 50 = 150)
            invoice.Total.Should().Be(200m);
            invoice.Discount.Should().Be(50m);
            invoice.NetTotal.Should().Be(150m);
            invoice.Paid.Should().Be(0m);
            invoice.Balance.Should().Be(150m);

            // Side effect: Verify invoice is persisted
            var saved = await _db.Invoices.FindAsync(invoice.InvoiceId);
            saved.Should().NotBeNull();
            saved!.VisitId.Should().Be(visit.VisitId);
        }

        [Fact]
        public async Task AddPaymentAsync_InvalidAmount_Should_Throw_LogicGuard()
        {
            // Refactored to Logic Guard - verifies financial integrity
            var invoice = new Invoice { VisitId = 1, Total = 100m, Discount = 0m, NetTotal = 100m, Paid = 0m, Balance = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddPaymentAsync(invoice.InvoiceId, 0m, 1);

            // Assert - Logic Guard: Verify invoice state remains unchanged after invalid payment attempt
            await act.Should().ThrowAsync<ArgumentException>();
            var unchanged = await _db.Invoices.FindAsync(invoice.InvoiceId);
            unchanged.Should().NotBeNull();
            unchanged!.Paid.Should().Be(0m);
            unchanged.Balance.Should().Be(100m);
        }

        [Fact]
        public async Task AddAdditionalChargeAsync_InvalidDescription_Should_Throw_LogicGuard()
        {
            // Refactored to Logic Guard - verifies financial integrity
            var invoice = new Invoice { VisitId = 1, Total = 100m, Discount = 0m, NetTotal = 100m, Paid = 0m, Balance = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddAdditionalChargeAsync(invoice.InvoiceId, "   ", 10m);

            // Assert - Logic Guard: Verify no charge was added and invoice unchanged
            await act.Should().ThrowAsync<ArgumentException>();
            var charges = await _db.AdditionalCharges.Where(c => c.InvoiceId == invoice.InvoiceId).ToListAsync();
            charges.Should().BeEmpty();
            var unchanged = await _db.Invoices.FindAsync(invoice.InvoiceId);
            unchanged.Should().NotBeNull();
            unchanged!.Total.Should().Be(100m);
        }

        [Fact]
        public async Task GetVisitTotalAsync_WithCharges_Returns_Sum_LogicGuard()
        {
            // Refactored to Logic Guard - verifies calculation accuracy
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 150m });
            await _db.SaveChangesAsync();

            _db.AdditionalCharges.Add(new AdditionalCharge { InvoiceId = invoice.InvoiceId, Description = "Extra", Amount = 25m });
            await _db.SaveChangesAsync();

            // Act
            var total = await _service.GetVisitTotalAsync(visit.VisitId);

            // Assert - Logic Guard: Verify exact calculation (150 + 25 = 175)
            total.Should().Be(175m);

            // Side effect verification: Verify components are stored correctly
            var visitTests = await _db.VisitTests.Where(vt => vt.VisitId == visit.VisitId).ToListAsync();
            visitTests.Should().ContainSingle();
            visitTests[0].Price.Should().Be(150m);

            var charges = await _db.AdditionalCharges.Where(c => c.InvoiceId == invoice.InvoiceId).ToListAsync();
            charges.Should().ContainSingle();
            charges[0].Amount.Should().Be(25m);
        }

        [Fact]
        public async Task EditPaymentAsync_Should_Update_Amount_And_Recalculate_Invoice_LogicGuard()
        {
            // Refactored to Logic Guard - verifies financial recalculation
            var invoice = new Invoice { VisitId = 1, Total = 200m, Discount = 0m, NetTotal = 200m, Paid = 50m, Balance = 150m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            var payment = new Payment { InvoiceId = invoice.InvoiceId, Amount = 50m, UserId = 1, PaymentDate = DateTime.Now.AddMinutes(-10) };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            var edited = await _service.EditPaymentAsync(payment.PaymentId, 120m, 2, "Test Reason");

            // Assert - Logic Guard: Verify payment update and invoice recalculation
            edited.Amount.Should().Be(120m);
            edited.UserId.Should().Be(2);
            var updatedInvoice = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updatedInvoice.Should().NotBeNull();
            updatedInvoice!.Paid.Should().Be(120m);
            updatedInvoice.Balance.Should().Be(80m);
            updatedInvoice.NetTotal.Should().Be(200m);
            updatedInvoice.Total.Should().Be(200m);

            // Side effect: Verify payment record is updated
            var updatedPayment = await _db.Payments.FindAsync(payment.PaymentId);
            updatedPayment.Should().NotBeNull();
            updatedPayment!.Amount.Should().Be(120m);
            updatedPayment.UserId.Should().Be(2);

            // Audit Trail Verification (Function 2.5)
            var log = await _db.AuditLogs.FirstOrDefaultAsync(l => l.Action == "EDIT_PAYMENT" && l.RecordId == payment.PaymentId.ToString());
            log.Should().NotBeNull();
            log!.NewValues.Should().Contain("Test Reason");
            log.UserId.Should().Be(2);
        }

        [Fact]
        public async Task GetPatientAccount_ByDate_Should_Return_Invoices_And_Payments_For_Same_Patient_LogicGuard()
        {
            // Refactored to Logic Guard - verifies data integrity
            var patient = new Patient { LabId = "LACC1", FullName = "Account P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 100m, Discount = 0m, NetTotal = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            _db.Payments.Add(new Payment
            {
                InvoiceId = invoice.InvoiceId,
                Amount = 60m,
                PaymentDate = DateTime.Today.AddHours(1),
                UserId = 1
            });
            await _db.SaveChangesAsync();

            // Act
            var invoices = await _service.GetPatientInvoicesByDateAsync(patient.PatientId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            var payments = await _service.GetPatientPaymentsByDateAsync(patient.PatientId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Logic Guard: Verify exact data returned
            invoices.Should().ContainSingle();
            invoices[0].InvoiceId.Should().Be(invoice.InvoiceId);
            invoices[0].Total.Should().Be(100m);
            invoices[0].NetTotal.Should().Be(100m);

            payments.Should().ContainSingle();
            payments[0].Amount.Should().Be(60m);
            payments[0].InvoiceId.Should().Be(invoice.InvoiceId);
        }

        // 2.8 - Invoice Generation - Financial Integrity Tests
        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_WithZeroDiscount_Should_Calculate_Correctly()
        {
            // Arrange
            var patient = new Patient { LabId = "LZ1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 300m });
            await _db.SaveChangesAsync();

            // Act
            var invoice = await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 0m, 0m);

            // Assert
            invoice.Total.Should().Be(300m);
            invoice.Discount.Should().Be(0m);
            invoice.NetTotal.Should().Be(300m);
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_WithLargeValues_Should_Handle_Correctly()
        {
            // Arrange
            var patient = new Patient { LabId = "LL1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 999999.99m });
            await _db.SaveChangesAsync();

            // Act
            var invoice = await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 100000m, 500000m);

            // Assert
            invoice.Total.Should().Be(999999.99m);
            invoice.Discount.Should().Be(100000m);
            invoice.NetTotal.Should().Be(899999.99m);
            invoice.Paid.Should().Be(500000m);
            invoice.Balance.Should().Be(399999.99m);
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_NegativeDiscount_Should_Throw()
        {
            // Arrange
            var patient = new Patient { LabId = "LN1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 100m });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, -50m, 0m);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Discount cannot be negative*");
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_NegativePaid_Should_Throw()
        {
            // Arrange
            var patient = new Patient { LabId = "LN2", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 100m });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 0m, -100m);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Paid cannot be negative*");
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_DiscountExceedsTotal_Should_Throw_LogicGuard()
        {
            // Arrange
            var patient = new Patient { LabId = "LC1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 100m });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 150m, 0m);

            // Assert - Function 2.2 / BR-ACC-004
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*الخصم لا يمكن أن يتجاوز إجمالي الفاتورة*");
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_FullPayment_Should_Set_Status_To_Paid()
        {
            // Arrange
            var patient = new Patient { LabId = "LFP1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 200m });
            await _db.SaveChangesAsync();

            // Act
            var invoice = await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 0m, 200m);

            // Assert
            invoice.Status.Should().Be("Paid");
            invoice.Balance.Should().Be(0m);
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_PartialPayment_Should_Set_Status_To_Partial()
        {
            // Arrange
            var patient = new Patient { LabId = "LPP1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 200m });
            await _db.SaveChangesAsync();

            // Act
            var invoice = await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 0m, 100m);

            // Assert
            invoice.Status.Should().Be("Partial");
            invoice.Balance.Should().Be(100m);
        }

        // 2.3 - Payment Registration - Financial Integrity Tests
        [Fact]
        public async Task AddPaymentAsync_FullPayment_Should_Update_Balance_To_Zero()
        {
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 100m, Discount = 0m, NetTotal = 100m, Paid = 0m, Balance = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            await _service.AddPaymentAsync(invoice.InvoiceId, 100m, 1);

            // Assert - Logic Guard: Verify balance becomes zero
            var updated = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updated.Should().NotBeNull();
            updated!.Paid.Should().Be(100m);
            updated.Balance.Should().Be(0m);
            updated.Status.Should().Be("Paid");

            // Side effect: Verify payment record created
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.InvoiceId == invoice.InvoiceId);
            payment.Should().NotBeNull();
            payment!.Amount.Should().Be(100m);
        }

        [Fact]
        public async Task AddPaymentAsync_PartialPayment_Should_Update_Balance_Correctly()
        {
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 200m, Discount = 0m, NetTotal = 200m, Paid = 0m, Balance = 200m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            await _service.AddPaymentAsync(invoice.InvoiceId, 50m, 1);

            // Assert - Logic Guard: Verify partial payment calculation
            var updated = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updated.Should().NotBeNull();
            updated!.Paid.Should().Be(50m);
            updated.Balance.Should().Be(150m);
            updated.Status.Should().Be("Partial");
        }

        // 2.6 - Delete Payment - Financial Integrity Tests
        [Fact]
        public async Task DeletePaymentAsync_Should_Recalculate_Balance()
        {
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 200m, Discount = 0m, NetTotal = 200m, Paid = 100m, Balance = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            var payment = new Payment { InvoiceId = invoice.InvoiceId, Amount = 100m, UserId = 1, PaymentDate = DateTime.Now };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeletePaymentAsync(payment.PaymentId, "Audit Reason");

            // Assert - Logic Guard: Verify balance recalculated correctly
            var updated = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updated.Should().NotBeNull();
            updated!.Paid.Should().Be(0m);
            updated.Balance.Should().Be(200m);

            // Side effect: Verify payment deleted
            var deleted = await _db.Payments.FindAsync(payment.PaymentId);
            deleted.Should().BeNull();

            // Audit Trail Verification (Function 2.6)
            var log = await _db.AuditLogs.FirstOrDefaultAsync(l => l.Action == "DELETE_PAYMENT" && l.RecordId == payment.PaymentId.ToString());
            log.Should().NotBeNull();
            log!.NewValues.Should().Contain("Audit Reason");
            log.NewValues.Should().Contain("100"); // Amount deleted
        }

        [Fact]
        public async Task DeletePaymentAsync_NonExistent_Should_Return_Without_Throwing()
        {
            // Act - The service may not throw for non-existent payment
            await _service.DeletePaymentAsync(99999, "Reason");

            // Assert - Just verify no exception is thrown
            true.Should().BeTrue();
        }

        // 2.7 - Additional Charges - Financial Integrity Tests
        [Fact]
        public async Task AddAdditionalChargeAsync_Should_Create_Charge_Record()
        {
            // Arrange
            var patient = new Patient { LabId = "LCH1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();
            var invoice = new Invoice { VisitId = visit.VisitId, Total = 100m, Discount = 0m, NetTotal = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            await _service.AddAdditionalChargeAsync(invoice.InvoiceId, "Urgent Fee", 25m);

            // Assert - Logic Guard: Verify charge record created
            var charge = await _db.AdditionalCharges.FirstOrDefaultAsync(c => c.InvoiceId == invoice.InvoiceId);
            charge.Should().NotBeNull();
            charge!.Description.Should().Be("Urgent Fee");
            charge.Amount.Should().Be(25m);
        }

        [Fact]
        public async Task AddAdditionalChargeAsync_NegativeAmount_Should_Throw()
        {
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 100m, Discount = 0m, NetTotal = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddAdditionalChargeAsync(invoice.InvoiceId, "Fee", -10m);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task LogInvoicePrintedAsync_Should_Create_AuditLog_Entry()
        {
            // Arrange
            var invoiceId = 100;
            var userId = 1;

            // Act
            await _service.LogInvoicePrintedAsync(invoiceId, userId);

            // Assert
            var log = await _db.AuditLogs.FirstOrDefaultAsync(l => l.TableName == "Invoice" && l.Action == "Print" && l.RecordId == "100");
            log.Should().NotBeNull();
            log!.UserId.Should().Be(userId);
        }
    }
}
