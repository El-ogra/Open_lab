using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Tests.Infrastructure
{
    public static class InMemoryDbContextFactory
    {
        public static OpenLabDbContext Create(string dbName)
        {
            var options = new DbContextOptionsBuilder<OpenLabDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new OpenLabDbContext(options);
        }
    }
}
