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
        private readonly OpenLabDbContext _db;

        public UserAdminService(OpenLabDbContext db)
        {
            _db = db;
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
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Username is required.", nameof(user));
            }

            user.PasswordHash = string.IsNullOrWhiteSpace(plainPassword) ? string.Empty : plainPassword;
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user, string? plainPassword)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var current = await _db.Users.FirstAsync(u => u.UserId == user.UserId);
            current.Username = user.Username;
            current.FullName = user.FullName;
            current.IsActive = user.IsActive;
            if (!string.IsNullOrWhiteSpace(plainPassword))
            {
                current.PasswordHash = plainPassword;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<Role> CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role name is required.", nameof(roleName));
            }

            var role = new Role { RoleName = roleName };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return role;
        }

        public async Task AssignSingleRoleAsync(int userId, int roleId)
        {
            var existing = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _db.UserRoles.RemoveRange(existing);
            _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
            await _db.SaveChangesAsync();
        }

        public async Task SaveRolePermissionsAsync(int roleId, IEnumerable<string> permissionCodes)
        {
            var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _db.RolePermissions.RemoveRange(existing);

            foreach (var code in permissionCodes.Distinct())
            {
                _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionCode = code });
            }

            await _db.SaveChangesAsync();
        }
    }
}
