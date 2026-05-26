using System;
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

        public async Task<User?> ValidateCredentialsAsync(string username, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            // Backward-compatible path: legacy plain-text passwords are migrated on successful login.
            var hasSalt = !string.IsNullOrWhiteSpace(user.Salt);
            if (!hasSalt)
            {
                if (!string.Equals(user.PasswordHash, password, StringComparison.Ordinal))
                {
                    return null;
                }

                var salt = PasswordSecurity.GenerateSalt();
                user.Salt = salt;
                user.PasswordHash = PasswordSecurity.ComputeSha256(password, salt);
                await _db.SaveChangesAsync();
                return user;
            }

            if (PasswordSecurity.Verify(password, user.Salt, user.PasswordHash))
            {
                return user;
            }

            return null;
        }
    }
}
