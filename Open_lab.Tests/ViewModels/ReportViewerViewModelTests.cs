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
        private readonly Mock<IResultsService> _resultsServiceMock;
        private readonly ReportViewerViewModel _viewModel;

        public ReportViewerViewModelTests()
        {
            _reportServiceMock = new Mock<IReportService>();
            _printServiceMock = new Mock<IPrintService>();
            _resultsServiceMock = new Mock<IResultsService>();
            _viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
        }

        [Fact]
        public async Task LoadReportAsync_With_Valid_Visit_Should_Build_Preview_LogicGuard()
        {
            // Function: 4.4 — Composite Report - 4.5 Report Order Validation
            // Arrange
            // Act
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
            _viewModel.Tests[0].Results[0].Result.Value.Should().Be("12");
            _viewModel.Tests[0].Results[0].Result.Parameter.Should().NotBeNull();
            _viewModel.Tests[0].Results[0].Result.Parameter.Name.Should().Be("Param");

            _viewModel.PreviewContent.Should().Contain("معاينة التقرير");
            _viewModel.StatusMessage.Should().Contain("تم تحميل التقرير");
        }

        [Fact]
        public async Task PrintAsync_Should_Call_PrintService_For_Report_LogicGuard()
        {
            // Function: 4.7 — Print Report - Logic Guard: Verify correct data is passed to print service
            // Arrange
            // Act
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

            // Verify Audit Logging (Function 4.7)
            _resultsServiceMock.Verify(x => x.LogVisitReportPrintedAsync(20, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task LoadReportAsync_With_InvalidVisitId_Should_Set_Validation_Message_FailureGuard()
        {
            // Function: 4.7 — Print Report - Logic Guard: Verify correct data is passed to print service
            // Arrange
            // Act
            _viewModel.VisitId = 0;

            await _viewModel.InvokePrivateAsync("LoadReportAsync");

            // Assert
            _viewModel.StatusMessage.Should().Be("يرجى إدخال رقم الزيارة.");
            _reportServiceMock.Verify(x => x.GetVisitReportAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task LoadReportAsync_When_ServiceReturnsNull_Should_Set_NotFound_Message_FailureGuard()
        {
            // Function: 4.7 — Print Report - Logic Guard: Verify correct data is passed to print service
            // Arrange
            // Act
            _viewModel.VisitId = 99;
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(99)).ReturnsAsync((VisitReportData?)null);

            await _viewModel.InvokePrivateAsync("LoadReportAsync");

            // Assert
            _viewModel.StatusMessage.Should().Be("لم يتم العثور على تقرير.");
            _viewModel.Report.Should().BeNull();
            _viewModel.Tests.Should().BeEmpty();
        }

        [Fact]
        public async Task PrintAsync_When_PrintServiceThrows_Should_Set_PrintErrorMessage_EdgeGuard()
        {
            // Function: 4.7 — Print Report - Logic Guard: Verify correct data is passed to print service
            // Arrange
            // Act
            var report = BuildReport(visitId: 55, isSendOut: false);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(55)).ReturnsAsync(report);
            _printServiceMock.Setup(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("printer offline"));
            _viewModel.VisitId = 55;
            await _viewModel.InvokePrivateAsync("LoadReportAsync");

            await _viewModel.InvokePrivateAsync("PrintAsync", false);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ طباعة:");
            _viewModel.StatusMessage.Should().Contain("printer offline");
            _resultsServiceMock.Verify(x => x.LogVisitReportPrintedAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
                        Results = new List<ResultValueReportItem> { new ResultValueReportItem { Result = result } }
                    }
                }
            };
        }
    }
}
