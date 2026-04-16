using System.Threading.Tasks;

namespace Open_lab.Services
{
    public interface IBackupRestoreService
    {
        Task BackupAsync(string backupPath);
        Task RestoreAsync(string restorePath);
    }
}
