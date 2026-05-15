using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.ViewModels;

namespace Open_lab.IntegrationTests.Infrastructure;

public abstract class SqliteIntegrationTestBase : IAsyncLifetime
{
    private SqliteConnection? _connection;

    protected OpenLabDbContext Db { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        AppSession.Clear();
        AppSession.IsAdmin = true;
        AppSession.UserId = 1;
        AppSession.Username = "integration-admin";

        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();
        Db = CreateContext();
        await Db.Database.EnsureCreatedAsync();
        Db.Users.Add(new User
        {
            UserId = 1,
            Username = "integration-admin",
            PasswordHash = "hash",
            Salt = "salt",
            FullName = "Integration Admin",
            IsActive = true
        });
        await Db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        AppSession.Clear();
        await Db.DisposeAsync();
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }

    protected OpenLabDbContext CreateContext()
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("SQLite connection was not initialized.");
        }

        var options = new DbContextOptionsBuilder<OpenLabDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options;
        return new OpenLabDbContext(options);
    }

    protected async Task<Test> SeedTestAsync(string code, string name, decimal price)
    {
        var test = new Test
        {
            Code = code,
            NameReport = name,
            NameReceipt = name,
            Price = price,
            PatientPrice = price,
            IsRoutine = true,
            TurnaroundHours = 24,
            ReportOrder = 1
        };
        Db.Tests.Add(test);
        await Db.SaveChangesAsync();
        return test;
    }

    protected static async Task WaitUntilAsync(Func<bool> predicate, string failureMessage)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        throw new TimeoutException(failureMessage);
    }
}
