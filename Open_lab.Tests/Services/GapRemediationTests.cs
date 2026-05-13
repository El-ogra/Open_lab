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
        public async Task EditPatientData_WithUserId_ShouldWriteAuditLog()
        {
            // Function: 1.2 — Edit Patient Data
            // Arrange
            var user = new User { Username = "auditor", PasswordHash = "hash", Salt = "salt", IsActive = true };
            var patient = new Patient { LabId = "P-AUD-1", FullName = "Old", Gender = "Male" };
            _db.Users.Add(user);
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var service = new PatientService(_db);

            // Act
            await service.UpdateAsync(new Patient
            {
                PatientId = patient.PatientId,
                LabId = patient.LabId,
                FullName = "New",
                Gender = "Female"
            }, user.UserId);

            // Assert
            var audit = await _db.AuditLogs.SingleAsync(a => a.Action == "EDIT_PATIENT");
            audit.UserId.Should().Be(user.UserId);
            audit.OldValues.Should().Contain("FullName=Old");
            audit.NewValues.Should().Contain("FullName=New");
        }

        [Fact]
        public async Task DeleteTests_WithEnteredResult_ShouldThrowInvalidOperation()
        {
            // Function: 1.4 — Delete Tests
            // Arrange
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

            // Act
            Func<Task> act = async () => await service.RemoveVisitTestAsync(visitTest.VisitTestId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*result entry*");
        }

        [Fact]
        public async Task EditResults_WithUserId_ShouldWriteAuditLogForValueFlagAndComment()
        {
            // Function: 4.3 — Edit Results
            // Arrange
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

            // Act
            await service.SaveResultAsync(visitTest.VisitTestId, parameter.ParameterId, "12", "H", "Changed", user.UserId);

            // Assert
            var audit = await _db.AuditLogs.SingleAsync(a => a.Action == "EDIT_RESULT");
            audit.UserId.Should().Be(user.UserId);
            audit.OldValues.Should().Contain("Value=10").And.Contain("Flag=N").And.Contain("Comment=Initial");
            audit.NewValues.Should().Contain("Value=12").And.Contain("Flag=H").And.Contain("Comment=Changed");
        }

        [Fact]
        public async Task AssignPatientToContract_WithPatientReferral_ShouldApplyReferralToNewVisit()
        {
            // Function: 12.5 — Assign Patient to Contract
            // Arrange
            var referral = new Referral { Name = "Contract A", ReferralType = "Contract" };
            var patient = new Patient { LabId = "P-CON-1", FullName = "Patient", Gender = "Male", Referral = referral };
            _db.Referrals.Add(referral);
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var service = new VisitService(_db);

            // Act
            var visit = await service.CreateAsync(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now });

            // Assert
            visit.ReferralId.Should().Be(referral.ReferralId);
            visit.AccountType.Should().Be("Referral");
        }

        [Fact]
        public async Task ConfigureHeaderFooter_WithProfileFields_ShouldPersistReportAndInvoiceSettings()
        {
            // Function: 13.3 — Configure Header/Footer
            // Arrange
            var service = new SystemSettingsService(_db);

            // Act
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

            // Assert
            loaded.ReportHeader.Should().Be("Header");
            loaded.ReportFooter.Should().Be("Footer");
            loaded.ReportMarginLeft.Should().Be(3);
            loaded.ReportMarginRight.Should().Be(4);
            loaded.ReportLogoPath.Should().Be(@"C:\logo-report.png");
            loaded.InvoiceLogoPath.Should().Be(@"C:\logo-invoice.png");
            loaded.InvoiceCurrency.Should().Be("USD");
            loaded.InvoiceShowMedicalDetails.Should().BeFalse();
        }
    }
}
