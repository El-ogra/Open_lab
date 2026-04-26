using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class CultureSensitivityViewModelTests : IDisposable
    {
        private readonly Mock<ICultureSensitivityService> _serviceMock;
        private readonly Mock<IPrintService> _printServiceMock;
        private readonly CultureSensitivityViewModel _viewModel;

        public CultureSensitivityViewModelTests()
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

        [Fact]
        public async Task AddCultureAsync_Should_Send_Full_Metadata_To_Service()
        {
            Culture? captured = null;
            _serviceMock.Setup(s => s.CreateCultureAsync(It.IsAny<Culture>()))
                .Callback<Culture>(c => captured = c)
                .ReturnsAsync(new Culture
                {
                    CultureId = 1,
                    Name = "Urine Culture",
                    SampleType = "Urine",
                    IsolatedOrganism = "E. coli",
                    GrowthConditions = "Aerobic",
                    ColonyCount = 150
                });

            _viewModel.NewCultureName = "Urine Culture";
            _viewModel.NewCultureSampleType = "Urine";
            _viewModel.NewCultureOrganism = "E. coli";
            _viewModel.NewCultureConditions = "Aerobic";
            _viewModel.NewCultureColonyCount = 150;

            await _viewModel.InvokePrivateAsync("AddCultureAsync");

            captured.Should().NotBeNull();
            captured!.SampleType.Should().Be("Urine");
            captured.IsolatedOrganism.Should().Be("E. coli");
            captured.GrowthConditions.Should().Be("Aerobic");
            captured.ColonyCount.Should().Be(150);
        }

        [Fact]
        public async Task AddAntibioticAsync_Should_Send_Safety_Classification_To_Service()
        {
            Antibiotic? captured = null;
            _serviceMock.Setup(s => s.CreateAntibioticAsync(It.IsAny<Antibiotic>()))
                .Callback<Antibiotic>(a => captured = a)
                .ReturnsAsync(new Antibiotic
                {
                    AntibioticId = 5,
                    Name = "Amoxicillin",
                    IsSafeForPregnancy = false,
                    IsSafeForChildren = true
                });

            _viewModel.NewAntibioticName = "Amoxicillin";
            _viewModel.NewAntibioticSafeForPregnancy = false;
            _viewModel.NewAntibioticSafeForChildren = true;

            await _viewModel.InvokePrivateAsync("AddAntibioticAsync");

            captured.Should().NotBeNull();
            captured!.IsSafeForPregnancy.Should().BeFalse();
            captured.IsSafeForChildren.Should().BeTrue();
        }

        [Fact]
        public async Task BuildResultRowsAsync_When_Visit_Selected_Should_Apply_Filtering()
        {
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 10,
                Name = "Blood Culture",
                SampleType = "Blood",
                IsolatedOrganism = "Staph",
                GrowthConditions = "Aerobic",
                ColonyCount = 40
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 99,
                VisitId = 77,
                LabId = "L-1",
                PatientName = "P"
            };

            _serviceMock.Setup(s => s.GetCultureAntibioticsAsync(10)).ReturnsAsync(new List<CultureAntibiotic>
            {
                new CultureAntibiotic { CultureId = 10, AntibioticId = 1, Antibiotic = new Antibiotic { AntibioticId = 1, Name = "A-Allowed" } },
                new CultureAntibiotic { CultureId = 10, AntibioticId = 2, Antibiotic = new Antibiotic { AntibioticId = 2, Name = "B-Blocked" } }
            });
            _serviceMock.Setup(s => s.GetFilteredAntibioticsAsync(99)).ReturnsAsync(new List<Antibiotic>
            {
                new Antibiotic { AntibioticId = 1, Name = "A-Allowed" }
            });

            await _viewModel.InvokePrivateAsync("BuildResultRowsAsync");

            _viewModel.ResultRows.Should().ContainSingle();
            _viewModel.ResultRows[0].AntibioticId.Should().Be(1);
        }

        [Fact]
        public async Task SaveResultAsync_Should_Save_Classified_Values_Through_Service()
        {
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 3,
                Name = "Urine Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 80
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 11,
                VisitId = 4,
                LabId = "LAB-77",
                PatientName = "Patient"
            };
            _viewModel.ResultRows.Add(new CultureSensitivityRow
            {
                AntibioticId = 15,
                AntibioticName = "Ampicillin",
                Sensitivity = "Sensitive",
                Comment = "note"
            });

            _serviceMock.Setup(s => s.ClassifySensitivity("Sensitive")).Returns("S");

            IReadOnlyCollection<CultureSensitivityValue>? savedValues = null;
            _serviceMock.Setup(s => s.SaveCultureResultAsync(11, 3, It.IsAny<IReadOnlyCollection<CultureSensitivityValue>>()))
                .Callback<int, int, IReadOnlyCollection<CultureSensitivityValue>>((_, _, values) => savedValues = values)
                .Returns(Task.CompletedTask);

            await _viewModel.InvokePrivateAsync("SaveResultAsync");

            savedValues.Should().NotBeNull();
            savedValues!.Should().ContainSingle();
            savedValues!.First().Sensitivity.Should().Be("S");
        }

        [Fact]
        public async Task PrintCultureReportAsync_Should_Call_Print_Service_With_Results()
        {
            _viewModel.SelectedCulture = new Culture
            {
                CultureId = 4,
                Name = "Urine Culture",
                SampleType = "Urine",
                IsolatedOrganism = "E. coli",
                GrowthConditions = "Aerobic",
                ColonyCount = 60
            };
            _viewModel.SelectedVisitTest = new CultureVisitTestRow
            {
                VisitTestId = 12,
                VisitId = 5,
                LabId = "LAB-9",
                PatientName = "Patient 9",
                VisitDate = new DateTime(2026, 4, 1)
            };
            _viewModel.ResultRows.Add(new CultureSensitivityRow
            {
                AntibioticId = 21,
                AntibioticName = "Ceftriaxone",
                Sensitivity = "R",
                Comment = "high resistance"
            });

            _serviceMock.Setup(s => s.ClassifySensitivity("R")).Returns("R");

            CultureReportData? printed = null;
            _printServiceMock.Setup(p => p.PrintCultureReportAsync(It.IsAny<CultureReportData>()))
                .Callback<CultureReportData>(data => printed = data)
                .Returns(Task.CompletedTask);

            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            printed.Should().NotBeNull();
            printed!.CultureName.Should().Be("Urine Culture");
            printed.Results.Should().ContainSingle();
            printed.Results[0].Sensitivity.Should().Be("R");
        }

        [Fact]
        public async Task SaveResultAsync_When_SelectionMissing_Should_Set_ValidationMessage_FailureGuard()
        {
            // Arrange
            _viewModel.SelectedVisitTest = null;
            _viewModel.SelectedCulture = null;

            // Act
            await _viewModel.InvokePrivateAsync("SaveResultAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("اختر الزيارة والمزرعة");
            _serviceMock.Verify(s => s.SaveCultureResultAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<CultureSensitivityValue>>()), Times.Never);
        }

        [Fact]
        public async Task PrintCultureReportAsync_When_DataIncomplete_Should_Set_Message_EdgeGuard()
        {
            // Arrange
            _viewModel.SelectedVisitTest = null;
            _viewModel.SelectedCulture = null;

            // Act
            await _viewModel.InvokePrivateAsync("PrintCultureReportAsync");

            // Assert
            _viewModel.StatusMessage.Should().Be("بيانات الطباعة غير مكتملة.");
            _printServiceMock.Verify(p => p.PrintCultureReportAsync(It.IsAny<CultureReportData>()), Times.Never);
        }
    }
}
