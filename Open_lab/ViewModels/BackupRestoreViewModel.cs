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

        public BackupRestoreViewModel(IBackupRestoreService backupRestoreService)
        {
            _backupRestoreService = backupRestoreService;
            BackupFiles = new ObservableCollection<string>();
            BackupCommand = new RelayCommand(async _ => await BackupAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
            RestoreCommand = new RelayCommand(async _ => await RestoreAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
            LoadBackupsCommand = new RelayCommand(async _ => await LoadBackupsAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
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

        public ICommand BackupCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand LoadBackupsCommand { get; }

        private async Task BackupAsync()
        {
            if (string.IsNullOrWhiteSpace(BackupPath))
            {
                StatusMessage = "أدخل مسار النسخ الاحتياطي.";
                return;
            }

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
        }

        private async Task RestoreAsync()
        {
            if (string.IsNullOrWhiteSpace(RestorePath))
            {
                StatusMessage = "أدخل مسار الاستعادة.";
                return;
            }

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

            await LoadBackupsFromPathAsync(sourcePath);
            StatusMessage = "تم تحديث قائمة النسخ.";
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
    }
}
