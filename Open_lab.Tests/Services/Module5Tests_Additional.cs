using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests
{
    /// <summary>
    /// Additional comprehensive tests for Module 5: Culture & Sensitivity Module
    /// Covers Functions 5.1-5.7 with Service and ViewModel layer tests
    /// </summary>
    public class Module5ServiceTests_Additional : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly CultureSensitivityService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module5ServiceTests_Additional()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new CultureSensitivityService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        #region Function 5.1 - Enter Culture Data (BR-MED-004)

        [Fact]
        public async Task CreateCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard()
        {
            // Function: 5.1 — Enter Culture Data (Zero Colony Count Edge Case)
            // Arrange
            var culture = new Culture
            {
                Name = "No Growth Culture",
                SampleType = "Urine",
                IsolatedOrganism = "No Growth",
                GrowthConditions = "Aerobic",
                ColonyCount = 0
            };

            // Act
            var created = await _service.CreateCultureAsync(culture);

            // Assert
            created.CultureId.Should().BeGreaterThan(0);
            created.ColonyCount.Should().Be(0);
        }

        [Fact]
        public async Task CreateCultureAsync_With_Null_Organism_Should_Succeed_EdgeGuard()
        {
            // Function: 5.1 — Enter Culture Data (Null Organism Edge Case)
            // Arrange
            var culture = new Culture
            {
                Name = "Pending Culture",
                SampleType = "Blood",
                IsolatedOrganism = null,
                GrowthConditions = "Anaerobic",
                ColonyCount = 50
            };

            // Act
            var created = await _service.CreateCultureAsync(culture);

            // Assert
            created.CultureId.Should().BeGreaterThan(0);
            created.IsolatedOrganism.Should().BeNull();
        }

        [Fact]
        public async Task CreateCultureAsync_With_Null_Organism_Should_Throw_FailureGuard()
        {
            // Function: 5.1 — Enter Culture Data (BR-MED-004: Validation)
            // Arrange
            var culture = new Culture
            {
                Name = "Invalid Culture",
                SampleType = "CSF",
                IsolatedOrganism = null,
                GrowthConditions = null,
                ColonyCount = 10
            };

            // Act
            Func<Task> act = async () => await _service.CreateCultureAsync(culture);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteCultureAsync_Should_Remove_Culture_SuccessGuard()
        {
            // Function: 5.1 — Enter Culture Data (Delete Culture)
            // Arrange
            var culture = new Culture
            {
                Name = "Delete Me",
                SampleType = "Stool",
                IsolatedOrganism = "Salmonella",
                GrowthConditions = "Aerobic",
                ColonyCount = 200
            };
            var created = await _service.CreateCultureAsync(culture);

            // Act
            await _service.DeleteCultureAsync(created.CultureId);

            // Assert
            var cultures = await _service.GetCulturesAsync();
            cultures.Should().NotContain(c => c.CultureId == created.CultureId);
        }

        [Fact]
        public async Task DeleteCultureAsync_With_NonExistent_Id_Should_NotThrow_EdgeGuard()
        {
            // Function: 5.1 — Enter Culture Data (Delete Non-Existent Edge Case)
            // Act & Assert
            Func<Task> act = async () => await _service.DeleteCultureAsync(9999);
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region Function 5.2 - Add Antibiotics

        [Fact]
        public async Task CreateAntibioticAsync_With_All_Safety_Flags_Should_Persist_EdgeGuard()
        {
            // Function: 5.2 — Add Antibiotics (All Safety Flags Set)
            // Arrange
            var antibiotic = new Antibiotic
            {
                Name = "Safe Antibiotic",
                IsSafeForChildren = true,
                IsSafeForPregnancy = true
            };

            // Act
            var created = await _service.CreateAntibioticAsync(antibiotic);

            // Assert
            created.AntibioticId.Should().BeGreaterThan(0);
            created.IsSafeForChildren.Should().BeTrue();
            created.IsSafeForPregnancy.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAntibioticAsync_With_Neither_Safety_Flag_Should_Persist_EdgeGuard()
        {
            // Function: 5.2 — Add Antibiotics (Neither Safety Flag Set)
            // Arrange
            var antibiotic = new Antibiotic
            {
                Name = "Restricted Antibiotic",
                IsSafeForChildren = false,
                IsSafeForPregnancy = false
            };

            // Act
            var created = await _service.CreateAntibioticAsync(antibiotic);

            // Assert
            created.AntibioticId.Should().BeGreaterThan(0);
            created.IsSafeForChildren.Should().BeFalse();
            created.IsSafeForPregnancy.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAntibioticAsync_Should_Remove_Antibiotic_SuccessGuard()
        {
            // Function: 5.2 — Add Antibiotics (Delete Antibiotic)
            // Arrange
            var antibiotic = await _service.CreateAntibioticAsync(new Antibiotic { Name = "To Delete" });

            // Act
            await _service.DeleteAntibioticAsync(antibiotic.AntibioticId);

            // Assert
            var antibiotics = await _service.GetAntibioticsAsync();
            antibiotics.Should().NotContain(a => a.AntibioticId == antibiotic.AntibioticId);
        }

        [Fact]
        public async Task DeleteAntibioticAsync_With_NonExistent_Id_Should_NotThrow_EdgeGuard()
        {
            // Function: 5.2 — Add Antibiotics (Delete Non-Existent Edge Case)
            // Act & Assert
            Func<Task> act = async () => await _service.DeleteAntibioticAsync(8888);
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region Function 5.3 & 5.4 - Set Sensitivity & Classify Sensitivity (BR-MED-005)

        [Fact]
        public async Task SaveCultureResultAsync_With_Multiple_Sensitivities_Should_Classify_Each_SuccessGuard()
        {
            // Function: 5.3 — Set Sensitivity (BR-MED-005: Multiple Sensitivities)
            // Arrange
            var test = new Test { Code = "CULT-SENS", NameReport = "Culture Sensitivity", NameReceipt = "Culture", Price = 10m };
            _db.Tests.Add(test);

            var antibiotic1 = new Antibiotic { Name = "Abx1" };
            var antibiotic2 = new Antibiotic { Name = "Abx2" };
            var antibiotic3 = new Antibiotic { Name = "Abx3" };
            _db.Antibiotics.AddRange(antibiotic1, antibiotic2, antibiotic3);
            await _db.SaveChangesAsync();

            var culture = await _service.CreateCultureAsync(new Culture
            {
                Name = "Multi-Sens Culture",
                SampleType = "Wound",
                IsolatedOrganism = "Pseudomonas",
                GrowthConditions = "Aerobic",
                ColonyCount = 300
            });

            var visitTest = new VisitTest { VisitId = 1, TestId = test.TestId, Price = 10m, Status = "Pending" };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            var sensitivities = new List<CultureSensitivityValue>
            {
                new() { AntibioticId = antibiotic1.AntibioticId, Sensitivity = "Sensitive", Comment = "Good response" },
                new() { AntibioticId = antibiotic2.AntibioticId, Sensitivity = "Intermediate", Comment = "Partial response" },
                new() { AntibioticId = antibiotic3.AntibioticId, Sensitivity = "Resistant", Comment = "No response" }
            };

            // Act
            await _service.SaveCultureResultAsync(visitTest.VisitTestId, culture.CultureId, sensitivities);

            // Assert - Verify all values were saved correctly
            var values = await _db.ResultValues
                .Where(v => v.VisitTestId == visitTest.VisitTestId)
                .ToListAsync();

            values.Should().HaveCount(3);
            values.Should().Contain(v => v.Value == "S");
            values.Should().Contain(v => v.Value == "I");
            values.Should().Contain(v => v.Value == "R");
        }

        [Theory]
        [InlineData("sensitive", "S")]
        [InlineData("SENSITIVE", "S")]
        [InlineData("s", "S")]
        [InlineData("intermediate", "I")]
        [InlineData("INTERMEDIATE", "I")]
        [InlineData("i", "I")]
        [InlineData("resistant", "R")]
        [InlineData("RESISTANT", "R")]
        [InlineData("r", "R")]
        public void ClassifySensitivity_Should_Handle_Case_Insensitive_SuccessGuard(string raw, string expected)
        {
            // Function: 5.4 — Classify Sensitivity (BR-MED-005: Case Insensitive)
            // Act
            var result = _service.ClassifySensitivity(raw);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("مستعمرات قليله", "S")]
        [InlineData("حساس جدا", "S")]
        [InlineData("مقاومة", "R")]
        [InlineData("متوسط الحساسية", "I")]
        public void ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(string raw, string expected)
        {
            // Function: 5.4 — Classify Sensitivity (BR-MED-005: Arabic Support)
            // Act
            var result = _service.ClassifySensitivity(raw);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void ClassifySensitivity_With_Empty_String_Should_Return_Empty_EdgeGuard()
        {
            // Function: 5.4 — Classify Sensitivity (Empty String Edge Case)
            // Act
            var result = _service.ClassifySensitivity("");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ClassifySensitivity_With_Invalid_Value_Should_Throw_FailureGuard()
        {
            // Function: 5.4 — Classify Sensitivity (BR-MED-005: Invalid Classification)
            // Act
            Action act = () => _service.ClassifySensitivity("INVALID");

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ClassifySensitivity_With_Null_Should_Return_Empty_EdgeGuard()
        {
            // Function: 5.4 — Classify Sensitivity (Null Edge Case)
            // Act
            var result = _service.ClassifySensitivity(null);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Function 5.5 - Filter Pregnancy Antibiotics (BR-MED-006)

        [Fact]
        public async Task GetFilteredAntibioticsAsync_Pregnant_With_Mixed_Safety_Should_Return_Only_Safe_SuccessGuard()
        {
            // Function: 5.5 — Filter Pregnancy Antibiotics (BR-MED-006)
            // Arrange
            var safePreg = await _service.CreateAntibioticAsync(new Antibiotic { Name = "SafeForPreg", IsSafeForPregnancy = true });
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "UnsafeForPreg", IsSafeForPregnancy = false });
            var bothSafe = await _service.CreateAntibioticAsync(new Antibiotic { Name = "BothSafe", IsSafeForPregnancy = true });

            var visitTestId = await SeedVisitTestAsync(new Patient
            {
                LabId = "LPREG1",
                FullName = "Pregnant Patient",
                Gender = "Female",
                Age = 28,
                IsPregnant = true
            });

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(visitTestId);

            // Assert
            filtered.Should().HaveCount(2);
            filtered.Select(a => a.Name).Should().Contain("SafeForPreg");
            filtered.Select(a => a.Name).Should().Contain("BothSafe");
            filtered.Select(a => a.Name).Should().NotContain("UnsafeForPreg");
        }

        [Fact]
        public async Task GetFilteredAntibioticsAsync_Non_Pregnant_Should_Return_All_EdgeGuard()
        {
            // Function: 5.5 — Filter Pregnancy Antibiotics (Non-Pregnant Patient Edge Case)
            // Arrange
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "Abx1", IsSafeForPregnancy = true });
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "Abx2", IsSafeForPregnancy = false });

            var visitTestId = await SeedVisitTestAsync(new Patient
            {
                LabId = "LNP1",
                FullName = "Non-Pregnant Patient",
                Gender = "Female",
                Age = 35,
                IsPregnant = false
            });

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(visitTestId);

            // Assert - All antibiotics should be returned
            filtered.Should().HaveCount(2);
        }

        #endregion

        #region Function 5.6 - Filter Children Antibiotics (BR-MED-007)

        [Fact]
        public async Task GetFilteredAntibioticsAsync_Child_With_Mixed_Safety_Should_Return_Only_Safe_SuccessGuard()
        {
            // Function: 5.6 — Filter Children Antibiotics (BR-MED-007)
            // Arrange
            var safeChild = await _service.CreateAntibioticAsync(new Antibiotic { Name = "SafeForChild", IsSafeForChildren = true });
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "UnsafeForChild", IsSafeForChildren = false });
            var bothSafe = await _service.CreateAntibioticAsync(new Antibiotic { Name = "BothSafeChild", IsSafeForChildren = true });

            var visitTestId = await SeedVisitTestAsync(new Patient
            {
                LabId = "LCHILD1",
                FullName = "Child Patient",
                Gender = "Male",
                Age = 6,
                IsPregnant = false
            });

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(visitTestId);

            // Assert
            filtered.Should().HaveCount(2);
            filtered.Select(a => a.Name).Should().Contain("SafeForChild");
            filtered.Select(a => a.Name).Should().Contain("BothSafeChild");
            filtered.Select(a => a.Name).Should().NotContain("UnsafeForChild");
        }

        [Fact]
        public async Task GetFilteredAntibioticsAsync_Adult_Should_Return_All_EdgeGuard()
        {
            // Function: 5.6 — Filter Children Antibiotics (Adult Patient Edge Case)
            // Arrange
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "AbxAdult1", IsSafeForChildren = true });
            await _service.CreateAntibioticAsync(new Antibiotic { Name = "AbxAdult2", IsSafeForChildren = false });

            var visitTestId = await SeedVisitTestAsync(new Patient
            {
                LabId = "LADULT1",
                FullName = "Adult Patient",
                Gender = "Male",
                Age = 45,
                IsPregnant = false
            });

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(visitTestId);

            // Assert - All antibiotics should be returned
            filtered.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetFilteredAntibioticsAsync_Combined_Filter_Pregnant_And_Child_Should_Apply_Both_SuccessGuard()
        {
            // Function: 5.5 & 5.6 — Combined Filter (BR-MED-006, BR-MED-007)
            // Arrange - Pregnant child patient (edge case)
            var safe = await _service.CreateAntibioticAsync(new Antibiotic
            {
                Name = "SafeForBoth",
                IsSafeForPregnancy = true,
                IsSafeForChildren = true
            });
            await _service.CreateAntibioticAsync(new Antibiotic
            {
                Name = "UnsafePregOnly",
                IsSafeForPregnancy = false,
                IsSafeForChildren = true
            });
            await _service.CreateAntibioticAsync(new Antibiotic
            {
                Name = "UnsafeChildOnly",
                IsSafeForPregnancy = true,
                IsSafeForChildren = false
            });

            var visitTestId = await SeedVisitTestAsync(new Patient
            {
                LabId = "LPREGCHILD",
                FullName = "Pregnant Teen",
                Gender = "Female",
                Age = 15, // Under 18 - child but potentially pregnant
                IsPregnant = true
            });

            // Act
            var filtered = await _service.GetFilteredAntibioticsAsync(visitTestId);

            // Assert - Only antibiotics safe for BOTH pregnancy and children
            filtered.Should().HaveCount(1);
            filtered.First().Name.Should().Be("SafeForBoth");
        }

        #endregion

        #region Function 5.7 - Print Culture Report (Audit Logging)

        [Fact]
        public async Task SearchCultureVisitTestsAsync_With_Empty_LabId_Should_Return_All_EdgeGuard()
        {
            // Function: 5.7 — Print Culture Report (Search Logic)
            // Arrange
            var patient1 = new Patient { LabId = "L-A", FullName = "Patient A", Gender = "Male" };
            var patient2 = new Patient { LabId = "L-B", FullName = "Patient B", Gender = "Female" };
            _db.Patients.AddRange(patient1, patient2);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = patient1.PatientId, VisitDate = DateTime.Today };
            var visit2 = new Visit { PatientId = patient2.PatientId, VisitDate = DateTime.Today };
            _db.Visits.AddRange(visit1, visit2);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "CULT-PRINT", NameReport = "Culture", NameReceipt = "C", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit1.VisitId, TestId = test.TestId, Price = 10m });
            _db.VisitTests.Add(new VisitTest { VisitId = visit2.VisitId, TestId = test.TestId, Price = 10m });
            await _db.SaveChangesAsync();

            // Act - Empty/null LabId should return all
            var results = await _service.SearchCultureVisitTestsAsync("", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task SearchCultureVisitTestsAsync_With_NonExistent_LabId_Should_Return_Empty_EdgeGuard()
        {
            // Function: 5.7 — Print Culture Report (No Results Edge Case)
            // Act
            var results = await _service.SearchCultureVisitTestsAsync("NONEXISTENT", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Link/Unlink Antibiotics Tests

        [Fact]
        public async Task LinkAntibioticAsync_Should_Add_Link_SuccessGuard()
        {
            // Function: 5.1 — Enter Culture Data (Link Antibiotic to Culture)
            // Arrange
            var culture = await _service.CreateCultureAsync(new Culture
            {
                Name = "Link Test Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 100
            });
            var antibiotic = await _service.CreateAntibioticAsync(new Antibiotic { Name = "Linked Abx" });

            // Act
            await _service.LinkAntibioticAsync(culture.CultureId, antibiotic.AntibioticId);

            // Assert
            var links = await _service.GetCultureAntibioticsAsync(culture.CultureId);
            links.Should().ContainSingle();
            links[0].AntibioticId.Should().Be(antibiotic.AntibioticId);
        }

        [Fact]
        public async Task UnlinkAntibioticAsync_Should_Remove_Link_SuccessGuard()
        {
            // Function: 5.1 — Enter Culture Data (Unlink Antibiotic from Culture)
            // Arrange
            var culture = await _service.CreateCultureAsync(new Culture
            {
                Name = "Unlink Test Culture",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 50
            });
            var antibiotic = await _service.CreateAntibioticAsync(new Antibiotic { Name = "Unlinked Abx" });
            await _service.LinkAntibioticAsync(culture.CultureId, antibiotic.AntibioticId);

            // Act
            await _service.UnlinkAntibioticAsync(culture.CultureId, antibiotic.AntibioticId);

            // Assert
            var links = await _service.GetCultureAntibioticsAsync(culture.CultureId);
            links.Should().BeEmpty();
        }

        #endregion

        private async Task<int> SeedVisitTestAsync(Patient patient)
        {
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = $"CULT-{Guid.NewGuid():N}".Substring(0, 12), NameReport = "Culture Test", NameReceipt = "C", Price = 1m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 1m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            return visitTest.VisitTestId;
        }
    }

    /// <summary>
    /// Additional ViewModel tests for Module 5
    /// </summary>
    public class Module5ViewModelTests_Additional : IDisposable
    {
        private readonly Mock<ICultureSensitivityService> _serviceMock;
        private readonly Mock<IPrintService> _printServiceMock;
        private readonly CultureSensitivityViewModel _viewModel;

        public Module5ViewModelTests_Additional()
        {
            AppSessionTestHelper.ResetToAdmin();
            _serviceMock = new Mock<ICultureSensitivityService>();
            _printServiceMock = new Mock<IPrintService>();

            _serviceMock.Setup(s => s.GetCulturesAsync()).ReturnsAsync(new List<Culture>());
            _serviceMock.Setup(s => s.GetAntibioticsAsync()).ReturnsAsync(new List<Antibiotic>());
            _serviceMock.Setup(s => s.GetCultureAntibioticsAsync(It.IsAny<int>())).ReturnsAsync(new List<CultureAntibiotic>());
            _serviceMock.Setup(s => s.SearchCultureVisitTestsAsync(It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<CultureVisitTestRow>());
            _serviceMock.Setup(s => s.GetFilteredAntibioticsAsync(It.IsAny<int>())).ReturnsAsync(new List<Antibiotic>());
            _serviceMock.Setup(s => s.ClassifySensitivity(It.IsAny<string?>())).Returns((string? raw) => raw ?? string.Empty);

            _viewModel = new CultureSensitivityViewModel(_serviceMock.Object, _printServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        #region Function 5.1 ViewModel Tests

        [Fact]
        public async Task AddCultureAsync_With_Empty_Name_Should_Set_Error_FailureGuard()
        {
            // Function: 5.1 — Enter Culture Data (Empty Name Validation)
            _viewModel.NewCultureName = "";
            _viewModel.NewCultureSampleType = "Urine";
            _viewModel.NewCultureOrganism = "E. coli";
            _viewModel.NewCultureConditions = "Aerobic";
            _viewModel.NewCultureColonyCount = 100;

            await _viewModel.InvokePrivateAsync("AddCultureAsync");

            _viewModel.StatusMessage.Should().Contain("خطأ:");
        }

        [Fact]
        public async Task AddCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard()
        {
            // Function: 5.1 — Enter Culture Data (Zero Colony Count)
            _viewModel.NewCultureName = "No Growth";
            _viewModel.NewCultureSampleType = "Urine";
            _viewModel.NewCultureOrganism = "No Growth";
            _viewModel.NewCultureConditions = "Aerobic";
            _viewModel.NewCultureColonyCount = 0;

            _serviceMock.Setup(s => s.CreateCultureAsync(It.IsAny<Culture>()))
                .ReturnsAsync((Culture c) => { c.CultureId = 1; return c; });

            await _viewModel.InvokePrivateAsync("AddCultureAsync");

            _serviceMock.Verify(s => s.CreateCultureAsync(It.Is<Culture>(c => c.ColonyCount == 0)), Times.Once);
        }

        #endregion

        #region Function 5.2 ViewModel Tests

        [Fact]
        public async Task AddAntibioticAsync_With_Empty_Name_Should_Set_Error_FailureGuard()
        {
            // Function: 5.2 — Add Antibiotics (Empty Name Validation)
            _viewModel.NewAntibioticName = "";
            _viewModel.NewAntibioticSafeForPregnancy = true;
            _viewModel.NewAntibioticSafeForChildren = true;

            await _viewModel.InvokePrivateAsync("AddAntibioticAsync");

            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _serviceMock.Verify(s => s.CreateAntibioticAsync(It.IsAny<Antibiotic>()), Times.Never);
        }

        [Fact]
        public async Task AddAntibioticAsync_When_Service_Throws_Should_Set_Error_FailureGuard()
        {
            // Function: 5.2 — Add Antibiotics (Service Exception Handling)
            _viewModel.NewAntibioticName = "NewAbx";
            _viewModel.NewAntibioticSafeForPregnancy = true;
            _viewModel.NewAntibioticSafeForChildren = true;

            _serviceMock.Setup(s => s.CreateAntibioticAsync(It.IsAny<Antibiotic>()))
                .ThrowsAsync(new Exception("database-error"));

            await _viewModel.InvokePrivateAsync("AddAntibioticAsync");

            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("database-error");
        }

        #endregion

        #region Function 5.3 & 5.4 ViewModel Tests

        [Fact]
        public async Task SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard()
        {
            // Function: 5.3 — Set Sensitivity (Empty Results Edge Case)
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 10,
                Name = "Test Culture",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 50
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 20,
                VisitId = 5,
                LabId = "LAB-TEST",
                PatientName = "Test"
            };
            _viewModel.ResultRows.Clear(); // No results to save

            await _viewModel.InvokePrivateAsync("SaveResultAsync");

            _viewModel.StatusMessage.Should().Contain("لا توجد نتائج");
            _serviceMock.Verify(s => s.SaveCultureResultAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<CultureSensitivityValue>>()), Times.Never);
        }

        [Fact]
        public async Task BuildResultRowsAsync_With_Pregnant_Patient_Should_Filter_Antibiotics_SuccessGuard()
        {
            // Function: 5.5 — Filter Pregnancy Antibiotics (BR-MED-006)
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 30,
                Name = "Urine Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 200
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 40,
                VisitId = 10,
                LabId = "L-PREG",
                PatientName = "Pregnant Patient",
                PatientIsPregnant = true
            };

            var antibiotics = new List<Antibiotic>
            {
                new() { AntibioticId = 1, Name = "SafePreg", IsSafeForPregnancy = true },
                new() { AntibioticId = 2, Name = "UnsafePreg", IsSafeForPregnancy = false }
            };

            _serviceMock.Setup(s => s.GetCultureAntibioticsAsync(30)).ReturnsAsync(
                antibiotics.Select(a => new CultureAntibiotic { CultureId = 30, AntibioticId = a.AntibioticId, Antibiotic = a }).ToList()
            );
            _serviceMock.Setup(s => s.GetFilteredAntibioticsAsync(40)).ReturnsAsync(
                antibiotics.Where(a => a.IsSafeForPregnancy).ToList()
            );

            await _viewModel.InvokePrivateAsync("BuildResultRowsAsync");

            _viewModel.ResultRows.Should().HaveCount(1);
            _viewModel.ResultRows[0].AntibioticName.Should().Be("SafePreg");
        }

        [Fact]
        public async Task BuildResultRowsAsync_With_Child_Patient_Should_Filter_Antibiotics_SuccessGuard()
        {
            // Function: 5.6 — Filter Children Antibiotics (BR-MED-007)
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 31,
                Name = "Blood Culture",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 100
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 41,
                VisitId = 11,
                LabId = "L-CHILD",
                PatientName = "Child Patient",
                PatientAge = 8
            };

            var antibiotics = new List<Antibiotic>
            {
                new() { AntibioticId = 1, Name = "SafeChild", IsSafeForChildren = true },
                new() { AntibioticId = 2, Name = "UnsafeChild", IsSafeForChildren = false }
            };

            _serviceMock.Setup(s => s.GetCultureAntibioticsAsync(31)).ReturnsAsync(
                antibiotics.Select(a => new CultureAntibiotic { CultureId = 31, AntibioticId = a.AntibioticId, Antibiotic = a }).ToList()
            );
            _serviceMock.Setup(s => s.GetFilteredAntibioticsAsync(41)).ReturnsAsync(
                antibiotics.Where(a => a.IsSafeForChildren).ToList()
            );

            await _viewModel.InvokePrivateAsync("BuildResultRowsAsync");

            _viewModel.ResultRows.Should().HaveCount(1);
            _viewModel.ResultRows[0].AntibioticName.Should().Be("SafeChild");
        }

        #endregion

        #region Function 5.7 ViewModel Tests

        [Fact]
        public async Task PrintCultureReportAsync_With_Null_Culture_Should_Set_Error_FailureGuard()
        {
            // Function: 5.7 — Print Culture Report (Missing Culture Edge Case)
            _viewModel.SelectedCulture = null;
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 50,
                VisitId = 15,
                LabId = "L-PRINT",
                PatientName = "Patient"
            };

            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            _viewModel.StatusMessage.Should().Contain("بيانات الطباعة غير مكتملة");
            _printServiceMock.Verify(p => p.PrintCultureReportAsync(It.IsAny<CultureReportData>()), Times.Never);
        }

        [Fact]
        public async Task PrintCultureReportAsync_With_Null_VisitTest_Should_Set_Error_FailureGuard()
        {
            // Function: 5.7 — Print Culture Report (Missing Visit Edge Case)
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 60,
                Name = "Test Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 100
            };
            _viewModel.SelectedVisitTest = null;

            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            _viewModel.StatusMessage.Should().Contain("بيانات الطباعة غير مكتملة");
        }

        [Fact]
        public async Task PrintCultureReportAsync_With_Empty_ResultRows_Should_Print_Anyway_SuccessGuard()
        {
            // Function: 5.7 — Print Culture Report (Empty Results - Valid Scenario)
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 70,
                Name = "No Growth Culture",
                SampleType = "Urine",
                IsolatedOrganism = "No Growth",
                GrowthConditions = "Aerobic",
                ColonyCount = 0
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 80,
                VisitId = 20,
                LabId = "L-PRINT2",
                PatientName = "Patient"
            };
            _viewModel.ResultRows.Clear();

            CultureReportData? printedData = null;
            _printServiceMock.Setup(p => p.PrintCultureReportAsync(It.IsAny<CultureReportData>()))
                .Callback<CultureReportData>(data => printedData = data)
                .Returns(Task.CompletedTask);

            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            _printServiceMock.Verify(p => p.PrintCultureReportAsync(It.IsAny<CultureReportData>()), Times.Once);
            printedData.Should().NotBeNull();
            printedData!.CultureName.Should().Be("No Growth Culture");
        }

        [Fact]
        public async Task PrintCultureReportAsync_When_PrintService_Throws_Should_Set_Error_FailureGuard()
        {
            // Function: 5.7 — Print Culture Report (Print Service Exception)
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 90,
                Name = "Print Test Culture",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 150
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 100,
                VisitId = 25,
                LabId = "L-PRINT3",
                PatientName = "Patient"
            };
            _viewModel.ResultRows.Add(new CultureSensitivityRow
            {
                AntibioticId = 1,
                AntibioticName = "Test Abx",
                Sensitivity = "S",
                Comment = "Test"
            });

            _printServiceMock.Setup(p => p.PrintCultureReportAsync(It.IsAny<CultureReportData>()))
                .ThrowsAsync(new InvalidOperationException("Printer offline"));

            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("Printer offline");
        }

        #endregion

        #region Search Functionality Tests

        [Fact]
        public async Task SearchCommand_With_Valid_LabId_Should_Load_Results_SuccessGuard()
        {
            // Function: 5.1 — Enter Culture Data (Search by LabId)
            _viewModel.SearchLabId = "LAB123";

            var searchResults = new List<CultureVisitTestRow>
            {
                new() { VisitTestId = 1, LabId = "LAB123", PatientName = "Test Patient" }
            };
            _serviceMock.Setup(s => s.SearchCultureVisitTestsAsync("LAB123", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(searchResults);

            await _viewModel.InvokePrivateAsync("SearchCommand");

            _serviceMock.Verify(s => s.SearchCultureVisitTestsAsync("LAB123", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task SearchCommand_With_NonExistent_LabId_Should_Show_NotFound_EdgeGuard()
        {
            // Function: 5.1 — Enter Culture Data (Search No Results)
            _viewModel.SearchLabId = "NONEXISTENT";

            _serviceMock.Setup(s => s.SearchCultureVisitTestsAsync("NONEXISTENT", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<CultureVisitTestRow>());

            await _viewModel.InvokePrivateAsync("SearchCommand");

            _viewModel.StatusMessage.Should().Contain("لم يتم العثور");
        }

        #endregion
    }
}