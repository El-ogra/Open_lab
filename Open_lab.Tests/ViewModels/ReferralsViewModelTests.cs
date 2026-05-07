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
            // Function: 12.1 — `Create Contract Entity`
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
            // Function: 12.3 — `Set Entity Discount`
            _viewModel.Name = "Company Y";
            _viewModel.Type = "Company";
            _viewModel.DiscountPercentage = 120;

            await _viewModel.InvokePrivateAsync("SaveAsync");

            _catalogMock.Verify(x => x.CreateReferralAsync(It.IsAny<Referral>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("نسبة الخصم");
        }

        [Fact]
        public async Task SaveAsync_Without_NameOrType_Should_Stop_Validation_FailureGuard()
        {
            // Function: 12.3 — `Set Entity Discount`
            // Arrange
            _viewModel.Name = "";
            _viewModel.Type = "";

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _catalogMock.Verify(x => x.CreateReferralAsync(It.IsAny<Referral>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("الاسم والنوع");
        }

        [Fact]
        public async Task SaveAsync_Invalid_CommissionPercentage_Should_Stop_Creation_EdgeGuard()
        {
            // Function: 12.3 — `Set Entity Discount`
            // Arrange
            _viewModel.Name = "Company Z";
            _viewModel.Type = "Company";
            _viewModel.DiscountPercentage = 10;
            _viewModel.CommissionPercentage = 120;

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _catalogMock.Verify(x => x.CreateReferralAsync(It.IsAny<Referral>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("نسبة العمولة");
        }

        [Fact]
        public async Task SaveCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 12.3 — `Set Entity Discount`
            // Arrange
            _viewModel.Name = "Company Err";
            _viewModel.Type = "Company";
            _viewModel.DiscountPercentage = 0;
            _viewModel.CommissionPercentage = 0;
            _catalogMock.Setup(x => x.CreateReferralAsync(It.IsAny<Referral>()))
                .ThrowsAsync(new InvalidOperationException("save-failed"));

            // Act
            _viewModel.SaveCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("save-failed");
        }

        [Fact]
        public async Task DeleteCommand_When_SelectedReferral_Exists_Should_Delete_Success()
        {
            // Function: 12.3 — `Set Entity Discount`
            // Arrange
            var referral = new Referral { ReferralId = 9, Name = "ToDelete", ReferralType = "Company" };
            _viewModel.Referrals.Add(referral);
            _viewModel.SelectedReferral = referral;
            _catalogMock.Setup(x => x.DeleteReferralAsync(9)).Returns(Task.CompletedTask);

            // Act
            _viewModel.DeleteCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _catalogMock.Verify(x => x.DeleteReferralAsync(9), Times.Once);
            _viewModel.Referrals.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Be("تم حذف الجهة.");
        }

        [Fact]
        public async Task DeleteCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 12.3 — `Set Entity Discount`
            // Arrange
            var referral = new Referral { ReferralId = 10, Name = "Fail", ReferralType = "Company" };
            _viewModel.Referrals.Add(referral);
            _viewModel.SelectedReferral = referral;
            _catalogMock.Setup(x => x.DeleteReferralAsync(10))
                .ThrowsAsync(new InvalidOperationException("delete-failed"));

            // Act
            _viewModel.DeleteCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("delete-failed");
        }
    }
}
