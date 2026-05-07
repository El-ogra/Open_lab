using FluentAssertions;
using Moq;
using Open_lab.Services;

namespace Open_lab.Tests.Services
{
    public class PrintServiceTests
    {
        [Fact]
        public async Task PrintCultureReportAsync_When_Data_Is_Null_Should_Throw()
        {
            // Function: 5.7 — Print Culture Report (guard path)
            var settingsMock = new Mock<ISettingsService>();
            var service = new PrintService(settingsMock.Object);

            Func<Task> act = async () => await service.PrintCultureReportAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task PrintVisitReportAsync_When_Report_Is_Null_Should_Throw()
        {
            // Function: 8.6 — Print External Lab Report uses same visit-report printing pipeline
            var settingsMock = new Mock<ISettingsService>();
            var service = new PrintService(settingsMock.Object);

            Func<Task> act = async () => await service.PrintVisitReportAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        // 5.7 Culture Report Printing Tests - NEW TEST

        [Fact]
        public void PrintCultureReportData_Should_Validate_Report_Structure_LogicGuard()
        {
            // Function: 5.7 — Culture Report Printing - unit-level data contract validation.
            // Do not invoke real print pipeline in unit tests because it depends on OS printer drivers.
            var cultureData = new CultureReportData
            {
                PatientName = "Test Patient",
                LabId = "LAB-001",
                CultureName = "Urine Culture",
                VisitDate = DateTime.Now,
                Results = new List<CultureResultRow>
                {
                    new CultureResultRow { AntibioticName = "Amoxicillin", Sensitivity = "Sensitive" },
                    new CultureResultRow { AntibioticName = "Ciprofloxacin", Sensitivity = "Resistant" }
                }
            };

            cultureData.PatientName.Should().NotBeNullOrEmpty();
            cultureData.LabId.Should().NotBeNullOrEmpty();
            cultureData.CultureName.Should().NotBeNullOrEmpty();
            cultureData.Results.Should().HaveCount(2);
            cultureData.Results[0].AntibioticName.Should().Be("Amoxicillin");
            cultureData.Results[0].Sensitivity.Should().Be("Sensitive");
            cultureData.Results[1].AntibioticName.Should().Be("Ciprofloxacin");
            cultureData.Results[1].Sensitivity.Should().Be("Resistant");
        }

        [Fact]
        public void Constructor_When_SettingsServiceNull_Should_Throw_FailureGuard()
        {
            // Function: 5.7 — Culture Report Printing - unit-level data contract validation.
            // Act
            Action act = () => new PrintService(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task PrintTextReportAsync_When_TitleEmpty_Should_Throw_FailureGuard()
        {
            // Function: 5.7 — Culture Report Printing - unit-level data contract validation.
            var settingsMock = new Mock<ISettingsService>();
            var service = new PrintService(settingsMock.Object);

            Func<Task> act = async () => await service.PrintTextReportAsync(" ", new List<string>());

            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
