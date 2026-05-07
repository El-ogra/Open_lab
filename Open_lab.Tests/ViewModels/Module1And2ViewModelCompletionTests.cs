using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    /// <summary>
    /// Completion tests for Module 1 (Patient Management) and Module 2 (Financial Accounting)
    /// ViewModel layer — fills all missing Success / Failure / Edge scenarios so both modules
    /// reach 100 % coverage per UnitTest_Skill.md.
    /// </summary>
    public class Module1And2ViewModelCompletionTests : IDisposable
    {
        private readonly Mock<IPatientService> _patientMock;
        private readonly Mock<IVisitService> _visitMock;
        private readonly Mock<ITestCatalogService> _catalogMock;
        private readonly Mock<IInvoiceService> _invoiceMock;
        private readonly Mock<IPatientSearchService> _searchMock;

        public Module1And2ViewModelCompletionTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _patientMock = new Mock<IPatientService>();
            _visitMock = new Mock<IVisitService>();
            _catalogMock = new Mock<ITestCatalogService>();
            _invoiceMock = new Mock<IInvoiceService>();
            _searchMock = new Mock<IPatientSearchService>();
        }

        public void Dispose() => AppSessionTestHelper.Reset();

        // ────────────────────────────────────────────────────────────────────────
        // 1.2 — Edit Patient Data — ViewModel edge cases
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task EditPatient_SaveAsync_WhenUpdateThrows_ShouldSetErrorMessage_FailureGuard()
        {
            // Function: 1.2 — Edit Patient Data
            // Arrange
            var vm = new PatientRegistrationViewModel(_patientMock.Object);
            vm.InvokePrivate("set_PatientId", 15);
            vm.LabId = "ED-VM-001";
            vm.FullName = "Edit Test";
            vm.Gender = "Male";
            _patientMock.Setup(x => x.UpdateAsync(It.IsAny<Patient>()))
                .ThrowsAsync(new InvalidOperationException("update-failed"));

            // Act
            vm.SaveCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            vm.StatusMessage.Should().Contain("update-failed");
            _patientMock.Verify(x => x.UpdateAsync(It.IsAny<Patient>()), Times.Once);
            vm.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task EditPatient_LoadByLabId_WhenPatientFoundWithMedicalHistory_ShouldPopulateAllFields_SuccessGuard()
        {
            // Function: 1.2 — Edit Patient Data
            // Arrange
            var vm = new PatientRegistrationViewModel(_patientMock.Object);
            vm.LabId = "ED-VM-002";

            var patient = new Patient
            {
                PatientId = 20,
                LabId = "ED-VM-002",
                FullName = "Full Patient",
                Gender = "Female",
                Phone = "0501111111",
                Address = "Test Address",
                BirthDate = new DateTime(1990, 3, 10)
            };
            var history = new MedicalHistory
            {
                ChronicDiseases = "Diabetes",
                Allergies = "Latex",
                Medications = "Metformin",
                Notes = "Monitor closely"
            };
            _patientMock.Setup(x => x.GetByLabIdAsync("ED-VM-002")).ReturnsAsync(patient);
            _patientMock.Setup(x => x.GetMedicalHistoryAsync(20)).ReturnsAsync(history);

            // Act
            vm.LoadByLabIdCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            vm.PatientId.Should().Be(20);
            vm.FullName.Should().Be("Full Patient");
            vm.Gender.Should().Be("Female");
            vm.Phone.Should().Be("0501111111");
            vm.Address.Should().Be("Test Address");
            vm.ChronicDiseases.Should().Be("Diabetes");
            vm.Allergies.Should().Be("Latex");
            vm.Medications.Should().Be("Metformin");
            vm.MedicalNotes.Should().Be("Monitor closely");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.3 — Add Tests to Patient — PatientTestsSelectionViewModel edge cases
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public void AddTestCommand_When_VisitIdIsZero_Should_Be_Disabled_EdgeGuard()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var vm = new PatientTestsSelectionViewModel(
                _patientMock.Object, _visitMock.Object, _catalogMock.Object, _invoiceMock.Object);
            // VisitId stays 0 (default)
            vm.SelectedAvailableTest = new Test { TestId = 5, Code = "X1", Price = 50m };

            // Act & Assert
            vm.AddTestCommand.CanExecute(null).Should().BeFalse("command should be disabled when VisitId is 0");
        }

        [Fact]
        public async Task LoadPatient_WhenLabIdEmpty_ShouldNotCallService_FailureGuard()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var vm = new PatientTestsSelectionViewModel(
                _patientMock.Object, _visitMock.Object, _catalogMock.Object, _invoiceMock.Object);
            vm.LabId = "";

            // Act
            vm.LoadPatientCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _patientMock.Verify(x => x.GetByLabIdAsync(It.IsAny<string>()), Times.Never);
            vm.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadPatient_WhenPatientNotFound_ShouldSetStatusMessage_FailureGuard()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var vm = new PatientTestsSelectionViewModel(
                _patientMock.Object, _visitMock.Object, _catalogMock.Object, _invoiceMock.Object);
            vm.LabId = "LAB-NOT-FOUND";
            _patientMock.Setup(x => x.GetByLabIdAsync("LAB-NOT-FOUND")).ReturnsAsync((Patient?)null);

            // Act
            vm.LoadPatientCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            vm.StatusMessage.Should().NotBeNullOrEmpty();
            vm.PatientId.Should().Be(0, "patient ID should stay 0 when not found");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.4 — Delete Tests — ViewModel edge: delete when VisitId is zero
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public void RemoveTestCommand_WhenSelectedVisitTestIsNull_ShouldBeDisabled_EdgeGuard()
        {
            // Function: 1.4 — Delete Tests
            // Arrange
            var vm = new PatientTestsSelectionViewModel(
                _patientMock.Object, _visitMock.Object, _catalogMock.Object, _invoiceMock.Object);
            vm.SelectedVisitTest = null;

            // Act & Assert
            vm.RemoveTestCommand.CanExecute(null).Should().BeFalse("cannot remove a test when none is selected");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.5 — Search Patient — PatientSearchViewModel: edge case empty results
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SearchPatient_WhenNoResults_ShouldShowZeroCountInStatus_EdgeGuard()
        {
            // Function: 1.5 — Search Patient
            // Arrange
            var vm = new PatientSearchViewModel(_searchMock.Object);
            vm.Name = "NonExistent";
            _searchMock.Setup(s => s.SearchPatientsAsync("NonExistent", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(new List<Patient>());

            // Act
            vm.SearchCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            vm.Patients.Should().BeEmpty();
            vm.StatusMessage.Should().Contain("0");
        }

        [Fact]
        public async Task SearchPatient_ByLabIdOnly_ShouldReturnMatch_SuccessGuard()
        {
            // Function: 1.5 — Search Patient
            // Arrange
            var vm = new PatientSearchViewModel(_searchMock.Object);
            vm.LabId = "LAB-007";
            var results = new List<Patient>
            {
                new Patient { PatientId = 7, FullName = "Lab Seven", LabId = "LAB-007" }
            };
            _searchMock.Setup(s => s.SearchPatientsAsync(
                    It.IsAny<string>(), It.IsAny<string>(), "LAB-007", It.IsAny<DateTime?>()))
                .ReturnsAsync(results);

            // Act
            vm.SearchCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            vm.Patients.Should().ContainSingle();
            vm.Patients[0].LabId.Should().Be("LAB-007");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.6 — View Patient History — ViewModel Success test (SelectedPatient loads visits)
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ViewPatientHistory_MultipleVisits_ShouldPopulateVisitsList_SuccessGuard()
        {
            // Function: 1.6 — View Patient History
            // Arrange
            var vm = new PatientSearchViewModel(_searchMock.Object);
            var patient = new Patient { PatientId = 50, FullName = "History Test" };
            var visits = new List<Visit>
            {
                new Visit { VisitId = 10, PatientId = 50, VisitDate = DateTime.Today.AddDays(-5) },
                new Visit { VisitId = 11, PatientId = 50, VisitDate = DateTime.Today.AddDays(-1) }
            };
            _searchMock.Setup(s => s.GetPatientVisitsAsync(50)).ReturnsAsync(visits);

            // Act
            vm.SelectedPatient = patient;
            await Task.Delay(150);

            // Assert
            vm.Visits.Should().HaveCount(2);
            _searchMock.Verify(s => s.GetPatientVisitsAsync(50), Times.Once);
            vm.StatusMessage.Should().NotBeNullOrEmpty();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.7 — Add Medical History — missing ViewModel Success test (update path)
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SaveMedicalHistory_WhenExistingPatient_ShouldCallSaveMedicalHistoryAsync_SuccessGuard()
        {
            // Function: 1.7 — Add Medical History
            // Arrange
            var vm = new PatientRegistrationViewModel(_patientMock.Object);
            vm.InvokePrivate("set_PatientId", 77);
            vm.LabId = "MH-VM-001";
            vm.FullName = "History VM";
            vm.Gender = "Male";
            vm.ChronicDiseases = "Heart Disease";
            vm.Allergies = "None";
            vm.Medications = "Aspirin";
            vm.MedicalNotes = "Monitor BP";

            MedicalHistory? captured = null;
            _patientMock.Setup(x => x.UpdateAsync(It.IsAny<Patient>())).Returns(Task.CompletedTask);
            _patientMock.Setup(x => x.SaveMedicalHistoryAsync(77, It.IsAny<MedicalHistory>()))
                .Callback<int, MedicalHistory>((_, h) => captured = h)
                .Returns(Task.CompletedTask);

            // Act
            vm.SaveCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            _patientMock.Verify(x => x.SaveMedicalHistoryAsync(77, It.IsAny<MedicalHistory>()), Times.Once);
            captured.Should().NotBeNull();
            captured!.ChronicDiseases.Should().Be("Heart Disease");
            captured.Medications.Should().Be("Aspirin");
            captured.Notes.Should().Be("Monitor BP");
            vm.StatusMessage.Should().NotBeNullOrEmpty();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 1.8 — Add Group of Tests — ViewModel: no-group selected edge case
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task AddGroupOfTests_WhenNoVisitCreated_ShouldNotCallService_EdgeGuard()
        {
            // Function: 1.8 — Add Group of Tests
            // Arrange
            var vm = new PatientTestsSelectionViewModel(
                _patientMock.Object, _visitMock.Object, _catalogMock.Object, _invoiceMock.Object);
            // VisitId = 0, SelectedCustomGroup = null
            vm.SelectedCustomGroup = null;

            // Act
            vm.AddCustomGroupCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _visitMock.Verify(v => v.AddCustomGroupToVisitAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            vm.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task AddGroupOfTests_WhenServiceSucceeds_ShouldUpdateSelectedTests_SuccessGuard()
        {
            // Function: 1.8 — Add Group of Tests
            // Arrange
            var vm = new PatientTestsSelectionViewModel(
                _patientMock.Object, _visitMock.Object, _catalogMock.Object, _invoiceMock.Object);
            vm.InvokePrivate("set_VisitId", 55);
            var group = new CustomGroup { CustomGroupId = 3, Name = "CBC Profile" };
            vm.SelectedCustomGroup = group;

            var addedTests = new List<VisitTest>
            {
                new VisitTest { VisitTestId = 201, VisitId = 55, TestId = 10, Price = 30m },
                new VisitTest { VisitTestId = 202, VisitId = 55, TestId = 11, Price = 40m }
            };
            var refreshTests = new List<VisitTest>
            {
                new VisitTest { VisitTestId = 201, VisitId = 55, TestId = 10, Price = 30m, Test = new Test { NameReport = "CBC" } },
                new VisitTest { VisitTestId = 202, VisitId = 55, TestId = 11, Price = 40m, Test = new Test { NameReport = "HGB" } }
            };
            _visitMock.Setup(v => v.AddCustomGroupToVisitAsync(55, 3)).ReturnsAsync(addedTests);
            _visitMock.Setup(v => v.GetVisitTestsAsync(55)).ReturnsAsync(refreshTests);
            _invoiceMock.Setup(i => i.CreateOrUpdateInvoiceAsync(55, It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ReturnsAsync(new Invoice { InvoiceId = 9, VisitId = 55, Total = 70, Discount = 0, NetTotal = 70, Paid = 0, Balance = 70 });

            // Act
            vm.AddCustomGroupCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            vm.SelectedTests.Should().HaveCount(2);
            vm.StatusMessage.Should().Contain("المجموعة");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.1 — Calculate Total — ViewModel: success when VisitId > 0
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CalculateTotal_WhenVisitHasTests_ShouldSetTotal_SuccessGuard()
        {
            // Function: 2.1 — Calculate Total
            // Arrange
            var vm = new PatientBillingViewModel(_invoiceMock.Object);
            vm.VisitId = 10;
            _invoiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(450m);
            _invoiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(new Invoice
            {
                InvoiceId = 1,
                Total = 450m,
                Discount = 0m,
                NetTotal = 450m,
                Paid = 0m,
                Balance = 450m
            });
            _invoiceMock.Setup(x => x.GetPaymentsAsync(1)).ReturnsAsync(new List<Payment>());
            _invoiceMock.Setup(x => x.GetAdditionalChargesAsync(1)).ReturnsAsync(new List<AdditionalCharge>());

            // Act
            vm.LoadVisitCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            vm.Total.Should().Be(450m);
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.2 — Apply Discount — ViewModel: success discount calculation
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ApplyDiscount_WhenValidVisitAndDiscount_ShouldUpdateNetTotal_SuccessGuard()
        {
            // Function: 2.2 — Apply Discount
            // Arrange
            var vm = new PatientBillingViewModel(_invoiceMock.Object);
            vm.VisitId = 10;
            vm.Discount = 30m;
            _invoiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, 30m, 0))
                .ReturnsAsync(new Invoice { InvoiceId = 1, Total = 300m, Discount = 30m, NetTotal = 270m });
            _invoiceMock.Setup(x => x.GetPaymentsAsync(1)).ReturnsAsync(new List<Payment>());

            // Act
            vm.SaveInvoiceCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            vm.NetTotal.Should().Be(270m);
            _invoiceMock.Verify(x => x.CreateOrUpdateInvoiceAsync(10, 30m, 0), Times.Once);
            vm.StatusMessage.Should().NotBeNullOrEmpty();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.3 — Record Payment — ViewModel: negative payment edge
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RecordPayment_WhenNegativeAmount_ShouldShowValidationError_EdgeGuard()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            var vm = new PatientBillingViewModel(_invoiceMock.Object);
            vm.VisitId = 10;
            vm.Paid = -50m;

            // Act
            vm.AddPaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceMock.Verify(x => x.AddPaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            vm.StatusMessage.Should().NotBeNullOrEmpty();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.4 — Settle Account — ViewModel: VisitId = 0 edge
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SettleAccount_WhenVisitIdIsZero_ShouldNotCallService_EdgeGuard()
        {
            // Function: 2.4 — Settle Account
            // Arrange
            var vm = new PatientBillingViewModel(_invoiceMock.Object);
            vm.VisitId = 0;

            // Act
            vm.SettleAccountCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceMock.Verify(x => x.SettleAccountAsync(It.IsAny<int>()), Times.Never);
            vm.StatusMessage.Should().NotBeNullOrEmpty();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.8 — Generate Invoice — ViewModel: Print success when invoice found
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task PrintInvoice_WhenInvoiceFound_ShouldCallLogPrinted_SuccessGuard()
        {
            // Function: 2.8 — Generate Invoice
            // Arrange
            var vm = new PatientBillingViewModel(_invoiceMock.Object);
            vm.VisitId = 88;
            var invoice = new Invoice { InvoiceId = 888, VisitId = 88 };
            _invoiceMock.Setup(x => x.GetByVisitIdAsync(88)).ReturnsAsync(invoice);
            _invoiceMock.Setup(x => x.LogInvoicePrintedAsync(888, It.IsAny<int>())).Returns(Task.CompletedTask);

            // Act
            vm.PrintInvoiceCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            _invoiceMock.Verify(x => x.LogInvoicePrintedAsync(888, It.IsAny<int>()), Times.Once);
            vm.StatusMessage.Should().NotBeNullOrEmpty();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 2.9 — View Patient Account — ViewModel: load with payments list
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoadVisit_WhenInvoiceHasPayments_ShouldPopulatePaymentsList_SuccessGuard()
        {
            // Function: 2.9 — View Patient Account
            // Arrange
            var vm = new PatientBillingViewModel(_invoiceMock.Object);
            vm.VisitId = 10;
            var invoice = new Invoice
            {
                InvoiceId = 50,
                Total = 200m,
                Discount = 0m,
                NetTotal = 200m,
                Paid = 100m,
                Balance = 100m
            };
            var payments = new List<Payment>
            {
                new Payment { PaymentId = 1, InvoiceId = 50, Amount = 100m, PaymentMethod = "Cash" }
            };
            _invoiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(200m);
            _invoiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceMock.Setup(x => x.GetPaymentsAsync(50)).ReturnsAsync(payments);
            _invoiceMock.Setup(x => x.GetAdditionalChargesAsync(50)).ReturnsAsync(new List<AdditionalCharge>());

            // Act
            vm.LoadVisitCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            vm.Total.Should().Be(200m);
            vm.Payments.Should().ContainSingle();
            vm.Payments[0].Amount.Should().Be(100m);
        }
    }
}
