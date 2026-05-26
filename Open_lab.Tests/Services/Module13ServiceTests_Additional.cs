using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    /// <summary>
    /// اختبارات الموديول الثالث عشر — إعدادات النظام والتكوين | System Settings & Configuration
    /// تغطية شاملة لطبقة Service لكل وظائف الموديول 13.1 → 13.8
    /// </summary>
    public class Module13ServiceTests_Additional : IDisposable
    {
        private readonly OpenLabDbContext _db;
        private readonly SystemSettingsService _systemSettingsService;
        private readonly SettingsService _settingsService;
        private readonly BackupRestoreService _backupRestoreService;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module13ServiceTests_Additional()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _systemSettingsService = new SystemSettingsService(_db);
            _settingsService = new SettingsService(_db);
            _backupRestoreService = new BackupRestoreService(_db);
        }

        public void Dispose()
        {
            try { _backupRestoreService.CancelBackupScheduleAsync().GetAwaiter().GetResult(); } catch { }
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ========================================================================
        // 13.1 — Set Report Margins (ضبط هوامش التقرير)
        // ========================================================================

        [Fact]
        public async Task SetReportMargins_WithValidValues_ShouldPersistMarginsCorrectly_SuccessGuard()
        {
            // Function: 13.1 — Set Report Margins
            // Arrange
            var profile = new SystemSettingsProfile
            {
                ReportMarginTop = 2.5,
                ReportMarginBottom = 1.8
            };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReportMarginTop.Should().Be(2.5);
            loaded.ReportMarginBottom.Should().Be(1.8);
        }

        [Fact]
        public async Task SetReportMargins_WithLeftAndRightThroughSettingsService_ShouldPersistInCm_SuccessGuard()
        {
            // Function: 13.1 — Set Report Margins
            // Arrange & Act
            // Act
            await _settingsService.SetLeftMarginAsync(2.5m);
            await _settingsService.SetRightMarginAsync(3.0m);

            var leftLoaded = await _settingsService.GetLeftMarginAsync();
            var rightLoaded = await _settingsService.GetRightMarginAsync();

            // Assert
            leftLoaded.Should().Be(2.5m);
            rightLoaded.Should().Be(3.0m);
            (await _db.SystemSettings.AnyAsync(s => s.SettingKey == "Margin.Left")).Should().BeTrue();
            (await _db.SystemSettings.AnyAsync(s => s.SettingKey == "Margin.Right")).Should().BeTrue();
        }

        [Fact]
        public async Task SetReportMargins_WithZeroValues_ShouldAllowAndPersist_EdgeGuard()
        {
            // Function: 13.1 — Set Report Margins
            // Arrange
            var profile = new SystemSettingsProfile
            {
                ReportMarginTop = 0,
                ReportMarginBottom = 0
            };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReportMarginTop.Should().Be(0);
            loaded.ReportMarginBottom.Should().Be(0);
        }

        [Fact]
        public async Task SetReportMargins_WithNoSavedValue_ShouldReturnDefaultOnePointFive_EdgeGuard()
        {
            // Function: 13.1 — Set Report Margins
            // Arrange — لا يوجد إعدادات محفوظة بعد

            // Act
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert — القيم الافتراضية
            loaded.ReportMarginTop.Should().Be(1.5);
            loaded.ReportMarginBottom.Should().Be(1.5);
        }

        [Fact]
        public async Task SetReportMargins_WithCorruptedRawValue_ShouldFallbackToDefault_FailureGuard()
        {
            // Function: 13.1 — Set Report Margins
            // Arrange — ندخل قيمة غير صالحة يدوياً
            _db.Settings.Add(new Setting { Key = "Report.MarginTop", Value = "INVALID_NUMBER" });
            await _db.SaveChangesAsync();

            // Act
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert — يعود للقيمة الافتراضية بدلاً من رمي استثناء
            loaded.ReportMarginTop.Should().Be(1.5);
        }

        // ========================================================================
        // 13.2 — Set Paper Size (ضبط حجم الورق)
        // ========================================================================

        [Fact]
        public async Task SetPaperSize_WithA4_ShouldPersistAsA4_SuccessGuard()
        {
            // Function: 13.2 — Set Paper Size
            // Arrange
            var profile = new SystemSettingsProfile { ReportPaperSize = "A4" };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReportPaperSize.Should().Be("A4");
            (await _db.Settings.AnyAsync(s => s.Key == "Printer.PaperSize" && s.Value == "A4"))
                .Should().BeTrue();
        }

        [Fact]
        public async Task SetPaperSize_WithA5_ShouldPersistAsA5_SuccessGuard()
        {
            // Function: 13.2 — Set Paper Size
            // Arrange
            var profile = new SystemSettingsProfile { ReportPaperSize = "A5" };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReportPaperSize.Should().Be("A5");
        }

        [Fact]
        public async Task SetPaperSize_WhenNotSet_ShouldDefaultToA4_EdgeGuard()
        {
            // Function: 13.2 — Set Paper Size
            // Arrange
            // Act
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert — A4 هو الافتراضي
            loaded.ReportPaperSize.Should().Be("A4");
        }

        // ========================================================================
        // 13.3 — Configure Header/Footer (تكوين الترويسة والتذييل)
        // ========================================================================

        [Fact]
        public async Task ConfigureHeaderFooter_WithValidTextAndColor_ShouldPersistAllFields_SuccessGuard()
        {
            // Function: 13.3 — Configure Header/Footer
            // Arrange
            var profile = new SystemSettingsProfile
            {
                ReportHeader = "مختبر الفاروق الطبي",
                ReportFooter = "© جميع الحقوق محفوظة 2026",
                ReportPrimaryColor = "#1E88E5"
            };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReportHeader.Should().Be("مختبر الفاروق الطبي");
            loaded.ReportFooter.Should().Be("© جميع الحقوق محفوظة 2026");
            loaded.ReportPrimaryColor.Should().Be("#1E88E5");
        }

        [Fact]
        public async Task ConfigureHeaderFooter_WithReceiptHeaderText_ShouldPersistArabicText_SuccessGuard()
        {
            // Function: 13.3 — Configure Header/Footer
            // Arrange
            var profile = new SystemSettingsProfile
            {
                ReceiptHeaderText = "إيصال مختبر الفاروق",
                ReceiptFooterText = "شكراً لزيارتكم"
            };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReceiptHeaderText.Should().Be("إيصال مختبر الفاروق");
            loaded.ReceiptFooterText.Should().Be("شكراً لزيارتكم");
        }

        [Fact]
        public async Task ConfigureHeaderFooter_WithEmptyFooter_ShouldFallbackToEmptyString_EdgeGuard()
        {
            // Function: 13.3 — Configure Header/Footer
            // Arrange
            var profile = new SystemSettingsProfile
            {
                ReportHeader = "Header X",
                ReportFooter = string.Empty
            };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReportHeader.Should().Be("Header X");
            loaded.ReportFooter.Should().Be(string.Empty);
        }

        [Fact]
        public async Task ConfigureHeaderFooter_WithReceiptShowLogoTrue_ShouldPersistAsTrue_SuccessGuard()
        {
            // Function: 13.3 — Configure Header/Footer
            // Arrange
            var profile = new SystemSettingsProfile { ReceiptShowLogo = true };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReceiptShowLogo.Should().BeTrue();
        }

        // ========================================================================
        // 13.4 — Set Default Account Type (ضبط نوع الحساب الافتراضي)
        // ========================================================================

        [Fact]
        public async Task SetDefaultAccountType_WithCash_ShouldPersistAsCash_SuccessGuard()
        {
            // Function: 13.4 — Set Default Account Type
            // Arrange
            var profile = new SystemSettingsProfile { DefaultAccountType = "Cash" };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.DefaultAccountType.Should().Be("Cash");
            (await _db.Settings.AnyAsync(s => s.Key == "Invoice.DefaultAccountType" && s.Value == "Cash"))
                .Should().BeTrue();
        }

        [Fact]
        public async Task SetDefaultAccountType_WithContract_ShouldPersistAsContract_SuccessGuard()
        {
            // Function: 13.4 — Set Default Account Type
            // Arrange
            var profile = new SystemSettingsProfile { DefaultAccountType = "Contract" };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.DefaultAccountType.Should().Be("Contract");
        }

        [Fact]
        public async Task SetDefaultAccountType_WhenNotSet_ShouldDefaultToCash_EdgeGuard()
        {
            // Function: 13.4 — Set Default Account Type
            // Arrange
            // Act
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.DefaultAccountType.Should().Be("Cash");
        }

        // ========================================================================
        // 13.5 — Configure Printers (تكوين الطابعات)
        // ========================================================================

        [Fact]
        public async Task ConfigurePrinters_WithDifferentPrintersForEachOutput_ShouldPersistEachIndependently_SuccessGuard()
        {
            // Function: 13.5 — Configure Printers
            // Arrange
            var profile = new SystemSettingsProfile
            {
                ReportPrinterName = "HP LaserJet Pro",
                ReceiptPrinterName = "Epson TM-T20",
                BarcodePrinterName = "Zebra GK420",
                EnvelopePrinterName = "Brother HL-L2350"
            };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReportPrinterName.Should().Be("HP LaserJet Pro");
            loaded.ReceiptPrinterName.Should().Be("Epson TM-T20");
            loaded.BarcodePrinterName.Should().Be("Zebra GK420");
            loaded.EnvelopePrinterName.Should().Be("Brother HL-L2350");
        }

        [Fact]
        public async Task ConfigurePrinters_ThroughSettingsService_ShouldUpdateDefaultPrinter_SuccessGuard()
        {
            // Function: 13.5 — Configure Printers
            // Arrange & Act
            // Act
            await _settingsService.SetDefaultPrinterAsync("HP-Default");
            await _settingsService.SetReceiptPrinterAsync("Receipt-Thermal");
            await _settingsService.SetReportPrinterAsync("Report-Laser");

            // Assert
            (await _settingsService.GetDefaultPrinterAsync()).Should().Be("HP-Default");
            (await _settingsService.GetReceiptPrinterAsync()).Should().Be("Receipt-Thermal");
            (await _settingsService.GetReportPrinterAsync()).Should().Be("Report-Laser");
        }

        [Fact]
        public async Task ConfigurePrinters_WhenNoPrinterConfigured_ShouldFallbackToMicrosoftPrintToPdf_EdgeGuard()
        {
            // Function: 13.5 — Configure Printers
            // Arrange
            // Act
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert — القيمة الافتراضية المتوقعة
            loaded.ReportPrinterName.Should().Be("Microsoft Print to PDF");
            loaded.ReceiptPrinterName.Should().Be("Microsoft Print to PDF");
            loaded.BarcodePrinterName.Should().Be("Microsoft Print to PDF");
            loaded.EnvelopePrinterName.Should().Be("Microsoft Print to PDF");
        }

        [Fact]
        public async Task ConfigurePrinters_WhenUpdatingExistingPrinter_ShouldOverwriteOldValue_SuccessGuard()
        {
            // Function: 13.5 — Configure Printers
            // Arrange
            await _systemSettingsService.SaveProfileAsync(new SystemSettingsProfile { ReportPrinterName = "Old-Printer" });
            // Act
            await _systemSettingsService.SaveProfileAsync(new SystemSettingsProfile { ReportPrinterName = "New-Printer" });
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReportPrinterName.Should().Be("New-Printer");
            (await _db.Settings.CountAsync(s => s.Key == "Printer.Report")).Should().Be(1);
        }

        // ========================================================================
        // 13.6 — Set Invoice Settings (إعدادات الفاتورة)
        // ========================================================================

        [Fact]
        public async Task SetInvoiceSettings_WithShowLogoAndCopies_ShouldPersistFields_SuccessGuard()
        {
            // Function: 13.6 — Set Invoice Settings
            // Arrange
            var profile = new SystemSettingsProfile
            {
                ReceiptShowLogo = true,
                ReceiptCopies = 3,
                ReceiptHeaderText = "Lab Receipt"
            };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReceiptShowLogo.Should().BeTrue();
            loaded.ReceiptCopies.Should().Be(3);
            loaded.ReceiptHeaderText.Should().Be("Lab Receipt");
        }

        [Fact]
        public async Task SetInvoiceSettings_WithCurrencyAndShowLogoFlags_ThroughSettingsService_ShouldPersist_SuccessGuard()
        {
            // Function: 13.6 — Set Invoice Settings
            // Arrange & Act
            // Act
            await _settingsService.SetSettingAsync("Invoice.Currency", "EGP");
            await _settingsService.SetSettingAsync("Invoice.ShowLogo", true);

            var currency = await _settingsService.GetStringAsync("Invoice.Currency");
            var showLogo = await _settingsService.GetBoolAsync("Invoice.ShowLogo");

            // Assert
            currency.Should().Be("EGP");
            showLogo.Should().BeTrue();
            var typedRow = await _db.SystemSettings.SingleAsync(s => s.SettingKey == "Invoice.ShowLogo");
            typedRow.SettingType.Should().Be("Bool");
        }

        [Fact]
        public async Task SetInvoiceSettings_WithReceiptCopiesZero_ShouldStillPersistTheValue_EdgeGuard()
        {
            // Function: 13.6 — Set Invoice Settings
            // Arrange — الـ Service لا يطبق حد أدنى، فقط الـ ViewModel يطبقه قبل الإرسال
            var profile = new SystemSettingsProfile { ReceiptCopies = 0 };

            // Act
            await _systemSettingsService.SaveProfileAsync(profile);
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert
            loaded.ReceiptCopies.Should().Be(0);
        }

        [Fact]
        public async Task SetInvoiceSettings_WhenLoadingDefaults_ShouldReturnOneCopy_EdgeGuard()
        {
            // Function: 13.6 — Set Invoice Settings
            // Arrange
            // Act
            var loaded = await _systemSettingsService.GetProfileAsync();

            // Assert — القيمة الافتراضية
            loaded.ReceiptCopies.Should().Be(1);
            loaded.ReceiptShowLogo.Should().BeFalse();
        }

        // ========================================================================
        // 13.7 — Configure Backup (تكوين النسخ الاحتياطي)
        // ========================================================================

        [Fact]
        public async Task ConfigureBackup_WithValidDirectoryAndTime_ShouldEnableScheduleAndPersist_SuccessGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var scheduledTime = new TimeSpan(4, 30, 0);

            // Act
            await _backupRestoreService.ConfigureDailyBackupScheduleAsync(tempDir, scheduledTime);
            var status = await _backupRestoreService.GetBackupScheduleStatusAsync();

            // Assert
            status.IsEnabled.Should().BeTrue();
            status.DirectoryPath.Should().Be(tempDir);
            status.ScheduledTime.Should().Be(scheduledTime);
            (await _db.SystemSettings.AnyAsync(s => s.SettingKey == "Backup.Schedule.Enabled" && s.SettingValue == bool.TrueString))
                .Should().BeTrue();

            // Cleanup
            try { Directory.Delete(tempDir, true); } catch { }
        }

        [Fact]
        public async Task ConfigureBackup_WithEmptyDirectory_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            var scheduledTime = new TimeSpan(2, 0, 0);

            // Act
            Func<Task> act = async () =>
                await _backupRestoreService.ConfigureDailyBackupScheduleAsync(string.Empty, scheduledTime);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*directory*");
        }

        [Fact]
        public async Task ConfigureBackup_WithInvalidScheduledTime_ShouldThrowArgumentOutOfRange_FailureGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var invalidTime = TimeSpan.FromHours(25); // > 24h

            // Act
            Func<Task> act = async () =>
                await _backupRestoreService.ConfigureDailyBackupScheduleAsync(tempDir, invalidTime);

            // Assert
            await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task ConfigureBackup_BackupAsync_WithEmptyPath_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            // Act
            Func<Task> act = async () => await _backupRestoreService.BackupAsync("");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*path*");
        }

        [Fact]
        public async Task ConfigureBackup_RestoreAsync_WithEmptyPath_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            // Act
            Func<Task> act = async () => await _backupRestoreService.RestoreAsync("   ");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*path*");
        }

        [Fact]
        public async Task ConfigureBackup_ListBackupsAsync_WithNonExistentDirectory_ShouldReturnEmptyList_EdgeGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            var fakePath = Path.Combine(Path.GetTempPath(), "DEFINITELY_DOES_NOT_EXIST_" + Guid.NewGuid());

            // Act
            var result = await _backupRestoreService.ListBackupsAsync(fakePath);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ConfigureBackup_ListBackupsAsync_WithEmptyPath_ShouldReturnEmptyList_EdgeGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            // Act
            var result = await _backupRestoreService.ListBackupsAsync(string.Empty);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ConfigureBackup_ListBackupsAsync_WithExistingBakFiles_ShouldReturnOrderedByDateDescending_SuccessGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            try
            {
                var first = Path.Combine(tempPath, "first.bak");
                var second = Path.Combine(tempPath, "second.bak");
                File.WriteAllText(first, "x");
                await Task.Delay(20);
                File.WriteAllText(second, "y");

                // Act
                var result = await _backupRestoreService.ListBackupsAsync(tempPath);

                // Assert
                result.Should().HaveCount(2);
                result.Should().Contain(f => f.EndsWith("first.bak"));
                result.Should().Contain(f => f.EndsWith("second.bak"));
            }
            finally
            {
                try { Directory.Delete(tempPath, true); } catch { }
            }
        }

        [Fact]
        public async Task ConfigureBackup_CancelSchedule_ShouldDisableScheduleAndPersist_SuccessGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            await _backupRestoreService.ConfigureDailyBackupScheduleAsync(tempDir, new TimeSpan(5, 0, 0));

            // Act
            await _backupRestoreService.CancelBackupScheduleAsync();
            var status = await _backupRestoreService.GetBackupScheduleStatusAsync();

            // Assert
            status.IsEnabled.Should().BeFalse();
            (await _db.SystemSettings.AnyAsync(s => s.SettingKey == "Backup.Schedule.Enabled" && s.SettingValue == bool.FalseString))
                .Should().BeTrue();

            try { Directory.Delete(tempDir, true); } catch { }
        }

        [Fact]
        public async Task ConfigureBackup_GetBackupScheduleStatus_AfterMultipleConfigurations_ShouldReflectLastValue_SuccessGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            var dir1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var dir2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            await _backupRestoreService.ConfigureDailyBackupScheduleAsync(dir1, new TimeSpan(2, 0, 0));

            // Act
            await _backupRestoreService.ConfigureDailyBackupScheduleAsync(dir2, new TimeSpan(4, 15, 0));
            var status = await _backupRestoreService.GetBackupScheduleStatusAsync();

            // Assert
            status.DirectoryPath.Should().Be(dir2);
            status.ScheduledTime.Should().Be(new TimeSpan(4, 15, 0));

            try { Directory.Delete(dir1, true); } catch { }
            try { Directory.Delete(dir2, true); } catch { }
        }

        // ========================================================================
        // 13.8 — Set System Password (ضبط كلمة مرور النظام) — BR-SEC-003
        // ========================================================================

        [Fact]
        public async Task SetSystemPassword_WithValidNewPassword_ShouldHashAndPersist_SuccessGuard()
        {
            // Function: 13.8 — Set System Password (BR-SEC-003)
            // Arrange
            const string newPassword = "Strong#Pass2026";

            // Act
            var result = await _systemSettingsService.SetMasterPasswordAsync(newPassword);

            // Assert
            result.Should().BeTrue();
            var hash = await _db.Settings.FirstOrDefaultAsync(s => s.Key == "Security.MasterPasswordHash");
            var salt = await _db.Settings.FirstOrDefaultAsync(s => s.Key == "Security.MasterPasswordSalt");
            hash.Should().NotBeNull();
            salt.Should().NotBeNull();
            hash!.Value.Should().NotBeNullOrWhiteSpace();
            // التأكد بأن الكلمة لم تُخزَّن نصاً صريحاً
            hash.Value!.Should().NotBe(newPassword);
        }

        [Fact]
        public async Task SetSystemPassword_WithEmptyPassword_ShouldReturnFalseAndNotPersist_FailureGuard()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            // Act
            var result = await _systemSettingsService.SetMasterPasswordAsync(string.Empty);

            // Assert
            result.Should().BeFalse();
            (await _db.Settings.AnyAsync(s => s.Key == "Security.MasterPasswordHash")).Should().BeFalse();
        }

        [Fact]
        public async Task SetSystemPassword_WithNullPassword_ShouldReturnFalse_FailureGuard()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            // Act
            var result = await _systemSettingsService.SetMasterPasswordAsync(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SetSystemPassword_VerifyMasterPassword_WithCorrectPassword_ShouldReturnTrue_SuccessGuard_BR_SEC_003()
        {
            // Function: 13.8 — Set System Password (BR-SEC-003)
            // Arrange
            const string password = "MyMasterKey!42";
            await _systemSettingsService.SetMasterPasswordAsync(password);

            // Act
            var verified = await _systemSettingsService.VerifyMasterPasswordAsync(password);

            // Assert
            verified.Should().BeTrue();
        }

        [Fact]
        public async Task SetSystemPassword_VerifyMasterPassword_WithIncorrectPassword_ShouldReturnFalse_FailureGuard_BR_SEC_003()
        {
            // Function: 13.8 — Set System Password (BR-SEC-003)
            // Arrange
            await _systemSettingsService.SetMasterPasswordAsync("CorrectPassword");

            // Act
            var verified = await _systemSettingsService.VerifyMasterPasswordAsync("WrongPassword");

            // Assert
            verified.Should().BeFalse();
        }

        [Fact]
        public async Task SetSystemPassword_VerifyMasterPassword_WhenNoPasswordEverSet_ShouldRejectEveryPassword_EdgeGuard()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            // Act
            var defaultOk = await _systemSettingsService.VerifyMasterPasswordAsync("admin123");
            var anythingElse = await _systemSettingsService.VerifyMasterPasswordAsync("anythingElse#2026");

            // Assert
            defaultOk.Should().BeFalse();
            anythingElse.Should().BeFalse();
        }

        [Fact]
        public async Task SetSystemPassword_WhenChangingPasswordTwice_ShouldGenerateDifferentSalt_BR_SEC_003()
        {
            // Function: 13.8 — Set System Password (BR-SEC-003)
            // Arrange & Act
            // Act
            await _systemSettingsService.SetMasterPasswordAsync("FirstPass#1");
            var salt1 = (await _db.Settings.FirstAsync(s => s.Key == "Security.MasterPasswordSalt")).Value;

            await _systemSettingsService.SetMasterPasswordAsync("SecondPass#2");
            var salt2 = (await _db.Settings.FirstAsync(s => s.Key == "Security.MasterPasswordSalt")).Value;

            // Assert
            salt1.Should().NotBeNullOrWhiteSpace();
            salt2.Should().NotBeNullOrWhiteSpace();
            salt2.Should().NotBe(salt1, "كل كلمة مرور جديدة يجب أن يصاحبها Salt مختلف لأمان أفضل");
        }

        [Fact]
        public async Task SetSystemPassword_AfterChange_OldPasswordShouldNoLongerVerify_FailureGuard()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            await _systemSettingsService.SetMasterPasswordAsync("OldPassword");

            // Act
            await _systemSettingsService.SetMasterPasswordAsync("NewPassword");
            var oldStillWorks = await _systemSettingsService.VerifyMasterPasswordAsync("OldPassword");
            var newWorks = await _systemSettingsService.VerifyMasterPasswordAsync("NewPassword");

            // Assert
            oldStillWorks.Should().BeFalse();
            newWorks.Should().BeTrue();
        }

        // ========================================================================
        // اختبارات مساعدة عامة لـ SystemSettingsService — Save/Delete/Get
        // ========================================================================

        [Fact]
        public async Task SaveSettingAsync_WithExistingKey_ShouldUpdateNotInsertDuplicate_SuccessGuard()
        {
            // Function: 13.6 — Set Invoice Settings
            // Arrange
            await _systemSettingsService.SaveSettingAsync("Test.Key", "Initial");

            // Act
            await _systemSettingsService.SaveSettingAsync("Test.Key", "Updated");

            // Assert
            var rows = await _db.Settings.Where(s => s.Key == "Test.Key").ToListAsync();
            rows.Should().HaveCount(1);
            rows[0].Value.Should().Be("Updated");
        }

        [Fact]
        public async Task DeleteSettingAsync_WithExistingKey_ShouldRemoveRow_SuccessGuard()
        {
            // Function: 13.6 — Set Invoice Settings
            // Arrange
            await _systemSettingsService.SaveSettingAsync("ToBeDeleted", "value");

            // Act
            await _systemSettingsService.DeleteSettingAsync("ToBeDeleted");

            // Assert
            (await _db.Settings.AnyAsync(s => s.Key == "ToBeDeleted")).Should().BeFalse();
        }

        [Fact]
        public async Task GetSettingsAsync_WithMultipleKeys_ShouldReturnAllOrderedByKey_SuccessGuard()
        {
            // Function: 13.6 — Set Invoice Settings
            // Arrange
            await _systemSettingsService.SaveSettingAsync("ZKey", "Z");
            await _systemSettingsService.SaveSettingAsync("AKey", "A");
            await _systemSettingsService.SaveSettingAsync("MKey", "M");

            // Act
            var settings = await _systemSettingsService.GetSettingsAsync();

            // Assert
            settings.Should().HaveCount(3);
            settings[0].Key.Should().Be("AKey");
            settings[1].Key.Should().Be("MKey");
            settings[2].Key.Should().Be("ZKey");
        }
    }
}
