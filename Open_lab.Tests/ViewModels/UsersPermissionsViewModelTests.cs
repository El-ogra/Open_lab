using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// Tests for Module 10: Functions 10.1 (Create User), 10.2 (Set Permissions), 10.3 (Edit User Data)
    /// ViewModel layer for UsersPermissionsViewModel
    /// </summary>
    public class UsersPermissionsViewModelTests : IDisposable
    {
        private readonly Mock<IUserAdminService> _userAdminServiceMock = new();
        private readonly UsersPermissionsViewModel _viewModel;

        public UsersPermissionsViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _userAdminServiceMock.Setup(x => x.GetUsersAsync()).ReturnsAsync(new List<User>());
            _userAdminServiceMock.Setup(x => x.GetRolesAsync()).ReturnsAsync(new List<Role>());
            _userAdminServiceMock
                .Setup(x => x.GetRolePermissionCodesAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<string>());

            _viewModel = new UsersPermissionsViewModel(_userAdminServiceMock.Object);
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.1 — Create User
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SaveUserCommand_WhenNewUserWithValidData_ShouldCreateUserAndAddToList()
        {
            // Function: 10.1 — Create User
            // Arrange
            _viewModel.Username = "newuser";
            _viewModel.FullName = "New Employee";
            _viewModel.Password = "pass";
            _viewModel.IsActive = true;
            _userAdminServiceMock
                .Setup(x => x.CreateUserAsync(It.IsAny<User>(), "pass"))
                .ReturnsAsync(new User
                {
                    UserId = 9,
                    Username = "newuser",
                    FullName = "New Employee",
                    IsActive = true
                });

            // Act
            _viewModel.SaveUserCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Users.Should().ContainSingle();
            _viewModel.SelectedUser.Should().NotBeNull();
            _viewModel.SelectedUser!.UserId.Should().Be(9);
            _viewModel.StatusMessage.Should().Be("تم إنشاء المستخدم.");
            _userAdminServiceMock.Verify(
                x => x.CreateUserAsync(It.IsAny<User>(), "pass"), Times.Once);
        }

        [Fact]
        public async Task SaveUserCommand_WhenUsernameIsEmpty_ShouldSetValidationMessage()
        {
            // Function: 10.1 — Create User (failure: empty username)
            // Arrange
            _viewModel.Username = " ";

            // Act
            _viewModel.SaveUserCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Be("أدخل اسم المستخدم.");
            _userAdminServiceMock.Verify(
                x => x.CreateUserAsync(It.IsAny<User>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task SaveUserCommand_WhenServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.1 — Create User (failure: service error)
            // Arrange
            _viewModel.Username = "dup_user";
            _userAdminServiceMock
                .Setup(x => x.CreateUserAsync(It.IsAny<User>(), It.IsAny<string?>()))
                .ThrowsAsync(new InvalidOperationException("اسم المستخدم موجود بالفعل."));

            // Act
            _viewModel.SaveUserCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().NotBeEmpty();
            _viewModel.StatusMessage.Should().Contain("موجود");
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.3 — Edit User Data
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SaveUserCommand_WhenExistingUserSelected_ShouldUpdateUserData()
        {
            // Function: 10.3 — Edit User Data
            // Arrange
            var existing = new User
            {
                UserId = 5,
                Username = "olduser",
                FullName = "Old Name",
                IsActive = true
            };
            _viewModel.Users.Add(existing);
            _viewModel.SelectedUser = existing;
            _viewModel.Username = "updated_user";
            _viewModel.FullName = "Updated Name";
            _viewModel.Password = "newpass";
            _userAdminServiceMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<User>(), "newpass"))
                .Returns(Task.CompletedTask);
            _userAdminServiceMock
                .Setup(x => x.GetUsersAsync())
                .ReturnsAsync(new List<User> { existing });
            _userAdminServiceMock
                .Setup(x => x.GetRolesAsync())
                .ReturnsAsync(new List<Role>());

            // Act
            _viewModel.SaveUserCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _userAdminServiceMock.Verify(
                x => x.UpdateUserAsync(
                    It.Is<User>(u => u.UserId == 5 && u.Username == "updated_user"),
                    "newpass"),
                Times.Once);
            _viewModel.StatusMessage.Should().Be("تم تحديث المستخدم.");
        }

        [Fact]
        public async Task SaveUserCommand_WhenUpdateServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.3 — Edit User Data (failure: update error)
            // Arrange
            var existing = new User { UserId = 7, Username = "edit_user", IsActive = true };
            _viewModel.Users.Add(existing);
            _viewModel.SelectedUser = existing;
            _viewModel.Username = "admin";
            _userAdminServiceMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<User>(), It.IsAny<string?>()))
                .ThrowsAsync(new InvalidOperationException("لا يمكن تغيير اسم مستخدم admin."));

            // Act
            _viewModel.SaveUserCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().NotBeEmpty();
            _viewModel.StatusMessage.Should().Contain("admin");
        }

        // ──────────────────────────────────────────────────────────────────
        // Delete User — extends 10.1
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteUserCommand_WhenSelectedUserExists_ShouldCallServiceAndShowSuccess()
        {
            // Function: 10.1 — Create User (delete path)
            // Arrange
            var user = new User { UserId = 10, Username = "delete-me" };
            _viewModel.SelectedUser = user;
            _userAdminServiceMock
                .Setup(x => x.DeleteUserAsync(10))
                .Returns(Task.CompletedTask);
            _userAdminServiceMock
                .Setup(x => x.GetUsersAsync())
                .ReturnsAsync(new List<User>());
            _userAdminServiceMock
                .Setup(x => x.GetRolesAsync())
                .ReturnsAsync(new List<Role>());

            // Act
            _viewModel.DeleteUserCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _userAdminServiceMock.Verify(x => x.DeleteUserAsync(10), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم حذف المستخدم.");
        }

        [Fact]
        public async Task DeleteUserCommand_WhenServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.1 — Create User (failure: delete protected user)
            // Arrange
            _viewModel.SelectedUser = new User { UserId = 10, Username = "delete-me" };
            _userAdminServiceMock
                .Setup(x => x.DeleteUserAsync(10))
                .ThrowsAsync(new InvalidOperationException("لا يمكن حذف حساب admin."));

            // Act
            _viewModel.DeleteUserCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("admin");
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.2 — Set Permissions
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SaveRolePermissionsCommand_WhenRoleSelected_ShouldSendGrantedCodes()
        {
            // Function: 10.2 — Set Permissions (BR-SEC-001)
            // Arrange
            var role = new Role { RoleId = 4, RoleName = "Reception" };
            _userAdminServiceMock
                .Setup(x => x.GetRolePermissionCodesAsync(role.RoleId))
                .ReturnsAsync(new List<string> { PermissionCodes.UsersView });

            _viewModel.SelectedRole = role;
            await _viewModel.InvokePrivateAsync("LoadPermissionsAsync");

            var editPermission = _viewModel.Permissions
                .First(p => p.Code == PermissionCodes.UsersEdit);
            editPermission.IsGranted = true;

            // Act
            await _viewModel.InvokePrivateAsync("SaveRolePermissionsAsync");

            // Assert
            _userAdminServiceMock.Verify(
                x => x.SaveRolePermissionsAsync(
                    role.RoleId,
                    It.Is<IEnumerable<string>>(codes =>
                        codes.Contains(PermissionCodes.UsersView) &&
                        codes.Contains(PermissionCodes.UsersEdit))),
                Times.Once);
            _viewModel.StatusMessage.Should().Be("تم حفظ الصلاحيات.");
        }

        [Fact]
        public async Task SaveRolePermissionsCommand_WhenNoRoleSelected_ShouldNotCallService()
        {
            // Function: 10.2 — Set Permissions (edge: no role selected)
            // Arrange
            _viewModel.SelectedRole = null;

            // Act
            _viewModel.SaveRolePermissionsCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _userAdminServiceMock.Verify(
                x => x.SaveRolePermissionsAsync(
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<string>>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignRoleCommand_WhenUserAndRoleSelected_ShouldCallServiceAndShowSuccess()
        {
            // Function: 10.2 — Set Permissions (BR-SEC-001: assign role to user)
            // Arrange
            _viewModel.SelectedUser = new User { UserId = 1, Username = "u" };
            _viewModel.SelectedRole = new Role { RoleId = 2, RoleName = "r" };
            _userAdminServiceMock
                .Setup(x => x.AssignSingleRoleAsync(1, 2))
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.AssignRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _userAdminServiceMock.Verify(x => x.AssignSingleRoleAsync(1, 2), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم ربط المستخدم بالدور.");
        }

        [Fact]
        public async Task AssignRoleCommand_WhenServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.2 — Set Permissions (failure: assign error)
            // Arrange
            _viewModel.SelectedUser = new User { UserId = 1, Username = "u" };
            _viewModel.SelectedRole = new Role { RoleId = 2, RoleName = "r" };
            _userAdminServiceMock
                .Setup(x => x.AssignSingleRoleAsync(1, 2))
                .ThrowsAsync(new InvalidOperationException("المستخدم أو الدور غير موجود."));

            // Act
            _viewModel.AssignRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().NotBeEmpty();
        }

        [Fact]
        public async Task UnassignRoleCommand_WhenSelectedUserAndRole_ShouldCallServiceAndShowSuccess()
        {
            // Function: 10.2 — Set Permissions (revoke role)
            // Arrange
            _viewModel.SelectedUser = new User { UserId = 1, Username = "u" };
            _viewModel.SelectedRole = new Role { RoleId = 2, RoleName = "r" };
            _userAdminServiceMock
                .Setup(x => x.RemoveUserRoleAsync(1, 2))
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.UnassignRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _userAdminServiceMock.Verify(x => x.RemoveUserRoleAsync(1, 2), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم فك ربط الدور من المستخدم.");
        }

        [Fact]
        public async Task UnassignRoleCommand_WhenServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.2 — Set Permissions (failure: unlink error)
            // Arrange
            _viewModel.SelectedUser = new User { UserId = 1, Username = "u" };
            _viewModel.SelectedRole = new Role { RoleId = 2, RoleName = "r" };
            _userAdminServiceMock
                .Setup(x => x.RemoveUserRoleAsync(1, 2))
                .ThrowsAsync(new InvalidOperationException("لا يمكن فك دور admin."));

            // Act
            _viewModel.UnassignRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("admin");
        }

        [Fact]
        public async Task SaveRoleCommand_WithValidRoleName_ShouldCreateRoleAndShowSuccess()
        {
            // Function: 10.2 — Set Permissions (create role)
            // Arrange
            _viewModel.RoleName = "Reception";
            _userAdminServiceMock
                .Setup(x => x.CreateRoleAsync("Reception"))
                .ReturnsAsync(new Role { RoleId = 7, RoleName = "Reception" });

            // Act
            _viewModel.SaveRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Roles.Should().ContainSingle(r => r.RoleId == 7);
            _viewModel.StatusMessage.Should().Be("تم إنشاء الدور.");
        }

        [Fact]
        public async Task SaveRoleCommand_WhenRoleNameIsEmpty_ShouldSetValidationMessage()
        {
            // Function: 10.2 — Set Permissions (failure: empty role name)
            // Arrange
            _viewModel.RoleName = " ";

            // Act
            _viewModel.SaveRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Be("أدخل اسم الدور.");
            _userAdminServiceMock.Verify(
                x => x.CreateRoleAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SaveRoleCommand_WhenServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.2 — Set Permissions (failure: duplicate role)
            // Arrange
            _viewModel.RoleName = "Reception";
            _userAdminServiceMock
                .Setup(x => x.CreateRoleAsync("Reception"))
                .ThrowsAsync(new InvalidOperationException("اسم الدور موجود بالفعل."));

            // Act
            _viewModel.SaveRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().NotBeEmpty();
        }

        [Fact]
        public async Task DeleteRoleCommand_WhenSelectedRoleExists_ShouldCallServiceAndShowSuccess()
        {
            // Function: 10.2 — Set Permissions (delete role)
            // Arrange
            _viewModel.SelectedRole = new Role { RoleId = 20, RoleName = "r20" };
            _userAdminServiceMock
                .Setup(x => x.DeleteRoleAsync(20))
                .Returns(Task.CompletedTask);
            _userAdminServiceMock
                .Setup(x => x.GetUsersAsync())
                .ReturnsAsync(new List<User>());
            _userAdminServiceMock
                .Setup(x => x.GetRolesAsync())
                .ReturnsAsync(new List<Role>());

            // Act
            _viewModel.DeleteRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _userAdminServiceMock.Verify(x => x.DeleteRoleAsync(20), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم حذف الدور.");
        }

        [Fact]
        public async Task DeleteRoleCommand_WhenServiceThrows_ShouldSetErrorMessage()
        {
            // Function: 10.2 — Set Permissions (failure: role in use)
            // Arrange
            _viewModel.SelectedRole = new Role { RoleId = 21, RoleName = "r21" };
            _userAdminServiceMock
                .Setup(x => x.DeleteRoleAsync(21))
                .ThrowsAsync(new InvalidOperationException("لا يمكن حذف الدور لأنه مرتبط بمستخدمين."));

            // Act
            _viewModel.DeleteRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ReloadCommand_WhenInvoked_ShouldLoadUsersAndRoles()
        {
            // Function: 10.1 — Create User (edge: reload list)
            // Arrange
            _userAdminServiceMock
                .Setup(x => x.GetUsersAsync())
                .ReturnsAsync(new List<User> { new() { UserId = 8, Username = "u8" } });
            _userAdminServiceMock
                .Setup(x => x.GetRolesAsync())
                .ReturnsAsync(new List<Role> { new() { RoleId = 9, RoleName = "r9" } });

            // Act
            _viewModel.ReloadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Users.Should().ContainSingle(u => u.UserId == 8);
            _viewModel.Roles.Should().ContainSingle(r => r.RoleId == 9);
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}

