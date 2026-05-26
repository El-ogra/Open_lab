using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class BootstrapViewModel : BaseViewModel
    {
        private readonly IUserAdminService _userAdminService;
        private readonly IAdminSetupService _adminSetupService;
        private readonly Action _onSetupComplete;
        private string _username = "admin";
        private string _fullName = "System Administrator";
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isBusy;
        private bool _isPasswordVisible;

        public BootstrapViewModel(
            IUserAdminService userAdminService,
            IAdminSetupService adminSetupService,
            Action onSetupComplete)
        {
            _userAdminService = userAdminService;
            _adminSetupService = adminSetupService;
            _onSetupComplete = onSetupComplete;

            CreateAdminCommand = new RelayCommand(async _ => await CreateAdminAsync(), _ => !IsBusy);
            TogglePasswordVisibilityCommand = new RelayCommand(_ => IsPasswordVisible = !IsPasswordVisible);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
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
                    (CreateAdminCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand CreateAdminCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }

        private async Task CreateAdminAsync()
        {
            var validationError = ValidateInput();
            if (!string.IsNullOrEmpty(validationError))
            {
                StatusMessage = validationError;
                return;
            }

            IsBusy = true;
            try
            {
                if (!await _adminSetupService.IsBootstrapRequiredAsync())
                {
                    _onSetupComplete();
                    return;
                }

                SessionContext.Current.IsSystemOperation = true;
                try
                {
                    var user = await _userAdminService.CreateUserAsync(new User
                    {
                        Username = Username.Trim(),
                        FullName = string.IsNullOrWhiteSpace(FullName) ? null : FullName.Trim(),
                        IsActive = true
                    }, Password);

                    await _adminSetupService.EnsureAdminAccessAsync(user.UserId);
                    await _adminSetupService.MarkBootstrapCompleteAsync();
                }
                finally
                {
                    SessionContext.Current.IsSystemOperation = false;
                }

                StatusMessage = "تم إنشاء حساب المشرف بنجاح.";
                _onSetupComplete();
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

        private string? ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                return "يرجى إدخال اسم المستخدم.";
            }

            if (Password != ConfirmPassword)
            {
                return "كلمة المرور وتأكيدها غير متطابقين.";
            }

            return ValidatePassword(Password);
        }

        private static string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
            {
                return "كلمة المرور يجب ألا تقل عن 12 حرفاً.";
            }

            if (string.Equals(password, "admin123", StringComparison.Ordinal))
            {
                return "لا يمكن استخدام كلمة المرور الافتراضية القديمة.";
            }

            if (!password.Any(char.IsUpper) ||
                !password.Any(char.IsLower) ||
                !password.Any(char.IsDigit) ||
                !password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return "كلمة المرور يجب أن تحتوي على حروف كبيرة وصغيرة وأرقام ورمز.";
            }

            return null;
        }
    }
}
