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

    public class VisitSummary : BaseViewModel
    {
        private string _patientQuickSearch = string.Empty;

        public int VisitId { get; set; }
        public int PatientId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string ReferralSource { get; set; } = string.Empty;
        public string LabId { get; set; } = string.Empty;
        public string StatusMarker { get; set; } = string.Empty;
        public string? PatientPhoto { get; set; }
        public DateTime VisitDate { get; set; }
        public string DisplayText => $"{VisitId} - {VisitDate:yyyy-MM-dd}";
    }

    public class VisitTestRow : BaseViewModel
    {
        private string? _resultValue;
        private string? _status;
        private bool _isFinished;
        private bool _isVerified;
        private bool _shouldPrint;
        private bool _shouldExport;

        public int VisitTestId { get; set; }
        public int PatientId { get; set; }
        public int TestId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientGender { get; set; } = "Male";
        public int PatientAge { get; set; }
        public string TestName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }

        public string? ResultValue
        {
            get => _resultValue;
            set => SetProperty(ref _resultValue, value);
        }

        public string? Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsFinished
        {
            get => _isFinished;
            set => SetProperty(ref _isFinished, value);
        }

        public bool IsVerified
        {
            get => _isVerified;
            set => SetProperty(ref _isVerified, value);
        }

        public bool ShouldPrint
        {
            get => _shouldPrint;
            set => SetProperty(ref _shouldPrint, value);
        }

        public bool ShouldExport
        {
            get => _shouldExport;
            set => SetProperty(ref _shouldExport, value);
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }
    }

    public class ResultEntryItem : BaseViewModel
    {
        private string? _value;
        private string? _flag;
        private string? _comment;

        public int ParameterId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        
        public Action<ResultEntryItem>? OnValueChanged { get; set; }

        public string? Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    OnValueChanged?.Invoke(this);
                }
            }
        }

        public string? Flag
        {
            get => _flag;
            set => SetProperty(ref _flag, value);
        }

        public string? Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }
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

    public class TreasuryByBranchRow
    {
        public string BranchName { get; set; } = string.Empty;
        public int VisitsCount { get; set; }
        public decimal TotalInvoiced { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalBalance { get; set; }
    }

    public class TreasuryByDoctorRow
    {
        public string DoctorName { get; set; } = string.Empty;
        public int VisitsCount { get; set; }
        public decimal TotalInvoiced { get; set; }
        public decimal CommissionAmount { get; set; }
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
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public int UserId { get; set; }
    }

    public class AdditionalChargeRow
    {
        public int AdditionalChargeId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
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
    public class DeliveryVisitRow
    {
        public int VisitId { get; set; }
        public string LabId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string? VisitStatus { get; set; }
        public int TestsCount { get; set; }
        public int VerifiedCount { get; set; }
        public int DeliveredCount { get; set; }
        public decimal Balance { get; set; }
        public bool IsReadyForDelivery { get; set; }
        public bool IsDelivered { get; set; }
    }

    public class ReferralItem
    {
        public int ReferralId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
