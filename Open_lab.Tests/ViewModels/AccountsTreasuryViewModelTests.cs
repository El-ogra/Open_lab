using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class AccountsTreasuryViewModelTests
    {
        private readonly Mock<IAccountsTreasuryService> _mockAccountsTreasuryService;
        private readonly Mock<IPrintService> _mockPrintService;
        private readonly AccountsTreasuryViewModel _viewModel;

        public AccountsTreasuryViewModelTests()
        {
            _mockAccountsTreasuryService = new Mock<IAccountsTreasuryService>();
            _mockPrintService = new Mock<IPrintService>();
            
            // To emulate Permission being passed in commands, assuming tests pass UI permission checks,
            // we will invoke the async methods via reflection if commands are ignored, 
            // but setting up proper mocking or calling Execute covers it if AppSession allows.
            _viewModel = new AccountsTreasuryViewModel(_mockAccountsTreasuryService.Object, _mockPrintService.Object);
        }

        [Fact]
        public async Task LoadCommand_Should_SetIsLoading_And_LoadData_SuccessGuard()
        {
            // Arrange
            var snapshot = new AccountsTreasurySnapshot
            {
                TotalInvoiced = 1000,
                TotalPaid = 800,
                TotalBalance = 200,
                TotalDiscount = 50,
                TotalExpenses = 100,
                NetProfit = 700
            };
            
            _mockAccountsTreasuryService
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(snapshot);

            // Act - bypassing permission checks using direct reflection if needed, but let's assume Execute works
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50); // allow async void to continue

            // Assert
            _viewModel.TotalInvoiced.Should().Be(1000);
            _viewModel.TotalPaid.Should().Be(800);
            _viewModel.StatusMessage.Should().Be("تم تحميل بيانات الخزينة.");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task LoadCommand_Failure_Should_HandleException_FailureGuard()
        {
            // Arrange
            _mockAccountsTreasuryService
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("Database connection failed");
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task DailyCommand_Should_LoadDataForToday_SuccessGuard()
        {
            // Arrange
            _mockAccountsTreasuryService
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new AccountsTreasurySnapshot());

            // Act
            _viewModel.DailyCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.DateFrom.Date.Should().Be(DateTime.Today);
            _viewModel.DateTo.Date.Should().Be(DateTime.Today);
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task WeeklyCommand_Should_CalculateStartOfWeek_SuccessGuard()
        {
            // Arrange
            _mockAccountsTreasuryService
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new AccountsTreasurySnapshot());

            // Act
            _viewModel.WeeklyCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.DateFrom.Should().BeBefore(DateTime.Today.AddDays(1));
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task MonthlyCommand_Should_CalculateStartOfMonth_SuccessGuard()
        {
            // Arrange
            _mockAccountsTreasuryService
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new AccountsTreasurySnapshot());

            // Act
            _viewModel.MonthlyCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.DateFrom.Day.Should().Be(1);
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task DailyCommand_When_ServiceThrows_Should_Set_ErrorMessage_FailureGuard()
        {
            // Arrange
            _mockAccountsTreasuryService
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ThrowsAsync(new Exception("daily-failed"));

            // Act
            _viewModel.DailyCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("daily-failed");
            _viewModel.IsLoading.Should().BeFalse();
        }
        
        [Fact]
        public async Task PrintCommand_Should_CallPrintService_SuccessGuard()
        {
            // Arrange
            _mockPrintService
                .Setup(p => p.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _mockPrintService.Verify(p => p.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>()), Times.Once);
            _viewModel.IsLoading.Should().BeFalse();
            _viewModel.StatusMessage.Should().Be("تم إرسال تقرير الخزينة للطباعة.");
        }
        
        [Fact]
        public async Task PrintCommand_Failure_Should_HandleException_FailureGuard()
        {
            // Arrange
            _mockPrintService
                .Setup(p => p.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Printer out of paper"));

            // Act
            _viewModel.PrintCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _viewModel.StatusMessage.Should().Contain("Printer out of paper");
            _viewModel.IsLoading.Should().BeFalse();
        }
    }
}
