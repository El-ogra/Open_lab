using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class PriceListsViewModelTests : IDisposable
    {
        private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
        private readonly Mock<IPrintService> _printServiceMock;
        private readonly PriceListsViewModel _viewModel;

        public PriceListsViewModelTests()
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
        public async Task LinkPriceListToEntity_WithSelectedReferral_ShouldCreatePriceListWithReferralId()
        {
            // Function: 12.2 — Link Price List to Entity
            // Arrange
            _viewModel.ListName = "Corporate List";
            _viewModel.SelectedReferral = new Referral { ReferralId = 5, Name = "Ref1" };
            _viewModel.IsDefault = true;

            _testCatalogServiceMock.Setup(x => x.CreatePriceListAsync(It.IsAny<PriceList>()))
                .ReturnsAsync(new PriceList { PriceListId = 1, Name = "Corporate List", ReferralId = 5 });

            // Act
            await _viewModel.InvokePrivateAsync("SaveListAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.CreatePriceListAsync(It.Is<PriceList>(pl =>
                pl.Name == "Corporate List" && pl.ReferralId == 5 && pl.IsDefault == true
            )), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LinkPriceListToEntity_WhenServiceRejectsReferral_ShouldShowErrorMessage()
        {
            // Function: 12.2 — Link Price List to Entity
            // Arrange
            _viewModel.ListName = "Rejected Referral List";
            _viewModel.SelectedReferral = new Referral { ReferralId = 77, Name = "Rejected" };

            _testCatalogServiceMock
                .Setup(x => x.CreatePriceListAsync(It.IsAny<PriceList>()))
                .ThrowsAsync(new InvalidOperationException("invalid referral"));

            // Act
            await _viewModel.InvokePrivateAsync("SaveListAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.CreatePriceListAsync(It.Is<PriceList>(pl =>
                pl.Name == "Rejected Referral List" && pl.ReferralId == 77)), Times.Once);
            _viewModel.StatusMessage.Should().Contain("invalid referral");
        }

        [Fact]
        public async Task LinkPriceListToEntity_WithGeneralList_ShouldCreatePriceListWithoutReferralId()
        {
            // Function: 12.2 — Link Price List to Entity
            // Arrange
            _viewModel.ListName = "General List";
            _viewModel.SelectedReferral = new Referral { ReferralId = 0, Name = "عام (بدون جهة)" };

            _testCatalogServiceMock
                .Setup(x => x.CreatePriceListAsync(It.IsAny<PriceList>()))
                .ReturnsAsync((PriceList list) => new PriceList
                {
                    PriceListId = 10,
                    Name = list.Name,
                    ReferralId = list.ReferralId
                });

            // Act
            await _viewModel.InvokePrivateAsync("SaveListAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.CreatePriceListAsync(It.Is<PriceList>(pl =>
                pl.Name == "General List" && pl.ReferralId == null)), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم حفظ قائمة الأسعار");
        }

        [Fact]
        public async Task SaveListAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
        {
            // Function: 3.7 — Create Price List (Failure Case)
            // Arrange
            _viewModel.ListName = "Fail List";
            _testCatalogServiceMock.Setup(x => x.CreatePriceListAsync(It.IsAny<PriceList>()))
                .ThrowsAsync(new Exception("list-create-failed"));

            // Act
            await _viewModel.InvokePrivateAsync("SaveListAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("list-create-failed");
        }

        [Fact]
        public async Task SaveListAsync_When_DuplicateName_Should_Show_Error_EdgeGuard()
        {
            // Function: 3.7 — Create Price List (Edge Case)
            // Arrange
            _viewModel.ListName = "Duplicate";
            _testCatalogServiceMock.Setup(x => x.CreatePriceListAsync(It.IsAny<PriceList>()))
                .ThrowsAsync(new InvalidOperationException("name exists"));

            // Act
            await _viewModel.InvokePrivateAsync("SaveListAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("name exists");
        }

        [Fact]
        public async Task AddItemAsync_Should_Add_Test_To_PriceList()
        {
            // Function: 3.8 — Update Prices - Logic Guard
            // Arrange
            _viewModel.SelectedPriceList = new PriceList { PriceListId = 1 };
            _viewModel.SelectedTest = new Test { TestId = 10, Code = "T10", Price = 100m };
            _viewModel.Price = 80m; // Discounted Price for list

            _testCatalogServiceMock.Setup(x => x.AddPriceListItemAsync(It.IsAny<PriceListItem>()))
                .ReturnsAsync(new PriceListItem { PriceListItemId = 1, PriceListId = 1, TestId = 10, Price = 80m });

            // Act
            await _viewModel.InvokePrivateAsync("AddItemAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.AddPriceListItemAsync(It.Is<PriceListItem>(item =>
                item.PriceListId == 1 && item.TestId == 10 && item.Price == 80m
            )), Times.Once);
            
            _viewModel.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task AddItemAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
        {
            // Function: 3.8 — Update Prices (Failure Case)
            // Arrange
            _viewModel.SelectedPriceList = new PriceList { PriceListId = 1 };
            _viewModel.SelectedTest = new Test { TestId = 1 };
            _testCatalogServiceMock.Setup(x => x.AddPriceListItemAsync(It.IsAny<PriceListItem>()))
                .ThrowsAsync(new Exception("add-item-failed"));

            // Act
            await _viewModel.InvokePrivateAsync("AddItemAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("add-item-failed");
        }

        [Fact]
        public async Task AddItemAsync_WithNegativePrice_Should_Fallback_To_TestPrice_EdgeGuard()
        {
            // Function: 3.8 — Update Prices (Edge Case)
            // Arrange
            _viewModel.SelectedPriceList = new PriceList { PriceListId = 1 };
            _viewModel.SelectedTest = new Test { TestId = 1, Price = 100m };
            _viewModel.Price = -10m; // Negative

            _testCatalogServiceMock.Setup(x => x.AddPriceListItemAsync(It.IsAny<PriceListItem>()))
                .ReturnsAsync(new PriceListItem { PriceListItemId = 10, Price = 100m });

            // Act
            await _viewModel.InvokePrivateAsync("AddItemAsync");

            // Assert
            // The VM logic (line 261) says: Price > 0 ? Price : SelectedTest.Price
            // So if Price is -10, it should use 100m.
            _testCatalogServiceMock.Verify(x => x.AddPriceListItemAsync(It.Is<PriceListItem>(i => i.Price == 100m)), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SaveListAsync_With_EmptyName_Should_NotCall_Service_FailureGuard()
        {
            // Function: 3.8 — Update Prices (Edge Case)
            // Arrange
            _viewModel.ListName = " ";

            // Act
            await _viewModel.InvokePrivateAsync("SaveListAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.CreatePriceListAsync(It.IsAny<PriceList>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("أدخل اسم القائمة");
        }

        [Fact]
        public async Task AddItemAsync_When_PriceZero_Should_Fallback_To_TestPrice_EdgeGuard()
        {
            // Function: 3.8 — Update Prices (Edge Case)
            // Arrange
            _viewModel.SelectedPriceList = new PriceList { PriceListId = 2 };
            _viewModel.SelectedTest = new Test { TestId = 20, Code = "T20", Price = 135m };
            _viewModel.Price = 0m;

            _testCatalogServiceMock
                .Setup(x => x.AddPriceListItemAsync(It.IsAny<PriceListItem>()))
                .ReturnsAsync((PriceListItem item) => new PriceListItem
                {
                    PriceListItemId = 2,
                    PriceListId = item.PriceListId,
                    TestId = item.TestId,
                    Price = item.Price
                });

            // Act
            await _viewModel.InvokePrivateAsync("AddItemAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.AddPriceListItemAsync(It.Is<PriceListItem>(i =>
                i.PriceListId == 2 && i.TestId == 20 && i.Price == 135m)), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }
    }
}
