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

            if (string.IsNullOrWhiteSpace(user.Salt) || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return null;
            }

            if (PasswordSecurity.Verify(password, user.Salt, user.PasswordHash, user.HashVersion))
            {
                if (user.HashVersion == PasswordSecurity.LegacySha256Version)
                {
                    user.Salt = PasswordSecurity.GenerateSecureSalt();
                    user.PasswordHash = PasswordSecurity.ComputePbkdf2(password, user.Salt);
                    user.HashVersion = PasswordSecurity.Pbkdf2Version;
                    await _db.SaveChangesAsync();
                }

                return user;
            }

            return null;
        }
    }
}
