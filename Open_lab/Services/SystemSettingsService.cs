using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private const string ReportHeaderKey = "Report.Header";
        private const string ReportFooterKey = "Report.Footer";
        private const string ReportMarginTopKey = "Report.MarginTop";
        private const string ReportMarginBottomKey = "Report.MarginBottom";
        private const string ReportPrimaryColorKey = "Report.PrimaryColor";
        private const string PrinterReportKey = "Printer.Report";
        private const string PrinterReceiptKey = "Printer.Receipt";
        private const string PrinterBarcodeKey = "Printer.Barcode";
        private const string PrinterEnvelopeKey = "Printer.Envelope";
        private const string ReceiptHeaderKey = "Receipt.Header";
        private const string ReceiptFooterKey = "Receipt.Footer";
        private const string ReceiptShowLogoKey = "Receipt.ShowLogo";
        private const string ReceiptCopiesKey = "Receipt.Copies";
        private const string PrinterPaperSizeKey = "Printer.PaperSize";
        private const string DefaultAccountTypeKey = "Invoice.DefaultAccountType";
        private const string MasterPasswordHashKey = "Security.MasterPasswordHash";
        private const string MasterPasswordSaltKey = "Security.MasterPasswordSalt";


        private readonly OpenLabDbContext _db;

        public SystemSettingsService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<List<Setting>> GetSettingsAsync()
        {
            return _db.Settings.AsNoTracking().OrderBy(s => s.Key).ToListAsync();
        }

        public async Task SaveSettingAsync(string key, string? value)
        {
            var setting = await _db.Settings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                setting = new Setting { Key = key, Value = value };
                _db.Settings.Add(setting);
            }
            else
            {
                setting.Value = value;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteSettingAsync(string key)
        {
            var setting = await _db.Settings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                return;
            }

            _db.Settings.Remove(setting);
            await _db.SaveChangesAsync();
        }

        public async Task<SystemSettingsProfile> GetProfileAsync()
        {
            var dictionary = await _db.Settings
                .AsNoTracking()
                .Where(s =>
                    s.Key == ReportHeaderKey ||
                    s.Key == ReportFooterKey ||
                    s.Key == ReportMarginTopKey ||
                    s.Key == ReportMarginBottomKey ||
                    s.Key == ReportPrimaryColorKey ||
                    s.Key == PrinterReportKey ||
                    s.Key == PrinterReceiptKey ||
                    s.Key == PrinterBarcodeKey ||
                    s.Key == PrinterEnvelopeKey ||
                    s.Key == ReceiptFooterKey ||
                    s.Key == ReceiptShowLogoKey ||
                    s.Key == ReceiptCopiesKey ||
                    s.Key == PrinterPaperSizeKey ||
                    s.Key == DefaultAccountTypeKey ||
                    s.Key == MasterPasswordHashKey)
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            return new SystemSettingsProfile
            {
                ReportHeader = GetValue(dictionary, ReportHeaderKey, "Open_lab"),
                ReportFooter = GetValue(dictionary, ReportFooterKey, string.Empty),
                ReportMarginTop = ParseDouble(GetValue(dictionary, ReportMarginTopKey, "1.5"), 1.5),
                ReportMarginBottom = ParseDouble(GetValue(dictionary, ReportMarginBottomKey, "1.5"), 1.5),
                ReportPrimaryColor = GetValue(dictionary, ReportPrimaryColorKey, "#2B2B2B"),
                ReportPrinterName = GetValue(dictionary, PrinterReportKey, "Microsoft Print to PDF"),
                ReceiptPrinterName = GetValue(dictionary, PrinterReceiptKey, "Microsoft Print to PDF"),
                BarcodePrinterName = GetValue(dictionary, PrinterBarcodeKey, "Microsoft Print to PDF"),
                EnvelopePrinterName = GetValue(dictionary, PrinterEnvelopeKey, "Microsoft Print to PDF"),
                ReceiptHeaderText = GetValue(dictionary, ReceiptHeaderKey, "إيصال مختبر"),
                ReceiptFooterText = GetValue(dictionary, ReceiptFooterKey, "شكراً لتعاملكم"),
                ReceiptShowLogo = ParseBool(GetValue(dictionary, ReceiptShowLogoKey, "false")),
                ReceiptCopies = ParseInt(GetValue(dictionary, ReceiptCopiesKey, "1"), 1),
                ReportPaperSize = GetValue(dictionary, PrinterPaperSizeKey, "A4"),
                DefaultAccountType = GetValue(dictionary, DefaultAccountTypeKey, "Cash"),
                MasterPasswordHash = dictionary.TryGetValue(MasterPasswordHashKey, out var hash) ? hash : null
            };
        }

        public async Task SaveProfileAsync(SystemSettingsProfile profile)
        {
            await SaveSettingAsync(ReportHeaderKey, profile.ReportHeader);
            await SaveSettingAsync(ReportFooterKey, profile.ReportFooter);
            await SaveSettingAsync(ReportMarginTopKey, profile.ReportMarginTop.ToString(CultureInfo.InvariantCulture));
            await SaveSettingAsync(ReportMarginBottomKey, profile.ReportMarginBottom.ToString(CultureInfo.InvariantCulture));
            await SaveSettingAsync(ReportPrimaryColorKey, profile.ReportPrimaryColor);
            await SaveSettingAsync(PrinterReportKey, profile.ReportPrinterName);
            await SaveSettingAsync(PrinterReceiptKey, profile.ReceiptPrinterName);
            await SaveSettingAsync(PrinterBarcodeKey, profile.BarcodePrinterName);
            await SaveSettingAsync(PrinterEnvelopeKey, profile.EnvelopePrinterName);
            await SaveSettingAsync(ReceiptHeaderKey, profile.ReceiptHeaderText);
            await SaveSettingAsync(ReceiptFooterKey, profile.ReceiptFooterText);
            await SaveSettingAsync(ReceiptShowLogoKey, profile.ReceiptShowLogo ? "true" : "false");
            await SaveSettingAsync(ReceiptCopiesKey, profile.ReceiptCopies.ToString(CultureInfo.InvariantCulture));
            await SaveSettingAsync(PrinterPaperSizeKey, profile.ReportPaperSize);
            await SaveSettingAsync(DefaultAccountTypeKey, profile.DefaultAccountType);

            if (!string.IsNullOrEmpty(profile.MasterPasswordHash))
            {
                // Generate a new salt for the master password
                var newSalt = GenerateSecureSalt();
                await SaveSettingAsync(MasterPasswordSaltKey, newSalt);
                await SaveSettingAsync(MasterPasswordHashKey, profile.MasterPasswordHash);
            }
        }

        public async Task<bool> VerifyMasterPasswordAsync(string password)
        {
            var hash = await _db.Settings.Where(s => s.Key == MasterPasswordHashKey).Select(s => s.Value).FirstOrDefaultAsync();
            var salt = await _db.Settings.Where(s => s.Key == MasterPasswordSaltKey).Select(s => s.Value).FirstOrDefaultAsync();

            // Default if none exists (admin123)
            if (string.IsNullOrEmpty(hash))
            {
                if (password == "admin123") return true; 
                return false;
            }

            if (string.IsNullOrEmpty(salt)) salt = GenerateSecureSalt(); // Generate secure random salt

            return PasswordSecurity.Verify(password, salt, hash);
        }

        /// <summary>
        /// Sets a new master password with a secure randomly generated salt.
        /// </summary>
        /// <param name="newPassword">The new master password to set</param>
        /// <returns>True if password was set successfully</returns>
        public async Task<bool> SetMasterPasswordAsync(string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
                return false;

            // Generate a new secure salt for this password
            var newSalt = GenerateSecureSalt();
            var newHash = PasswordSecurity.ComputeSha256(newPassword, newSalt);

            // Save both salt and hash
            await SaveSettingAsync(MasterPasswordSaltKey, newSalt);
            await SaveSettingAsync(MasterPasswordHashKey, newHash);

            return true;
        }

        private static string GetValue(IReadOnlyDictionary<string, string?> dictionary, string key, string fallback)
        {
            return dictionary.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
        }

        /// <summary>
        /// Generates a cryptographically secure random salt string.
        /// </summary>
        /// <returns>Base64 encoded random salt (32 bytes)</returns>
        private static string GenerateSecureSalt()
        {
            const int saltLength = 32;
            var salt = new byte[saltLength];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        private static double ParseDouble(string raw, double fallback)
        {
            if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            if (double.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                return value;
            }

            return fallback;
        }

        private static int ParseInt(string raw, int fallback)
        {
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;
        }

        private static bool ParseBool(string raw)
        {
            return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase) || raw == "1";
        }
    }
}
