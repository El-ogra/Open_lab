using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAdminSetupService _adminSetupService;
        private readonly IAttendanceService _attendanceService;
        private readonly Action _onLoginSuccess;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isBusy;

        public LoginViewModel(
            IAuthService authService,
            IAuthorizationService authorizationService,
            IAdminSetupService adminSetupService,
            IAttendanceService attendanceService,
            Action onLoginSuccess)
        {
            _authService = authService;
            _authorizationService = authorizationService;
            _adminSetupService = adminSetupService;
            _attendanceService = attendanceService;
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
                var user = await _authService.ValidateCredentialsAsync(Username, Password);
                if (user == null)
                {
                    StatusMessage = "بيانات الدخول غير صحيحة.";
                    return;
                }

                AppSession.UserId = user.UserId;
                AppSession.Username = user.Username;
                AppSession.IsAdmin = user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase);

                if (AppSession.IsAdmin)
                {
                    await _adminSetupService.EnsureAdminAccessAsync(user.UserId);
                }

                var permissionCodes = await _authorizationService.GetPermissionCodesAsync(user.UserId);
                AppSession.SetPermissions(permissionCodes);

                var attendance = await _attendanceService.CreateLoginAsync(user.UserId, "تسجيل دخول");
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
