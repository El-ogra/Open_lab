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
    public class WorkSheetByPatientViewModelCommandMatrixTests : IDisposable
    {
        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadCommand_Should_Load_Patient_Rows_Success()
        {
            // Arrange
            AppSessionTestHelper.ResetToAdmin();

            var worksheet = new Mock<IWorksheetService>();
            var print = new Mock<IPrintService>();
            worksheet.Setup(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow> { new() { VisitId = 1, PatientName = "P1", TestsCount = 1 } });
            var vm = new WorkSheetByPatientViewModel(worksheet.Object, print.Object);

            // Act
            vm.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            vm.Rows.Should().ContainSingle(r => r.VisitId == 1);
        }

        [Fact]
        public async Task LoadCommand_When_PatientService_Throws_Should_Set_Error_Failure()
        {
            // Arrange
            AppSessionTestHelper.ResetToAdmin();

            var worksheet = new Mock<IWorksheetService>();
            var print = new Mock<IPrintService>();
            worksheet.Setup(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("patient-load-failed"));
            var vm = new WorkSheetByPatientViewModel(worksheet.Object, print.Object);

            // Act
            vm.LoadCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            vm.StatusMessage.Should().Contain("patient-load-failed");
        }

        [Fact]
        public void PrintCommand_When_No_PatientRows_Should_Be_Disabled_Edge()
        {
            // Arrange
            var vm = new WorkSheetByPatientViewModel(Mock.Of<IWorksheetService>(), Mock.Of<IPrintService>());
            vm.Rows.Clear();

            // Act/Assert
            vm.PrintCommand.CanExecute(null).Should().BeFalse();
        }
    }
}

