using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly IPrintService _printService;

        private ObservableCollection<SystemSetting> _settings = new();
        private SystemSetting? _selectedSetting;
        private ObservableCollection<string> _availablePrinters = new();

        // Printer settings
        private string _defaultPrinter = string.Empty;
        private string _receiptPrinter = string.Empty;
        private string _reportPrinter = string.Empty;

        // Margin settings
        private decimal _leftMargin;
        private decimal _rightMargin;

        public SettingsViewModel(ISettingsService settingsService, IPrintService printService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _printService = printService ?? throw new ArgumentNullException(nameof(printService));

            LoadSettingsCommand = new RelayCommand(async _ => await LoadSettingsAsync());
            SavePrinterSettingsCommand = new RelayCommand(async _ => await SavePrinterSettingsAsync());
            SaveMarginSettingsCommand = new RelayCommand(async _ => await SaveMarginSettingsAsync());
            RefreshPrintersCommand = new RelayCommand(_ => LoadAvailablePrinters());

            LoadAvailablePrinters();
        }

        public ObservableCollection<SystemSetting> Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public SystemSetting? SelectedSetting
        {
            get => _selectedSetting;
            set => SetProperty(ref _selectedSetting, value);
        }

        public ObservableCollection<string> AvailablePrinters
        {
            get => _availablePrinters;
            set => SetProperty(ref _availablePrinters, value);
        }

        public string DefaultPrinter
        {
            get => _defaultPrinter;
            set => SetProperty(ref _defaultPrinter, value);
        }

        public string ReceiptPrinter
        {
            get => _receiptPrinter;
            set => SetProperty(ref _receiptPrinter, value);
        }

        public string ReportPrinter
        {
            get => _reportPrinter;
            set => SetProperty(ref _reportPrinter, value);
        }

        public decimal LeftMargin
        {
            get => _leftMargin;
            set => SetProperty(ref _leftMargin, value);
        }

        public decimal RightMargin
        {
            get => _rightMargin;
            set => SetProperty(ref _rightMargin, value);
        }

        public ICommand LoadSettingsCommand { get; }
        public ICommand SavePrinterSettingsCommand { get; }
        public ICommand SaveMarginSettingsCommand { get; }
        public ICommand RefreshPrintersCommand { get; }

        private void LoadAvailablePrinters()
        {
            try
            {
                var server = new LocalPrintServer();
                var queues = server.GetPrintQueues(new[]
                {
                    EnumeratedPrintQueueTypes.Local,
                    EnumeratedPrintQueueTypes.Connections
                });

                AvailablePrinters.Clear();
                AvailablePrinters.Add(string.Empty); // Empty option
                foreach (var queue in queues.OrderBy(q => q.Name))
                {
                    AvailablePrinters.Add(queue.Name);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error loading printers: {ex.Message}");
            }
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                var settings = await _settingsService.GetAllSettingsAsync();
                Settings = new ObservableCollection<SystemSetting>(settings);

                // Load printer settings
                DefaultPrinter = await _settingsService.GetDefaultPrinterAsync();
                ReceiptPrinter = await _settingsService.GetReceiptPrinterAsync();
                ReportPrinter = await _settingsService.GetReportPrinterAsync();

                // Load margin settings
                LeftMargin = await _settingsService.GetLeftMarginAsync();
                RightMargin = await _settingsService.GetRightMarginAsync();
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private async Task SavePrinterSettingsAsync()
        {
            try
            {
                await _settingsService.SetDefaultPrinterAsync(DefaultPrinter);
                await _settingsService.SetReceiptPrinterAsync(ReceiptPrinter);
                await _settingsService.SetReportPrinterAsync(ReportPrinter);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving printer settings: {ex.Message}");
            }
        }

        private async Task SaveMarginSettingsAsync()
        {
            try
            {
                await _settingsService.SetLeftMarginAsync(LeftMargin);
                await _settingsService.SetRightMarginAsync(RightMargin);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving margin settings: {ex.Message}");
            }
        }
    }
}
