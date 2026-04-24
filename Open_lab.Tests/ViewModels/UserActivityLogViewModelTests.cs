using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class UserActivityLogViewModelTests : IDisposable
    {
        private readonly Mock<IUserActivityService> _userActivityServiceMock = new();
        private readonly UserActivityLogViewModel _viewModel;

        public UserActivityLogViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _userActivityServiceMock.Setup(x => x.GetRecentActivitiesAsync(It.IsAny<int?>(), It.IsAny<int>()))
                .ReturnsAsync(new List<UserActivityRow>
                {
                    new() { AuditLogId = 1, Username = "admin", ActivityDescription = "تعديل في بيانات مريض" }
                });

            _viewModel = new UserActivityLogViewModel(_userActivityServiceMock.Object);
        }

        [Fact]
        public async Task LoadAsync_When_MaxCount_Is_Invalid_Should_Request_Default_100_And_Populate_Items()
        {
            _viewModel.MaxCount = 0;
            _userActivityServiceMock.Invocations.Clear();

            await _viewModel.InvokePrivateAsync("LoadAsync");

            _userActivityServiceMock.Verify(x => x.GetRecentActivitiesAsync(null, 100), Times.Once);
            _viewModel.Items.Should().ContainSingle();
            _viewModel.StatusMessage.Should().Contain("تم تحميل 1 سجل");
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
