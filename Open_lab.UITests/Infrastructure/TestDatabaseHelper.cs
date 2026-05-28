using System;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.UITests.Infrastructure;

public class TestDatabaseHelper : IDisposable
{
    private readonly string _connectionString;
    private OpenLabDbContext? _context;
    private readonly string? _originalConnectionEnv;

    public TestDatabaseHelper()
    {
        // Create a unique database name
        var dbName = $"OpenLab_UITest_{Guid.NewGuid():N}";
        _connectionString = $"Server=(localdb)\\OpenLabUITests;Database={dbName};Integrated Security=True;TrustServerCertificate=True";

        // Save original environment variable
        _originalConnectionEnv = Environment.GetEnvironmentVariable("OPENLAB_CONNECTION");

        // Set environment variable for the application to use
        Environment.SetEnvironmentVariable("OPENLAB_CONNECTION", _connectionString);
    }

    public async Task<OpenLabDbContext> InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<OpenLabDbContext>()
            .UseSqlServer(_connectionString)
            .EnableSensitiveDataLogging()
            .Options;

        _context = new OpenLabDbContext(options);
        await _context.Database.MigrateAsync();

        // Seed test data
        await SeedTestDataAsync();

        return _context;
    }

    private async Task SeedTestDataAsync()
    {
        if (_context == null) throw new InvalidOperationException("Context not initialized");

        // Create test user with hashed password
        var testPassword = "TestPassword123!";
        var salt = PasswordSecurity.GenerateSecureSalt();
        var passwordHash = PasswordSecurity.ComputePbkdf2(testPassword, salt);

        var testUser = new User
        {
            Username = "testuser",
            PasswordHash = passwordHash,
            Salt = salt,
            HashVersion = PasswordSecurity.Pbkdf2Version,
            FullName = "Test User",
            IsActive = true
        };

        _context.Users.Add(testUser);
        await _context.SaveChangesAsync();

        // Seed admin role + bootstrap marker so IsBootstrapRequiredAsync() returns false
        // and the app shows the login window instead of the bootstrap window.
        var adminRole = new Role { RoleName = "Administrator" };
        _context.Roles.Add(adminRole);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new UserRole { UserId = testUser.UserId, RoleId = adminRole.RoleId });
        _context.Settings.Add(new Setting
        {
            Key = "Bootstrap.AdminCreatedAt",
            Value = DateTimeOffset.UtcNow.ToString("O")
        });
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        // Restore original environment variable
        if (_originalConnectionEnv != null)
        {
            Environment.SetEnvironmentVariable("OPENLAB_CONNECTION", _originalConnectionEnv);
        }
        else
        {
            Environment.SetEnvironmentVariable("OPENLAB_CONNECTION", null);
        }

        // Delete the test database
        try
        {
            _context?.Database.EnsureDeletedAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Ignore deletion errors
        }

        _context?.Dispose();
    }
}
