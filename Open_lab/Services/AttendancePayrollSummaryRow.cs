using System;

namespace Open_lab.Services
{
    public class AttendancePayrollSummaryRow
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int GrossMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int NetMinutes { get; set; }

        public int DelayMinutes { get; set; }
        public int OvertimeMinutes { get; set; }

        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int VacationDays { get; set; }
        public int HolidayDays { get; set; }
        public int SickLeaveDays { get; set; }
        public int OtherStatusDays { get; set; }
    }
}

