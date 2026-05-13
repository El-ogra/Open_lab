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

namespace Open_lab.Tests.Services
{
    public class UnitTestCoverageEnforcementTests : IDisposable
    {
        private readonly Mock<ISystemSettingsService> _settingsServiceMock;
        private readonly SystemSettingsViewModel _viewModel;

        public UnitTestCoverageEnforcementTests()
        {
            AppSessionTestHelper.ResetToAdmin();

            _settingsServiceMock = new Mock<ISystemSettingsService>();
            _settingsServiceMock.Setup(x => x.GetProfileAsync()).ReturnsAsync(new SystemSettingsProfile());
            _settingsServiceMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new List<Setting>());
            _settingsServiceMock.Setup(x => x.VerifyMasterPasswordAsync(It.IsAny<string>())).ReturnsAsync(true);
            _settingsServiceMock.Setup(x => x.SetMasterPasswordAsync(It.IsAny<string>())).ReturnsAsync(true);

            _viewModel = new SystemSettingsViewModel(_settingsServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task ChangeMasterPasswordCommand_When_CurrentPasswordMissing_Should_Not_Verify_Or_Set_Password()
        {
            // Function: 13.8 — Set System Password (BR-SEC-003)
            // Arrange
            await Task.Delay(50);
            _viewModel.CurrentMasterPassword = string.Empty;
            _viewModel.NewMasterPassword = "NewPass#2026";
            _viewModel.ConfirmMasterPassword = "NewPass#2026";

            // Act
            _viewModel.ChangeMasterPasswordCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsServiceMock.Verify(x => x.VerifyMasterPasswordAsync(It.IsAny<string>()), Times.Never);
            _settingsServiceMock.Verify(x => x.SetMasterPasswordAsync(It.IsAny<string>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("كلمة المرور الحالية");
        }

        [Fact]
        public async Task ChangeMasterPasswordCommand_When_SetPasswordFails_Should_Show_Failure_And_Keep_Fields()
        {
            // Function: 13.8 — Set System Password (BR-SEC-003)
            // Arrange
            await Task.Delay(50);
            _settingsServiceMock.Setup(x => x.VerifyMasterPasswordAsync("current")).ReturnsAsync(true);
            _settingsServiceMock.Setup(x => x.SetMasterPasswordAsync("NewPass#2026")).ReturnsAsync(false);
            _viewModel.CurrentMasterPassword = "current";
            _viewModel.NewMasterPassword = "NewPass#2026";
            _viewModel.ConfirmMasterPassword = "NewPass#2026";

            // Act
            _viewModel.ChangeMasterPasswordCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsServiceMock.Verify(x => x.VerifyMasterPasswordAsync("current"), Times.Once);
            _settingsServiceMock.Verify(x => x.SetMasterPasswordAsync("NewPass#2026"), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تعذر تحديث كلمة المرور");
            _viewModel.CurrentMasterPassword.Should().Be("current");
            _viewModel.NewMasterPassword.Should().Be("NewPass#2026");
            _viewModel.ConfirmMasterPassword.Should().Be("NewPass#2026");
        }
    }
}
