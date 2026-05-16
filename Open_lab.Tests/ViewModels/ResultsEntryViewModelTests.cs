using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    /// <summary>
    /// اختبارات ResultsEntryViewModel — أُعيد كتابتها لتستخدم الأوامر العامة
    /// (LoadVisitTestsCommand / SaveResultsCommand / VerifyResultsCommand / ReopenResultsCommand)
    /// بدلاً من InvokePrivateAsync لاستدعاء دوال private مباشرة.
    /// المنطق الوظيفي محفوظ بالكامل، فقط آلية الاستدعاء تغيّرت.
    /// </summary>
    public class ResultsEntryViewModelTests : IDisposable
    {
        private readonly Mock<IResultsService> _resultsServiceMock;
        private readonly ResultsEntryViewModel _viewModel;

        public ResultsEntryViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _resultsServiceMock = new Mock<IResultsService>();

            // setup افتراضي لتفادي NullReferenceException عندما يُعيَّن SelectedVisitTest
            // وهو ما يُطلق LoadResultsAsync داخلياً.
            _resultsServiceMock
                .Setup(x => x.GetParametersForTestAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<TestParameter>());
            _resultsServiceMock
                .Setup(x => x.GetResultsForVisitTestAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ResultValue>());

            _viewModel = new ResultsEntryViewModel(_resultsServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public void ResultsEntry_Commands_When_Admin_Should_Be_Enabled()
        {
            // Function: 4.1 — Enter Test Results
            // Arrange
            // Act
            // Assert
            _viewModel.LoadVisitTestsCommand.CanExecute(null).Should().BeTrue();
            _viewModel.SaveResultsCommand.CanExecute(null).Should().BeFalse();
            _viewModel.VerifyResultsCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public async Task LoadVisitTestsCommand_Should_Call_Service()
        {
            // Function: 4.1 — Enter Test Results
            // Arrange
            var visitTests = new List<VisitTest> { new VisitTest { VisitTestId = 1 } };
            _resultsServiceMock.Setup(x => x.GetVisitTestsByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(visitTests);

            // Act — استخدام Command.Execute بدلاً من InvokePrivateAsync
            _viewModel.LoadVisitTestsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _resultsServiceMock.Verify(x => x.GetVisitTestsByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce);
            _viewModel.VisitTests.Should().HaveCount(1);
            _viewModel.StatusMessage.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadVisitTestsCommand_When_Service_Throws_Should_Set_Error()
        {
            // Function: 4.1 — Enter Test Results
            // Arrange
            _resultsServiceMock.Setup(x => x.GetVisitTestsByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("DB Error"));

            // Act
            _viewModel.LoadVisitTestsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("DB Error");
        }

        [Fact]
        public async Task SaveResultsCommand_With_Null_SelectedVisitTest_Should_Do_Nothing()
        {
            // Function: 4.3 — Edit Results
            // Arrange
            _viewModel.SelectedVisitTest = null;

            // Act
            _viewModel.SaveResultsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _resultsServiceMock.Verify(x => x.SaveResultAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")).Should().BeTrue();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SaveResultsCommand_With_Results_Should_Call_Service()
        {
            // Function: 4.3 — Edit Results
            // Arrange
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10 };
            await Task.Delay(50); // انتظار LoadResultsAsync الذي يُطلق ضمنياً عند تعيين SelectedVisitTest

            _viewModel.ResultItems.Clear();
            _viewModel.ResultItems.Add(new ResultEntryItem { ParameterId = 1, Value = "5.0" });
            _resultsServiceMock.Setup(x => x.SaveResultAsync(10, 1, "5.0", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            _viewModel.SaveResultsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _resultsServiceMock.Verify(x => x.SaveResultAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.AtLeastOnce);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task VerifyResultsCommand_With_Null_SelectedVisitTest_Should_Do_Nothing()
        {
            // Function: 4.4 — Create Composite Report
            // Arrange
            _viewModel.SelectedVisitTest = null;

            // Act
            _viewModel.VerifyResultsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _resultsServiceMock.Verify(x => x.VerifyVisitTestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")).Should().BeTrue();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task VerifyResultsCommand_With_Valid_VisitTest_Should_Call_Service()
        {
            // Function: 4.4 — Create Composite Report
            // Arrange
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10 };
            await Task.Delay(50);
            _resultsServiceMock.Setup(x => x.VerifyVisitTestAsync(10, It.IsAny<int>())).Returns(Task.CompletedTask);

            // Act
            _viewModel.VerifyResultsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _resultsServiceMock.Verify(x => x.VerifyVisitTestAsync(10, It.IsAny<int>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SaveResultsCommand_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Function: 4.3 — Edit Results
            // Arrange
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10, TestId = 2, Status = "InProgress" };
            await Task.Delay(50);

            _viewModel.ResultItems.Clear();
            _viewModel.ResultItems.Add(new ResultEntryItem { ParameterId = 1, Value = "5.0" });
            _resultsServiceMock
                .Setup(x => x.SaveResultAsync(10, 1, "5.0", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("save-failed"));

            // Act
            _viewModel.SaveResultsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("save-failed");
        }

        [Fact]
        public async Task VerifyResultsCommand_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Function: 4.4 — Create Composite Report
            // Arrange
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10, TestId = 2, Status = "InProgress" };
            await Task.Delay(50);

            _resultsServiceMock
                .Setup(x => x.VerifyVisitTestAsync(10, It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("verify-failed"));

            // Act
            _viewModel.VerifyResultsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("verify-failed");
        }

        [Fact]
        public async Task ReopenResultsCommand_With_Null_SelectedVisitTest_Should_Do_Nothing()
        {
            // Function: 4.3 — Edit Results
            // Arrange
            _viewModel.SelectedVisitTest = null;

            // Act
            _viewModel.ReopenResultsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _resultsServiceMock.Verify(x => x.ReopenVisitTestAsync(It.IsAny<int>()), Times.Never);
            (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")).Should().BeTrue();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReopenResultsCommand_With_Valid_VisitTest_Should_Call_Service()
        {
            // Function: 4.3 — Edit Results
            // Arrange — لجعل CanExecute = true يجب أن يكون Status = "Verified"
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10, Status = "Verified" };
            await Task.Delay(50);
            _resultsServiceMock.Setup(x => x.ReopenVisitTestAsync(10)).Returns(Task.CompletedTask);

            // Act
            _viewModel.ReopenResultsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _resultsServiceMock.Verify(x => x.ReopenVisitTestAsync(10), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SaveResultsCommand_Should_Save_Selected_ResultItems()
        {
            _resultsServiceMock.Setup(x => x.GetParametersForTestAsync(10))
                .ReturnsAsync(new List<TestParameter> { new TestParameter { ParameterId = 2, TestId = 10, Name = "Hb" } });
            _resultsServiceMock.Setup(x => x.GetResultsForVisitTestAsync(5))
                .ReturnsAsync(new List<ResultValue>());

            _viewModel.SelectedVisitTest = new VisitTestRow
            {
                VisitTestId = 5,
                TestId = 10,
                PatientAge = 30,
                PatientGender = "Male",
                ResultValue = "13.5"
            };
            await Task.Delay(100);

            _viewModel.SaveResultsCommand.Execute(null);
            await Task.Delay(100);

            _resultsServiceMock.Verify(x => x.SaveResultAsync(
                5,
                2,
                "13.5",
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()), Times.Once);
            _viewModel.SelectedVisitTest.Status.Should().Be("Completed");
        }

        [Fact]
        public async Task MarkFinishedAndPrintedCommands_Should_Call_Persistent_Service_Methods()
        {
            _viewModel.VisitTests.Add(new VisitTestRow { VisitTestId = 1, Status = "Pending" });
            _viewModel.VisitTests.Add(new VisitTestRow { VisitTestId = 2, Status = "InProgress" });

            _viewModel.MarkFinishedAllCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.MarkPrintedAllCommand.Execute(null);
            await Task.Delay(100);

            _resultsServiceMock.Verify(x => x.MarkVisitTestsCompletedAsync(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1, 2 }))), Times.Once);
            _resultsServiceMock.Verify(x => x.MarkVisitTestsPrintedAsync(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1, 2 })), It.IsAny<int>()), Times.Once);
            _viewModel.VisitTests.Should().OnlyContain(t => t.IsFinished && t.ShouldPrint);
        }
    }
}