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
        public async Task CreateAntibioticAsync_Should_Create_And_Filter_LogicGuard()
        {
            // Refactored to Logic Guard - verifies all fields and filtering logic
            var a1 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "A1", IsSafeForChildren = true, IsSafeForPregnancy = false });
            var a2 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "A2", IsSafeForChildren = false, IsSafeForPregnancy = true });

            // Assert - Logic Guard: Verify all fields are saved correctly
            a1.AntibioticId.Should().BeGreaterThan(0);
            a2.AntibioticId.Should().BeGreaterThan(0);
            a1.IsSafeForChildren.Should().BeTrue();
            a1.IsSafeForPregnancy.Should().BeFalse();
            a2.IsSafeForChildren.Should().BeFalse();
            a2.IsSafeForPregnancy.Should().BeTrue();

            var all = await _service.GetAntibioticsAsync();
            all.Should().HaveCount(2);
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

            // Assert - Logic Guard: Verify filtering logic works correctly
            filtered.Should().ContainSingle();
            filtered[0].Name.Should().Be("A1");
            filtered[0].IsSafeForChildren.Should().BeTrue();
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

        [Fact]
        public async Task SearchCultureVisitTestsAsync_Should_Filter_By_Date_And_LabId()
        {
            // Arrange
            var patient = new Patient { LabId = "LAB123", FullName = "John", Gender = "Male" };
            _db.Patients.Add(patient);
            var test = new Test { NameReport = "Culture & Sensitivity", Price = 10 };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();
            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();
            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10 });
            await _db.SaveChangesAsync();

            // Act
            var results = await _service.SearchCultureVisitTestsAsync("123", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            results.Should().ContainSingle();
            results[0].LabId.Should().Be("LAB123");
        }

        [Fact]
        public async Task SaveCultureResultAsync_Should_Update_Status_And_Save_Values()
        {
            // Arrange
            var test = new Test { NameReport = "Culture Test", Price = 10 };
            _db.Tests.Add(test);
            var antibiotic = new Antibiotic { Name = "Amoxicillin" };
            _db.Antibiotics.Add(antibiotic);
            var culture = new Culture { Name = "E.Coli" };
            _db.Cultures.Add(culture);
            await _db.SaveChangesAsync();
            var vt = new VisitTest { VisitId = 1, TestId = test.TestId, Price = 10, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var sensitivities = new List<CultureSensitivityValue>
            {
                new CultureSensitivityValue { AntibioticId = antibiotic.AntibioticId, Sensitivity = "Sensitive", Comment = "High dose" }
            };

            // Act
            await _service.SaveCultureResultAsync(vt.VisitTestId, culture.CultureId, sensitivities);

            // Assert
            var updated = await _db.VisitTests.FindAsync(vt.VisitTestId);
            updated!.Status.Should().Be("InProgress");
            var values = await _db.ResultValues.ToListAsync();
            values.Should().Contain(v => v.Value == "E.Coli");
            values.Should().Contain(v => v.Value == "Sensitive" && v.Comment == "High dose");
        }

        // 5.5 - Culture Sensitivity Filtering - Clinical Filter Rules Tests
        [Fact]
        public async Task GetFilteredAntibioticsAsync_PregnantPatient_Should_OnlyReturn_SafeForPregnancy()
        {
            // Arrange
            var a1 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "SafePreg", IsSafeForChildren = false, IsSafeForPregnancy = true });
            var a2 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "UnsafePreg", IsSafeForChildren = true, IsSafeForPregnancy = false });
            var a3 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "BothSafe", IsSafeForChildren = true, IsSafeForPregnancy = true });

            var patient = new Patient { LabId = "LCS1", FullName = "P", Gender = "Female", Age = 25, IsPregnant = true };
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

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(vt.VisitTestId);

            // Assert - Logic Guard: Only pregnancy-safe antibiotics should be returned
            filtered.Should().HaveCount(2);
            filtered.Should().Contain(a => a.Name == "SafePreg");
            filtered.Should().Contain(a => a.Name == "BothSafe");
            filtered.Should().NotContain(a => a.Name == "UnsafePreg");
        }

        [Fact]
        public async Task GetFilteredAntibioticsAsync_ChildPatient_Should_OnlyReturn_SafeForChildren()
        {
            // Arrange
            var a1 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "SafeChild", IsSafeForChildren = true, IsSafeForPregnancy = false });
            var a2 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "UnsafeChild", IsSafeForChildren = false, IsSafeForPregnancy = true });
            var a3 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "BothSafe", IsSafeForChildren = true, IsSafeForPregnancy = true });

            var patient = new Patient { LabId = "LCS2", FullName = "C", Gender = "Male", Age = 5, IsPregnant = false };
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

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(vt.VisitTestId);

            // Assert - Logic Guard: Only child-safe antibiotics should be returned
            filtered.Should().HaveCount(2);
            filtered.Should().Contain(a => a.Name == "SafeChild");
            filtered.Should().Contain(a => a.Name == "BothSafe");
            filtered.Should().NotContain(a => a.Name == "UnsafeChild");
        }

        [Fact]
        public async Task GetFilteredAntibioticsAsync_PregnantChild_Should_Require_Both_SafeFlags()
        {
            // Arrange
            var a1 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "OnlyPregSafe", IsSafeForChildren = false, IsSafeForPregnancy = true });
            var a2 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "OnlyChildSafe", IsSafeForChildren = true, IsSafeForPregnancy = false });
            var a3 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "BothSafe", IsSafeForChildren = true, IsSafeForPregnancy = true });
            var a4 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "NeitherSafe", IsSafeForChildren = false, IsSafeForPregnancy = false });

            var patient = new Patient { LabId = "LCS3", FullName = "PC", Gender = "Female", Age = 8, IsPregnant = true };
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

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(vt.VisitTestId);

            // Assert - Logic Guard: Only antibiotics safe for both pregnancy and children
            filtered.Should().ContainSingle();
            filtered[0].Name.Should().Be("BothSafe");
            filtered.Should().NotContain(a => a.Name == "OnlyPregSafe");
            filtered.Should().NotContain(a => a.Name == "OnlyChildSafe");
            filtered.Should().NotContain(a => a.Name == "NeitherSafe");
        }

        [Fact]
        public async Task GetFilteredAntibioticsAsync_AdultNonPregnant_Should_Return_All()
        {
            // Arrange
            var a1 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "A1", IsSafeForChildren = false, IsSafeForPregnancy = false });
            var a2 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "A2", IsSafeForChildren = true, IsSafeForPregnancy = false });
            var a3 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "A3", IsSafeForChildren = false, IsSafeForPregnancy = true });

            var patient = new Patient { LabId = "LCS4", FullName = "Adult", Gender = "Male", Age = 30, IsPregnant = false };
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

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(vt.VisitTestId);

            // Assert - Logic Guard: All antibiotics should be returned for adult non-pregnant
            filtered.Should().HaveCount(3);
            filtered.Should().Contain(a => a.Name == "A1");
            filtered.Should().Contain(a => a.Name == "A2");
            filtered.Should().Contain(a => a.Name == "A3");
        }

        [Fact]
        public async Task GetFilteredAntibioticsAsync_NoVisitTest_Should_Return_All()
        {
            // Arrange
            var a1 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "X1", IsSafeForChildren = false, IsSafeForPregnancy = false });
            var a2 = await _service.CreateAntibioticAsync(new Antibiotic { Name = "X2", IsSafeForChildren = true, IsSafeForPregnancy = true });

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(99999);

            // Assert - Logic Guard: When visit test not found, return all antibiotics
            filtered.Should().HaveCount(2);
            filtered.Should().Contain(a => a.Name == "X1");
            filtered.Should().Contain(a => a.Name == "X2");
        }

        // 5.4 Clinical Classification Tests - NEW TEST

        [Fact]
        public async Task SaveCultureResultAsync_Should_Classify_Sensitivity_Correctly_LogicGuard()
        {
            // 5.4 Clinical Classification - Logic Guard: Verify sensitivity classification
            // Arrange
            var test = new Test { NameReport = "Culture Test", Price = 10 };
            _db.Tests.Add(test);
            var antibiotic1 = new Antibiotic { Name = "Amoxicillin" };
            var antibiotic2 = new Antibiotic { Name = "Ciprofloxacin" };
            var antibiotic3 = new Antibiotic { Name = "Vancomycin" };
            _db.Antibiotics.AddRange(antibiotic1, antibiotic2, antibiotic3);
            var culture = new Culture { Name = "E.Coli" };
            _db.Cultures.Add(culture);
            await _db.SaveChangesAsync();
            var vt = new VisitTest { VisitId = 1, TestId = test.TestId, Price = 10, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var sensitivities = new List<CultureSensitivityValue>
            {
                new CultureSensitivityValue { AntibioticId = antibiotic1.AntibioticId, Sensitivity = "Sensitive", Comment = "Effective" },
                new CultureSensitivityValue { AntibioticId = antibiotic2.AntibioticId, Sensitivity = "Resistant", Comment = "Not effective" },
                new CultureSensitivityValue { AntibioticId = antibiotic3.AntibioticId, Sensitivity = "Intermediate", Comment = "Moderate" }
            };

            // Act
            await _service.SaveCultureResultAsync(vt.VisitTestId, culture.CultureId, sensitivities);

            // Assert - Logic Guard: Verify all sensitivity classifications are saved correctly
            var updated = await _db.VisitTests.FindAsync(vt.VisitTestId);
            updated!.Status.Should().Be("InProgress");

            var values = await _db.ResultValues.ToListAsync();
            values.Should().Contain(v => v.Value == "E.Coli");
            values.Should().Contain(v => v.Value == "Sensitive" && v.Comment == "Effective");
            values.Should().Contain(v => v.Value == "Resistant" && v.Comment == "Not effective");
            values.Should().Contain(v => v.Value == "Intermediate" && v.Comment == "Moderate");

            // Verify classification logic - Sensitive, Resistant, Intermediate are all valid
            var sensitiveResult = values.FirstOrDefault(v => v.Value == "Sensitive");
            var resistantResult = values.FirstOrDefault(v => v.Value == "Resistant");
            var intermediateResult = values.FirstOrDefault(v => v.Value == "Intermediate");

            sensitiveResult.Should().NotBeNull("Sensitive classification should be saved");
            resistantResult.Should().NotBeNull("Resistant classification should be saved");
            intermediateResult.Should().NotBeNull("Intermediate classification should be saved");
        }
    }
}
