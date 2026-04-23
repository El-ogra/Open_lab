using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class CustomGroupsViewModelTests : IDisposable
    {
        private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
        private readonly CustomGroupsViewModel _viewModel;

        public CustomGroupsViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _testCatalogServiceMock = new Mock<ITestCatalogService>();
            _viewModel = new CustomGroupsViewModel(_testCatalogServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task SaveGroupAsync_Should_Create_Group()
        {
            // 3.5 Custom Group - Logic Guard
            // Arrange
            _viewModel.GroupName = "Profile Test";
            _viewModel.GroupPrice = 150m;

            _testCatalogServiceMock.Setup(x => x.CreateCustomGroupAsync(It.IsAny<CustomGroup>()))
                .ReturnsAsync(new CustomGroup { CustomGroupId = 1, Name = "Profile Test", Price = 150m });

            // Act
            await _viewModel.InvokePrivateAsync("SaveGroupAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.CreateCustomGroupAsync(It.Is<CustomGroup>(g =>
                g.Name == "Profile Test" && g.Price == 150m
            )), Times.Once);
            
            _viewModel.Groups.Should().HaveCount(1);
        }

        [Fact]
        public async Task AddItemAsync_Should_Link_Test_To_Group()
        {
            // Arrange
            var group = new CustomGroup { CustomGroupId = 10 };
            _viewModel.SelectedGroup = group;
            var test = new Test { TestId = 5, Code = "T5" };
            _viewModel.SelectedTest = test;

            _testCatalogServiceMock.Setup(x => x.AddCustomGroupItemAsync(It.IsAny<CustomGroupItem>()))
                .ReturnsAsync(new CustomGroupItem { CustomGroupItemId = 1, CustomGroupId = 10, TestId = 5 });

            // Act
            await _viewModel.InvokePrivateAsync("AddItemAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.AddCustomGroupItemAsync(It.Is<CustomGroupItem>(item =>
                item.CustomGroupId == 10 && item.TestId == 5
            )), Times.Once);
            
            _viewModel.GroupItems.Should().HaveCount(1);
        }
    }
}
