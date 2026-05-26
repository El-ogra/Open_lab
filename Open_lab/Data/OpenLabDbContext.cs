using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Open_lab.Models;

namespace Open_lab.Data
{
    public class OpenLabDbContext : DbContext
    {
        private const string MissingDatabasePasswordMessage = "Database password is not configured. Set the OPENLAB_DB_PASSWORD environment variable or provide a secure OPENLAB_CONNECTION value. Hardcoded fallback credentials are not supported.";

        public OpenLabDbContext()
        {
        }

        public OpenLabDbContext(DbContextOptions<OpenLabDbContext> options)
            : base(options)
        {
        }

        public int? CurrentUserId { get; set; }

        public DbSet<User> Users => Set<User>();
        public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();
        public DbSet<AttendanceBreak> AttendanceBreaks => Set<AttendanceBreak>();
        public DbSet<AttendanceDayStatus> AttendanceDayStatuses => Set<AttendanceDayStatus>();
        public DbSet<ShiftSchedule> ShiftSchedules => Set<ShiftSchedule>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Visit> Visits => Set<Visit>();
        public DbSet<Referral> Referrals => Set<Referral>();
        public DbSet<Test> Tests => Set<Test>();
        public DbSet<TestGroup> TestGroups => Set<TestGroup>();
        public DbSet<SampleType> SampleTypes => Set<SampleType>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<TestReferenceRange> TestReferenceRanges => Set<TestReferenceRange>();
        public DbSet<TestComment> TestComments => Set<TestComment>();
        public DbSet<VisitTest> VisitTests => Set<VisitTest>();
        public DbSet<TestParameter> TestParameters => Set<TestParameter>();
        public DbSet<ResultValue> ResultValues => Set<ResultValue>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ContractInvoice> ContractInvoices => Set<ContractInvoice>();
        public DbSet<PriceList> PriceLists => Set<PriceList>();
        public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
        public DbSet<CustomGroup> CustomGroups => Set<CustomGroup>();
        public DbSet<CustomGroupItem> CustomGroupItems => Set<CustomGroupItem>();
        public DbSet<Culture> Cultures => Set<Culture>();
        public DbSet<Antibiotic> Antibiotics => Set<Antibiotic>();
        public DbSet<CultureAntibiotic> CultureAntibiotics => Set<CultureAntibiotic>();
        public DbSet<SampleCollection> SampleCollections => Set<SampleCollection>();
        public DbSet<Setting> Settings => Set<Setting>();
        public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<AdditionalCharge> AdditionalCharges => Set<AdditionalCharge>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<DoctorCommission> DoctorCommissions => Set<DoctorCommission>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ExternalLabQueue> ExternalLabQueues => Set<ExternalLabQueue>();
        public DbSet<ShipmentManifest> ShipmentManifests => Set<ShipmentManifest>();
        public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
        public DbSet<ExternalLabSettlement> ExternalLabSettlements { get; set; } = null!;
        public DbSet<Reagent> Reagents { get; set; } = null!;
        public DbSet<TestConsumption> TestConsumptions { get; set; } = null!;
        public DbSet<Physician> Physicians { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            var envConnection = Environment.GetEnvironmentVariable("OPENLAB_CONNECTION");
            var configConnection = ConfigurationManager.ConnectionStrings["OpenLabDb"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(envConnection))
            {
                optionsBuilder.UseSqlServer(envConnection);
                optionsBuilder.AddInterceptors(new AuditInterceptor());
                return;
            }

            var dbPassword = Environment.GetEnvironmentVariable("OPENLAB_DB_PASSWORD");
            if (string.IsNullOrWhiteSpace(dbPassword))
            {
                throw new InvalidOperationException(MissingDatabasePasswordMessage);
            }

            var connectionString = !string.IsNullOrWhiteSpace(configConnection)
                ? configConnection
                : "Server=.\\SQLEXPRESS;Database=OpenLab;User ID=sa;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False;Connect Timeout=30";
            var connectionBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                Password = dbPassword
            };

            connectionString = connectionBuilder.ConnectionString;
            optionsBuilder.UseSqlServer(connectionString);
            
            // Add audit interceptor for automatic logging
            optionsBuilder.AddInterceptors(new AuditInterceptor());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Salt).IsRequired();
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.HasIndex(e => e.RoleName).IsUnique();
                entity.Property(e => e.RoleName).IsRequired();
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionCode });
                entity.Property(e => e.PermissionCode).IsRequired();
                entity.HasOne(e => e.Role)
                    .WithMany(e => e.RolePermissions)
                    .HasForeignKey(e => e.RoleId);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserRoles)
                    .HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Role)
                    .WithMany(e => e.UserRoles)
                    .HasForeignKey(e => e.RoleId);
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.PatientId);
                entity.HasIndex(e => e.LabId).IsUnique();
                entity.HasIndex(e => e.NationalId);
                entity.Property(e => e.LabId).IsRequired();
                entity.Property(e => e.FullName).IsRequired();
                entity.Property(e => e.Gender).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.HomePhone).HasMaxLength(50);
                entity.Property(e => e.NationalId).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.AgeUnit).HasMaxLength(10);
                entity.HasOne(e => e.Referral)
                    .WithMany(e => e.Patients)
                    .HasForeignKey(e => e.ReferralId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Visit>(entity =>
            {
                entity.HasKey(e => e.VisitId);
                entity.HasOne(e => e.Patient)
                    .WithMany(e => e.Visits)
                    .HasForeignKey(e => e.PatientId);
                entity.HasOne(e => e.Referral)
                    .WithMany(e => e.Visits)
                    .HasForeignKey(e => e.ReferralId);
                entity.HasOne(e => e.Branch)
                    .WithMany(e => e.Visits)
                    .HasForeignKey(e => e.BranchId);
            });

            modelBuilder.Entity<Referral>(entity =>
            {
                entity.HasKey(e => e.ReferralId);
                entity.Property(e => e.ReferralType).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.DiscountPercentage).HasPrecision(18, 2);
                entity.Property(e => e.CommissionPercentage).HasPrecision(18, 2);
            });

            modelBuilder.Entity<ContractInvoice>(entity =>
            {
                entity.HasKey(e => e.ContractInvoiceId);
                entity.HasOne(e => e.Referral)
                    .WithMany(e => e.ContractInvoices)
                    .HasForeignKey(e => e.ReferralId);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.NetAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasKey(e => e.TestId);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code).IsRequired();
                entity.Property(e => e.NameReport).IsRequired();
                entity.Property(e => e.NameReceipt).IsRequired();
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.CostPrice).HasPrecision(18, 2);
                entity.Property(e => e.PatientPrice).HasPrecision(18, 2);
                entity.HasOne(e => e.Group)
                    .WithMany(e => e.Tests)
                    .HasForeignKey(e => e.GroupId);
                entity.HasOne(e => e.SampleType)
                    .WithMany(e => e.Tests)
                    .HasForeignKey(e => e.SampleTypeId);
                entity.HasOne(e => e.Unit)
                    .WithMany(e => e.Tests)
                    .HasForeignKey(e => e.UnitId);
            });

            modelBuilder.Entity<TestGroup>(entity =>
            {
                entity.HasKey(e => e.GroupId);
                entity.Property(e => e.GroupName).IsRequired();
            });

            modelBuilder.Entity<SampleType>(entity =>
            {
                entity.HasKey(e => e.SampleTypeId);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.HasKey(e => e.UnitId);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<TestReferenceRange>(entity =>
            {
                entity.HasKey(e => e.RangeId);
                entity.Property(e => e.LowValue).HasPrecision(18, 2);
                entity.Property(e => e.HighValue).HasPrecision(18, 2);
                entity.Property(e => e.AgeFromUnit).HasMaxLength(10);
                entity.Property(e => e.AgeToUnit).HasMaxLength(10);
                entity.HasOne(e => e.Test)
                    .WithMany(e => e.ReferenceRanges)
                    .HasForeignKey(e => e.TestId);
                entity.HasOne(e => e.Parameter)
                    .WithMany()
                    .HasForeignKey(e => e.ParameterId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.ParameterId);
                entity.HasIndex(e => e.AgeFromDays);
                entity.HasIndex(e => e.AgeToDays);
            });

            modelBuilder.Entity<TestComment>(entity =>
            {
                entity.HasKey(e => e.CommentId);
                entity.Property(e => e.CommentText).IsRequired();
                entity.HasOne(e => e.Test)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.TestId);
                entity.HasOne(e => e.Parameter)
                    .WithMany()
                    .HasForeignKey(e => e.ParameterId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.ParameterId);
            });

            modelBuilder.Entity<VisitTest>(entity =>
            {
                entity.HasKey(e => e.VisitTestId);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.HasOne(e => e.Visit)
                    .WithMany(e => e.VisitTests)
                    .HasForeignKey(e => e.VisitId);
                entity.HasOne(e => e.Test)
                    .WithMany(e => e.VisitTests)
                    .HasForeignKey(e => e.TestId);
            });

            modelBuilder.Entity<TestParameter>(entity =>
            {
                entity.HasKey(e => e.ParameterId);
                entity.Property(e => e.Name).IsRequired();
                entity.HasOne(e => e.Test)
                    .WithMany(e => e.Parameters)
                    .HasForeignKey(e => e.TestId);
                entity.HasOne(e => e.Unit)
                    .WithMany(e => e.TestParameters)
                    .HasForeignKey(e => e.UnitId);
            });

            modelBuilder.Entity<ResultValue>(entity =>
            {
                entity.HasKey(e => e.ResultValueId);
                entity.HasOne(e => e.VisitTest)
                    .WithMany(e => e.ResultValues)
                    .HasForeignKey(e => e.VisitTestId);
                entity.HasOne(e => e.Parameter)
                    .WithMany(e => e.ResultValues)
                    .HasForeignKey(e => e.ParameterId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.VerifiedByUser)
                    .WithMany(e => e.VerifiedResults)
                    .HasForeignKey(e => e.VerifiedBy);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);
                entity.Property(e => e.Total).HasPrecision(18, 2);
                entity.Property(e => e.Discount).HasPrecision(18, 2);
                entity.Property(e => e.NetTotal).HasPrecision(18, 2);
                entity.Property(e => e.Paid).HasPrecision(18, 2);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.HasOne(e => e.Visit)
                    .WithOne(e => e.Invoice)
                    .HasForeignKey<Invoice>(e => e.VisitId);
                entity.HasIndex(e => e.VisitId).IsUnique();
                entity.HasOne(e => e.Branch)
                    .WithMany(e => e.Invoices)
                    .HasForeignKey(e => e.BranchId);
                entity.HasOne(e => e.ContractInvoice)
                    .WithMany(e => e.Invoices)
                    .HasForeignKey(e => e.ContractInvoiceId);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Invoice)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.InvoiceId);
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Branch)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.BranchId);
            });

            modelBuilder.Entity<PriceList>(entity =>
            {
                entity.HasKey(e => e.PriceListId);
                entity.Property(e => e.Name).IsRequired();
                entity.HasOne(e => e.Referral)
                    .WithMany(e => e.PriceLists)
                    .HasForeignKey(e => e.ReferralId);
            });

            modelBuilder.Entity<PriceListItem>(entity =>
            {
                entity.HasKey(e => e.PriceListItemId);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.HasOne(e => e.PriceList)
                    .WithMany(e => e.Items)
                    .HasForeignKey(e => e.PriceListId);
                entity.HasOne(e => e.Test)
                    .WithMany(e => e.PriceListItems)
                    .HasForeignKey(e => e.TestId);
            });

            modelBuilder.Entity<CustomGroup>(entity =>
            {
                entity.HasKey(e => e.CustomGroupId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });

            modelBuilder.Entity<CustomGroupItem>(entity =>
            {
                entity.HasKey(e => e.CustomGroupItemId);
                entity.HasOne(e => e.CustomGroup)
                    .WithMany(e => e.Items)
                    .HasForeignKey(e => e.CustomGroupId);
                entity.HasOne(e => e.Test)
                    .WithMany(e => e.CustomGroupItems)
                    .HasForeignKey(e => e.TestId);
            });

            modelBuilder.Entity<Culture>(entity =>
            {
                entity.HasKey(e => e.CultureId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.SampleType).IsRequired();
                entity.Property(e => e.IsolatedOrganism).IsRequired();
                entity.Property(e => e.GrowthConditions).IsRequired();
            });

            modelBuilder.Entity<Antibiotic>(entity =>
            {
                entity.HasKey(e => e.AntibioticId);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<CultureAntibiotic>(entity =>
            {
                entity.HasKey(e => new { e.CultureId, e.AntibioticId });
                entity.HasOne(e => e.Culture)
                    .WithMany(e => e.CultureAntibiotics)
                    .HasForeignKey(e => e.CultureId);
                entity.HasOne(e => e.Antibiotic)
                    .WithMany(e => e.CultureAntibiotics)
                    .HasForeignKey(e => e.AntibioticId);
            });

            modelBuilder.Entity<SampleCollection>(entity =>
            {
                entity.HasKey(e => e.SampleId);
                entity.HasOne(e => e.VisitTest)
                    .WithOne(e => e.SampleCollection)
                    .HasForeignKey<SampleCollection>(e => e.VisitTestId);
                entity.HasIndex(e => e.VisitTestId).IsUnique();
                entity.HasOne(e => e.CollectedByUser)
                    .WithMany(e => e.SampleCollections)
                    .HasForeignKey(e => e.CollectedBy);
                entity.HasOne(e => e.ReceivedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ReceivedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AttendanceLog>(entity =>
            {
                entity.HasKey(e => e.AttendanceLogId);
                entity.HasOne(e => e.User)
                    .WithMany(e => e.AttendanceLogs)
                    .HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Shift)
                    .WithMany(e => e.AttendanceLogs)
                    .HasForeignKey(e => e.ShiftId);
            });

            modelBuilder.Entity<AttendanceBreak>(entity =>
            {
                entity.HasKey(e => e.BreakId);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(30);
                entity.HasOne(e => e.AttendanceLog)
                    .WithMany(e => e.Breaks)
                    .HasForeignKey(e => e.AttendanceLogId);
                entity.HasIndex(e => new { e.AttendanceLogId, e.StartAt });
            });

            modelBuilder.Entity<AttendanceDayStatus>(entity =>
            {
                entity.HasKey(e => e.DayStatusId);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
                entity.HasOne(e => e.User)
                    .WithMany(e => e.AttendanceDayStatuses)
                    .HasForeignKey(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique();
            });

            modelBuilder.Entity<ShiftSchedule>(entity =>
            {
                entity.HasKey(e => e.ShiftId);
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(e => e.Key);
            });

            modelBuilder.Entity<MedicalHistory>(entity =>
            {
                entity.HasKey(e => e.MedicalHistoryId);
                entity.HasOne(e => e.Patient)
                    .WithOne(e => e.MedicalHistory)
                    .HasForeignKey<MedicalHistory>(e => e.PatientId);
                entity.HasIndex(e => e.PatientId).IsUnique();
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditLogId);
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.TableName).IsRequired();
                entity.HasOne(e => e.User)
                    .WithMany(e => e.AuditLogs)
                    .HasForeignKey(e => e.UserId);
            });

            modelBuilder.Entity<AdditionalCharge>(entity =>
            {
                entity.HasKey(e => e.AdditionalChargeId);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.HasOne(e => e.Invoice)
                    .WithMany(e => e.AdditionalCharges)
                    .HasForeignKey(e => e.InvoiceId);
            });

            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(e => e.BranchId);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<DoctorCommission>(entity =>
            {
                entity.HasKey(e => e.CommissionId);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.HasOne(e => e.Referral)
                    .WithMany()
                    .HasForeignKey(e => e.ReferralId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Visit)
                    .WithMany()
                    .HasForeignKey(e => e.VisitId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.ExpenseId);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId);
            });

            modelBuilder.Entity<ExternalLabQueue>(entity =>
            {
                entity.HasKey(e => e.QueueId);
                entity.HasOne(e => e.VisitTest)
                    .WithOne(e => e.ExternalQueueItem)
                    .HasForeignKey<ExternalLabQueue>(e => e.VisitTestId);
                entity.HasOne(e => e.Referral)
                    .WithMany()
                    .HasForeignKey(e => e.ReferralId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ShipmentManifest>(entity =>
            {
                entity.HasKey(e => e.ManifestId);
                entity.Property(e => e.ManifestNumber).IsRequired();
                entity.HasOne(e => e.Referral)
                    .WithMany()
                    .HasForeignKey(e => e.ReferralId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ShipmentItem>(entity =>
            {
                entity.HasKey(e => e.ShipmentItemId);
                entity.HasOne(e => e.Manifest)
                    .WithMany(e => e.Items)
                    .HasForeignKey(e => e.ManifestId);
                entity.HasOne(e => e.QueueItem)
                    .WithOne(e => e.ShipmentItem)
                    .HasForeignKey<ShipmentItem>(e => e.QueueId);
            });

            modelBuilder.Entity<ExternalLabSettlement>(entity =>
            {
                entity.HasKey(e => e.SettlementId);
                entity.Property(e => e.TotalCost).HasPrecision(18, 2);
                entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.HasOne(e => e.Referral)
                    .WithMany(e => e.Settlements)
                    .HasForeignKey(e => e.ReferralId);
            });

            modelBuilder.Entity<Reagent>(entity =>
            {
                entity.HasKey(e => e.ReagentId);
                entity.Property(e => e.CurrentStock).HasPrecision(18, 2);
            });

            modelBuilder.Entity<TestConsumption>(entity =>
            {
                entity.HasKey(e => e.ConsumptionId);
                entity.Property(e => e.AmountPerTest).HasPrecision(18, 4);
                entity.HasOne(e => e.Test)
                    .WithMany()
                    .HasForeignKey(e => e.TestId);
                entity.HasOne(e => e.Reagent)
                    .WithMany(e => e.Consumptions)
                    .HasForeignKey(e => e.ReagentId);
            });

            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(e => e.SettingId);
                entity.HasIndex(e => e.SettingKey).IsUnique();
                entity.Property(e => e.SettingKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SettingType).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
            });
        }
    }
}







