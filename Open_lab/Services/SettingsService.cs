using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly OpenLabDbContext _db;
        private const string TYPE_STRING = "String";
        private const string TYPE_INT = "Int";
        private const string TYPE_DECIMAL = "Decimal";
        private const string TYPE_BOOL = "Bool";
        private const string TYPE_JSON = "Json";

        public SettingsService(OpenLabDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<SystemSetting?> GetSettingAsync(string key)
        {
            return await _db.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
        }

        public async Task<string?> GetStringAsync(string key, string? defaultValue = null)
        {
            var setting = await GetSettingAsync(key);
            return setting?.SettingValue ?? defaultValue;
        }

        public async Task<int> GetIntAsync(string key, int defaultValue = 0)
        {
            var value = await GetStringAsync(key);
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        public async Task<decimal> GetDecimalAsync(string key, decimal defaultValue = 0)
        {
            var value = await GetStringAsync(key);
            return decimal.TryParse(value, out var result) ? result : defaultValue;
        }

        public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
        {
            var value = await GetStringAsync(key);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        public async Task<T?> GetJsonAsync<T>(string key) where T : class
        {
            var json = await GetStringAsync(key);
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return null;
            }
        }

        public async Task SetSettingAsync(string key, string value, string? description = null)
        {
            await SetSettingInternalAsync(key, value, TYPE_STRING, description);
        }

        public async Task SetSettingAsync(string key, int value, string? description = null)
        {
            await SetSettingInternalAsync(key, value.ToString(), TYPE_INT, description);
        }

        public async Task SetSettingAsync(string key, decimal value, string? description = null)
        {
            await SetSettingInternalAsync(key, value.ToString(), TYPE_DECIMAL, description);
        }

        public async Task SetSettingAsync(string key, bool value, string? description = null)
        {
            await SetSettingInternalAsync(key, value.ToString(), TYPE_BOOL, description);
        }

        public async Task SetJsonAsync<T>(string key, T value, string? description = null) where T : class
        {
            var json = JsonSerializer.Serialize(value);
            await SetSettingInternalAsync(key, json, TYPE_JSON, description);
        }

        private async Task SetSettingInternalAsync(string key, string value, string type, string? description)
        {
            var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    SettingType = type,
                    Description = description,
                    LastModified = DateTime.Now
                };
                _db.SystemSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = value;
                setting.SettingType = type;
                if (description != null) setting.Description = description;
                setting.LastModified = DateTime.Now;
            }
            await _db.SaveChangesAsync();
        }

        public async Task<List<SystemSetting>> GetAllSettingsAsync()
        {
            return await _db.SystemSettings.OrderBy(s => s.SettingKey).ToListAsync();
        }

        public async Task DeleteSettingAsync(string key)
        {
            var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting != null)
            {
                _db.SystemSettings.Remove(setting);
                await _db.SaveChangesAsync();
            }
        }

        // Printer settings
        public Task<string> GetDefaultPrinterAsync() => GetStringAsync("Printer.Default", "Microsoft Print to PDF")!;
        public Task SetDefaultPrinterAsync(string printerName) => SetSettingAsync("Printer.Default", printerName, "Default printer for general printing");
        public Task<string> GetReceiptPrinterAsync() => GetStringAsync("Printer.Receipt", string.Empty)!;
        public Task SetReceiptPrinterAsync(string printerName) => SetSettingAsync("Printer.Receipt", printerName, "Printer for receipts");
        public Task<string> GetReportPrinterAsync() => GetStringAsync("Printer.Report", string.Empty)!;
        public Task SetReportPrinterAsync(string printerName) => SetSettingAsync("Printer.Report", printerName, "Printer for reports");

        // Margin settings
        public Task<decimal> GetLeftMarginAsync() => GetDecimalAsync("Margin.Left", 0);
        public Task SetLeftMarginAsync(decimal marginCm) => SetSettingAsync("Margin.Left", marginCm, "Left margin in centimeters");
        public Task<decimal> GetRightMarginAsync() => GetDecimalAsync("Margin.Right", 0);
        public Task SetRightMarginAsync(decimal marginCm) => SetSettingAsync("Margin.Right", marginCm, "Right margin in centimeters");
    }
}
