using System;

namespace Open_lab.ViewModels
{
    public class SelectedTestItem
    {
        public int VisitTestId { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class VisitTestRow
    {
        public int VisitTestId { get; set; }
        public int TestId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string? Status { get; set; }
    }

    public class ResultEntryItem
    {
        public int ParameterId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Flag { get; set; }
        public string? Comment { get; set; }
    }

    public class WorkSheetPatientRow
    {
        public int VisitId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public int TestsCount { get; set; }
    }

    public class WorkSheetTestRow
    {
        public string TestName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class AttendanceLogRow
    {
        public int AttendanceLogId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public DateTime LoginAt { get; set; }
        public DateTime? LogoutAt { get; set; }
        public string Duration { get; set; } = "-";
    }

    public class AccountsPaymentRow
    {
        public int PaymentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? ReferralName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Username { get; set; }
    }

    public class SampleCollectionRow
    {
        public int VisitTestId { get; set; }
        public int TestId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string Status { get; set; } = "غير مسحوبة";
        public DateTime? CollectedAt { get; set; }
        public string? CollectedBy { get; set; }
    }
}

