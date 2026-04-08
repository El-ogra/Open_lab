using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class AdminSetupService
    {
        private readonly OpenLabDbContext _db;

        public AdminSetupService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task EnsureAdminAccessAsync(int userId)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "Administrator");
            if (role == null)
            {
                role = new Role { RoleName = "Administrator" };
                _db.Roles.Add(role);
                await _db.SaveChangesAsync();
            }

            var hasUserRole = await _db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.RoleId);
            if (!hasUserRole)
            {
                _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.RoleId });
                await _db.SaveChangesAsync();
            }

            var existing = await _db.RolePermissions
                .Where(rp => rp.RoleId == role.RoleId)
                .Select(rp => rp.PermissionCode)
                .ToListAsync();

            foreach (var code in PermissionCodes.All)
            {
                if (!existing.Contains(code))
                {
                    _db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionCode = code });
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
