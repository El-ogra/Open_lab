using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class TestCommentsViewModelTests : IDisposable
    {
        private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
        private readonly TestCommentsViewModel _viewModel;

        public TestCommentsViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _testCatalogServiceMock = new Mock<ITestCatalogService>();
            _viewModel = new TestCommentsViewModel(_testCatalogServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task SaveAsync_Should_Persist_Low_High_Comments_LogicGuard()
        {
            // 3.4 Low/High Comments - Logic Guard: Verify properties are correctly sent to service
            // Arrange
            var test = new Test { TestId = 1, Code = "T1" };
            _viewModel.SelectedTest = test;
            _viewModel.CommentText = "Normal Comment";
            _viewModel.LowComment = "LOW ALERT";
            _viewModel.HighComment = "HIGH ALERT";
            _viewModel.IsDefault = true;

            _testCatalogServiceMock.Setup(x => x.CreateTestCommentAsync(It.IsAny<TestComment>()))
                .ReturnsAsync((TestComment c) => { c.CommentId = 100; return c; });

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.CreateTestCommentAsync(It.Is<TestComment>(c =>
                c.TestId == 1 &&
                c.CommentText == "Normal Comment" &&
                c.LowComment == "LOW ALERT" &&
                c.HighComment == "HIGH ALERT" &&
                c.IsDefault == true
            )), Times.Once);
            
            _viewModel.Comments.Should().HaveCount(1);
            _viewModel.StatusMessage.Should().Contain("تم حفظ");
        }

        [Fact]
        public void SelectedComment_Setter_Should_Load_Low_High_Comments()
        {
            // Arrange
            var comment = new TestComment
            {
                CommentId = 5,
                CommentText = "Text",
                LowComment = "Low",
                HighComment = "High",
                IsDefault = true
            };

            // Act
            _viewModel.SelectedComment = comment;

            // Assert
            _viewModel.CommentText.Should().Be("Text");
            _viewModel.LowComment.Should().Be("Low");
            _viewModel.HighComment.Should().Be("High");
            _viewModel.IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task SaveAsync_When_SelectedTestIsNull_Should_Set_ValidationMessage_FailureGuard()
        {
            // Arrange
            _viewModel.SelectedTest = null;
            _viewModel.CommentText = "Any";

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("اختر تحليلًا");
            _testCatalogServiceMock.Verify(x => x.CreateTestCommentAsync(It.IsAny<TestComment>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Arrange
            _viewModel.SelectedComment = new TestComment { CommentId = 7, TestId = 1, CommentText = "C" };
            _testCatalogServiceMock
                .Setup(x => x.DeleteTestCommentAsync(7))
                .ThrowsAsync(new Exception("delete-failed"));

            // Act
            await _viewModel.InvokePrivateAsync("DeleteAsync");

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("delete-failed");
        }
    }
}
