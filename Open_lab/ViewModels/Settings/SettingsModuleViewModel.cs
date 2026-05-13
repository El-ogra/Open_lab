using System;
using System.Windows.Input;
using Open_lab.ViewModels;

namespace Open_lab.ViewModels.Settings
{
    public class SettingsModuleViewModel : BaseViewModel
    {
        private readonly Action<string> _openPlaceholder;

        public SettingsModuleViewModel(Action<string> openPlaceholder)
        {
            _openPlaceholder = openPlaceholder ?? throw new ArgumentNullException(nameof(openPlaceholder));

            OpenSystemSettingsCommand = new RelayCommand(_ => _openPlaceholder("اعدادات النظام"));
            OpenDatabaseMaintenanceCommand = new RelayCommand(_ => _openPlaceholder("Database Maintenance"));
        }

        public ICommand OpenSystemSettingsCommand { get; }
        public ICommand OpenDatabaseMaintenanceCommand { get; }
    }
}
