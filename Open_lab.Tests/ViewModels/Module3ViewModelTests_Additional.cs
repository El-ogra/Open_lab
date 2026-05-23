using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    /// <summary>
    /// Additional comprehensive tests for Module 3 ViewModels
    /// Covers: TestCatalogViewModel, ReferenceRangesViewModel, CustomGroupsViewModel, PriceListsViewModel, TestCommentsViewModel
    /// </summary>
    public class Module3ViewModelTests_Additional : IDisposable
    {
        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        #region TestCatalogViewModel Additional Tests

        public class TestCatalogViewModel_AdditionalTests : IDisposable
        {
            private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
            private readonly Mock<IBarcodeService> _barcodeServiceMock;
            private readonly TestCatalogViewModel _viewModel;

            public TestCatalogViewModel_AdditionalTests()
            {
                AppSessionTestHelper.ResetToAdmin();
                _testCatalogServiceMock = new Mock<ITestCatalogService>();
                _barcodeServiceMock = new Mock<IBarcodeService>();

                _testCatalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test>());
                _testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync()).ReturnsAsync(new List<TestGroup>());
                _testCatalogServiceMock.Setup(x => x.GetSampleTypesAsync()).ReturnsAsync(new List<SampleType>());
                _testCatalogServiceMock.Setup(x => x.GetUnitsAsync()).ReturnsAsync(new List<Unit>());

                _viewModel = new TestCatalogViewModel(_testCatalogServiceMock.Object, _barcodeServiceMock.Object);
            }

            public void Dispose()
            {
                AppSessionTestHelper.Reset();
            }

            [Fact]
            public void LoadFromSelected_Should_Map_All_Fields_SuccessGuard()
            {
                // Function: 3.1 — Add New Test (Complete Fields)
                // Arrange
                var test = new Test
                {
                    TestId = 1,
                    Code = "HBA",
                    NameReport = "Hemoglobin A",
                    NameReceipt = "HB A Receipt",
                    Price = 75m,
                    TurnaroundHours = 24,
                    ReportOrder = 5,
                    IsRoutine = true,
                    IsSendOut = false,
                    CostPrice = 20m,
                    PatientPrice = 55m,
                    GroupId = 3,
                    SampleTypeId = 2,
                    UnitId = 1
                };

                // Act
                _viewModel.SelectedTest = test;

                // Assert
                _viewModel.Code.Should().Be("HBA");
                _viewModel.NameReport.Should().Be("Hemoglobin A");
                _viewModel.NameReceipt.Should().Be("HB A Receipt");
                _viewModel.Price.Should().Be(75m);
                _viewModel.TurnaroundHours.Should().Be(24);
                _viewModel.ReportOrder.Should().Be(5);
                _viewModel.IsRoutine.Should().BeTrue();
                _viewModel.IsSendOut.Should().BeFalse();
                _viewModel.CostPrice.Should().Be(20m);
                _viewModel.PatientPrice.Should().Be(55m);
            }

            [Fact]
            public void LoadFromSelected_When_Null_Should_Do_Nothing_EdgeGuard()
            {
                // Function: 3.1 — Add New Test (Complete Fields)
                // Arrange
                _viewModel.Code = "ORIG";
                _viewModel.NameReport = "Original";

                // Act
                _viewModel.SelectedTest = null;

                // Assert - values should remain unchanged
                _viewModel.Code.Should().Be("ORIG");
                _viewModel.NameReport.Should().Be("Original");
            }

            [Fact]
            public async Task SaveAsync_With_AllFields_Should_Persist_Correctly_SuccessGuard()
            {
                // Function: 3.1 — Add New Test (Complete Fields)
                // Arrange
                _viewModel.Code = "TSH";
                _viewModel.NameReport = "Thyroid Stimulating Hormone";
                _viewModel.NameReceipt = "TSH Receipt";
                _viewModel.Price = 120m;
                _viewModel.TurnaroundHours = 48;
                _viewModel.ReportOrder = 10;
                _viewModel.IsRoutine = true;
                _viewModel.IsSendOut = false;
                _viewModel.CostPrice = 30m;
                _viewModel.PatientPrice = 90m;

                _testCatalogServiceMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                    .ReturnsAsync((Test t) => { t.TestId = 200; return t; });

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.CreateTestAsync(It.Is<Test>(t =>
                    t.Code == "TSH" &&
                    t.NameReport == "Thyroid Stimulating Hormone" &&
                    t.Price == 120m &&
                    t.TurnaroundHours == 48
                )), Times.Once);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task ReferenceRanges_SaveAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
            {
                // Function: 3.1 — Add New Test (Failure: service error)
                // Arrange
                _viewModel.Code = "ERR";
                _viewModel.NameReport = "Error Test";
                _testCatalogServiceMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                    .ThrowsAsync(new Exception("db-error"));

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
                _viewModel.StatusMessage.Should().Contain("db-error");
            }

            [Fact]
            public async Task SaveAsync_With_DuplicateCode_Should_Show_Error_EdgeGuard()
            {
                // Function: 3.1 — Add New Test (Edge: duplicate code)
                // Arrange
                _viewModel.Code = "DUP";
                _viewModel.NameReport = "Duplicate Test";
                _testCatalogServiceMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                    .ThrowsAsync(new InvalidOperationException("code exists"));

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
                _viewModel.StatusMessage.Should().Contain("code exists");
            }

            [Fact]
            public async Task SaveAsync_With_IsSendOut_True_Should_Include_Pricing_Fields_BR_ACC_007()
            {
                // Function: 3.9 — Mark as Outsourced (BR-ACC-007 Logic Guard)
                // Arrange
                _viewModel.Code = "OUTSRC";
                _viewModel.NameReport = "Outsourced Test";
                _viewModel.IsSendOut = true;
                _viewModel.CostPrice = 15.50m;
                _viewModel.PatientPrice = 45.00m;
                _viewModel.Price = 0m;

                _testCatalogServiceMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                    .ReturnsAsync((Test t) => { t.TestId = 201; return t; });

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.CreateTestAsync(It.Is<Test>(t =>
                    t.IsSendOut == true &&
                    t.CostPrice == 15.50m &&
                    t.PatientPrice == 45.00m
                )), Times.Once);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task SaveAsync_With_Null_Optional_Fields_Should_Succeed_EdgeGuard()
            {
                // Function: 3.9 — Mark as Outsourced (BR-ACC-007 Logic Guard)
                // Arrange
                _viewModel.Code = "MIN";
                _viewModel.NameReport = "Minimal Test";
                _viewModel.CostPrice = null;
                _viewModel.PatientPrice = null;
                _viewModel.SelectedGroup = null;
                _viewModel.SelectedSampleType = null;
                _viewModel.SelectedUnit = null;

                _testCatalogServiceMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                    .ReturnsAsync((Test t) => { t.TestId = 202; return t; });

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.CreateTestAsync(It.Is<Test>(t =>
                    t.GroupId == null &&
                    t.SampleTypeId == null &&
                    t.UnitId == null
                )), Times.Once);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task LoadAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
            {
                // Function: 3.9 — Mark as Outsourced (BR-ACC-007 Logic Guard)
                // Arrange
                _testCatalogServiceMock.Setup(x => x.GetAllTestsAsync())
                    .ThrowsAsync(new Exception("db-connection-failed"));

                // Act
                await _viewModel.InvokePrivateAsync("LoadAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
                _viewModel.StatusMessage.Should().Contain("db-connection-failed");
            }

            [Fact]
            public async Task TestCatalog_DeleteAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
            {
                // Function: 3.9 — Mark as Outsourced (BR-ACC-007 Logic Guard)
                // Arrange
                _viewModel.SelectedTest = new Test { TestId = 50, Code = "DEL" };
                _testCatalogServiceMock.Setup(x => x.DeleteTestAsync(50))
                    .ThrowsAsync(new Exception("delete-failed"));

                // Act
                await _viewModel.InvokePrivateAsync("DeleteAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
                _viewModel.StatusMessage.Should().Contain("delete-failed");
            }

            [Fact]
            public void GenerateBarcode_When_Code_Whitespace_Should_Clear_Barcode_EdgeGuard()
            {
                // Function: 3.9 — Mark as Outsourced (BR-ACC-007 Logic Guard)
                // Arrange
                _barcodeServiceMock.Setup(x => x.GenerateCode128(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(It.IsAny<System.Windows.Media.ImageSource>());

                _viewModel.Code = "TEST";

                // Act
                _viewModel.Code = "   ";

                // Assert
                _viewModel.BarcodeImage.Should().BeNull();
            }

            [Fact]
            public async Task UpdateTestAsync_Should_Modify_SelectedTest_Fields_SuccessGuard()
            {
                // Function: 3.2 — Edit Test Data (BR-VAL-005 Logic Guard)
                // Arrange
                var existingTest = new Test { TestId = 100, Code = "OLD", NameReport = "Old Name", Price = 50m };
                _viewModel.SelectedTest = existingTest;
                _viewModel.Code = "UPDATED";
                _viewModel.NameReport = "Updated Name";
                _viewModel.Price = 75m;

                _testCatalogServiceMock.Setup(x => x.UpdateTestAsync(It.IsAny<Test>()))
                    .Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.UpdateTestAsync(It.Is<Test>(t =>
                    t.TestId == 100 &&
                    t.Code == "UPDATED" &&
                    t.NameReport == "Updated Name" &&
                    t.Price == 75m
                )), Times.Once);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task UpdateTestAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
            {
                // Function: 3.2 — Edit Test Data (Failure: service error)
                // Arrange
                _viewModel.SelectedTest = new Test { TestId = 101, Code = "E1" };
                _viewModel.Code = "E2";
                _testCatalogServiceMock.Setup(x => x.UpdateTestAsync(It.IsAny<Test>()))
                    .ThrowsAsync(new Exception("update-failed"));

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
                _viewModel.StatusMessage.Should().Contain("update-failed");
            }

            [Fact]
            public async Task UpdateTestAsync_When_NoSelection_Should_Return_EdgeGuard()
            {
                // Function: 3.2 — Edit Test Data (Edge: no selection)
                // Arrange
                _viewModel.SelectedTest = null;
                _viewModel.Code = "NEWCODE";

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                // When SelectedTest is null, SaveAsync should call CreateTestAsync, not UpdateTestAsync.
                // We verify UpdateTestAsync is NOT called.
                _testCatalogServiceMock.Verify(x => x.UpdateTestAsync(It.IsAny<Test>()), Times.Never);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }
        }

        #endregion

        #region ReferenceRangesViewModel Additional Tests

        public class ReferenceRangesViewModel_AdditionalTests : IDisposable
        {
            private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
            private readonly ReferenceRangesViewModel _viewModel;

            public ReferenceRangesViewModel_AdditionalTests()
            {
                AppSessionTestHelper.ResetToAdmin();
                _testCatalogServiceMock = new Mock<ITestCatalogService>();

                _testCatalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test>());
                _viewModel = new ReferenceRangesViewModel(_testCatalogServiceMock.Object);
            }

            public void Dispose()
            {
                AppSessionTestHelper.Reset();
            }

            [Fact]
            public async Task LoadRangesAsync_Should_Populate_Ranges_SuccessGuard()
            {
                // Function: 3.3 — Set Reference Values (Load Guard)
                // Arrange
                var test = new Test { TestId = 1, Code = "GLU" };
                _viewModel.SelectedTest = test;

                var ranges = new List<TestReferenceRange>
                {
                    new TestReferenceRange { RangeId = 1, TestId = 1, Gender = "Male", AgeFromValue = 18, AgeFromUnit = AgeConverter.UnitYear, AgeFromDays = 18 * AgeConverter.DaysPerYear, AgeToValue = 60, AgeToUnit = AgeConverter.UnitYear, AgeToDays = 60 * AgeConverter.DaysPerYear, LowValue = 70, HighValue = 100 },
                    new TestReferenceRange { RangeId = 2, TestId = 1, Gender = "Female", AgeFromValue = 18, AgeFromUnit = AgeConverter.UnitYear, AgeFromDays = 18 * AgeConverter.DaysPerYear, AgeToValue = 60, AgeToUnit = AgeConverter.UnitYear, AgeToDays = 60 * AgeConverter.DaysPerYear, LowValue = 65, HighValue = 95 }
                };

                _testCatalogServiceMock.Setup(x => x.GetReferenceRangesAsync(1)).ReturnsAsync(ranges);

                // Act
                await _viewModel.InvokePrivateAsync("LoadRangesAsync");

                // Assert
                _viewModel.Ranges.Should().HaveCount(2);
            }

            [Fact]
            public async Task LoadRangesAsync_When_NoTest_Should_Clear_Ranges_EdgeGuard()
            {
                // Function: 3.3 — Set Reference Values (Load Guard)
                // Arrange
                _viewModel.SelectedTest = null;
                _viewModel.Ranges.Add(new TestReferenceRange { RangeId = 99 }); // Pre-existing

                // Act
                await _viewModel.InvokePrivateAsync("LoadRangesAsync");

                // Assert
                _viewModel.Ranges.Should().BeEmpty();
            }

            [Fact]
            public async Task UpdateRangeAsync_Should_Call_Update_Service_SuccessGuard()
            {
                // Function: 3.3 — Set Reference Values (Update Logic)
                // Arrange
                var test = new Test { TestId = 5, Code = "CHOL" };
                _viewModel.SelectedTest = test;

                var existingRange = new TestReferenceRange
                {
                    RangeId = 10,
                    TestId = 5,
                    Gender = "Both",
                    AgeFromValue = 20,
                    AgeFromUnit = AgeConverter.UnitYear,
                    AgeFromDays = 20 * AgeConverter.DaysPerYear,
                    AgeToValue = 70,
                    AgeToUnit = AgeConverter.UnitYear,
                    AgeToDays = 70 * AgeConverter.DaysPerYear,
                    LowValue = 0,
                    HighValue = 200
                };
                _viewModel.SelectedRange = existingRange;

                _viewModel.Gender = "Both";
                _viewModel.AgeFromValue = 20;
                _viewModel.AgeToValue = 70;
                _viewModel.LowValue = 0;
                _viewModel.HighValue = 200;
                _viewModel.NormalText = "Desirable < 200 mg/dL";

                _testCatalogServiceMock.Setup(x => x.UpdateReferenceRangeAsync(It.IsAny<TestReferenceRange>()))
                    .Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.UpdateReferenceRangeAsync(It.Is<TestReferenceRange>(r =>
                    r.RangeId == 10 &&
                    r.TestId == 5 &&
                    r.HighValue == 200
                )), Times.Once);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task DeleteAsync_Should_Remove_Range_And_Clear_Selection_SuccessGuard()
            {
                // Function: 3.3 — Set Reference Values (Update Logic)
                // Arrange
                var range = new TestReferenceRange { RangeId = 20, TestId = 3 };
                _viewModel.SelectedRange = range;

                _testCatalogServiceMock.Setup(x => x.DeleteReferenceRangeAsync(20)).Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("DeleteAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.DeleteReferenceRangeAsync(20), Times.Once);
                _viewModel.SelectedRange.Should().BeNull();
                _viewModel.Ranges.Should().BeEmpty();
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task DeleteAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
            {
                // Function: 3.3 — Set Reference Values (Update Logic)
                // Arrange
                _viewModel.SelectedRange = new TestReferenceRange { RangeId = 30 };
                _testCatalogServiceMock.Setup(x => x.DeleteReferenceRangeAsync(30))
                    .ThrowsAsync(new Exception("delete-range-failed"));

                // Act
                await _viewModel.InvokePrivateAsync("DeleteAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
                _viewModel.StatusMessage.Should().Contain("delete-range-failed");
            }

            [Fact]
            public async Task TestCatalog_SaveAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
            {
                // Function: 3.3 — Set Reference Values (Update Logic)
                // Arrange
                _viewModel.SelectedTest = new Test { TestId = 4 };
                _viewModel.Gender = "Male";
                _viewModel.LowValue = 10;
                _viewModel.HighValue = 20;

                _testCatalogServiceMock.Setup(x => x.CreateReferenceRangeAsync(It.IsAny<TestReferenceRange>()))
                    .ThrowsAsync(new Exception("create-range-failed"));

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
            }

            [Fact]
            public void LoadFromRange_Should_Set_All_Properties_SuccessGuard()
            {
                // Function: 3.3 — Set Reference Values (Update Logic)
                // Arrange
                var range = new TestReferenceRange
                {
                    RangeId = 5,
                    Gender = "Female",
                    AgeFromValue = 18,
                    AgeFromUnit = AgeConverter.UnitYear,
                    AgeFromDays = 18 * AgeConverter.DaysPerYear,
                    AgeToValue = 50,
                    AgeToUnit = AgeConverter.UnitYear,
                    AgeToDays = 50 * AgeConverter.DaysPerYear,
                    LowValue = 3.5m,
                    HighValue = 5.0m,
                    NormalText = "Normal thyroid function"
                };

                // Act
                _viewModel.SelectedRange = range;

                // Assert
                _viewModel.Gender.Should().Be("Female");
                _viewModel.AgeFromValue.Should().Be(18);
                _viewModel.AgeToValue.Should().Be(50);
                _viewModel.LowValue.Should().Be(3.5m);
                _viewModel.HighValue.Should().Be(5.0m);
                _viewModel.NormalText.Should().Be("Normal thyroid function");
            }
        }

        #endregion

        #region CustomGroupsViewModel Additional Tests

        public class CustomGroupsViewModel_AdditionalTests : IDisposable
        {
            private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
            private readonly CustomGroupsViewModel _viewModel;

            public CustomGroupsViewModel_AdditionalTests()
            {
                AppSessionTestHelper.ResetToAdmin();
                _testCatalogServiceMock = new Mock<ITestCatalogService>();

                _testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync()).ReturnsAsync(new List<CustomGroup>());
                _testCatalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test>());

                _viewModel = new CustomGroupsViewModel(_testCatalogServiceMock.Object);
            }

            public void Dispose()
            {
                AppSessionTestHelper.Reset();
            }

            [Fact]
            public async Task SaveGroupAsync_With_Zero_Price_Should_Allow_EdgeGuard()
            {
                // Function: 3.5 — Create Custom Group (Zero Price Edge Case)
                // Arrange
                _viewModel.GroupName = "Free Screening";
                _viewModel.GroupPrice = 0m;

                _testCatalogServiceMock.Setup(x => x.CreateCustomGroupAsync(It.IsAny<CustomGroup>()))
                    .ReturnsAsync((CustomGroup g) => { g.CustomGroupId = 1; return g; });

                // Act
                await _viewModel.InvokePrivateAsync("SaveGroupAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.CreateCustomGroupAsync(It.Is<CustomGroup>(g =>
                    g.Name == "Free Screening" && g.Price == 0m
                )), Times.Once);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task SaveGroupAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
            {
                // Function: 3.5 — Create Custom Group (Failure: service error)
                // Arrange
                _viewModel.GroupName = "Error Group";
                _testCatalogServiceMock.Setup(x => x.CreateCustomGroupAsync(It.IsAny<CustomGroup>()))
                    .ThrowsAsync(new Exception("group-error"));

                // Act
                await _viewModel.InvokePrivateAsync("SaveGroupAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
                _viewModel.StatusMessage.Should().Contain("group-error");
            }

            [Fact]
            public async Task CustomGroups_DeleteItemAsync_Should_Remove_Item_SuccessGuard()
            {
                // Function: 3.5 — Create Custom Group (Delete Item)
                // Arrange
                var group = new CustomGroup { CustomGroupId = 5 };
                _viewModel.SelectedGroup = group;

                var item = new CustomGroupItem { CustomGroupItemId = 10, CustomGroupId = 5, TestId = 3 };
                _viewModel.SelectedItem = item;

                _testCatalogServiceMock.Setup(x => x.DeleteCustomGroupItemAsync(10)).Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("DeleteItemAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.DeleteCustomGroupItemAsync(10), Times.Once);
                _viewModel.GroupItems.Should().BeEmpty();
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task CustomGroups_DeleteItemAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
            {
                // Function: 3.5 — Create Custom Group (Delete Item)
                // Arrange
                _viewModel.SelectedItem = new CustomGroupItem { CustomGroupItemId = 15 };
                _testCatalogServiceMock.Setup(x => x.DeleteCustomGroupItemAsync(15))
                    .ThrowsAsync(new Exception("delete-item-failed"));

                // Act
                await _viewModel.InvokePrivateAsync("DeleteItemAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
                _viewModel.StatusMessage.Should().Contain("delete-item-failed");
            }

            [Fact]
            public async Task LoadItemsAsync_Should_Populate_GroupItems_SuccessGuard()
            {
                // Function: 3.5 — Create Custom Group (Delete Item)
                // Arrange
                var group = new CustomGroup { CustomGroupId = 7 };
                _viewModel.SelectedGroup = group;

                var items = new List<CustomGroupItem>
                {
                    new CustomGroupItem { CustomGroupItemId = 1, CustomGroupId = 7, TestId = 1 },
                    new CustomGroupItem { CustomGroupItemId = 2, CustomGroupId = 7, TestId = 2 }
                };

                _testCatalogServiceMock.Setup(x => x.GetCustomGroupItemsAsync(7)).ReturnsAsync(items);

                // Act
                await _viewModel.InvokePrivateAsync("LoadItemsAsync");

                // Assert
                _viewModel.GroupItems.Should().HaveCount(2);
            }

            [Fact]
            public async Task LoadItemsAsync_When_No_Group_Should_Clear_EdgeGuard()
            {
                // Function: 3.5 — Create Custom Group (Delete Item)
                // Arrange
                _viewModel.SelectedGroup = null;
                _viewModel.GroupItems.Add(new CustomGroupItem { CustomGroupItemId = 99 });

                // Act
                await _viewModel.InvokePrivateAsync("LoadItemsAsync");

                // Assert
                _viewModel.GroupItems.Should().BeEmpty();
            }

            [Fact]
            public async Task AddItemAsync_With_Existing_Item_Should_Set_Test_Reference_SuccessGuard()
            {
                // Function: 3.5 — Create Custom Group (Delete Item)
                // Arrange
                var group = new CustomGroup { CustomGroupId = 10 };
                _viewModel.SelectedGroup = group;

                var test = new Test { TestId = 25, Code = "T25", NameReport = "Test 25" };
                _viewModel.SelectedTest = test;

                var addedItem = new CustomGroupItem
                {
                    CustomGroupItemId = 100,
                    CustomGroupId = 10,
                    TestId = 25,
                    Test = test
                };

                _testCatalogServiceMock.Setup(x => x.AddCustomGroupItemAsync(It.IsAny<CustomGroupItem>()))
                    .ReturnsAsync(addedItem);

                // Act
                await _viewModel.InvokePrivateAsync("AddItemAsync");

                // Assert
                _viewModel.GroupItems.Should().HaveCount(1);
                _viewModel.GroupItems.First().Test.Should().NotBeNull();
            }
        }

        #endregion

        #region PriceListsViewModel Additional Tests

        public class PriceListsViewModel_AdditionalTests : IDisposable
        {
            private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
            private readonly Mock<IPrintService> _printServiceMock;
            private readonly PriceListsViewModel _viewModel;

            public PriceListsViewModel_AdditionalTests()
            {
                AppSessionTestHelper.ResetToAdmin();
                _testCatalogServiceMock = new Mock<ITestCatalogService>();
                _printServiceMock = new Mock<IPrintService>();

                _testCatalogServiceMock.Setup(x => x.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
                _testCatalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test>());
                _testCatalogServiceMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral>());

                _viewModel = new PriceListsViewModel(_testCatalogServiceMock.Object, _printServiceMock.Object);
            }

            public void Dispose()
            {
                AppSessionTestHelper.Reset();
            }

            [Fact]
            public async Task UpdateListAsync_Should_Call_Update_Service_SuccessGuard()
            {
                // Function: 3.7 — Create Price List (Update Logic)
                // Arrange
                var priceList = new PriceList { PriceListId = 1, Name = "Old Name" };
                _viewModel.SelectedPriceList = priceList;
                _viewModel.ListName = "New Name";
                _viewModel.IsDefault = true;

                _testCatalogServiceMock.Setup(x => x.UpdatePriceListAsync(It.IsAny<PriceList>()))
                    .Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("UpdateListAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.UpdatePriceListAsync(It.Is<PriceList>(pl =>
                    pl.PriceListId == 1 && pl.Name == "New Name"
                )), Times.Once);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task UpdateItemAsync_Should_Change_Price_SuccessGuard()
            {
                // Function: 3.8 — Update Prices (Update Item Logic)
                // Arrange
                var item = new PriceListItem { PriceListItemId = 5, Price = 50m };
                _viewModel.SelectedItem = item;
                _viewModel.Price = 45m;

                _testCatalogServiceMock.Setup(x => x.UpdatePriceListItemAsync(It.IsAny<PriceListItem>()))
                    .Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("UpdateItemAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.UpdatePriceListItemAsync(It.Is<PriceListItem>(i =>
                    i.PriceListItemId == 5 && i.Price == 45m
                )), Times.Once);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task PriceLists_DeleteItemAsync_Should_Remove_Item_SuccessGuard()
            {
                // Function: 3.8 — Update Prices (Update Item Logic)
                // Arrange
                var item = new PriceListItem { PriceListItemId = 10 };
                _viewModel.SelectedItem = item;

                _testCatalogServiceMock.Setup(x => x.DeletePriceListItemAsync(10)).Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("DeleteItemAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.DeletePriceListItemAsync(10), Times.Once);
                _viewModel.SelectedItem.Should().BeNull();
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task PriceLists_DeleteItemAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
            {
                // Function: 3.8 — Update Prices (Update Item Logic)
                // Arrange
                _viewModel.SelectedItem = new PriceListItem { PriceListItemId = 20 };
                _testCatalogServiceMock.Setup(x => x.DeletePriceListItemAsync(20))
                    .ThrowsAsync(new Exception("delete-item-failed"));

                // Act
                await _viewModel.InvokePrivateAsync("DeleteItemAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
            }

            [Fact]
            public async Task PrintListAsync_Should_Call_PrintService_SuccessGuard()
            {
                // Function: 3.7 — Create Price List (Print Logic)
                // Arrange
                var priceList = new PriceList { PriceListId = 1, Name = "Test List", IsDefault = false };
                _viewModel.SelectedPriceList = priceList;
                _viewModel.Items.Add(new PriceListItem { PriceListItemId = 1, TestId = 1, Price = 100m });
                _viewModel.SelectedReferral = new Referral { ReferralId = 0, Name = "عام (بدون جهة)", ReferralType = "General" };

                _printServiceMock.Setup(x => x.PrintTextReportAsync(
                    It.IsAny<string>(),
                    It.IsAny<ObservableCollection<string>>(),
                    It.IsAny<string>()
                )).Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("PrintListAsync");

                // Assert
                _printServiceMock.Verify(x => x.PrintTextReportAsync(
                    "قائمة الأسعار",
                    It.IsAny<ObservableCollection<string>>(),
                    "PriceList_1"
                ), Times.Once);
                (_viewModel.StatusMessage.Contains("تم") || _viewModel.StatusMessage.Contains("print") || _viewModel.StatusMessage.Contains("طباع")).Should().BeTrue();
            }

            [Fact]
            public async Task PrintListAsync_When_NoItems_Should_Not_Print_EdgeGuard()
            {
                // Function: 3.7 — Create Price List (Print Logic)
                // Arrange
                // The print-prevention guard for an empty price list is enforced through the
                // PrintListCommand's CanExecute predicate (Items.Count > 0). Invoking the private
                // PrintListAsync method directly would bypass that gate, so the edge-case is
                // verified at the public command boundary that real users actually trigger.
                _viewModel.SelectedPriceList = new PriceList { PriceListId = 2 };
                _viewModel.Items.Clear(); // No items

                // Act
                var canExecute = _viewModel.PrintListCommand.CanExecute(null);
                if (canExecute)
                {
                    _viewModel.PrintListCommand.Execute(null);
                    await Task.Delay(50);
                }

                // Assert
                canExecute.Should().BeFalse("the print command must be disabled when there are no items to print");
                _printServiceMock.Verify(x => x.PrintTextReportAsync(
                    It.IsAny<string>(),
                    It.IsAny<ObservableCollection<string>>(),
                    It.IsAny<string>()
                ), Times.Never);
                _viewModel.StatusMessage.Should().NotBeNull();
            }

            [Fact]
            public async Task LoadItemsAsync_Should_Clear_And_Populate_SuccessGuard()
            {
                // Function: 3.7 — Create Price List (Print Logic)
                // Arrange
                var priceList = new PriceList { PriceListId = 3 };
                _viewModel.SelectedPriceList = priceList;

                var items = new List<PriceListItem>
                {
                    new PriceListItem { PriceListItemId = 1, PriceListId = 3, TestId = 1, Price = 50m },
                    new PriceListItem { PriceListItemId = 2, PriceListId = 3, TestId = 2, Price = 75m }
                };

                _testCatalogServiceMock.Setup(x => x.GetPriceListItemsAsync(3)).ReturnsAsync(items);

                // Act
                await _viewModel.InvokePrivateAsync("LoadItemsAsync");

                // Assert
                _viewModel.Items.Should().HaveCount(2);
            }

            [Fact]
            public async Task UpdateListAsync_When_Null_Selected_Should_Do_Nothing_EdgeGuard()
            {
                // Function: 3.7 — Create Price List (Print Logic)
                // Arrange
                _viewModel.SelectedPriceList = null;
                _viewModel.ListName = "Test";

                // Act
                await _viewModel.InvokePrivateAsync("UpdateListAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.UpdatePriceListAsync(It.IsAny<PriceList>()), Times.Never);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public void FindReferral_Should_Return_Matching_Referral_SuccessGuard()
            {
                // Function: 3.7 — Create Price List (Print Logic)
                // Arrange
                var referral = new Referral { ReferralId = 5, Name = "Insurance Co" };
                _viewModel.Referrals.Add(referral);

                // Act
                var found = _viewModel.InvokePrivateMethod<Referral>("FindReferral", 5);

                // Assert
                found.Should().NotBeNull();
                found!.Name.Should().Be("Insurance Co");
            }
        }

        #endregion

        #region TestCommentsViewModel Additional Tests

        public class TestCommentsViewModel_AdditionalTests : IDisposable
        {
            private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
            private readonly TestCommentsViewModel _viewModel;

            public TestCommentsViewModel_AdditionalTests()
            {
                AppSessionTestHelper.ResetToAdmin();
                _testCatalogServiceMock = new Mock<ITestCatalogService>();

                _testCatalogServiceMock.Setup(x => x.GetAllTestsAsync()).ReturnsAsync(new List<Test>());
                _viewModel = new TestCommentsViewModel(_testCatalogServiceMock.Object);
            }

            public void Dispose()
            {
                AppSessionTestHelper.Reset();
            }

            [Fact]
            public async Task SaveAsync_Update_Existing_Comment_Should_Call_Update_Service_SuccessGuard()
            {
                // Function: 3.6 — Add Test Comments
                // Arrange
                var test = new Test { TestId = 1 };
                _viewModel.SelectedTest = test;

                var existingComment = new TestComment { CommentId = 10, TestId = 1 };
                _viewModel.SelectedComment = existingComment;

                _viewModel.CommentText = "Updated Comment";
                _viewModel.LowComment = "Low Update";
                _viewModel.HighComment = "High Update";

                _testCatalogServiceMock.Setup(x => x.UpdateTestCommentAsync(It.IsAny<TestComment>()))
                    .Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.UpdateTestCommentAsync(It.Is<TestComment>(c =>
                    c.CommentId == 10 &&
                    c.CommentText == "Updated Comment" &&
                    c.LowComment == "Low Update" &&
                    c.HighComment == "High Update"
                )), Times.Once);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task SaveAsync_With_Empty_Comment_Text_Should_NotCall_Service_EdgeGuard()
            {
                // Function: 3.6 — Add Test Comments
                // Arrange
                var test = new Test { TestId = 1 };
                _viewModel.SelectedTest = test;
                _viewModel.CommentText = "   ";
                _viewModel.LowComment = "Low";

                _testCatalogServiceMock.Setup(x => x.CreateTestCommentAsync(It.IsAny<TestComment>()))
                    .ReturnsAsync((TestComment c) => { c.CommentId = 1; return c; });

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.CreateTestCommentAsync(It.IsAny<TestComment>()), Times.Never);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task DeleteAsync_With_Valid_Selection_Should_Remove_Comment_SuccessGuard()
            {
                // Function: 3.6 — Add Test Comments
                // Arrange
                var comment = new TestComment { CommentId = 20, TestId = 1, CommentText = "Delete Me" };
                _viewModel.SelectedComment = comment;

                _testCatalogServiceMock.Setup(x => x.DeleteTestCommentAsync(20)).Returns(Task.CompletedTask);

                // Act
                await _viewModel.InvokePrivateAsync("DeleteAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.DeleteTestCommentAsync(20), Times.Once);
                _viewModel.Comments.Should().BeEmpty();
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task DeleteAsync_When_Null_Selection_Should_Do_Nothing_EdgeGuard()
            {
                // Function: 3.6 — Add Test Comments
                // Arrange
                _viewModel.SelectedComment = null;

                // Act
                await _viewModel.InvokePrivateAsync("DeleteAsync");

                // Assert
                _testCatalogServiceMock.Verify(x => x.DeleteTestCommentAsync(It.IsAny<int>()), Times.Never);
                _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            }

            [Fact]
            public async Task LoadCommentsAsync_Should_Populate_Comments_SuccessGuard()
            {
                // Function: 3.6 — Add Test Comments
                // Arrange
                var test = new Test { TestId = 5 };
                _viewModel.SelectedTest = test;

                var comments = new List<TestComment>
                {
                    new TestComment { CommentId = 1, TestId = 5, CommentText = "Comment 1" },
                    new TestComment { CommentId = 2, TestId = 5, CommentText = "Comment 2" }
                };

                _testCatalogServiceMock.Setup(x => x.GetTestCommentsAsync(5)).ReturnsAsync(comments);

                // Act
                await _viewModel.InvokePrivateAsync("LoadCommentsAsync");

                // Assert
                _viewModel.Comments.Should().HaveCount(2);
            }

            [Fact]
            public async Task LoadCommentsAsync_When_No_Test_Should_Clear_EdgeGuard()
            {
                // Function: 3.6 — Add Test Comments
                // Arrange
                _viewModel.SelectedTest = null;
                _viewModel.Comments.Add(new TestComment { CommentId = 99 });

                // Act
                await _viewModel.InvokePrivateAsync("LoadCommentsAsync");

                // Assert
                _viewModel.Comments.Should().BeEmpty();
            }

            [Fact]
            public async Task TestComments_SaveAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
            {
                // Function: 3.6 — Add Test Comments
                // Arrange
                _viewModel.SelectedTest = new Test { TestId = 1 };
                _viewModel.CommentText = "Test Comment";
                _viewModel.LowComment = "Low";
                _viewModel.HighComment = "High";

                _testCatalogServiceMock.Setup(x => x.CreateTestCommentAsync(It.IsAny<TestComment>()))
                    .ThrowsAsync(new Exception("create-comment-failed"));

                // Act
                await _viewModel.InvokePrivateAsync("SaveAsync");

                // Assert
                _viewModel.StatusMessage.Should().Contain("خطأ:");
            }
        }

        #endregion
    }
}
