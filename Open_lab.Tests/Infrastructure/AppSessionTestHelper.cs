using Open_lab.ViewModels;

namespace Open_lab.Tests.Infrastructure
{
    public static class AppSessionTestHelper
    {
        public static void ResetToAdmin()
        {
            AppSession.Clear();
            AppSession.IsAdmin = true;
            AppSession.UserId = 1;
            AppSession.Username = "admin";
        }

        public static void ResetToUserWithPermissions(params string[] permissions)
        {
            AppSession.Clear();
            AppSession.IsAdmin = false;
            AppSession.UserId = 2;
            AppSession.Username = "testuser";
            AppSession.SetPermissions(permissions);
        }

        public static void Reset()
        {
            AppSession.Clear();
        }
    }
}
