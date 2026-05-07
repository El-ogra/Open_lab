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
    public class WorksheetServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly WorksheetService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public WorksheetServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new WorksheetService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetWorksheetByPatientAsync_Should_Return_PatientRows()
        {
            // Function: X.X — To Be Determined
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 10m });
            await _db.SaveChangesAsync();

            var rows = await _service.GetWorksheetByPatientAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            rows.Should().ContainSingle();
            rows.First().TestsCount.Should().Be(1);
        }

        [Fact]
        public async Task GetWorksheetByTestAsync_Should_Return_GroupedCounts()
        {
            // Function: X.X — To Be Determined
            var patient = new Patient { LabId = "L2", FullName = "P2", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T1", NameReport = "CBC", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m });
            await _db.SaveChangesAsync();

            var grouped = await _service.GetWorksheetByTestAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            grouped.Should().ContainSingle().Which.TestName.Should().Be("CBC");
            grouped.First().Count.Should().Be(1);
        }

        [Fact]
        public async Task GetWorksheetByPatientAsync_When_NoVisitsInRange_Should_Return_Empty_FailureGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var patient = new Patient { LabId = "L9", FullName = "Old Visit", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-15) };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetWorksheetByPatientAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetWorksheetByTestAsync_When_MultipleVisitsSameTest_Should_Aggregate_Count_EdgeGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var patient = new Patient { LabId = "L10", FullName = "P10", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            var visit2 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.AddRange(visit1, visit2);

            var test = new Test { Code = "T-CBC", NameReport = "CBC", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit1.VisitId, TestId = test.TestId, Price = 10m },
                new VisitTest { VisitId = visit2.VisitId, TestId = test.TestId, Price = 10m });
            await _db.SaveChangesAsync();

            // Act
            var grouped = await _service.GetWorksheetByTestAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            grouped.Should().ContainSingle();
            grouped[0].TestName.Should().Be("CBC");
            grouped[0].Count.Should().Be(2);
        }

        [Fact]
        public async Task GetWorksheetByPatientAsync_Should_Include_Visit_On_Range_Boundary_EdgeGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var patient = new Patient { LabId = "L11", FullName = "Boundary", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var from = DateTime.Today;
            var to = DateTime.Today.AddDays(1);
            var visit = new Visit { PatientId = patient.PatientId, VisitDate = to };
            _db.Visits.Add(visit);

            var test = new Test { Code = "T-B", NameReport = "BoundaryTest", Price = 7m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 7m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetWorksheetByPatientAsync(from, to);

            // Assert
            rows.Should().ContainSingle();
            rows[0].VisitId.Should().Be(visit.VisitId);
        }

        [Fact]
        public async Task GetWorksheetByTestAsync_When_No_Tests_In_Range_Should_Return_Empty_FailureGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var patient = new Patient { LabId = "L12", FullName = "Out of range", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-20) };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T-OLD", NameReport = "OldTest", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m });
            await _db.SaveChangesAsync();

            // Act
            var grouped = await _service.GetWorksheetByTestAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            grouped.Should().BeEmpty();
        }
    }
}
