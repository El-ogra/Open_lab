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
            // Function: 10.8 — Logout
            // Arrange
            // Act
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
            SessionContext.Current.UserId = 99;
            SessionContext.Current.AttendanceLogId = 42;
            await viewModel.InvokePrivateAsync("LogoutAsync");

            attendanceServiceMock.Verify(x => x.CloseAsync(42), Times.Once);
            // Assert
            SessionContext.Current.AttendanceLogId.Should().Be(0);
            SessionContext.Current.UserId.Should().Be(0);
            viewModel.IsLoggedIn.Should().BeFalse();
            navigationServiceMock.Verify(x => x.Navigate(NavigationTarget.Login, It.IsAny<System.Action?>()), Times.Exactly(2));
            windowLayoutServiceMock.Verify(x => x.ApplyLoginLayout(), Times.Exactly(2));
        }

        [Fact]
        public async Task LogoutAsync_When_NoAttendanceLog_Should_NotCall_Close_EdgeGuard()
        {
            // Function: 10.8 — Logout
            // Arrange
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
            SessionContext.Current.AttendanceLogId = 0;

            // Act
            await viewModel.InvokePrivateAsync("LogoutAsync");

            // Assert
            attendanceServiceMock.Verify(x => x.CloseAsync(It.IsAny<int>()), Times.Never);
            SessionContext.Current.AttendanceLogId.Should().Be(0);
            viewModel.IsLoggedIn.Should().BeFalse();
        }

        [Fact]
        public async Task LogoutAsync_When_CloseAttendanceThrows_Should_Still_Clear_AttendanceId_FailureGuard()
        {
            // Function: 10.8 — Logout
            // Arrange
            AppSessionTestHelper.ResetToAdmin();

            var navigationServiceMock = new Mock<INavigationService>();
            navigationServiceMock.SetupAdd(x => x.PropertyChanged += It.IsAny<PropertyChangedEventHandler>());
            navigationServiceMock.SetupRemove(x => x.PropertyChanged -= It.IsAny<PropertyChangedEventHandler>());
            navigationServiceMock.SetupGet(x => x.CurrentViewModel).Returns(new HomeViewModel());

            var attendanceServiceMock = new Mock<IAttendanceService>();
            attendanceServiceMock
                .Setup(x => x.CloseAsync(77))
                .ThrowsAsync(new System.InvalidOperationException("close-failed"));

            var windowLayoutServiceMock = new Mock<IMainWindowLayoutService>();
            var viewModel = new MainViewModel(
                navigationServiceMock.Object,
                attendanceServiceMock.Object,
                windowLayoutServiceMock.Object);

            await viewModel.InvokePrivateAsync("OnLoginSuccessAsync");
            SessionContext.Current.AttendanceLogId = 77;

            // Act
            await viewModel.InvokePrivateAsync("LogoutAsync");

            // Assert
            SessionContext.Current.AttendanceLogId.Should().Be(0);
            viewModel.IsLoggedIn.Should().BeFalse();
        }
    }
}

