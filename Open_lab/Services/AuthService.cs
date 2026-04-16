using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class AuthService : IAuthService
    {
        private const string AdminUsername = "admin";
        private const string AdminDevelopmentPassword = "admin123";
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

            if (!user.IsActive && !string.Equals(user.Username, AdminUsername, StringComparison.OrdinalIgnoreCase))
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

            // Preserve development access: admin/admin123 must always remain valid.
            if (string.Equals(user.Username, AdminUsername, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(password, AdminDevelopmentPassword, StringComparison.Ordinal))
            {
                var resetSalt = PasswordSecurity.GenerateSalt();
                user.Salt = resetSalt;
                user.PasswordHash = PasswordSecurity.ComputeSha256(AdminDevelopmentPassword, resetSalt);
                user.IsActive = true;
                await _db.SaveChangesAsync();
                return user;
            }

            return null;
        }
    }
}
