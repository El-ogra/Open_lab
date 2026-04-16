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

    public class TreasuryByUserRow
    {
        public string Username { get; set; } = string.Empty;
        public int PaymentsCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TreasuryByReferralRow
    {
        public string ReferralName { get; set; } = string.Empty;
        public int VisitsCount { get; set; }
        public decimal TotalInvoiced { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalBalance { get; set; }
    }

    public class StatisticsGenderRow
    {
        public string Gender { get; set; } = string.Empty;
        public int VisitsCount { get; set; }
        public int TestsCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class StatisticsReferralRow
    {
        public string ReferralName { get; set; } = string.Empty;
        public int VisitsCount { get; set; }
        public int TestsCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Paid { get; set; }
    }

    public class InvoicePaymentRow
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int UserId { get; set; }
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

    public class CultureVisitTestRow
    {
        public int VisitTestId { get; set; }
        public int VisitId { get; set; }
        public string LabId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string? Status { get; set; }
    }

    public class CultureSensitivityRow : BaseViewModel
    {
        private string _sensitivity = string.Empty;
        private string? _comment;

        public int AntibioticId { get; set; }
        public string AntibioticName { get; set; } = string.Empty;

        public string Sensitivity
        {
            get => _sensitivity;
            set => SetProperty(ref _sensitivity, value);
        }

        public string? Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }
    }
}
