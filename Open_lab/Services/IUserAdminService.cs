using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IUserAdminService
    {
        Task<List<User>> GetUsersAsync();
        Task<List<Role>> GetRolesAsync();
        Task<List<string>> GetRolePermissionCodesAsync(int roleId);
        Task<User> CreateUserAsync(User user, string? plainPassword);
        Task UpdateUserAsync(User user, string? plainPassword);
        Task<Role> CreateRoleAsync(string roleName);
        Task AssignSingleRoleAsync(int userId, int roleId);
        Task SaveRolePermissionsAsync(int roleId, IEnumerable<string> permissionCodes);
    }
}
