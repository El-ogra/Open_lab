using System;
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
        private string _statusMessage = string.Empty;

        public BackupRestoreViewModel(IBackupRestoreService backupRestoreService)
        {
            _backupRestoreService = backupRestoreService;
            BackupCommand = new RelayCommand(async _ => await BackupAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
            RestoreCommand = new RelayCommand(async _ => await RestoreAsync(), _ => AppSession.HasPermission(PermissionCodes.BackupRestore));
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

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand BackupCommand { get; }
        public ICommand RestoreCommand { get; }

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
                StatusMessage = "تمت الاستعادة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}

