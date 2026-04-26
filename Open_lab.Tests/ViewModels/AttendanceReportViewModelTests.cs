using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class AttendanceReportViewModelTests : IDisposable
    {
        private readonly Mock<ITardinessService> _tardinessServiceMock = new();
        private readonly Mock<IUserAdminService> _userAdminServiceMock = new();
        private readonly Mock<IAttendancePayrollReportService> _payrollServiceMock = new();
        private readonly AttendanceReportViewModel _viewModel;

        public AttendanceReportViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();

            _userAdminServiceMock
                .Setup(x => x.GetUsersAsync())
                .ReturnsAsync(new List<User>());

            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendanceReportRow>());

            _payrollServiceMock
                .Setup(x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendancePayrollSummaryRow>());

            _viewModel = new AttendanceReportViewModel(
                _tardinessServiceMock.Object,
                _userAdminServiceMock.Object,
                _payrollServiceMock.Object);
        }

        public void Dispose() => AppSessionTestHelper.Reset();

        [Fact]
        public async Task GenerateReportAsync_Should_Load_Report_Rows_For_Module11_5()
        {
            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendanceReportRow>
                {
                    new() { Username = "u1", TotalHours = 8.5, DelayMinutes = 20, OvertimeMinutes = 60 }
                });

            await _viewModel.InvokePrivateAsync("GenerateReportAsync");

            _viewModel.ReportRows.Should().ContainSingle();
            _viewModel.ReportRows[0].Username.Should().Be("u1");
            _viewModel.StatusMessage.Should().Contain("تم إنشاء التقرير");
        }

        [Fact]
        public async Task LoadUsersCommand_When_Executed_Should_Load_Users_Success()
        {
            // Arrange
            _userAdminServiceMock.Setup(x => x.GetUsersAsync()).ReturnsAsync(new List<User>
            {
                new() { UserId = 2, Username = "u2" }
            });

            // Act
            _viewModel.LoadUsersCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.Users.Should().Contain(u => u.UserId == null);
            _viewModel.Users.Should().Contain(u => u.UserId == 2);
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 مستخدم");
        }

        [Fact]
        public async Task LoadUsersCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Arrange
            _userAdminServiceMock.Setup(x => x.GetUsersAsync()).ThrowsAsync(new InvalidOperationException("users-failed"));

            // Act
            _viewModel.LoadUsersCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("users-failed");
        }

        [Fact]
        public async Task GeneratePayrollSummaryAsync_Should_Load_Summary_Rows_For_Module11_5()
        {
            _payrollServiceMock
                .Setup(x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendancePayrollSummaryRow>
                {
                    new() { UserId = 1, Username = "u1", NetMinutes = 450, DelayMinutes = 20, OvertimeMinutes = 60, AbsentDays = 0 }
                });

            await _viewModel.InvokePrivateAsync("GeneratePayrollSummaryAsync");

            _viewModel.PayrollRows.Should().ContainSingle();
            _viewModel.PayrollRows[0].NetMinutes.Should().Be(450);
            _viewModel.StatusMessage.Should().Contain("ملخص الرواتب");
        }

        [Fact]
        public async Task GenerateReportCommand_When_Executed_Should_Fill_ReportRows_Success()
        {
            // Arrange
            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AttendanceReportRow> { new() { Username = "cmd-u", TotalHours = 8 } });

            // Act
            _viewModel.GenerateReportCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.ReportRows.Should().ContainSingle(r => r.Username == "cmd-u");
        }

        [Fact]
        public async Task GeneratePayrollSummaryCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Arrange
            _payrollServiceMock
                .Setup(x => x.GeneratePayrollSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ThrowsAsync(new InvalidOperationException("payroll-failed"));

            // Act
            _viewModel.GeneratePayrollSummaryCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("payroll-failed");
        }

        [Fact]
        public async Task GenerateReportAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Arrange
            _tardinessServiceMock
                .Setup(x => x.GetPunctualityReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ThrowsAsync(new InvalidOperationException("report-failed"));

            // Act
            await _viewModel.InvokePrivateAsync("GenerateReportAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("report-failed");
        }

        [Fact]
        public void ClearFilter_Should_Reset_User_And_DefaultDateRange_EdgeGuard()
        {
            // Arrange
            _viewModel.SelectedUserId = 10;
            _viewModel.From = DateTime.Today.AddDays(-30);
            _viewModel.To = DateTime.Today.AddDays(-20);

            // Act
            _viewModel.ClearFilterCommand.Execute(null);

            // Assert
            _viewModel.SelectedUserId.Should().BeNull();
            _viewModel.From.Date.Should().Be(DateTime.Today.AddDays(-7).Date);
            _viewModel.To.Date.Should().Be(DateTime.Today.Date);
        }
    }
}
