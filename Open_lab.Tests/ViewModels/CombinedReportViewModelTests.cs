using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class CombinedReportViewModelTests
    {
        private readonly Mock<IReportService> _reportServiceMock;
        private readonly CombinedReportViewModel _viewModel;

        public CombinedReportViewModelTests()
        {
            _reportServiceMock = new Mock<IReportService>();
            _viewModel = new CombinedReportViewModel(_reportServiceMock.Object);
        }

        [Fact]
        public async Task LoadCommand_With_Invalid_VisitId_Should_Show_Validation_Message_FailureGuard()
        {
            _viewModel.VisitId = 0;

            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.StatusMessage.Should().Be("يرجى إدخال رقم الزيارة.");
            _reportServiceMock.Verify(s => s.GetCompositeReportAsync(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
        }

        [Fact]
        public async Task LoadCommand_With_Existing_Visit_Should_Populate_Report_Data_SuccessGuard()
        {
            var report = BuildReport(visitId: 22);
            _reportServiceMock
                .Setup(s => s.GetCompositeReportAsync(22, It.IsAny<IReadOnlyCollection<int>>()))
                .ReturnsAsync(report);

            _viewModel.VisitId = 22;
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.PatientName.Should().Be("Patient 22");
            _viewModel.LabId.Should().Be("LAB-22");
            _viewModel.Tests.Should().HaveCount(2);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task LoadCommand_When_Service_Throws_Should_Set_Error_Message_FailureGuard()
        {
            _reportServiceMock
                .Setup(s => s.GetCompositeReportAsync(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>>()))
                .ThrowsAsync(new InvalidOperationException("Load failed"));

            _viewModel.VisitId = 9;
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.StatusMessage.Should().Contain("Load failed");
        }

        [Fact]
        public async Task MoveCommands_Should_Reorder_Items_When_Selection_Changes_EdgeGuard()
        {
            var report = BuildReport(visitId: 30);
            _reportServiceMock
                .Setup(s => s.GetCompositeReportAsync(30, It.IsAny<IReadOnlyCollection<int>>()))
                .ReturnsAsync(report);

            _viewModel.VisitId = 30;
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.SelectedTest = _viewModel.Tests[1];
            _viewModel.MoveUpCommand.Execute(null);
            _viewModel.Tests[0].Test.Code.Should().Be("CBC");

            _viewModel.MoveDownCommand.Execute(null);
            _viewModel.Tests[1].Test.Code.Should().Be("CBC");
        }

        private static VisitReportData BuildReport(int visitId)
        {
            var patient = new Patient
            {
                PatientId = 1,
                FullName = $"Patient {visitId}",
                LabId = $"LAB-{visitId}",
                Gender = "Male"
            };

            var visit = new Visit
            {
                VisitId = visitId,
                PatientId = patient.PatientId,
                Patient = patient,
                VisitDate = new DateTime(2026, 4, 26)
            };

            var test1 = new Test { TestId = 1, Code = "GLU", NameReport = "Glucose", NameReceipt = "Glucose", Price = 100m };
            var test2 = new Test { TestId = 2, Code = "CBC", NameReport = "CBC", NameReceipt = "CBC", Price = 150m };
            var vt1 = new VisitTest { VisitTestId = 11, VisitId = visitId, Visit = visit, TestId = 1, Test = test1, Price = 100m, Status = "Completed" };
            var vt2 = new VisitTest { VisitTestId = 12, VisitId = visitId, Visit = visit, TestId = 2, Test = test2, Price = 150m, Status = "Completed" };

            return new VisitReportData
            {
                Visit = visit,
                Patient = patient,
                Tests = new List<VisitTestReportItem>
                {
                    new VisitTestReportItem { VisitTest = vt1, Test = test1, Results = new List<ResultValueReportItem>() },
                    new VisitTestReportItem { VisitTest = vt2, Test = test2, Results = new List<ResultValueReportItem>() }
                }
            };
        }
    }
}
