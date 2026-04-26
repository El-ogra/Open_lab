using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class UserAdminServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly UserAdminService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public UserAdminServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new UserAdminService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateUserAsync_Should_Create_And_Hash_Password()
        {
            var user = new User { Username = "newuser", FullName = "New" };
            var created = await _service.CreateUserAsync(user, "pass123");
            created.UserId.Should().BeGreaterThan(0);
            created.PasswordHash.Should().NotBeNullOrWhiteSpace();
            created.Salt.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task CreateUserAsync_Duplicate_Should_Throw()
        {
            _db.Users.Add(new User { Username = "exist" });
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.CreateUserAsync(new User { Username = "exist" }, "p");
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task DeleteUserAsync_Admin_Should_Throw()
        {
            _db.Users.Add(new User { Username = "admin" });
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.DeleteUserAsync(_db.Users.First(u => u.Username == "admin").UserId);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateRoleAsync_Duplicate_Should_Throw()
        {
            _db.Roles.Add(new Role { RoleName = "Role1" });
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.CreateRoleAsync("Role1");
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AssignSingleRoleAsync_Should_Assign_And_Remove_Old_Roles()
        {
            // Refactored to Logic Guard - verifies single role assignment and removal of old roles
            var user = new User { Username = "u1" };
            var role1 = new Role { RoleName = "r1" };
            var role2 = new Role { RoleName = "r2" };
            _db.Users.Add(user);
            _db.Roles.AddRange(role1, role2);
            await _db.SaveChangesAsync();

            // Pre-assign role2 to test removal
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role2.RoleId });
            await _db.SaveChangesAsync();

            // Act
            await _service.AssignSingleRoleAsync(user.UserId, role1.RoleId);

            // Assert - Logic Guard: Verify only role1 is assigned, role2 is removed
            var links = await _db.UserRoles.Where(ur => ur.UserId == user.UserId).ToListAsync();
            links.Should().ContainSingle();
            links[0].UserId.Should().Be(user.UserId);
            links[0].RoleId.Should().Be(role1.RoleId);
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Update_Fields_And_Hash_New_Password()
        {
            // Arrange
            var user = new User { Username = "old", FullName = "Old" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            user.FullName = "New Name";
            user.Username = "newname";

            // Act
            await _service.UpdateUserAsync(user, "newpass123");

            // Assert
            var updated = await _db.Users.FindAsync(user.UserId);
            updated!.FullName.Should().Be("New Name");
            updated.Username.Should().Be("newname");
            PasswordSecurity.Verify("newpass123", updated.Salt, updated.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task SaveRolePermissionsAsync_Should_Replace_Existing_Permissions_Completely()
        {
            // Refactored to Logic Guard - verifies complete replacement including removal of old permissions
            var role = new Role { RoleName = "TestRole" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionCode = "OldCode1" });
            _db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionCode = "OldCode2" });
            await _db.SaveChangesAsync();

            // Act
            await _service.SaveRolePermissionsAsync(role.RoleId, new[] { "NewCode1", "NewCode2", "NewCode3" });

            // Assert - Logic Guard: Verify old permissions are removed and new ones are added
            var current = await _db.RolePermissions.Where(rp => rp.RoleId == role.RoleId).ToListAsync();
            current.Should().HaveCount(3);
            current.Should().Contain(rp => rp.PermissionCode == "NewCode1");
            current.Should().Contain(rp => rp.PermissionCode == "NewCode2");
            current.Should().Contain(rp => rp.PermissionCode == "NewCode3");
            current.Should().NotContain(rp => rp.PermissionCode == "OldCode1");
            current.Should().NotContain(rp => rp.PermissionCode == "OldCode2");
        }

        [Fact]
        public async Task GetUsersAsync_Should_Return_Users_Ordered_By_Username_Success()
        {
            // Arrange
            _db.Users.AddRange(
                new User { Username = "z-user" },
                new User { Username = "a-user" });
            await _db.SaveChangesAsync();

            // Act
            var users = await _service.GetUsersAsync();

            // Assert
            users.Select(u => u.Username).Should().ContainInOrder("a-user", "z-user");
        }

        [Fact]
        public async Task GetUsersAsync_When_No_Users_Should_Return_Empty_Edge()
        {
            var users = await _service.GetUsersAsync();

            users.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRolesAsync_Should_Return_Roles_Ordered_By_Name_Success()
        {
            // Arrange
            _db.Roles.AddRange(new Role { RoleName = "ZRole" }, new Role { RoleName = "ARole" });
            await _db.SaveChangesAsync();

            // Act
            var roles = await _service.GetRolesAsync();

            // Assert
            roles.Select(r => r.RoleName).Should().ContainInOrder("ARole", "ZRole");
        }

        [Fact]
        public async Task GetRolesAsync_When_No_Roles_Should_Return_Empty_Edge()
        {
            var roles = await _service.GetRolesAsync();

            roles.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRolePermissionCodesAsync_Should_Return_Codes_Success()
        {
            var role = new Role { RoleName = "R1" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.RolePermissions.AddRange(
                new RolePermission { RoleId = role.RoleId, PermissionCode = "A" },
                new RolePermission { RoleId = role.RoleId, PermissionCode = "B" });
            await _db.SaveChangesAsync();

            var codes = await _service.GetRolePermissionCodesAsync(role.RoleId);

            codes.Should().BeEquivalentTo(new[] { "A", "B" });
        }

        [Fact]
        public async Task GetRolePermissionCodesAsync_When_No_Permissions_Should_Return_Empty_Edge()
        {
            // Act
            var codes = await _service.GetRolePermissionCodesAsync(777);

            // Assert
            codes.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateRoleAsync_When_Name_Is_Whitespace_Should_Throw_Failure()
        {
            // Act
            Func<Task> act = async () => await _service.CreateRoleAsync("   ");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateRoleAsync_Should_Create_Role_Success()
        {
            var role = await _service.CreateRoleAsync("Cashier");

            role.RoleId.Should().BeGreaterThan(0);
            role.RoleName.Should().Be("Cashier");
        }

        [Fact]
        public async Task DeleteUserAsync_When_User_Not_Found_Should_Not_Throw_Edge()
        {
            // Act
            await _service.DeleteUserAsync(404);

            // Assert
            (await _db.Users.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Remove_User_And_UserRoles_Success()
        {
            var user = new User { Username = "delete-ok" };
            var role = new Role { RoleName = "R-Delete" };
            _db.Users.Add(user);
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            await _db.SaveChangesAsync();

            await _service.DeleteUserAsync(user.UserId);

            (await _db.Users.AnyAsync(u => u.UserId == user.UserId)).Should().BeFalse();
            (await _db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId)).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteRoleAsync_When_Role_Assigned_To_User_Should_Throw_Failure()
        {
            // Arrange
            var user = new User { Username = "assigned-user" };
            var role = new Role { RoleName = "AssignedRole" };
            _db.Users.Add(user);
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.DeleteRoleAsync(role.RoleId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task DeleteRoleAsync_Should_Remove_Role_And_Permissions_Success()
        {
            var role = new Role { RoleName = "TempRole" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionCode = "UsersView" });
            await _db.SaveChangesAsync();

            await _service.DeleteRoleAsync(role.RoleId);

            (await _db.Roles.AnyAsync(r => r.RoleId == role.RoleId)).Should().BeFalse();
            (await _db.RolePermissions.AnyAsync(rp => rp.RoleId == role.RoleId)).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteRoleAsync_When_Role_Not_Found_Should_Not_Throw_Edge()
        {
            await _service.DeleteRoleAsync(404);

            (await _db.Roles.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task AssignSingleRoleAsync_When_User_Or_Role_Missing_Should_Throw_Failure()
        {
            Func<Task> act = async () => await _service.AssignSingleRoleAsync(999, 888);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task RemoveUserRoleAsync_When_Link_Not_Found_Should_Not_Throw_Edge()
        {
            // Act
            await _service.RemoveUserRoleAsync(1, 1);

            // Assert
            (await _db.UserRoles.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task RemoveUserRoleAsync_Should_Remove_Link_Success()
        {
            var user = new User { Username = "normal-user" };
            var role = new Role { RoleName = "normal-role" };
            _db.Users.Add(user);
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            await _db.SaveChangesAsync();

            await _service.RemoveUserRoleAsync(user.UserId, role.RoleId);

            (await _db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId && ur.RoleId == role.RoleId)).Should().BeFalse();
        }

        [Fact]
        public async Task RemoveUserRoleAsync_When_Admin_User_Should_Throw_Failure()
        {
            var user = new User { Username = "admin" };
            var role = new Role { RoleName = "r-admin" };
            _db.Users.Add(user);
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.RemoveUserRoleAsync(user.UserId, role.RoleId);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SaveRolePermissionsAsync_When_Source_Has_Duplicates_Should_Save_Distinct_Edge()
        {
            var role = new Role { RoleName = "DistinctRole" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            await _service.SaveRolePermissionsAsync(role.RoleId, new[] { "A", "A", "B" });

            var permissions = await _db.RolePermissions.Where(x => x.RoleId == role.RoleId).Select(x => x.PermissionCode).ToListAsync();
            permissions.Should().BeEquivalentTo(new[] { "A", "B" });
        }
    }
}
