using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.Services
{
    public class BackupRestoreService : IBackupRestoreService
    {
        private readonly OpenLabDbContext _db;

        public BackupRestoreService(OpenLabDbContext db)
        {
            _db = db;
        }

        public async Task BackupAsync(string backupPath)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
            {
                throw new ArgumentException("Backup path is required.", nameof(backupPath));
            }

            var connectionString = _db.Database.GetConnectionString();
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "BACKUP DATABASE [OpenLab] TO DISK = @path WITH INIT";
            command.Parameters.Add(new SqlParameter("@path", SqlDbType.NVarChar, 4000) { Value = backupPath });
            await command.ExecuteNonQueryAsync();
        }

        public async Task RestoreAsync(string restorePath)
        {
            if (string.IsNullOrWhiteSpace(restorePath))
            {
                throw new ArgumentException("Restore path is required.", nameof(restorePath));
            }

            var connectionString = _db.Database.GetConnectionString();
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"ALTER DATABASE [OpenLab] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [OpenLab] FROM DISK = @path WITH REPLACE;
ALTER DATABASE [OpenLab] SET MULTI_USER;";
            command.Parameters.Add(new SqlParameter("@path", SqlDbType.NVarChar, 4000) { Value = restorePath });
            await command.ExecuteNonQueryAsync();
        }
    }
}

