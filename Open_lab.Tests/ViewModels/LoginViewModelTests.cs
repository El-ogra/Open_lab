using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class LoginViewModelTests : IDisposable
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<IAdminSetupService> _adminSetupServiceMock;
        private readonly Mock<IAttendanceService> _attendanceServiceMock;
        private readonly Mock<IUserPreferenceService> _userPreferenceServiceMock;
        private readonly Mock<Action> _onLoginSuccessMock;
        private readonly LoginViewModel _viewModel;

        public LoginViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _authServiceMock = new Mock<IAuthService>();
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _adminSetupServiceMock = new Mock<IAdminSetupService>();
            _attendanceServiceMock = new Mock<IAttendanceService>();
            _userPreferenceServiceMock = new Mock<IUserPreferenceService>();
            _onLoginSuccessMock = new Mock<Action>();

            _userPreferenceServiceMock.Setup(x => x.GetRememberedUsername()).Returns(string.Empty);

            _viewModel = new LoginViewModel(
                _authServiceMock.Object,
                _authorizationServiceMock.Object,
                _adminSetupServiceMock.Object,
                _attendanceServiceMock.Object,
                _userPreferenceServiceMock.Object,
                _onLoginSuccessMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public void Constructor_When_RememberedUsername_Exists_Should_Set_Username_And_RememberMe()
        {
            // Arrange
            _userPreferenceServiceMock.Setup(x => x.GetRememberedUsername()).Returns("john");

            // Act
            var vm = new LoginViewModel(
                _authServiceMock.Object,
                _authorizationServiceMock.Object,
                _adminSetupServiceMock.Object,
                _attendanceServiceMock.Object,
                _userPreferenceServiceMock.Object,
                _onLoginSuccessMock.Object);

            // Assert
            vm.Username.Should().Be("john");
            vm.RememberMe.Should().BeTrue();
        }

        [Fact]
        public void LoginCommand_CanExecute_Should_Return_True()
        {
            // Act
            var canExecute = _viewModel.LoginCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public async Task LoginAsync_With_Empty_Username_Should_Set_StatusMessage()
        {
            // Arrange
            _viewModel.Username = "";
            _viewModel.Password = "";

            // Act
            await _viewModel.InvokePrivateAsync("LoginAsync");

            // Assert
            _viewModel.StatusMessage.Should().Be("يرجى إدخال اسم المستخدم وكلمة المرور.");
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task LoginAsync_With_Invalid_Credentials_Should_Set_StatusMessage()
        {
            // Arrange
            _viewModel.Username = "user";
            _viewModel.Password = "wrong";
            _authServiceMock.Setup(x => x.ValidateCredentialsAsync("user", "wrong"))
                .ReturnsAsync((User?)null);

            // Act
            await _viewModel.InvokePrivateAsync("LoginAsync");

            // Assert
            _viewModel.StatusMessage.Should().Be("بيانات الدخول غير صحيحة.");
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task LoginAsync_With_Valid_Credentials_Should_Set_Session_And_Invoke_Success()
        {
            // Arrange
            _viewModel.Username = "admin";
            _viewModel.Password = "admin123";
            var user = new User { UserId = 1, Username = "admin", IsActive = true };
            var permissions = new[] { PermissionCodes.FullAccess };
            var attendance = new AttendanceLog { AttendanceLogId = 100 };

            _authServiceMock.Setup(x => x.ValidateCredentialsAsync("admin", "admin123"))
                .ReturnsAsync(user);
            _adminSetupServiceMock.Setup(x => x.EnsureAdminAccessAsync(1)).Returns(Task.CompletedTask);
            _authorizationServiceMock.Setup(x => x.GetPermissionCodesAsync(1)).ReturnsAsync(permissions);
            _attendanceServiceMock.Setup(x => x.CreateLoginAsync(1, It.IsAny<string>())).ReturnsAsync(attendance);

            // Act
            await _viewModel.InvokePrivateAsync("LoginAsync");

            // Assert
            AppSession.UserId.Should().Be(1);
            AppSession.Username.Should().Be("admin");
            AppSession.IsAdmin.Should().BeTrue();
            AppSession.AttendanceLogId.Should().Be(100);
            _onLoginSuccessMock.Verify(x => x(), Times.Once);
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task LoginAsync_With_RememberMe_Should_Save_Username()
        {
            // Arrange
            _viewModel.Username = "john";
            _viewModel.Password = "pass";
            _viewModel.RememberMe = true;
            var user = new User { UserId = 2, Username = "john", IsActive = true };
            var permissions = new[] { PermissionCodes.PatientsView };
            var attendance = new AttendanceLog { AttendanceLogId = 200 };

            _authServiceMock.Setup(x => x.ValidateCredentialsAsync("john", "pass"))
                .ReturnsAsync(user);
            _authorizationServiceMock.Setup(x => x.GetPermissionCodesAsync(2)).ReturnsAsync(permissions);
            _attendanceServiceMock.Setup(x => x.CreateLoginAsync(2, It.IsAny<string>())).ReturnsAsync(attendance);

            // Act
            await _viewModel.InvokePrivateAsync("LoginAsync");

            // Assert
            _userPreferenceServiceMock.Verify(x => x.SetRememberedUsername("john"), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_Without_RememberMe_Should_Clear_Username()
        {
            // Arrange
            _viewModel.Username = "john";
            _viewModel.Password = "pass";
            _viewModel.RememberMe = false;
            var user = new User { UserId = 2, Username = "john", IsActive = true };
            var permissions = new[] { PermissionCodes.PatientsView };
            var attendance = new AttendanceLog { AttendanceLogId = 200 };

            _authServiceMock.Setup(x => x.ValidateCredentialsAsync("john", "pass"))
                .ReturnsAsync(user);
            _authorizationServiceMock.Setup(x => x.GetPermissionCodesAsync(2)).ReturnsAsync(permissions);
            _attendanceServiceMock.Setup(x => x.CreateLoginAsync(2, It.IsAny<string>())).ReturnsAsync(attendance);

            // Act
            await _viewModel.InvokePrivateAsync("LoginAsync");

            // Assert
            _userPreferenceServiceMock.Verify(x => x.SetRememberedUsername(null), Times.Once);
        }

        [Fact]
        public void TogglePasswordVisibilityCommand_Should_Toggle_IsPasswordVisible()
        {
            // Arrange
            _viewModel.IsPasswordVisible = false;

            // Act
            _viewModel.TogglePasswordVisibilityCommand.Execute(null);

            // Assert
            _viewModel.IsPasswordVisible.Should().BeTrue();
        }
    }
}
