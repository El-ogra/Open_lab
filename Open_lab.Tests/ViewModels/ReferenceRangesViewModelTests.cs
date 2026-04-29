using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class ReferenceRangesViewModelTests : IDisposable
    {
        private readonly Mock<ITestCatalogService> _testCatalogServiceMock;
        private readonly ReferenceRangesViewModel _viewModel;

        public ReferenceRangesViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _testCatalogServiceMock = new Mock<ITestCatalogService>();
            _viewModel = new ReferenceRangesViewModel(_testCatalogServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task SaveAsync_NewRange_Should_Call_CreateRange()
        {
            // Function: 3.3 — Reference Values - Logic Guard
            // Arrange
            var test = new Test { TestId = 1, Code = "T1" };
            _viewModel.SelectedTest = test;
            _viewModel.Gender = "Male";
            _viewModel.AgeFrom = 20;
            _viewModel.AgeTo = 40;
            _viewModel.LowValue = 10;
            _viewModel.HighValue = 20;

            _testCatalogServiceMock.Setup(x => x.CreateReferenceRangeAsync(It.IsAny<TestReferenceRange>()))
                .ReturnsAsync((TestReferenceRange r) => { r.RangeId = 50; return r; });

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.CreateReferenceRangeAsync(It.Is<TestReferenceRange>(r =>
                r.TestId == 1 &&
                r.Gender == "Male" &&
                r.AgeFrom == 20 &&
                r.AgeTo == 40 &&
                r.LowValue == 10 &&
                r.HighValue == 20
            )), Times.Once);
            
            _viewModel.Ranges.Should().HaveCount(1);
        }

        [Fact]
        public void SelectedRange_Setter_Should_Load_Data()
        {
            // Arrange
            var range = new TestReferenceRange
            {
                RangeId = 1,
                Gender = "Female",
                AgeFrom = 1,
                AgeTo = 12,
                LowValue = 5,
                HighValue = 8,
                NormalText = "Normal"
            };

            // Act
            _viewModel.SelectedRange = range;

            // Assert
            _viewModel.Gender.Should().Be("Female");
            _viewModel.AgeFrom.Should().Be(1);
            _viewModel.AgeTo.Should().Be(12);
            _viewModel.LowValue.Should().Be(5);
            _viewModel.HighValue.Should().Be(8);
            _viewModel.NormalText.Should().Be("Normal");
        }

        [Fact]
        public async Task SaveAsync_Without_SelectedTest_Should_NotCall_Service_FailureGuard()
        {
            // Arrange
            _viewModel.SelectedTest = null;

            // Act
            await _viewModel.InvokePrivateAsync("SaveAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.CreateReferenceRangeAsync(It.IsAny<TestReferenceRange>()), Times.Never);
            _testCatalogServiceMock.Verify(x => x.UpdateReferenceRangeAsync(It.IsAny<TestReferenceRange>()), Times.Never);
            _viewModel.StatusMessage.Should().Contain("اختر تحليلًا");
        }

        [Fact]
        public async Task DeleteAsync_When_NoSelection_Should_NotCall_Service_EdgeGuard()
        {
            // Arrange
            _viewModel.SelectedRange = null;

            // Act
            await _viewModel.InvokePrivateAsync("DeleteAsync");

            // Assert
            _testCatalogServiceMock.Verify(x => x.DeleteReferenceRangeAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
