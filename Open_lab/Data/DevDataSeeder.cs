#if DEBUG
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;

namespace Open_lab.Data
{
    /// <summary>
    /// Seeds realistic test data for development/debugging only.
    /// This entire class is excluded from Release builds via #if DEBUG.
    /// </summary>
    public static class DevDataSeeder
    {
        public static async Task SeedAsync(OpenLabDbContext db)
        {
            // Guard: skip if data already exists
            if (await db.Patients.AnyAsync())
            {
                return;
            }

            // ── Referral (جهة تحويل) ──────────────────────────
            var referral = new Referral
            {
                ReferralType = "Hospital",
                Name = "مستشفى النيل التخصصي",
                Phone = "0227654321",
                City = "القاهرة",
                DiscountPercentage = 10m,
                CommissionPercentage = 5m
            };
            db.Referrals.Add(referral);

            // ── Physician (طبيب محوّل) ────────────────────────
            var physician = new Physician
            {
                FullName = "د. أحمد سعيد",
                Phone = "01012345678",
                Specialty = "باطنة",
                IsActive = true
            };
            db.Physicians.Add(physician);

            // ── Tests (التحاليل) ──────────────────────────────
            var testCBC = new Test
            {
                Code = "CBC",
                NameReport = "Complete Blood Count",
                NameReceipt = "صورة دم كاملة",
                Price = 120m,
                TurnaroundHours = 2,
                IsRoutine = true
            };

            var testSugar = new Test
            {
                Code = "GLU",
                NameReport = "Blood Glucose (Fasting)",
                NameReceipt = "سكر دم صائم",
                Price = 60m,
                TurnaroundHours = 1,
                IsRoutine = true
            };

            var testLiver = new Test
            {
                Code = "LFT",
                NameReport = "Liver Function Tests",
                NameReceipt = "وظائف الكبد",
                Price = 200m,
                TurnaroundHours = 4,
                IsRoutine = true
            };

            var testUrine = new Test
            {
                Code = "UA",
                NameReport = "Urine Analysis",
                NameReceipt = "تحليل بول كامل",
                Price = 50m,
                TurnaroundHours = 1,
                IsRoutine = true
            };

            var testKidney = new Test
            {
                Code = "KFT",
                NameReport = "Kidney Function Tests",
                NameReceipt = "وظائف الكلى",
                Price = 180m,
                TurnaroundHours = 3,
                IsRoutine = true
            };

            db.Tests.AddRange(testCBC, testSugar, testLiver, testUrine, testKidney);

            // ── Test Parameters (باراميترات التحاليل) ──────────
            var paramHgb = new TestParameter { Test = testCBC, Name = "HGB", OrderNo = 1 };
            var paramWbc = new TestParameter { Test = testCBC, Name = "WBC", OrderNo = 2 };
            var paramRbc = new TestParameter { Test = testCBC, Name = "RBC", OrderNo = 3 };
            var paramPlt = new TestParameter { Test = testCBC, Name = "PLT", OrderNo = 4 };

            var paramGlu = new TestParameter { Test = testSugar, Name = "Glucose", OrderNo = 1 };

            var paramAlt = new TestParameter { Test = testLiver, Name = "ALT (SGPT)", OrderNo = 1 };
            var paramAst = new TestParameter { Test = testLiver, Name = "AST (SGOT)", OrderNo = 2 };
            var paramAlb = new TestParameter { Test = testLiver, Name = "Albumin", OrderNo = 3 };

            var paramColor = new TestParameter { Test = testUrine, Name = "Color", OrderNo = 1 };
            var paramPh = new TestParameter { Test = testUrine, Name = "pH", OrderNo = 2 };

            var paramCreat = new TestParameter { Test = testKidney, Name = "Creatinine", OrderNo = 1 };
            var paramUrea = new TestParameter { Test = testKidney, Name = "Urea", OrderNo = 2 };

            db.TestParameters.AddRange(
                paramHgb, paramWbc, paramRbc, paramPlt,
                paramGlu,
                paramAlt, paramAst, paramAlb,
                paramColor, paramPh,
                paramCreat, paramUrea);

            // ── Patients & Visits ─────────────────────────────

            // ═══ Patient 1: محمد عبد الرحمن — ذكر بالغ، حساب مسدد بالكامل، نتائج مسلّمة ═══
            var patient1 = new Patient
            {
                LabId = "LAB-2026-001",
                FullName = "محمد عبد الرحمن أحمد",
                Gender = "Male",
                Age = 35,
                BirthDate = new DateTime(1991, 3, 15),
                Phone = "01001234567",
                NationalId = "29103151234567",
                Address = "12 شارع التحرير، الدقي، الجيزة",
                ReferralId = null
            };
            db.Patients.Add(patient1);
            await db.SaveChangesAsync(); // Save to get IDs

            var visit1 = new Visit
            {
                PatientId = patient1.PatientId,
                VisitDate = DateTime.Today.AddDays(-3),
                AccountType = "Cash",
                Status = "Completed",
                PhysicianId = physician.PhysicianId
            };
            db.Visits.Add(visit1);
            await db.SaveChangesAsync();

            var vt1_cbc = new VisitTest { VisitId = visit1.VisitId, TestId = testCBC.TestId, Price = 120m, Status = "Delivered" };
            var vt1_sugar = new VisitTest { VisitId = visit1.VisitId, TestId = testSugar.TestId, Price = 60m, Status = "Delivered" };
            var vt1_liver = new VisitTest { VisitId = visit1.VisitId, TestId = testLiver.TestId, Price = 200m, Status = "Delivered" };
            db.VisitTests.AddRange(vt1_cbc, vt1_sugar, vt1_liver);
            await db.SaveChangesAsync();

            // Results for patient 1 — all entered and verified
            db.ResultValues.AddRange(
                new ResultValue { VisitTestId = vt1_cbc.VisitTestId, ParameterId = paramHgb.ParameterId, Value = "14.2", Flag = "Normal" },
                new ResultValue { VisitTestId = vt1_cbc.VisitTestId, ParameterId = paramWbc.ParameterId, Value = "7500", Flag = "Normal" },
                new ResultValue { VisitTestId = vt1_cbc.VisitTestId, ParameterId = paramRbc.ParameterId, Value = "5.1", Flag = "Normal" },
                new ResultValue { VisitTestId = vt1_cbc.VisitTestId, ParameterId = paramPlt.ParameterId, Value = "250000", Flag = "Normal" },
                new ResultValue { VisitTestId = vt1_sugar.VisitTestId, ParameterId = paramGlu.ParameterId, Value = "95", Flag = "Normal" },
                new ResultValue { VisitTestId = vt1_liver.VisitTestId, ParameterId = paramAlt.ParameterId, Value = "25", Flag = "Normal" },
                new ResultValue { VisitTestId = vt1_liver.VisitTestId, ParameterId = paramAst.ParameterId, Value = "22", Flag = "Normal" },
                new ResultValue { VisitTestId = vt1_liver.VisitTestId, ParameterId = paramAlb.ParameterId, Value = "4.1", Flag = "Normal" }
            );

            // Invoice — fully paid
            var inv1 = new Invoice
            {
                VisitId = visit1.VisitId,
                Total = 380m,
                Discount = 0m,
                NetTotal = 380m,
                Paid = 380m,
                Balance = 0m,
                Status = "Closed"
            };
            db.Invoices.Add(inv1);
            await db.SaveChangesAsync();

            db.Payments.Add(new Payment
            {
                InvoiceId = inv1.InvoiceId,
                Amount = 380m,
                PaymentMethod = "Cash",
                PaymentDate = visit1.VisitDate,
                UserId = 1
            });

            // ═══ Patient 2: فاطمة حسن — أنثى بالغة، نتائج جاهزة لم تستلمها بعد ═══
            var patient2 = new Patient
            {
                LabId = "LAB-2026-002",
                FullName = "فاطمة حسن إبراهيم",
                Gender = "Female",
                Age = 28,
                BirthDate = new DateTime(1998, 7, 22),
                Phone = "01112345678",
                Address = "5 شارع الهرم، الجيزة",
                IsVip = true
            };
            db.Patients.Add(patient2);
            await db.SaveChangesAsync();

            var visit2 = new Visit
            {
                PatientId = patient2.PatientId,
                VisitDate = DateTime.Today.AddDays(-1),
                AccountType = "Cash",
                Status = "Open",
                ReferralId = referral.ReferralId
            };
            db.Visits.Add(visit2);
            await db.SaveChangesAsync();

            var vt2_cbc = new VisitTest { VisitId = visit2.VisitId, TestId = testCBC.TestId, Price = 108m, Status = "Verified" };
            var vt2_kidney = new VisitTest { VisitId = visit2.VisitId, TestId = testKidney.TestId, Price = 162m, Status = "Verified" };
            var vt2_urine = new VisitTest { VisitId = visit2.VisitId, TestId = testUrine.TestId, Price = 45m, Status = "Verified" };
            db.VisitTests.AddRange(vt2_cbc, vt2_kidney, vt2_urine);
            await db.SaveChangesAsync();

            // Results for patient 2 — entered & verified, not delivered
            db.ResultValues.AddRange(
                new ResultValue { VisitTestId = vt2_cbc.VisitTestId, ParameterId = paramHgb.ParameterId, Value = "11.8", Flag = "Low" },
                new ResultValue { VisitTestId = vt2_cbc.VisitTestId, ParameterId = paramWbc.ParameterId, Value = "6200", Flag = "Normal" },
                new ResultValue { VisitTestId = vt2_cbc.VisitTestId, ParameterId = paramRbc.ParameterId, Value = "4.2", Flag = "Normal" },
                new ResultValue { VisitTestId = vt2_cbc.VisitTestId, ParameterId = paramPlt.ParameterId, Value = "310000", Flag = "Normal" },
                new ResultValue { VisitTestId = vt2_kidney.VisitTestId, ParameterId = paramCreat.ParameterId, Value = "0.9", Flag = "Normal" },
                new ResultValue { VisitTestId = vt2_kidney.VisitTestId, ParameterId = paramUrea.ParameterId, Value = "28", Flag = "Normal" },
                new ResultValue { VisitTestId = vt2_urine.VisitTestId, ParameterId = paramColor.ParameterId, Value = "Yellow", Flag = "Normal" },
                new ResultValue { VisitTestId = vt2_urine.VisitTestId, ParameterId = paramPh.ParameterId, Value = "6.0", Flag = "Normal" }
            );

            // Invoice — fully paid, waiting delivery only
            var inv2 = new Invoice
            {
                VisitId = visit2.VisitId,
                Total = 315m,
                Discount = 31.50m,
                NetTotal = 283.50m,
                Paid = 283.50m,
                Balance = 0m,
                Status = "Open"
            };
            db.Invoices.Add(inv2);
            await db.SaveChangesAsync();

            db.Payments.Add(new Payment
            {
                InvoiceId = inv2.InvoiceId,
                Amount = 283.50m,
                PaymentMethod = "Cash",
                PaymentDate = visit2.VisitDate,
                UserId = 1
            });

            // ═══ Patient 3: يوسف كريم — طفل (8 أشهر)، نتائج لم تدخل بعد ═══
            var patient3 = new Patient
            {
                LabId = "LAB-2026-003",
                FullName = "يوسف كريم محمود",
                Gender = "Male",
                Age = 0,
                BirthDate = DateTime.Today.AddMonths(-8),
                Phone = "01223456789",
                Address = "8 شارع الجمهورية، المنصورة"
            };
            db.Patients.Add(patient3);
            await db.SaveChangesAsync();

            var visit3 = new Visit
            {
                PatientId = patient3.PatientId,
                VisitDate = DateTime.Today,
                AccountType = "Cash",
                Status = "Open",
                PhysicianId = physician.PhysicianId
            };
            db.Visits.Add(visit3);
            await db.SaveChangesAsync();

            // Tests pending — no results entered yet
            db.VisitTests.AddRange(
                new VisitTest { VisitId = visit3.VisitId, TestId = testCBC.TestId, Price = 120m, Status = "Pending" },
                new VisitTest { VisitId = visit3.VisitId, TestId = testSugar.TestId, Price = 60m, Status = "Pending" }
            );

            var inv3 = new Invoice
            {
                VisitId = visit3.VisitId,
                Total = 180m,
                Discount = 0m,
                NetTotal = 180m,
                Paid = 180m,
                Balance = 0m,
                Status = "Open"
            };
            db.Invoices.Add(inv3);
            await db.SaveChangesAsync();

            db.Payments.Add(new Payment
            {
                InvoiceId = inv3.InvoiceId,
                Amount = 180m,
                PaymentMethod = "Cash",
                PaymentDate = visit3.VisitDate,
                UserId = 1
            });

            // ═══ Patient 4: أحمد سمير — له زيارتان في تواريخ مختلفة ═══
            var patient4 = new Patient
            {
                LabId = "LAB-2026-004",
                FullName = "أحمد سمير عبد الله",
                Gender = "Male",
                Age = 45,
                BirthDate = new DateTime(1981, 11, 5),
                Phone = "01098765432",
                NationalId = "28111051234567",
                Address = "22 شارع رمسيس، القاهرة"
            };
            db.Patients.Add(patient4);
            await db.SaveChangesAsync();

            // Visit A — older visit, fully completed
            var visit4a = new Visit
            {
                PatientId = patient4.PatientId,
                VisitDate = DateTime.Today.AddDays(-15),
                AccountType = "Cash",
                Status = "Completed"
            };
            db.Visits.Add(visit4a);
            await db.SaveChangesAsync();

            var vt4a_liver = new VisitTest { VisitId = visit4a.VisitId, TestId = testLiver.TestId, Price = 200m, Status = "Delivered" };
            var vt4a_kidney = new VisitTest { VisitId = visit4a.VisitId, TestId = testKidney.TestId, Price = 180m, Status = "Delivered" };
            db.VisitTests.AddRange(vt4a_liver, vt4a_kidney);
            await db.SaveChangesAsync();

            db.ResultValues.AddRange(
                new ResultValue { VisitTestId = vt4a_liver.VisitTestId, ParameterId = paramAlt.ParameterId, Value = "55", Flag = "High" },
                new ResultValue { VisitTestId = vt4a_liver.VisitTestId, ParameterId = paramAst.ParameterId, Value = "48", Flag = "High" },
                new ResultValue { VisitTestId = vt4a_liver.VisitTestId, ParameterId = paramAlb.ParameterId, Value = "3.5", Flag = "Normal" },
                new ResultValue { VisitTestId = vt4a_kidney.VisitTestId, ParameterId = paramCreat.ParameterId, Value = "1.3", Flag = "Normal" },
                new ResultValue { VisitTestId = vt4a_kidney.VisitTestId, ParameterId = paramUrea.ParameterId, Value = "42", Flag = "High" }
            );

            var inv4a = new Invoice
            {
                VisitId = visit4a.VisitId,
                Total = 380m,
                Discount = 0m,
                NetTotal = 380m,
                Paid = 380m,
                Balance = 0m,
                Status = "Closed"
            };
            db.Invoices.Add(inv4a);
            await db.SaveChangesAsync();

            db.Payments.Add(new Payment
            {
                InvoiceId = inv4a.InvoiceId,
                Amount = 380m,
                PaymentMethod = "Cash",
                PaymentDate = visit4a.VisitDate,
                UserId = 1
            });

            // Visit B — recent follow-up, results partially entered
            var visit4b = new Visit
            {
                PatientId = patient4.PatientId,
                VisitDate = DateTime.Today,
                AccountType = "Cash",
                Status = "Open",
                PhysicianId = physician.PhysicianId
            };
            db.Visits.Add(visit4b);
            await db.SaveChangesAsync();

            var vt4b_liver = new VisitTest { VisitId = visit4b.VisitId, TestId = testLiver.TestId, Price = 200m, Status = "Completed" };
            var vt4b_sugar = new VisitTest { VisitId = visit4b.VisitId, TestId = testSugar.TestId, Price = 60m, Status = "Pending" };
            db.VisitTests.AddRange(vt4b_liver, vt4b_sugar);
            await db.SaveChangesAsync();

            // Only liver results entered for visit B, sugar still pending
            db.ResultValues.AddRange(
                new ResultValue { VisitTestId = vt4b_liver.VisitTestId, ParameterId = paramAlt.ParameterId, Value = "38", Flag = "Normal" },
                new ResultValue { VisitTestId = vt4b_liver.VisitTestId, ParameterId = paramAst.ParameterId, Value = "30", Flag = "Normal" },
                new ResultValue { VisitTestId = vt4b_liver.VisitTestId, ParameterId = paramAlb.ParameterId, Value = "3.8", Flag = "Normal" }
            );

            var inv4b = new Invoice
            {
                VisitId = visit4b.VisitId,
                Total = 260m,
                Discount = 0m,
                NetTotal = 260m,
                Paid = 260m,
                Balance = 0m,
                Status = "Open"
            };
            db.Invoices.Add(inv4b);
            await db.SaveChangesAsync();

            db.Payments.Add(new Payment
            {
                InvoiceId = inv4b.InvoiceId,
                Amount = 260m,
                PaymentMethod = "Cash",
                PaymentDate = visit4b.VisitDate,
                UserId = 1
            });

            // ═══ Patient 5: نورهان علي — لها باقي حساب عند المعمل ═══
            var patient5 = new Patient
            {
                LabId = "LAB-2026-005",
                FullName = "نورهان علي مصطفى",
                Gender = "Female",
                Age = 32,
                BirthDate = new DateTime(1994, 1, 10),
                Phone = "01556789012",
                Address = "15 شارع المعز، القاهرة",
                IsPregnant = true
            };
            db.Patients.Add(patient5);
            await db.SaveChangesAsync();

            var visit5 = new Visit
            {
                PatientId = patient5.PatientId,
                VisitDate = DateTime.Today.AddDays(-2),
                AccountType = "Cash",
                Status = "Open",
                ReferralId = referral.ReferralId,
                PhysicianId = physician.PhysicianId
            };
            db.Visits.Add(visit5);
            await db.SaveChangesAsync();

            var vt5_cbc = new VisitTest { VisitId = visit5.VisitId, TestId = testCBC.TestId, Price = 108m, Status = "Verified" };
            var vt5_urine = new VisitTest { VisitId = visit5.VisitId, TestId = testUrine.TestId, Price = 45m, Status = "Completed" };
            var vt5_sugar = new VisitTest { VisitId = visit5.VisitId, TestId = testSugar.TestId, Price = 54m, Status = "Pending" };
            var vt5_kidney = new VisitTest { VisitId = visit5.VisitId, TestId = testKidney.TestId, Price = 162m, Status = "Pending" };
            db.VisitTests.AddRange(vt5_cbc, vt5_urine, vt5_sugar, vt5_kidney);
            await db.SaveChangesAsync();

            // CBC results entered & verified; Urine entered not verified; Sugar & Kidney pending
            db.ResultValues.AddRange(
                new ResultValue { VisitTestId = vt5_cbc.VisitTestId, ParameterId = paramHgb.ParameterId, Value = "12.5", Flag = "Normal" },
                new ResultValue { VisitTestId = vt5_cbc.VisitTestId, ParameterId = paramWbc.ParameterId, Value = "9800", Flag = "Normal" },
                new ResultValue { VisitTestId = vt5_cbc.VisitTestId, ParameterId = paramRbc.ParameterId, Value = "4.0", Flag = "Normal" },
                new ResultValue { VisitTestId = vt5_cbc.VisitTestId, ParameterId = paramPlt.ParameterId, Value = "280000", Flag = "Normal" },
                new ResultValue { VisitTestId = vt5_urine.VisitTestId, ParameterId = paramColor.ParameterId, Value = "Pale Yellow", Flag = "Normal" },
                new ResultValue { VisitTestId = vt5_urine.VisitTestId, ParameterId = paramPh.ParameterId, Value = "5.5", Flag = "Normal" }
            );

            // Invoice — partially paid (balance remaining)
            var inv5 = new Invoice
            {
                VisitId = visit5.VisitId,
                Total = 369m,
                Discount = 36.90m,
                NetTotal = 332.10m,
                Paid = 150m,
                Balance = 182.10m,
                Status = "Open"
            };
            db.Invoices.Add(inv5);
            await db.SaveChangesAsync();

            db.Payments.Add(new Payment
            {
                InvoiceId = inv5.InvoiceId,
                Amount = 150m,
                PaymentMethod = "Cash",
                PaymentDate = visit5.VisitDate,
                UserId = 1
            });

            await db.SaveChangesAsync();
        }
    }
}
#endif
