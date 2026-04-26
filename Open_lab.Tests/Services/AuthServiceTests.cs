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

        [Fact]
        public async Task ValidateCredentialsAsync_With_Hashed_Password_Should_Return_User_Success()
        {
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256("p@ss", salt);
            _db.Users.Add(new User { Username = "tech1", PasswordHash = hash, Salt = salt, IsActive = true });
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("tech1", "p@ss");

            // Assert
            user.Should().NotBeNull();
            user!.Username.Should().Be("tech1");
        }

        [Fact]
        public async Task ValidateCredentialsAsync_When_User_Not_Found_Should_Return_Null_Failure()
        {
            // Act
            var user = await _service.ValidateCredentialsAsync("missing", "x");

            // Assert
            user.Should().BeNull();
        }

        [Fact]
        public async Task ValidateCredentialsAsync_With_Legacy_PlainText_Should_Migrate_To_Hash_Edge()
        {
            // Arrange
            var legacy = new User { Username = "legacy", PasswordHash = "plain", Salt = "", IsActive = true };
            _db.Users.Add(legacy);
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("legacy", "plain");

            // Assert
            user.Should().NotBeNull();
            var refreshed = await _db.Users.SingleAsync(u => u.Username == "legacy");
            refreshed.Salt.Should().NotBeNullOrWhiteSpace();
            refreshed.PasswordHash.Should().NotBe("plain");
            PasswordSecurity.Verify("plain", refreshed.Salt, refreshed.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task ValidateCredentialsAsync_When_User_Inactive_And_Not_Admin_Should_Return_Null_Failure()
        {
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256("secret", salt);
            _db.Users.Add(new User { Username = "inactive", PasswordHash = hash, Salt = salt, IsActive = false });
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("inactive", "secret");

            // Assert
            user.Should().BeNull();
        }

        [Fact]
        public async Task ValidateCredentialsAsync_Admin_Development_Fallback_Should_Reset_Hash_And_Return_User_Edge()
        {
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();
            var wrongHash = PasswordSecurity.ComputeSha256("wrong", salt);
            _db.Users.Add(new User { Username = "admin", PasswordHash = wrongHash, Salt = salt, IsActive = false });
            await _db.SaveChangesAsync();

            // Act
            var user = await _service.ValidateCredentialsAsync("admin", "admin123");

            // Assert
            user.Should().NotBeNull();
            var refreshed = await _db.Users.SingleAsync(u => u.Username == "admin");
            refreshed.IsActive.Should().BeTrue();
            PasswordSecurity.Verify("admin123", refreshed.Salt, refreshed.PasswordHash).Should().BeTrue();
        }
    }
}
