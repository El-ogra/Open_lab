using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class SampleCollectionViewModelTests : IDisposable
    {
        private readonly Mock<ISampleCollectionService> _sampleCollectionServiceMock;
        private readonly SampleCollectionViewModel _viewModel;

        public SampleCollectionViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _sampleCollectionServiceMock = new Mock<ISampleCollectionService>();
            _viewModel = new SampleCollectionViewModel(_sampleCollectionServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public void Commands_When_Admin_Should_Be_Enabled()
        {
            _viewModel.LoadCommand.CanExecute(null).Should().BeTrue();
            _viewModel.MarkCollectedCommand.CanExecute(null).Should().BeFalse(); // SelectedRow == null
        }

        [Fact]
        public void MarkCollectedCommand_CanExecute_When_Row_Selected_Should_Return_True()
        {
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _viewModel.MarkCollectedCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public async Task LoadAsync_Should_Load_Sample_Collection_Rows()
        {
            var rows = new List<SampleCollectionRow> { new SampleCollectionRow { VisitTestId = 1 } };
            _sampleCollectionServiceMock.Setup(x => x.GetRowsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(rows);

            await _viewModel.InvokePrivateAsync("LoadAsync");

            _viewModel.Items.Should().HaveCount(1);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task MarkCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing()
        {
            _viewModel.SelectedRow = null;
            await _viewModel.InvokePrivateAsync("MarkCollectedAsync");
            _sampleCollectionServiceMock.Verify(x => x.MarkCollectedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task MarkCollectedAsync_With_Valid_Row_Should_Call_Service()
        {
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _sampleCollectionServiceMock.Setup(x => x.MarkCollectedAsync(10, It.IsAny<int>(), false, It.IsAny<int?>())).Returns(Task.CompletedTask);

            await _viewModel.InvokePrivateAsync("MarkCollectedAsync");

            _sampleCollectionServiceMock.Verify(x => x.MarkCollectedAsync(10, It.IsAny<int>(), false, It.IsAny<int?>()), Times.Once);
        }


        [Fact]
        public async Task MarkSeparatedAsync_With_Valid_Row_Should_Call_Service()
        {
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _sampleCollectionServiceMock.Setup(x => x.MarkSeparatedAsync(10, "Centrifuge")).Returns(Task.CompletedTask);

            await _viewModel.InvokePrivateAsync("MarkSeparatedAsync");

            _sampleCollectionServiceMock.Verify(x => x.MarkSeparatedAsync(10, "Centrifuge"), Times.Once);
        }

        [Fact]
        public async Task MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service()
        {
            _viewModel.SelectedRow = new SampleCollectionRow { VisitTestId = 10 };
            _sampleCollectionServiceMock.Setup(x => x.MarkNotCollectedAsync(10)).Returns(Task.CompletedTask);

            await _viewModel.InvokePrivateAsync("MarkNotCollectedAsync");

            _sampleCollectionServiceMock.Verify(x => x.MarkNotCollectedAsync(10), Times.Once);
        }
    }
}
