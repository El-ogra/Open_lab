using System;
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

        [Fact]
        public async Task ReloadCommand_When_Executed_Should_Refresh_Profile_And_Settings_Success()
        {
            // Act
            _viewModel.ReloadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsServiceMock.Verify(x => x.GetProfileAsync(), Times.AtLeastOnce);
            _settingsServiceMock.Verify(x => x.GetSettingsAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SaveRawSettingCommand_When_Key_Empty_Should_Set_Validation_Message_Failure()
        {
            // Arrange
            _viewModel.Key = "";
            _viewModel.Value = "v";

            // Act
            _viewModel.SaveRawSettingCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsServiceMock.Verify(x => x.SaveSettingAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("أدخل المفتاح");
        }

        [Fact]
        public async Task SaveRawSettingCommand_When_Key_Valid_Should_Call_Service_Success()
        {
            // Arrange
            _viewModel.Key = "K1";
            _viewModel.Value = "V1";

            // Act
            _viewModel.SaveRawSettingCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsServiceMock.Verify(x => x.SaveSettingAsync("K1", "V1"), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم حفظ الإعداد المتقدم");
        }

        [Fact]
        public async Task DeleteRawSettingCommand_When_SelectedSetting_Exists_Should_Call_Service_Success()
        {
            // Arrange
            _viewModel.SelectedSetting = new Setting { Key = "X", Value = "1" };

            // Act
            _viewModel.DeleteRawSettingCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsServiceMock.Verify(x => x.DeleteSettingAsync("X"), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم حذف الإعداد المتقدم");
        }

        [Fact]
        public async Task ChangeMasterPasswordCommand_When_CurrentPassword_Invalid_Should_Set_User_Message_Failure()
        {
            // Arrange
            _settingsServiceMock.Setup(x => x.VerifyMasterPasswordAsync("old")).ReturnsAsync(false);
            _viewModel.CurrentMasterPassword = "old";
            _viewModel.NewMasterPassword = "new123";
            _viewModel.ConfirmMasterPassword = "new123";

            // Act
            _viewModel.ChangeMasterPasswordCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsServiceMock.Verify(x => x.SetMasterPasswordAsync(It.IsAny<string>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("غير صحيحة");
        }
    }
}
