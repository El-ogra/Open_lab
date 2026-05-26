using FluentAssertions;
using Open_lab.Services;

namespace Open_lab.Tests.Services
{
    public class SessionContextTests : IDisposable
    {
        public SessionContextTests()
        {
            SessionContext.Current.EndSession();
        }

        public void Dispose()
        {
            SessionContext.Current.EndSession();
        }

        [Fact]
        public void HasPermission_When_FullAccess_Should_Always_Return_True()
        {
            SessionContext.Current.BeginSession(1, "admin", new[] { PermissionCodes.FullAccess });

            SessionContext.Current.HasPermission("Any.Random.Permission").Should().BeTrue();
            SessionContext.Current.HasPermission(PermissionCodes.PatientsEdit).Should().BeTrue();
            SessionContext.Current.IsAdmin.Should().BeTrue();
        }

        [Fact]
        public void HasPermission_When_Has_Specific_Permission_Should_Return_True_Only_For_That_Permission()
        {
            SessionContext.Current.BeginSession(2, "testuser", new[] { PermissionCodes.PatientsView, PermissionCodes.TestsEdit });

            SessionContext.Current.HasPermission(PermissionCodes.PatientsView).Should().BeTrue();
            SessionContext.Current.HasPermission(PermissionCodes.TestsEdit).Should().BeTrue();
            SessionContext.Current.HasPermission(PermissionCodes.PatientsEdit).Should().BeFalse();
            SessionContext.Current.HasPermission(PermissionCodes.AccountsView).Should().BeFalse();
            SessionContext.Current.IsAdmin.Should().BeFalse();
        }

        [Fact]
        public void HasPermission_When_No_Permissions_Should_Return_False()
        {
            SessionContext.Current.BeginSession(2, "testuser", Array.Empty<string>());

            SessionContext.Current.HasPermission(PermissionCodes.PatientsView).Should().BeFalse();
            SessionContext.Current.HasPermission(PermissionCodes.TestsEdit).Should().BeFalse();
            SessionContext.Current.HasPermission("Any.Random.Permission").Should().BeFalse();
        }

        [Fact]
        public void EndSession_Should_Reset_All_Properties()
        {
            SessionContext.Current.BeginSession(42, "testuser", new[] { PermissionCodes.FullAccess }, 99);

            SessionContext.Current.EndSession();

            SessionContext.Current.UserId.Should().Be(0);
            SessionContext.Current.Username.Should().BeEmpty();
            SessionContext.Current.IsAdmin.Should().BeFalse();
            SessionContext.Current.AttendanceLogId.Should().Be(0);
            SessionContext.Current.HasPermission(PermissionCodes.FullAccess).Should().BeFalse();
        }

        [Fact]
        public void BeginSession_Should_Replace_Previous_Permissions()
        {
            SessionContext.Current.BeginSession(2, "testuser", new[] { PermissionCodes.PatientsView });

            SessionContext.Current.BeginSession(2, "testuser", new[] { PermissionCodes.TestsEdit });

            SessionContext.Current.HasPermission(PermissionCodes.PatientsView).Should().BeFalse();
            SessionContext.Current.HasPermission(PermissionCodes.TestsEdit).Should().BeTrue();
        }
    }
}
