using System.Threading.Tasks;

namespace Open_lab.Services
{
    public interface IAdminSetupService
    {
        Task<bool> IsBootstrapRequiredAsync();
        Task MarkBootstrapCompleteAsync();
        Task EnsureAdminAccessAsync(int userId);
    }
}
