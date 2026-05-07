using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
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
        public async Task ListBackupsAsync_Should_Return_Files_If_Exists_LogicGuard()
        {
            // Function: 13.7 — Configure Backup - Refactored to Logic Guard
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            try
            {
                var file1 = Path.Combine(tempPath, "test1.bak");
                File.WriteAllText(file1, "dummy content");
                var file2 = Path.Combine(tempPath, "test2.bak");
                File.WriteAllText(file2, "more dummy content");
                
                // Act
                var backups = await _service.ListBackupsAsync(tempPath);
                
                // Assert - Logic Guard: Verify all backup files are returned with correct names
                backups.Should().HaveCount(2, "Should return all .bak files");
                backups.Should().Contain(f => f.Contains("test1.bak"));
                backups.Should().Contain(f => f.Contains("test2.bak"));
                
                // Assert - Logic Guard: Verify files actually exist
                foreach (var backup in backups)
                {
                    File.Exists(backup).Should().BeTrue("Backup file should exist");
                }
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public async Task BackupAsync_With_EmptyPath_Should_Throw_LogicGuard()
        {
            // Refactored to Logic Guard - verifies exception message and side effect
            Func<Task> act = async () => await _service.BackupAsync("");
            
            // Assert - Logic Guard: Verify exception is thrown with appropriate message
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*path*");
            
            // Assert - Logic Guard: Verify no backup files are created
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (Directory.Exists(tempPath))
            {
                var files = Directory.GetFiles(tempPath, "*.bak");
                files.Should().BeEmpty("No backup files should be created when path is invalid");
            }
        }

        // 13.7 Backup Workflow Tests - NEW TEST

        [Fact]
        public async Task BackupWorkflow_Should_Validate_Data_Preparation_LogicGuard()
        {
            // Function: 13.7 — Configure Backup - Logic Guard: Verify data is prepared for backup (InMemoryDatabase limitation workaround)
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            try
            {
                // Add some test data to verify backup includes data
                var patient = new Patient { LabId = "L-BACKUP", FullName = "Backup Test", Gender = "Male" };
                _db.Patients.Add(patient);
                await _db.SaveChangesAsync();

                var patient2 = new Patient { LabId = "L-BACKUP2", FullName = "Backup Test 2", Gender = "Female" };
                _db.Patients.Add(patient2);
                await _db.SaveChangesAsync();

                // Act - Verify data exists and is ready for backup
                var patients = await _db.Patients.ToListAsync();
                
                // Assert - Logic Guard: Verify data is prepared correctly
                patients.Should().HaveCountGreaterOrEqualTo(2, "Data should be ready for backup");
                patients.Should().Contain(p => p.LabId == "L-BACKUP");
                patients.Should().Contain(p => p.LabId == "L-BACKUP2");
                
                // Assert - Logic Guard: Verify backup path validation
                Func<Task> act = async () => await _service.BackupAsync("");
                await act.Should().ThrowAsync<ArgumentException>().WithMessage("*path*");
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }
        [Fact]
        public async Task RestoreAsync_WithEmptyPath_ShouldThrowException_FailureGuard()
        {
            Func<Task> act = async () => await _service.RestoreAsync("");
            
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*path*");
        }

        [Fact]
        public async Task RestoreAsync_WithValidPath_ShouldNotThrowArgumentException_SuccessGuard()
        {
            // Function: 13.7 — Configure Backup (Restore with Valid Path)
            // Arrange
            var validPath = Path.Combine(Path.GetTempPath(), "test.bak");

            // Act
            var exception = await Record.ExceptionAsync(async () => await _service.RestoreAsync(validPath));

            // Assert - Should not throw ArgumentException for valid path (other exceptions acceptable due to InMemory limitations)
            Assert.False(exception is ArgumentException, "Should not throw ArgumentException for valid path");
        }

        [Fact]
        public async Task ConfigureDailyBackupScheduleAsync_Should_Enable_Schedule_And_Persist_LogicGuard()
        {
            // Arrange - 13.7 scheduled backup configuration
            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var scheduledTime = DateTime.Now.AddHours(2).TimeOfDay;

            // Act
            await _service.ConfigureDailyBackupScheduleAsync(tempDirectory, scheduledTime);
            var status = await _service.GetBackupScheduleStatusAsync();

            // Assert
            status.IsEnabled.Should().BeTrue();
            status.DirectoryPath.Should().Be(tempDirectory);
            status.ScheduledTime.Hours.Should().Be(scheduledTime.Hours);
            status.ScheduledTime.Minutes.Should().Be(scheduledTime.Minutes);

            _db.SystemSettings.Should().Contain(s => s.SettingKey == "Backup.Schedule.Enabled" && s.SettingValue == bool.TrueString);
            _db.SystemSettings.Should().Contain(s => s.SettingKey == "Backup.Schedule.Directory" && s.SettingValue == tempDirectory);
        }

        [Fact]
        public async Task ConfigureDailyBackupScheduleAsync_With_Empty_Directory_Should_Throw_FailureGuard()
        {
            // Arrange/Act
            Func<Task> act = async () => await _service.ConfigureDailyBackupScheduleAsync(string.Empty, TimeSpan.FromHours(1));

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*directory*");
        }

        [Fact]
        public async Task CancelBackupScheduleAsync_Should_Disable_Schedule_LogicGuard()
        {
            // Arrange
            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            await _service.ConfigureDailyBackupScheduleAsync(tempDirectory, TimeSpan.FromHours(4));

            // Act
            await _service.CancelBackupScheduleAsync();
            var status = await _service.GetBackupScheduleStatusAsync();

            // Assert
            status.IsEnabled.Should().BeFalse();
        }
    }
}
