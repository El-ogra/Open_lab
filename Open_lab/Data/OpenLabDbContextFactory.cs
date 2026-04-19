using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Open_lab.Data
{
    public class OpenLabDbContextFactory : IDesignTimeDbContextFactory<OpenLabDbContext>
    {
        public OpenLabDbContext CreateDbContext(string[] args)
        {
            var envConnection = Environment.GetEnvironmentVariable("OPENLAB_CONNECTION");
            var configConnection = ConfigurationManager.ConnectionStrings["OpenLabDb"]?.ConnectionString;
            var connectionString = !string.IsNullOrWhiteSpace(envConnection)
                ? envConnection
                : configConnection;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = "Server=.\\SQLEXPRESS;Database=OpenLab;User ID=sa;Password=og2026ra;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False;Connect Timeout=30";
            }

            var optionsBuilder = new DbContextOptionsBuilder<OpenLabDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new OpenLabDbContext(optionsBuilder.Options);
        }
    }
}
