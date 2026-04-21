using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    /// <summary>
    /// Service for managing system-wide configuration settings.
    /// </summary>
    public interface ISettingsService
    {
        Task<SystemSetting?> GetSettingAsync(string key);
        Task<string?> GetStringAsync(string key, string? defaultValue = null);
        Task<int> GetIntAsync(string key, int defaultValue = 0);
        Task<decimal> GetDecimalAsync(string key, decimal defaultValue = 0);
        Task<bool> GetBoolAsync(string key, bool defaultValue = false);
        Task<T?> GetJsonAsync<T>(string key) where T : class;

        Task SetSettingAsync(string key, string value, string? description = null);
        Task SetSettingAsync(string key, int value, string? description = null);
        Task SetSettingAsync(string key, decimal value, string? description = null);
        Task SetSettingAsync(string key, bool value, string? description = null);
        Task SetJsonAsync<T>(string key, T value, string? description = null) where T : class;

        Task<List<SystemSetting>> GetAllSettingsAsync();
        Task DeleteSettingAsync(string key);

        // Printer settings
        Task<string> GetDefaultPrinterAsync();
        Task SetDefaultPrinterAsync(string printerName);
        Task<string> GetReceiptPrinterAsync();
        Task SetReceiptPrinterAsync(string printerName);
        Task<string> GetReportPrinterAsync();
        Task SetReportPrinterAsync(string printerName);

        // Margin settings
        Task<decimal> GetLeftMarginAsync();
        Task SetLeftMarginAsync(decimal marginCm);
        Task<decimal> GetRightMarginAsync();
        Task SetRightMarginAsync(decimal marginCm);
    }
}
