using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class PatientRegistrationViewModelTests : IDisposable
    {
        private readonly Mock<IPatientService> _patientServiceMock;
        private readonly PatientRegistrationViewModel _viewModel;

        public PatientRegistrationViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _patientServiceMock = new Mock<IPatientService>();
            _viewModel = new PatientRegistrationViewModel(_patientServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public void Constructor_Should_Generate_LabId_Automatically()
        {
            // Arrange
            _patientServiceMock.Setup(x => x.GenerateNextLabIdAsync(It.IsAny<DateTime?>())).ReturnsAsync("LAB-2024-001");
            var vm = new PatientRegistrationViewModel(_patientServiceMock.Object);

            // Assert - eventually LabId should be set via constructor's fire-and-forget
            // Since constructor calls _ = GenerateLabIdAsync() we can't reliably await it,
            // so we verify the mock was called
            _patientServiceMock.Verify(x => x.GenerateNextLabIdAsync(It.IsAny<DateTime?>()), Times.AtLeastOnce);
        }

        [Fact]
        public void Commands_When_Admin_Should_All_Be_Enabled()
        {
            // Assert
            _viewModel.SaveCommand.CanExecute(null).Should().BeTrue();
            _viewModel.NewCommand.CanExecute(null).Should().BeTrue();
            _viewModel.GenerateLabIdCommand.CanExecute(null).Should().BeTrue();
            _viewModel.LoadByLabIdCommand.CanExecute(null).Should().BeTrue();
            _viewModel.SearchCommand.CanExecute(null).Should().BeTrue();
            _viewModel.DeleteCommand.CanExecute(null).Should().BeFalse(); // PatientId == 0
        }

        [Fact]
        public async Task SaveAsync_With_New_Patient_Should_Create_New_Patient()
        {
            // Arrange - PatientId defaults to 0 for new patient
            _viewModel.LabId = "LAB-001";
            _viewModel.FullName = "John Doe";
            _viewModel.Gender = "Male";
            _viewModel.Phone = "1234567890";
            _viewModel.Address = "123 Street";
            _viewModel.ChronicDiseases = "Diabetes";
            _viewModel.Allergies = "Penicillin";
            _viewModel.Medications = "Insulin";
            _viewModel.MedicalNotes = "Note";

            var createdPatient = new Patient { PatientId = 42, LabId = "LAB-001" };
            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>())).ReturnsAsync(createdPatient);
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(42, It.IsAny<MedicalHistory>())).Returns(Task.CompletedTask);

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("تم إنشاء المريض بنجاح");
            _patientServiceMock.Verify(x => x.CreateAsync(It.Is<Patient>(p =>
                p.LabId == "LAB-001" &&
                p.FullName == "John Doe" &&
                p.Gender == "Male")), Times.Once);
        }


        [Fact]
        public async Task SaveAsync_When_Service_Throws_Should_Set_Error_StatusMessage()
        {
            // Arrange
            _viewModel.LabId = "LAB-001";
            _viewModel.FullName = "Test";
            _viewModel.Gender = "Male";
            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>())).ThrowsAsync(new Exception("DB Error"));

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("DB Error");
        }

        [Fact]
        public async Task LoadByLabIdAsync_With_Empty_LabId_Should_Set_StatusMessage()
        {
            // Arrange
            _viewModel.LabId = "";

            // Act
            await _viewModel.InvokePrivateAsync("LoadByLabIdAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("يرجى إدخال Lab ID");
        }

        [Fact]
        public async Task LoadByLabIdAsync_With_NonExistent_LabId_Should_Set_StatusMessage()
        {
            // Arrange
            _viewModel.LabId = "LAB-999";
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("LAB-999")).ReturnsAsync((Patient?)null);

            // Act
            await _viewModel.InvokePrivateAsync("LoadByLabIdAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("لم يتم العثور");
        }

        [Fact]
        public async Task LoadByLabIdAsync_With_Existing_LabId_Should_Load_Patient_Data()
        {
            // Arrange
            _viewModel.LabId = "LAB-001";
            var patient = new Patient
            {
                PatientId = 10,
                LabId = "LAB-001",
                FullName = "Alice",
                Gender = "Female",
                BirthDate = new DateTime(1990, 1, 1),
                Phone = "555",
                Address = "Address"
            };
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("LAB-001")).ReturnsAsync(patient);
            _patientServiceMock.Setup(x => x.GetMedicalHistoryAsync(10)).ReturnsAsync((MedicalHistory?)null);

            // Act
            await _viewModel.InvokePrivateAsync("LoadByLabIdAsync");

            // Assert
            _viewModel.PatientId.Should().Be(10);
            _viewModel.FullName.Should().Be("Alice");
            _viewModel.Gender.Should().Be("Female");
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task SearchAsync_Should_Populate_Results()
        {
            // Arrange
            _viewModel.FullName = "John";
            _viewModel.Phone = "123";
            var patients = new List<Patient>
            {
                new Patient { PatientId = 1, FullName = "John Doe" },
                new Patient { PatientId = 2, FullName = "John Smith" }
            };
            _patientServiceMock.Setup(x => x.SearchAsync("John", "123")).ReturnsAsync(patients);

            // Act
            await _viewModel.InvokePrivateAsync("SearchAsync");

            // Assert
            _viewModel.Results.Should().HaveCount(2);
            _viewModel.StatusMessage.Should().Contain("2");
        }

        [Fact]
        public async Task DeleteAsync_With_PatientId_Zero_Should_Return_Immediately()
        {
            // Arrange - PatientId defaults to 0

            // Act
            await _viewModel.InvokePrivateAsync("DeleteAsync");

            // Assert
            _patientServiceMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }



        [Fact]
        public void SelectedPatient_Setter_Should_Load_Patient_Data()
        {
            // Arrange
            var patient = new Patient
            {
                PatientId = 7,
                LabId = "LAB-007",
                FullName = "James",
                Gender = "Male"
            };
            _patientServiceMock.Setup(x => x.GetMedicalHistoryAsync(7)).ReturnsAsync((MedicalHistory?)null);

            // Act
            _viewModel.SelectedPatient = patient;

            // Assert
            _viewModel.PatientId.Should().Be(7);
            _viewModel.LabId.Should().Be("LAB-007");
            _viewModel.FullName.Should().Be("James");
        }
    }
}
