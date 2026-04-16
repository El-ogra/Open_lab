namespace Open_lab.ViewModels
{
    public static class AppSession
    {
        public static int UserId { get; set; }
        public static string Username { get; set; } = string.Empty;
        public static bool IsAdmin { get; set; }
        public static int AttendanceLogId { get; set; }
    }
}

