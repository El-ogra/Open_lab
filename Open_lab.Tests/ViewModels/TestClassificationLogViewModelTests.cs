using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class TestClassificationLogViewModelTests : IDisposable
    {
        private readonly Mock<ITestClassificationService> _classificationServiceMock = new();
        private readonly Mock<IPrintService> _printServiceMock = new();
        private readonly TestClassificationLogViewModel _viewModel;

        public TestClassificationLogViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _classificationServiceMock.Setup(x => x.GetConsumptionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ReagentConsumptionReport>
                {
                    new() { ReagentName = "R1", TotalConsumed = 5m, Unit = "ml", TestCount = 1 }
                });

            _viewModel = new TestClassificationLogViewModel(_classificationServiceMock.Object, _printServiceMock.Object);
        }

        [Fact]
        public async Task LoadAsync_Should_Populate_Classification_Log_Items()
        {
            // Function: 7.4 — Test Classification LOG
            // Arrange
            // Act
            await _viewModel.InvokePrivateAsync("LoadAsync");

            // Assert
            _viewModel.Items.Should().ContainSingle();
            _viewModel.Items[0].ReagentName.Should().Be("R1");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 سجل");
        }

        [Fact]
        public async Task PrintAsync_Should_Send_Text_Report()
        {
            // Function: 7.4 — Test Classification LOG
            // Arrange
            // Act
            _viewModel.Items.Add(new ReagentConsumptionReport { ReagentName = "R1", TotalConsumed = 5m, Unit = "ml", TestCount = 1 });

            await _viewModel.InvokePrivateAsync("PrintAsync");

            _printServiceMock.Verify(x => x.PrintTextReportAsync("سجل تصنيف التحاليل", It.IsAny<IReadOnlyCollection<string>>(), "TestClassificationLog"), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
