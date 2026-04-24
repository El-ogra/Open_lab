using System.ComponentModel;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class MainViewModelTests
    {
        [Fact]
        public async Task LogoutAsync_Should_Close_Attendance_And_Clear_Session()
        {
            AppSessionTestHelper.ResetToAdmin();

            var navigationServiceMock = new Mock<INavigationService>();
            navigationServiceMock.SetupAdd(x => x.PropertyChanged += It.IsAny<PropertyChangedEventHandler>());
            navigationServiceMock.SetupRemove(x => x.PropertyChanged -= It.IsAny<PropertyChangedEventHandler>());
            navigationServiceMock.SetupGet(x => x.CurrentViewModel).Returns(new HomeViewModel());

            var attendanceServiceMock = new Mock<IAttendanceService>();
            var windowLayoutServiceMock = new Mock<IMainWindowLayoutService>();

            var viewModel = new MainViewModel(
                navigationServiceMock.Object,
                attendanceServiceMock.Object,
                windowLayoutServiceMock.Object);

            await viewModel.InvokePrivateAsync("OnLoginSuccessAsync");
            AppSession.UserId = 99;
            AppSession.AttendanceLogId = 42;
            await viewModel.InvokePrivateAsync("LogoutAsync");

            attendanceServiceMock.Verify(x => x.CloseAsync(42), Times.Once);
            AppSession.AttendanceLogId.Should().Be(0);
            AppSession.UserId.Should().Be(0);
            viewModel.IsLoggedIn.Should().BeFalse();
            navigationServiceMock.Verify(x => x.Navigate(NavigationTarget.Login, It.IsAny<System.Action?>()), Times.Exactly(2));
            windowLayoutServiceMock.Verify(x => x.ApplyLoginLayout(), Times.Exactly(2));
        }
    }
}
