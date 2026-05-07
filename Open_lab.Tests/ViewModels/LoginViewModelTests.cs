using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    /// <summary>
    /// Tests for Module 10: Functions 10.4 (Record Attendance), 10.8 (Logout)
    /// ViewModel layer for LoginViewModel
    /// </summary>
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

            _userPreferenceServiceMock
                .Setup(x => x.GetRememberedUsername())
                .Returns(string.Empty);

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

        // ──────────────────────────────────────────────────────────────────
        // 10.4 — Record Attendance + 10.8 — Logout (Login flow)
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void Constructor_WhenRememberedUsernameExists_ShouldSetUsernameAndRememberMe()
        {
            // Function: 10.4 — Record Attendance (pre-fill remembered username)
            // Arrange
            _userPreferenceServiceMock
                .Setup(x => x.GetRememberedUsername())
                .Returns("john");

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
        public void LoginCommand_CanExecute_WhenNotBusy_ShouldReturnTrue()
        {
            // Function: 10.4 — Record Attendance (command availability)
            // Arrange & Act
            var canExecute = _viewModel.LoginCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public async Task LoginAsync_WithEmptyCredentials_ShouldSetValidationMessage()
        {
            // Function: 10.4 — Record Attendance (failure: empty credentials)
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
        public async Task LoginAsync_WithInvalidCredentials_ShouldSetFailureMessage()
        {
            // Function: 10.4 — Record Attendance (failure: wrong credentials)
            // Arrange
            _viewModel.Username = "user";
            _viewModel.Password = "wrong";
            _authServiceMock
                .Setup(x => x.ValidateCredentialsAsync("user", "wrong"))
                .ReturnsAsync((User?)null);

            // Act
            await _viewModel.InvokePrivateAsync("LoginAsync");

            // Assert
            _viewModel.StatusMessage.Should().Be("بيانات الدخول غير صحيحة.");
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task LoginAsync_WithValidAdminCredentials_ShouldSetSessionAndCreateAttendanceRecord()
        {
            // Function: 10.4 — Record Attendance (BR-SEC-004: login creates attendance record)
            // Arrange
            _viewModel.Username = "admin";
            _viewModel.Password = "admin123";
            var user = new User { UserId = 1, Username = "admin", IsActive = true };
            var permissions = new[] { PermissionCodes.FullAccess };
            var attendance = new AttendanceLog { AttendanceLogId = 100 };

            _authServiceMock
                .Setup(x => x.ValidateCredentialsAsync("admin", "admin123"))
                .ReturnsAsync(user);
            _adminSetupServiceMock
                .Setup(x => x.EnsureAdminAccessAsync(1))
                .Returns(Task.CompletedTask);
            _authorizationServiceMock
                .Setup(x => x.GetPermissionCodesAsync(1))
                .ReturnsAsync(permissions);
            _attendanceServiceMock
                .Setup(x => x.CreateLoginAsync(1, It.IsAny<string>()))
                .ReturnsAsync(attendance);

            // Act
            await _viewModel.InvokePrivateAsync("LoginAsync");

            // Assert
            AppSession.UserId.Should().Be(1);
            AppSession.Username.Should().Be("admin");
            AppSession.IsAdmin.Should().BeTrue();
            AppSession.AttendanceLogId.Should().Be(100);
            _attendanceServiceMock.Verify(
                x => x.CreateLoginAsync(1, It.IsAny<string>()), Times.Once);
            _onLoginSuccessMock.Verify(x => x(), Times.Once);
            _viewModel.IsBusy.Should().BeFalse();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithRememberMe_ShouldSaveUsername()
        {
            // Function: 10.4 — Record Attendance (remember me preference)
            // Arrange
            _viewModel.Username = "john";
            _viewModel.Password = "pass";
            _viewModel.RememberMe = true;
            var user = new User { UserId = 2, Username = "john", IsActive = true };
            var permissions = new List<string> { PermissionCodes.PatientsView };
            var attendance = new AttendanceLog { AttendanceLogId = 200 };

            _authServiceMock
                .Setup(x => x.ValidateCredentialsAsync("john", "pass"))
                .ReturnsAsync(user);
            _authorizationServiceMock
                .Setup(x => x.GetPermissionCodesAsync(2))
                .ReturnsAsync(permissions);
            _attendanceServiceMock
                .Setup(x => x.CreateLoginAsync(2, It.IsAny<string>()))
                .ReturnsAsync(attendance);

            // Act
            await _viewModel.InvokePrivateAsync("LoginAsync");

            // Assert
            _userPreferenceServiceMock.Verify(x => x.SetRememberedUsername("john"), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithoutRememberMe_ShouldClearSavedUsername()
        {
            // Function: 10.8 — Logout (clear remembered username on opt-out)
            // Arrange
            _viewModel.Username = "john";
            _viewModel.Password = "pass";
            _viewModel.RememberMe = false;
            var user = new User { UserId = 2, Username = "john", IsActive = true };
            var permissions = new List<string> { PermissionCodes.PatientsView };
            var attendance = new AttendanceLog { AttendanceLogId = 200 };

            _authServiceMock
                .Setup(x => x.ValidateCredentialsAsync("john", "pass"))
                .ReturnsAsync(user);
            _authorizationServiceMock
                .Setup(x => x.GetPermissionCodesAsync(2))
                .ReturnsAsync(permissions);
            _attendanceServiceMock
                .Setup(x => x.CreateLoginAsync(2, It.IsAny<string>()))
                .ReturnsAsync(attendance);

            // Act
            await _viewModel.InvokePrivateAsync("LoginAsync");

            // Assert
            _userPreferenceServiceMock.Verify(x => x.SetRememberedUsername(null), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void TogglePasswordVisibilityCommand_ShouldToggleIsPasswordVisible()
        {
            // Function: 10.8 — Logout (password visibility toggle for login screen)
            // Arrange
            _viewModel.IsPasswordVisible = false;

            // Act
            _viewModel.TogglePasswordVisibilityCommand.Execute(null);

            // Assert
            _viewModel.IsPasswordVisible.Should().BeTrue();
        }

        [Fact]
        public async Task LoginCommand_WhenAuthServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.4 — Record Attendance (failure: auth service error)
            // Arrange
            _viewModel.Username = "user";
            _viewModel.Password = "pass";
            _authServiceMock
                .Setup(x => x.ValidateCredentialsAsync("user", "pass"))
                .ThrowsAsync(new InvalidOperationException("auth-failed"));

            // Act
            _viewModel.LoginCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("auth-failed");
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task LoginCommand_WhenAdminSetupThrows_ShouldSetErrorMessage()
        {
            // Function: 10.4 — Record Attendance (failure: admin setup error)
            // Arrange
            _viewModel.Username = "admin";
            _viewModel.Password = "admin123";
            var user = new User { UserId = 1, Username = "admin", IsActive = true };

            _authServiceMock
                .Setup(x => x.ValidateCredentialsAsync("admin", "admin123"))
                .ReturnsAsync(user);
            _adminSetupServiceMock
                .Setup(x => x.EnsureAdminAccessAsync(1))
                .ThrowsAsync(new InvalidOperationException("admin-setup-failed"));

            // Act
            _viewModel.LoginCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("admin-setup-failed");
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public void TogglePasswordVisibilityCommand_WhenExecutedTwice_ShouldReturnToInitialState()
        {
            // Function: 10.8 — Logout (edge: double-toggle returns to original state)
            // Arrange
            _viewModel.IsPasswordVisible = false;

            // Act
            _viewModel.TogglePasswordVisibilityCommand.Execute(null);
            _viewModel.TogglePasswordVisibilityCommand.Execute(null);

            // Assert
            _viewModel.IsPasswordVisible.Should().BeFalse();
        }
    }
}
