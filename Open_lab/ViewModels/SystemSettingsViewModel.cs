using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.ViewModels
{
    public class SystemSettingsViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private Setting? _selectedSetting;
        private string _key = string.Empty;
        private string _value = string.Empty;
        private string _statusMessage = string.Empty;

        public SystemSettingsViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            Settings = new ObservableCollection<Setting>();
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedSetting != null);
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
            using var db = _dbFactory();
            var items = await db.Settings.AsNoTracking().OrderBy(s => s.Key).ToListAsync();
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

            using var db = _dbFactory();
            var setting = await db.Settings.FirstOrDefaultAsync(s => s.Key == Key);
            if (setting == null)
            {
                setting = new Setting { Key = Key, Value = Value };
                db.Settings.Add(setting);
            }
            else
            {
                setting.Value = Value;
            }
            await db.SaveChangesAsync();
            await LoadAsync();
            StatusMessage = "تم حفظ الإعداد.";
        }

        private async Task DeleteAsync()
        {
            if (SelectedSetting == null)
            {
                return;
            }

            using var db = _dbFactory();
            var setting = await db.Settings.FirstAsync(s => s.Key == SelectedSetting.Key);
            db.Settings.Remove(setting);
            await db.SaveChangesAsync();
            await LoadAsync();
            SelectedSetting = null;
            StatusMessage = "تم حذف الإعداد.";
        }
    }
}
