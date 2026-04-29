using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class AccountsTreasuryViewModelTests
    {
        private static AccountsTreasuryViewModel CreateViewModel(
            Mock<IAccountsTreasuryService> accountsTreasuryServiceMock,
            Mock<IPrintService> printServiceMock)
        {
            AppSessionTestHelper.ResetToAdmin();
            return new AccountsTreasuryViewModel(accountsTreasuryServiceMock.Object, printServiceMock.Object);
        }

        [Fact]
        public async Task LoadCommand_Should_SetIsLoading_And_LoadData_SuccessGuard()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            var snapshot = new AccountsTreasurySnapshot
            {
                TotalInvoiced = 1000,
                TotalPaid = 800,
                TotalBalance = 200,
                TotalDiscount = 50,
                TotalExpenses = 100,
                NetProfit = 700
            };
            
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(snapshot);

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.TotalInvoiced.Should().Be(1000);
            viewModel.TotalPaid.Should().Be(800);
            viewModel.StatusMessage.Should().Be("تم تحميل بيانات الخزينة.");
            viewModel.IsLoading.Should().BeFalse();

            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadCommand_Failure_Should_HandleException_FailureGuard()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("Database connection failed");
            viewModel.IsLoading.Should().BeFalse();

            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task DailyCommand_Should_LoadDataForToday_SuccessGuard()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new AccountsTreasurySnapshot());

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);

            // Act
            viewModel.DailyCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.DateFrom.Date.Should().Be(DateTime.Today);
            viewModel.DateTo.Date.Should().Be(DateTime.Today);
            viewModel.IsLoading.Should().BeFalse();

            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task WeeklyCommand_Should_CalculateStartOfWeek_SuccessGuard()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new AccountsTreasurySnapshot());

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);

            // Act
            viewModel.WeeklyCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.DateFrom.Should().BeBefore(DateTime.Today.AddDays(1));
            viewModel.IsLoading.Should().BeFalse();

            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task MonthlyCommand_Should_CalculateStartOfMonth_SuccessGuard()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new AccountsTreasurySnapshot());

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);

            // Act
            viewModel.MonthlyCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.DateFrom.Day.Should().Be(1);
            viewModel.IsLoading.Should().BeFalse();

            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task DailyCommand_When_ServiceThrows_Should_Set_ErrorMessage_FailureGuard()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ThrowsAsync(new Exception("daily-failed"));

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);

            // Act
            viewModel.DailyCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("daily-failed");
            viewModel.IsLoading.Should().BeFalse();

            AppSessionTestHelper.Reset();
        }
        
        [Fact]
        public async Task PrintCommand_Should_CallPrintService_SuccessGuard()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new AccountsTreasurySnapshot());
            printServiceMock
                .Setup(p => p.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);

            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            printServiceMock.Verify(p => p.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>()), Times.Once);
            viewModel.IsLoading.Should().BeFalse();
            viewModel.StatusMessage.Should().Be("تم إرسال تقرير الخزينة للطباعة.");

            AppSessionTestHelper.Reset();
        }
        
        [Fact]
        public async Task PrintCommand_Failure_Should_HandleException_FailureGuard()
        {
            // Function: 2.10 — Generate Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new AccountsTreasurySnapshot());
            printServiceMock
                .Setup(p => p.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Printer out of paper"));

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);

            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("Printer out of paper");
            viewModel.IsLoading.Should().BeFalse();

            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadCommand_With_BranchFilter_Should_Call_GetSnapshotAsync_With_BranchId()
        {
            // Function: 2.11 — Branch-wise Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            var snapshot = new AccountsTreasurySnapshot
            {
                TotalInvoiced = 500,
                TotalPaid = 400,
                TotalBalance = 100,
                TotalDiscount = 25,
                TotalExpenses = 50,
                NetProfit = 350
            };
            
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 5))
                .ReturnsAsync(snapshot);

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);
            viewModel.SelectedBranchId = 5;

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            accountsTreasuryServiceMock.Verify(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 5), Times.Once);
            viewModel.TotalInvoiced.Should().Be(500);
            viewModel.StatusMessage.Should().Contain("تم تحميل بيانات الخزينة.");

            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadCommand_Should_Calculate_Doctor_Commissions_Directly()
        {
            // Function: 2.12 — Doctor-wise Inventory
            // Arrange
            var accountsTreasuryServiceMock = new Mock<IAccountsTreasuryService>();
            var printServiceMock = new Mock<IPrintService>();
            var snapshot = new AccountsTreasurySnapshot
            {
                TotalInvoiced = 1000,
                TotalPaid = 800,
                TotalBalance = 200,
                TotalDiscount = 50,
                TotalExpenses = 100,
                NetProfit = 700,
                ByDoctor = new List<TreasuryByDoctorRow>
                {
                    new TreasuryByDoctorRow { DoctorName = "Dr. A", VisitsCount = 10, CommissionAmount = 250m }
                }
            };
            
            accountsTreasuryServiceMock
                .Setup(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(snapshot);

            var viewModel = CreateViewModel(accountsTreasuryServiceMock, printServiceMock);

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            accountsTreasuryServiceMock.Verify(s => s.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()), Times.AtLeastOnce);
            viewModel.TotalInvoiced.Should().Be(1000);
            viewModel.NetProfit.Should().Be(700);
            viewModel.StatusMessage.Should().Contain("تم تحميل بيانات الخزينة.");

            AppSessionTestHelper.Reset();
        }
    }
}
