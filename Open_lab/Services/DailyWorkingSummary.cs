using System;

namespace Open_lab.Services
{
    public class DailyWorkingSummary
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; } // date-only semantics
        public bool HasOpenLog { get; set; }

        public int GrossMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int NetMinutes { get; set; }

        public DateTime? FirstLoginAt { get; set; }
        public DateTime? LastLogoutAt { get; set; }
    }
}

