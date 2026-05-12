using System;
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
    public class GapRemediationTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public GapRemediationTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task PatientUpdate_WithUserId_Should_Write_AuditLog()
        {
            var user = new User { Username = "auditor", PasswordHash = "hash", Salt = "salt", IsActive = true };
            var patient = new Patient { LabId = "P-AUD-1", FullName = "Old", Gender = "Male" };
            _db.Users.Add(user);
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var service = new PatientService(_db);
            await service.UpdateAsync(new Patient
            {
                PatientId = patient.PatientId,
                LabId = patient.LabId,
                FullName = "New",
                Gender = "Female"
            }, user.UserId);

            var audit = await _db.AuditLogs.SingleAsync(a => a.Action == "EDIT_PATIENT");
            audit.UserId.Should().Be(user.UserId);
            audit.OldValues.Should().Contain("FullName=Old");
            audit.NewValues.Should().Contain("FullName=New");
        }

        [Fact]
        public async Task RemoveVisitTestAsync_WithEnteredResult_Should_Throw()
        {
            var patient = new Patient { LabId = "P-DEL-1", FullName = "Patient", Gender = "Male" };
            var test = new Test { Code = "T-DEL", NameReport = "Delete Guard", NameReceipt = "Delete Guard", Price = 10 };
            _db.Patients.Add(patient);
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Status = "InProgress", Price = 10 };
            var parameter = new TestParameter { TestId = test.TestId, Name = "Result", OrderNo = 1 };
            _db.VisitTests.Add(visitTest);
            _db.TestParameters.Add(parameter);
            await _db.SaveChangesAsync();

            _db.ResultValues.Add(new ResultValue { VisitTestId = visitTest.VisitTestId, ParameterId = parameter.ParameterId, Value = "12" });
            await _db.SaveChangesAsync();

            var service = new VisitService(_db);
            Func<Task> act = async () => await service.RemoveVisitTestAsync(visitTest.VisitTestId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*result entry*");
        }

        [Fact]
        public async Task SaveResultAsync_EditWithUserId_Should_Write_AuditLog_ForValueFlagAndComment()
        {
            var user = new User { Username = "result-user", PasswordHash = "hash", Salt = "salt", IsActive = true };
            var patient = new Patient { LabId = "P-RES-1", FullName = "Patient", Gender = "Male" };
            var test = new Test { Code = "T-RES", NameReport = "Result Audit", NameReceipt = "Result Audit", Price = 10 };
            _db.Users.Add(user);
            _db.Patients.Add(patient);
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Status = "InProgress", Price = 10 };
            var parameter = new TestParameter { TestId = test.TestId, Name = "Result", OrderNo = 1 };
            _db.VisitTests.Add(visitTest);
            _db.TestParameters.Add(parameter);
            await _db.SaveChangesAsync();

            var service = new ResultsService(_db);
            await service.SaveResultAsync(visitTest.VisitTestId, parameter.ParameterId, "10", "N", "Initial", user.UserId);
            await service.SaveResultAsync(visitTest.VisitTestId, parameter.ParameterId, "12", "H", "Changed", user.UserId);

            var audit = await _db.AuditLogs.SingleAsync(a => a.Action == "EDIT_RESULT");
            audit.UserId.Should().Be(user.UserId);
            audit.OldValues.Should().Contain("Value=10").And.Contain("Flag=N").And.Contain("Comment=Initial");
            audit.NewValues.Should().Contain("Value=12").And.Contain("Flag=H").And.Contain("Comment=Changed");
        }

        [Fact]
        public async Task VisitCreateAsync_Should_Use_PatientReferral_When_NoVisitReferralProvided()
        {
            var referral = new Referral { Name = "Contract A", ReferralType = "Contract" };
            var patient = new Patient { LabId = "P-CON-1", FullName = "Patient", Gender = "Male", Referral = referral };
            _db.Referrals.Add(referral);
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var service = new VisitService(_db);
            var visit = await service.CreateAsync(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now });

            visit.ReferralId.Should().Be(referral.ReferralId);
            visit.AccountType.Should().Be("Referral");
        }

        [Fact]
        public async Task SystemSettingsProfile_Should_Save_And_Load_ReportAndInvoiceCompletionFields()
        {
            var service = new SystemSettingsService(_db);

            await service.SaveProfileAsync(new SystemSettingsProfile
            {
                ReportHeader = "Header",
                ReportFooter = "Footer",
                ReportMarginTop = 1,
                ReportMarginBottom = 2,
                ReportMarginLeft = 3,
                ReportMarginRight = 4,
                ReportLogoPath = @"C:\logo-report.png",
                ReceiptHeaderText = "Receipt H",
                ReceiptFooterText = "Receipt F",
                ReceiptCopies = 2,
                InvoiceLogoPath = @"C:\logo-invoice.png",
                InvoiceCurrency = "usd",
                InvoiceShowMedicalDetails = false
            });

            var loaded = await service.GetProfileAsync();

            loaded.ReportMarginLeft.Should().Be(3);
            loaded.ReportMarginRight.Should().Be(4);
            loaded.ReportLogoPath.Should().Be(@"C:\logo-report.png");
            loaded.InvoiceLogoPath.Should().Be(@"C:\logo-invoice.png");
            loaded.InvoiceCurrency.Should().Be("USD");
            loaded.InvoiceShowMedicalDetails.Should().BeFalse();
        }
    }
}
