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
        }

        [Fact]
        public async Task SearchAsync_Should_Replace_List_With_Search_Results()
        {
            _viewModel.SearchTerm = "john";
            _physicianServiceMock
                .Setup(x => x.SearchAsync("john"))
                .ReturnsAsync(new List<Physician> { new() { PhysicianId = 1, FullName = "Dr. John" } });

            await _viewModel.InvokePrivateAsync("SearchAsync");

            _viewModel.Physicians.Should().ContainSingle();
            _viewModel.Physicians[0].FullName.Should().Be("Dr. John");
        }
    }
}
