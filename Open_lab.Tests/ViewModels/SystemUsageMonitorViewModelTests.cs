using System;
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
    public class SystemUsageMonitorViewModelTests : IDisposable
    {
        private readonly Mock<ISystemMonitorService> _systemMonitorServiceMock = new();
        private readonly SystemUsageMonitorViewModel _viewModel;

        public SystemUsageMonitorViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _systemMonitorServiceMock.Setup(x => x.GetActiveSessionsAsync())
                .ReturnsAsync(new List<ActiveSessionRow>
                {
                    new() { UserId = 1, Username = "user1", LoginAt = DateTime.Now, LastActivityAt = DateTime.Now, Status = "متصل" }
                });

            _viewModel = new SystemUsageMonitorViewModel(_systemMonitorServiceMock.Object);
        }

        [Fact]
        public async Task RefreshAsync_Should_Populate_Sessions()
        {
            await _viewModel.InvokePrivateAsync("RefreshAsync");

            _viewModel.Sessions.Should().ContainSingle();
            _viewModel.Sessions[0].Username.Should().Be("user1");
            _viewModel.StatusMessage.Should().Contain("1");
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
