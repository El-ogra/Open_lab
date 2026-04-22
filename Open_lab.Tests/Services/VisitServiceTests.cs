using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class VisitServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly VisitService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public VisitServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new VisitService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task AddTestToVisitAsync_Should_Add_VisitTest_With_Price()
        {
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T1", NameReport = "Test", Price = 100m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Act
            var vt = await _service.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            vt.Should().NotBeNull();
            vt.Price.Should().Be(100m);
        }

        [Fact]
        public async Task AddTestToVisitAsync_Duplicate_Should_Throw()
        {
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T1", NameReport = "Test", Price = 100m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 100m });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task RemoveVisitTestAsync_Verified_Should_Throw()
        {
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 50m, Status = "Verified" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.RemoveVisitTestAsync(vt.VisitTestId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
