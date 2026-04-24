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
    public class ReferralsViewModelTests : IDisposable
    {
        private readonly Mock<ITestCatalogService> _catalogMock = new();
        private readonly ReferralsViewModel _viewModel;

        public ReferralsViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _catalogMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            _viewModel = new ReferralsViewModel(_catalogMock.Object);
        }

        public void Dispose() => AppSessionTestHelper.Reset();

        [Fact]
        public async Task SaveAsync_Should_Create_Referral_For_Module12_1()
        {
            _viewModel.Name = "Company X";
            _viewModel.Type = "Company";
            _viewModel.DiscountPercentage = 10;
            _viewModel.CommissionPercentage = 5;

            _catalogMock.Setup(x => x.CreateReferralAsync(It.IsAny<Referral>()))
                .ReturnsAsync(new Referral
                {
                    ReferralId = 7,
                    Name = "Company X",
                    ReferralType = "Company",
                    DiscountPercentage = 10,
                    CommissionPercentage = 5
                });

            await _viewModel.InvokePrivateAsync("SaveAsync");

            _catalogMock.Verify(x => x.CreateReferralAsync(It.Is<Referral>(r =>
                r.Name == "Company X" &&
                r.ReferralType == "Company" &&
                r.DiscountPercentage == 10 &&
                r.CommissionPercentage == 5)), Times.Once);
            _viewModel.Referrals.Should().ContainSingle();
        }

        [Fact]
        public async Task SaveAsync_Invalid_Discount_Should_Stop_For_Module12_3()
        {
            _viewModel.Name = "Company Y";
            _viewModel.Type = "Company";
            _viewModel.DiscountPercentage = 120;

            await _viewModel.InvokePrivateAsync("SaveAsync");

            _catalogMock.Verify(x => x.CreateReferralAsync(It.IsAny<Referral>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("نسبة الخصم");
        }
    }
}
