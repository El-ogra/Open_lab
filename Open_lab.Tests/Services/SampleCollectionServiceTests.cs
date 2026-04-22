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
    public class SampleCollectionServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly SampleCollectionService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public SampleCollectionServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new SampleCollectionService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetRowsAsync_Should_Return_Rows()
        {
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T1", NameReport = "Test", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var rows = await _service.GetRowsAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
            rows.Should().ContainSingle().Which.VisitTestId.Should().Be(vt.VisitTestId);
        }

        [Fact]
        public async Task MarkCollectedAsync_Should_Create_SampleCollection()
        {
            var patient = new Patient { LabId = "L2", FullName = "P2", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T2", NameReport = "Test2", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            await _service.MarkCollectedAsync(vt.VisitTestId, userId: 7, isExternal: false, receivedBy: null);

            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId);
            sample.Should().NotBeNull();
            sample!.Status.Should().Be("مسحوبة");
            sample.CollectedBy.Should().Be(7);
        }

        [Fact]
        public async Task MarkSeparatedAsync_Without_Sample_Should_Throw()
        {
            Func<Task> act = async () => await _service.MarkSeparatedAsync(999, "Centrifuge");
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task MarkNotCollectedAsync_Should_Remove_Sample()
        {
            var patient = new Patient { LabId = "L3", FullName = "P3", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T3", NameReport = "Test3", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            _db.SampleCollections.Add(new SampleCollection { VisitTestId = vt.VisitTestId, CollectedBy = 1, CollectedAt = DateTime.Now, Status = "مسحوبة" });
            await _db.SaveChangesAsync();

            await _service.MarkNotCollectedAsync(vt.VisitTestId);
            (await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId)).Should().BeNull();
        }
    }
}
