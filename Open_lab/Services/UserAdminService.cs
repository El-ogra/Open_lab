using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class UserAdminService : IUserAdminService
    {
        private const string AdminUsername = "admin";
        private const string AdministratorRoleName = "Administrator";
        private readonly OpenLabDbContext _db;
        private readonly ISessionContext _sessionContext;

        public UserAdminService(OpenLabDbContext db, ISessionContext sessionContext)
        {
            _db = db;
            _sessionContext = sessionContext;
        }

        public Task<List<User>> GetUsersAsync()
        {
            return _db.Users.AsNoTracking().OrderBy(u => u.Username).ToListAsync();
        }

        public Task<List<Role>> GetRolesAsync()
        {
            return _db.Roles.AsNoTracking().OrderBy(r => r.RoleName).ToListAsync();
        }

        public Task<List<string>> GetRolePermissionCodesAsync(int roleId)
        {
            return _db.RolePermissions.AsNoTracking()
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionCode)
                .ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user, string? plainPassword)
        {
            EnsureUsersEditPermission();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Username is required.", nameof(user));
            }

            var normalizedUsername = user.Username.Trim();
            var exists = await _db.Users.AnyAsync(u => u.Username == normalizedUsername);
            if (exists)
            {
                throw new InvalidOperationException("اسم المستخدم موجود بالفعل.");
            }

            user.Username = normalizedUsername;
            ApplyPassword(user, plainPassword);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user, string? plainPassword)
        {
            EnsureUsersEditPermission();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var originalUsername = await _db.Users
                .AsNoTracking()
                .Where(u => u.UserId == user.UserId)
                .Select(u => u.Username)
                .FirstAsync();

            var current = await _db.Users.FirstAsync(u => u.UserId == user.UserId);
            var normalizedUsername = user.Username.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                throw new InvalidOperationException("اسم المستخدم مطلوب.");
            }

            if (string.Equals(originalUsername, AdminUsername, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedUsername, AdminUsername, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("لا يمكن تغيير اسم مستخدم admin.");
            }

            var usernameUsed = await _db.Users.AnyAsync(u => u.UserId != user.UserId && u.Username == normalizedUsername);
            if (usernameUsed)
            {
                throw new InvalidOperationException("اسم المستخدم مستخدم من حساب آخر.");
            }

            current.Username = normalizedUsername;
            current.FullName = user.FullName;
            current.IsActive = user.IsActive || string.Equals(current.Username, AdminUsername, StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(plainPassword))
            {
                ApplyPassword(current, plainPassword);
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            EnsureUsersEditPermission();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return;
            }

            if (string.Equals(user.Username, AdminUsername, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("لا يمكن حذف حساب admin.");
            }

            var hasOperationalData =
                await _db.Payments.AnyAsync(p => p.UserId == userId) ||
                await _db.AttendanceLogs.AnyAsync(a => a.UserId == userId) ||
                await _db.ResultValues.AnyAsync(r => r.VerifiedBy == userId) ||
                await _db.SampleCollections.AnyAsync(s => s.CollectedBy == userId);

            if (hasOperationalData)
            {
                throw new InvalidOperationException("لا يمكن حذف المستخدم لارتباطه بسجلات تشغيلية. قم بإيقافه بدلاً من ذلك.");
            }

            var links = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            if (links.Count > 0)
            {
                _db.UserRoles.RemoveRange(links);
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        public async Task<Role> CreateRoleAsync(string roleName)
        {
            EnsureUsersEditPermission();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role name is required.", nameof(roleName));
            }

            var normalizedName = roleName.Trim();
            var exists = await _db.Roles.AnyAsync(r => r.RoleName == normalizedName);
            if (exists)
            {
                throw new InvalidOperationException("اسم الدور موجود بالفعل.");
            }

            var role = new Role { RoleName = normalizedName };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return role;
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            EnsureUsersEditPermission();

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null)
            {
                return;
            }

            if (string.Equals(role.RoleName, AdministratorRoleName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("لا يمكن حذف دور Administrator.");
            }

            var isAssigned = await _db.UserRoles.AnyAsync(ur => ur.RoleId == roleId);
            if (isAssigned)
            {
                throw new InvalidOperationException("لا يمكن حذف الدور لأنه مرتبط بمستخدمين.");
            }

            var permissions = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            if (permissions.Count > 0)
            {
                _db.RolePermissions.RemoveRange(permissions);
            }

            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
        }

        public async Task AssignSingleRoleAsync(int userId, int roleId)
        {
            EnsureUsersEditPermission();

            var userExists = await _db.Users.AnyAsync(u => u.UserId == userId);
            var roleExists = await _db.Roles.AnyAsync(r => r.RoleId == roleId);
            if (!userExists || !roleExists)
            {
                throw new InvalidOperationException("المستخدم أو الدور غير موجود.");
            }

            var existing = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _db.UserRoles.RemoveRange(existing);
            _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
            await _db.SaveChangesAsync();
        }

        public async Task RemoveUserRoleAsync(int userId, int roleId)
        {
            EnsureUsersEditPermission();

            var link = await _db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (link == null)
            {
                return;
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user != null && string.Equals(user.Username, AdminUsername, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("لا يمكن فك دور admin.");
            }

            _db.UserRoles.Remove(link);
            await _db.SaveChangesAsync();
        }

        public async Task SaveRolePermissionsAsync(int roleId, IEnumerable<string> permissionCodes)
        {
            EnsureUsersEditPermission();

            var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _db.RolePermissions.RemoveRange(existing);

            foreach (var code in permissionCodes.Distinct())
            {
                _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionCode = code });
            }

            await _db.SaveChangesAsync();
        }

        private void EnsureUsersEditPermission()
        {
            if (_sessionContext.IsSystemOperation)
            {
                return;
            }

            if (!_sessionContext.HasPermission(PermissionCodes.UsersEdit))
            {
                throw new UnauthorizedAccessException("UsersEdit permission is required to manage users and roles.");
            }
        }

        private static void ApplyPassword(User user, string? plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                user.PasswordHash = string.Empty;
                user.Salt = string.Empty;
                return;
            }

            var salt = PasswordSecurity.GenerateSalt();
            user.Salt = salt;
            user.PasswordHash = PasswordSecurity.ComputeSha256(plainPassword, salt);
        }
    }
}
