using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class TestCatalogViewModelTests : IDisposable
    {
        private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
        private readonly Mock<IBarcodeService> _barcodeServiceMock;
        private readonly TestCatalogViewModel _viewModel;

        public TestCatalogViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _testCatalogServiceMock = new Mock<ITestCatalogService>();
            _barcodeServiceMock = new Mock<IBarcodeService>();
            _viewModel = new TestCatalogViewModel(_testCatalogServiceMock.Object, _barcodeServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public void TestCatalog_Commands_When_Admin_Should_Be_Enabled()
        {
            _viewModel.SaveCommand.CanExecute(null).Should().BeTrue();
            _viewModel.NewCommand.CanExecute(null).Should().BeTrue();
            _viewModel.ReloadCommand.CanExecute(null).Should().BeTrue();
            _viewModel.GenerateBarcodeCommand.CanExecute(null).Should().BeFalse(); // Code is empty
        }

        [Fact]
        public void GenerateBarcodeCommand_CanExecute_When_Code_Not_Empty_Should_Return_True()
        {
            _viewModel.Code = "CBC";
            _viewModel.GenerateBarcodeCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void Code_Setter_Should_Generate_Barcode_Automatically()
        {
            _barcodeServiceMock.Setup(x => x.GenerateCode128("CBC", It.IsAny<int>(), It.IsAny<int>())).Returns(It.IsAny<System.Windows.Media.ImageSource>());
            _viewModel.Code = "CBC";
            _barcodeServiceMock.Verify(x => x.GenerateCode128("CBC", It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadAsync_Should_Load_Catalog_Data()
        {
            var tests = new List<Test> { new Test { TestId = 1, Code = "CBC" } };
            var groups = new List<TestGroup> { new TestGroup { GroupId = 1 } };
            var sampleTypes = new List<SampleType> { new SampleType { SampleTypeId = 1 } };
            var units = new List<Unit> { new Unit { UnitId = 1 } };

            _testCatalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(tests);
            _testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ReturnsAsync(groups);
            _testCatalogServiceMock.Setup(x => x.GetSampleTypesAsync()).ReturnsAsync(sampleTypes);
            _testCatalogServiceMock.Setup(x => x.GetUnitsAsync()).ReturnsAsync(units);

            await _viewModel.InvokePrivateAsync("LoadAsync");

            _viewModel.Tests.Should().HaveCount(1);
            _viewModel.Groups.Should().HaveCount(1);
            _viewModel.SampleTypes.Should().HaveCount(1);
            _viewModel.Units.Should().HaveCount(1);
        }

        [Fact]
        public async Task SaveAsync_With_SelectedTest_Should_Update()
        {
            _viewModel.SelectedTest = new Test { TestId = 1, Code = "CBC", NameReport = "Complete Blood Count" };
            _viewModel.Code = "CBC";
            _viewModel.NameReport = "Complete Blood Count";
            _viewModel.Price = 50;
            _viewModel.SelectedGroup = new TestGroup { GroupId = 1 };

            _testCatalogServiceMock.Setup(x => x.UpdateTestAsync(It.IsAny<Test>())).Returns(Task.CompletedTask);

            await _viewModel.InvokePrivateAsync("SaveAsync");

            _testCatalogServiceMock.Verify(x => x.UpdateTestAsync(It.IsAny<Test>()), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم تحديث");
        }

        [Fact]
        public async Task SaveAsync_Without_SelectedTest_Should_Create()
        {
            _viewModel.SelectedTest = null;
            _viewModel.Code = "NEW";
            _viewModel.NameReport = "New Test";
            _viewModel.Price = 100;
            _viewModel.SelectedGroup = new TestGroup { GroupId = 1 };

            _testCatalogServiceMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>())).ReturnsAsync(new Test { TestId = 99 });

            await _viewModel.InvokePrivateAsync("SaveAsync");

            _testCatalogServiceMock.Verify(x => x.CreateTestAsync(It.IsAny<Test>()), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم إنشاء");
        }

        [Fact]
        public async Task SaveAsync_With_OutsourceData_Should_Persist_Pricing_LogicGuard()
        {
            // Function: 3.9 — Outsourced - Logic Guard: Verify pricing fields are sent to service
            // Arrange
            _viewModel.SelectedTest = null;
            _viewModel.Code = "OUT";
            _viewModel.IsSendOut = true;
            _viewModel.CostPrice = 25.5m;
            _viewModel.PatientPrice = 60.0m;

            _testCatalogServiceMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                .ReturnsAsync((Test t) => { t.TestId = 77; return t; });

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.CreateTestAsync(It.Is<Test>(t =>
                t.Code == "OUT" &&
                t.IsSendOut == true &&
                t.CostPrice == 25.5m &&
                t.PatientPrice == 60.0m
            )), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_With_OutsourceData_When_ServiceThrows_Should_Show_Error_FailureGuard()
        {
            // Function: 3.9 — Outsourced (Failure Case)
            // Arrange
            _viewModel.Code = "OUT_FAIL";
            _viewModel.IsSendOut = true;
            _testCatalogServiceMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                .ThrowsAsync(new Exception("outsource-save-failed"));

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("outsource-save-failed");
        }

        [Fact]
        public async Task SaveAsync_With_OutsourceData_And_InvalidPricing_Should_Show_Error_EdgeGuard()
        {
            // Function: 3.9 — Outsourced (Edge Case)
            // Arrange
            _viewModel.Code = "OUT_EDGE";
            _viewModel.IsSendOut = true;
            _viewModel.CostPrice = -50m; // Invalid
            _testCatalogServiceMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                .ThrowsAsync(new ArgumentException("cost cannot be negative"));

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
        }

        [Fact]
        public async Task DeleteAsync_With_Null_SelectedTest_Should_Do_Nothing()
        {
            _viewModel.SelectedTest = null;
            await _viewModel.InvokePrivateAsync("DeleteAsync");
            _testCatalogServiceMock.Verify(x => x.DeleteTestAsync(It.IsAny<int>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DeleteAsync_With_Valid_Test_Should_Delete()
        {
            _viewModel.SelectedTest = new Test { TestId = 5 };
            _testCatalogServiceMock.Setup(x => x.DeleteTestAsync(5)).Returns(Task.CompletedTask);
            _testCatalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test>());

            await _viewModel.InvokePrivateAsync("DeleteAsync");

            _testCatalogServiceMock.Verify(x => x.DeleteTestAsync(5), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم حذف");
        }

        [Fact]
        public void ClearForm_Should_Reset_All_Properties()
        {
            _viewModel.SelectedTest = new Test { TestId = 1 };
            _viewModel.Code = "TEST";
            _viewModel.NameReport = "Test Name";
            _viewModel.Price = 100;

            _viewModel.InvokePrivateAsync("ClearForm");

            _viewModel.SelectedTest.Should().BeNull();
            _viewModel.Code.Should().BeEmpty();
            _viewModel.NameReport.Should().BeEmpty();
            _viewModel.Price.Should().Be(0);
            _viewModel.CostPrice.Should().BeNull();
            _viewModel.PatientPrice.Should().BeNull();
        }

        [Fact]
        public async Task SaveAsync_When_ServiceThrows_Should_Set_ErrorMessage_FailureGuard()
        {
            // Arrange
            _viewModel.SelectedTest = null;
            _viewModel.Code = "ERR";
            _viewModel.NameReport = "Err Name";
            _viewModel.NameReceipt = "Err Receipt";
            _viewModel.Price = 10m;
            _testCatalogServiceMock
                .Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                .ThrowsAsync(new Exception("save-failed"));

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("save-failed");
        }

        [Fact]
        public void GenerateBarcode_When_CodeWhitespace_Should_Clear_BarcodeImage_EdgeGuard()
        {
            // Arrange
            _viewModel.Code = "ABC";
            _viewModel.Code.Should().Be("ABC");

            // Act
            _viewModel.Code = "   ";

            // Assert
            _viewModel.BarcodeImage.Should().BeNull();
        }
    }
}

