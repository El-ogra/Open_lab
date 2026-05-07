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
            _serviceMock.Setup(s => s.SearchCultureVisitTestsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
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

            // Arrange
            _viewModel.NewCultureName = "";
            _viewModel.NewCultureSampleType = "Urine";
            _viewModel.NewCultureOrganism = "E. coli";
            _viewModel.NewCultureConditions = "Aerobic";
            _viewModel.NewCultureColonyCount = 100;

            // Act
            await _viewModel.InvokePrivateAsync("AddCultureAsync");

            // Assert
            // Production validation message for empty culture name (no "خطأ:" prefix on validation guards).
            _viewModel.StatusMessage.Should().Contain("أدخل اسم المزرعة");
            _serviceMock.Verify(s => s.CreateCultureAsync(It.IsAny<Culture>()), Times.Never);
        }

        [Fact]
        public async Task AddCultureAsync_With_Minimum_ColonyCount_Should_Succeed_EdgeGuard()
        {
            // Function: 5.1 — Enter Culture Data (Minimum Colony Count Boundary)

            // Arrange
            // Production rule: ColonyCount must be > 0. The smallest accepted value is 1.
            _viewModel.NewCultureName = "Minimum Growth";
            _viewModel.NewCultureSampleType = "Urine";
            _viewModel.NewCultureOrganism = "E. coli";
            _viewModel.NewCultureConditions = "Aerobic";
            _viewModel.NewCultureColonyCount = 1;

            _serviceMock.Setup(s => s.CreateCultureAsync(It.IsAny<Culture>()))
                .ReturnsAsync((Culture c) => { c.CultureId = 1; return c; });

            // Act
            await _viewModel.InvokePrivateAsync("AddCultureAsync");

            // Assert
            _serviceMock.Verify(s => s.CreateCultureAsync(It.Is<Culture>(c => c.ColonyCount == 1)), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم إضافة المزرعة");
        }

        [Fact]
        public async Task EnterCultureData_WithValidCultureFields_ShouldCreateCultureAndClearInputs()
        {
            // Function: 5.1 — Enter Culture Data
            // Arrange
            _viewModel.NewCultureName = "Urine Culture";
            _viewModel.NewCultureSampleType = "Urine";
            _viewModel.NewCultureOrganism = "E. coli";
            _viewModel.NewCultureConditions = "Aerobic";
            _viewModel.NewCultureColonyCount = 120;

            _serviceMock.Setup(s => s.CreateCultureAsync(It.IsAny<Culture>()))
                .ReturnsAsync((Culture culture) =>
                {
                    culture.CultureId = 501;
                    return culture;
                });

            // Act
            _viewModel.AddCultureCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _serviceMock.Verify(s => s.CreateCultureAsync(It.Is<Culture>(culture =>
                culture.Name == "Urine Culture" &&
                culture.SampleType == "Urine" &&
                culture.IsolatedOrganism == "E. coli" &&
                culture.GrowthConditions == "Aerobic" &&
                culture.ColonyCount == 120)), Times.Once);
            _viewModel.Cultures.Should().ContainSingle(c => c.CultureId == 501 && c.Name == "Urine Culture");
            _viewModel.NewCultureName.Should().BeEmpty();
            _viewModel.NewCultureSampleType.Should().BeEmpty();
            _viewModel.NewCultureOrganism.Should().BeEmpty();
            _viewModel.NewCultureConditions.Should().BeEmpty();
            _viewModel.NewCultureColonyCount.Should().Be(1);
            _viewModel.StatusMessage.Should().Contain("تم إضافة المزرعة");
        }

        [Fact]
        public async Task EnterCultureData_WhenCreateCultureThrows_ShouldSetErrorStatusMessage()
        {
            // Function: 5.1 — Enter Culture Data
            // Arrange
            _viewModel.NewCultureName = "Blood Culture";
            _viewModel.NewCultureSampleType = "Blood";
            _viewModel.NewCultureOrganism = "Staph";
            _viewModel.NewCultureConditions = "Aerobic";
            _viewModel.NewCultureColonyCount = 75;
            _serviceMock.Setup(s => s.CreateCultureAsync(It.IsAny<Culture>()))
                .ThrowsAsync(new InvalidOperationException("culture-create-failed"));

            // Act
            _viewModel.AddCultureCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("culture-create-failed");
            _viewModel.Cultures.Should().BeEmpty();
        }

        #endregion

        #region Function 5.2 ViewModel Tests

        [Fact]
        public async Task AddAntibioticAsync_With_Empty_Name_Should_Set_Error_FailureGuard()
        {
            // Function: 5.2 — Add Antibiotics (Empty Name Validation)

            // Arrange
            _viewModel.NewAntibioticName = "";
            _viewModel.NewAntibioticSafeForPregnancy = true;
            _viewModel.NewAntibioticSafeForChildren = true;

            // Act
            await _viewModel.InvokePrivateAsync("AddAntibioticAsync");

            // Assert
            // Production validation message for empty antibiotic name (no "خطأ:" prefix on validation guards).
            _viewModel.StatusMessage.Should().Contain("أدخل اسم المضاد الحيوي");
            _serviceMock.Verify(s => s.CreateAntibioticAsync(It.IsAny<Antibiotic>()), Times.Never);
        }

        [Fact]
        public async Task AddAntibioticAsync_When_Service_Throws_Should_Set_Error_FailureGuard()
        {
            // Function: 5.2 — Add Antibiotics (Service Exception Handling)

            // Arrange
            _viewModel.NewAntibioticName = "NewAbx";
            _viewModel.NewAntibioticSafeForPregnancy = true;
            _viewModel.NewAntibioticSafeForChildren = true;

            _serviceMock.Setup(s => s.CreateAntibioticAsync(It.IsAny<Antibiotic>()))
                .ThrowsAsync(new Exception("database-error"));

            // Act
            await _viewModel.InvokePrivateAsync("AddAntibioticAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("database-error");
        }

        [Fact]
        public async Task AddAntibiotics_WithValidAntibiotic_ShouldCreateAntibioticAndClearInput()
        {
            // Function: 5.2 — Add Antibiotics
            // Arrange
            _viewModel.NewAntibioticName = "Ceftriaxone";
            _viewModel.NewAntibioticSafeForPregnancy = false;
            _viewModel.NewAntibioticSafeForChildren = true;

            _serviceMock.Setup(s => s.CreateAntibioticAsync(It.IsAny<Antibiotic>()))
                .ReturnsAsync((Antibiotic antibiotic) =>
                {
                    antibiotic.AntibioticId = 702;
                    return antibiotic;
                });

            // Act
            _viewModel.AddAntibioticCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _serviceMock.Verify(s => s.CreateAntibioticAsync(It.Is<Antibiotic>(antibiotic =>
                antibiotic.Name == "Ceftriaxone" &&
                !antibiotic.IsSafeForPregnancy &&
                antibiotic.IsSafeForChildren)), Times.Once);
            _viewModel.Antibiotics.Should().ContainSingle(a => a.AntibioticId == 702 && a.Name == "Ceftriaxone");
            _viewModel.NewAntibioticName.Should().BeEmpty();
            _viewModel.NewAntibioticSafeForPregnancy.Should().BeTrue();
            _viewModel.NewAntibioticSafeForChildren.Should().BeTrue();
            _viewModel.StatusMessage.Should().Contain("تم إضافة المضاد الحيوي");
        }

        #endregion

        #region Function 5.3 & 5.4 ViewModel Tests

        [Fact]
        public async Task SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard()
        {
            // Function: 5.3 — Set Sensitivity (Empty Results Edge Case)

            // Arrange
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

            // Act
            await _viewModel.InvokePrivateAsync("SaveResultAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("لا توجد نتائج");
            _serviceMock.Verify(s => s.SaveCultureResultAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<CultureSensitivityValue>>()), Times.Never);
        }

        [Fact]
        public async Task SetSensitivity_WithSensitivityRows_ShouldSaveCultureResults()
        {
            // Function: 5.3 — Set Sensitivity
            // Arrange
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 110,
                Name = "Urine Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 80
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 210,
                VisitId = 21,
                LabId = "LAB-210",
                PatientName = "Patient"
            };
            _viewModel.ResultRows.Clear();
            _viewModel.ResultRows.Add(new CultureSensitivityRow
            {
                AntibioticId = 1,
                AntibioticName = "Ampicillin",
                Sensitivity = "S",
                Comment = "Sensitive"
            });
            _serviceMock.Setup(s => s.SaveCultureResultAsync(210, 110, It.IsAny<IReadOnlyCollection<CultureSensitivityValue>>()))
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.SaveResultCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _serviceMock.Verify(s => s.SaveCultureResultAsync(210, 110, It.Is<IReadOnlyCollection<CultureSensitivityValue>>(values =>
                values.Count == 1 &&
                values.Any(v => v.AntibioticId == 1 && v.Sensitivity == "S" && v.Comment == "Sensitive"))), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم حفظ نتيجة المزرعة");
        }

        [Fact]
        public async Task SetSensitivity_WhenSaveCultureResultThrows_ShouldSetErrorStatusMessage()
        {
            // Function: 5.3 — Set Sensitivity
            // Arrange
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 111,
                Name = "Blood Culture",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 90
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 211,
                VisitId = 22,
                LabId = "LAB-211",
                PatientName = "Patient"
            };
            _viewModel.ResultRows.Clear();
            _viewModel.ResultRows.Add(new CultureSensitivityRow
            {
                AntibioticId = 2,
                AntibioticName = "Vancomycin",
                Sensitivity = "R",
                Comment = "Resistant"
            });
            _serviceMock.Setup(s => s.SaveCultureResultAsync(211, 111, It.IsAny<IReadOnlyCollection<CultureSensitivityValue>>()))
                .ThrowsAsync(new InvalidOperationException("save-culture-result-failed"));

            // Act
            _viewModel.SaveResultCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("save-culture-result-failed");
        }

        [Fact]
        public async Task ClassifySensitivity_WithValidSIRValues_ShouldClassifyEverySavedValue()
        {
            // Function: 5.4 — Classify Sensitivity
            // Arrange
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 112,
                Name = "Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 60
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 212,
                VisitId = 23,
                LabId = "LAB-212",
                PatientName = "Patient"
            };
            _viewModel.ResultRows.Clear();
            _viewModel.ResultRows.Add(new CultureSensitivityRow { AntibioticId = 1, AntibioticName = "A", Sensitivity = "S" });
            _viewModel.ResultRows.Add(new CultureSensitivityRow { AntibioticId = 2, AntibioticName = "B", Sensitivity = "I" });
            _viewModel.ResultRows.Add(new CultureSensitivityRow { AntibioticId = 3, AntibioticName = "C", Sensitivity = "R" });
            _serviceMock.Setup(s => s.SaveCultureResultAsync(212, 112, It.IsAny<IReadOnlyCollection<CultureSensitivityValue>>()))
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.SaveResultCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _serviceMock.Verify(s => s.ClassifySensitivity("S"), Times.Once);
            _serviceMock.Verify(s => s.ClassifySensitivity("I"), Times.Once);
            _serviceMock.Verify(s => s.ClassifySensitivity("R"), Times.Once);
            _serviceMock.Verify(s => s.SaveCultureResultAsync(212, 112, It.Is<IReadOnlyCollection<CultureSensitivityValue>>(values =>
                values.Select(v => v.Sensitivity).OrderBy(v => v).SequenceEqual(new[] { "I", "R", "S" }))), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم حفظ نتيجة المزرعة");
        }

        [Fact]
        public async Task BuildResultRowsAsync_With_Pregnant_Patient_Should_Filter_Antibiotics_SuccessGuard()
        {
            // Function: 5.5 — Filter Pregnancy Antibiotics (BR-MED-006)

            // Arrange
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
                PatientName = "Pregnant Patient"
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

            // Act
            await _viewModel.InvokePrivateAsync("BuildResultRowsAsync");

            // Assert
            _viewModel.ResultRows.Should().HaveCount(1);
            _viewModel.ResultRows[0].AntibioticName.Should().Be("SafePreg");
        }

        [Fact]
        public async Task FilterPregnancyAntibiotics_WithoutVisitTest_ShouldDisplayAllLinkedAntibiotics()
        {
            // Function: 5.5 — Filter Pregnancy Antibiotics
            // Arrange
            _viewModel.SelectedVisitTest = null;
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 32,
                Name = "Urine Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 150
            };

            var antibiotics = new List<Antibiotic>
            {
                new() { AntibioticId = 1, Name = "SafePreg", IsSafeForPregnancy = true },
                new() { AntibioticId = 2, Name = "UnsafePreg", IsSafeForPregnancy = false }
            };
            _serviceMock.Setup(s => s.GetCultureAntibioticsAsync(32)).ReturnsAsync(
                antibiotics.Select(a => new CultureAntibiotic { CultureId = 32, AntibioticId = a.AntibioticId, Antibiotic = a }).ToList());

            // Act
            await _viewModel.InvokePrivateAsync("BuildResultRowsAsync");

            // Assert
            _viewModel.ResultRows.Should().HaveCount(2);
            _viewModel.ResultRows.Select(r => r.AntibioticName).Should().Contain(new[] { "SafePreg", "UnsafePreg" });
            _serviceMock.Verify(s => s.GetFilteredAntibioticsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task BuildResultRowsAsync_With_Child_Patient_Should_Filter_Antibiotics_SuccessGuard()
        {
            // Function: 5.6 — Filter Children Antibiotics (BR-MED-007)

            // Arrange
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
                PatientName = "Child Patient"
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

            // Act
            await _viewModel.InvokePrivateAsync("BuildResultRowsAsync");

            // Assert
            _viewModel.ResultRows.Should().HaveCount(1);
            _viewModel.ResultRows[0].AntibioticName.Should().Be("SafeChild");
        }

        [Fact]
        public async Task FilterChildrenAntibiotics_WithAdultPatient_ShouldKeepAllAllowedAntibiotics()
        {
            // Function: 5.6 — Filter Children Antibiotics
            // Arrange
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 33,
                Name = "Blood Culture",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 110
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 43,
                VisitId = 12,
                LabId = "L-ADULT",
                PatientName = "Adult Patient"
            };

            var antibiotics = new List<Antibiotic>
            {
                new() { AntibioticId = 1, Name = "AdultSafe", IsSafeForChildren = true },
                new() { AntibioticId = 2, Name = "AdultOnly", IsSafeForChildren = false }
            };
            _serviceMock.Setup(s => s.GetCultureAntibioticsAsync(33)).ReturnsAsync(
                antibiotics.Select(a => new CultureAntibiotic { CultureId = 33, AntibioticId = a.AntibioticId, Antibiotic = a }).ToList());
            _serviceMock.Setup(s => s.GetFilteredAntibioticsAsync(43)).ReturnsAsync(antibiotics);

            // Act
            await _viewModel.InvokePrivateAsync("BuildResultRowsAsync");

            // Assert
            _viewModel.ResultRows.Should().HaveCount(2);
            _viewModel.ResultRows.Select(r => r.AntibioticName).Should().Contain(new[] { "AdultSafe", "AdultOnly" });
        }

        #endregion

        #region Function 5.7 ViewModel Tests

        [Fact]
        public async Task PrintCultureReportAsync_With_Null_Culture_Should_Set_Error_FailureGuard()
        {
            // Function: 5.7 — Print Culture Report (Missing Culture Edge Case)

            // Arrange
            _viewModel.SelectedCulture = null;
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 50,
                VisitId = 15,
                LabId = "L-PRINT",
                PatientName = "Patient"
            };

            // Act
            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("بيانات الطباعة غير مكتملة");
            _printServiceMock.Verify(p => p.PrintCultureReportAsync(It.IsAny<CultureReportData>()), Times.Never);
        }

        [Fact]
        public async Task PrintCultureReportAsync_With_Null_VisitTest_Should_Set_Error_FailureGuard()
        {
            // Function: 5.7 — Print Culture Report (Missing Visit Edge Case)

            // Arrange
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

            // Act
            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("بيانات الطباعة غير مكتملة");
        }

        [Fact]
        public async Task PrintCultureReportAsync_With_Empty_ResultRows_Should_Print_Anyway_SuccessGuard()
        {
            // Function: 5.7 — Print Culture Report (Empty Results - Valid Scenario)

            // Arrange
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

            // Act
            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            // Assert
            _printServiceMock.Verify(p => p.PrintCultureReportAsync(It.IsAny<CultureReportData>()), Times.Once);
            printedData.Should().NotBeNull();
            printedData!.CultureName.Should().Be("No Growth Culture");
        }

        [Fact]
        public async Task PrintCultureReportAsync_When_PrintService_Throws_Should_Set_Error_FailureGuard()
        {
            // Function: 5.7 — Print Culture Report (Print Service Exception)

            // Arrange
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

            // Act
            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("Printer offline");
        }

        [Fact]
        public async Task PrintCultureReport_WithSensitivityRows_ShouldMapCultureReportData()
        {
            // Function: 5.7 — Print Culture Report
            // Arrange
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 91,
                Name = "Urine Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 150
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 101,
                VisitId = 26,
                LabId = "L-PRINT4",
                PatientName = "Culture Patient",
                VisitDate = new DateTime(2026, 5, 7)
            };
            _viewModel.ResultRows.Clear();
            _viewModel.ResultRows.Add(new CultureSensitivityRow
            {
                AntibioticId = 1,
                AntibioticName = "Ciprofloxacin",
                Sensitivity = "S",
                Comment = "Use normally"
            });
            _viewModel.ResultRows.Add(new CultureSensitivityRow
            {
                AntibioticId = 2,
                AntibioticName = "Amoxicillin",
                Sensitivity = "R",
                Comment = "Avoid"
            });

            CultureReportData? printedData = null;
            _printServiceMock.Setup(p => p.PrintCultureReportAsync(It.IsAny<CultureReportData>()))
                .Callback<CultureReportData>(data => printedData = data)
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.PrintCultureReportCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            printedData.Should().NotBeNull();
            printedData!.VisitId.Should().Be(26);
            printedData.PatientName.Should().Be("Culture Patient");
            printedData.LabId.Should().Be("L-PRINT4");
            printedData.CultureName.Should().Be("Urine Culture");
            printedData.Results.Should().HaveCount(2);
            printedData.Results.Should().Contain(r => r.AntibioticName == "Ciprofloxacin" && r.Sensitivity == "S" && r.Comment == "Use normally");
            printedData.Results.Should().Contain(r => r.AntibioticName == "Amoxicillin" && r.Sensitivity == "R" && r.Comment == "Avoid");
            _viewModel.StatusMessage.Should().Contain("تم إرسال تقرير المزرعة للطباعة");
        }

        #endregion

        #region Search Functionality Tests

        [Fact]
        public async Task SearchCommand_With_Valid_LabId_Should_Load_Results_SuccessGuard()
        {
            // Function: 5.1 — Enter Culture Data (Search by LabId)

            // Arrange
            _viewModel.LabIdFilter = "LAB123";

            var searchResults = new List<CultureVisitTestRow>
            {
                new() { VisitTestId = 1, LabId = "LAB123", PatientName = "Test Patient" }
            };
            _serviceMock.Setup(s => s.SearchCultureVisitTestsAsync("LAB123", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(searchResults);

            // The CultureSensitivityViewModel exposes the LabId-based search through LoadVisitTestsAsync (bound to LoadVisitTestsCommand).
            // Act
            await _viewModel.InvokePrivateAsync("LoadVisitTestsAsync");

            // Assert
            _serviceMock.Verify(s => s.SearchCultureVisitTestsAsync("LAB123", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            _viewModel.VisitTests.Should().ContainSingle(r => r.LabId == "LAB123");
        }

        [Fact]
        public async Task SearchCommand_With_NonExistent_LabId_Should_Show_NotFound_EdgeGuard()
        {
            // Function: 5.1 — Enter Culture Data (Search No Results)

            // Arrange
            _viewModel.LabIdFilter = "NONEXISTENT";

            _serviceMock.Setup(s => s.SearchCultureVisitTestsAsync("NONEXISTENT", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<CultureVisitTestRow>());

            // The CultureSensitivityViewModel exposes the LabId-based search through LoadVisitTestsAsync (bound to LoadVisitTestsCommand).
            // Act
            await _viewModel.InvokePrivateAsync("LoadVisitTestsAsync");

            // Assert
            // Production reports the empty result-set with the load-count message (zero requests found).
            _viewModel.StatusMessage.Should().Contain("تم تحميل 0");
            _viewModel.VisitTests.Should().BeEmpty();
        }

        #endregion
    }
}
