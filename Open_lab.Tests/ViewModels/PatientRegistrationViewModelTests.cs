using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using System.Linq;

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
            // Function: 1.1 — Add New Patient
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
            // Function: 1.1 — Add New Patient
            // Arrange
            // Act
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
            // Function: 1.1 — Add New Patient
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
            _viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

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
            // Function: 1.1 — Add New Patient
            // Arrange
            _viewModel.LabId = "LAB-001";
            _viewModel.FullName = "Test";
            _viewModel.Gender = "Male";
            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>())).ThrowsAsync(new Exception("DB Error"));

            // Act
            _viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("DB Error");
        }

        [Fact]
        public async Task LoadByLabIdAsync_With_Empty_LabId_Should_Set_StatusMessage()
        {
            // Function: 1.2 — Edit Patient Data
            // Arrange
            _viewModel.LabId = "";

            // Act
            _viewModel.LoadByLabIdCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("يرجى إدخال Lab ID");
        }

        [Fact]
        public async Task LoadByLabIdAsync_With_NonExistent_LabId_Should_Set_StatusMessage()
        {
            // Function: 1.2 — Edit Patient Data
            // Arrange
            _viewModel.LabId = "LAB-999";
            _patientServiceMock.Setup(x => x.GetByLabIdAsync("LAB-999")).ReturnsAsync((Patient?)null);

            // Act
            _viewModel.LoadByLabIdCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("لم يتم العثور");
        }

        [Fact]
        public async Task LoadByLabIdAsync_With_Existing_LabId_Should_Load_Patient_Data()
        {
            // Function: 1.2 — Edit Patient Data
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
            _viewModel.LoadByLabIdCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.PatientId.Should().Be(10);
            _viewModel.FullName.Should().Be("Alice");
            _viewModel.Gender.Should().Be("Female");
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task SearchAsync_Should_Populate_Results_LogicGuard()
        {
            // Function: 1.5 — Search Patient
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
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100);

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
            // Function: 1.2 — Edit Patient Data
            // Arrange - PatientId defaults to 0
            var deleteCalled = false;
            _patientServiceMock.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .Callback<int>(id => deleteCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.DeleteCommand.Execute(null);
            await Task.Delay(100);

            // Assert - Logic Guard: Verify delete service was NOT called for PatientId 0
            deleteCalled.Should().BeFalse();
            _patientServiceMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }



        [Fact]
        public async Task SelectedPatient_Setter_Should_Load_Patient_Data_LogicGuard()
        {
            // Function: 1.2 — Edit Patient Data
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
            await Task.Delay(100);

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
            // Function: 1.5 — Search Patient
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
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100);

            // Assert - Logic Guard: Verify search returns all patients matching name
            _viewModel.Results.Should().HaveCount(3);
            _viewModel.Results.Should().Contain(r => r.FullName == "Ahmed Ali");
            _viewModel.Results.Should().Contain(r => r.FullName == "Ahmed Mohamed");
            _viewModel.Results.Should().Contain(r => r.FullName == "Sara Ahmed");
        }

        [Fact]
        public async Task SearchAsync_By_Phone_Should_Return_Exact_Match_LogicGuard()
        {
            // Function: 1.5 — Search Patient
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
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100);

            // Assert - Logic Guard: Verify search returns exact phone match
            _viewModel.Results.Should().ContainSingle();
            _viewModel.Results[0].PatientId.Should().Be(10);
            _viewModel.Results[0].Phone.Should().Be("5551234");
            _viewModel.Results[0].FullName.Should().Be("John Doe");
        }

        [Fact]
        public async Task SearchAsync_With_No_Results_Should_Return_Empty_LogicGuard()
        {
            // Function: 1.5 — Search Patient
            // 1.5 Break case: no results found
            // Arrange
            _viewModel.FullName = "NonExistent";
            _viewModel.Phone = "9999999";
            _patientServiceMock.Setup(x => x.SearchAsync("NonExistent", "9999999", It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(new List<Patient>());

            // Act
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100);

            // Assert - Logic Guard: Verify empty result set and appropriate message
            _viewModel.Results.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("0");
        }

        [Fact]
        public async Task SearchAsync_Partial_Match_Should_Return_Relevant_Results_LogicGuard()
        {
            // Function: 1.5 — Search Patient
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
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(100);

            // Assert - Logic Guard: Verify partial matches are returned correctly
            _viewModel.Results.Should().HaveCount(2);
            _viewModel.Results.Should().Contain(r => r.FullName.Contains("Moh"));
        }

        // 1.7 Medical History Integrity Tests - NEW TEST

        [Fact]
        public async Task SaveAsync_With_MedicalHistory_Should_Save_Complete_History_LogicGuard()
        {
            // Function: 1.7 — Add Medical History
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
            _viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

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
        public async Task SaveAsync_When_MedicalHistorySaveFails_Should_Set_Error_Message_FailureGuard()
        {
            // Function: 1.7 — Add Medical History
            // Arrange
            _viewModel.LabId = "LAB-HIST-ERR";
            _viewModel.FullName = "History Patient";
            _viewModel.Gender = "Female";
            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) => { p.PatientId = 101; return p; });
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(101, It.IsAny<MedicalHistory>()))
                .ThrowsAsync(new InvalidOperationException("history-failed"));

            // Act
            _viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("history-failed");
        }

        [Fact]
        public async Task SaveAsync_With_Empty_MedicalHistoryFields_Should_Save_Without_Throw_EdgeGuard()
        {
            // Function: 1.7 — Add Medical History
            // Arrange
            _viewModel.LabId = "LAB-HIST-EMPTY";
            _viewModel.FullName = "History Patient";
            _viewModel.Gender = "Female";
            _viewModel.ChronicDiseases = string.Empty;
            _viewModel.Allergies = string.Empty;
            _viewModel.Medications = string.Empty;
            _viewModel.MedicalNotes = string.Empty;

            MedicalHistory? capturedHistory = null;
            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) => { p.PatientId = 102; return p; });
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(102, It.IsAny<MedicalHistory>()))
                .Callback<int, MedicalHistory>((_, h) => capturedHistory = h)
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            capturedHistory.Should().NotBeNull();
            capturedHistory!.ChronicDiseases.Should().BeEmpty();
            capturedHistory.Allergies.Should().BeEmpty();
            capturedHistory.Medications.Should().BeEmpty();
            capturedHistory.Notes.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveAsync_With_Existing_Patient_Should_Update_Patient_LogicGuard()
        {
            // Function: 1.2 — Edit Patient Data
            // 1.2 Edit patient data
            // Arrange - Set PatientId > 0 to trigger UpdateAsync
            _viewModel.InvokePrivate("set_PatientId", 500);
            _viewModel.LabId = "LAB-500";
            _viewModel.FullName = "Updated Name";
            _viewModel.Gender = "Male";

            Patient? capturedPatient = null;
            _patientServiceMock.Setup(x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<int>()))
                .Callback<Patient, int>((p, _) => capturedPatient = p)
                .Returns(Task.CompletedTask);
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(500, It.IsAny<MedicalHistory>()))
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert - Logic Guard: Verify UpdateAsync was called instead of CreateAsync
            _viewModel.StatusMessage.Should().Contain("تم تحديث بيانات المريض");
            _patientServiceMock.Verify(x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<int>()), Times.Once);
            _patientServiceMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
            capturedPatient.Should().NotBeNull();
            capturedPatient!.FullName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task SaveAsync_With_SelectedTests_Should_CreateVisit_AddTests_And_CreateInvoice()
        {
            // Function: 1.1 — Add New Patient with visit tests and invoice
            // Arrange
            var visitServiceMock = new Mock<IVisitService>();
            var invoiceServiceMock = new Mock<IInvoiceService>();
            var vm = new PatientRegistrationViewModel(
                _patientServiceMock.Object,
                null,
                visitServiceMock.Object,
                invoiceServiceMock.Object,
                null,
                null);

            vm.LabId = "LAB-WF-001";
            vm.FullName = "Workflow Patient";
            vm.Gender = "Male";
            vm.Age = 35;
            vm.SelectedTests.Add(new SelectedTestItem { TestId = 10, TestName = "CBC", Price = 150m });
            vm.DiscountPercent = 10m;
            vm.PaidAmount = 50m;

            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient p) =>
                {
                    p.PatientId = 500;
                    return p;
                });
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(500, It.IsAny<MedicalHistory>()))
                .Returns(Task.CompletedTask);

            visitServiceMock.Setup(x => x.CreateAsync(It.IsAny<Visit>()))
                .ReturnsAsync((Visit v) =>
                {
                    v.VisitId = 700;
                    return v;
                });
            visitServiceMock.Setup(x => x.GetVisitTestsAsync(700))
                .ReturnsAsync(new List<VisitTest>());
            visitServiceMock.Setup(x => x.AddTestToVisitAsync(700, 10, 150m))
                .ReturnsAsync(new VisitTest { VisitTestId = 900, VisitId = 700, TestId = 10, Price = 150m });
            invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(700, 15m, 50m))
                .ReturnsAsync(new Invoice { InvoiceId = 800, VisitId = 700, Total = 150m, Discount = 15m, Paid = 50m, Balance = 85m });

            // Act
            vm.SaveCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            visitServiceMock.Verify(x => x.CreateAsync(It.Is<Visit>(v => v.PatientId == 500)), Times.Once);
            visitServiceMock.Verify(x => x.AddTestToVisitAsync(700, 10, 150m), Times.Once);
            invoiceServiceMock.Verify(x => x.CreateOrUpdateInvoiceAsync(700, 15m, 50m), Times.Once);
            vm.CurrentVisitId.Should().Be(700);
            vm.SelectedTests[0].VisitTestId.Should().Be(900);
        }

        [Fact]
        public async Task AssignPatientToContract_WithSelectedReferral_ShouldPassReferralIdToUpdate()
        {
            // Function: 12.5 — Assign Patient to Contract
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            var referral = new Referral { ReferralId = 12, Name = "Contract Entity", ReferralType = "Company" };
            catalogMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral> { referral });
            _patientServiceMock.Setup(x => x.GenerateNextLabIdAsync(It.IsAny<DateTime?>())).ReturnsAsync("LAB-CON-001");

            var viewModel = new PatientRegistrationViewModel(_patientServiceMock.Object, catalogMock.Object);
            await Task.Delay(100);
            viewModel.InvokePrivate("set_PatientId", 700);
            viewModel.LabId = "LAB-700";
            viewModel.FullName = "Contract Patient";
            viewModel.Gender = "Male";
            viewModel.SelectedReferral = referral;

            Patient? capturedPatient = null;
            _patientServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<int>()))
                .Callback<Patient, int>((patient, _) => capturedPatient = patient)
                .Returns(Task.CompletedTask);
            _patientServiceMock
                .Setup(x => x.SaveMedicalHistoryAsync(700, It.IsAny<MedicalHistory>()))
                .Returns(Task.CompletedTask);

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _patientServiceMock.Verify(x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<int>()), Times.Once);
            capturedPatient.Should().NotBeNull();
            capturedPatient!.ReferralId.Should().Be(12);
            viewModel.StatusMessage.Should().Contain("تم تحديث بيانات المريض");
        }

        [Fact]
        public async Task AssignPatientToContract_WhenServiceRejectsReferral_ShouldShowErrorMessage()
        {
            // Function: 12.5 — Assign Patient to Contract
            // Arrange
            var rejectedReferral = new Referral { ReferralId = 99, Name = "Rejected Contract", ReferralType = "Company" };
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral> { rejectedReferral });

            var viewModel = new PatientRegistrationViewModel(_patientServiceMock.Object, catalogMock.Object);
            await Task.Delay(100);
            viewModel.InvokePrivate("set_PatientId", 701);
            viewModel.LabId = "LAB-701";
            viewModel.FullName = "Rejected Patient";
            viewModel.Gender = "Female";
            viewModel.SelectedReferral = rejectedReferral;

            _patientServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Referral contract not found."));

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _patientServiceMock.Verify(x => x.UpdateAsync(It.Is<Patient>(p => p.ReferralId == 99), It.IsAny<int>()), Times.Once);
            viewModel.StatusMessage.Should().Contain("Referral contract not found");
        }

        [Fact]
        public async Task AssignPatientToContract_WithNoReferralSelected_ShouldPassNullReferralId()
        {
            // Function: 12.5 — Assign Patient to Contract
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral>());

            var viewModel = new PatientRegistrationViewModel(_patientServiceMock.Object, catalogMock.Object);
            await Task.Delay(100);
            viewModel.InvokePrivate("set_PatientId", 702);
            viewModel.LabId = "LAB-702";
            viewModel.FullName = "Cash Patient";
            viewModel.Gender = "Male";
            viewModel.SelectedReferral = null;

            Patient? capturedPatient = null;
            _patientServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<int>()))
                .Callback<Patient, int>((patient, _) => capturedPatient = patient)
                .Returns(Task.CompletedTask);
            _patientServiceMock
                .Setup(x => x.SaveMedicalHistoryAsync(702, It.IsAny<MedicalHistory>()))
                .Returns(Task.CompletedTask);

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _patientServiceMock.Verify(x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<int>()), Times.Once);
            capturedPatient.Should().NotBeNull();
            capturedPatient!.ReferralId.Should().BeNull();
        }

        [Fact]
        public async Task SaveAsync_When_No_FullName_Should_Show_Error_LogicGuard()
        {
            // Function: 1.1 — Add New Patient
            // 1.1 Validation
            // Arrange
            _viewModel.LabId = "LAB-001";
            _viewModel.FullName = "";

            // Act
            _viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("يرجى إدخال اسم المريض");
            _patientServiceMock.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Never);
        }

        [Fact]
        public async Task GenerateLabIdAsync_When_ServiceThrows_Should_Set_ErrorMessage_FailureGuard()
        {
            // Function: 1.1 — Add New Patient
            // Arrange
            _patientServiceMock
                .Setup(x => x.GenerateNextLabIdAsync(It.IsAny<DateTime?>()))
                .ThrowsAsync(new Exception("gen-failed"));

            // Act
            _viewModel.GenerateLabIdCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ توليد Lab ID");
            _viewModel.StatusMessage.Should().Contain("gen-failed");
        }

        [Fact]
        public async Task DeleteAsync_When_DeleteServiceThrows_Should_Set_ErrorMessage_FailureGuard()
        {
            // Function: 1.2 — Edit Patient Data
            // Arrange
            _viewModel.InvokePrivate("set_PatientId", 9);
            _patientServiceMock
                .Setup(x => x.DeleteAsync(9))
                .ThrowsAsync(new InvalidOperationException("cannot-delete"));

            // Act
            _viewModel.DeleteCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("cannot-delete");
        }

        [Fact]
        public async Task ClearFormAsync_When_PatientExists_Should_Reset_Fields_And_Generate_NewLabId_EdgeGuard()
        {
            // Function: 1.1 — Add New Patient
            // Arrange
            _viewModel.InvokePrivate("set_PatientId", 50);
            _viewModel.FullName = "Filled";
            _viewModel.Gender = "Male";
            _viewModel.LabId = "LAB-OLD";
            _patientServiceMock.Setup(x => x.GenerateNextLabIdAsync(It.IsAny<DateTime?>())).ReturnsAsync("LAB-NEW");

            // Act
            _viewModel.NewCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.PatientId.Should().Be(0);
            _viewModel.FullName.Should().BeEmpty();
            _viewModel.Gender.Should().BeEmpty();
            _viewModel.LabId.Should().Be("LAB-NEW");
        }

        [Fact]
        public void PatientRegistrationViewModel_Should_Expose_Visit_And_Invoice_Dependencies_For_RegistrationWorkflow()
        {
            // Function: 1.1 — Add New Patient with Visit and Invoice
            // Arrange
            var constructor = typeof(PatientRegistrationViewModel).GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First();
            var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType).ToList();

            // Act
            var hasResultEntryDependency = parameterTypes.Any(t =>
                t.Name.Contains("Result", StringComparison.OrdinalIgnoreCase));

            // Assert
            parameterTypes.Should().Contain(typeof(IPatientService));
            parameterTypes.Should().Contain(typeof(ITestCatalogService));
            parameterTypes.Should().Contain(typeof(IVisitService));
            parameterTypes.Should().Contain(typeof(IInvoiceService));
            hasResultEntryDependency.Should().BeFalse("registration owns visit and invoice creation but still must not depend on result-entry services.");
        }

        [Fact]
        public async Task PrintReceiptCommand_Should_Print_Receipt_With_SelectedTests_And_InvoiceTotals()
        {
            var printMock = new Mock<IPrintService>();
            var visitMock = new Mock<IVisitService>();
            var invoiceMock = new Mock<IInvoiceService>();
            var vm = new PatientRegistrationViewModel(
                _patientServiceMock.Object,
                null,
                visitMock.Object,
                invoiceMock.Object,
                null,
                printMock.Object);

            vm.LabId = "LAB-20";
            vm.FullName = "Receipt Patient";
            vm.SelectedTests.Add(new SelectedTestItem { TestId = 1, TestName = "CBC", Price = 120m });
            vm.DiscountPercent = 10m;
            vm.PaidAmount = 50m;

            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient patient) =>
                {
                    patient.PatientId = 10;
                    return patient;
                });
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(10, It.IsAny<MedicalHistory>()))
                .Returns(Task.CompletedTask);
            visitMock.Setup(x => x.CreateAsync(It.IsAny<Visit>()))
                .ReturnsAsync((Visit visit) =>
                {
                    visit.VisitId = 20;
                    return visit;
                });
            visitMock.Setup(x => x.GetVisitTestsAsync(20)).ReturnsAsync(new List<VisitTest>());
            visitMock.Setup(x => x.AddTestToVisitAsync(20, 1, 120m))
                .ReturnsAsync(new VisitTest { VisitTestId = 30, VisitId = 20, TestId = 1, Price = 120m });
            invoiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(20, 12m, 50m))
                .ReturnsAsync(new Invoice { InvoiceId = 40, VisitId = 20, Discount = 12m, Paid = 50m });

            vm.SaveCommand.Execute(null);
            await Task.Delay(150);
            vm.PrintReceiptCommand.Execute(null);
            await Task.Delay(100);

            printMock.Verify(x => x.PrintReceiptAsync(
                It.Is<ReceiptData>(data =>
                    data.Patient.FullName == "Receipt Patient" &&
                    data.VisitTests.Count == 1 &&
                    data.Invoice != null &&
                    data.Invoice.Discount == 12m),
                "LAB-20"), Times.Once);
        }

        [Fact]
        public async Task WorksheetCommand_Should_Print_Patient_Worksheet()
        {
            var printMock = new Mock<IPrintService>();
            var vm = new PatientRegistrationViewModel(
                _patientServiceMock.Object,
                null,
                null,
                null,
                null,
                printMock.Object);

            vm.FullName = "Worksheet Patient";
            vm.SelectedTests.Add(new SelectedTestItem { TestId = 2, TestName = "Glucose", Price = 80m });

            vm.WorksheetCommand.Execute(null);
            await Task.Delay(100);

            printMock.Verify(x => x.PrintWorksheetByPatientAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.Is<IReadOnlyCollection<WorkSheetPatientRow>>(rows =>
                    rows.Count == 1 &&
                    rows.Single().PatientName == "Worksheet Patient" &&
                    rows.Single().TestsCount == 1)), Times.Once);
        }

        [Fact]
        public async Task SettleCommand_When_VisitSaved_Should_Save_InvoicePayment()
        {
            var visitMock = new Mock<IVisitService>();
            var invoiceMock = new Mock<IInvoiceService>();

            var vm = new PatientRegistrationViewModel(
                _patientServiceMock.Object,
                null,
                visitMock.Object,
                invoiceMock.Object,
                null,
                null);

            vm.FullName = "Settle Patient";
            vm.LabId = "LAB-44";
            vm.SelectedTests.Add(new SelectedTestItem { TestId = 3, TestName = "ALT", Price = 100m });
            _patientServiceMock.Setup(x => x.CreateAsync(It.IsAny<Patient>()))
                .ReturnsAsync((Patient patient) =>
                {
                    patient.PatientId = 44;
                    return patient;
                });
            _patientServiceMock.Setup(x => x.SaveMedicalHistoryAsync(44, It.IsAny<MedicalHistory>()))
                .Returns(Task.CompletedTask);
            visitMock.Setup(x => x.CreateAsync(It.IsAny<Visit>()))
                .ReturnsAsync((Visit visit) =>
                {
                    visit.VisitId = 44;
                    return visit;
                });
            visitMock.Setup(x => x.GetVisitTestsAsync(44)).ReturnsAsync(new List<VisitTest>());
            visitMock.Setup(x => x.AddTestToVisitAsync(44, 3, 100m))
                .ReturnsAsync(new VisitTest { VisitTestId = 55, VisitId = 44, TestId = 3, Price = 100m });
            invoiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(44, 0m, 0m))
                .ReturnsAsync(new Invoice { VisitId = 44, Total = 100m, NetTotal = 100m, Paid = 0m, Balance = 100m });
            invoiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(44, 0m, 100m))
                .ReturnsAsync(new Invoice { VisitId = 44, Total = 100m, NetTotal = 100m, Paid = 100m, Balance = 0m });

            vm.SaveCommand.Execute(null);
            await Task.Delay(150);
            vm.SettleCommand.Execute(null);
            await Task.Delay(100);

            invoiceMock.Verify(x => x.CreateOrUpdateInvoiceAsync(44, 0m, 100m), Times.Once);
            vm.BalanceForLab.Should().Be(0m);
        }
    }
}
