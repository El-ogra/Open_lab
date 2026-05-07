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
            _viewModel.NewCultureName = "";
            _viewModel.NewCultureSampleType = "Urine";
            _viewModel.NewCultureOrganism = "E. coli";
            _viewModel.NewCultureConditions = "Aerobic";
            _viewModel.NewCultureColonyCount = 100;

            await _viewModel.InvokePrivateAsync("AddCultureAsync");

            // Production validation message for empty culture name (no "خطأ:" prefix on validation guards).
            _viewModel.StatusMessage.Should().Contain("أدخل اسم المزرعة");
            _serviceMock.Verify(s => s.CreateCultureAsync(It.IsAny<Culture>()), Times.Never);
        }

        [Fact]
        public async Task AddCultureAsync_With_Minimum_ColonyCount_Should_Succeed_EdgeGuard()
        {
            // Function: 5.1 — Enter Culture Data (Minimum Colony Count Boundary)
            // Production rule: ColonyCount must be > 0. The smallest accepted value is 1.
            _viewModel.NewCultureName = "Minimum Growth";
            _viewModel.NewCultureSampleType = "Urine";
            _viewModel.NewCultureOrganism = "E. coli";
            _viewModel.NewCultureConditions = "Aerobic";
            _viewModel.NewCultureColonyCount = 1;

            _serviceMock.Setup(s => s.CreateCultureAsync(It.IsAny<Culture>()))
                .ReturnsAsync((Culture c) => { c.CultureId = 1; return c; });

            await _viewModel.InvokePrivateAsync("AddCultureAsync");

            _serviceMock.Verify(s => s.CreateCultureAsync(It.Is<Culture>(c => c.ColonyCount == 1)), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم إضافة المزرعة");
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

            // Production validation message for empty antibiotic name (no "خطأ:" prefix on validation guards).
            _viewModel.StatusMessage.Should().Contain("أدخل اسم المضاد الحيوي");
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
            _viewModel.LabIdFilter = "LAB123";

            var searchResults = new List<CultureVisitTestRow>
            {
                new() { VisitTestId = 1, LabId = "LAB123", PatientName = "Test Patient" }
            };
            _serviceMock.Setup(s => s.SearchCultureVisitTestsAsync("LAB123", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(searchResults);

            // The CultureSensitivityViewModel exposes the LabId-based search through LoadVisitTestsAsync (bound to LoadVisitTestsCommand).
            await _viewModel.InvokePrivateAsync("LoadVisitTestsAsync");

            _serviceMock.Verify(s => s.SearchCultureVisitTestsAsync("LAB123", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            _viewModel.VisitTests.Should().ContainSingle(r => r.LabId == "LAB123");
        }

        [Fact]
        public async Task SearchCommand_With_NonExistent_LabId_Should_Show_NotFound_EdgeGuard()
        {
            // Function: 5.1 — Enter Culture Data (Search No Results)
            _viewModel.LabIdFilter = "NONEXISTENT";

            _serviceMock.Setup(s => s.SearchCultureVisitTestsAsync("NONEXISTENT", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<CultureVisitTestRow>());

            // The CultureSensitivityViewModel exposes the LabId-based search through LoadVisitTestsAsync (bound to LoadVisitTestsCommand).
            await _viewModel.InvokePrivateAsync("LoadVisitTestsAsync");

            // Production reports the empty result-set with the load-count message (zero requests found).
            _viewModel.StatusMessage.Should().Contain("تم تحميل 0");
            _viewModel.VisitTests.Should().BeEmpty();
        }

        #endregion
    }
}
