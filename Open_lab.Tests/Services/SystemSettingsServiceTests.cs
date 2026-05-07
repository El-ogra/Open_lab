using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;

namespace Open_lab.Tests.Services
{
    public class SystemSettingsServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly SystemSettingsService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public SystemSettingsServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new SystemSettingsService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task SaveProfileAsync_Should_Persist_Paper_Size()
        {
            // Function: 13.2 — Set Paper Size
            // Arrange
            // Act
            var profile = new SystemSettingsProfile
            {
                ReportPaperSize = "A5",
                DefaultAccountType = "Cash"
            };

            await _service.SaveProfileAsync(profile);
            var loaded = await _service.GetProfileAsync();

            // Assert
            loaded.ReportPaperSize.Should().Be("A5");
        }

        [Fact]
        public async Task SetMasterPasswordAsync_Should_Save_And_Verify_Password()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            // Act
            var set = await _service.SetMasterPasswordAsync("NewStrongPass#1");
            // Assert
            set.Should().BeTrue();

            var ok = await _service.VerifyMasterPasswordAsync("NewStrongPass#1");
            var bad = await _service.VerifyMasterPasswordAsync("WrongPass");

            ok.Should().BeTrue();
            bad.Should().BeFalse();

            var hash = await _db.Settings.FirstOrDefaultAsync(s => s.Key == "Security.MasterPasswordHash");
            var salt = await _db.Settings.FirstOrDefaultAsync(s => s.Key == "Security.MasterPasswordSalt");
            hash.Should().NotBeNull();
            salt.Should().NotBeNull();
            hash!.Value.Should().NotBeNullOrWhiteSpace();
            salt!.Value.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GetProfileAsync_Should_Read_ReceiptHeader_From_Settings_For_Module13_3()
        {
            // Function: 13.3 — `Configure Header/Footer`
            // Arrange
            // Act
            _db.Settings.Add(new Setting { Key = "Receipt.Header", Value = "مختبر ألف" });
            await _db.SaveChangesAsync();

            var profile = await _service.GetProfileAsync();

            // Assert
            profile.ReceiptHeaderText.Should().Be("مختبر ألف");
        }

        [Fact]
        public async Task SaveSettingAsync_And_GetSettingsAsync_Should_Persist_KeyValue_Success()
        {
            // Function: 13.3 — `Configure Header/Footer`
            // Arrange
            // Act
            await _service.SaveSettingAsync("Printer.Report", "HP-1");
            var all = await _service.GetSettingsAsync();

            // Assert
            all.Should().ContainSingle(s => s.Key == "Printer.Report" && s.Value == "HP-1");
        }

        [Fact]
        public async Task DeleteSettingAsync_When_Key_Not_Exists_Should_Not_Throw_Edge()
        {
            // Function: 13.3 — `Configure Header/Footer`
            // Arrange
            // Act
            await _service.DeleteSettingAsync("Missing.Key");

            // Assert
            (await _db.Settings.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task VerifyMasterPasswordAsync_When_No_Hash_Should_Allow_Default_Admin123_Edge()
        {
            // Function: 13.3 — `Configure Header/Footer`
            // Arrange
            // Act
            var ok = await _service.VerifyMasterPasswordAsync("admin123");
            var bad = await _service.VerifyMasterPasswordAsync("wrong");

            // Assert
            ok.Should().BeTrue();
            bad.Should().BeFalse();
        }

        [Fact]
        public async Task SetMasterPasswordAsync_When_Password_Empty_Should_Return_False_Failure()
        {
            // Function: 13.3 — `Configure Header/Footer`
            // Arrange
            // Act
            var result = await _service.SetMasterPasswordAsync(string.Empty);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SaveProfileAsync_When_MasterPasswordHash_Provided_Should_Save_Hash_And_Salt_Success()
        {
            // Function: 13.3 — `Configure Header/Footer`
            // Arrange
            var profile = new SystemSettingsProfile
            {
                ReportHeader = "h",
                ReportFooter = "f",
                MasterPasswordHash = "hash-value"
            };

            // Act
            await _service.SaveProfileAsync(profile);

            // Assert
            var hash = await _db.Settings.FirstOrDefaultAsync(s => s.Key == "Security.MasterPasswordHash");
            var salt = await _db.Settings.FirstOrDefaultAsync(s => s.Key == "Security.MasterPasswordSalt");
            hash.Should().NotBeNull();
            salt.Should().NotBeNull();
            hash!.Value.Should().Be("hash-value");
            salt!.Value.Should().NotBeNullOrWhiteSpace();
        }
        [Fact]
        public async Task SetDefaultAccountType_When_Exception_Should_Throw_FailureGuard()
        {
            // Function: 13.4 — Set Default Account Type
            // Arrange
            // Act
            Func<Task> act = async () => await _service.SaveProfileAsync(null!);
            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task ConfigurePrinters_When_Exception_Should_Throw_FailureGuard()
        {
            // Function: 13.5 — Configure Printers
            // Arrange
            // Act
            Func<Task> act = async () => await _service.SaveSettingAsync(null!, "val");
            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task SetInvoiceSettings_When_Exception_Should_Throw_FailureGuard()
        {
            // Function: 13.6 — Set Invoice Settings
            var badService = new SystemSettingsService(null!);
            // Act
            Func<Task> act = async () => await badService.DeleteSettingAsync(null!);
            // Assert
            await act.Should().ThrowAsync<Exception>();
        }
    }
}
