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
        public async Task Constructor_Should_Generate_LabId_Automatically_LogicGuard()
        {
            // Arrange
            var expectedLabId = "LAB-2024-001";
            _patientServiceMock.Setup(x => x.GenerateNextLabIdAsync(It.IsAny<DateTime?>())).ReturnsAsync(expectedLabId);

            // Act
            var vm = new PatientRegistrationViewModel(_patientServiceMock.Object);
            await Task.Delay(100); // Allow fire-and-forget to complete

            // Assert - Logic Guard: Verify LabId is actually set to the generated value
            vm.LabId.Should().Be(expectedLabId);
        }

        [Fact]
        public void Commands_When_Admin_Should_All_Be_Enabled_LogicGuard()
        {
            // Assert - Logic Guard: Verify command states reflect actual business rules
            _viewModel.SaveCommand.CanExecute(null).Should().BeTrue("Save should be enabled for new patient");
            _viewModel.NewCommand.CanExecute(null).Should().BeTrue("New should always be enabled for admin");
            _viewModel.GenerateLabIdCommand.CanExecute(null).Should().BeTrue("GenerateLabId should be enabled for admin");
            _viewModel.LoadByLabIdCommand.CanExecute(null).Should().BeTrue("LoadByLabId should be enabled for admin");
            _viewModel.SearchCommand.CanExecute(null).Should().BeTrue("Search should be enabled for admin");
            _viewModel.DeleteCommand.CanExecute(null).Should().BeFalse("Delete should be disabled when PatientId == 0");

            // Logic Guard: Verify Delete becomes enabled when PatientId is set
            // Note: PatientId is read-only, this test validates the initial state only
        }

        [Fact]
        public async Task SaveAsync_With_New_Patient_Should_Create_New_Patient_LogicGuard()
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

            Patient? capturedPatient = null;
            MedicalHistory? capturedHistory = null;
            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .Callback<Patient>(p => capturedPatient = p)
                .ReturnsAsync((Patient p) => { p.PatientId = 42; return p; });
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(42, It.IsAny<MedicalHistory>()))
                .Callback<int, MedicalHistory>((id, h) => capturedHistory = h)
                .Returns(Task.CompletedTask);

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert - Logic Guard: Verify all patient data is correctly captured and saved
            _viewModel.StatusMessage.Should().Contain("تم إنشاء المريض بنجاح");
            capturedPatient.Should().NotBeNull();
            capturedPatient!.LabId.Should().Be("LAB-001");
            capturedPatient.FullName.Should().Be("John Doe");
            capturedPatient.Gender.Should().Be("Male");
            capturedPatient.Phone.Should().Be("1234567890");
            capturedPatient.Address.Should().Be("123 Street");
            capturedPatient.PatientId.Should().Be(42);

            // Assert - Logic Guard: Verify medical history is correctly captured
            capturedHistory.Should().NotBeNull();
            capturedHistory!.ChronicDiseases.Should().Be("Diabetes");
            capturedHistory.Allergies.Should().Be("Penicillin");
            capturedHistory.Medications.Should().Be("Insulin");
            capturedHistory.Notes.Should().Be("Note");
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
        public async Task SearchAsync_Should_Populate_Results_LogicGuard()
        {
            // Arrange - 1.5 Search Semantics Tests
            _viewModel.FullName = "John";
            _viewModel.Phone = "123";
            var patients = new List<Patient>
            {
                new Patient { PatientId = 1, FullName = "John Doe", LabId = "LAB-001", Phone = "123456" },
                new Patient { PatientId = 2, FullName = "John Smith", LabId = "LAB-002", Phone = "123789" }
            };
            _patientServiceMock.Setup(x => x.SearchAsync("John", "123", It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(patients);

            // Act
            await _viewModel.InvokePrivateAsync("SearchAsync");

            // Assert - Logic Guard: Verify search returns correct patients with full data
            _viewModel.Results.Should().HaveCount(2);
            _viewModel.StatusMessage.Should().Contain("2");
            _viewModel.Results[0].PatientId.Should().Be(1);
            _viewModel.Results[0].FullName.Should().Be("John Doe");
            _viewModel.Results[0].LabId.Should().Be("LAB-001");
            _viewModel.Results[1].PatientId.Should().Be(2);
            _viewModel.Results[1].FullName.Should().Be("John Smith");
            _viewModel.Results[1].LabId.Should().Be("LAB-002");
        }

        [Fact]
        public async Task DeleteAsync_With_PatientId_Zero_Should_Not_Call_Service_LogicGuard()
        {
            // Arrange - PatientId defaults to 0
            var deleteCalled = false;
            _patientServiceMock.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .Callback<int>(id => deleteCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await _viewModel.InvokePrivateAsync("DeleteAsync");

            // Assert - Logic Guard: Verify delete service was NOT called for PatientId 0
            deleteCalled.Should().BeFalse();
            _patientServiceMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }



        [Fact]
        public void SelectedPatient_Setter_Should_Load_Patient_Data_LogicGuard()
        {
            // Arrange
            var patient = new Patient
            {
                PatientId = 7,
                LabId = "LAB-007",
                FullName = "James",
                Gender = "Male",
                Phone = "555-1234",
                Address = "456 Oak St",
                BirthDate = new DateTime(1985, 5, 15)
            };
            var history = new MedicalHistory
            {
                ChronicDiseases = "Hypertension",
                Allergies = "None",
                Medications = "Lisinopril",
                Notes = "Regular checkup"
            };
            _patientServiceMock.Setup(x => x.GetMedicalHistoryAsync(7)).ReturnsAsync(history);

            // Act
            _viewModel.SelectedPatient = patient;
            Thread.Sleep(50); // Allow async medical history load to complete

            // Assert - Logic Guard: Verify all patient fields are correctly loaded
            _viewModel.PatientId.Should().Be(7);
            _viewModel.LabId.Should().Be("LAB-007");
            _viewModel.FullName.Should().Be("James");
            _viewModel.Gender.Should().Be("Male");
            _viewModel.Phone.Should().Be("555-1234");
            _viewModel.Address.Should().Be("456 Oak St");
            _viewModel.BirthDate.Should().Be(new DateTime(1985, 5, 15));

            // Assert - Logic Guard: Verify medical history is loaded
            _viewModel.ChronicDiseases.Should().Be("Hypertension");
            _viewModel.Allergies.Should().Be("None");
            _viewModel.Medications.Should().Be("Lisinopril");
            _viewModel.MedicalNotes.Should().Be("Regular checkup");
        }

        // 1.5 Search Semantics Tests - NEW TESTS

        [Fact]
        public async Task SearchAsync_By_Name_Should_Return_Matching_Patients_LogicGuard()
        {
            // 1.5 Search by name
            // Arrange
            _viewModel.FullName = "Ahmed";
            _viewModel.Phone = "";
            var patients = new List<Patient>
            {
                new Patient { PatientId = 1, FullName = "Ahmed Ali", LabId = "LAB-001", Phone = "111" },
                new Patient { PatientId = 2, FullName = "Ahmed Mohamed", LabId = "LAB-002", Phone = "222" },
                new Patient { PatientId = 3, FullName = "Sara Ahmed", LabId = "LAB-003", Phone = "333" }
            };
            _patientServiceMock.Setup(x => x.SearchAsync("Ahmed", "", It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(patients);

            // Act
            await _viewModel.InvokePrivateAsync("SearchAsync");

            // Assert - Logic Guard: Verify search returns all patients matching name
            _viewModel.Results.Should().HaveCount(3);
            _viewModel.Results.Should().Contain(r => r.FullName == "Ahmed Ali");
            _viewModel.Results.Should().Contain(r => r.FullName == "Ahmed Mohamed");
            _viewModel.Results.Should().Contain(r => r.FullName == "Sara Ahmed");
        }

        [Fact]
        public async Task SearchAsync_By_Phone_Should_Return_Exact_Match_LogicGuard()
        {
            // 1.5 Search by phone number
            // Arrange
            _viewModel.FullName = "";
            _viewModel.Phone = "5551234";
            var patients = new List<Patient>
            {
                new Patient { PatientId = 10, FullName = "John Doe", LabId = "LAB-010", Phone = "5551234" }
            };
            _patientServiceMock.Setup(x => x.SearchAsync("", "5551234", It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(patients);

            // Act
            await _viewModel.InvokePrivateAsync("SearchAsync");

            // Assert - Logic Guard: Verify search returns exact phone match
            _viewModel.Results.Should().ContainSingle();
            _viewModel.Results[0].PatientId.Should().Be(10);
            _viewModel.Results[0].Phone.Should().Be("5551234");
            _viewModel.Results[0].FullName.Should().Be("John Doe");
        }

        [Fact]
        public async Task SearchAsync_With_No_Results_Should_Return_Empty_LogicGuard()
        {
            // 1.5 Break case: no results found
            // Arrange
            _viewModel.FullName = "NonExistent";
            _viewModel.Phone = "9999999";
            _patientServiceMock.Setup(x => x.SearchAsync("NonExistent", "9999999", It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(new List<Patient>());

            // Act
            await _viewModel.InvokePrivateAsync("SearchAsync");

            // Assert - Logic Guard: Verify empty result set and appropriate message
            _viewModel.Results.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("0");
        }

        [Fact]
        public async Task SearchAsync_Partial_Match_Should_Return_Relevant_Results_LogicGuard()
        {
            // 1.5 Partial search
            // Arrange
            _viewModel.FullName = "Moh";
            _viewModel.Phone = "";
            var patients = new List<Patient>
            {
                new Patient { PatientId = 5, FullName = "Mohammed Ali", LabId = "LAB-005", Phone = "555" },
                new Patient { PatientId = 6, FullName = "Sarah Mohamed", LabId = "LAB-006", Phone = "666" }
            };
            _patientServiceMock.Setup(x => x.SearchAsync("Moh", "", It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(patients);

            // Act
            await _viewModel.InvokePrivateAsync("SearchAsync");

            // Assert - Logic Guard: Verify partial matches are returned correctly
            _viewModel.Results.Should().HaveCount(2);
            _viewModel.Results.Should().Contain(r => r.FullName.Contains("Moh"));
        }

        // 1.7 Medical History Integrity Tests - NEW TEST

        [Fact]
        public async Task SaveAsync_With_MedicalHistory_Should_Save_Complete_History_LogicGuard()
        {
            // 1.7 Add medical history with integrity validation
            // Arrange
            _viewModel.LabId = "LAB-HIST";
            _viewModel.FullName = "History Patient";
            _viewModel.Gender = "Female";
            _viewModel.ChronicDiseases = "Diabetes, Hypertension";
            _viewModel.Allergies = "Penicillin, Sulfa";
            _viewModel.Medications = "Metformin, Lisinopril";
            _viewModel.MedicalNotes = "Patient requires regular monitoring";

            Patient? capturedPatient = null;
            MedicalHistory? capturedHistory = null;
            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .Callback<Patient>(p => capturedPatient = p)
                .ReturnsAsync((Patient p) => { p.PatientId = 100; return p; });
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(100, It.IsAny<MedicalHistory>()))
                .Callback<int, MedicalHistory>((id, h) => capturedHistory = h)
                .Returns(Task.CompletedTask);

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert - Logic Guard: Verify medical history is saved with complete data
            capturedHistory.Should().NotBeNull();
            capturedHistory.Should().NotBeNull();
            capturedHistory!.ChronicDiseases.Should().Be("Diabetes, Hypertension");
            capturedHistory.Allergies.Should().Be("Penicillin, Sulfa");
            capturedHistory.Medications.Should().Be("Metformin, Lisinopril");
            capturedHistory.Notes.Should().Be("Patient requires regular monitoring");
            capturedPatient.Should().NotBeNull();
            capturedPatient!.PatientId.Should().Be(100);
        }

        [Fact]
        public async Task SaveAsync_With_Existing_Patient_Should_Update_Patient_LogicGuard()
        {
            // 1.2 Edit patient data
            // Arrange - Set PatientId > 0 to trigger UpdateAsync
            _viewModel.InvokePrivate("set_PatientId", 500);
            _viewModel.LabId = "LAB-500";
            _viewModel.FullName = "Updated Name";
            _viewModel.Gender = "Male";

            Patient? capturedPatient = null;
            _patientServiceMock.Setup(x => x.UpdateAsync(It.IsAny<Patient>()))
                .Callback<Patient>(p => capturedPatient = p)
                .Returns(Task.CompletedTask);
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(500, It.IsAny<MedicalHistory>()))
                .Returns(Task.CompletedTask);

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert - Logic Guard: Verify UpdateAsync was called instead of CreateAsync
            _viewModel.StatusMessage.Should().Contain("تم تحديث بيانات المريض");
            _patientServiceMock.Verify(x => x.UpdateAsync(It.IsAny<Patient>()), Times.Once);
            _patientServiceMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
            capturedPatient.Should().NotBeNull();
            capturedPatient!.FullName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task SaveAsync_When_No_FullName_Should_Show_Error_LogicGuard()
        {
            // 1.1 Validation
            // Arrange
            _viewModel.LabId = "LAB-001";
            _viewModel.FullName = "";

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("يرجى إدخال اسم المريض");
            _patientServiceMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }
    }
}
