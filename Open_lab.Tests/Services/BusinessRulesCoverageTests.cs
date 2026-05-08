using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;
using Xunit;

namespace Open_lab.Tests.Services
{
    /// <summary>
    /// Dedicated tests for the 7 Business Rules (BR tags) that were
    /// documented in Open_lab_Modules_Documentation.md but had no
    /// explicit test coverage:
    ///   BR-VAL-001, BR-VAL-004, BR-ACC-003, BR-ACC-005,
    ///   BR-ACC-006, BR-OPS-002, BR-OPS-004
    /// </summary>
    public class BusinessRulesCoverageTests
    {
        // ───────────────────────────────────────────────
        //  Helper: InMemory DbContext
        // ───────────────────────────────────────────────

        private static OpenLabDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<OpenLabDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new OpenLabDbContext(options);
        }

        // ═══════════════════════════════════════════════
        //  BR-VAL-001  — البيانات الأساسية إلزامية قبل الحفظ
        //  Function: 1.1 — Add New Patient
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task CreatePatient_WithMissingFullName_ShouldThrowArgumentException_BR_VAL_001()
        {
            // Function: 1.1 — Add New Patient
            // Arrange
            using var db = CreateDb();
            var service = new PatientService(db);
            var patient = new Patient
            {
                FullName = "",      // required but empty
                Gender = "Male",
                LabId = "LAB001"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(patient));

            // Assert
            Assert.Contains("FullName", ex.Message);
        }

        [Fact]
        public async Task CreatePatient_WithMissingGender_ShouldThrowArgumentException_BR_VAL_001()
        {
            // Function: 1.1 — Add New Patient
            // Arrange
            using var db = CreateDb();
            var service = new PatientService(db);
            var patient = new Patient
            {
                FullName = "Ahmed Ali",
                Gender = "",        // required but empty
                LabId = "LAB002"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(patient));

            // Assert
            Assert.Contains("Gender", ex.Message);
        }

        [Fact]
        public async Task CreatePatient_WithNullPatient_ShouldThrowArgumentNullException_BR_VAL_001()
        {
            // Function: 1.1 — Add New Patient
            // Arrange
            using var db = CreateDb();
            var service = new PatientService(db);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreatePatient_WithAllRequiredFields_ShouldSaveSuccessfully_BR_VAL_001()
        {
            // Function: 1.1 — Add New Patient
            // Arrange
            using var db = CreateDb();
            var service = new PatientService(db);
            var patient = new Patient
            {
                FullName = "Ahmed Ali",
                Gender = "Male",
                LabId = "LAB003"
            };

            // Act
            var result = await service.CreateAsync(patient);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ahmed Ali", result.FullName);
            Assert.Equal("Male", result.Gender);
            Assert.True(result.PatientId > 0);
        }

        // ═══════════════════════════════════════════════
        //  BR-VAL-004  — منع الحذف بعد تسجيل النتيجة
        //  Function: 1.4 — Delete Tests
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task RemoveVisitTest_WhenTestIsVerified_ShouldThrowInvalidOperation_BR_VAL_004()
        {
            // Function: 1.4 — Delete Tests
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Test Patient", Gender = "Male", LabId = "LAB100" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var test = new Test { NameReport = "CBC", Price = 100 };
            db.Tests.Add(test);
            await db.SaveChangesAsync();

            var visitTest = new VisitTest
            {
                VisitId = visit.VisitId,
                TestId = test.TestId,
                Price = 100,
                Status = "Verified"      // Result already entered/verified
            };
            db.VisitTests.Add(visitTest);
            await db.SaveChangesAsync();

            var service = new VisitService(db);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RemoveVisitTestAsync(visitTest.VisitTestId));

            // Assert
            Assert.Contains("Verified", ex.Message);
        }

        [Fact]
        public async Task RemoveVisitTest_WhenTestIsPending_ShouldRemoveSuccessfully_BR_VAL_004()
        {
            // Function: 1.4 — Delete Tests
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Test Patient", Gender = "Male", LabId = "LAB101" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var test = new Test { NameReport = "FBS", Price = 50 };
            db.Tests.Add(test);
            await db.SaveChangesAsync();

            var visitTest = new VisitTest
            {
                VisitId = visit.VisitId,
                TestId = test.TestId,
                Price = 50,
                Status = "Pending"       // No result entered yet
            };
            db.VisitTests.Add(visitTest);
            await db.SaveChangesAsync();

            var service = new VisitService(db);

            // Act
            await service.RemoveVisitTestAsync(visitTest.VisitTestId);

            // Assert
            var exists = await db.VisitTests.AnyAsync(vt => vt.VisitTestId == visitTest.VisitTestId);
            Assert.False(exists);
        }

        // ═══════════════════════════════════════════════
        //  BR-ACC-003  — تسجيل الدفع فور التأكيد في قاعدة البيانات
        //  Function: 2.3 — Record Payment
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task AddPayment_WithValidData_ShouldPersistImmediately_BR_ACC_003()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Payment Patient", Gender = "Male", LabId = "LAB200" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 500, Discount = 0, NetTotal = 500, Paid = 0, Balance = 500, Status = "Partial" };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            var service = new InvoiceService(db);

            // Act
            var payment = await service.AddPaymentAsync(invoice.InvoiceId, 200m, "Cash", userId: 1);

            // Assert — payment must be persisted immediately in DB (BR-ACC-003)
            var savedPayment = await db.Payments.FirstOrDefaultAsync(p => p.PaymentId == payment.PaymentId);
            Assert.NotNull(savedPayment);
            Assert.Equal(200m, savedPayment.Amount);
            Assert.Equal("Cash", savedPayment.PaymentMethod);
            Assert.Equal(1, savedPayment.UserId);
            Assert.True(savedPayment.PaymentDate <= DateTime.Now);
        }

        [Fact]
        public async Task AddPayment_WithZeroAmount_ShouldThrowArgumentException_BR_ACC_003()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Payment Patient", Gender = "Male", LabId = "LAB201" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 500, Discount = 0, NetTotal = 500, Paid = 0, Balance = 500, Status = "Partial" };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            var service = new InvoiceService(db);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => service.AddPaymentAsync(invoice.InvoiceId, 0m, "Cash", userId: 1));

            // Assert
            Assert.Contains("Amount", ex.Message);
        }

        // ═══════════════════════════════════════════════
        //  BR-ACC-005  — توثيق جميع التعديلات المالية في سجلات الأمان
        //  Functions: 2.5 — Edit Payment, 2.6 — Delete Payment
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task EditPayment_ShouldCreateAuditLogEntry_BR_ACC_005()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Audit Patient", Gender = "Male", LabId = "LAB300" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 1000, Discount = 0, NetTotal = 1000, Paid = 300, Balance = 700, Status = "Partial" };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            var payment = new Payment { InvoiceId = invoice.InvoiceId, Amount = 300, PaymentMethod = "Cash", PaymentDate = DateTime.Now, UserId = 1 };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            var service = new InvoiceService(db);

            // Act
            await service.EditPaymentAsync(payment.PaymentId, 500m, userId: 2, reason: "Correction");

            // Assert — audit log entry must exist (BR-ACC-005)
            var auditLog = await db.AuditLogs
                .FirstOrDefaultAsync(a => a.Action == "EDIT_PAYMENT" && a.RecordId == payment.PaymentId.ToString());
            Assert.NotNull(auditLog);
            Assert.Equal(2, auditLog.UserId);
            Assert.Equal("Payments", auditLog.TableName);
            Assert.Contains("Correction", auditLog.NewValues);
            Assert.Contains("500", auditLog.NewValues);
        }

        [Fact]
        public async Task DeletePayment_ShouldCreateAuditLogEntry_BR_ACC_005()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Delete Audit", Gender = "Female", LabId = "LAB301" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 800, Discount = 0, NetTotal = 800, Paid = 200, Balance = 600, Status = "Partial" };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            var payment = new Payment { InvoiceId = invoice.InvoiceId, Amount = 200, PaymentMethod = "Visa", PaymentDate = DateTime.Now, UserId = 1 };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
            var paymentId = payment.PaymentId;

            var service = new InvoiceService(db);

            // Act
            await service.DeletePaymentAsync(paymentId, userId: 3, reason: "Entered by mistake");

            // Assert — audit log entry must exist (BR-ACC-005)
            var auditLog = await db.AuditLogs
                .FirstOrDefaultAsync(a => a.Action == "DELETE_PAYMENT" && a.RecordId == paymentId.ToString());
            Assert.NotNull(auditLog);
            Assert.Equal(3, auditLog.UserId);
            Assert.Contains("Entered by mistake", auditLog.NewValues);
            Assert.Contains("200", auditLog.NewValues);
        }

        [Fact]
        public async Task EditPayment_AfterSettlement_ShouldThrowInvalidOperation_BR_ACC_005()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Settled Patient", Gender = "Male", LabId = "LAB302" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Closed" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 500, Discount = 0, NetTotal = 500, Paid = 500, Balance = 0, Status = "Settled" };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            var payment = new Payment { InvoiceId = invoice.InvoiceId, Amount = 500, PaymentMethod = "Cash", PaymentDate = DateTime.Now, UserId = 1 };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            var service = new InvoiceService(db);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.EditPaymentAsync(payment.PaymentId, 600m, userId: 2, reason: "Late correction"));

            // Assert
            Assert.Contains("settlement", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ═══════════════════════════════════════════════
        //  BR-ACC-006  — شمول الخصومات وصافي الربح في تقارير الجرد المالي
        //  Function: 2.10 — Generate Inventory
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task GetSnapshot_ShouldIncludeTotalDiscountAndNetProfit_BR_ACC_006()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Inventory Patient", Gender = "Male", LabId = "LAB400" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(2026, 1, 15), Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var invoice = new Invoice
            {
                VisitId = visit.VisitId,
                Total = 1000,
                Discount = 150,          // Discount exists
                NetTotal = 850,
                Paid = 850,
                Balance = 0,
                Status = "Paid"
            };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            var service = new AccountsTreasuryService(db);
            var from = new DateTime(2026, 1, 1);
            var to = new DateTime(2026, 1, 31);

            // Act
            var snapshot = await service.GetSnapshotAsync(from, to);

            // Assert — TotalDiscount and NetProfit must be included (BR-ACC-006)
            Assert.Equal(150m, snapshot.TotalDiscount);
            Assert.Equal(850m, snapshot.TotalPaid);
            Assert.Equal(850m, snapshot.TotalInvoiced);
            // NetProfit = TotalPaid - TotalExpenses (no expenses in this test)
            Assert.Equal(850m, snapshot.NetProfit);
        }

        [Fact]
        public async Task GetSnapshot_WithExpenses_ShouldCalculateNetProfitCorrectly_BR_ACC_006()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Profit Patient", Gender = "Female", LabId = "LAB401" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(2026, 3, 10), Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var invoice = new Invoice
            {
                VisitId = visit.VisitId,
                Total = 2000,
                Discount = 200,
                NetTotal = 1800,
                Paid = 1800,
                Balance = 0,
                Status = "Paid"
            };
            db.Invoices.Add(invoice);

            db.Expenses.Add(new Expense { Description = "Reagents", Amount = 500, Date = new DateTime(2026, 3, 10) });
            db.Expenses.Add(new Expense { Description = "Electricity", Amount = 100, Date = new DateTime(2026, 3, 15) });
            await db.SaveChangesAsync();

            var service = new AccountsTreasuryService(db);
            var from = new DateTime(2026, 3, 1);
            var to = new DateTime(2026, 3, 31);

            // Act
            var snapshot = await service.GetSnapshotAsync(from, to);

            // Assert — NetProfit = Paid (1800) - Expenses (500+100=600) = 1200
            Assert.Equal(200m, snapshot.TotalDiscount);
            Assert.Equal(600m, snapshot.TotalExpenses);
            Assert.Equal(1200m, snapshot.NetProfit);
        }

        // ═══════════════════════════════════════════════
        //  BR-OPS-002  — مراجعة الرصيد قبل إغلاق الحساب
        //  Function: 2.4 — Settle Account
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task SettleAccount_WithOutstandingBalance_ShouldThrowInvalidOperation_BR_OPS_002()
        {
            // Function: 2.4 — Settle Account
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Balance Patient", Gender = "Male", LabId = "LAB500" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var test = new Test { NameReport = "Lipid Profile", Price = 300 };
            db.Tests.Add(test);
            await db.SaveChangesAsync();

            db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 300, Status = "Pending" });
            await db.SaveChangesAsync();

            // Create invoice with balance remaining (paid 100 of 300)
            var invoice = new Invoice { VisitId = visit.VisitId, Total = 300, Discount = 0, NetTotal = 300, Paid = 100, Balance = 200, Status = "Partial" };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            var service = new InvoiceService(db);

            // Act & Assert — cannot settle with remaining balance (BR-OPS-002)
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SettleAccountAsync(visit.VisitId));

            // Assert
            Assert.Contains("balance", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SettleAccount_WithZeroBalance_ShouldSettleSuccessfully_BR_OPS_002()
        {
            // Function: 2.4 — Settle Account
            // Arrange
            using var db = CreateDb();
            var patient = new Patient { FullName = "Settled OK", Gender = "Female", LabId = "LAB501" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            var test = new Test { NameReport = "HbA1c", Price = 200 };
            db.Tests.Add(test);
            await db.SaveChangesAsync();

            db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 200, Status = "Pending" });
            await db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 200, Discount = 0, NetTotal = 200, Paid = 0, Balance = 200, Status = "Partial" };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            // Pay full amount so balance becomes zero
            db.Payments.Add(new Payment { InvoiceId = invoice.InvoiceId, Amount = 200, PaymentMethod = "Cash", PaymentDate = DateTime.Now, UserId = 1 });
            await db.SaveChangesAsync();

            var service = new InvoiceService(db);

            // Act
            var result = await service.SettleAccountAsync(visit.VisitId);

            // Assert
            Assert.Equal("Settled", result.Status);
        }

        // ═══════════════════════════════════════════════
        //  BR-OPS-004  — الربح = سعر المريض − سعر التكلفة
        //  Function: 2.13 — Lab-to-Lab Settlement
        // ═══════════════════════════════════════════════

        [Fact]
        public async Task GetTotalProfit_ShouldCalculatePatientPriceMinusCostPrice_BR_OPS_004()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            using var db = CreateDb();

            var referral = new Referral { Name = "External Lab A", ReferralType = "Lab" };
            db.Referrals.Add(referral);
            await db.SaveChangesAsync();

            var patient = new Patient { FullName = "External Patient", Gender = "Male", LabId = "LAB600" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open", ReferralId = referral.ReferralId };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            // Test with Patient Price = 500, Cost Price = 300 → Profit per test = 200
            var test = new Test { NameReport = "Thyroid Panel", Price = 500, CostPrice = 300, IsSendOut = true };
            db.Tests.Add(test);
            await db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 500, Status = "Pending" };
            db.VisitTests.Add(visitTest);
            await db.SaveChangesAsync();

            // Queue it as shipped to external lab
            db.ExternalLabQueues.Add(new ExternalLabQueue
            {
                VisitTestId = visitTest.VisitTestId,
                ReferralId = referral.ReferralId,
                Status = "Shipped",
                DateQueued = DateTime.Now
            });
            await db.SaveChangesAsync();

            var service = new ExternalSettlementService(db);

            // Act
            var profit = await service.GetTotalProfitAsync(referral.ReferralId);

            // Assert — Profit = Patient Price (500) - Cost Price (300) = 200 (BR-OPS-004)
            Assert.Equal(200m, profit);
        }

        [Fact]
        public async Task GetPendingBalance_ShouldCalculateTotalCostMinusTotalPaid_BR_OPS_004()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            using var db = CreateDb();

            var referral = new Referral { Name = "External Lab B", ReferralType = "Lab" };
            db.Referrals.Add(referral);
            await db.SaveChangesAsync();

            var patient = new Patient { FullName = "Balance Patient", Gender = "Female", LabId = "LAB601" };
            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open", ReferralId = referral.ReferralId };
            db.Visits.Add(visit);
            await db.SaveChangesAsync();

            // Two tests sent externally: CostPrice = 200 + 150 = 350
            var test1 = new Test { NameReport = "Vitamin D", Price = 400, CostPrice = 200, IsSendOut = true };
            var test2 = new Test { NameReport = "Vitamin B12", Price = 350, CostPrice = 150, IsSendOut = true };
            db.Tests.AddRange(test1, test2);
            await db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 400, Status = "Pending" };
            var vt2 = new VisitTest { VisitId = visit.VisitId, TestId = test2.TestId, Price = 350, Status = "Pending" };
            db.VisitTests.AddRange(vt1, vt2);
            await db.SaveChangesAsync();

            db.ExternalLabQueues.Add(new ExternalLabQueue { VisitTestId = vt1.VisitTestId, ReferralId = referral.ReferralId, Status = "Shipped", DateQueued = DateTime.Now });
            db.ExternalLabQueues.Add(new ExternalLabQueue { VisitTestId = vt2.VisitTestId, ReferralId = referral.ReferralId, Status = "Received", DateQueued = DateTime.Now });
            await db.SaveChangesAsync();

            // Already paid 100 of the 350 total cost
            db.ExternalLabSettlements.Add(new ExternalLabSettlement
            {
                ReferralId = referral.ReferralId,
                AmountPaid = 100,
                TotalCost = 350,
                Balance = 250,
                SettlementDate = DateTime.Now
            });
            await db.SaveChangesAsync();

            var service = new ExternalSettlementService(db);

            // Act
            var pendingBalance = await service.GetPendingBalanceAsync(referral.ReferralId);

            // Assert — Pending = TotalCost (200+150=350) - TotalPaid (100) = 250
            Assert.Equal(250m, pendingBalance);
        }
    }
}
