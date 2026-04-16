using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private readonly Action _onLoginSuccess;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isBusy;

        public LoginViewModel(Func<OpenLabDbContext> dbFactory, Action onLoginSuccess)
        {
            _dbFactory = dbFactory;
            _onLoginSuccess = onLoginSuccess;
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsBusy);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "يرجى إدخال اسم المستخدم وكلمة المرور.";
                return;
            }

            IsBusy = true;
            try
            {
                using var db = _dbFactory();
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username == Username);
                if (user == null || user.PasswordHash != Password)
                {
                    StatusMessage = "بيانات الدخول غير صحيحة.";
                    return;
                }

                AppSession.UserId = user.UserId;
                AppSession.Username = user.Username;
                AppSession.IsAdmin = user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase);

                var attendance = new AttendanceLog
                {
                    UserId = user.UserId,
                    LoginAt = DateTime.Now,
                    Note = "تسجيل دخول"
                };
                db.AttendanceLogs.Add(attendance);

                if (AppSession.IsAdmin)
                {
                    var setup = new AdminSetupService(db);
                    await setup.EnsureAdminAccessAsync(user.UserId);
                }

                await db.SaveChangesAsync();
                AppSession.AttendanceLogId = attendance.AttendanceLogId;

                StatusMessage = string.Empty;
                _onLoginSuccess();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
