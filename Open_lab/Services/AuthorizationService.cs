using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly OpenLabDbContext _db;

        public AuthorizationService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
        {
            var codes = await GetPermissionCodesInternalAsync(userId);
            return codes.Contains(PermissionCodes.FullAccess, StringComparer.OrdinalIgnoreCase)
                || codes.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(int userId)
        {
            return await GetPermissionCodesInternalAsync(userId);
        }

        private async Task<List<string>> GetPermissionCodesInternalAsync(int userId)
        {
            var roleIds = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (roleIds.Count == 0)
            {
                return new List<string>();
            }

            return await _db.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.PermissionCode)
                .Distinct()
                .ToListAsync();
        }
    }
}
