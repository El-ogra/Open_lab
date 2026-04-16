using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class UsersPermissionsViewModel : BaseViewModel
    {
        private readonly IUserAdminService _userAdminService;
        private User? _selectedUser;
        private Role? _selectedRole;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _fullName = string.Empty;
        private bool _isActive = true;
        private string _roleName = string.Empty;
        private string _statusMessage = string.Empty;

        public UsersPermissionsViewModel(IUserAdminService userAdminService)
        {
            _userAdminService = userAdminService;
            Users = new ObservableCollection<User>();
            Roles = new ObservableCollection<Role>();
            Permissions = new ObservableCollection<PermissionToggle>();

            SaveUserCommand = new RelayCommand(async _ => await SaveUserAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersEdit));
            DeleteUserCommand = new RelayCommand(async _ => await DeleteUserAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersEdit) && SelectedUser != null);
            SaveRoleCommand = new RelayCommand(async _ => await SaveRoleAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersEdit));
            DeleteRoleCommand = new RelayCommand(async _ => await DeleteRoleAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersEdit) && SelectedRole != null);
            SaveRolePermissionsCommand = new RelayCommand(async _ => await SaveRolePermissionsAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersEdit));
            AssignRoleCommand = new RelayCommand(async _ => await AssignRoleAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersEdit) && SelectedUser != null && SelectedRole != null);
            UnassignRoleCommand = new RelayCommand(async _ => await UnassignRoleAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersEdit) && SelectedUser != null && SelectedRole != null);
            ReloadCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersView));

            _ = LoadAsync();
        }

        public ObservableCollection<User> Users { get; }
        public ObservableCollection<Role> Roles { get; }
        public ObservableCollection<PermissionToggle> Permissions { get; }

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    (AssignRoleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UnassignRoleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DeleteUserCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    LoadFromUser();
                }
            }
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    (AssignRoleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UnassignRoleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DeleteRoleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    _ = LoadPermissionsAsync();
                }
            }
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

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string RoleName
        {
            get => _roleName;
            set => SetProperty(ref _roleName, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SaveUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand SaveRoleCommand { get; }
        public ICommand DeleteRoleCommand { get; }
        public ICommand SaveRolePermissionsCommand { get; }
        public ICommand AssignRoleCommand { get; }
        public ICommand UnassignRoleCommand { get; }
        public ICommand ReloadCommand { get; }

        private async Task LoadAsync()
        {
            var users = await _userAdminService.GetUsersAsync();
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }

            var roles = await _userAdminService.GetRolesAsync();
            Roles.Clear();
            foreach (var role in roles)
            {
                Roles.Add(role);
            }
        }

        private void LoadFromUser()
        {
            if (SelectedUser == null)
            {
                return;
            }

            Username = SelectedUser.Username;
            FullName = SelectedUser.FullName ?? string.Empty;
            IsActive = SelectedUser.IsActive;
            Password = string.Empty;
        }

        private async Task LoadPermissionsAsync()
        {
            Permissions.Clear();
            if (SelectedRole == null)
            {
                return;
            }

            var granted = await _userAdminService.GetRolePermissionCodesAsync(SelectedRole.RoleId);
            foreach (var code in PermissionCodes.All)
            {
                Permissions.Add(new PermissionToggle(code, granted.Contains(code)));
            }
        }

        private async Task SaveUserAsync()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                StatusMessage = "أدخل اسم المستخدم.";
                return;
            }

            try
            {
                if (SelectedUser == null || SelectedUser.UserId == 0)
                {
                    var user = await _userAdminService.CreateUserAsync(new User
                    {
                        Username = Username,
                        FullName = FullName,
                        IsActive = IsActive
                    }, Password);

                    Users.Add(user);
                    SelectedUser = user;
                    StatusMessage = "تم إنشاء المستخدم.";
                }
                else
                {
                    await _userAdminService.UpdateUserAsync(new User
                    {
                        UserId = SelectedUser.UserId,
                        Username = Username,
                        FullName = FullName,
                        IsActive = IsActive
                    }, Password);

                    StatusMessage = "تم تحديث المستخدم.";
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null)
            {
                return;
            }

            try
            {
                await _userAdminService.DeleteUserAsync(SelectedUser.UserId);
                StatusMessage = "تم حذف المستخدم.";
                SelectedUser = null;
                await LoadAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task SaveRoleAsync()
        {
            if (string.IsNullOrWhiteSpace(RoleName))
            {
                StatusMessage = "أدخل اسم الدور.";
                return;
            }

            try
            {
                var role = await _userAdminService.CreateRoleAsync(RoleName);
                Roles.Add(role);
                SelectedRole = role;
                RoleName = string.Empty;
                StatusMessage = "تم إنشاء الدور.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task DeleteRoleAsync()
        {
            if (SelectedRole == null)
            {
                return;
            }

            try
            {
                await _userAdminService.DeleteRoleAsync(SelectedRole.RoleId);
                StatusMessage = "تم حذف الدور.";
                SelectedRole = null;
                await LoadAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task AssignRoleAsync()
        {
            if (SelectedUser == null || SelectedRole == null)
            {
                return;
            }

            try
            {
                await _userAdminService.AssignSingleRoleAsync(SelectedUser.UserId, SelectedRole.RoleId);
                StatusMessage = "تم ربط المستخدم بالدور.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task UnassignRoleAsync()
        {
            if (SelectedUser == null || SelectedRole == null)
            {
                return;
            }

            try
            {
                await _userAdminService.RemoveUserRoleAsync(SelectedUser.UserId, SelectedRole.RoleId);
                StatusMessage = "تم فك ربط الدور من المستخدم.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task SaveRolePermissionsAsync()
        {
            if (SelectedRole == null)
            {
                return;
            }

            try
            {
                await _userAdminService.SaveRolePermissionsAsync(SelectedRole.RoleId, Permissions.Where(p => p.IsGranted).Select(p => p.Code));
                StatusMessage = "تم حفظ الصلاحيات.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }
    }
}
