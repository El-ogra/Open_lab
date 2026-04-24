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
    public class WorkSheetByPatientViewModelTests : IDisposable
    {
        private readonly Mock<IWorksheetService> _worksheetServiceMock = new();
        private readonly Mock<IPrintService> _printServiceMock = new();
        private readonly WorkSheetByPatientViewModel _viewModel;

        public WorkSheetByPatientViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _viewModel = new WorkSheetByPatientViewModel(_worksheetServiceMock.Object, _printServiceMock.Object);
        }

        [Fact]
        public async Task LoadAsync_Should_Populate_Patient_Worksheet_Rows()
        {
            _worksheetServiceMock.Setup(x => x.GetWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow>
                {
                    new() { VisitId = 1, PatientName = "P1", VisitDate = DateTime.Today, TestsCount = 2 }
                });

            await _viewModel.InvokePrivateAsync("LoadAsync");

            _viewModel.Rows.Should().ContainSingle();
            _viewModel.Rows[0].PatientName.Should().Be("P1");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 زيارة");
        }

        [Fact]
        public async Task PrintAsync_Should_Send_Patient_Worksheet_To_Print_Service()
        {
            _viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 1, PatientName = "P1", VisitDate = DateTime.Today, TestsCount = 1 });

            await _viewModel.InvokePrivateAsync("PrintAsync");

            _printServiceMock.Verify(x => x.PrintWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetPatientRow>>()), Times.Once);
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }

    public class WorkSheetByTestViewModelTests : IDisposable
    {
        private readonly Mock<IWorksheetService> _worksheetServiceMock = new();
        private readonly Mock<IPrintService> _printServiceMock = new();
        private readonly WorkSheetByTestViewModel _viewModel;

        public WorkSheetByTestViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _viewModel = new WorkSheetByTestViewModel(_worksheetServiceMock.Object, _printServiceMock.Object);
        }

        [Fact]
        public async Task LoadAsync_Should_Populate_Test_Worksheet_Rows()
        {
            _worksheetServiceMock.Setup(x => x.GetWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetTestRow>
                {
                    new() { TestName = "CBC", Count = 3 }
                });

            await _viewModel.InvokePrivateAsync("LoadAsync");

            _viewModel.Rows.Should().ContainSingle();
            _viewModel.Rows[0].TestName.Should().Be("CBC");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 تحليل");
        }

        [Fact]
        public async Task PrintAsync_Should_Send_Test_Worksheet_To_Print_Service()
        {
            _viewModel.Rows.Add(new WorkSheetTestRow { TestName = "CBC", Count = 3 });

            await _viewModel.InvokePrivateAsync("PrintAsync");

            _printServiceMock.Verify(x => x.PrintWorksheetByTestAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetTestRow>>()), Times.Once);
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
