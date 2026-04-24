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
    public class GroupWorksheetViewModelTests : IDisposable
    {
        private readonly Mock<IGroupWorksheetService> _groupWorksheetServiceMock = new();
        private readonly Mock<ITestCatalogService> _testCatalogServiceMock = new();
        private readonly Mock<IPrintService> _printServiceMock = new();
        private readonly GroupWorksheetViewModel _viewModel;

        public GroupWorksheetViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _testCatalogServiceMock.Setup(x => x.GetTestGroupsAsync())
                .ReturnsAsync(new List<TestGroup> { new() { GroupId = 1, GroupName = "G1" } });
            _testCatalogServiceMock.Setup(x => x.GetCustomGroupsAsync())
                .ReturnsAsync(new List<CustomGroup> { new() { CustomGroupId = 2, Name = "CG1" } });

            _viewModel = new GroupWorksheetViewModel(
                _groupWorksheetServiceMock.Object,
                _testCatalogServiceMock.Object,
                _printServiceMock.Object);
        }

        [Fact]
        public async Task LoadWorksheetAsync_Should_Load_Group_Worksheet_Rows()
        {
            _groupWorksheetServiceMock.Setup(x => x.GetGroupWorksheetByGroupAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WorkSheetPatientRow> { new() { VisitId = 1, PatientName = "P1", TestsCount = 2 } });

            _viewModel.SelectedGroupId = 1;
            await _viewModel.InvokePrivateAsync("LoadWorksheetAsync");

            _viewModel.Rows.Should().ContainSingle();
            _viewModel.Rows[0].PatientName.Should().Be("P1");
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 زيارة");
        }

        [Fact]
        public async Task PrintAsync_Should_Send_Group_Worksheet_To_Print_Service()
        {
            _viewModel.Rows.Add(new WorkSheetPatientRow { VisitId = 1, PatientName = "P1", TestsCount = 1 });

            await _viewModel.InvokePrivateAsync("PrintAsync");

            _printServiceMock.Verify(x => x.PrintWorksheetByPatientAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IReadOnlyCollection<WorkSheetPatientRow>>()), Times.Once);
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
