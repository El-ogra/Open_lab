using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class PhysicianViewModelTests : IDisposable
    {
        private readonly Mock<IPhysicianService> _physicianServiceMock = new();
        private readonly Mock<ITestCatalogService> _catalogServiceMock = new();
        private readonly PhysicianViewModel _viewModel;

        public PhysicianViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();

            _physicianServiceMock.Setup(x => x.GetActiveAsync()).ReturnsAsync(new List<Physician>());
            _physicianServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Physician>());
            _catalogServiceMock.Setup(x => x.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());

            _viewModel = new PhysicianViewModel(_physicianServiceMock.Object, _catalogServiceMock.Object);
        }

        public void Dispose() => AppSessionTestHelper.Reset();

        [Fact]
        public async Task SaveAsync_Should_Create_Physician_For_Module12_6_12_7()
        {
            // Function: 12.6 — `Add Referring Physician`
            // Arrange
            // Act
            _viewModel.FullName = "Dr. New";
            _viewModel.Specialty = "Lab";
            _viewModel.PriceListId = 4;
            _viewModel.CommissionPercentage = 12;

            _physicianServiceMock.Setup(x => x.CreateAsync(It.IsAny<Physician>()))
                .ReturnsAsync(new Physician { PhysicianId = 11, FullName = "Dr. New", PriceListId = 4, CommissionPercentage = 12 });

            await _viewModel.InvokePrivateAsync("SaveAsync");

            _physicianServiceMock.Verify(x => x.CreateAsync(It.Is<Physician>(p =>
                p.FullName == "Dr. New" &&
                p.PriceListId == 4 &&
                p.CommissionPercentage == 12)), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SearchAsync_Should_Replace_List_With_Search_Results()
        {
            // Function: 12.6 — `Add Referring Physician`
            // Arrange
            // Act
            _viewModel.SearchTerm = "john";
            _physicianServiceMock
                .Setup(x => x.SearchAsync("john"))
                .ReturnsAsync(new List<Physician> { new() { PhysicianId = 1, FullName = "Dr. John" } });

            await _viewModel.InvokePrivateAsync("SearchAsync");

            // Assert
            _viewModel.Physicians.Should().ContainSingle();
            _viewModel.Physicians[0].FullName.Should().Be("Dr. John");
        }

        [Fact]
        public void SaveCommand_When_FullNameEmpty_Should_Be_Disabled_FailureGuard()
        {
            // Function: 12.6 — `Add Referring Physician`
            // Arrange
            _viewModel.FullName = " ";

            // Act
            var canExecute = _viewModel.SaveCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public void SearchCommand_When_SearchTermWhitespace_Should_Be_Disabled_EdgeGuard()
        {
            // Function: 12.6 — `Add Referring Physician`
            // Arrange
            _viewModel.SearchTerm = " ";

            // Act
            var canExecute = _viewModel.SearchCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public async Task LoadPhysiciansCommand_When_Executed_Should_Load_Active_Physicians_Success()
        {
            // Function: 12.6 — `Add Referring Physician`
            // Arrange
            _physicianServiceMock.Setup(x => x.GetActiveAsync()).ReturnsAsync(new List<Physician>
            {
                new() { PhysicianId = 1, FullName = "Dr. A", IsActive = true }
            });

            // Act
            _viewModel.LoadPhysiciansCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Physicians.Should().ContainSingle(p => p.PhysicianId == 1);
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 طبيب");
        }

        [Fact]
        public void NewCommand_When_Executed_Should_Clear_Editor_Edge()
        {
            // Function: 12.6 — `Add Referring Physician`
            // Arrange
            _viewModel.FullName = "Dr. X";
            _viewModel.Specialty = "Lab";
            _viewModel.PriceListId = 3;

            // Act
            _viewModel.NewCommand.Execute(null);

            // Assert
            _viewModel.FullName.Should().BeEmpty();
            _viewModel.Specialty.Should().BeNull();
            _viewModel.PriceListId.Should().BeNull();
        }

        [Fact]
        public async Task SearchCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 12.6 — `Add Referring Physician`
            // Arrange
            _viewModel.SearchTerm = "john";
            _physicianServiceMock.Setup(x => x.SearchAsync("john"))
                .ThrowsAsync(new InvalidOperationException("search-failed"));

            // Act
            _viewModel.SearchCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("search-failed");
        }
    }
}
