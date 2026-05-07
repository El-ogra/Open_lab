using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class FunctionCoverageGapTests : IDisposable
    {
        private readonly OpenLabDbContext _db;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public FunctionCoverageGapTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task Patient_And_Visit_Functions_Should_Work_EndToEnd()
        {
            // Function: 1.1 — , 1.3, 1.4
            var patientService = new PatientService(_db);
            var visitService = new VisitService(_db);

            var patient = await patientService.CreateAsync(new Patient { FullName = "Patient A", Gender = "Male" });
            var visit = await visitService.CreateAsync(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now });
            var test = new Test { Code = "GLU", NameReport = "Glucose", NameReceipt = "Glucose", Price = 50m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = await visitService.AddTestToVisitAsync(visit.VisitId, test.TestId);
            visitTest.Price.Should().Be(50m);

            await visitService.RemoveVisitTestAsync(visitTest.VisitTestId);
            (await _db.VisitTests.AnyAsync(vt => vt.VisitTestId == visitTest.VisitTestId)).Should().BeFalse();
        }

        [Fact]
        public async Task Finance_Gap_Functions_Should_Work()
        {
            // Function: 2.1 — , 2.2, 2.5, 2.9
            var invoiceService = new InvoiceService(_db);

            var patient = new Patient { LabId = "L-FIN", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = SeedTest("T-FIN").TestId, Price = 200m, Status = "Pending" });
            await _db.SaveChangesAsync();

            var invoice = await invoiceService.CreateOrUpdateInvoiceAsync(visit.VisitId, discount: 20m, paid: 0m);
            invoice.Total.Should().Be(200m);
            invoice.Discount.Should().Be(20m);
            invoice.NetTotal.Should().Be(180m);

            var payment = await invoiceService.AddPaymentAsync(invoice.InvoiceId, 50m, paymentMethod: "Cash", userId: 1);
            await invoiceService.EditPaymentAsync(payment.PaymentId, 70m, userId: 2, reason: "Correction");

            var updated = await _db.Payments.FindAsync(payment.PaymentId);
            updated!.Amount.Should().Be(70m);

            var invoices = await invoiceService.GetPatientInvoicesByDateAsync(patient.PatientId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            invoices.Should().ContainSingle();
        }

        [Fact]
        public async Task Catalog_Gap_Functions_Should_Work()
        {
            // Function: 3.1 — , 3.2, 3.4, 3.5, 3.6, 3.8, 3.9
            var catalog = new TestCatalogService(_db);

            var test = await catalog.CreateTestAsync(new Test
            {
                Code = "CBC",
                NameReport = "Complete Blood Count",
                NameReceipt = "CBC",
                Price = 120m
            });

            test.NameReport = "CBC Updated";
            await catalog.UpdateTestAsync(test);

            var group = await catalog.CreateCustomGroupAsync(new CustomGroup { Name = "Profile A", Price = 200m });
            await catalog.AddCustomGroupItemAsync(new CustomGroupItem { CustomGroupId = group.CustomGroupId, TestId = test.TestId });

            var list = await catalog.CreatePriceListAsync(new PriceList { Name = "PL-A", IsDefault = true });
            var item = await catalog.AddPriceListItemAsync(new PriceListItem { PriceListId = list.PriceListId, TestId = test.TestId, Price = 99m });
            item.Price = 89m;
            await catalog.UpdatePriceListItemAsync(item);

            var comment = await catalog.CreateTestCommentAsync(new TestComment { TestId = test.TestId, CommentText = "High value comment", IsDefault = true });
            comment.CommentText = "Updated comment";
            await catalog.UpdateTestCommentAsync(comment);

            test.IsSendOut = true;
            await catalog.UpdateTestAsync(test);

            var saved = await catalog.GetTestByIdAsync(test.TestId);
            saved!.IsSendOut.Should().BeTrue();
        }

        [Fact]
        public async Task Results_And_Preview_Gap_Functions_Should_Work()
        {
            // Function: 4.1 — , 4.2, 4.6
            var resultsService = new ResultsService(_db);
            var reportService = new ReportService(_db);

            var test = SeedTest("T-RSL");
            var patient = new Patient { LabId = "L-RSL", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var param = new TestParameter { TestId = test.TestId, Name = "Value", OrderNo = 1 };
            _db.TestParameters.Add(param);
            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m, Status = "Pending" };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            await resultsService.SaveResultAsync(visitTest.VisitTestId, param.ParameterId, "5.5", "N", "ok");
            var saved = await _db.VisitTests.FindAsync(visitTest.VisitTestId);
            saved!.Status.Should().Be("Completed");

            var report = await reportService.GetVisitReportAsync(visit.VisitId);
            report.Should().NotBeNull();
            report!.Tests.Should().ContainSingle();
        }

        [Fact]
        public async Task Culture_Gap_Functions_Should_Work()
        {
            // Function: 5.1 — , 5.2, 5.3, 5.6
            var service = new CultureSensitivityService(_db);

            var culture = await service.CreateCultureAsync(new Culture
            {
                Name = "Urine Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 140
            });
            var antibiotic = await service.CreateAntibioticAsync(new Antibiotic { Name = "AB-1", IsSafeForChildren = true });
            await service.LinkAntibioticAsync(culture.CultureId, antibiotic.AntibioticId);

            var linked = await service.GetCultureAntibioticsAsync(culture.CultureId);
            linked.Should().ContainSingle();

            var patient = new Patient { LabId = "L-CUL", FullName = "Child", Gender = "Male", Age = 8 };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            var test = SeedTest("CULT");
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 100m, Status = "Pending" };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            await service.SaveCultureResultAsync(visitTest.VisitTestId, culture.CultureId, new List<CultureSensitivityValue>
            {
                new CultureSensitivityValue { AntibioticId = antibiotic.AntibioticId, Sensitivity = "S", Comment = "good" }
            });

            var filtered = await service.GetFilteredAntibioticsAsync(visitTest.VisitTestId);
            filtered.Should().Contain(a => a.AntibioticId == antibiotic.AntibioticId);
        }

        [Fact]
        public async Task Sample_Collection_Gap_Functions_Should_Work()
        {
            // Function: 6.1 — , 6.2
            var service = new SampleCollectionService(_db);
            var patient = new Patient { LabId = "L-SMP", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            var test = SeedTest("SMP");
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            await service.MarkCollectedAsync(visitTest.VisitTestId, userId: 1);
            await service.MarkSeparatedAsync(visitTest.VisitTestId, "Serum");

            var row = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == visitTest.VisitTestId);
            row.Should().NotBeNull();
            row!.IsSeparated.Should().BeTrue();
        }

        [Fact]
        public async Task Worksheet_Gap_Functions_Should_Work()
        {
            // Function: 7.1 — , 7.2
            var service = new WorksheetService(_db);
            var patient = new Patient { LabId = "L-WS", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            var test = SeedTest("WS");
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m, Status = "Pending" });
            await _db.SaveChangesAsync();

            var byPatient = await service.GetWorksheetByPatientAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            var byTest = await service.GetWorksheetByTestAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            byPatient.Should().ContainSingle();
            byTest.Should().ContainSingle();
        }

        [Fact]
        public async Task External_Lab_Gap_Functions_Should_Work()
        {
            // Function: 8.1 — , 8.2, 8.3, 8.4
            var visitService = new VisitService(_db);
            var externalService = new ExternalLabService(_db);

            var referral = new Referral { Name = "Ref Lab", ReferralType = "Lab" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-EXT", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = await visitService.CreateAsync(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, ReferralId = referral.ReferralId });
            var test = SeedTest("EXT");
            test.IsSendOut = true;
            await _db.SaveChangesAsync();

            var visitTest = await visitService.AddTestToVisitAsync(visit.VisitId, test.TestId);
            var queueItem = await _db.ExternalLabQueues.FirstOrDefaultAsync(q => q.VisitTestId == visitTest.VisitTestId);
            queueItem.Should().NotBeNull();

            var manifest = await externalService.CreateManifestAsync(referral.ReferralId, new List<int> { queueItem!.QueueId }, "courier");
            manifest.ManifestId.Should().BeGreaterThan(0);

            await externalService.UpdateQueueStatusAsync(queueItem.QueueId, "Sent", "EXT-REF-1");
            var updated = await _db.ExternalLabQueues.FindAsync(queueItem.QueueId);
            updated!.Status.Should().Be("Sent");
            updated.ExternalReference.Should().Be("EXT-REF-1");
        }

        [Fact]
        public async Task Statistics_Gap_Functions_Should_Work()
        {
            // Function: 9.1 — , 9.2, 9.3, 9.4, 9.5
            var service = new StatisticsService(_db);

            var referral = new Referral { Name = "Referral", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var p1 = new Patient { LabId = "L-ST1", FullName = "M", Gender = "Male" };
            var p2 = new Patient { LabId = "L-ST2", FullName = "F", Gender = "Female" };
            _db.Patients.AddRange(p1, p2);
            await _db.SaveChangesAsync();

            var test = SeedTest("ST");
            await _db.SaveChangesAsync();

            var v1 = new Visit { PatientId = p1.PatientId, ReferralId = referral.ReferralId, VisitDate = new DateTime(DateTime.Today.Year, 1, 10) };
            var v2 = new Visit { PatientId = p2.PatientId, ReferralId = referral.ReferralId, VisitDate = new DateTime(DateTime.Today.Year, 2, 10) };
            _db.Visits.AddRange(v1, v2);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = v1.VisitId, TestId = test.TestId, Price = 50m },
                new VisitTest { VisitId = v2.VisitId, TestId = test.TestId, Price = 80m });
            _db.Invoices.AddRange(
                new Invoice { VisitId = v1.VisitId, Total = 50m, Discount = 0m, NetTotal = 50m, Paid = 50m },
                new Invoice { VisitId = v2.VisitId, Total = 80m, Discount = 0m, NetTotal = 80m, Paid = 20m });
            await _db.SaveChangesAsync();

            var snapshot = await service.GetSnapshotAsync(DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1), null, null);
            var monthly = await service.GetMonthlyAnalysisAsync(DateTime.Today.Year);
            var top = await service.GetTop10TestsAsync(DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));
            var yearly = await service.GetSampleCountPerYearAsync(3);

            snapshot.ByGender.Should().HaveCountGreaterThan(1);
            snapshot.ByReferral.Should().Contain(r => r.ReferralName == "Referral");
            monthly.Should().HaveCount(12);
            top.Should().ContainSingle();
            yearly.Should().NotBeEmpty();
        }

        [Fact]
        public async Task User_Attendance_Gap_Functions_Should_Work()
        {
            // Function: 10.1 — , 10.2, 10.3, 10.4, 10.5, 11.1, 11.2, 11.4
            var userAdmin = new UserAdminService(_db);
            var attendance = new AttendanceService(_db);
            var tardiness = new TardinessService(_db);

            var role = await userAdmin.CreateRoleAsync("LabTech");
            await userAdmin.SaveRolePermissionsAsync(role.RoleId, new[] { "R1", "R2" });

            var user = await userAdmin.CreateUserAsync(new User { Username = "u1", FullName = "User One", IsActive = true }, "pass");
            await userAdmin.AssignSingleRoleAsync(user.UserId, role.RoleId);

            user.FullName = "User One Updated";
            await userAdmin.UpdateUserAsync(user, "newpass");

            var log = await attendance.CreateLoginAsync(user.UserId, "start");
            await attendance.CloseAsync(log.AttendanceLogId);
            var closed = await _db.AttendanceLogs.FindAsync(log.AttendanceLogId);
            closed!.LogoutAt.Should().NotBeNull();

            var shift = new ShiftSchedule
            {
                Name = "Morning",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                GracePeriodMinutes = 5
            };
            _db.ShiftSchedules.Add(shift);
            await _db.SaveChangesAsync();

            _db.AttendanceLogs.Add(new AttendanceLog
            {
                UserId = user.UserId,
                ShiftId = shift.ShiftId,
                LoginAt = DateTime.Today.AddHours(8).AddMinutes(30),
                LogoutAt = DateTime.Today.AddHours(16),
                LastActivityAt = DateTime.Today.AddHours(16)
            });
            await _db.SaveChangesAsync();

            var report = await tardiness.GetPunctualityReportAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), user.UserId);
            report.Should().NotBeEmpty();
            report.Max(r => r.DelayMinutes).Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Contract_Invoice_Gap_Functions_Should_Work()
        {
            // Function: 12.8 — , 12.9
            var service = new ContractInvoiceService(_db);
            var referral = new Referral { Name = "Insurance A", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-CON", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, ReferralId = referral.ReferralId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.Invoices.Add(new Invoice { VisitId = visit.VisitId, Total = 100m, Discount = 10m, NetTotal = 90m, Paid = 0m });
            await _db.SaveChangesAsync();

            var contractInvoiceId = await service.CreateContractInvoiceAsync(referral.ReferralId, "CI-001", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            var settled = await service.SettleContractInvoiceAsync(contractInvoiceId);

            settled.IsPaid.Should().BeTrue();
        }

        [Fact]
        public async Task System_Settings_Gap_Function_Should_Work()
        {
            // Function: 13.1 — Set Report Margins
            var service = new SystemSettingsService(_db);
            var profile = await service.GetProfileAsync();
            var updatedProfile = new SystemSettingsProfile
            {
                ReportHeader = profile.ReportHeader,
                ReportFooter = profile.ReportFooter,
                ReportMarginTop = 2.2,
                ReportMarginBottom = 1.3,
                ReportPrimaryColor = profile.ReportPrimaryColor,
                ReportPrinterName = profile.ReportPrinterName,
                ReportPaperSize = profile.ReportPaperSize,
                ReceiptPrinterName = profile.ReceiptPrinterName,
                BarcodePrinterName = profile.BarcodePrinterName,
                EnvelopePrinterName = profile.EnvelopePrinterName,
                ReceiptHeaderText = profile.ReceiptHeaderText,
                ReceiptFooterText = profile.ReceiptFooterText,
                ReceiptShowLogo = profile.ReceiptShowLogo,
                ReceiptCopies = profile.ReceiptCopies,
                DefaultAccountType = profile.DefaultAccountType,
                MasterPasswordHash = profile.MasterPasswordHash
            };

            await service.SaveProfileAsync(updatedProfile);

            var updated = await service.GetProfileAsync();
            updated.ReportMarginTop.Should().Be(2.2);
            updated.ReportMarginBottom.Should().Be(1.3);
        }

        [Fact]
        public async Task PatientSearchService_SearchPatientsAsync_Should_Filter_By_Name_Phone_And_LabId_SuccessGuard()
        {
            // Function: 13.1 — Set Report Margins
            var service = new PatientSearchService(_db);
            _db.Patients.AddRange(
                new Patient { LabId = "LAB-A", FullName = "Ali Hassan", Gender = "Male", Phone = "01000111" },
                new Patient { LabId = "LAB-B", FullName = "Mona Ali", Gender = "Female", Phone = "01100222" },
                new Patient { LabId = "LAB-C", FullName = "Ziad", Gender = "Male", Phone = "01200333" });
            await _db.SaveChangesAsync();

            var result = await service.SearchPatientsAsync("Ali", "0100", "LAB-A");

            result.Should().ContainSingle();
            result[0].LabId.Should().Be("LAB-A");
            result[0].FullName.Should().Contain("Ali");
        }

        [Fact]
        public async Task PatientSearchService_SearchPatientsAsync_With_AllFiltersEmpty_Should_Return_AllOrdered_EdgeGuard()
        {
            // Function: 13.1 — Set Report Margins
            var service = new PatientSearchService(_db);
            _db.Patients.AddRange(
                new Patient { LabId = "L2", FullName = "Zed", Gender = "Male" },
                new Patient { LabId = "L1", FullName = "Adam", Gender = "Male" });
            await _db.SaveChangesAsync();

            var result = await service.SearchPatientsAsync(null, string.Empty, " ");

            result.Should().HaveCount(2);
            result[0].FullName.Should().Be("Adam");
            result[1].FullName.Should().Be("Zed");
        }

        [Fact]
        public async Task PatientSearchService_SearchPatientsAsync_With_DateFilter_Should_Exclude_Patients_Without_Visits_FailureGuard()
        {
            // Function: 13.1 — Set Report Margins
            var service = new PatientSearchService(_db);
            var withVisit = new Patient { LabId = "LAB-D1", FullName = "With Visit", Gender = "Male" };
            var withoutVisit = new Patient { LabId = "LAB-D2", FullName = "Without Visit", Gender = "Female" };
            _db.Patients.AddRange(withVisit, withoutVisit);
            await _db.SaveChangesAsync();
            _db.Visits.Add(new Visit { PatientId = withVisit.PatientId, VisitDate = DateTime.Today });
            await _db.SaveChangesAsync();

            var result = await service.SearchPatientsAsync(null, null, null, DateTime.Today);

            result.Should().ContainSingle();
            result[0].LabId.Should().Be("LAB-D1");
        }

        [Fact]
        public async Task CompareWithHistoryService_GetLastResultsAsync_Should_Return_MostRecent_VisitResults_SuccessGuard()
        {
            // Function: 13.1 — Set Report Margins
            var service = new CompareWithHistoryService(_db);
            var patient = new Patient { LabId = "LAB-H1", FullName = "History P", Gender = "Male" };
            var test = new Test { Code = "HIS1", NameReport = "History Test", NameReceipt = "History Test", Price = 20m };
            _db.Patients.Add(patient);
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var parameter = new TestParameter { TestId = test.TestId, Name = "Param1", OrderNo = 1 };
            _db.TestParameters.Add(parameter);
            await _db.SaveChangesAsync();

            var oldVisit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-10) };
            var newVisit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-1) };
            _db.Visits.AddRange(oldVisit, newVisit);
            await _db.SaveChangesAsync();

            var oldVisitTest = new VisitTest { VisitId = oldVisit.VisitId, TestId = test.TestId, Price = 20m, Status = "Completed" };
            var newVisitTest = new VisitTest { VisitId = newVisit.VisitId, TestId = test.TestId, Price = 20m, Status = "Completed" };
            _db.VisitTests.AddRange(oldVisitTest, newVisitTest);
            await _db.SaveChangesAsync();

            _db.ResultValues.AddRange(
                new ResultValue { VisitTestId = oldVisitTest.VisitTestId, ParameterId = parameter.ParameterId, Value = "8.1", Flag = "H" },
                new ResultValue { VisitTestId = newVisitTest.VisitTestId, ParameterId = parameter.ParameterId, Value = "5.2", Flag = "N" });
            await _db.SaveChangesAsync();

            var result = await service.GetLastResultsAsync(patient.PatientId, test.TestId, count: 1);

            result.Should().ContainSingle();
            result[0].Value.Should().Be("5.2");
            result[0].VisitId.Should().Be(newVisit.VisitId);
        }

        [Fact]
        public async Task CompareWithHistoryService_GetLastResultsAsync_When_NoHistory_Should_Return_EmptyList_FailureGuard()
        {
            // Function: 13.1 — Set Report Margins
            var service = new CompareWithHistoryService(_db);

            var result = await service.GetLastResultsAsync(patientId: 999, testId: 999, count: 3);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CompareWithHistoryService_GetLastResultsAsync_With_ZeroCount_Should_Return_Empty_EdgeGuard()
        {
            // Function: 13.1 — Set Report Margins
            var service = new CompareWithHistoryService(_db);
            var patient = new Patient { LabId = "LAB-H2", FullName = "Edge", Gender = "Male" };
            var test = new Test { Code = "HIS2", NameReport = "H2", NameReceipt = "H2", Price = 10m };
            _db.Patients.Add(patient);
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var result = await service.GetLastResultsAsync(patient.PatientId, test.TestId, count: 0);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AuthService_ValidateCredentialsAsync_With_HashedPassword_Should_Return_User_SuccessGuard()
        {
            // Function: 13.1 — Set Report Margins
            var service = new AuthService(_db);
            var salt = PasswordSecurity.GenerateSalt();
            var user = new User
            {
                Username = "hashed-user",
                Salt = salt,
                PasswordHash = PasswordSecurity.ComputeSha256("p@ss", salt),
                IsActive = true
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var result = await service.ValidateCredentialsAsync("hashed-user", "p@ss");

            result.Should().NotBeNull();
            result!.Username.Should().Be("hashed-user");
        }

        [Fact]
        public async Task AuthService_ValidateCredentialsAsync_With_InvalidPassword_Should_Return_Null_FailureGuard()
        {
            // Function: 13.1 — Set Report Margins
            var service = new AuthService(_db);
            var salt = PasswordSecurity.GenerateSalt();
            _db.Users.Add(new User
            {
                Username = "wrong-pass-user",
                Salt = salt,
                PasswordHash = PasswordSecurity.ComputeSha256("correct", salt),
                IsActive = true
            });
            await _db.SaveChangesAsync();

            var result = await service.ValidateCredentialsAsync("wrong-pass-user", "incorrect");

            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthService_ValidateCredentialsAsync_With_LegacyPassword_Should_MigrateSaltAndHash_EdgeGuard()
        {
            // Function: 13.1 — Set Report Margins
            var service = new AuthService(_db);
            var user = new User
            {
                Username = "legacy-user",
                PasswordHash = "legacy-pass",
                Salt = string.Empty,
                IsActive = true
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var result = await service.ValidateCredentialsAsync("legacy-user", "legacy-pass");
            var persisted = await _db.Users.FirstAsync(u => u.UserId == user.UserId);

            result.Should().NotBeNull();
            persisted.Salt.Should().NotBeNullOrWhiteSpace();
            persisted.PasswordHash.Should().NotBe("legacy-pass");
        }

        private Test SeedTest(string code)
        {
            var test = new Test
            {
                Code = code,
                NameReport = code,
                NameReceipt = code,
                Price = 10m
            };
            _db.Tests.Add(test);
            _db.SaveChanges();
            return test;
        }
    }
}
