using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class SystemSettingsViewModelTests : IDisposable
    {
        private readonly Mock<ISystemSettingsService> _settingsServiceMock;
        private readonly SystemSettingsViewModel _viewModel;

        public SystemSettingsViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();

            _settingsServiceMock = new Mock<ISystemSettingsService>();
            _settingsServiceMock.Setup(x => x.GetProfileAsync())
                .ReturnsAsync(new SystemSettingsProfile
                {
                    ReportHeader = "Open_lab",
                    ReportFooter = "Footer",
                    ReportPaperSize = "A5",
                    DefaultAccountType = "Credit"
                });
            _settingsServiceMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new List<Setting>());
            _settingsServiceMock.Setup(x => x.SaveProfileAsync(It.IsAny<SystemSettingsProfile>())).Returns(Task.CompletedTask);
            _settingsServiceMock.Setup(x => x.SaveSettingAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _settingsServiceMock.Setup(x => x.DeleteSettingAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _settingsServiceMock.Setup(x => x.VerifyMasterPasswordAsync(It.IsAny<string>())).ReturnsAsync(true);
            _settingsServiceMock.Setup(x => x.SetMasterPasswordAsync(It.IsAny<string>())).ReturnsAsync(true);

            _viewModel = new SystemSettingsViewModel(_settingsServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadAsync_Should_Load_PaperSize_And_DefaultAccountType()
        {
            // 13.2 + 13.4
            await _viewModel.InvokePrivateAsync("LoadAsync");

            _viewModel.ReportPaperSize.Should().Be("A5");
            _viewModel.DefaultAccountType.Should().Be("Credit");
        }

        [Fact]
        public async Task SaveProfileAsync_Should_Persist_PaperSize_And_DefaultAccountType()
        {
            // 13.2 + 13.4
            _viewModel.ReportPaperSize = "A4";
            _viewModel.DefaultAccountType = "Cash";

            await _viewModel.InvokePrivateAsync("SaveProfileAsync");

            _settingsServiceMock.Verify(x => x.SaveProfileAsync(It.Is<SystemSettingsProfile>(p =>
                p.ReportPaperSize == "A4" && p.DefaultAccountType == "Cash")), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ChangeMasterPasswordAsync_When_ConfirmationMismatch_Should_Not_Call_Service()
        {
            // 13.8
            _viewModel.CurrentMasterPassword = "old";
            _viewModel.NewMasterPassword = "new1";
            _viewModel.ConfirmMasterPassword = "new2";

            await _viewModel.InvokePrivateAsync("ChangeMasterPasswordAsync");

            _settingsServiceMock.Verify(x => x.VerifyMasterPasswordAsync(It.IsAny<string>()), Times.Never);
            _settingsServiceMock.Verify(x => x.SetMasterPasswordAsync(It.IsAny<string>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("غير مطابق");
        }

        [Fact]
        public async Task ChangeMasterPasswordAsync_With_Valid_CurrentPassword_Should_Update_And_Clear_Fields()
        {
            // 13.8
            _viewModel.CurrentMasterPassword = "old";
            _viewModel.NewMasterPassword = "newStrong";
            _viewModel.ConfirmMasterPassword = "newStrong";

            await _viewModel.InvokePrivateAsync("ChangeMasterPasswordAsync");

            _settingsServiceMock.Verify(x => x.VerifyMasterPasswordAsync("old"), Times.Once);
            _settingsServiceMock.Verify(x => x.SetMasterPasswordAsync("newStrong"), Times.Once);
            _viewModel.CurrentMasterPassword.Should().BeEmpty();
            _viewModel.NewMasterPassword.Should().BeEmpty();
            _viewModel.ConfirmMasterPassword.Should().BeEmpty();
        }
    }
}
