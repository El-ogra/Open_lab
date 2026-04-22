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
            updatedVt!.Status.Should().Be("InProgress");
        }

        [Fact]
        public async Task SaveResultAsync_When_VisitTestNotFound_Should_Throw()
        {
            // Act
            Func<Task> act = async () => await _service.SaveResultAsync(999, 1, "5", null, null);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task VerifyVisitTestAsync_NoResults_Should_Throw()
        {
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
    }
}
