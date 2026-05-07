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
    public class SettingsViewModelTests
    {
        private readonly Mock<ISettingsService> _settingsMock = new();
        private readonly Mock<IPrintService> _printMock = new();
        private readonly SettingsViewModel _viewModel;

        public SettingsViewModelTests()
        {
            _settingsMock.Setup(x => x.GetAllSettingsAsync()).ReturnsAsync(new List<SystemSetting>());
            _settingsMock.Setup(x => x.GetDefaultPrinterAsync()).ReturnsAsync("DefaultPrinter");
            _settingsMock.Setup(x => x.GetReceiptPrinterAsync()).ReturnsAsync("ReceiptPrinter");
            _settingsMock.Setup(x => x.GetReportPrinterAsync()).ReturnsAsync("ReportPrinter");
            _settingsMock.Setup(x => x.GetLeftMarginAsync()).ReturnsAsync(1.5m);
            _settingsMock.Setup(x => x.GetRightMarginAsync()).ReturnsAsync(2.0m);

            _viewModel = new SettingsViewModel(_settingsMock.Object, _printMock.Object);
        }

        [Fact]
        public async Task LoadSettingsAsync_Should_Load_Printer_And_Margins_For_Module13()
        {
            // Function: 13.1 — `Set Report Margins`
            // Arrange
            // Act
            await _viewModel.LoadSettingsAsync();

            // Assert
            _viewModel.DefaultPrinter.Should().Be("DefaultPrinter");
            _viewModel.ReceiptPrinter.Should().Be("ReceiptPrinter");
            _viewModel.ReportPrinter.Should().Be("ReportPrinter");
            _viewModel.LeftMargin.Should().Be(1.5m);
            _viewModel.RightMargin.Should().Be(2.0m);
        }

        [Fact]
        public async Task SaveMarginSettingsAsync_Should_Save_Updated_Margins_For_Module13_1()
        {
            // Function: 13.1 — `Set Report Margins`
            // Arrange
            // Act
            _viewModel.LeftMargin = 3.2m;
            _viewModel.RightMargin = 1.1m;

            await _viewModel.InvokePrivateAsync("SaveMarginSettingsAsync");

            _settingsMock.Verify(x => x.SetLeftMarginAsync(3.2m), Times.Once);
            _settingsMock.Verify(x => x.SetRightMarginAsync(1.1m), Times.Once);
            // Assert
        }

        [Fact]
        public async Task LoadSettingsCommand_When_Executed_Should_Call_Service_And_Update_ViewModel_Success()
        {
            // Function: 13.1 — `Set Report Margins`
            // Arrange
            // Act
            _viewModel.LoadSettingsCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsMock.Verify(x => x.GetAllSettingsAsync(), Times.AtLeastOnce);
            _viewModel.DefaultPrinter.Should().Be("DefaultPrinter");
        }

        [Fact]
        public async Task SavePrinterSettingsCommand_When_Executed_Should_Save_All_Printer_Values_Success()
        {
            // Function: 13.1 — `Set Report Margins`
            // Arrange
            _viewModel.DefaultPrinter = "P-Default";
            _viewModel.ReceiptPrinter = "P-Receipt";
            _viewModel.ReportPrinter = "P-Report";

            // Act
            _viewModel.SavePrinterSettingsCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsMock.Verify(x => x.SetDefaultPrinterAsync("P-Default"), Times.Once);
            _settingsMock.Verify(x => x.SetReceiptPrinterAsync("P-Receipt"), Times.Once);
            _settingsMock.Verify(x => x.SetReportPrinterAsync("P-Report"), Times.Once);
        }

        [Fact]
        public async Task SaveMarginSettingsCommand_When_Service_Throws_Should_Not_Propagate_Exception_Failure()
        {
            // Function: 13.1 — `Set Report Margins`
            // Arrange
            _viewModel.LeftMargin = 1m;
            _viewModel.RightMargin = 2m;
            _settingsMock.Setup(x => x.SetLeftMarginAsync(It.IsAny<decimal>())).ThrowsAsync(new InvalidOperationException("margin-failed"));

            // Act
            _viewModel.SaveMarginSettingsCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _settingsMock.Verify(x => x.SetLeftMarginAsync(1m), Times.Once);
        }
    }
}
