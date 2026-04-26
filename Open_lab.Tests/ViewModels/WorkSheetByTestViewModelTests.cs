using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class WorkSheetByTestViewModelCommandMatrixTests
    {
        [Fact]
        public async Task LoadCommand_Should_Load_Rows_Success()
        {
            // Arrange
            var worksheet = new Mock<IWorksheetService>();
            var print = new Mock<IPrintService>();
            worksheet.Setup(x => x.GetWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetTestRow> { new() { TestName = "CBC", Count = 2 } });
            var vm = new WorkSheetByTestViewModel(worksheet.Object, print.Object);

            // Act
            vm.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            vm.Rows.Should().ContainSingle(r => r.TestName == "CBC" && r.Count == 2);
        }

        [Fact]
        public async Task LoadCommand_When_Service_Throws_Should_Set_Error_Failure()
        {
            // Arrange
            var worksheet = new Mock<IWorksheetService>();
            var print = new Mock<IPrintService>();
            worksheet.Setup(x => x.GetWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("test-load-failed"));
            var vm = new WorkSheetByTestViewModel(worksheet.Object, print.Object);

            // Act
            vm.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            vm.StatusMessage.Should().Contain("test-load-failed");
        }

        [Fact]
        public void PrintCommand_When_No_Rows_Should_Be_Disabled_Edge()
        {
            // Arrange
            var vm = new WorkSheetByTestViewModel(Mock.Of<IWorksheetService>(), Mock.Of<IPrintService>());
            vm.Rows.Clear();

            // Act/Assert
            vm.PrintCommand.CanExecute(null).Should().BeFalse();
        }
    }
}
