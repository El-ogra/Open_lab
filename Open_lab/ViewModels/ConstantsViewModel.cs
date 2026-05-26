using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ConstantsViewModel : BaseViewModel
    {
        private readonly IConstantsService _constantsService;
        private Setting? _selected;
        private string _key = string.Empty;
        private string _value = string.Empty;
        private string _statusMessage = string.Empty;

        public ConstantsViewModel(IConstantsService constantsService)
        {
            _constantsService = constantsService;
            Items = new ObservableCollection<Setting>();

            SeedDefaultsCommand = new RelayCommand(async _ => await SeedDefaultsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ConstantsEdit));
            ReloadCommand = new RelayCommand(async _ => await LoadAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ConstantsView));
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ConstantsEdit));
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.ConstantsEdit) && Selected != null);

            _ = LoadAsync();
        }

        public ObservableCollection<Setting> Items { get; }

        public Setting? Selected
        {
            get => _selected;
            set
            {
                if (SetProperty(ref _selected, value))
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

        public ICommand SeedDefaultsCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var rows = await _constantsService.GetConstantsAsync();
                Items.Clear();
                foreach (var row in rows)
                {
                    Items.Add(row);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task SeedDefaultsAsync()
        {
            try
            {
                await _constantsService.SeedDefaultsAsync();
                await LoadAsync();
                StatusMessage = "تم إدراج الثوابت الافتراضية.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Key))
            {
                StatusMessage = "أدخل اسم الثابت.";
                return;
            }

            try
            {
                await _constantsService.SaveConstantAsync(Key, Value);
                await LoadAsync();
                StatusMessage = "تم حفظ الثابت.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task DeleteAsync()
        {
            if (Selected == null)
            {
                return;
            }

            try
            {
                await _constantsService.DeleteConstantAsync(Selected.Key);
                await LoadAsync();
                Selected = null;
                StatusMessage = "تم حذف الثابت.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }
    }
}
