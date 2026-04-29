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
            // Function: 2.8 — Generate Invoice
            // Arrange
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

            // Assert
            invoice.Total.Should().Be(200m);
            invoice.Discount.Should().Be(50m);
            invoice.NetTotal.Should().Be(150m);
            invoice.Paid.Should().Be(0m);
            invoice.Balance.Should().Be(150m);

            // Assert
            var saved = await _db.Invoices.FindAsync(invoice.InvoiceId);
            saved.Should().NotBeNull();
            saved!.VisitId.Should().Be(visit.VisitId);
        }

        [Fact]
        public async Task AddPaymentAsync_InvalidAmount_Should_Throw_LogicGuard()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 100m, Discount = 0m, NetTotal = 100m, Paid = 0m, Balance = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddPaymentAsync(invoice.InvoiceId, 0m, "Cash", 1);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
            var unchanged = await _db.Invoices.FindAsync(invoice.InvoiceId);
            unchanged.Should().NotBeNull();
            unchanged!.Paid.Should().Be(0m);
            unchanged.Balance.Should().Be(100m);
        }

        [Fact]
        public async Task AddAdditionalChargeAsync_InvalidDescription_Should_Throw_LogicGuard()
        {
            // Function: 2.7 — Add Additional Charge
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 100m, Discount = 0m, NetTotal = 100m, Paid = 0m, Balance = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddAdditionalChargeAsync(invoice.InvoiceId, "   ", 10m);

            // Assert
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
            // Function: 2.1 — Calculate Total
            // Arrange
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

            // Assert
            total.Should().Be(175m);
            var visitTests = await _db.VisitTests.Where(vt => vt.VisitId == visit.VisitId).ToListAsync();
            visitTests.Should().ContainSingle();
            visitTests[0].Price.Should().Be(150m);

            var charges = await _db.AdditionalCharges.Where(c => c.InvoiceId == invoice.InvoiceId).ToListAsync();
            charges.Should().ContainSingle();
            charges[0].Amount.Should().Be(25m);
        }

        [Fact]
        public async Task GetVisitTotalAsync_When_VisitNotFound_Should_Return_Zero_FailureGuard()
        {
            // Function: 2.1 — Calculate Total
            // Arrange
            var nonExistentVisitId = 99999;

            // Act
            var total = await _service.GetVisitTotalAsync(nonExistentVisitId);

            // Assert
            total.Should().Be(0m);
        }

        [Fact]
        public async Task GetVisitTotalAsync_With_NoTestsOrCharges_Should_Return_Zero_EdgeGuard()
        {
            // Function: 2.1 — Calculate Total
            // Arrange
            var patient = new Patient { LabId = "L2", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var total = await _service.GetVisitTotalAsync(visit.VisitId);

            // Assert
            total.Should().Be(0m);
        }

        [Fact]
        public async Task EditPaymentAsync_Should_Update_Amount_And_Recalculate_Invoice_LogicGuard()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 200m, Discount = 0m, NetTotal = 200m, Paid = 50m, Balance = 150m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            var payment = new Payment { InvoiceId = invoice.InvoiceId, Amount = 50m, UserId = 1, PaymentDate = DateTime.Now.AddMinutes(-10) };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            var edited = await _service.EditPaymentAsync(payment.PaymentId, 120m, 2, "Test Reason");

            // Assert
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

            var log = await _db.AuditLogs.FirstOrDefaultAsync(l => l.Action == "EDIT_PAYMENT" && l.RecordId == payment.PaymentId.ToString());
            log.Should().NotBeNull();
            log!.NewValues.Should().Contain("Test Reason");
            log.UserId.Should().Be(2);
        }


        [Fact]
        public async Task GetPatientAccount_ByDate_Should_Return_Invoices_And_Payments_For_Same_Patient_LogicGuard()
        {
            // Function: 2.9 — View Patient Account
            // Arrange
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

            // Assert
            invoices.Should().ContainSingle();
            invoices[0].InvoiceId.Should().Be(invoice.InvoiceId);
            invoices[0].Total.Should().Be(100m);
            invoices[0].NetTotal.Should().Be(100m);

            payments.Should().ContainSingle();
            payments[0].Amount.Should().Be(60m);
            payments[0].InvoiceId.Should().Be(invoice.InvoiceId);
        }

        [Fact]
        public async Task GetPatientAccount_When_PatientNotFound_Should_Return_Empty_FailureGuard()
        {
            // Function: 2.9 — View Patient Account
            // Arrange
            var nonExistentPatientId = 99999;

            // Act
            var invoices = await _service.GetPatientInvoicesByDateAsync(nonExistentPatientId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            var payments = await _service.GetPatientPaymentsByDateAsync(nonExistentPatientId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            invoices.Should().BeEmpty();
            payments.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPatientAccount_With_DateRangeOutside_VisitDate_Should_Return_Empty_EdgeGuard()
        {
            // Function: 2.9 — View Patient Account
            // Arrange
            var patient = new Patient { LabId = "LACC2", FullName = "Account P2", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 100m, Discount = 0m, NetTotal = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act - query for a date range that doesn't include the visit date
            var invoices = await _service.GetPatientInvoicesByDateAsync(patient.PatientId, DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-5));
            var payments = await _service.GetPatientPaymentsByDateAsync(patient.PatientId, DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-5));

            // Assert
            invoices.Should().BeEmpty();
            payments.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_WithZeroDiscount_Should_Calculate_Correctly()
        {
            // Function: 2.2 — Apply Discount
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
            // Function: 2.2 — Apply Discount
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
            // Function: 2.2 — Apply Discount
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
            // Function: 2.3 — Record Payment
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
            // Function: 2.2 — Apply Discount
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

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*الخصم لا يمكن أن يتجاوز إجمالي الفاتورة*");
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_FullPayment_Should_Set_Status_To_Paid()
        {
            // Function: 2.4 — Settle Account
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
            // Function: 2.3 — Record Payment
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

        [Fact]
        public async Task AddPaymentAsync_PartialPayment_Should_Update_Balance_Correctly()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 200m, Discount = 0m, NetTotal = 200m, Paid = 0m, Balance = 200m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            await _service.AddPaymentAsync(invoice.InvoiceId, 50m, "Cash", 1);

            // Assert
            var updated = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updated.Should().NotBeNull();
            updated!.Paid.Should().Be(50m);
            updated.Balance.Should().Be(150m);
            updated.Status.Should().Be("Partial");
        }

        [Fact]
        public async Task AddPaymentAsync_When_PaymentExceedsBalance_Should_Allow_Overpayment_EdgeGuard()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 100m, Discount = 0m, NetTotal = 100m, Paid = 0m, Balance = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act - pay more than balance
            await _service.AddPaymentAsync(invoice.InvoiceId, 150m, "Cash", 1);

            // Assert
            var updated = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updated.Should().NotBeNull();
            updated!.Paid.Should().Be(150m);
            updated.Balance.Should().Be(0m); // Production code caps balance at 0, not negative
            updated.Status.Should().Be("Paid");

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.InvoiceId == invoice.InvoiceId);
            payment.Should().NotBeNull();
            payment!.Amount.Should().Be(150m);
        }

        [Fact]
        public async Task DeletePaymentAsync_Should_Recalculate_Balance()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 200m, Discount = 0m, NetTotal = 200m, Paid = 100m, Balance = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            var payment = new Payment { InvoiceId = invoice.InvoiceId, Amount = 100m, UserId = 1, PaymentDate = DateTime.Now };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeletePaymentAsync(payment.PaymentId, 1, "Audit Reason");

            // Assert
            var updated = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updated.Should().NotBeNull();
            updated!.Paid.Should().Be(0m);
            updated.Balance.Should().Be(200m);

            var deleted = await _db.Payments.FindAsync(payment.PaymentId);
            deleted.Should().BeNull();

            var log = await _db.AuditLogs.FirstOrDefaultAsync(l => l.Action == "DELETE_PAYMENT" && l.RecordId == payment.PaymentId.ToString());
            log.Should().NotBeNull();
            log!.NewValues.Should().Contain("Audit Reason");
            log.NewValues.Should().Contain("100"); // Amount deleted
        }

        [Fact]
        public async Task DeletePaymentAsync_NonExistent_Should_Return_Without_Throwing()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            var auditBefore = await _db.AuditLogs.CountAsync();
            var paymentsBefore = await _db.Payments.CountAsync();

            // Act
            await _service.DeletePaymentAsync(99999, 1, "Reason");

            // Assert
            var auditAfter = await _db.AuditLogs.CountAsync();
            var paymentsAfter = await _db.Payments.CountAsync();
            auditAfter.Should().Be(auditBefore);
            paymentsAfter.Should().Be(paymentsBefore);
        }

        [Fact]
        public async Task DeletePaymentAsync_Without_Reason_Should_Throw()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 200m, Discount = 0m, NetTotal = 200m, Paid = 100m, Balance = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            var payment = new Payment { InvoiceId = invoice.InvoiceId, Amount = 100m, PaymentMethod = "Card", UserId = 1, PaymentDate = DateTime.Now };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.DeletePaymentAsync(payment.PaymentId, 1, "");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Reason is required*");
        }

        [Fact]
        public async Task AddAdditionalChargeAsync_Should_Create_Charge_Record()
        {
            // Function: 2.7 — Add Additional Charge
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

            // Assert
            var charge = await _db.AdditionalCharges.FirstOrDefaultAsync(c => c.InvoiceId == invoice.InvoiceId);
            charge.Should().NotBeNull();
            charge!.Description.Should().Be("Urgent Fee");
            charge.Amount.Should().Be(25m);
        }

        [Fact]
        public async Task AddAdditionalChargeAsync_NegativeAmount_Should_Throw()
        {
            // Function: 2.7 — Add Additional Charge
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
            // Function: 2.8 — Generate Invoice
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

        [Fact]
        public async Task LogInvoicePrintedAsync_With_InvalidInvoiceId_Should_Still_Create_Log_FailureGuard()
        {
            // Function: 2.8 — Generate Invoice
            // Arrange
            var invalidInvoiceId = -1;
            var userId = 1;

            // Act
            await _service.LogInvoicePrintedAsync(invalidInvoiceId, userId);

            // Assert
            var log = await _db.AuditLogs.FirstOrDefaultAsync(l => l.TableName == "Invoice" && l.Action == "Print" && l.RecordId == "-1");
            log.Should().NotBeNull();
            log!.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task SettleAccountAsync_When_BalanceRemaining_Should_Throw_FailureGuard()
        {
            // Function: 2.4 — Settle Account
            // Arrange
            var patient = new Patient { LabId = "LSET1", FullName = "Settle P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();
            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 120m });
            await _db.SaveChangesAsync();
            await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 0m, 0m);

            // Act
            Func<Task> act = async () => await _service.SettleAccountAsync(visit.VisitId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*remaining balance*");
        }

        [Fact]
        public async Task SettleAccountAsync_When_FullyPaid_Should_CloseVisit_EdgeGuard()
        {
            // Function: 2.4 — Settle Account
            // Arrange
            var patient = new Patient { LabId = "LSET2", FullName = "Settle P2", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();
            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 80m });
            await _db.SaveChangesAsync();
            var invoice = await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 0m, 0m);
            await _service.AddPaymentAsync(invoice.InvoiceId, 80m, "Cash", 1);

            // Act
            var settled = await _service.SettleAccountAsync(visit.VisitId);

            // Assert
            settled.Status.Should().Be("Settled");
            settled.Visit!.Status.Should().Be("Closed");
            settled.Balance.Should().Be(0m);
        }

        [Fact]
        public async Task CalculateReferralDiscountAsync_When_ReferralMissing_Should_Return_Zero_EdgeGuard()
        {
            // Function: 2.2 — Apply Discount
            // Arrange

            // Act
            var discount = await _service.CalculateReferralDiscountAsync(99999, 500m);

            // Assert
            discount.Should().Be(0m);
        }

        [Fact]
        public async Task CalculateReferralDiscountAsync_With_Valid_Referral_Should_Apply_Discount_LogicGuard()
        {
            // Function: 2.2 — Apply Discount
            // Arrange
            var patient = new Patient { LabId = "LREF1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var referral = new Referral { ReferralId = 1, Name = "Referral A", DiscountPercentage = 10m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, ReferralId = referral.ReferralId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { TestId = 1, NameReceipt = "CBC", Price = 100m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 100m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            // Act
            var discount = await _service.CalculateReferralDiscountAsync(visit.VisitId, 100m);

            // Assert - BR-ACC-001/BR-ACC-002: Referral discount applied (10% of 100 = 10)
            discount.Should().Be(10m);
        }
    }
}
