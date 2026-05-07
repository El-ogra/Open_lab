using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Tests.Infrastructure;
using Open_lab.Services;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class BackupRestoreViewModelTests : IDisposable
    {
        private readonly Mock<IBackupRestoreService> _mockService;
        private readonly BackupRestoreViewModel _viewModel;

        public BackupRestoreViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _mockService = new Mock<IBackupRestoreService>();
            _mockService
                .Setup(s => s.GetBackupScheduleStatusAsync())
                .ReturnsAsync(new BackupScheduleStatus
                {
                    IsEnabled = false,
                    DirectoryPath = string.Empty,
                    ScheduledTime = TimeSpan.FromHours(2)
                });
            
            _viewModel = new BackupRestoreViewModel(_mockService.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task BackupCommand_Should_FailIfPathEmpty_FailureGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _viewModel.BackupPath = "";

            // Act
            _viewModel.BackupCommand.Execute(null);
            await Task.Delay(50); // allow async command execution

            // Assert
            _viewModel.StatusMessage.Should().Be("أدخل مسار النسخ الاحتياطي.");
            _viewModel.IsLoading.Should().BeFalse();
            _mockService.Verify(s => s.BackupAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task BackupCommand_Should_SetIsLoading_And_CallService_SuccessGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _viewModel.BackupPath = "C:\\Backups";
            _mockService.Setup(s => s.BackupAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockService.Setup(s => s.ListBackupsAsync(It.IsAny<string>())).ReturnsAsync(new List<string> { "backup1.bak" });

            // Act
            _viewModel.BackupCommand.Execute(null);
            await Task.Delay(50); 

            // Assert
            _mockService.Verify(s => s.BackupAsync("C:\\Backups"), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم إنشاء النسخة الاحتياطية.");
            _viewModel.BackupFiles.Should().Contain("backup1.bak");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreCommand_Should_FailIfPathEmpty_FailureGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _viewModel.RestorePath = "";

            // Act
            _viewModel.RestoreCommand.Execute(null);
            await Task.Delay(50); 

            // Assert
            _viewModel.StatusMessage.Should().Be("أدخل مسار الاستعادة.");
            _viewModel.IsLoading.Should().BeFalse();
            _mockService.Verify(s => s.RestoreAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RestoreCommand_Should_SetIsLoading_And_CallService_SuccessGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _viewModel.RestorePath = "C:\\Backups\\db.bak";
            _mockService.Setup(s => s.RestoreAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockService.Setup(s => s.ListBackupsAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());

            // Act
            _viewModel.RestoreCommand.Execute(null);
            await Task.Delay(50); 

            // Assert
            _mockService.Verify(s => s.RestoreAsync("C:\\Backups\\db.bak"), Times.Once);
            _viewModel.StatusMessage.Should().Be("تمت الاستعادة.");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task LoadBackupsCommand_Should_FailIfNoPath_FailureGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _viewModel.BackupPath = "";
            _viewModel.RestorePath = "";

            // Act
            _viewModel.LoadBackupsCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Be("حدد مسار ملف أو مجلد النسخ الاحتياطية أولاً.");
        }

        [Fact]
        public async Task Exception_DuringServiceCall_Should_BeCaught_And_ShowInStatusMessage_FailureGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _viewModel.BackupPath = "C:\\BadPath";
            _mockService.Setup(s => s.BackupAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Access Denied"));

            // Act
            _viewModel.BackupCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("Access Denied");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task ConfigureScheduleCommand_Should_Fail_When_Directory_Missing_FailureGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _viewModel.ScheduledBackupDirectory = string.Empty;
            _viewModel.ScheduledBackupTime = "03:30";

            // Act
            _viewModel.ConfigureScheduleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Be("حدد مجلد النسخ الاحتياطي المجدول.");
            _mockService.Verify(s => s.ConfigureDailyBackupScheduleAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task ConfigureScheduleCommand_Should_Call_Service_And_Enable_Schedule_SuccessGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _viewModel.ScheduledBackupDirectory = "C:\\Backups\\Daily";
            _viewModel.ScheduledBackupTime = "03:30";
            _mockService.Setup(s => s.ConfigureDailyBackupScheduleAsync("C:\\Backups\\Daily", It.IsAny<TimeSpan>())).Returns(Task.CompletedTask);
            _mockService.Setup(s => s.GetBackupScheduleStatusAsync()).ReturnsAsync(new BackupScheduleStatus
            {
                IsEnabled = true,
                DirectoryPath = "C:\\Backups\\Daily",
                ScheduledTime = new TimeSpan(3, 30, 0),
                LastRunUtc = DateTime.UtcNow
            });

            // Act
            _viewModel.ConfigureScheduleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _mockService.Verify(s => s.ConfigureDailyBackupScheduleAsync("C:\\Backups\\Daily", new TimeSpan(3, 30, 0)), Times.Once);
            _viewModel.IsScheduleEnabled.Should().BeTrue();
            _viewModel.ScheduledBackupTime.Should().Be("03:30");
        }

        [Fact]
        public async Task DisableScheduleCommand_Should_Call_Service_And_Disable_Schedule_SuccessGuard()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _mockService.Setup(s => s.CancelBackupScheduleAsync()).Returns(Task.CompletedTask);
            _mockService.Setup(s => s.GetBackupScheduleStatusAsync()).ReturnsAsync(new BackupScheduleStatus
            {
                IsEnabled = false,
                DirectoryPath = string.Empty,
                ScheduledTime = new TimeSpan(2, 0, 0)
            });

            // Act
            _viewModel.DisableScheduleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _mockService.Verify(s => s.CancelBackupScheduleAsync(), Times.Once);
            _viewModel.IsScheduleEnabled.Should().BeFalse();
            _viewModel.StatusMessage.Should().Be("تم إيقاف النسخ الاحتياطي المجدول.");
        }

        [Fact]
        public async Task ConfigureScheduleCommand_When_Time_Format_Invalid_Should_Set_Validation_Message_Failure()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _viewModel.ScheduledBackupDirectory = "C:\\Backups";
            _viewModel.ScheduledBackupTime = "invalid";

            // Act
            _viewModel.ConfigureScheduleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("صيغة وقت الجدولة غير صحيحة");
            _mockService.Verify(s => s.ConfigureDailyBackupScheduleAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task RefreshScheduleCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 13.7 — Configure Backup
            // Arrange
            _mockService.Setup(s => s.GetBackupScheduleStatusAsync()).ThrowsAsync(new InvalidOperationException("schedule-failed"));

            // Act
            _viewModel.RefreshScheduleCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("schedule-failed");
        }
    }
}
