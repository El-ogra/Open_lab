using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class BlankReportViewModelTests
    {
        private readonly Mock<IReportService> _reportServiceMock;
        private readonly BlankReportViewModel _viewModel;

        public BlankReportViewModelTests()
        {
            _reportServiceMock = new Mock<IReportService>();
            _viewModel = new BlankReportViewModel(_reportServiceMock.Object);
        }

        [Fact]
        public async Task LoadAsync_With_Valid_Visit_Should_Populate_Blank_Report_Data_LogicGuard()
        {
            // Function: 4.8 — Print Blank Report - Logic Guard: Verify all blank report fields are populated
            // Arrange
            // Act
            var patient = new Patient
            {
                PatientId = 1,
                FullName = "Patient A",
                LabId = "LAB-1",
                Gender = "Female",
                Phone = "0100"
            };
            var visit = new Visit
            {
                VisitId = 7,
                PatientId = 1,
                VisitDate = DateTime.Today,
                Referral = new Referral { Name = "Referral X", ReferralId = 5 }
            };
            var report = new VisitReportData { Patient = patient, Visit = visit };
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(7)).ReturnsAsync(report);
            _viewModel.VisitId = 7;

            await _viewModel.InvokePrivateAsync("LoadAsync");

            // Assert - Logic Guard: Verify all patient data is loaded for blank report
            _viewModel.PatientName.Should().Be("Patient A");
            _viewModel.LabId.Should().Be("LAB-1");
            _viewModel.Gender.Should().Be("Female");
            _viewModel.Phone.Should().Be("0100");

            // Assert - Logic Guard: Verify visit and referral data
            _viewModel.VisitDate.Should().NotBeNullOrEmpty();
            _viewModel.ReferralName.Should().Be("Referral X");
            _viewModel.StatusMessage.Should().Contain("تم تحميل بيانات المريض");
        }

        [Fact]
        public async Task LoadAsync_With_InvalidVisitId_Should_Set_Validation_Message_FailureGuard()
        {
            // Function: 4.8 — Print Blank Report - Logic Guard: Verify all blank report fields are populated
            // Arrange
            // Act
            _viewModel.VisitId = 0;

            await _viewModel.InvokePrivateAsync("LoadAsync");

            // Assert
            _viewModel.StatusMessage.Should().Be("يرجى إدخال رقم الزيارة.");
            _reportServiceMock.Verify(x => x.GetVisitReportAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task LoadAsync_When_ReportData_NotFound_Should_Set_NotFound_Message_FailureGuard()
        {
            // Function: 4.8 — Print Blank Report - Logic Guard: Verify all blank report fields are populated
            // Arrange
            // Act
            _viewModel.VisitId = 42;
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(42)).ReturnsAsync((VisitReportData?)null);

            await _viewModel.InvokePrivateAsync("LoadAsync");

            // Assert
            _viewModel.StatusMessage.Should().Be("لم يتم العثور على بيانات.");
            _viewModel.PatientName.Should().BeEmpty();
        }

        [Fact]
        public async Task PrintBlankAsync_Without_PrintService_Should_Set_ServiceUnavailable_EdgeGuard()
        {
            // Function: 4.8 — Print Blank Report - Logic Guard: Verify all blank report fields are populated
            // Arrange
            // Act
            _viewModel.VisitId = 7;

            await _viewModel.InvokePrivateAsync("PrintBlankAsync");

            // Assert
            _viewModel.StatusMessage.Should().Be("خدمة الطباعة غير متاحة.");
        }
    }
}
