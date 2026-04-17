using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public sealed class SystemSettingsProfile
    {
        public string ReportHeader { get; init; } = "Open_lab";
        public string ReportFooter { get; init; } = "";
        public double ReportMarginTop { get; init; } = 1.5;
        public double ReportMarginBottom { get; init; } = 1.5;
        public string ReportPrimaryColor { get; init; } = "#2B2B2B";
        public string ReportPrinterName { get; init; } = "Microsoft Print to PDF";
        public string ReceiptPrinterName { get; init; } = "Microsoft Print to PDF";
        public string BarcodePrinterName { get; init; } = "Microsoft Print to PDF";
        public string EnvelopePrinterName { get; init; } = "Microsoft Print to PDF";
        public string ReceiptHeaderText { get; init; } = "إيصال مختبر";
        public string ReceiptFooterText { get; init; } = "شكراً لتعاملكم";
        public bool ReceiptShowLogo { get; init; } = false;
        public int ReceiptCopies { get; init; } = 1;
    }

    public interface ISystemSettingsService
    {
        Task<List<Setting>> GetSettingsAsync();
        Task SaveSettingAsync(string key, string? value);
        Task DeleteSettingAsync(string key);
        Task<SystemSettingsProfile> GetProfileAsync();
        Task SaveProfileAsync(SystemSettingsProfile profile);
    }
}
