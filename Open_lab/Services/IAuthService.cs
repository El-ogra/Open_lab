using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IAuthService
    {
        Task<User?> ValidateCredentialsAsync(string username, string password);
    }
}
