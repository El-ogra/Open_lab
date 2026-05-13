using System;
using System.Windows.Input;
using Open_lab.ViewModels;

namespace Open_lab.ViewModels.Users
{
    public class UsersModuleViewModel : BaseViewModel
    {
        private readonly Action<string> _openPlaceholder;

        public UsersModuleViewModel(Action<string> openPlaceholder)
        {
            _openPlaceholder = openPlaceholder ?? throw new ArgumentNullException(nameof(openPlaceholder));

            OpenCreateUsersCommand = new RelayCommand(_ => _openPlaceholder("انشاء مستخدمين"));
            OpenChangePasswordCommand = new RelayCommand(_ => _openPlaceholder("تغيير كلمة المرور"));
            OpenAttendanceCommand = new RelayCommand(_ => _openPlaceholder("الحضور والإنصراف"));
            OpenLoginDetectorCommand = new RelayCommand(_ => _openPlaceholder("Login detector"));
        }

        public ICommand OpenCreateUsersCommand { get; }
        public ICommand OpenChangePasswordCommand { get; }
        public ICommand OpenAttendanceCommand { get; }
        public ICommand OpenLoginDetectorCommand { get; }
    }
}
