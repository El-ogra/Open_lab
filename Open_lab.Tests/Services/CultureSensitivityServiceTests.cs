using System;
using System.Collections.Generic;
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
    public class CultureSensitivityServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly CultureSensitivityService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public CultureSensitivityServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new CultureSensitivityService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateCultureAsync_Should_Create_Culture()
        {
            var culture = new Culture { Name = "Stool" };
            var created = await _service.CreateCultureAsync(culture);
            created.CultureId.Should().BeGreaterThan(0);
            (await _db.Cultures.FindAsync(created.CultureId)).Should().NotBeNull();
        }

        [Fact]
        public async Task CreateCultureAsync_Duplicate_Should_Throw()
        {
            _db.Cultures.Add(new Culture { Name = "Blood" });
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.CreateCultureAsync(new Culture { Name = "Blood" });
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateAntibioticAsync_Should_Create_And_Filter()
        {
            var a1 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "A1", IsSafeForChildren = true, IsSafeForPregnancy = false });
            var a2 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "A2", IsSafeForChildren = false, IsSafeForPregnancy = true });

            var all = await _service.GetAntibioticsAsync();
            all.Select(a => a.Name).Should().Contain(new[] { "A1", "A2" });

            // prepare visit test with child patient
            var patient = new Patient { LabId = "L1", FullName = "C", Gender = "Male", Age = 5 };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "CULT", NameReport = "Culture Test", Price = 1m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 1m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var filtered = await _service.GetFilteredAntibioticsAsync(vt.VisitTestId);
            filtered.Select(a => a.Name).Should().Contain("A1");
            filtered.Select(a => a.Name).Should().NotContain("A2");
        }

        [Fact]
        public async Task LinkAndUnlinkAntibiotic_Should_Work()
        {
            var culture = new Culture { Name = "Urine" };
            _db.Cultures.Add(culture);
            var antibiotic = new Antibiotic { Name = "Abc" };
            _db.Antibiotics.Add(antibiotic);
            await _db.SaveChangesAsync();

            await _service.LinkAntibioticAsync(culture.CultureId, antibiotic.AntibioticId);
            var links = await _service.GetCultureAntibioticsAsync(culture.CultureId);
            links.Should().ContainSingle().Which.Antibiotic.Name.Should().Be("Abc");

            await _service.UnlinkAntibioticAsync(culture.CultureId, antibiotic.AntibioticId);
            var after = await _service.GetCultureAntibioticsAsync(culture.CultureId);
            after.Should().BeEmpty();
        }
    }
}
