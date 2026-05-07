using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    /// <summary>
    /// Completion tests for Module 1 (Patient Management) and Module 2 (Financial Accounting)
    /// Service layer — fills all missing Success / Failure / Edge scenarios so both modules
    /// reach 100 % coverage per UnitTest_Skill.md.
    /// </summary>
    public class Module1And2ServiceCompletionTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly PatientService _patientService;
        private readonly VisitService _visitService;
        private readonly InvoiceService _invoiceService;
        private readonly PatientSearchService _searchService;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module1And2ServiceCompletionTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _patientService = new PatientService(_db);
            _visitService = new VisitService(_db);
            _invoiceService = new InvoiceService(_db);
            _searchService = new PatientSearchService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.2 — Edit Patient Data — missing Service Success test
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task EditPatientData_WithValidBirthDate_ShouldUpdateSuccessfully_SuccessGuard()
        {
            // Function: 1.2 — Edit Patient Data
            // Arrange
            var patient = new Patient { LabId = "ED-001", FullName = "Original", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var updated = new Patient
            {
                PatientId = patient.PatientId,
                LabId = "ED-001",
                FullName = "Updated Name",
                Gender = "Female",
                BirthDate = new DateTime(1995, 6, 15)
            };

            // Act
            await _patientService.UpdateAsync(updated);

            // Assert
            var persisted = await _db.Patients.FindAsync(patient.PatientId);
            persisted.Should().NotBeNull();
            persisted!.FullName.Should().Be("Updated Name");
            persisted.Gender.Should().Be("Female");
            persisted.BirthDate.Should().Be(new DateTime(1995, 6, 15));
        }

        [Fact]
        public async Task EditPatientData_WithNullBirthDate_ShouldClearBirthDate_EdgeGuard()
        {
            // Function: 1.2 — Edit Patient Data
            // Arrange
            var patient = new Patient
            {
                LabId = "ED-002",
                FullName = "Has Birth",
                Gender = "Male",
                BirthDate = new DateTime(1980, 1, 1)
            };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var updated = new Patient
            {
                PatientId = patient.PatientId,
                LabId = "ED-002",
                FullName = "Has Birth",
                Gender = "Male",
                BirthDate = null
            };

            // Act
            await _patientService.UpdateAsync(updated);

            // Assert
            var persisted = await _db.Patients.FindAsync(patient.PatientId);
            persisted.Should().NotBeNull();
            persisted!.PatientId.Should().BeGreaterThan(0);
            persisted!.BirthDate.Should().BeNull();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.3 — Add Tests to Patient — missing ViewModel-side Edge (Service layer only here)
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task AddTestToVisit_WithCashAccount_ShouldUseBasePriceWhenNoPriceList_SuccessGuard()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var patient = new Patient { LabId = "AT-001", FullName = "Cash Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, AccountType = "Cash" };
            _db.Visits.Add(visit);
            var test = new Test { Code = "AT1", NameReport = "Test A", Price = 75m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Act
            var vt = await _visitService.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            vt.Should().NotBeNull();
            vt.Price.Should().Be(75m);
            vt.Status.Should().Be("Pending");
        }

        [Fact]
        public async Task AddTestToVisit_WithDefaultPriceList_ShouldUseListPrice_SuccessGuard()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var patient = new Patient { LabId = "AT-002", FullName = "PL Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            var test = new Test { Code = "AT2", NameReport = "Test B", Price = 200m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var pl = new PriceList { Name = "Standard", IsDefault = true };
            _db.PriceLists.Add(pl);
            await _db.SaveChangesAsync();
            _db.PriceListItems.Add(new PriceListItem { PriceListId = pl.PriceListId, TestId = test.TestId, Price = 130m });
            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, AccountType = "Cash" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var vt = await _visitService.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            vt.Price.Should().Be(130m, "default price list overrides base test price");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.4 — Delete Tests — missing Edge: delete last test from visit
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveVisitTest_WhenLastTestInVisit_ShouldLeaveVisitEmpty_EdgeGuard()
        {
            // Function: 1.4 — Delete Tests
            // Arrange
            var patient = new Patient { LabId = "DEL-001", FullName = "Last Test Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 50m, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            await _visitService.RemoveVisitTestAsync(vt.VisitTestId);

            // Assert
            var remaining = await _db.VisitTests.CountAsync(v => v.VisitId == visit.VisitId);
            remaining.Should().Be(0, "removing the last test should leave the visit with zero tests");
        }

        [Fact]
        public async Task RemoveVisitTest_WithInProgressStatus_ShouldDelete_SuccessGuard()
        {
            // Function: 1.4 — Delete Tests
            // Arrange
            var patient = new Patient { LabId = "DEL-002", FullName = "InProgress Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = 2, Price = 90m, Status = "InProgress" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            await _visitService.RemoveVisitTestAsync(vt.VisitTestId);

            // Assert
            var deleted = await _db.VisitTests.AnyAsync(v => v.VisitTestId == vt.VisitTestId);
            deleted.Should().BeFalse("in-progress tests that are not verified should be deletable");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.5 — Search Patient — missing Success test for name-only search
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SearchPatient_ByNameOnly_ShouldReturnMatchingPatients_SuccessGuard()
        {
            // Function: 1.5 — Search Patient
            // Arrange
            _db.Patients.AddRange(
                new Patient { LabId = "SRN-001", FullName = "Khalid Omar", Gender = "Male", Phone = "111" },
                new Patient { LabId = "SRN-002", FullName = "Hala Nour", Gender = "Female", Phone = "222" });
            await _db.SaveChangesAsync();

            // Act
            var results = await _searchService.SearchPatientsAsync("Khalid", null, null);

            // Assert
            results.Should().ContainSingle();
            results[0].FullName.Should().Be("Khalid Omar");
        }

        [Fact]
        public async Task SearchPatient_ByPhoneOnly_ShouldReturnExactMatch_SuccessGuard()
        {
            // Function: 1.5 — Search Patient
            // Arrange
            _db.Patients.AddRange(
                new Patient { LabId = "SRP-001", FullName = "Amr", Gender = "Male", Phone = "0501234567" },
                new Patient { LabId = "SRP-002", FullName = "Dina", Gender = "Female", Phone = "0509999999" });
            await _db.SaveChangesAsync();

            // Act
            var results = await _searchService.SearchPatientsAsync(null, "0501234567", null);

            // Assert
            results.Should().ContainSingle();
            results[0].Phone.Should().Be("0501234567");
        }

        [Fact]
        public async Task SearchPatient_WithNoMatchingPhone_ShouldReturnEmpty_FailureGuard()
        {
            // Function: 1.5 — Search Patient
            // Arrange
            _db.Patients.Add(new Patient { LabId = "SRP-010", FullName = "Someone", Gender = "Male", Phone = "0500000001" });
            await _db.SaveChangesAsync();

            // Act
            var results = await _searchService.SearchPatientsAsync(null, "9999999999", null);

            // Assert
            results.Should().BeEmpty();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.7 — Add Medical History — missing Service Edge: null patient ID
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SaveMedicalHistory_WithZeroPatientId_ShouldThrow_FailureGuard()
        {
            // Function: 1.7 — Add Medical History
            // Arrange
            var history = new MedicalHistory { ChronicDiseases = "Asthma" };

            // Act
            Func<Task> act = async () => await _patientService.SaveMedicalHistoryAsync(0, history);

            // Assert
            await act.Should().ThrowAsync<Exception>("patient ID 0 is invalid and should be rejected");
        }

        [Fact]
        public async Task SaveMedicalHistory_UpdateExisting_ShouldReplaceAllFields_SuccessGuard()
        {
            // Function: 1.7 — Add Medical History
            // Arrange
            var patient = new Patient { LabId = "MH-UPD", FullName = "Hist Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var existing = new MedicalHistory
            {
                PatientId = patient.PatientId,
                ChronicDiseases = "Old Disease",
                Medications = "Old Med"
            };
            _db.MedicalHistories.Add(existing);
            await _db.SaveChangesAsync();

            // Act
            await _patientService.SaveMedicalHistoryAsync(patient.PatientId, new MedicalHistory
            {
                ChronicDiseases = "New Disease",
                Medications = "New Med",
                Allergies = "Penicillin",
                Notes = "Updated"
            });

            // Assert
            var saved = await _db.MedicalHistories.FirstOrDefaultAsync(m => m.PatientId == patient.PatientId);
            saved.Should().NotBeNull();
            saved!.ChronicDiseases.Should().Be("New Disease");
            saved.Medications.Should().Be("New Med");
            saved.Allergies.Should().Be("Penicillin");
            saved.Notes.Should().Be("Updated");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.8 — Add Group of Tests — missing: Success with group price override
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task AddGroupOfTests_WhenGroupHasThreeTests_ShouldAddAllThree_SuccessGuard()
        {
            // Function: 1.8 — Add Group of Tests
            // Arrange
            var patient = new Patient { LabId = "GRP-S1", FullName = "Group Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            _db.Visits.Add(visit);

            var t1 = new Test { Code = "G10", NameReport = "G10", Price = 10m };
            var t2 = new Test { Code = "G11", NameReport = "G11", Price = 20m };
            var t3 = new Test { Code = "G12", NameReport = "G12", Price = 30m };
            _db.Tests.AddRange(t1, t2, t3);
            await _db.SaveChangesAsync();

            var group = new CustomGroup { Name = "Full Profile", Price = 0m };
            _db.CustomGroups.Add(group);
            await _db.SaveChangesAsync();

            _db.CustomGroupItems.AddRange(
                new CustomGroupItem { CustomGroupId = group.CustomGroupId, TestId = t1.TestId },
                new CustomGroupItem { CustomGroupId = group.CustomGroupId, TestId = t2.TestId },
                new CustomGroupItem { CustomGroupId = group.CustomGroupId, TestId = t3.TestId });
            await _db.SaveChangesAsync();

            // Act
            var added = await _visitService.AddCustomGroupToVisitAsync(visit.VisitId, group.CustomGroupId);

            // Assert
            added.Should().HaveCount(3);
            var inVisit = await _db.VisitTests.CountAsync(vt => vt.VisitId == visit.VisitId);
            inVisit.Should().Be(3);
        }

        [Fact]
        public async Task AddGroupOfTests_WhenAllTestsAlreadyInVisit_ShouldAddNone_EdgeGuard()
        {
            // Function: 1.8 — Add Group of Tests
            // Arrange
            var patient = new Patient { LabId = "GRP-E1", FullName = "Already Has", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            _db.Visits.Add(visit);
            var test = new Test { Code = "G20", NameReport = "G20", Price = 50m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Pre-add the test to visit
            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 50m, Status = "Pending" });

            var group = new CustomGroup { Name = "Single Profile", Price = 0m };
            _db.CustomGroups.Add(group);
            await _db.SaveChangesAsync();

            _db.CustomGroupItems.Add(new CustomGroupItem { CustomGroupId = group.CustomGroupId, TestId = test.TestId });
            await _db.SaveChangesAsync();

            // Act
            var added = await _visitService.AddCustomGroupToVisitAsync(visit.VisitId, group.CustomGroupId);

            // Assert
            added.Should().BeEmpty("all tests in the group already exist in the visit");
            var totalInVisit = await _db.VisitTests.CountAsync(vt => vt.VisitId == visit.VisitId);
            totalInVisit.Should().Be(1, "no duplicates should be created");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.3 — Record Payment — missing: Success guard
        // ────────────────────────────────────────────────────────────────────────
        [Fact]
        public async Task AddPaymentAsync_With_Valid_Data_Should_RecordPayment_SuccessGuard()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            var invoice = new Invoice
            {
                VisitId = 5,
                Total = 500m,
                Discount = 0m,
                NetTotal = 500m,
                Paid = 0m,
                Balance = 500m
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            var payment = await _invoiceService.AddPaymentAsync(invoice.InvoiceId, 200m, "Cash", 1);

            // Assert
            payment.Should().NotBeNull();
            payment.Amount.Should().Be(200m);
            payment.PaymentMethod.Should().Be("Cash");

            var updatedInvoice = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updatedInvoice!.Paid.Should().Be(200m);
            updatedInvoice.Balance.Should().Be(300m);
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.5 — Edit Payment — missing Success edge: edit to higher amount
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task EditPayment_ToHigherAmount_ShouldRecalculateBalance_SuccessGuard()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            var invoice = new Invoice
            {
                VisitId = 10,
                Total = 300m,
                Discount = 0m,
                NetTotal = 300m,
                Paid = 100m,
                Balance = 200m
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                Amount = 100m,
                UserId = 1,
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            var edited = await _invoiceService.EditPaymentAsync(payment.PaymentId, 200m, 1, "Correcting amount");

            // Assert
            edited.Amount.Should().Be(200m);
            var updatedInvoice = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updatedInvoice!.Paid.Should().Be(200m);
            updatedInvoice.Balance.Should().Be(100m);
        }

        [Fact]
        public async Task EditPayment_ToLowerAmount_ShouldIncreaseBalance_EdgeGuard()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            var invoice = new Invoice
            {
                VisitId = 11,
                Total = 200m,
                Discount = 0m,
                NetTotal = 200m,
                Paid = 150m,
                Balance = 50m
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                Amount = 150m,
                UserId = 2,
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            var edited = await _invoiceService.EditPaymentAsync(payment.PaymentId, 50m, 2, "Reduced payment");

            // Assert
            edited.Amount.Should().Be(50m);
            var updatedInvoice = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updatedInvoice!.Paid.Should().Be(50m);
            updatedInvoice.Balance.Should().Be(150m);
        }

        [Fact]
        public async Task EditPayment_WithNegativeAmount_ShouldThrow_FailureGuard()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            var invoice = new Invoice
            {
                VisitId = 12,
                Total = 100m,
                Discount = 0m,
                NetTotal = 100m,
                Paid = 50m,
                Balance = 50m
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                Amount = 50m,
                UserId = 1,
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () =>
                await _invoiceService.EditPaymentAsync(payment.PaymentId, -10m, 1, "Negative amount");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>("negative payment amount should be rejected");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.6 — Delete Payment — missing: Success guard (additional scenario)
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeletePayment_WhenMultiplePaymentsExist_ShouldDeleteOnlyTargetPayment_SuccessGuard()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            var invoice = new Invoice
            {
                VisitId = 20,
                Total = 300m,
                Discount = 0m,
                NetTotal = 300m,
                Paid = 200m,
                Balance = 100m
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var p1 = new Payment { InvoiceId = invoice.InvoiceId, Amount = 100m, UserId = 1, PaymentDate = DateTime.Now.AddHours(-2) };
            var p2 = new Payment { InvoiceId = invoice.InvoiceId, Amount = 100m, UserId = 1, PaymentDate = DateTime.Now.AddHours(-1) };
            _db.Payments.AddRange(p1, p2);
            await _db.SaveChangesAsync();

            // Act
            await _invoiceService.DeletePaymentAsync(p1.PaymentId, 1, "Deleting first payment");

            // Assert
            var deletedP1 = await _db.Payments.FindAsync(p1.PaymentId);
            deletedP1.Should().BeNull("target payment should be deleted");

            var remainingP2 = await _db.Payments.FindAsync(p2.PaymentId);
            remainingP2.Should().NotBeNull("other payments should remain untouched");

            var updatedInvoice = await _db.Invoices.FindAsync(invoice.InvoiceId);
            updatedInvoice!.Paid.Should().Be(100m, "invoice paid should reflect remaining payment only");
        }

        [Fact]
        public async Task DeletePayment_WithWhitespaceReason_ShouldThrow_EdgeGuard()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            var invoice = new Invoice
            {
                VisitId = 21,
                Total = 100m,
                Discount = 0m,
                NetTotal = 100m,
                Paid = 60m,
                Balance = 40m
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                Amount = 60m,
                UserId = 1,
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () =>
                await _invoiceService.DeletePaymentAsync(payment.PaymentId, 1, "   ");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>("whitespace reason should be treated as missing reason");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.5 / 2.6 — Additional audit log verification tests
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task EditPayment_ShouldCreateAuditLog_WithReasonAndUserId_SuccessGuard()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            var invoice = new Invoice
            {
                VisitId = 30,
                Total = 500m,
                Discount = 0m,
                NetTotal = 500m,
                Paid = 250m,
                Balance = 250m
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                Amount = 250m,
                UserId = 5,
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            await _invoiceService.EditPaymentAsync(payment.PaymentId, 300m, 5, "Audit verification test");

            // Assert
            var log = await _db.AuditLogs.FirstOrDefaultAsync(
                l => l.Action == "EDIT_PAYMENT" && l.RecordId == payment.PaymentId.ToString());
            log.Should().NotBeNull("editing a payment must create an audit trail");
            log!.NewValues.Should().Contain("Audit verification test");
            log.UserId.Should().Be(5);
        }

        [Fact]
        public async Task DeletePayment_ShouldCreateAuditLog_SuccessGuard()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            var invoice = new Invoice
            {
                VisitId = 31,
                Total = 150m,
                Discount = 0m,
                NetTotal = 150m,
                Paid = 75m,
                Balance = 75m
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                Amount = 75m,
                UserId = 3,
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            var paymentId = payment.PaymentId;

            // Act
            await _invoiceService.DeletePaymentAsync(paymentId, 3, "Audit delete test");

            // Assert
            var log = await _db.AuditLogs.FirstOrDefaultAsync(
                l => l.Action == "DELETE_PAYMENT" && l.RecordId == paymentId.ToString());
            log.Should().NotBeNull("deleting a payment must create an audit trail");
            log!.NewValues.Should().Contain("Audit delete test");
        }
    }
}
