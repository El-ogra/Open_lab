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
        public async Task SaveListAsync_Should_Create_PriceList()
        {
            // 3.7 Price List - Logic Guard
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
        }

        [Fact]
        public async Task AddItemAsync_Should_Add_Test_To_PriceList()
        {
            // 3.8 Update Prices - Logic Guard
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
    }
}
