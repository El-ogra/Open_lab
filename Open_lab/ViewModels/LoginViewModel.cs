using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly IDialogService _dialogService;
        private readonly Action _onLoginSuccess;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _rememberMe;
        private bool _isBusy;
        private bool _isPasswordVisible;

        public LoginViewModel(
            IAuthService authService,
            IAuthorizationService authorizationService,
            IAdminSetupService adminSetupService,
            IAttendanceService attendanceService,
            IUserPreferenceService userPreferenceService,
            IDialogService dialogService,
            Action onLoginSuccess)
        {
            _authService = authService;
            _authorizationService = authorizationService;
            _adminSetupService = adminSetupService;
            _attendanceService = attendanceService;
            _userPreferenceService = userPreferenceService;
            _dialogService = dialogService;
            _onLoginSuccess = onLoginSuccess;

            var rememberedUsername = _userPreferenceService.GetRememberedUsername();
            if (!string.IsNullOrWhiteSpace(rememberedUsername))
            {
                _username = rememberedUsername;
                _rememberMe = true;
            }

            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsBusy);
            TogglePasswordVisibilityCommand = new RelayCommand(_ => IsPasswordVisible = !IsPasswordVisible);
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

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
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
        public ICommand TogglePasswordVisibilityCommand { get; }

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
                    _dialogService.ShowError(
                        "اسم المستخدم أو كلمة المرور غير صحيحة",
                        "خطأ");
                    StatusMessage = string.Empty;
                    return;
                }

                var isAdminAccount = user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase);
                if (isAdminAccount)
                {
                    await _adminSetupService.EnsureAdminAccessAsync(user.UserId);
                }

                var permissionCodes = await _authorizationService.GetPermissionCodesAsync(user.UserId);

                SessionContext.Current.BeginSession(user.UserId, user.Username, permissionCodes);

                if (RememberMe)
                {
                    _userPreferenceService.SetRememberedUsername(user.Username);
                }
                else
                {
                    _userPreferenceService.SetRememberedUsername(null);
                }

                var attendance = await _attendanceService.CreateLoginAsync(user.UserId, "تسجيل دخول");
                SessionContext.Current.UpdateAttendanceLog(attendance.AttendanceLogId);

                StatusMessage = "تم تسجيل الدخول بنجاح.";
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
