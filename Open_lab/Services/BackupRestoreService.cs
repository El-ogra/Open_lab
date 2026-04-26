using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class BackupRestoreService : IBackupRestoreService
    {
        private readonly OpenLabDbContext _db;
        private static readonly object ScheduleLock = new();
        private static readonly SemaphoreSlim ScheduleExecutionLock = new(1, 1);
        private static Timer? _scheduleTimer;
        private static bool _isScheduleEnabled;
        private static string _scheduledDirectoryPath = string.Empty;
        private static TimeSpan _scheduledLocalTime = TimeSpan.FromHours(2);
        private static DateTime? _lastRunUtc;
        private static string? _lastError;
        private static bool _scheduleLoaded;

        private const string BackupScheduleEnabledKey = "Backup.Schedule.Enabled";
        private const string BackupScheduleDirectoryKey = "Backup.Schedule.Directory";
        private const string BackupScheduleTimeKey = "Backup.Schedule.Time";

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

        public Task<List<string>> ListBackupsAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return Task.FromResult(new List<string>());
            }

            if (!Directory.Exists(directoryPath))
            {
                return Task.FromResult(new List<string>());
            }

            var files = Directory.GetFiles(directoryPath, "*.bak", SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTime)
                .ToList();

            return Task.FromResult(files);
        }

        public async Task ConfigureDailyBackupScheduleAsync(string directoryPath, TimeSpan scheduledLocalTime)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("Backup directory is required.", nameof(directoryPath));
            }

            if (scheduledLocalTime < TimeSpan.Zero || scheduledLocalTime >= TimeSpan.FromDays(1))
            {
                throw new ArgumentOutOfRangeException(nameof(scheduledLocalTime), "Scheduled time must be between 00:00:00 and 23:59:59.");
            }

            Directory.CreateDirectory(directoryPath);
            await EnsureScheduleLoadedAsync();

            lock (ScheduleLock)
            {
                _isScheduleEnabled = true;
                _scheduledDirectoryPath = directoryPath;
                _scheduledLocalTime = new TimeSpan(scheduledLocalTime.Hours, scheduledLocalTime.Minutes, scheduledLocalTime.Seconds);
                _lastError = null;
            }

            await PersistScheduleConfigAsync();
            RearmTimer();
        }

        public async Task CancelBackupScheduleAsync()
        {
            await EnsureScheduleLoadedAsync();

            lock (ScheduleLock)
            {
                _isScheduleEnabled = false;
                _scheduleTimer?.Dispose();
                _scheduleTimer = null;
            }

            await PersistScheduleConfigAsync();
        }

        public async Task<BackupScheduleStatus> GetBackupScheduleStatusAsync()
        {
            await EnsureScheduleLoadedAsync();

            lock (ScheduleLock)
            {
                return new BackupScheduleStatus
                {
                    IsEnabled = _isScheduleEnabled,
                    DirectoryPath = _scheduledDirectoryPath,
                    ScheduledTime = _scheduledLocalTime,
                    LastRunUtc = _lastRunUtc,
                    LastError = _lastError
                };
            }
        }

        private async Task EnsureScheduleLoadedAsync()
        {
            if (_scheduleLoaded)
            {
                return;
            }

            var enabledSetting = await _db.SystemSettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == BackupScheduleEnabledKey);
            var directorySetting = await _db.SystemSettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == BackupScheduleDirectoryKey);
            var timeSetting = await _db.SystemSettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == BackupScheduleTimeKey);

            var enabled = bool.TryParse(enabledSetting?.SettingValue, out var parsedEnabled) && parsedEnabled;
            var time = TimeSpan.TryParse(timeSetting?.SettingValue, out var parsedTime)
                ? new TimeSpan(parsedTime.Hours, parsedTime.Minutes, parsedTime.Seconds)
                : TimeSpan.FromHours(2);

            lock (ScheduleLock)
            {
                _isScheduleEnabled = enabled;
                _scheduledDirectoryPath = directorySetting?.SettingValue ?? string.Empty;
                _scheduledLocalTime = time;
                _scheduleLoaded = true;
            }

            if (enabled && !string.IsNullOrWhiteSpace(_scheduledDirectoryPath))
            {
                RearmTimer();
            }
        }

        private async Task PersistScheduleConfigAsync()
        {
            await UpsertSystemSettingAsync(BackupScheduleEnabledKey, _isScheduleEnabled.ToString(), "Bool", "Enable/disable scheduled backup.");
            await UpsertSystemSettingAsync(BackupScheduleDirectoryKey, _scheduledDirectoryPath, "String", "Scheduled backup target directory.");
            await UpsertSystemSettingAsync(BackupScheduleTimeKey, _scheduledLocalTime.ToString("c"), "String", "Scheduled daily backup local time.");
            await _db.SaveChangesAsync();
        }

        private async Task UpsertSystemSettingAsync(string key, string value, string type, string description)
        {
            var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    SettingType = type,
                    Description = description,
                    LastModified = DateTime.Now
                };
                _db.SystemSettings.Add(setting);
                return;
            }

            setting.SettingValue = value;
            setting.SettingType = type;
            setting.Description = description;
            setting.LastModified = DateTime.Now;
        }

        private void RearmTimer()
        {
            lock (ScheduleLock)
            {
                _scheduleTimer?.Dispose();
                _scheduleTimer = null;

                if (!_isScheduleEnabled || string.IsNullOrWhiteSpace(_scheduledDirectoryPath))
                {
                    return;
                }

                var due = ComputeNextDueTime(DateTime.Now, _scheduledLocalTime);
                _scheduleTimer = new Timer(_ => _ = ExecuteScheduledBackupAsync(), null, due, Timeout.InfiniteTimeSpan);
            }
        }

        private async Task ExecuteScheduledBackupAsync()
        {
            if (!await ScheduleExecutionLock.WaitAsync(0))
            {
                return;
            }

            try
            {
                string directoryPath;
                lock (ScheduleLock)
                {
                    if (!_isScheduleEnabled || string.IsNullOrWhiteSpace(_scheduledDirectoryPath))
                    {
                        return;
                    }

                    directoryPath = _scheduledDirectoryPath;
                }

                var fileName = $"OpenLab_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var backupPath = Path.Combine(directoryPath, fileName);

                try
                {
                    await BackupAsync(backupPath);
                    lock (ScheduleLock)
                    {
                        _lastRunUtc = DateTime.UtcNow;
                        _lastError = null;
                    }
                }
                catch (Exception ex)
                {
                    lock (ScheduleLock)
                    {
                        _lastError = ex.Message;
                    }
                }
            }
            finally
            {
                RearmTimer();
                ScheduleExecutionLock.Release();
            }
        }

        private static TimeSpan ComputeNextDueTime(DateTime nowLocal, TimeSpan scheduledLocalTime)
        {
            var next = nowLocal.Date.Add(scheduledLocalTime);
            if (next <= nowLocal)
            {
                next = next.AddDays(1);
            }

            return next - nowLocal;
        }
    }
}
