using System.Windows.Controls;
using Open_lab.ViewModels;

namespace Open_lab.Views
{
    public partial class SystemSettingsView : UserControl
    {
        public SystemSettingsView()
        {
            InitializeComponent();
        }

        private void CurrentPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SystemSettingsViewModel vm && sender is PasswordBox passwordBox)
            {
                vm.CurrentMasterPassword = passwordBox.Password;
            }
        }

        private void NewPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SystemSettingsViewModel vm && sender is PasswordBox passwordBox)
            {
                vm.NewMasterPassword = passwordBox.Password;
            }
        }

        private void ConfirmPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SystemSettingsViewModel vm && sender is PasswordBox passwordBox)
            {
                vm.ConfirmMasterPassword = passwordBox.Password;
            }
        }
    }
}
