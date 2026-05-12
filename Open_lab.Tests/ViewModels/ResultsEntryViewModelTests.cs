using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class ResultsEntryViewModelTests : IDisposable
    {
        private readonly Mock<IResultsService> _resultsServiceMock;
        private readonly ResultsEntryViewModel _viewModel;

        public ResultsEntryViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _resultsServiceMock = new Mock<IResultsService>();
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
        public async Task LoadVisitTestsAsync_Should_Call_Service()
        {
            // Function: 4.1 — Enter Test Results
            // Arrange
            // Act
            var visitTests = new List<VisitTest> { new VisitTest { VisitTestId = 1 } };
            _resultsServiceMock.Setup(x => x.GetVisitTestsByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(visitTests);
            await _viewModel.InvokePrivateAsync("LoadVisitTestsAsync");
            _resultsServiceMock.Verify(x => x.GetVisitTestsByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            // Assert
            _viewModel.VisitTests.Should().HaveCount(1);
            _viewModel.StatusMessage.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadVisitTestsAsync_When_Service_Throws_Should_Set_Error()
        {
            // Function: 4.1 — Enter Test Results
            // Arrange
            // Act
            _resultsServiceMock.Setup(x => x.GetVisitTestsByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("DB Error"));
            await _viewModel.InvokePrivateAsync("LoadVisitTestsAsync");
            // Assert
            _viewModel.StatusMessage.Should().Contain("DB Error");
        }

        [Fact]
        public async Task SaveResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
        {
            // Function: 4.3 — Edit Results
            // Arrange
            // Act
            _viewModel.SelectedVisitTest = null;
            await _viewModel.InvokePrivateAsync("SaveResultsAsync");
            _resultsServiceMock.Verify(x => x.SaveResultAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            // Assert
            (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")).Should().BeTrue();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SaveResultsAsync_With_Results_Should_Call_Service()
        {
            // Function: 4.3 — Edit Results
            // Arrange
            // Act
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10 };
            _viewModel.ResultItems.Clear();
            _viewModel.ResultItems.Add(new ResultEntryItem { ParameterId = 1, Value = "5.0" });
            _resultsServiceMock.Setup(x => x.SaveResultAsync(10, 1, "5.0", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            await _viewModel.InvokePrivateAsync("SaveResultsAsync");
            _resultsServiceMock.Verify(x => x.SaveResultAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task VerifyResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
        {
            // Function: 4.4 — Create Composite Report
            // Arrange
            // Act
            _viewModel.SelectedVisitTest = null;
            await _viewModel.InvokePrivateAsync("VerifyResultsAsync");
            _resultsServiceMock.Verify(x => x.VerifyVisitTestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            // Assert
            (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")).Should().BeTrue();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task VerifyResultsAsync_With_Valid_VisitTest_Should_Call_Service()
        {
            // Function: 4.4 — Create Composite Report
            // Arrange
            // Act
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10 };
            _resultsServiceMock.Setup(x => x.VerifyVisitTestAsync(10, It.IsAny<int>())).Returns(Task.CompletedTask);
            await _viewModel.InvokePrivateAsync("VerifyResultsAsync");
            _resultsServiceMock.Verify(x => x.VerifyVisitTestAsync(10, It.IsAny<int>()), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SaveResultsAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Function: 4.3 — Edit Results
            // Arrange
            // Act
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10, TestId = 2, Status = "InProgress" };
            _viewModel.ResultItems.Clear();
            _viewModel.ResultItems.Add(new ResultEntryItem { ParameterId = 1, Value = "5.0" });
            _resultsServiceMock
                .Setup(x => x.SaveResultAsync(10, 1, "5.0", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("save-failed"));

            await _viewModel.InvokePrivateAsync("SaveResultsAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("save-failed");
        }

        [Fact]
        public async Task VerifyResultsAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Function: 4.4 — Create Composite Report
            // Arrange
            // Act
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10, TestId = 2, Status = "InProgress" };
            _resultsServiceMock
                .Setup(x => x.VerifyVisitTestAsync(10, It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("verify-failed"));

            await _viewModel.InvokePrivateAsync("VerifyResultsAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("verify-failed");
        }

        [Fact]
        public async Task ReopenResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
        {
            // Function: 4.3 — Edit Results
            // Arrange
            // Act
            _viewModel.SelectedVisitTest = null;
            await _viewModel.InvokePrivateAsync("ReopenResultsAsync");
            _resultsServiceMock.Verify(x => x.ReopenVisitTestAsync(It.IsAny<int>()), Times.Never);
            // Assert
            (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")).Should().BeTrue();
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReopenResultsAsync_With_Valid_VisitTest_Should_Call_Service()
        {
            // Function: 4.3 — Edit Results
            // Arrange
            // Act
            _viewModel.SelectedVisitTest = new VisitTestRow { VisitTestId = 10 };
            _resultsServiceMock.Setup(x => x.ReopenVisitTestAsync(10)).Returns(Task.CompletedTask);
            await _viewModel.InvokePrivateAsync("ReopenResultsAsync");
            _resultsServiceMock.Verify(x => x.ReopenVisitTestAsync(10), Times.Once);
            // Assert
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }
    }
}


