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

        // ──────────────────────────────────────────────────────────────────
        // 10.1 — Create User
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateUser_WithValidData_ShouldCreateUserAndHashPassword()
        {
            // Function: 10.1 — Create User
            // Arrange
            var user = new User { Username = "newuser", FullName = "New Employee", IsActive = true };

            // Act
            var created = await _service.CreateUserAsync(user, "SecurePass123");

            // Assert
            created.UserId.Should().BeGreaterThan(0);
            created.Username.Should().Be("newuser");
            created.PasswordHash.Should().NotBeNullOrWhiteSpace();
            created.Salt.Should().NotBeNullOrWhiteSpace();
            PasswordSecurity.Verify("SecurePass123", created.Salt, created.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task CreateUser_WithDuplicateUsername_ShouldThrowInvalidOperation()
        {
            // Function: 10.1 — Create User
            // Arrange
            _db.Users.Add(new User { Username = "existinguser" });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () =>
                await _service.CreateUserAsync(new User { Username = "existinguser" }, "pass");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*موجود*");
        }

        [Fact]
        public async Task CreateUser_WithEmptyUsername_ShouldThrowArgumentException()
        {
            // Function: 10.1 — Create User
            // Arrange
            var user = new User { Username = "   " };

            // Act
            Func<Task> act = async () => await _service.CreateUserAsync(user, "pass");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateUser_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Function: 10.1 — Create User
            // Arrange & Act
            // Act
            Func<Task> act = async () => await _service.CreateUserAsync(null!, "pass");

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateUser_WithNullPassword_ShouldCreateUserWithEmptyHash()
        {
            // Function: 10.1 — Create User (edge: no password)
            // Arrange
            var user = new User { Username = "nopass_user", IsActive = true };

            // Act
            var created = await _service.CreateUserAsync(user, null);

            // Assert
            created.UserId.Should().BeGreaterThan(0);
            created.PasswordHash.Should().Be(string.Empty);
            created.Salt.Should().Be(string.Empty);
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.2 — Set Permissions (BR-SEC-001)
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SetPermissions_WithValidRoleAndCodes_ShouldPersistGrantedPermissions()
        {
            // Function: 10.2 — Set Permissions
            // Arrange
            var role = new Role { RoleName = "Reception" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            // Act
            await _service.SaveRolePermissionsAsync(role.RoleId,
                new[] { PermissionCodes.PatientsView, PermissionCodes.ResultsView });

            // Assert
            var saved = await _db.RolePermissions
                .Where(rp => rp.RoleId == role.RoleId)
                .Select(rp => rp.PermissionCode)
                .ToListAsync();
            saved.Should().BeEquivalentTo(new[] { PermissionCodes.PatientsView, PermissionCodes.ResultsView });
        }

        [Fact]
        public async Task SetPermissions_WhenReplacingOldPermissions_ShouldRemoveOldAndAddNew()
        {
            // Function: 10.2 — Set Permissions (BR-SEC-001: admin can grant/revoke per screen)
            // Arrange
            var role = new Role { RoleName = "TestRole" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            _db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionCode = "OldCode1" });
            _db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionCode = "OldCode2" });
            await _db.SaveChangesAsync();

            // Act
            await _service.SaveRolePermissionsAsync(role.RoleId,
                new[] { "NewCode1", "NewCode2", "NewCode3" });

            // Assert
            var current = await _db.RolePermissions
                .Where(rp => rp.RoleId == role.RoleId)
                .ToListAsync();
            current.Should().HaveCount(3);
            current.Should().Contain(rp => rp.PermissionCode == "NewCode1");
            current.Should().Contain(rp => rp.PermissionCode == "NewCode2");
            current.Should().Contain(rp => rp.PermissionCode == "NewCode3");
            current.Should().NotContain(rp => rp.PermissionCode == "OldCode1");
            current.Should().NotContain(rp => rp.PermissionCode == "OldCode2");
        }

        [Fact]
        public async Task SetPermissions_WithDuplicateCodes_ShouldSaveDistinctOnly()
        {
            // Function: 10.2 — Set Permissions (edge: duplicate codes)
            // Arrange
            var role = new Role { RoleName = "DistinctRole" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            // Act
            await _service.SaveRolePermissionsAsync(role.RoleId,
                new[] { PermissionCodes.PatientsView, PermissionCodes.PatientsView, PermissionCodes.ResultsView });

            // Assert
            var permissions = await _db.RolePermissions
                .Where(x => x.RoleId == role.RoleId)
                .Select(x => x.PermissionCode)
                .ToListAsync();
            permissions.Should().BeEquivalentTo(
                new[] { PermissionCodes.PatientsView, PermissionCodes.ResultsView });
        }

        [Fact]
        public async Task GetRolePermissions_WhenNoPermissions_ShouldReturnEmpty()
        {
            // Function: 10.2 — Set Permissions (edge: no permissions assigned)
            // Arrange & Act
            // Act
            var codes = await _service.GetRolePermissionCodesAsync(9999);

            // Assert
            codes.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRolePermissions_WithAssignedPermissions_ShouldReturnCorrectCodes()
        {
            // Function: 10.2 — Set Permissions
            // Arrange
            var role = new Role { RoleName = "R1" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.RolePermissions.AddRange(
                new RolePermission { RoleId = role.RoleId, PermissionCode = PermissionCodes.UsersView },
                new RolePermission { RoleId = role.RoleId, PermissionCode = PermissionCodes.UsersEdit });
            await _db.SaveChangesAsync();

            // Act
            var codes = await _service.GetRolePermissionCodesAsync(role.RoleId);

            // Assert
            codes.Should().BeEquivalentTo(
                new[] { PermissionCodes.UsersView, PermissionCodes.UsersEdit });
        }

        [Fact]
        public async Task AssignRole_ToUser_ShouldReplaceOldRoles()
        {
            // Function: 10.2 — Set Permissions (BR-SEC-001: single-role assignment)
            // Arrange
            var user = new User { Username = "u1" };
            var role1 = new Role { RoleName = "r1" };
            var role2 = new Role { RoleName = "r2" };
            _db.Users.Add(user);
            _db.Roles.AddRange(role1, role2);
            await _db.SaveChangesAsync();
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role2.RoleId });
            await _db.SaveChangesAsync();

            // Act
            await _service.AssignSingleRoleAsync(user.UserId, role1.RoleId);

            // Assert
            var links = await _db.UserRoles
                .Where(ur => ur.UserId == user.UserId)
                .ToListAsync();
            links.Should().ContainSingle();
            links[0].RoleId.Should().Be(role1.RoleId);
        }

        [Fact]
        public async Task AssignRole_WhenUserOrRoleMissing_ShouldThrowInvalidOperation()
        {
            // Function: 10.2 — Set Permissions (failure: user/role not found)
            // Arrange & Act
            // Act
            Func<Task> act = async () => await _service.AssignSingleRoleAsync(999, 888);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*غير موجود*");
        }

        [Fact]
        public async Task RemoveRole_WhenLinkNotFound_ShouldNotThrow()
        {
            // Function: 10.2 — Set Permissions (edge: role not assigned)
            // Arrange & Act
            // Act
            Func<Task> act = async () => await _service.RemoveUserRoleAsync(1, 1);

            // Assert
            await act.Should().NotThrowAsync();
            (await _db.UserRoles.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task RemoveRole_WhenAdminUser_ShouldThrowInvalidOperation()
        {
            // Function: 10.2 — Set Permissions (BR-SEC-001: admin protection)
            // Arrange
            var user = new User { Username = "admin" };
            var role = new Role { RoleName = "r-admin" };
            _db.Users.Add(user);
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.RemoveUserRoleAsync(user.UserId, role.RoleId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*admin*");
        }

        [Fact]
        public async Task RemoveRole_WhenLinkExists_ShouldRemoveSuccessfully()
        {
            // Function: 10.2 — Set Permissions
            // Arrange
            var user = new User { Username = "normal-user" };
            var role = new Role { RoleName = "normal-role" };
            _db.Users.Add(user);
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            await _db.SaveChangesAsync();

            // Act
            await _service.RemoveUserRoleAsync(user.UserId, role.RoleId);

            // Assert
            (await _db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId && ur.RoleId == role.RoleId))
                .Should().BeFalse();
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.3 — Edit User Data
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task EditUserData_WithValidData_ShouldUpdateFieldsAndHashNewPassword()
        {
            // Function: 10.3 — Edit User Data
            // Arrange
            var user = new User { Username = "old_user", FullName = "Old Name", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            user.FullName = "New Name";
            user.Username = "new_user";

            // Act
            await _service.UpdateUserAsync(user, "newSecurePass");

            // Assert
            var updated = await _db.Users.FindAsync(user.UserId);
            updated!.FullName.Should().Be("New Name");
            updated.Username.Should().Be("new_user");
            PasswordSecurity.Verify("newSecurePass", updated.Salt, updated.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation()
        {
            // Function: 10.3 — Edit User Data (failure: admin username is immutable)
            // Arrange
            var admin = new User { Username = "admin", FullName = "Administrator", IsActive = true };
            _db.Users.Add(admin);
            await _db.SaveChangesAsync();
            admin.Username = "new_admin_name";

            // Act
            Func<Task> act = async () => await _service.UpdateUserAsync(admin, null);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*admin*");
        }

        [Fact]
        public async Task EditUserData_WithDuplicateUsername_ShouldThrowInvalidOperation()
        {
            // Function: 10.3 — Edit User Data (failure: duplicate username)
            // Arrange
            _db.Users.AddRange(
                new User { Username = "user_a", IsActive = true },
                new User { Username = "user_b", IsActive = true });
            await _db.SaveChangesAsync();

            var userA = await _db.Users.FirstAsync(u => u.Username == "user_a");
            userA.Username = "user_b";

            // Act
            Func<Task> act = async () => await _service.UpdateUserAsync(userA, null);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*مستخدم*");
        }

        [Fact]
        public async Task EditUserData_WithEmptyNewUsername_ShouldThrowInvalidOperation()
        {
            // Function: 10.3 — Edit User Data (edge: blank username)
            // Arrange
            var user = new User { Username = "valid_user", IsActive = true };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            user.Username = "   ";

            // Act
            Func<Task> act = async () => await _service.UpdateUserAsync(user, null);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task EditUserData_WithNoNewPassword_ShouldNotChangeExistingHash()
        {
            // Function: 10.3 — Edit User Data (edge: no password change)
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256("OriginalPass", salt);
            var user = new User
            {
                Username = "keep_pass",
                PasswordHash = hash,
                Salt = salt,
                IsActive = true
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            user.FullName = "Updated Name";

            // Act
            await _service.UpdateUserAsync(user, null);

            // Assert
            var updated = await _db.Users.FindAsync(user.UserId);
            PasswordSecurity.Verify("OriginalPass", updated!.Salt, updated.PasswordHash).Should().BeTrue();
            updated.FullName.Should().Be("Updated Name");
        }

        // ──────────────────────────────────────────────────────────────────
        // User management helpers (GetUsers, Roles, Delete) — 10.1 / 10.2
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetUsers_ShouldReturnUsersOrderedByUsername()
        {
            // Function: 10.1 — Create User (list)
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
        public async Task GetUsers_WhenNoUsers_ShouldReturnEmpty()
        {
            // Function: 10.1 — Create User (edge: empty list)
            // Arrange & Act
            // Act
            var users = await _service.GetUsersAsync();

            // Assert
            users.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteUser_WhenAdminUser_ShouldThrowInvalidOperation()
        {
            // Function: 10.1 — Create User (protection: admin cannot be deleted)
            // Arrange
            _db.Users.Add(new User { Username = "admin" });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () =>
                await _service.DeleteUserAsync(
                    _db.Users.First(u => u.Username == "admin").UserId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*admin*");
        }

        [Fact]
        public async Task DeleteUser_WhenUserNotFound_ShouldNotThrow()
        {
            // Function: 10.1 — Create User (edge: delete non-existent)
            // Arrange & Act
            // Act
            Func<Task> act = async () => await _service.DeleteUserAsync(404);

            // Assert
            await act.Should().NotThrowAsync();
            (await _db.Users.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DeleteUser_ShouldRemoveUserAndUserRoles()
        {
            // Function: 10.1 — Create User (cascade delete roles)
            // Arrange
            var user = new User { Username = "delete-ok" };
            var role = new Role { RoleName = "R-Delete" };
            _db.Users.Add(user);
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteUserAsync(user.UserId);

            // Assert
            (await _db.Users.AnyAsync(u => u.UserId == user.UserId)).Should().BeFalse();
            (await _db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId)).Should().BeFalse();
        }

        [Fact]
        public async Task CreateRole_WithValidName_ShouldCreateRoleSuccessfully()
        {
            // Function: 10.2 — Set Permissions (create role)
            // Arrange & Act
            // Act
            var role = await _service.CreateRoleAsync("Cashier");

            // Assert
            role.RoleId.Should().BeGreaterThan(0);
            role.RoleName.Should().Be("Cashier");
        }

        [Fact]
        public async Task CreateRole_WithDuplicateName_ShouldThrowInvalidOperation()
        {
            // Function: 10.2 — Set Permissions (failure: duplicate role)
            // Arrange
            _db.Roles.Add(new Role { RoleName = "Role1" });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.CreateRoleAsync("Role1");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*موجود*");
        }

        [Fact]
        public async Task CreateRole_WithWhitespaceName_ShouldThrowArgumentException()
        {
            // Function: 10.2 — Set Permissions (edge: blank role name)
            // Arrange & Act
            // Act
            Func<Task> act = async () => await _service.CreateRoleAsync("   ");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteRole_WhenRoleAssignedToUser_ShouldThrowInvalidOperation()
        {
            // Function: 10.2 — Set Permissions (failure: role in use)
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
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*مرتبط*");
        }

        [Fact]
        public async Task DeleteRole_ShouldRemoveRoleAndPermissions()
        {
            // Function: 10.2 — Set Permissions (cascade delete permissions)
            // Arrange
            var role = new Role { RoleName = "TempRole" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            _db.RolePermissions.Add(
                new RolePermission { RoleId = role.RoleId, PermissionCode = PermissionCodes.UsersView });
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteRoleAsync(role.RoleId);

            // Assert
            (await _db.Roles.AnyAsync(r => r.RoleId == role.RoleId)).Should().BeFalse();
            (await _db.RolePermissions.AnyAsync(rp => rp.RoleId == role.RoleId)).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteRole_WhenRoleNotFound_ShouldNotThrow()
        {
            // Function: 10.2 — Set Permissions (edge: non-existent role)
            // Arrange & Act
            // Act
            Func<Task> act = async () => await _service.DeleteRoleAsync(404);

            // Assert
            await act.Should().NotThrowAsync();
            (await _db.Roles.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task GetRoles_ShouldReturnRolesOrderedByName()
        {
            // Function: 10.2 — Set Permissions (list roles)
            // Arrange
            _db.Roles.AddRange(
                new Role { RoleName = "ZRole" },
                new Role { RoleName = "ARole" });
            await _db.SaveChangesAsync();

            // Act
            var roles = await _service.GetRolesAsync();

            // Assert
            roles.Select(r => r.RoleName).Should().ContainInOrder("ARole", "ZRole");
        }

        [Fact]
        public async Task GetRoles_WhenNoRoles_ShouldReturnEmpty()
        {
            // Function: 10.2 — Set Permissions (edge: no roles)
            // Arrange & Act
            // Act
            var roles = await _service.GetRolesAsync();

            // Assert
            roles.Should().BeEmpty();
        }
    }
}
