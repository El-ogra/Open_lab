using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

const string AdminUsername = "admin";

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    if (args.Length == 0 || HasArg(args, "--help") || HasArg(args, "-h"))
    {
        PrintUsage();
        return args.Length == 0 ? 1 : 0;
    }

    if (!HasArg(args, "--reset-admin"))
    {
        Console.Error.WriteLine("Missing required option: --reset-admin");
        PrintUsage();
        return 1;
    }

    var newPassword = GetArgValue(args, "--new-password");
    if (string.IsNullOrWhiteSpace(newPassword))
    {
        Console.Error.WriteLine("Missing required option: --new-password");
        return 1;
    }

    var validationError = ValidatePassword(newPassword);
    if (validationError != null)
    {
        Console.Error.WriteLine(validationError);
        return 1;
    }

    if (!ConfirmReset())
    {
        Console.Error.WriteLine("Reset cancelled.");
        return 2;
    }

    await using var db = new OpenLabDbContextFactory().CreateDbContext(Array.Empty<string>());
    await db.Database.OpenConnectionAsync();
    await using var transaction = await db.Database.BeginTransactionAsync();

    var userAdminService = new UserAdminService(db, SessionContext.Current);
    var adminSetupService = new AdminSetupService(db);
    SessionContext.Current.IsSystemOperation = true;
    try
    {
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Username == AdminUsername);

        if (admin == null)
        {
            admin = await userAdminService.CreateUserAsync(new User
            {
                Username = AdminUsername,
                FullName = "System Administrator",
                IsActive = true
            }, newPassword);
        }
        else
        {
            admin.FullName ??= "System Administrator";
            admin.IsActive = true;
            await userAdminService.UpdateUserAsync(admin, newPassword);
        }

        await adminSetupService.EnsureAdminAccessAsync(admin.UserId);
        await adminSetupService.MarkBootstrapCompleteAsync();
        db.AuditLogs.Add(new AuditLog
        {
            UserId = admin.UserId,
            Action = "AdminPasswordReset",
            TableName = nameof(User),
            RecordId = admin.UserId.ToString(),
            Timestamp = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    finally
    {
        SessionContext.Current.IsSystemOperation = false;
    }

    Console.WriteLine("Admin password reset completed.");
    return 0;
}

static bool HasArg(string[] args, string name)
{
    return args.Any(arg => string.Equals(arg, name, StringComparison.OrdinalIgnoreCase));
}

static string? GetArgValue(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }

    return null;
}

static bool ConfirmReset()
{
    Console.Write("Type RESET-ADMIN to confirm this password reset: ");
    var confirmation = Console.ReadLine();
    return string.Equals(confirmation, "RESET-ADMIN", StringComparison.Ordinal);
}

static string? ValidatePassword(string password)
{
    if (password.Length < 12)
    {
        return "Password must be at least 12 characters.";
    }

    if (string.Equals(password, "admin123", StringComparison.Ordinal))
    {
        return "The legacy default password is not allowed.";
    }

    if (!password.Any(char.IsUpper) ||
        !password.Any(char.IsLower) ||
        !password.Any(char.IsDigit) ||
        !password.Any(ch => !char.IsLetterOrDigit(ch)))
    {
        return "Password must include uppercase, lowercase, digit, and symbol characters.";
    }

    return null;
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  Open_lab.AdminCli --reset-admin --new-password <strong-password>");
}
