using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.SqlClient;

namespace Open_lab.Data
{
    public class OpenLabDbContextFactory : IDesignTimeDbContextFactory<OpenLabDbContext>
    {
        private const string MissingDatabasePasswordMessage = "Database password is not configured. Set the OPENLAB_DB_PASSWORD environment variable or provide a secure OPENLAB_CONNECTION value. Hardcoded fallback credentials are not supported.";

        public OpenLabDbContext CreateDbContext(string[] args)
        {
            var envConnection = Environment.GetEnvironmentVariable("OPENLAB_CONNECTION");
            var configConnection = ConfigurationManager.ConnectionStrings["OpenLabDb"]?.ConnectionString;
            string connectionString;

            if (!string.IsNullOrWhiteSpace(envConnection))
            {
                connectionString = envConnection;
            }
            else
            {
                var dbPassword = Environment.GetEnvironmentVariable("OPENLAB_DB_PASSWORD");
                if (string.IsNullOrWhiteSpace(dbPassword))
                {
                    throw new InvalidOperationException(MissingDatabasePasswordMessage);
                }

                connectionString = !string.IsNullOrWhiteSpace(configConnection)
                    ? configConnection
                    : "Server=.\\SQLEXPRESS;Database=OpenLab;User ID=sa;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False;Connect Timeout=30";
                var connectionBuilder = new SqlConnectionStringBuilder(connectionString)
                {
                    Password = dbPassword
                };
                connectionString = connectionBuilder.ConnectionString;
            }

            var optionsBuilder = new DbContextOptionsBuilder<OpenLabDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new OpenLabDbContext(optionsBuilder.Options);
        }
    }
}
