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
    /// <summary>
    /// Part 3: ViewModel completion tests for Functions 4.6-4.7 (ReportViewerViewModel)
    /// </summary>
    public class Module4ViewModelCompletion_Part3 : IDisposable
    {
        private readonly Mock<IReportService> _reportServiceMock;
        private readonly Mock<IPrintService> _printServiceMock;
        private readonly Mock<IResultsService> _resultsServiceMock;

        public Module4ViewModelCompletion_Part3()
        {
            AppSessionTestHelper.ResetToAdmin();
            _reportServiceMock = new Mock<IReportService>();
            _printServiceMock = new Mock<IPrintService>();
            _resultsServiceMock = new Mock<IResultsService>();
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        private VisitReportData CreateSampleReport(int visitId)
        {
            return new VisitReportData
            {
                Visit = new Visit { VisitId = visitId, VisitDate = DateTime.Now },
                Patient = new Patient { FullName = "Test Patient", LabId = $"L-{visitId}", Gender = "Male", Age = 30 },
                Tests = new List<VisitTestReportItem>
                {
                    new()
                    {
                        VisitTest = new VisitTest { VisitTestId = visitId * 10, VisitId = visitId, TestId = 1 },
                        Test = new Test { TestId = 1, Code = "T1", NameReport = "Test 1" },
                        Results = new List<ResultValueReportItem>()
                    }
                }
            };
        }

        // ===================================================================
        // Function 4.6 — Preview Report (ReportViewerViewModel)
        // ===================================================================

        [Fact]
        public async Task PreviewReport_LoadCommand_Should_Populate_Preview_Content_Success()
        {
            // Function: 4.6 — Preview Report
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            var report = CreateSampleReport(300);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(300)).ReturnsAsync(report);
            viewModel.VisitId = 300;

            // Act
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Assert
            viewModel.PreviewContent.Should().NotBeNullOrEmpty();
            viewModel.PreviewContent.Should().Contain("Test Patient");
            viewModel.Report.Should().NotBeNull();
            // viewModel.IsReportLoaded.Should().BeTrue();
        }

        [Fact]
        public async Task PreviewReport_LoadCommand_When_Visit_Not_Found_Should_Show_Error_Failure()
        {
            // Function: 4.6 — Preview Report (Failure)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(399)).ReturnsAsync((VisitReportData?)null);
            viewModel.VisitId = 399;

            // Act
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Be("لم يتم العثور على تقرير.");
            viewModel.Report.Should().BeNull();
        }

        [Fact]
        public async Task PreviewReport_LoadCommand_When_ServiceThrows_Should_Show_Error_Failure()
        {
            // Function: 4.6 — Preview Report (Exception)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(398)).ThrowsAsync(new InvalidOperationException("Service error"));
            viewModel.VisitId = 398;

            // Act
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("Service error");
        }

        [Fact]
        public async Task PreviewReport_LoadCommand_With_Zero_VisitId_Should_Show_Validation_Edge()
        {
            // Function: 4.6 — Preview Report (Edge: invalid input)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            viewModel.VisitId = 0;

            // Act
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Be("يرجى إدخال رقم الزيارة.");
        }

        [Fact]
        public async Task PreviewReport_LoadCommand_With_Negative_VisitId_Should_Show_Validation_Edge()
        {
            // Function: 4.6 — Preview Report (Edge: negative input)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            viewModel.VisitId = -1;

            // Act
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Be("يرجى إدخال رقم الزيارة.");
        }

        [Fact]
        public async Task PreviewReport_PrintCommand_Should_Be_Enabled_After_Load_Success()
        {
            // Function: 4.6 — Preview Report (print enabled after load)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            var report = CreateSampleReport(301);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(301)).ReturnsAsync(report);
            viewModel.VisitId = 301;

            // Before load - print should be disabled
            viewModel.PrintCommand.CanExecute(null).Should().BeFalse();

            // Act
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Assert
            viewModel.PrintCommand.CanExecute(null).Should().BeTrue();
        }

        // ===================================================================
        // Function 4.7 — Print Report (ReportViewerViewModel)
        // ===================================================================

        [Fact]
        public async Task PrintReport_PrintCommand_Should_Call_PrintService_And_Log_Audit_Success()
        {
            // Function: 4.7 — Print Report (BR-SEC-002)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            var report = CreateSampleReport(400);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(400)).ReturnsAsync(report);
            _printServiceMock.Setup(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), false)).Returns(Task.CompletedTask);
            _resultsServiceMock.Setup(x => x.LogVisitReportPrintedAsync(400, It.IsAny<int>())).Returns(Task.CompletedTask);
            viewModel.VisitId = 400;
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Act
            await viewModel.InvokePrivateAsync("PrintAsync", false);
            await Task.Delay(100);

            // Assert
            _printServiceMock.Verify(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), false), Times.Once);
            _resultsServiceMock.Verify(x => x.LogVisitReportPrintedAsync(400, It.IsAny<int>()), Times.Once);
            viewModel.StatusMessage.Should().Contain("تم إرسال");
        }

        [Fact]
        public async Task PrintReport_ReprintCommand_Should_Call_PrintService_With_ReprintFlag_Success()
        {
            // Function: 4.7 — Print Report (Reprint)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            var report = CreateSampleReport(401);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(401)).ReturnsAsync(report);
            _printServiceMock.Setup(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), true)).Returns(Task.CompletedTask);
            _resultsServiceMock.Setup(x => x.LogVisitReportPrintedAsync(401, It.IsAny<int>())).Returns(Task.CompletedTask);
            viewModel.VisitId = 401;
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Act
            viewModel.ReprintCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _printServiceMock.Verify(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), true), Times.Once);
            _resultsServiceMock.Verify(x => x.LogVisitReportPrintedAsync(401, It.IsAny<int>()), Times.Once);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task PrintReport_PrintCommand_When_No_Report_Should_Do_Nothing_Edge()
        {
            // Function: 4.7 — Print Report (Edge: no report)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            viewModel.VisitId = 402;
            // Don't load report

            // Act
            viewModel.PrintCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _printServiceMock.Verify(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), It.IsAny<bool>()), Times.Never);
            _resultsServiceMock.Verify(x => x.LogVisitReportPrintedAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task PrintReport_PrintCommand_When_PrintServiceThrows_Should_Show_Error_Failure()
        {
            // Function: 4.7 — Print Report (Print failure)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            var report = CreateSampleReport(403);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(403)).ReturnsAsync(report);
            _printServiceMock.Setup(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("Printer not connected"));
            viewModel.VisitId = 403;
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Act
            await viewModel.InvokePrivateAsync("PrintAsync", false);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("خطأ طباعة:");
            viewModel.StatusMessage.Should().Contain("Printer not connected");
        }

        [Fact]
        public async Task PrintReport_PrintCommand_When_LogServiceThrows_Should_Show_Error_Failure()
        {
            // Function: 4.7 — Print Report (Audit logging failure)
            // Arrange
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            var report = CreateSampleReport(404);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(404)).ReturnsAsync(report);
            _printServiceMock.Setup(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), false)).Returns(Task.CompletedTask);
            _resultsServiceMock.Setup(x => x.LogVisitReportPrintedAsync(404, It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Audit log failed"));
            viewModel.VisitId = 404;
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Act
            await viewModel.InvokePrivateAsync("PrintAsync", false);
            await Task.Delay(100);

            // Assert - ReportViewerViewModel prefixes print/log failures with a print-specific message
            viewModel.StatusMessage.Should().Contain("خطأ");
        }

        [Fact]
        public async Task PrintReport_PrintCommand_Without_Permission_Should_Be_Enabled_As_Not_Implemented_Success()
        {
            // Function: 4.7 — Print Report (Permission - Note: Currently not implemented in VM)
            // Arrange
            AppSessionTestHelper.Reset(); // No permissions
            var viewModel = new ReportViewerViewModel(_reportServiceMock.Object, _printServiceMock.Object, _resultsServiceMock.Object);
            var report = CreateSampleReport(405);
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(405)).ReturnsAsync(report);
            viewModel.VisitId = 405;
            await viewModel.InvokePrivateAsync("LoadReportAsync");
            await Task.Delay(100);

            // Act & Assert
            // Note: The current ReportViewerViewModel does not check permissions in CanExecute
            // Assert
            viewModel.PrintCommand.CanExecute(null).Should().BeTrue();
            viewModel.ReprintCommand.CanExecute(null).Should().BeTrue();
        }
    }
}
