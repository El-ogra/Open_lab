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

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
