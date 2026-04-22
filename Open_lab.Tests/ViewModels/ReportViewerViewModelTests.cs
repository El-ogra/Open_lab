using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class ReportViewerViewModelTests
    {
        private readonly Mock<IReportService> _reportServiceMock;
        private readonly Mock<IPrintService> _printServiceMock;
        private readonly ReportViewerViewModel _viewModel;

        public ReportViewerViewModelTests()
        {
            _reportServiceMock = new Mock<IReportService>();
            _printServiceMock = new Mock<IPrintService>();
            _viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object);
        }

        [Fact]
        public async Task LoadReportAsync_With_Valid_Visit_Should_Build_Preview_LogicGuard()
        {
            // 4.4 Composite Report - 4.5 Report Order Validation
            var report = BuildReport(visitId: 10, isSendOut: false);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(10)).ReturnsAsync(report);
            _viewModel.VisitId = 10;

            await _viewModel.InvokePrivateAsync("LoadReportAsync");

            // Assert - Logic Guard: Verify report structure and content
            _viewModel.Report.Should().NotBeNull();
            _viewModel.Report!.Patient.Should().NotBeNull();
            _viewModel.Report.Patient.FullName.Should().Be("P");
            _viewModel.Report.Patient.LabId.Should().Be("L1");
            _viewModel.Report.Visit.Should().NotBeNull();
            _viewModel.Report.Visit.VisitId.Should().Be(10);

            // Assert - Logic Guard: Verify tests are loaded with correct order
            _viewModel.Tests.Should().HaveCount(1);
            _viewModel.Tests[0].Test.Should().NotBeNull();
            _viewModel.Tests[0].Test.NameReport.Should().Be("External Test");
            _viewModel.Tests[0].Test.IsSendOut.Should().BeFalse();
            _viewModel.Tests[0].Results.Should().HaveCount(1);
            _viewModel.Tests[0].Results[0].Value.Should().Be("12");
            _viewModel.Tests[0].Results[0].Parameter.Should().NotBeNull();
            _viewModel.Tests[0].Results[0].Parameter.Name.Should().Be("Param");

            _viewModel.PreviewDocument.Should().NotBeNull();
            _viewModel.StatusMessage.Should().Contain("تم تحميل التقرير");
        }

        [Fact]
        public async Task PrintAsync_Should_Call_PrintService_For_Report_LogicGuard()
        {
            // 4.7 Print Report - Logic Guard: Verify correct data is passed to print service
            VisitReportData? capturedReport = null;
            bool? capturedPreview = null;
            _printServiceMock.Setup(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), It.IsAny<bool>()))
                .Callback<VisitReportData, bool>((r, preview) =>
                {
                    capturedReport = r;
                    capturedPreview = preview;
                })
                .Returns(Task.CompletedTask);

            var report = BuildReport(visitId: 20, isSendOut: true);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(20)).ReturnsAsync(report);
            _viewModel.VisitId = 20;
            await _viewModel.InvokePrivateAsync("LoadReportAsync");

            await _viewModel.InvokePrivateAsync("PrintAsync", false);

            // Assert - Logic Guard: Verify print service received correct report data
            capturedReport.Should().NotBeNull();
            capturedReport!.Patient.Should().NotBeNull();
            capturedReport.Patient.FullName.Should().Be("P");
            capturedReport.Patient.LabId.Should().Be("L1");
            capturedReport.Visit.Should().NotBeNull();
            capturedReport.Visit.VisitId.Should().Be(20);
            capturedReport.Tests.Should().HaveCount(1);
            capturedReport.Tests[0].Test.NameReport.Should().Be("External Test");
            capturedReport.Tests[0].Test.IsSendOut.Should().BeTrue();
            capturedPreview.Should().Be(false);
        }

        private static VisitReportData BuildReport(int visitId, bool isSendOut)
        {
            var patient = new Patient { PatientId = 1, FullName = "P", LabId = "L1", Gender = "Male" };
            var test = new Test { TestId = 1, Code = "T1", NameReport = "External Test", NameReceipt = "T1", Price = 10m, IsSendOut = isSendOut };
            var visit = new Visit { VisitId = visitId, PatientId = patient.PatientId, Patient = patient, VisitDate = DateTime.Now };
            var visitTest = new VisitTest { VisitTestId = 1, VisitId = visitId, Visit = visit, TestId = test.TestId, Test = test, Price = 10m, Status = "Completed" };
            var parameter = new TestParameter { ParameterId = 1, TestId = test.TestId, Name = "Param", OrderNo = 1 };
            var result = new ResultValue { ResultValueId = 1, VisitTestId = visitTest.VisitTestId, VisitTest = visitTest, ParameterId = 1, Parameter = parameter, Value = "12" };

            return new VisitReportData
            {
                Visit = visit,
                Patient = patient,
                Tests = new List<VisitTestReportItem>
                {
                    new VisitTestReportItem
                    {
                        VisitTest = visitTest,
                        Test = test,
                        Results = new List<ResultValue> { result }
                    }
                }
            };
        }
    }
}
