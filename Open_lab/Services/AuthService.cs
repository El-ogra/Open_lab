using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class AuthService : IAuthService
    {
        private readonly OpenLabDbContext _db;

        public AuthService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<User?> ValidateCredentialsAsync(string username, string password)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);
        }
    }
}
