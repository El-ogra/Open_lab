using System;
using System.Security.Cryptography;
using System.Text;

namespace Open_lab.Services
{
    public static class PasswordSecurity
    {
        public static string GenerateSalt()
        {
            var buffer = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(buffer);
        }

        public static string ComputeSha256(string password, string salt)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + "::" + salt);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public static bool Verify(string password, string salt, string expectedHash)
        {
            var computed = ComputeSha256(password, salt);
            var left = Encoding.UTF8.GetBytes(computed);
            var right = Encoding.UTF8.GetBytes(expectedHash);
            return CryptographicOperations.FixedTimeEquals(left, right);
        }
    }
}
