using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class AuthorizationService
    {
        private readonly OpenLabDbContext _db;

        public AuthorizationService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
        {
            var roleIds = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (roleIds.Count == 0)
            {
                return false;
            }

            return await _db.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .AnyAsync(rp => rp.PermissionCode == PermissionCodes.FullAccess || rp.PermissionCode == permissionCode);
        }
    }
}
