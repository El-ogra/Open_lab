using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using System.Collections.ObjectModel;

namespace Open_lab.Tests.ViewModels
{
    public class PatientTestsSelectionViewModelTests : IDisposable
    {
        private readonly Mock<IPatientService> _patientServiceMock;
        private readonly Mock<IVisitService> _visitServiceMock;
        private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
        private readonly Mock<IInvoiceService> _invoiceServiceMock;
        private readonly PatientTestsSelectionViewModel _viewModel;

        public PatientTestsSelectionViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _patientServiceMock = new Mock<IPatientService>();
            _visitServiceMock = new Mock<IVisitService>();
            _testCatalogServiceMock = new Mock<ITestCatalogService>();
            _invoiceServiceMock = new Mock<IInvoiceService>();

            _viewModel = new PatientTestsSelectionViewModel(
                _patientServiceMock.Object,
                _visitServiceMock.Object,
                _testCatalogServiceMock.Object,
                _invoiceServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadPatientCommand_Should_Load_Patient_Data_LogicGuard()
        {
            // Arrange
            _viewModel.LabId = "LAB-001";
            var patient = new Patient { PatientId = 10, FullName = "Alice", LabId = "LAB-001" };
            _patientServiceMock.Setup(service => service.GetByLabIdAsync("LAB-001")).ReturnsAsync(patient);
            _visitServiceMock.Setup(service => service.GetByPatientIdAsync(10)).ReturnsAsync(new List<Visit>());
            _testCatalogServiceMock.Setup(service => service.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            _testCatalogServiceMock.Setup(service => service.GetAllTestsAsync()).ReturnsAsync(new List<Test>());
            _testCatalogServiceMock.Setup(service => service.GetCustomGroupsAsync()).ReturnsAsync(new List<CustomGroup>());

            // Act
            await _viewModel.InvokePrivateAsync("LoadPatientAsync");

            // Assert
            _viewModel.PatientId.Should().Be(10);
            _viewModel.PatientName.Should().Be("Alice");
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
            _patientServiceMock.Verify(service => service.GetByLabIdAsync("LAB-001"), Times.Once);
        }

        [Fact]
        public async Task AddTestCommand_Should_Add_Test_To_Visit_LogicGuard()
        {
            // Arrange - 1.3 Add test
            _viewModel.InvokePrivate("set_VisitId", 100);
            var test = new Test { TestId = 1, Code = "GLU", Price = 50m };
            _viewModel.SelectedAvailableTest = test;

            var visitTest = new VisitTest { VisitTestId = 1, VisitId = 100, TestId = 1, Price = 50m, Test = test };
            _visitServiceMock.Setup(service => service.AddTestToVisitAsync(100, 1, It.IsAny<decimal?>())).ReturnsAsync(visitTest);
            _visitServiceMock.Setup(service => service.GetVisitTestsAsync(100)).ReturnsAsync(new List<VisitTest> { visitTest });
            _invoiceServiceMock.Setup(i => i.CreateOrUpdateInvoiceAsync(100, It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ReturnsAsync(new Invoice { InvoiceId = 1, VisitId = 100, Total = 50, Discount = 0, NetTotal = 50, Paid = 0, Balance = 50 });

            // Act
            await _viewModel.InvokePrivateAsync("AddTestAsync");

            // Assert
            _viewModel.SelectedTests.Should().Contain(t => t.VisitTestId == 1);
            _viewModel.SelectedTests.Should().HaveCount(1);
            _viewModel.StatusMessage.Should().Contain("تمت إضافة");
            _invoiceServiceMock.Verify(i => i.CreateOrUpdateInvoiceAsync(100, It.IsAny<decimal>(), It.IsAny<decimal>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddTestCommand_When_Service_Throws_Should_Set_Error_StatusMessage()
        {
            // Arrange - FAILURE test for AddTestCommand
            _viewModel.InvokePrivate("set_VisitId", 100);
            var test = new Test { TestId = 1, Code = "GLU", Price = 50m };
            _viewModel.SelectedAvailableTest = test;
            _visitServiceMock.Setup(service => service.AddTestToVisitAsync(100, 1, It.IsAny<decimal?>()))
                .ThrowsAsync(new Exception("Service Error"));

            // Act
            await _viewModel.InvokePrivateAsync("AddTestAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ");
            _visitServiceMock.Verify(service => service.AddTestToVisitAsync(100, 1, It.IsAny<decimal?>()), Times.Once);
        }

        [Fact]
        public async Task AddTestCommand_When_SelectedAvailableTest_Null_Should_Not_Call_Service_LogicGuard()
        {
            // Arrange - FAILURE test for AddTestCommand with null test
            _viewModel.InvokePrivate("set_VisitId", 100);
            _viewModel.SelectedAvailableTest = null;

            // Act
            await _viewModel.InvokePrivateAsync("AddTestAsync");

            // Assert
            _visitServiceMock.Verify(service => service.AddTestToVisitAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal?>()), Times.Never);
        }

        [Fact]
        public async Task RemoveTestCommand_Should_Remove_Test_LogicGuard()
        {
            // Arrange - 1.4 Delete test
            _viewModel.InvokePrivate("set_VisitId", 100);
            var item = new SelectedTestItem { VisitTestId = 200, TestName = "Test" };
            _viewModel.SelectedTests.Add(item);
            _viewModel.SelectedVisitTest = item;

            _visitServiceMock.Setup(service => service.RemoveVisitTestAsync(200)).Returns(Task.CompletedTask);
            _visitServiceMock.Setup(service => service.GetVisitTestsAsync(100)).ReturnsAsync(new List<VisitTest>());
            _invoiceServiceMock.Setup(i => i.CreateOrUpdateInvoiceAsync(100, It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ReturnsAsync(new Invoice { InvoiceId = 2, VisitId = 100, Total = 0, Discount = 0, NetTotal = 0, Paid = 0, Balance = 0 });

            // Act
            await _viewModel.InvokePrivateAsync("RemoveTestAsync");

            // Assert
            _viewModel.SelectedTests.Should().NotContain(item);
            _viewModel.StatusMessage.Should().Contain("تم حذف");
            _visitServiceMock.Verify(service => service.RemoveVisitTestAsync(200), Times.Once);
        }

        [Fact]
        public async Task RemoveTestCommand_When_Service_Throws_Should_Set_Error_StatusMessage()
        {
            // Arrange - FAILURE test for RemoveTestCommand
            _viewModel.InvokePrivate("set_VisitId", 100);
            var item = new SelectedTestItem { VisitTestId = 200, TestName = "Test" };
            _viewModel.SelectedTests.Add(item);
            _viewModel.SelectedVisitTest = item;
            _visitServiceMock.Setup(service => service.RemoveVisitTestAsync(200))
                .ThrowsAsync(new Exception("Service Error"));

            // Act
            await _viewModel.InvokePrivateAsync("RemoveTestAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ");
            _visitServiceMock.Verify(service => service.RemoveVisitTestAsync(200), Times.Once);
        }

        [Fact]
        public async Task RemoveTestCommand_When_SelectedVisitTest_Null_Should_Not_Call_Service_LogicGuard()
        {
            // Arrange - FAILURE test for RemoveTestCommand with null selection
            _viewModel.InvokePrivate("set_VisitId", 100);
            _viewModel.SelectedVisitTest = null;

            // Act
            await _viewModel.InvokePrivateAsync("RemoveTestAsync");

            // Assert
            _visitServiceMock.Verify(service => service.RemoveVisitTestAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AddCustomGroupCommand_Should_Add_All_Tests_In_Group_LogicGuard()
        {
            // Arrange - 1.8 Add Group
            _viewModel.InvokePrivate("set_VisitId", 100);
            var group = new CustomGroup { CustomGroupId = 5, Name = "Basic Profile" };
            _viewModel.SelectedCustomGroup = group;

            _visitServiceMock.Setup(service => service.AddCustomGroupToVisitAsync(100, 5))
                .ReturnsAsync(new List<VisitTest> { new VisitTest { VisitTestId = 9, VisitId = 100, TestId = 8, Price = 20m } });
            _visitServiceMock.Setup(service => service.GetVisitTestsAsync(100))
                .ReturnsAsync(new List<VisitTest> { new VisitTest { VisitTestId = 9, VisitId = 100, TestId = 8, Price = 20m, Test = new Test { NameReport = "T" } } });
            _invoiceServiceMock.Setup(i => i.CreateOrUpdateInvoiceAsync(100, It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ReturnsAsync(new Invoice { InvoiceId = 3, VisitId = 100, Total = 20, Discount = 0, NetTotal = 20, Paid = 0, Balance = 20 });

            // Act
            await _viewModel.InvokePrivateAsync("AddCustomGroupAsync");

            // Assert
            _visitServiceMock.Verify(v => v.AddCustomGroupToVisitAsync(100, 5), Times.Once);
            _viewModel.SelectedTests.Should().HaveCount(1);
            _viewModel.StatusMessage.Should().Contain("المجموعة");
        }

        [Fact]
        public async Task AddCustomGroupCommand_When_Service_Throws_Should_Set_Error_StatusMessage()
        {
            // Arrange - FAILURE test for AddCustomGroupCommand
            _viewModel.InvokePrivate("set_VisitId", 100);
            var group = new CustomGroup { CustomGroupId = 5, Name = "Basic Profile" };
            _viewModel.SelectedCustomGroup = group;
            _visitServiceMock.Setup(service => service.AddCustomGroupToVisitAsync(100, 5))
                .ThrowsAsync(new Exception("Service Error"));

            // Act
            await _viewModel.InvokePrivateAsync("AddCustomGroupAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ");
            _visitServiceMock.Verify(v => v.AddCustomGroupToVisitAsync(100, 5), Times.Once);
        }

        [Fact]
        public async Task AddCustomGroupCommand_When_SelectedCustomGroup_Null_Should_Not_Call_Service_LogicGuard()
        {
            // Arrange - FAILURE test for AddCustomGroupCommand with null group
            _viewModel.InvokePrivate("set_VisitId", 100);
            _viewModel.SelectedCustomGroup = null;

            // Act
            await _viewModel.InvokePrivateAsync("AddCustomGroupAsync");

            // Assert
            _visitServiceMock.Verify(v => v.AddCustomGroupToVisitAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateVisitAsync_When_ReferralAccountWithoutReferral_Should_Set_ValidationMessage_FailureGuard()
        {
            // Arrange
            _viewModel.InvokePrivate("set_PatientId", 10);
            _viewModel.SelectedAccountType = "Referral";
            _viewModel.SelectedReferral = null;

            // Act
            await _viewModel.InvokePrivateAsync("CreateVisitAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("يرجى اختيار جهة إحالة");
            _visitServiceMock.Verify(v => v.CreateAsync(It.IsAny<Visit>()), Times.Never);
        }

        [Fact]
        public async Task CreateVisitAsync_When_ServiceThrows_Should_Set_ErrorMessage_FailureGuard()
        {
            // Arrange
            _viewModel.InvokePrivate("set_PatientId", 11);
            _viewModel.SelectedAccountType = "Cash";
            _visitServiceMock.Setup(v => v.CreateAsync(It.IsAny<Visit>()))
                .ThrowsAsync(new Exception("create-visit-failed"));

            // Act
            await _viewModel.InvokePrivateAsync("CreateVisitAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("create-visit-failed");
        }

        [Fact]
        public async Task RefreshTestsCommand_When_TestCatalogThrows_Should_Set_ErrorMessage_EdgeGuard()
        {
            // Arrange
            _testCatalogServiceMock.Setup(s => s.GetAllTestsAsync()).ThrowsAsync(new Exception("catalog-failed"));

            // Act
            await _viewModel.InvokePrivateAsync("LoadAvailableTestsAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("catalog-failed");
        }
    }
}
