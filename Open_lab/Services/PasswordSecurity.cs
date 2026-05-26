using System;
using System.Security.Cryptography;
using System.Text;

namespace Open_lab.Services
{
    public static class PasswordSecurity
    {
        public const int LegacySha256Version = 1;
        public const int Pbkdf2Version = 2;
        public const int Pbkdf2Iterations = 50000;

        public static string GenerateSalt()
        {
            var buffer = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(buffer);
        }

        public static string GenerateSecureSalt()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }

        public static string ComputeSha256(string password, string salt)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + "::" + salt);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public static string ComputePbkdf2(string password, string salt, int iterations = Pbkdf2Iterations)
        {
            var saltBytes = Convert.FromHexString(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(32);
            return Convert.ToHexString(hashBytes);
        }

        public static bool Verify(string password, string salt, string expectedHash)
        {
            return Verify(password, salt, expectedHash, LegacySha256Version);
        }

        public static bool Verify(string password, string salt, string expectedHash, int hashVersion)
        {
            string? computed;
            try
            {
                computed = hashVersion switch
                {
                    LegacySha256Version => ComputeSha256(password, salt),
                    Pbkdf2Version => ComputePbkdf2(password, salt),
                    _ => null
                };
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (computed == null)
            {
                return false;
            }

            var left = Encoding.UTF8.GetBytes(computed);
            var right = Encoding.UTF8.GetBytes(expectedHash);
            return CryptographicOperations.FixedTimeEquals(left, right);
        }
    }
}
