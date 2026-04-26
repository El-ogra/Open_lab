using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class BackupRestoreViewModel : BaseViewModel
    {
        private readonly IBackupRestoreService _backupRestoreService;
        private string _backupPath = string.Empty;
        private string _restorePath = string.Empty;
        private string? _selectedBackupFile;
        private string _statusMessage = string.Empty;
        private string _scheduledBackupDirectory = string.Empty;
        private string _scheduledBackupTime = "02:00";
        private bool _isScheduleEnabled;
        private DateTime? _lastScheduledRunUtc;
        private string? _scheduleErrorMessage;
        private bool _isLoading;

        public BackupRestoreViewModel(IBackupRestoreService backupRestoreService)
        {
            _backupRestoreService = backupRestoreService;
            BackupFiles = new ObservableCollection<string>();
            BackupCommand = new RelayCommand(async _ => await BackupAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
            RestoreCommand = new RelayCommand(async _ => await RestoreAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
            LoadBackupsCommand = new RelayCommand(async _ => await LoadBackupsAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
            ConfigureScheduleCommand = new RelayCommand(async _ => await ConfigureScheduleAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
            DisableScheduleCommand = new RelayCommand(async _ => await DisableScheduleAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
            RefreshScheduleCommand = new RelayCommand(async _ => await RefreshScheduleAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
            _ = RefreshScheduleAsync();
        }

        public string BackupPath
        {
            get => _backupPath;
            set => SetProperty(ref _backupPath, value);
        }

        public string RestorePath
        {
            get => _restorePath;
            set => SetProperty(ref _restorePath, value);
        }

        public ObservableCollection<string> BackupFiles { get; }

        public string? SelectedBackupFile
        {
            get => _selectedBackupFile;
            set
            {
                if (SetProperty(ref _selectedBackupFile, value) && !string.IsNullOrWhiteSpace(value))
                {
                    RestorePath = value;
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public string ScheduledBackupDirectory
        {
            get => _scheduledBackupDirectory;
            set => SetProperty(ref _scheduledBackupDirectory, value);
        }

        public string ScheduledBackupTime
        {
            get => _scheduledBackupTime;
            set => SetProperty(ref _scheduledBackupTime, value);
        }

        public bool IsScheduleEnabled
        {
            get => _isScheduleEnabled;
            private set => SetProperty(ref _isScheduleEnabled, value);
        }

        public DateTime? LastScheduledRunUtc
        {
            get => _lastScheduledRunUtc;
            private set => SetProperty(ref _lastScheduledRunUtc, value);
        }

        public string? ScheduleErrorMessage
        {
            get => _scheduleErrorMessage;
            private set => SetProperty(ref _scheduleErrorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public ICommand BackupCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand LoadBackupsCommand { get; }
        public ICommand ConfigureScheduleCommand { get; }
        public ICommand DisableScheduleCommand { get; }
        public ICommand RefreshScheduleCommand { get; }

        private async Task BackupAsync()
        {
            if (string.IsNullOrWhiteSpace(BackupPath))
            {
                StatusMessage = "أدخل مسار النسخ الاحتياطي.";
                return;
            }

            IsLoading = true;
            try
            {
                await _backupRestoreService.BackupAsync(BackupPath);
                await LoadBackupsFromPathAsync(BackupPath);
                StatusMessage = "تم إنشاء النسخة الاحتياطية.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RestoreAsync()
        {
            if (string.IsNullOrWhiteSpace(RestorePath))
            {
                StatusMessage = "أدخل مسار الاستعادة.";
                return;
            }

            IsLoading = true;
            try
            {
                await _backupRestoreService.RestoreAsync(RestorePath);
                await LoadBackupsFromPathAsync(RestorePath);
                StatusMessage = "تمت الاستعادة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadBackupsAsync()
        {
            var sourcePath = !string.IsNullOrWhiteSpace(BackupPath)
                ? BackupPath
                : RestorePath;

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                StatusMessage = "حدد مسار ملف أو مجلد النسخ الاحتياطية أولاً.";
                return;
            }

            IsLoading = true;
            try
            {
                await LoadBackupsFromPathAsync(sourcePath);
                StatusMessage = "تم تحديث قائمة النسخ.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadBackupsFromPathAsync(string sourcePath)
        {
            var directory = GetBackupDirectory(sourcePath);
            var files = await _backupRestoreService.ListBackupsAsync(directory);

            BackupFiles.Clear();
            foreach (var file in files)
            {
                BackupFiles.Add(file);
            }
        }

        private static string GetBackupDirectory(string sourcePath)
        {
            if (Directory.Exists(sourcePath))
            {
                return sourcePath;
            }

            var extension = Path.GetExtension(sourcePath);
            if (string.Equals(extension, ".bak", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetDirectoryName(sourcePath) ?? sourcePath;
            }

            return sourcePath;
        }

        private async Task ConfigureScheduleAsync()
        {
            if (string.IsNullOrWhiteSpace(ScheduledBackupDirectory))
            {
                StatusMessage = "حدد مجلد النسخ الاحتياطي المجدول.";
                return;
            }

            if (!TimeSpan.TryParse(ScheduledBackupTime, out var scheduledTime))
            {
                StatusMessage = "صيغة وقت الجدولة غير صحيحة (HH:mm).";
                return;
            }

            IsLoading = true;
            try
            {
                await _backupRestoreService.ConfigureDailyBackupScheduleAsync(ScheduledBackupDirectory, scheduledTime);
                await RefreshScheduleStateAsync();
                StatusMessage = "تم تفعيل النسخ الاحتياطي المجدول.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DisableScheduleAsync()
        {
            IsLoading = true;
            try
            {
                await _backupRestoreService.CancelBackupScheduleAsync();
                await RefreshScheduleStateAsync();
                StatusMessage = "تم إيقاف النسخ الاحتياطي المجدول.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshScheduleAsync()
        {
            IsLoading = true;
            try
            {
                await RefreshScheduleStateAsync();
                StatusMessage = "تم تحديث إعدادات الجدولة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshScheduleStateAsync()
        {
            var schedule = await _backupRestoreService.GetBackupScheduleStatusAsync();
            IsScheduleEnabled = schedule.IsEnabled;
            ScheduledBackupDirectory = schedule.DirectoryPath;
            ScheduledBackupTime = schedule.ScheduledTime.ToString(@"hh\:mm");
            LastScheduledRunUtc = schedule.LastRunUtc;
            ScheduleErrorMessage = schedule.LastError;
        }
    }
}
