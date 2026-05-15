using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using System.Collections.ObjectModel;

namespace Open_lab.Tests.ViewModels
{
    public class PatientSearchViewModelTests : IDisposable
    {
        private readonly Mock<IPatientSearchService> _patientSearchServiceMock;
        private readonly PatientSearchViewModel _viewModel;

        public PatientSearchViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _patientSearchServiceMock = new Mock<IPatientSearchService>();
            _viewModel = new PatientSearchViewModel(_patientSearchServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task SearchCommand_Should_Search_By_All_Criteria_And_Populate_Results()
        {
            // Function: 1.5 — Search Patient
            // Arrange - 1.5 Search by Name, Phone, LabId
            _viewModel.Name = "Ahmed";
            _viewModel.Phone = "123";
            _viewModel.LabId = "LAB-001";
            _viewModel.Date = new DateTime(2026, 4, 1);

            var patients = new List<Patient>
            {
                new Patient { PatientId = 1, FullName = "Ahmed test", LabId = "LAB-001" }
            };

            _patientSearchServiceMock.Setup(service => service.SearchPatientsAsync("Ahmed", "123", "LAB-001", It.IsAny<DateTime?>()))
                .ReturnsAsync(patients);

            // Act
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100); // Action is async

            // Assert
            _viewModel.Patients.Should().HaveCount(1);
            _viewModel.Patients[0].FullName.Should().Be("Ahmed test");
            _viewModel.StatusMessage.Should().Contain("1");
        }

        [Fact]
        public async Task SearchCommand_By_Date_Should_Filter_Results()
        {
            // Function: 1.5 — Search Patient
            // Arrange - 1.5 Search by Date
            var searchDate = new DateTime(2026, 4, 23);
            _viewModel.Date = searchDate;
            
            var patients = new List<Patient>
            {
                new Patient { PatientId = 5, FullName = "Today Patient", LabId = "LAB-TODAY" }
            };

            _patientSearchServiceMock.Setup(service => service.SearchPatientsAsync(string.Empty, string.Empty, string.Empty, searchDate))
                .ReturnsAsync(patients);

            // Act
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Patients.Should().HaveCount(1);
            _viewModel.StatusMessage.Should().Contain("1");
        }

        [Fact]
        public async Task SearchCommand_With_AdvancedFilters_Should_Call_Criteria_Overload()
        {
            // Function: 1.5 — Advanced Search Patient filters
            // Arrange
            _viewModel.Name = "Ali";
            _viewModel.NationalId = "298";
            _viewModel.DateFrom = new DateTime(2026, 5, 1);
            _viewModel.DateTo = new DateTime(2026, 5, 31);
            _viewModel.AgeGroup = "بالغين";

            _patientSearchServiceMock
                .Setup(service => service.SearchPatientsAsync(It.Is<PatientSearchCriteria>(criteria =>
                    criteria.Name == "Ali" &&
                    criteria.NationalId == "298" &&
                    criteria.DateFrom == new DateTime(2026, 5, 1) &&
                    criteria.DateTo == new DateTime(2026, 5, 31) &&
                    criteria.AgeGroup == "بالغين")))
                .ReturnsAsync(new List<Patient>
                {
                    new Patient { PatientId = 8, FullName = "Ali Advanced", LabId = "LAB-ADV" }
                });

            // Act
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Patients.Should().ContainSingle();
            _viewModel.Patients[0].LabId.Should().Be("LAB-ADV");
        }

        [Fact]
        public async Task SelectedPatient_Setter_Should_Load_Visits()
        {
            // Function: 1.6 — View Patient History
            // Arrange
            var patient = new Patient { PatientId = 42, FullName = "John" };
            var visits = new List<Visit>
            {
                new Visit { VisitId = 101, PatientId = 42, VisitDate = DateTime.Now }
            };

            _patientSearchServiceMock.Setup(service => service.GetPatientVisitsAsync(42))
                .ReturnsAsync(visits);

            // Act
            _viewModel.SelectedPatient = patient;
            await Task.Delay(100);

            // Assert
            _viewModel.Visits.Should().HaveCount(1);
            _viewModel.Visits[0].VisitId.Should().Be(101);
        }

        [Fact]
        public async Task SearchAsync_Should_Handle_Exceptions_Gracefully()
        {
            // Function: 1.5 — Search Patient
            // Arrange
            _patientSearchServiceMock.Setup(service => service.SearchPatientsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                .ThrowsAsync(new Exception("Search Failed"));

            // Act
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ: Search Failed");
            _viewModel.Patients.Should().BeEmpty();
        }

        [Fact]
        public async Task SelectedPatient_When_VisitServiceThrows_Should_Set_ErrorMessage_FailureGuard()
        {
            // Function: 1.6 — View Patient History
            // Arrange
            var patient = new Patient { PatientId = 99, FullName = "Err Patient" };
            _patientSearchServiceMock
                .Setup(service => service.GetPatientVisitsAsync(99))
                .ThrowsAsync(new Exception("visits-failed"));

            // Act
            _viewModel.SelectedPatient = patient;
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("visits-failed");
        }

        [Fact]
        public async Task SelectedPatient_When_SetToNull_Should_Clear_Visits_EdgeGuard()
        {
            // Function: 1.6 — View Patient History
            // Arrange
            var patient = new Patient { PatientId = 42, FullName = "John" };
            _patientSearchServiceMock.Setup(service => service.GetPatientVisitsAsync(42))
                .ReturnsAsync(new List<Visit> { new Visit { VisitId = 1, PatientId = 42, VisitDate = DateTime.Now } });

            _viewModel.SelectedPatient = patient;
            await Task.Delay(100);
            _viewModel.Visits.Should().HaveCount(1);

            // Act
            _viewModel.SelectedPatient = null;
            await Task.Delay(100);

            // Assert
            _viewModel.Visits.Should().BeEmpty();
        }
    }
}
