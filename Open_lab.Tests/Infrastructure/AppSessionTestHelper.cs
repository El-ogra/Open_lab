namespace Open_lab.Tests.Infrastructure
{
    public static class AppSessionTestHelper
    {
        public static void ResetToAdmin()
        {
            SessionContext.Current.BeginSession(1, "admin", new[] { PermissionCodes.FullAccess });
        }

        public static void ResetToUserWithPermissions(params string[] permissions)
        {
            SessionContext.Current.BeginSession(2, "testuser", permissions);
        }

        public static void Reset()
        {
            SessionContext.Current.EndSession();
        }
    }
}
