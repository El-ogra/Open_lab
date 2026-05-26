using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.IntegrationTests;

public class BootstrapIntegrationTests : Infrastructure.SqliteIntegrationTestBase
{
    [Fact]
    public async Task IsBootstrapRequiredAsync_EmptyDatabase_ReturnsTrue()
    {
        await ClearBootstrapDataAsync();
        using var context = CreateContext();
        var service = new AdminSetupService(context);

        var required = await service.IsBootstrapRequiredAsync();

        Assert.True(required);
    }

    [Fact]
    public async Task IsBootstrapRequiredAsync_DatabaseWithAdministrator_ReturnsFalse()
    {
        await ClearBootstrapDataAsync();
        using var context = CreateContext();
        var role = new Role { RoleName = "Administrator" };
        var user = new User
        {
            Username = "admin",
            FullName = "Admin",
            PasswordHash = "hash",
            Salt = "salt",
            IsActive = true
        };
        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
        await context.SaveChangesAsync();

        var service = new AdminSetupService(context);

        var required = await service.IsBootstrapRequiredAsync();

        Assert.False(required);
    }

    [Fact]
    public async Task IsBootstrapRequiredAsync_RestoredBackupWithMarker_ReturnsFalse()
    {
        await ClearBootstrapDataAsync();
        using var context = CreateContext();
        context.Settings.Add(new Setting
        {
            Key = AdminSetupService.BootstrapAdminCreatedAtKey,
            Value = DateTimeOffset.UtcNow.ToString("O")
        });
        await context.SaveChangesAsync();
        var service = new AdminSetupService(context);

        var required = await service.IsBootstrapRequiredAsync();

        Assert.False(required);
    }

    private async Task ClearBootstrapDataAsync()
    {
        Db.UserRoles.RemoveRange(Db.UserRoles);
        Db.RolePermissions.RemoveRange(Db.RolePermissions);
        Db.Roles.RemoveRange(Db.Roles);
        Db.Users.RemoveRange(Db.Users);
        Db.Settings.RemoveRange(Db.Settings.Where(s => s.Key == AdminSetupService.BootstrapAdminCreatedAtKey));
        await Db.SaveChangesAsync();
    }
}
