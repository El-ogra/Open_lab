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
    }
}
