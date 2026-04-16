using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class SystemSettingsViewModel : BaseViewModel
    {
        private readonly ISystemSettingsService _settingsService;
        private Setting? _selectedSetting;
        private string _key = string.Empty;
        private string _value = string.Empty;
        private string _statusMessage = string.Empty;

        public SystemSettingsViewModel(ISystemSettingsService settingsService)
        {
            _settingsService = settingsService;
            Settings = new ObservableCollection<Setting>();
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => AppSession.HasPermission(PermissionCodes.SettingsEdit));
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => AppSession.HasPermission(PermissionCodes.SettingsEdit) && SelectedSetting != null);
            _ = LoadAsync();
        }

        public ObservableCollection<Setting> Settings { get; }

        public Setting? SelectedSetting
        {
            get => _selectedSetting;
            set
            {
                if (SetProperty(ref _selectedSetting, value))
                {
                    if (value != null)
                    {
                        Key = value.Key;
                        Value = value.Value ?? string.Empty;
                    }
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        private async Task LoadAsync()
        {
            var items = await _settingsService.GetSettingsAsync();
            Settings.Clear();
            foreach (var item in items)
            {
                Settings.Add(item);
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Key))
            {
                StatusMessage = "أدخل المفتاح.";
                return;
            }

            await _settingsService.SaveSettingAsync(Key, Value);
            await LoadAsync();
            StatusMessage = "تم حفظ الإعداد.";
        }

        private async Task DeleteAsync()
        {
            if (SelectedSetting == null)
            {
                return;
            }

            await _settingsService.DeleteSettingAsync(SelectedSetting.Key);
            await LoadAsync();
            SelectedSetting = null;
            StatusMessage = "تم حذف الإعداد.";
        }
    }
}

