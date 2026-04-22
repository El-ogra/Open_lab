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
            // 13.2 Set Paper Size
            var profile = new SystemSettingsProfile
            {
                ReportPaperSize = "A5",
                DefaultAccountType = "Cash"
            };

            await _service.SaveProfileAsync(profile);
            var loaded = await _service.GetProfileAsync();

            loaded.ReportPaperSize.Should().Be("A5");
        }

        [Fact]
        public async Task SetMasterPasswordAsync_Should_Save_And_Verify_Password()
        {
            // 13.8 Set System Password
            var set = await _service.SetMasterPasswordAsync("NewStrongPass#1");
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
    }
}
