using System.Threading.Tasks;

namespace Open_lab.Services
{
    public interface IAdminSetupService
    {
        Task EnsureAdminAccessAsync(int userId);
    }
}
