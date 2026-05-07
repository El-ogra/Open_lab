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
        public async Task CreateCultureAsync_Should_Create_With_Full_Culture_Metadata()
        {
            // Function: X.X — To Be Determined
            var culture = new Culture
            {
                Name = "Urine Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 120
            };

            var created = await _service.CreateCultureAsync(culture);

            created.CultureId.Should().BeGreaterThan(0);
            created.SampleType.Should().Be("Urine");
            created.IsolatedOrganism.Should().Be("E. coli");
            created.GrowthConditions.Should().Be("Aerobic");
            created.ColonyCount.Should().Be(120);
        }

        [Fact]
        public async Task CreateCultureAsync_When_Missing_Clinical_Metadata_Should_Throw()
        {
            // Function: X.X — To Be Determined
            Func<Task> act = async () => await _service.CreateCultureAsync(new Culture
            {
                Name = "Blood Culture",
                SampleType = "",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 10
            });

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateCultureAsync_Duplicate_Should_Throw()
        {
            // Function: X.X — To Be Determined
            await _service.CreateCultureAsync(new Culture
            {
                Name = "Blood",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 50
            });

            Func<Task> act = async () => await _service.CreateCultureAsync(new Culture
            {
                Name = "Blood",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 70
            });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateAntibioticAsync_Should_Persist_Safety_Classification_Flags()
        {
            // Function: X.X — To Be Determined
            var created = await _service.CreateAntibioticAsync(new Antibiotic
            {
                Name = "Amoxicillin",
                IsSafeForChildren = true,
                IsSafeForPregnancy = false
            });

            created.AntibioticId.Should().BeGreaterThan(0);
            created.IsSafeForChildren.Should().BeTrue();
            created.IsSafeForPregnancy.Should().BeFalse();
        }

        [Fact]
        public async Task LinkAndUnlinkAntibiotic_Should_Work()
        {
            // Function: X.X — To Be Determined
            var culture = await _service.CreateCultureAsync(new Culture
            {
                Name = "Urine",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 100
            });
            var antibiotic = await _service.CreateAntibioticAsync(new Antibiotic { Name = "Abc" });

            await _service.LinkAntibioticAsync(culture.CultureId, antibiotic.AntibioticId);
            var links = await _service.GetCultureAntibioticsAsync(culture.CultureId);
            links.Should().ContainSingle().Which.Antibiotic.Name.Should().Be("Abc");

            await _service.UnlinkAntibioticAsync(culture.CultureId, antibiotic.AntibioticId);
            var after = await _service.GetCultureAntibioticsAsync(culture.CultureId);
            after.Should().BeEmpty();
        }

        [Theory]
        [InlineData("S", "S")]
        [InlineData("Sensitive", "S")]
        [InlineData("حساس", "S")]
        [InlineData("I", "I")]
        [InlineData("Intermediate", "I")]
        [InlineData("متوسط", "I")]
        [InlineData("R", "R")]
        [InlineData("Resistant", "R")]
        [InlineData("مقاوم", "R")]
        public void ClassifySensitivity_Should_Normalize_To_SIR(string raw, string expected)
        {
            // Function: X.X — To Be Determined
            _service.ClassifySensitivity(raw).Should().Be(expected);
        }

        [Fact]
        public void ClassifySensitivity_When_Unsupported_Should_Throw()
        {
            // Function: X.X — To Be Determined
            Action act = () => _service.ClassifySensitivity("UNKNOWN");
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task SearchCultureVisitTestsAsync_Should_Filter_By_Date_And_LabId()
        {
            // Function: X.X — To Be Determined
            var patient = new Patient { LabId = "LAB123", FullName = "John", Gender = "Male" };
            _db.Patients.Add(patient);
            var test = new Test { Code = "CULT-1", NameReport = "Culture & Sensitivity", NameReceipt = "Culture", Price = 10 };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10 });
            await _db.SaveChangesAsync();

            var results = await _service.SearchCultureVisitTestsAsync("123", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            results.Should().ContainSingle();
            results[0].LabId.Should().Be("LAB123");
        }

        [Fact]
        public async Task GetFilteredAntibioticsAsync_PregnantPatient_Should_OnlyReturn_PregnancySafe()
        {
            // Function: X.X — To Be Determined
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "SafePreg", IsSafeForPregnancy = true, IsSafeForChildren = false });
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "UnsafePreg", IsSafeForPregnancy = false, IsSafeForChildren = true });
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "BothSafe", IsSafeForPregnancy = true, IsSafeForChildren = true });

            var visitTestId = await SeedVisitTestAsync(new Patient
            {
                LabId = "LCS1",
                FullName = "Pregnant",
                Gender = "Female",
                Age = 25,
                IsPregnant = true
            });

            var filtered = await _service.GetFilteredAntibioticsAsync(visitTestId);

            filtered.Should().HaveCount(2);
            filtered.Select(a => a.Name).Should().Contain(new[] { "SafePreg", "BothSafe" });
            filtered.Select(a => a.Name).Should().NotContain("UnsafePreg");
        }

        [Fact]
        public async Task GetFilteredAntibioticsAsync_ChildPatient_Should_OnlyReturn_ChildrenSafe()
        {
            // Function: X.X — To Be Determined
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "SafeChild", IsSafeForPregnancy = false, IsSafeForChildren = true });
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "UnsafeChild", IsSafeForPregnancy = true, IsSafeForChildren = false });
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "BothSafe", IsSafeForPregnancy = true, IsSafeForChildren = true });

            var visitTestId = await SeedVisitTestAsync(new Patient
            {
                LabId = "LCS2",
                FullName = "Child",
                Gender = "Male",
                Age = 7,
                IsPregnant = false
            });

            var filtered = await _service.GetFilteredAntibioticsAsync(visitTestId);

            filtered.Should().HaveCount(2);
            filtered.Select(a => a.Name).Should().Contain(new[] { "SafeChild", "BothSafe" });
            filtered.Select(a => a.Name).Should().NotContain("UnsafeChild");
        }

        [Fact]
        public async Task SaveCultureResultAsync_Should_Save_Culture_Metadata_And_SIR_Classifications()
        {
            // Function: X.X — To Be Determined
            var test = new Test { Code = "CULT-2", NameReport = "Culture Test", NameReceipt = "Culture Test", Price = 10 };
            _db.Tests.Add(test);
            var antibiotic1 = new Antibiotic { Name = "Amoxicillin" };
            var antibiotic2 = new Antibiotic { Name = "Ciprofloxacin" };
            _db.Antibiotics.AddRange(antibiotic1, antibiotic2);
            var culture = new Culture
            {
                Name = "Urine Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 180
            };
            _db.Cultures.Add(culture);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = 1, TestId = test.TestId, Price = 10, Status = "Pending" };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            var sensitivities = new List<CultureSensitivityValue>
            {
                new CultureSensitivityValue { AntibioticId = antibiotic1.AntibioticId, Sensitivity = "Sensitive", Comment = "good" },
                new CultureSensitivityValue { AntibioticId = antibiotic2.AntibioticId, Sensitivity = "R", Comment = "avoid" }
            };

            await _service.SaveCultureResultAsync(visitTest.VisitTestId, culture.CultureId, sensitivities);

            var updated = await _db.VisitTests.FindAsync(visitTest.VisitTestId);
            updated!.Status.Should().Be("InProgress");

            var values = await _db.ResultValues.ToListAsync();
            values.Should().Contain(v =>
                (v.Value ?? string.Empty).Contains("Sample: Urine") &&
                (v.Value ?? string.Empty).Contains("Organism: E. coli") &&
                (v.Value ?? string.Empty).Contains("Colonies: 180"));
            values.Should().Contain(v => v.Value == "S" && v.Comment == "good");
            values.Should().Contain(v => v.Value == "R" && v.Comment == "avoid");
        }

        [Fact]
        public async Task SaveCultureResultAsync_When_VisitTestIsVerified_Should_Throw_FailureGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var culture = await _service.CreateCultureAsync(new Culture
            {
                Name = "Blood C",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 12
            });

            var test = new Test { Code = "C-LOCK", NameReport = "Culture Lock", NameReceipt = "Culture Lock", Price = 1m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = 1, TestId = test.TestId, Price = 1m, Status = "Verified" };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.SaveCultureResultAsync(visitTest.VisitTestId, culture.CultureId, new List<CultureSensitivityValue>());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*لا يمكن تعديل نتيجة معتمدة*");
        }

        private async Task<int> SeedVisitTestAsync(Patient patient)
        {
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = $"CULT-{Guid.NewGuid():N}".Substring(0, 12), NameReport = "Culture Test", NameReceipt = "Culture Test", Price = 1m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 1m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            return visitTest.VisitTestId;
        }
    }
}
