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
    }
}
