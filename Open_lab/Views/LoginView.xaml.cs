using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Open_lab.ViewModels;

namespace Open_lab.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            this.DataContextChanged += LoginView_DataContextChanged;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Resizing is now handled by MainViewModel for better reliability
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Resizing is now handled by MainViewModel
        }

        private void LoginView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is LoginViewModel oldVm)
            {
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;
            }
            if (e.NewValue is LoginViewModel newVm)
            {
                newVm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginViewModel.IsPasswordVisible))
            {
                if (DataContext is LoginViewModel vm && !vm.IsPasswordVisible)
                {
                    // Sync ViewModel password to PasswordBox when hiding
                    if (PasswordInput.Password != vm.Password)
                    {
                        PasswordInput.Password = vm.Password;
                    }
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LoginViewModel vm && sender is PasswordBox passwordBox)
            {
                // To avoid recursive updates if we were to sync back, 
                // but since PasswordBox doesn't bind, this is the primary source.
                if (vm.Password != passwordBox.Password)
                {
                    vm.Password = passwordBox.Password;
                }
            }
        }
    }
}
