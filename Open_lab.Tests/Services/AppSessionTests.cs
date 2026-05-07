using FluentAssertions;
using Open_lab.Services;
using Open_lab.ViewModels;

namespace Open_lab.Tests.Services
{
    public class AppSessionTests : IDisposable
    {
        public AppSessionTests()
        {
            AppSession.Clear();
        }

        public void Dispose()
        {
            AppSession.Clear();
        }

        [Fact]
        public void HasPermission_When_IsAdmin_True_Should_Always_Return_True()
        {
            // Function: 10.2 — Set Permissions
            // Arrange
            AppSession.IsAdmin = true;
            AppSession.SetPermissions(Array.Empty<string>());

            // Act & Assert
            // Assert
            AppSession.HasPermission("Any.Random.Permission").Should().BeTrue();
            AppSession.HasPermission(PermissionCodes.PatientsEdit).Should().BeTrue();
        }

        [Fact]
        public void HasPermission_When_Not_Admin_But_Has_FullAccess_Should_Return_True()
        {
            // Function: 10.2 — Set Permissions
            // Arrange
            AppSession.IsAdmin = false;
            AppSession.SetPermissions(new[] { PermissionCodes.FullAccess });

            // Act & Assert
            // Assert
            AppSession.HasPermission(PermissionCodes.AccountsEdit).Should().BeTrue();
            AppSession.HasPermission("Unknown").Should().BeTrue();
        }

        [Fact]
        public void HasPermission_When_Not_Admin_And_Has_Specific_Permission_Should_Return_True_Only_For_That_Permission()
        {
            // Function: 10.2 — Set Permissions
            // Arrange
            AppSession.IsAdmin = false;
            AppSession.SetPermissions(new[] { PermissionCodes.PatientsView, PermissionCodes.TestsEdit });

            // Act & Assert
            // Assert
            AppSession.HasPermission(PermissionCodes.PatientsView).Should().BeTrue();
            AppSession.HasPermission(PermissionCodes.TestsEdit).Should().BeTrue();
            AppSession.HasPermission(PermissionCodes.PatientsEdit).Should().BeFalse();
            AppSession.HasPermission(PermissionCodes.AccountsView).Should().BeFalse();
        }

        [Fact]
        public void HasPermission_When_No_Permissions_Should_Return_False()
        {
            // Function: 10.2 — Set Permissions
            // Arrange
            AppSession.IsAdmin = false;
            AppSession.SetPermissions(Array.Empty<string>());

            // Act & Assert
            // Assert
            AppSession.HasPermission(PermissionCodes.PatientsView).Should().BeFalse();
            AppSession.HasPermission(PermissionCodes.TestsEdit).Should().BeFalse();
            AppSession.HasPermission("Any.Random.Permission").Should().BeFalse();
        }

        [Fact]
        public void Clear_Should_Reset_All_Properties()
        {
            // Function: 10.2 — Set Permissions
            // Arrange
            AppSession.UserId = 42;
            AppSession.Username = "testuser";
            AppSession.IsAdmin = true;
            AppSession.AttendanceLogId = 99;
            AppSession.SetPermissions(new[] { PermissionCodes.FullAccess });

            // Act
            AppSession.Clear();

            // Assert
            AppSession.UserId.Should().Be(0);
            AppSession.Username.Should().BeEmpty();
            AppSession.IsAdmin.Should().BeFalse();
            AppSession.AttendanceLogId.Should().Be(0);
            AppSession.HasPermission(PermissionCodes.FullAccess).Should().BeFalse();
        }

        [Fact]
        public void SetPermissions_Should_Replace_Previous_Permissions()
        {
            // Function: 10.2 — Set Permissions
            // Arrange
            AppSession.IsAdmin = false;
            AppSession.SetPermissions(new[] { PermissionCodes.PatientsView });

            // Act
            AppSession.SetPermissions(new[] { PermissionCodes.TestsEdit });

            // Assert
            AppSession.HasPermission(PermissionCodes.PatientsView).Should().BeFalse();
            AppSession.HasPermission(PermissionCodes.TestsEdit).Should().BeTrue();
        }
    }
}
