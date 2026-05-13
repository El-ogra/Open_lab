using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class BackupRestoreServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly BackupRestoreService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public BackupRestoreServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new BackupRestoreService(_db);
        }

        public void Dispose()
        {
            _service.CancelBackupScheduleAsync().GetAwaiter().GetResult();
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task BackupAsync_With_EmptyPath_Should_Throw_LogicGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            // Act
            Func<Task> act = async () => await _service.BackupAsync("");
            
            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*path*");
        }

        [Fact]
        public async Task ConfigureBackup_WithEmptyRestorePath_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 13.7 — Configure Backup

            // Arrange
            var invalidPath = string.Empty;

            // Act
            Func<Task> act = async () => await _service.RestoreAsync(invalidPath);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Restore path*");
        }

        [Fact]
        public async Task ConfigureBackup_WithEmptyBackupDirectory_ShouldReturnEmptyBackupList_EdgeGuard()
        {
            // Function: 13.7 — Configure Backup

            // Arrange
            var emptyPath = string.Empty;

            // Act
            var backups = await _service.ListBackupsAsync(emptyPath);

            // Assert
            backups.Should().BeEmpty();
        }

        [Fact]
        public async Task ConfigureDailyBackupScheduleAsync_With_Empty_Directory_Should_Throw_FailureGuard()
        {
            // Function: 13.7 — Configure Backup (Restore with Valid Path)
            // Arrange/Act
            // Act
            Func<Task> act = async () => await _service.ConfigureDailyBackupScheduleAsync(string.Empty, TimeSpan.FromHours(1));

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*directory*");
        }
    }
}
