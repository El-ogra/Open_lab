using System;
using System.Collections.Generic;

namespace Open_lab.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public bool IsActive { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
        public ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();
        public ICollection<ResultValue> VerifiedResults { get; set; } = new HashSet<ResultValue>();
        public ICollection<SampleCollection> SampleCollections { get; set; } = new HashSet<SampleCollection>();
        public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new HashSet<AttendanceLog>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new HashSet<AuditLog>();
    }

    public class AttendanceLog
    {
        public int AttendanceLogId { get; set; }
        public int UserId { get; set; }
        public DateTime LoginAt { get; set; }
        public DateTime? LogoutAt { get; set; }
        public string? Note { get; set; }

        public User User { get; set; } = null!;
    }

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public ICollection<RolePermission> RolePermissions { get; set; } = new HashSet<RolePermission>();
        public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    }

    public class RolePermission
    {
        public int RoleId { get; set; }
        public string PermissionCode { get; set; } = string.Empty;

        public Role Role { get; set; } = null!;
    }
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }

    public class Patient
    {
        public int PatientId { get; set; }
        public string LabId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public int? Age { get; set; }
        public bool IsPregnant { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public MedicalHistory? MedicalHistory { get; set; }
        public ICollection<Visit> Visits { get; set; } = new HashSet<Visit>();
    }

    public class Visit
    {
        public int VisitId { get; set; }
        public int PatientId { get; set; }
        public DateTime VisitDate { get; set; }
        public string? AccountType { get; set; }
        public int? ReferralId { get; set; }
        public string? Status { get; set; }
        public int? BranchId { get; set; }

        public Patient Patient { get; set; } = null!;
        public Referral? Referral { get; set; }
        public Branch? Branch { get; set; }
        public ICollection<VisitTest> VisitTests { get; set; } = new HashSet<VisitTest>();
        public Invoice? Invoice { get; set; }
    }

    public class Referral
    {
        public int ReferralId { get; set; }
        public string ReferralType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? City { get; set; }

        public ICollection<Visit> Visits { get; set; } = new HashSet<Visit>();
        public ICollection<PriceList> PriceLists { get; set; } = new HashSet<PriceList>();
        public ICollection<ExternalLabSettlement> Settlements { get; set; } = new HashSet<ExternalLabSettlement>();
    }

    public class Test
    {
        public int TestId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameReport { get; set; } = string.Empty;
        public string NameReceipt { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public int? SampleTypeId { get; set; }
        public int? UnitId { get; set; }
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? PatientPrice { get; set; }
        public int TurnaroundHours { get; set; }
        public bool IsRoutine { get; set; }
        public bool IsSendOut { get; set; }

        public TestGroup? Group { get; set; }
        public SampleType? SampleType { get; set; }
        public Unit? Unit { get; set; }
        public ICollection<TestReferenceRange> ReferenceRanges { get; set; } = new HashSet<TestReferenceRange>();
        public ICollection<TestComment> Comments { get; set; } = new HashSet<TestComment>();
        public ICollection<TestParameter> Parameters { get; set; } = new HashSet<TestParameter>();
        public ICollection<VisitTest> VisitTests { get; set; } = new HashSet<VisitTest>();
        public ICollection<PriceListItem> PriceListItems { get; set; } = new HashSet<PriceListItem>();
        public ICollection<CustomGroupItem> CustomGroupItems { get; set; } = new HashSet<CustomGroupItem>();
    }

    public class TestGroup
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;

        public ICollection<Test> Tests { get; set; } = new HashSet<Test>();
    }

    public class SampleType
    {
        public int SampleTypeId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Test> Tests { get; set; } = new HashSet<Test>();
    }

    public class Unit
    {
        public int UnitId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Test> Tests { get; set; } = new HashSet<Test>();
        public ICollection<TestParameter> TestParameters { get; set; } = new HashSet<TestParameter>();
    }

    public class TestReferenceRange
    {
        public int RangeId { get; set; }
        public int TestId { get; set; }
        public string? Gender { get; set; }
        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }
        public decimal? LowValue { get; set; }
        public decimal? HighValue { get; set; }
        public string? NormalText { get; set; }

        public Test Test { get; set; } = null!;
    }

    public class TestComment
    {
        public int CommentId { get; set; }
        public int TestId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public bool IsDefault { get; set; }

        public Test Test { get; set; } = null!;
    }

    public class VisitTest
    {
        public int VisitTestId { get; set; }
        public int VisitId { get; set; }
        public int TestId { get; set; }
        public decimal Price { get; set; }
        public string? Status { get; set; }

        public Visit Visit { get; set; } = null!;
        public Test Test { get; set; } = null!;
        public ICollection<ResultValue> ResultValues { get; set; } = new HashSet<ResultValue>();
        public SampleCollection? SampleCollection { get; set; }
        public ExternalLabQueue? ExternalQueueItem { get; set; }
    }

    public class TestParameter
    {
        public int ParameterId { get; set; }
        public int TestId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? UnitId { get; set; }
        public int OrderNo { get; set; }

        public Test Test { get; set; } = null!;
        public Unit? Unit { get; set; }
        public ICollection<ResultValue> ResultValues { get; set; } = new HashSet<ResultValue>();
    }

    public class ResultValue
    {
        public int ResultValueId { get; set; }
        public int VisitTestId { get; set; }
        public int ParameterId { get; set; }
        public string? Value { get; set; }
        public string? Flag { get; set; }
        public string? Comment { get; set; }
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }

        public VisitTest VisitTest { get; set; } = null!;
        public TestParameter Parameter { get; set; } = null!;
        public User? VerifiedByUser { get; set; }
    }

    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int VisitId { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
        public string? Status { get; set; }
        public int? BranchId { get; set; }

        public Visit Visit { get; set; } = null!;
        public Branch? Branch { get; set; }
        public ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();
        public ICollection<AdditionalCharge> AdditionalCharges { get; set; } = new HashSet<AdditionalCharge>();
    }

    public class Payment
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int UserId { get; set; }
        public int? BranchId { get; set; }

        public Invoice Invoice { get; set; } = null!;
        public User User { get; set; } = null!;
        public Branch? Branch { get; set; }
    }

    public class PriceList
    {
        public int PriceListId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ReferralId { get; set; }
        public bool IsDefault { get; set; }

        public Referral? Referral { get; set; }
        public ICollection<PriceListItem> Items { get; set; } = new HashSet<PriceListItem>();
    }

    public class PriceListItem
    {
        public int PriceListItemId { get; set; }
        public int PriceListId { get; set; }
        public int TestId { get; set; }
        public decimal Price { get; set; }

        public PriceList PriceList { get; set; } = null!;
        public Test Test { get; set; } = null!;
    }

    public class CustomGroup
    {
        public int CustomGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public ICollection<CustomGroupItem> Items { get; set; } = new HashSet<CustomGroupItem>();
    }

    public class CustomGroupItem
    {
        public int CustomGroupItemId { get; set; }
        public int CustomGroupId { get; set; }
        public int TestId { get; set; }

        public CustomGroup CustomGroup { get; set; } = null!;
        public Test Test { get; set; } = null!;
    }

    public class Culture
    {
        public int CultureId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<CultureAntibiotic> CultureAntibiotics { get; set; } = new HashSet<CultureAntibiotic>();
    }

    public class Antibiotic
    {
        public int AntibioticId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsSafeForPregnancy { get; set; } = true;
        public bool IsSafeForChildren { get; set; } = true;

        public ICollection<CultureAntibiotic> CultureAntibiotics { get; set; } = new HashSet<CultureAntibiotic>();
    }

    public class CultureAntibiotic
    {
        public int CultureId { get; set; }
        public int AntibioticId { get; set; }

        public Culture Culture { get; set; } = null!;
        public Antibiotic Antibiotic { get; set; } = null!;
    }

    public class SampleCollection
    {
        public int SampleId { get; set; }
        public int VisitTestId { get; set; }
        public int CollectedBy { get; set; }
        public DateTime CollectedAt { get; set; }
        public string? Status { get; set; }

        public VisitTest VisitTest { get; set; } = null!;
        public User CollectedByUser { get; set; } = null!;
    }

    public class Setting
    {
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
    }

    public class MedicalHistory
    {
        public int MedicalHistoryId { get; set; }
        public int PatientId { get; set; }
        public string? ChronicDiseases { get; set; }
        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? Notes { get; set; }

        public Patient Patient { get; set; } = null!;
    }

    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string? RecordId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }

        public User User { get; set; } = null!;
    }

    public class AdditionalCharge
    {
        public int AdditionalChargeId { get; set; }
        public int InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        public Invoice Invoice { get; set; } = null!;
    }

    public class Branch
    {
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }

        public ICollection<Visit> Visits { get; set; } = new HashSet<Visit>();
        public ICollection<Invoice> Invoices { get; set; } = new HashSet<Invoice>();
        public ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();
    }

    public class DoctorCommission
    {
        public int CommissionId { get; set; }
        public int ReferralId { get; set; }
        public int VisitId { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime DateCalculated { get; set; }

        public Referral Referral { get; set; } = null!;
        public Visit Visit { get; set; } = null!;
    }

    public class Expense
    {
        public int ExpenseId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }

    public class ExternalLabQueue
    {
        public int QueueId { get; set; }
        public int VisitTestId { get; set; }
        public int? ReferralId { get; set; } // The target external lab
        public string Status { get; set; } = "Pending"; // Pending, InManifest, Shipped, Received
        public DateTime DateQueued { get; set; }
        public string? ExternalReference { get; set; }

        public VisitTest VisitTest { get; set; } = null!;
        public Referral? Referral { get; set; }
        public ShipmentItem? ShipmentItem { get; set; }
    }

    public class ShipmentManifest
    {
        public int ManifestId { get; set; }
        public string ManifestNumber { get; set; } = string.Empty;
        public int ReferralId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateShipped { get; set; }
        public string Status { get; set; } = "Open"; // Open, Shipped, Cancelled
        public string? CourierNotes { get; set; }

        public Referral Referral { get; set; } = null!;
        public ICollection<ShipmentItem> Items { get; set; } = new HashSet<ShipmentItem>();
    }

    public class ShipmentItem
    {
        public int ShipmentItemId { get; set; }
        public int ManifestId { get; set; }
        public int QueueId { get; set; }

        public ShipmentManifest Manifest { get; set; } = null!;
        public ExternalLabQueue QueueItem { get; set; } = null!;
    }

    public class ExternalLabSettlement
    {
        public int SettlementId { get; set; }
        public int ReferralId { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public DateTime SettlementDate { get; set; }
        public string? Note { get; set; }

        public Referral Referral { get; set; } = null!;
    }
}
