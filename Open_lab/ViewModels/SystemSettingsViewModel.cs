using System;
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
        private string _reportHeader = string.Empty;
        private string _reportFooter = string.Empty;
        private double _reportMarginTop = 1.5;
        private double _reportMarginBottom = 1.5;
        private string _reportPrimaryColor = "#2B2B2B";
        private string _defaultPrinterName = "Microsoft Print to PDF";
        private string _receiptHeaderText = "إيصال مختبر";
        private string _receiptFooterText = "شكراً لتعاملكم";
        private bool _receiptShowLogo;
        private int _receiptCopies = 1;
        private string _statusMessage = string.Empty;

        public SystemSettingsViewModel(ISystemSettingsService settingsService)
        {
            _settingsService = settingsService;
            Settings = new ObservableCollection<Setting>();

            SaveProfileCommand = new RelayCommand(async _ => await SaveProfileAsync(), _ => AppSession.HasPermission(PermissionCodes.SettingsEdit));
            ReloadCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.SettingsView));
            SaveRawSettingCommand = new RelayCommand(async _ => await SaveRawSettingAsync(), _ => AppSession.HasPermission(PermissionCodes.SettingsEdit));
            DeleteRawSettingCommand = new RelayCommand(async _ => await DeleteRawSettingAsync(), _ => AppSession.HasPermission(PermissionCodes.SettingsEdit) && SelectedSetting != null);

            _ = LoadAsync();
        }

        public ObservableCollection<Setting> Settings { get; }

        public string ReportHeader
        {
            get => _reportHeader;
            set => SetProperty(ref _reportHeader, value);
        }

        public string ReportFooter
        {
            get => _reportFooter;
            set => SetProperty(ref _reportFooter, value);
        }

        public double ReportMarginTop
        {
            get => _reportMarginTop;
            set => SetProperty(ref _reportMarginTop, value);
        }

        public double ReportMarginBottom
        {
            get => _reportMarginBottom;
            set => SetProperty(ref _reportMarginBottom, value);
        }

        public string ReportPrimaryColor
        {
            get => _reportPrimaryColor;
            set => SetProperty(ref _reportPrimaryColor, value);
        }

        public string DefaultPrinterName
        {
            get => _defaultPrinterName;
            set => SetProperty(ref _defaultPrinterName, value);
        }

        public string ReceiptHeaderText
        {
            get => _receiptHeaderText;
            set => SetProperty(ref _receiptHeaderText, value);
        }

        public string ReceiptFooterText
        {
            get => _receiptFooterText;
            set => SetProperty(ref _receiptFooterText, value);
        }

        public bool ReceiptShowLogo
        {
            get => _receiptShowLogo;
            set => SetProperty(ref _receiptShowLogo, value);
        }

        public int ReceiptCopies
        {
            get => _receiptCopies;
            set => SetProperty(ref _receiptCopies, value);
        }

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

                    (DeleteRawSettingCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

        public ICommand SaveProfileCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SaveRawSettingCommand { get; }
        public ICommand DeleteRawSettingCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var profile = await _settingsService.GetProfileAsync();
                ReportHeader = profile.ReportHeader;
                ReportFooter = profile.ReportFooter;
                ReportMarginTop = profile.ReportMarginTop;
                ReportMarginBottom = profile.ReportMarginBottom;
                ReportPrimaryColor = profile.ReportPrimaryColor;
                DefaultPrinterName = profile.DefaultPrinterName;
                ReceiptHeaderText = profile.ReceiptHeaderText;
                ReceiptFooterText = profile.ReceiptFooterText;
                ReceiptShowLogo = profile.ReceiptShowLogo;
                ReceiptCopies = profile.ReceiptCopies;

                var items = await _settingsService.GetSettingsAsync();
                Settings.Clear();
                foreach (var item in items)
                {
                    Settings.Add(item);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task SaveProfileAsync()
        {
            try
            {
                if (ReceiptCopies < 1)
                {
                    ReceiptCopies = 1;
                }

                await _settingsService.SaveProfileAsync(new SystemSettingsProfile
                {
                    ReportHeader = ReportHeader,
                    ReportFooter = ReportFooter,
                    ReportMarginTop = ReportMarginTop,
                    ReportMarginBottom = ReportMarginBottom,
                    ReportPrimaryColor = ReportPrimaryColor,
                    DefaultPrinterName = DefaultPrinterName,
                    ReceiptHeaderText = ReceiptHeaderText,
                    ReceiptFooterText = ReceiptFooterText,
                    ReceiptShowLogo = ReceiptShowLogo,
                    ReceiptCopies = ReceiptCopies
                });

                await LoadAsync();
                StatusMessage = "تم حفظ إعدادات النظام المتخصصة.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task SaveRawSettingAsync()
        {
            if (string.IsNullOrWhiteSpace(Key))
            {
                StatusMessage = "أدخل المفتاح.";
                return;
            }

            try
            {
                await _settingsService.SaveSettingAsync(Key, Value);
                await LoadAsync();
                StatusMessage = "تم حفظ الإعداد المتقدم.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task DeleteRawSettingAsync()
        {
            if (SelectedSetting == null)
            {
                return;
            }

            try
            {
                await _settingsService.DeleteSettingAsync(SelectedSetting.Key);
                await LoadAsync();
                SelectedSetting = null;
                StatusMessage = "تم حذف الإعداد المتقدم.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }
    }
}
