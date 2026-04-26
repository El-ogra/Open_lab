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
    public class UsersPermissionsViewModelTests : IDisposable
    {
        private readonly Mock<IUserAdminService> _userAdminServiceMock = new();
        private readonly UsersPermissionsViewModel _viewModel;

        public UsersPermissionsViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _userAdminServiceMock.Setup(x => x.GetUsersAsync()).ReturnsAsync(new List<User>());
            _userAdminServiceMock.Setup(x => x.GetRolesAsync()).ReturnsAsync(new List<Role>());
            _userAdminServiceMock.Setup(x => x.GetRolePermissionCodesAsync(It.IsAny<int>())).ReturnsAsync(new List<string>());
            _viewModel = new UsersPermissionsViewModel(_userAdminServiceMock.Object);
        }

        [Fact]
        public async Task SaveUserAsync_Should_Create_New_User_And_Select_It()
        {
            _viewModel.Username = "newuser";
            _viewModel.FullName = "New User";
            _viewModel.Password = "pass";
            _viewModel.IsActive = true;

            _userAdminServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<User>(), "pass"))
                .ReturnsAsync(new User { UserId = 9, Username = "newuser", FullName = "New User", IsActive = true });

            await _viewModel.InvokePrivateAsync("SaveUserAsync");

            _viewModel.Users.Should().ContainSingle();
            _viewModel.SelectedUser.Should().NotBeNull();
            _viewModel.SelectedUser!.UserId.Should().Be(9);
            _viewModel.StatusMessage.Should().Be("تم إنشاء المستخدم.");
        }

        [Fact]
        public async Task SaveUserAsync_Should_Update_Existing_User()
        {
            var existing = new User { UserId = 5, Username = "olduser", FullName = "Old User", IsActive = true };
            _viewModel.Users.Add(existing);
            _viewModel.SelectedUser = existing;
            _viewModel.Username = "updated";
            _viewModel.FullName = "Updated User";
            _viewModel.Password = "newpass";

            _userAdminServiceMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>(), "newpass")).Returns(Task.CompletedTask);

            await _viewModel.InvokePrivateAsync("SaveUserAsync");

            _userAdminServiceMock.Verify(x => x.UpdateUserAsync(
                It.Is<User>(u => u.UserId == 5 && u.Username == "updated" && u.FullName == "Updated User"),
                "newpass"), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم تحديث المستخدم.");
        }

        [Fact]
        public async Task SaveRolePermissionsAsync_Should_Send_Granted_Codes()
        {
            var role = new Role { RoleId = 4, RoleName = "Reception" };
            _userAdminServiceMock.Setup(x => x.GetRolePermissionCodesAsync(role.RoleId))
                .ReturnsAsync(new List<string> { PermissionCodes.UsersView });

            _viewModel.SelectedRole = role;
            await _viewModel.InvokePrivateAsync("LoadPermissionsAsync");

            var editPermission = _viewModel.Permissions.First(p => p.Code == PermissionCodes.UsersEdit);
            editPermission.IsGranted = true;

            await _viewModel.InvokePrivateAsync("SaveRolePermissionsAsync");

            _userAdminServiceMock.Verify(x => x.SaveRolePermissionsAsync(role.RoleId,
                It.Is<IEnumerable<string>>(codes =>
                    codes.Contains(PermissionCodes.UsersView) &&
                    codes.Contains(PermissionCodes.UsersEdit))), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم حفظ الصلاحيات.");
        }

        [Fact]
        public async Task DeleteUserCommand_When_Selected_User_Exists_Should_Call_Service_Success()
        {
            // Arrange
            var user = new User { UserId = 10, Username = "delete-me" };
            _viewModel.SelectedUser = user;
            _userAdminServiceMock.Setup(x => x.DeleteUserAsync(10)).Returns(Task.CompletedTask);

            // Act
            _viewModel.DeleteUserCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _userAdminServiceMock.Verify(x => x.DeleteUserAsync(10), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم حذف المستخدم.");
        }

        [Fact]
        public async Task SaveRoleCommand_When_RoleName_Empty_Should_Set_Validation_Message_Failure()
        {
            // Arrange
            _viewModel.RoleName = " ";

            // Act
            _viewModel.SaveRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Be("أدخل اسم الدور.");
            _userAdminServiceMock.Verify(x => x.CreateRoleAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SaveRoleCommand_When_Valid_RoleName_Should_Create_Role_Success()
        {
            // Arrange
            _viewModel.RoleName = "Reception";
            _userAdminServiceMock.Setup(x => x.CreateRoleAsync("Reception")).ReturnsAsync(new Role { RoleId = 7, RoleName = "Reception" });

            // Act
            _viewModel.SaveRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Roles.Should().ContainSingle(r => r.RoleId == 7);
            _viewModel.StatusMessage.Should().Be("تم إنشاء الدور.");
        }

        [Fact]
        public async Task AssignRoleCommand_When_User_And_Role_Selected_Should_Call_Service_Success()
        {
            // Arrange
            _viewModel.SelectedUser = new User { UserId = 1, Username = "u" };
            _viewModel.SelectedRole = new Role { RoleId = 2, RoleName = "r" };
            _userAdminServiceMock.Setup(x => x.AssignSingleRoleAsync(1, 2)).Returns(Task.CompletedTask);

            // Act
            _viewModel.AssignRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _userAdminServiceMock.Verify(x => x.AssignSingleRoleAsync(1, 2), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم ربط المستخدم بالدور.");
        }

        [Fact]
        public async Task UnassignRoleCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Arrange
            _viewModel.SelectedUser = new User { UserId = 1, Username = "u" };
            _viewModel.SelectedRole = new Role { RoleId = 2, RoleName = "r" };
            _userAdminServiceMock.Setup(x => x.RemoveUserRoleAsync(1, 2)).ThrowsAsync(new InvalidOperationException("unlink-failed"));

            // Act
            _viewModel.UnassignRoleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("unlink-failed");
        }

        [Fact]
        public async Task ReloadCommand_Should_Load_Users_And_Roles_Success()
        {
            // Arrange
            _userAdminServiceMock.Setup(x => x.GetUsersAsync()).ReturnsAsync(new List<User> { new() { UserId = 8, Username = "u8" } });
            _userAdminServiceMock.Setup(x => x.GetRolesAsync()).ReturnsAsync(new List<Role> { new() { RoleId = 9, RoleName = "r9" } });

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
