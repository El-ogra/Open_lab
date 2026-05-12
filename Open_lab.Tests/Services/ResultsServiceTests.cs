using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Open_lab.Tests.Services
{
    public class ResultsServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly ResultsService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public ResultsServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new ResultsService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task SaveResultAsync_Should_Create_Result_And_Set_Status()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T1", NameReport = "T", NameReceipt = "T", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            await _service.SaveResultAsync(vt.VisitTestId, 1, "5.0", "", "");

            // Assert
            var results = await _db.ResultValues.Where(r => r.VisitTestId == vt.VisitTestId).ToListAsync();
            results.Should().HaveCount(1);
            var updatedVt = await _db.VisitTests.FindAsync(vt.VisitTestId);
            updatedVt.Should().NotBeNull();
            updatedVt!.Status.Should().Be("Completed");
        }

        [Fact]
        public async Task SaveResultAsync_When_VisitTestNotFound_Should_Throw()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            // Act
            Func<Task> act = async () => await _service.SaveResultAsync(999, 1, "5", null, null);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task VerifyVisitTestAsync_NoResults_Should_Throw()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            var patient = new Patient { LabId = "L2", FullName = "P2", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T2", NameReport = "T2", NameReceipt = "T2", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m, Status = "InProgress" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.VerifyVisitTestAsync(vt.VisitTestId, 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task VerifyVisitTestAsync_With_AllResults_Should_Verify()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            var patient = new Patient { LabId = "L3", FullName = "P3", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T3", NameReport = "T3", NameReceipt = "T3", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var param = new TestParameter { TestId = test.TestId, Name = "p1", OrderNo = 1 };
            _db.TestParameters.Add(param);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m, Status = "InProgress" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            _db.ResultValues.Add(new ResultValue { VisitTestId = vt.VisitTestId, ParameterId = param.ParameterId, Value = "1.0" });
            await _db.SaveChangesAsync();

            // Act
            await _service.VerifyVisitTestAsync(vt.VisitTestId, 99);

            // Assert
            var results = await _db.ResultValues.Where(r => r.VisitTestId == vt.VisitTestId).ToListAsync();
            results.All(r => r.VerifiedBy == 99).Should().BeTrue();
            var updated = await _db.VisitTests.FindAsync(vt.VisitTestId);
            updated.Should().NotBeNull();
            updated!.Status.Should().Be("Verified");
        }

        [Fact]
        public async Task ReopenVisitTestAsync_When_NotVerified_Should_Return()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            var patient = new Patient { LabId = "L4", FullName = "P4", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T4", NameReport = "T4", NameReceipt = "T4", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m, Status = "InProgress" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            await _service.ReopenVisitTestAsync(vt.VisitTestId);

            // Assert - no exception and status remains InProgress
            var updated = await _db.VisitTests.FindAsync(vt.VisitTestId);
            updated.Should().NotBeNull();
            updated!.Status.Should().Be("InProgress");
        }

        [Fact]
        public async Task GetResultsByVisitAsync_Should_Return_Multiple_Tests_Results_LogicGuard()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            var visitId = 10;
            var patient = new Patient { LabId = "L-COMP", FullName = "Composite Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitId = visitId, PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test1 = new Test { Code = "T1", NameReport = "Test One", NameReceipt = "T1", Price = 50 };
            var test2 = new Test { Code = "T2", NameReport = "Test Two", NameReceipt = "T2", Price = 50 };
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            var param1 = new TestParameter { TestId = test1.TestId, Name = "Param1", OrderNo = 1 };
            var param2 = new TestParameter { TestId = test2.TestId, Name = "Param2", OrderNo = 1 };
            _db.TestParameters.AddRange(param1, param2);
            await _db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitId = visitId, TestId = test1.TestId, Price = 50, Status = "Completed" };
            var vt2 = new VisitTest { VisitId = visitId, TestId = test2.TestId, Price = 50, Status = "Completed" };
            _db.VisitTests.AddRange(vt1, vt2);
            await _db.SaveChangesAsync();

            _db.ResultValues.Add(new ResultValue { VisitTestId = vt1.VisitTestId, ParameterId = param1.ParameterId, Value = "1.0", Flag = "N" });
            _db.ResultValues.Add(new ResultValue { VisitTestId = vt2.VisitTestId, ParameterId = param2.ParameterId, Value = "2.0", Flag = "H" });
            await _db.SaveChangesAsync();

            // Act
            var results = await _db.ResultValues
                .Where(r => r.VisitTest.VisitId == visitId)
                .Include(r => r.VisitTest)
                .ThenInclude(vt => vt.Test)
                .Include(r => r.Parameter)
                .OrderBy(r => r.VisitTest.Test.Code)
                .ThenBy(r => r.Parameter.OrderNo)
                .ToListAsync();

            // Assert - Logic Guard: Verify composite report structure and ordering
            results.Should().HaveCount(2);
            results[0].Value.Should().Be("1.0");
            results[0].Flag.Should().Be("N");
            results[0].VisitTest.Test.Code.Should().Be("T1");
            results[0].VisitTest.Test.NameReport.Should().Be("Test One");
            results[0].Parameter.Name.Should().Be("Param1");

            results[1].Value.Should().Be("2.0");
            results[1].Flag.Should().Be("H");
            results[1].VisitTest.Test.Code.Should().Be("T2");
            results[1].VisitTest.Test.NameReport.Should().Be("Test Two");
            results[1].Parameter.Name.Should().Be("Param2");
        }

        // 4.3 - Result Modification - Result Audit Integrity Tests
        [Fact]
        public async Task SaveResultAsync_When_Verified_Should_Throw()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            var patient = new Patient { LabId = "L5", FullName = "P5", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T5", NameReport = "T5", NameReceipt = "T5", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var param = new TestParameter { TestId = test.TestId, Name = "p1", OrderNo = 1 };
            _db.TestParameters.Add(param);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m, Status = "Verified" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.SaveResultAsync(vt.VisitTestId, param.ParameterId, "10.0", null, null);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Verified results are locked*");
        }

        [Fact]
        public async Task SaveResultAsync_Should_Clear_Verification_On_Update()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            var patient = new Patient { LabId = "L6", FullName = "P6", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T6", NameReport = "T6", NameReceipt = "T6", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var param = new TestParameter { TestId = test.TestId, Name = "p1", OrderNo = 1 };
            _db.TestParameters.Add(param);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m, Status = "InProgress" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // First save with verification
            _db.ResultValues.Add(new ResultValue 
            { 
                VisitTestId = vt.VisitTestId, 
                ParameterId = param.ParameterId, 
                Value = "5.0",
                VerifiedBy = 99,
                VerifiedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();

            // Act - Update the result
            await _service.SaveResultAsync(vt.VisitTestId, param.ParameterId, "10.0", "H", "Updated value");

            // Assert - Verification should be cleared
            var result = await _db.ResultValues
                .FirstOrDefaultAsync(r => r.VisitTestId == vt.VisitTestId && r.ParameterId == param.ParameterId);
            result.Should().NotBeNull();
            result!.Value.Should().Be("10.0");
            result.Flag.Should().Be("H");
            result.Comment.Should().Be("Updated value");
            result.VerifiedBy.Should().BeNull();
            result.VerifiedAt.Should().BeNull();
        }

        [Fact]
        public async Task SaveResultAsync_Should_Set_Status_To_Completed_When_Result_Is_Present()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            var patient = new Patient { LabId = "L7", FullName = "P7", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T7", NameReport = "T7", NameReceipt = "T7", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            await _service.SaveResultAsync(vt.VisitTestId, 1, "5.0", null, null);

            // Assert
            var updated = await _db.VisitTests.FindAsync(vt.VisitTestId);
            updated.Should().NotBeNull();
            updated!.Status.Should().Be("Completed");
        }

        [Fact]
        public async Task ReopenVisitTestAsync_Should_Clear_Verification_Data()
        {
            // Function: 4.4 — Create Composite Report - Logic Guard: Verify composite report structure
            // Arrange
            var patient = new Patient { LabId = "L8", FullName = "P8", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T8", NameReport = "T8", NameReceipt = "T8", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var param = new TestParameter { TestId = test.TestId, Name = "p1", OrderNo = 1 };
            _db.TestParameters.Add(param);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m, Status = "Verified" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            _db.ResultValues.Add(new ResultValue 
            { 
                VisitTestId = vt.VisitTestId, 
                ParameterId = param.ParameterId, 
                Value = "5.0",
                VerifiedBy = 99,
                VerifiedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();

            // Act
            await _service.ReopenVisitTestAsync(vt.VisitTestId);

            // Assert
            var updatedVt = await _db.VisitTests.FindAsync(vt.VisitTestId);
            updatedVt.Should().NotBeNull();
            updatedVt!.Status.Should().Be("InProgress");

            var result = await _db.ResultValues
                .FirstOrDefaultAsync(r => r.VisitTestId == vt.VisitTestId && r.ParameterId == param.ParameterId);
            result.Should().NotBeNull();
            result!.VerifiedBy.Should().BeNull();
            result.VerifiedAt.Should().BeNull();
        }

        // 3.3 Reference Range Decision Tests - NEW TEST

        [Fact]
        public async Task SaveResultAsync_With_ReferenceRange_Should_Classify_Correctly_LogicGuard()
        {
            // Function: 3.3 — Reference Range Decision - Logic Guard: Verify result classification
            // Arrange
            var patient = new Patient { LabId = "L-REF", FullName = "Ref Patient", Gender = "Male", Age = 30 };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "REF", NameReport = "Glucose", NameReceipt = "Glucose", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var param = new TestParameter { TestId = test.TestId, Name = "Glucose", OrderNo = 1 };
            _db.TestParameters.Add(param);
            await _db.SaveChangesAsync();

            // Reference range: 70-100 is normal
            var refRange = new TestReferenceRange
            {
                TestId = test.TestId,
                AgeFrom = 0,
                AgeTo = 120,
                LowValue = 70m,
                HighValue = 100m,
                Gender = "All"
            };
            _db.TestReferenceRanges.Add(refRange);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act - Save a high value (150)
            await _service.SaveResultAsync(vt.VisitTestId, param.ParameterId, "150", "H", "High glucose");

            // Assert - Logic Guard: Verify result is saved with correct classification
            var result = await _db.ResultValues
                .FirstOrDefaultAsync(r => r.VisitTestId == vt.VisitTestId && r.ParameterId == param.ParameterId);
            result.Should().NotBeNull();
            result!.Value.Should().Be("150");
            result.Flag.Should().Be("H");
            result.Comment.Should().Be("High glucose");

            // Act - Save a normal value (85)
            await _service.SaveResultAsync(vt.VisitTestId, param.ParameterId, "85", "N", "Normal");

            var normalResult = await _db.ResultValues
                .Where(r => r.VisitTestId == vt.VisitTestId && r.ParameterId == param.ParameterId)
                .OrderByDescending(r => r.ResultValueId)
                .FirstOrDefaultAsync();
            normalResult.Should().NotBeNull();
            normalResult!.Value.Should().Be("85");
            normalResult.Flag.Should().Be("N");
            normalResult.Comment.Should().Be("Normal");

            // Act - Save a low value (50)
            await _service.SaveResultAsync(vt.VisitTestId, param.ParameterId, "50", "L", "Low glucose");

            var lowResult = await _db.ResultValues
                .Where(r => r.VisitTestId == vt.VisitTestId && r.ParameterId == param.ParameterId)
                .OrderByDescending(r => r.ResultValueId)
                .FirstOrDefaultAsync();
            lowResult.Should().NotBeNull();
            lowResult!.Value.Should().Be("50");
            lowResult.Flag.Should().Be("L");
            lowResult.Comment.Should().Be("Low glucose");
        }
        [Fact]
        public async Task Function_4_1_Automated_Intelligence_Full_Verification()
        {
            // Function: 4.1 — `Enter Test Results`
            // Arrange
            var patient = new Patient { PatientId = 1, Gender = "Female", Age = 25 };
            _db.Patients.Add(patient);

            var testId = 101;
            _db.Tests.Add(new Test { TestId = testId, Code = "HEM", NameReport = "Hemoglobin", NameReceipt = "HEM" });
            
            // Add Range: 12-16 normal for female
            _db.TestReferenceRanges.Add(new TestReferenceRange { TestId = testId, Gender = "Female", LowValue = 12m, HighValue = 16m });
            
            // Add Comments
            _db.TestComments.Add(new TestComment { TestId = testId, CommentText = "Sample Normal", LowComment = "Anemia Risk", HighComment = "Polycythemia Risk", IsDefault = true });
            
            await _db.SaveChangesAsync();

            // Act - Validate Low Value (10)
            var lowResult = await _service.ValidateResultAsync(testId, "10", "Female", 25);
            // Assert
            lowResult.Flag.Should().Be("L");
            lowResult.RecommendedComment.Should().Be("Anemia Risk");

            // Act - Validate High Value (18)
            var highResult = await _service.ValidateResultAsync(testId, "18", "Female", 25);
            highResult.Flag.Should().Be("H");
            highResult.RecommendedComment.Should().Be("Polycythemia Risk");
        }

        [Fact]
        public async Task Function_4_3_Audit_Trail_Integrity_Verification()
        {
            // Function: 4.3 — `Edit Results`
            // Arrange
            var vtId = 5;
            var pId = 20;
            var user = new User { UserId = 7, Username = "editor", PasswordHash = "hash", Salt = "salt", IsActive = true };
            _db.Users.Add(user);
            _db.VisitTests.Add(new VisitTest { VisitTestId = vtId, VisitId = 10, TestId = 10 });
            _db.ResultValues.Add(new ResultValue { VisitTestId = vtId, ParameterId = pId, Value = "Initial" });
            await _db.SaveChangesAsync();

            // Act - Modify results
            await _service.SaveResultAsync(vtId, pId, "Modified", "H", "Reason X", user.UserId);

            // Assert
            var log = await _db.AuditLogs.OrderByDescending(l => l.Timestamp).FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log!.Action.Should().Be("EDIT_RESULT");
            log.UserId.Should().Be(user.UserId);
            log.OldValues.Should().Contain("Initial");
            log.NewValues.Should().Contain("Modified");
        }

        [Fact]
        public async Task Function_4_7_Print_Audit_Logging_Verification()
        {
            // Function: 4.7 — `Print Report`
            // Arrange
            var visitId = 70;
            var userId = 5;

            // Act
            await _service.LogVisitReportPrintedAsync(visitId, userId);

            // Assert
            var log = await _db.AuditLogs.FirstOrDefaultAsync(l => l.Action == "PRINT_REPORT" && l.RecordId == "70");
            log.Should().NotBeNull();
            log!.UserId.Should().Be(userId);
        }
    }
}
