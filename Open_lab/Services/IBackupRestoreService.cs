using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public sealed class BackupScheduleStatus
    {
        public bool IsEnabled { get; set; }
        public string DirectoryPath { get; set; } = string.Empty;
        public TimeSpan ScheduledTime { get; set; }
        public DateTime? LastRunUtc { get; set; }
        public string? LastError { get; set; }
    }

    public interface IBackupRestoreService
    {
        Task BackupAsync(string backupPath);
        Task RestoreAsync(string restorePath);
        Task<List<string>> ListBackupsAsync(string directoryPath);
        Task ConfigureDailyBackupScheduleAsync(string directoryPath, TimeSpan scheduledLocalTime);
        Task CancelBackupScheduleAsync();
        Task<BackupScheduleStatus> GetBackupScheduleStatusAsync();
    }
}
