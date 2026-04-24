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
            await _viewModel.LoadSettingsAsync();

            _viewModel.DefaultPrinter.Should().Be("DefaultPrinter");
            _viewModel.ReceiptPrinter.Should().Be("ReceiptPrinter");
            _viewModel.ReportPrinter.Should().Be("ReportPrinter");
            _viewModel.LeftMargin.Should().Be(1.5m);
            _viewModel.RightMargin.Should().Be(2.0m);
        }

        [Fact]
        public async Task SaveMarginSettingsAsync_Should_Save_Updated_Margins_For_Module13_1()
        {
            _viewModel.LeftMargin = 3.2m;
            _viewModel.RightMargin = 1.1m;

            await _viewModel.InvokePrivateAsync("SaveMarginSettingsAsync");

            _settingsMock.Verify(x => x.SetLeftMarginAsync(3.2m), Times.Once);
            _settingsMock.Verify(x => x.SetRightMarginAsync(1.1m), Times.Once);
        }
    }
}
