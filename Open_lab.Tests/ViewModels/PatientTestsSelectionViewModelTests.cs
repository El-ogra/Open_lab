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

            // Act
            _viewModel.LoadPatientCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.PatientId.Should().Be(10);
            _viewModel.PatientName.Should().Be("Alice");
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

            // Act
            _viewModel.AddTestCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.SelectedTests.Should().Contain(t => t.VisitTestId == 1);
            _invoiceServiceMock.Verify(i => i.CreateOrUpdateInvoiceAsync(100, It.IsAny<decimal>(), It.IsAny<decimal>()), Times.AtLeastOnce);
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

            // Act
            _viewModel.RemoveTestCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.SelectedTests.Should().NotContain(item);
        }

        [Fact]
        public async Task AddCustomGroupCommand_Should_Add_All_Tests_In_Group_LogicGuard()
        {
            // Arrange - 1.8 Add Group
            _viewModel.InvokePrivate("set_VisitId", 100);
            var group = new CustomGroup { CustomGroupId = 5, Name = "Basic Profile" };
            _viewModel.SelectedCustomGroup = group;

            _visitServiceMock.Setup(service => service.AddCustomGroupToVisitAsync(100, 5)).ReturnsAsync(new List<VisitTest>());
            _visitServiceMock.Setup(service => service.GetVisitTestsAsync(100)).ReturnsAsync(new List<VisitTest>());

            // Act
            _viewModel.AddCustomGroupCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _visitServiceMock.Verify(v => v.AddCustomGroupToVisitAsync(100, 5), Times.Once);
        }
    }
}
