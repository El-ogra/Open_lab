using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class GroupWorksheetServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly GroupWorksheetService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public GroupWorksheetServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new GroupWorksheetService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetGroupWorksheetByCustomGroupAsync_Should_Return_Visits_For_Custom_Group()
        {
            var customGroup = new CustomGroup { Name = "Package A" };
            var test = new Test { Code = "T1", NameReport = "CBC", Price = 10m };
            var patient = new Patient { LabId = "L1", FullName = "Patient 1", Gender = "Male" };
            _db.CustomGroups.Add(customGroup);
            _db.Tests.Add(test);
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.CustomGroupItems.Add(new CustomGroupItem { CustomGroupId = customGroup.CustomGroupId, TestId = test.TestId });

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m });
            await _db.SaveChangesAsync();

            var rows = await _service.GetGroupWorksheetByCustomGroupAsync(customGroup.CustomGroupId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            rows.Should().ContainSingle();
            rows[0].PatientName.Should().Be("Patient 1");
            rows[0].TestsCount.Should().Be(1);
        }
    }
}
