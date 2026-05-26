using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly AuthService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public AuthServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new AuthService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ──────────────────────────────────────────────────────────────────
        // 10.8 — Logout / Validate Credentials (Auth boundary)
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidateCredentials_WithValidHashedPassword_ShouldReturnUser()
        {
            // Function: 10.8 — Logout (auth entry: validates credentials before logout)
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256("p@ss", salt);
            _db.Users.Add(new User
            {
                Username = "tech1",
                PasswordHash = hash,
                Salt = salt,
                IsActive = true
            });
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("tech1", "p@ss");

            // Assert
            user.Should().NotBeNull();
            user!.Username.Should().Be("tech1");
        }

        [Fact]
        public async Task ValidateCredentials_WithLegacySha256_ShouldUpgradeToPbkdf2()
        {
            // Function: 10.8 — Logout (security: legacy SHA-256 is upgraded after successful login)
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256("p@ss", salt);
            _db.Users.Add(new User
            {
                Username = "legacy_sha_user",
                PasswordHash = hash,
                Salt = salt,
                HashVersion = PasswordSecurity.LegacySha256Version,
                IsActive = true
            });
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("legacy_sha_user", "p@ss");

            // Assert
            user.Should().NotBeNull();
            var refreshed = await _db.Users.SingleAsync(u => u.Username == "legacy_sha_user");
            refreshed.HashVersion.Should().Be(PasswordSecurity.Pbkdf2Version);
            refreshed.Salt.Should().NotBe(salt);
            refreshed.PasswordHash.Should().NotBe(hash);
            PasswordSecurity.Verify("p@ss", refreshed.Salt, refreshed.PasswordHash, refreshed.HashVersion)
                .Should().BeTrue();
        }

        [Fact]
        public async Task ValidateCredentials_WhenUserNotFound_ShouldReturnNull()
        {
            // Function: 10.8 — Logout (failure: invalid credentials)
            // Arrange & Act
            // Act
            var user = await _service.ValidateCredentialsAsync("missing_user", "x");

            // Assert
            user.Should().BeNull();
        }

        [Fact]
        public async Task ValidateCredentials_WithLegacyPlainText_ShouldReturnNullAndNotMutateUser()
        {
            // Function: 10.8 — Logout (security: legacy plain-text password path is rejected)
            // Arrange
            var legacy = new User
            {
                Username = "legacy_user",
                PasswordHash = "plain_password",
                Salt = "",
                IsActive = true
            };
            _db.Users.Add(legacy);
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("legacy_user", "plain_password");

            // Assert
            user.Should().BeNull();
            var refreshed = await _db.Users.SingleAsync(u => u.Username == "legacy_user");
            refreshed.Salt.Should().Be(string.Empty);
            refreshed.PasswordHash.Should().Be("plain_password");
        }

        [Fact]
        public async Task ValidateCredentials_WhenUserInactiveAndNotAdmin_ShouldReturnNull()
        {
            // Function: 10.8 — Logout (failure: inactive user blocked)
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256("secret", salt);
            _db.Users.Add(new User
            {
                Username = "inactive_emp",
                PasswordHash = hash,
                Salt = salt,
                IsActive = false
            });
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("inactive_emp", "secret");

            // Assert
            user.Should().BeNull();
        }

        [Fact]
        public async Task ValidateCredentials_AdminDevelopmentFallback_ShouldReturnNullAndNotMutateUser()
        {
            // Function: 10.8 — Logout (security: hardcoded admin fallback is rejected)
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();
            var wrongHash = PasswordSecurity.ComputeSha256("wrong_pass", salt);
            _db.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = wrongHash,
                Salt = salt,
                IsActive = false
            });
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("admin", "admin123");

            // Assert
            user.Should().BeNull();
            var refreshed = await _db.Users.SingleAsync(u => u.Username == "admin");
            refreshed.IsActive.Should().BeFalse();
            refreshed.Salt.Should().Be(salt);
            refreshed.PasswordHash.Should().Be(wrongHash);
        }

        [Fact]
        public async Task ValidateCredentials_WithWrongPassword_ShouldReturnNull()
        {
            // Function: 10.8 — Logout (failure: wrong password)
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256("correct_pass", salt);
            _db.Users.Add(new User
            {
                Username = "emp1",
                PasswordHash = hash,
                Salt = salt,
                IsActive = true
            });
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("emp1", "wrong_pass");

            // Assert
            user.Should().BeNull();
        }
    }
}
