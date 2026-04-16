using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public interface IAuthorizationService
    {
        Task<bool> HasPermissionAsync(int userId, string permissionCode);
        Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(int userId);
    }
}
