using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.ViewModels
{
    public class BackupRestoreViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private string _backupPath = string.Empty;
        private string _restorePath = string.Empty;
        private string _statusMessage = string.Empty;

        public BackupRestoreViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            BackupCommand = new RelayCommand(async _ => await BackupAsync());
            RestoreCommand = new RelayCommand(async _ => await RestoreAsync());
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
                using var db = _dbFactory();
                var sql = $"BACKUP DATABASE [OpenLab] TO DISK = '{BackupPath}' WITH INIT";
                await db.Database.ExecuteSqlRawAsync(sql);
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
                using var db = _dbFactory();
                var sql = $@"ALTER DATABASE [OpenLab] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [OpenLab] FROM DISK = '{RestorePath}' WITH REPLACE;
ALTER DATABASE [OpenLab] SET MULTI_USER;";
                await db.Database.ExecuteSqlRawAsync(sql);
                StatusMessage = "تمت الاستعادة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
