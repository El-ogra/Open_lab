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
    /// Part 2: ViewModel completion tests for Functions 4.4-4.5 (CombinedReportViewModel)
    /// </summary>
    public class Module4ViewModelCompletion_Part2 : IDisposable
    {
        private readonly Mock<IReportService> _reportServiceMock;

        public Module4ViewModelCompletion_Part2()
        {
            AppSessionTestHelper.ResetToAdmin();
            _reportServiceMock = new Mock<IReportService>();
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        private VisitReportData CreateSampleReport(int visitId, int testCount = 1)
        {
            var patient = new Patient { PatientId = visitId, FullName = "Test Patient", LabId = $"L-{visitId}" };
            var tests = new List<VisitTestReportItem>();
            var codes = new[] { "A", "B", "C", "D", "E" };

            for (int i = 0; i < testCount && i < codes.Length; i++)
            {
                tests.Add(new VisitTestReportItem
                {
                    VisitTest = new VisitTest { VisitTestId = visitId * 10 + i, VisitId = visitId, TestId = visitId * 10 + i },
                    Test = new Test { TestId = visitId * 10 + i, Code = codes[i], NameReport = $"Test {codes[i]}" },
                    Results = new List<ResultValueReportItem>()
                });
            }

            return new VisitReportData
            {
                Visit = new Visit { VisitId = visitId, PatientId = visitId, VisitDate = DateTime.Now },
                Patient = patient,
                Tests = tests
            };
        }

        // ===================================================================
        // Function 4.4 — Create Composite Report (CombinedReportViewModel)
        // ===================================================================

        [Fact]
        public async Task CreateCompositeReport_LoadCommand_With_Valid_Visit_Should_Load_All_Tests_Success()
        {
            // Function: 4.4 — Create Composite Report
            // Arrange
            var viewModel = new CombinedReportViewModel(_reportServiceMock.Object);
            var report = CreateSampleReport(100, 3);
            _reportServiceMock.Setup(x => x.GetCompositeReportAsync(100, It.IsAny<IReadOnlyCollection<int>>())).ReturnsAsync(report);
            viewModel.VisitId = 100;

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.Tests.Should().HaveCount(3);
            viewModel.PatientName.Should().Be("Test Patient");
            viewModel.StatusMessage.Should().Contain("3");
        }

        [Fact]
        public async Task CreateCompositeReport_LoadCommand_When_Visit_Not_Found_Should_Show_Error_Failure()
        {
            // Function: 4.4 — Create Composite Report (Failure)
            // Arrange
            var viewModel = new CombinedReportViewModel(_reportServiceMock.Object);
            _reportServiceMock.Setup(x => x.GetCompositeReportAsync(999, It.IsAny<IReadOnlyCollection<int>>())).ReturnsAsync((VisitReportData?)null);
            viewModel.VisitId = 999;

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Be("لم يتم العثور على تقرير.");
            viewModel.Tests.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateCompositeReport_LoadCommand_With_Zero_VisitId_Should_Show_Validation_Edge()
        {
            // Function: 4.4 — Create Composite Report (Edge: invalid input)
            // Arrange
            var viewModel = new CombinedReportViewModel(_reportServiceMock.Object);
            viewModel.VisitId = 0;

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Be("يرجى إدخال رقم الزيارة.");
            _reportServiceMock.Verify(x => x.GetCompositeReportAsync(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
        }

        [Fact]
        public async Task CreateCompositeReport_LoadCommand_When_ServiceThrows_Should_Show_Error_Failure()
        {
            // Function: 4.4 — Create Composite Report (Exception)
            // Arrange
            var viewModel = new CombinedReportViewModel(_reportServiceMock.Object);
            _reportServiceMock.Setup(x => x.GetCompositeReportAsync(101, It.IsAny<IReadOnlyCollection<int>>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));
            viewModel.VisitId = 101;

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("Database error");
        }

        [Fact]
        public async Task CreateCompositeReport_LoadCommand_With_Empty_TestList_Should_Set_Status_Edge()
        {
            // Function: 4.4 — Create Composite Report (Edge: no tests)
            // Arrange
            var viewModel = new CombinedReportViewModel(_reportServiceMock.Object);
            var emptyReport = new VisitReportData
            {
                Visit = new Visit { VisitId = 102, VisitDate = DateTime.Now },
                Patient = new Patient { FullName = "Empty Patient" },
                Tests = new List<VisitTestReportItem>()
            };
            _reportServiceMock.Setup(x => x.GetCompositeReportAsync(102, It.IsAny<IReadOnlyCollection<int>>())).ReturnsAsync(emptyReport);
            viewModel.VisitId = 102;

            // Act
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.Tests.Should().BeEmpty();
            viewModel.PatientName.Should().Be("Empty Patient");
        }

        [Fact]
        public async Task ArrangeReportOrder_With_No_Selection_Should_Disable_Commands_Edge()
        {
            // Function: 4.5 — Arrange Report Order (Edge: no selection)
            // Arrange
            var viewModel = new CombinedReportViewModel(_reportServiceMock.Object);
            var report = CreateSampleReport(206, 2);
            _reportServiceMock.Setup(x => x.GetCompositeReportAsync(206, It.IsAny<IReadOnlyCollection<int>>())).ReturnsAsync(report);
            viewModel.VisitId = 206;
            viewModel.LoadCommand.Execute(null);
            await Task.Delay(100);

            // Act - Don't select anything

            // Assert
            viewModel.MoveUpCommand.CanExecute(null).Should().BeFalse();
            viewModel.MoveDownCommand.CanExecute(null).Should().BeFalse();
        }
    }
}
