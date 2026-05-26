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
        private double _reportMarginLeft = 1.5;
        private double _reportMarginRight = 1.5;
        private string _reportPrimaryColor = "#2B2B2B";
        private string _reportLogoPath = string.Empty;
        private string _reportPrinterName = "Microsoft Print to PDF";
        private string _receiptPrinterName = "Microsoft Print to PDF";
        private string _barcodePrinterName = "Microsoft Print to PDF";
        private string _envelopePrinterName = "Microsoft Print to PDF";
        private string _receiptHeaderText = "إيصال مختبر";
        private string _receiptFooterText = "شكراً لتعاملكم";
        private bool _receiptShowLogo;
        private int _receiptCopies = 1;
        private string _invoiceLogoPath = string.Empty;
        private string _invoiceCurrency = "EGP";
        private bool _invoiceShowMedicalDetails = true;
        private string _reportPaperSize = "A4";
        private string _defaultAccountType = "Cash";
        private string _currentMasterPassword = string.Empty;
        private string _newMasterPassword = string.Empty;
        private string _confirmMasterPassword = string.Empty;
        private string _statusMessage = string.Empty;

        public SystemSettingsViewModel(ISystemSettingsService settingsService)
        {
            _settingsService = settingsService;
            Settings = new ObservableCollection<Setting>();

            SaveProfileCommand = new RelayCommand(async _ => await SaveProfileAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.SettingsEdit));
            ReloadCommand = new RelayCommand(async _ => await LoadAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.SettingsView));
            SaveRawSettingCommand = new RelayCommand(async _ => await SaveRawSettingAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.SettingsEdit));
            DeleteRawSettingCommand = new RelayCommand(async _ => await DeleteRawSettingAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.SettingsEdit) && SelectedSetting != null);
            ChangeMasterPasswordCommand = new RelayCommand(async _ => await ChangeMasterPasswordAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.SettingsEdit));

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

        public double ReportMarginLeft
        {
            get => _reportMarginLeft;
            set => SetProperty(ref _reportMarginLeft, value);
        }

        public double ReportMarginRight
        {
            get => _reportMarginRight;
            set => SetProperty(ref _reportMarginRight, value);
        }

        public string ReportPrimaryColor
        {
            get => _reportPrimaryColor;
            set => SetProperty(ref _reportPrimaryColor, value);
        }

        public string ReportLogoPath
        {
            get => _reportLogoPath;
            set => SetProperty(ref _reportLogoPath, value);
        }

        public string ReportPrinterName
        {
            get => _reportPrinterName;
            set => SetProperty(ref _reportPrinterName, value);
        }

        public string ReceiptPrinterName
        {
            get => _receiptPrinterName;
            set => SetProperty(ref _receiptPrinterName, value);
        }

        public string BarcodePrinterName
        {
            get => _barcodePrinterName;
            set => SetProperty(ref _barcodePrinterName, value);
        }

        public string EnvelopePrinterName
        {
            get => _envelopePrinterName;
            set => SetProperty(ref _envelopePrinterName, value);
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

        public string InvoiceLogoPath
        {
            get => _invoiceLogoPath;
            set => SetProperty(ref _invoiceLogoPath, value);
        }

        public string InvoiceCurrency
        {
            get => _invoiceCurrency;
            set => SetProperty(ref _invoiceCurrency, value);
        }

        public bool InvoiceShowMedicalDetails
        {
            get => _invoiceShowMedicalDetails;
            set => SetProperty(ref _invoiceShowMedicalDetails, value);
        }

        public string ReportPaperSize
        {
            get => _reportPaperSize;
            set => SetProperty(ref _reportPaperSize, value);
        }

        public string DefaultAccountType
        {
            get => _defaultAccountType;
            set => SetProperty(ref _defaultAccountType, value);
        }

        public string CurrentMasterPassword
        {
            get => _currentMasterPassword;
            set => SetProperty(ref _currentMasterPassword, value);
        }

        public string NewMasterPassword
        {
            get => _newMasterPassword;
            set => SetProperty(ref _newMasterPassword, value);
        }

        public string ConfirmMasterPassword
        {
            get => _confirmMasterPassword;
            set => SetProperty(ref _confirmMasterPassword, value);
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
        public ICommand ChangeMasterPasswordCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var profile = await _settingsService.GetProfileAsync();
                ReportHeader = profile.ReportHeader;
                ReportFooter = profile.ReportFooter;
                ReportMarginTop = profile.ReportMarginTop;
                ReportMarginBottom = profile.ReportMarginBottom;
                ReportMarginLeft = profile.ReportMarginLeft;
                ReportMarginRight = profile.ReportMarginRight;
                ReportPrimaryColor = profile.ReportPrimaryColor;
                ReportLogoPath = profile.ReportLogoPath;
                ReportPrinterName = profile.ReportPrinterName;
                ReceiptPrinterName = profile.ReceiptPrinterName;
                BarcodePrinterName = profile.BarcodePrinterName;
                EnvelopePrinterName = profile.EnvelopePrinterName;
                ReceiptHeaderText = profile.ReceiptHeaderText;
                ReceiptFooterText = profile.ReceiptFooterText;
                ReceiptShowLogo = profile.ReceiptShowLogo;
                ReceiptCopies = profile.ReceiptCopies;
                InvoiceLogoPath = profile.InvoiceLogoPath;
                InvoiceCurrency = profile.InvoiceCurrency;
                InvoiceShowMedicalDetails = profile.InvoiceShowMedicalDetails;
                ReportPaperSize = profile.ReportPaperSize;
                DefaultAccountType = profile.DefaultAccountType;

                var items = await _settingsService.GetSettingsAsync();
                Settings.Clear();
                foreach (var item in items)
                {
                    Settings.Add(item);
                }
                StatusMessage = "تم تحميل الإعدادات بنجاح.";
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

                if (ReportMarginTop < 0 || ReportMarginBottom < 0 || ReportMarginLeft < 0 || ReportMarginRight < 0)
                {
                    StatusMessage = "هوامش التقرير لا يمكن أن تكون سالبة.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(InvoiceCurrency))
                {
                    StatusMessage = "أدخل عملة الفاتورة.";
                    return;
                }

                await _settingsService.SaveProfileAsync(new SystemSettingsProfile
                {
                    ReportHeader = ReportHeader,
                    ReportFooter = ReportFooter,
                    ReportMarginTop = ReportMarginTop,
                    ReportMarginBottom = ReportMarginBottom,
                    ReportMarginLeft = ReportMarginLeft,
                    ReportMarginRight = ReportMarginRight,
                    ReportPrimaryColor = ReportPrimaryColor,
                    ReportLogoPath = ReportLogoPath,
                    ReportPrinterName = ReportPrinterName,
                    ReceiptPrinterName = ReceiptPrinterName,
                    BarcodePrinterName = BarcodePrinterName,
                    EnvelopePrinterName = EnvelopePrinterName,
                    ReceiptHeaderText = ReceiptHeaderText,
                    ReceiptFooterText = ReceiptFooterText,
                    ReceiptShowLogo = ReceiptShowLogo,
                    ReceiptCopies = ReceiptCopies,
                    InvoiceLogoPath = InvoiceLogoPath,
                    InvoiceCurrency = InvoiceCurrency,
                    InvoiceShowMedicalDetails = InvoiceShowMedicalDetails,
                    ReportPaperSize = ReportPaperSize,
                    DefaultAccountType = DefaultAccountType
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

        private async Task ChangeMasterPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentMasterPassword))
            {
                StatusMessage = "أدخل كلمة المرور الحالية.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewMasterPassword))
            {
                StatusMessage = "أدخل كلمة المرور الجديدة.";
                return;
            }

            if (!string.Equals(NewMasterPassword, ConfirmMasterPassword, StringComparison.Ordinal))
            {
                StatusMessage = "تأكيد كلمة المرور غير مطابق.";
                return;
            }

            try
            {
                var isValid = await _settingsService.VerifyMasterPasswordAsync(CurrentMasterPassword);
                if (!isValid)
                {
                    StatusMessage = "كلمة المرور الحالية غير صحيحة.";
                    return;
                }

                var changed = await _settingsService.SetMasterPasswordAsync(NewMasterPassword);
                if (!changed)
                {
                    StatusMessage = "تعذر تحديث كلمة المرور.";
                    return;
                }

                CurrentMasterPassword = string.Empty;
                NewMasterPassword = string.Empty;
                ConfirmMasterPassword = string.Empty;
                StatusMessage = "تم تحديث كلمة مرور النظام.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }
    }
}
