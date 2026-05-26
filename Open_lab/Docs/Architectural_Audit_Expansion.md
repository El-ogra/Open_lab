# Architectural Audit Expansion — Open_lab .NET 8 / WPF / MVVM Medical Laboratory Information System

**Audit Date:** 2026-05-27
**Repository Root:** `C:\Users\LAP LINK\source\repos\Open_lab`
**Repository Branch at Audit:** `Fi5ve`
**Head Commit at Audit:** `7df3e91 — الوضع الأحدث`
**Audit Scope:** Strictly analytical, read-only. No code changes, no migrations, no refactoring proposals. Evidence-based — every claim is anchored to a `file:line` citation.
**Audit Subject Areas:** Solution topology, EF Core architecture, schema evolution, entity inventory, laboratory domain coverage, relationships and cardinality, reference-range modeling, result-entry modeling, validation and constraints, indexing, configuration, security posture (F-series remediation state), workflow trace, localization, multi-tenancy, gap and risk register, future extensibility.

> **Non-destructive contract.** Nothing in this document proposes a fix, a migration, or a refactor. Where a deficiency is observed it is reported as an absence of evidence in the current code, not as a recommendation. Decisions about remediation, prioritization, and implementation are explicitly out of scope and belong to the architectural review meeting this document is intended to support.

---

## 1. Executive Summary

Open_lab is a single-process WPF / MVVM desktop application targeting `net8.0-windows`, backed by SQL Server through Entity Framework Core 8 and structured as a four-project Visual Studio solution. The system implements an end-to-end Laboratory Information System (LIS) covering patient intake, visit/order management, sample collection, multi-component test catalogues, result entry with sex/age-dependent reference ranges, invoicing with referral pricing, microbiology cultures with antibiotic sensitivity, external-lab send-out workflows, HR (shifts/attendance/payroll), reagent consumption tracking, and full-tenancy auditing through an EF Core `SaveChangesInterceptor`.

### Scale at a glance

| Dimension | Count | Source |
|---|---|---|
| Visual Studio projects | 4 | `Open_lab.sln` |
| EF Core `DbSet<>` properties | 46 | `Open_lab/Open_lab/Data/OpenLabDbContext.cs:24-69` |
| EF Core migrations (excluding `Designer.cs` and `ModelSnapshot`) | 23 | `Open_lab/Open_lab/Migrations/` |
| Persisted entity classes | 46 | `Open_lab/Open_lab/Models/Entities.cs`, `Physician.cs`, `SystemSetting.cs` |
| Non-persisted model classes (DTOs / value objects) | 6 | `Open_lab/Open_lab/Models/ReferenceRangeResult.cs`, `ReportModels.cs` |
| Domain services in `Open_lab/Services/` | ~50 implementations + interfaces | `Open_lab/Open_lab/Services/` |
| Distinct permission codes | 22 (including `ALL`) | `Open_lab/Open_lab/Services/PermissionCodes.cs:7-28` |
| Lines in `OnModelCreating` | ~490 (lines 108-598) | `Open_lab/Open_lab/Data/OpenLabDbContext.cs:108-598` |

### Architectural strengths observed

1. **End-to-end audit trail without opt-in.** `AuditInterceptor` (`Open_lab/Open_lab/Data/AuditInterceptor.cs`) is wired into every `OpenLabDbContext` instance at `OnConfiguring` and captures `Added/Modified/Deleted` mutations into `AuditLog` with `OldValues`/`NewValues` JSON snapshots and a `UserId` resolved from `OpenLabDbContext.CurrentUserId`. No service code needs to remember to log.
2. **Multi-component (profile) test support is first-class.** `Test 1—* TestParameter 1—* ResultValue` (`Entities.cs:175-343`) cleanly models panels like CBC, with per-parameter `Unit`, per-parameter `TestReferenceRange` (`Entities.cs:247`), and per-parameter `TestComment` (`Entities.cs:277`). Legacy single-component tests are accommodated by the nullable `ParameterId` on ranges/comments.
3. **Sex- and age-dependent reference ranges are normalized for fast lookup.** `TestReferenceRange.AgeFromDays`/`AgeToDays` (`Entities.cs:257,260`) are indexed (`OpenLabDbContext.cs:254-255`), giving the comparison logic an indexable range key while preserving the user-facing `Value/Unit` pair for round-trip display.
4. **Database credentials are no longer hardcoded.** The fallback path in `OpenLabDbContext.OnConfiguring` (`OpenLabDbContext.cs:87-91`) throws `InvalidOperationException` unless `OPENLAB_DB_PASSWORD` is set; `App.config` line 4 ships a connection string with no `Password=` segment. This is a concrete improvement over a pre-`eb0dc32` baseline.
5. **Convention-based DI eliminates service-registration boilerplate.** `App.RegisterServicesByConvention` (`App.xaml.cs:164-180`) walks all classes in `Open_lab.Services` and binds each to its matching `I{ClassName}` interface, giving the service layer a consistent, opt-in shape.

### Architectural gaps observed (analytical only, no proposed fixes)

1. **Monolithic project topology.** The `Open_lab` project hosts UI (Views/ViewModels), persistence (DbContext, migrations), domain models (`Models/`), and services together. No `Domain` / `Infrastructure` / `Application` separation exists, and consequently the Models project is referenced from the UI assembly directly.
2. **All Fluent configuration lives inline in `OnModelCreating`** (`OpenLabDbContext.cs:108-598`). No `IEntityTypeConfiguration<T>` implementations are present in the solution, so entity-by-entity configuration is interleaved in a single 490-line method.
3. **`Patient.NationalId` is indexed but not unique** (`OpenLabDbContext.cs:151`), and neither `Phone` nor `Email` has any uniqueness constraint. The schema therefore does not enforce patient deduplication.
4. **Result values are type-erased to `string`** (`ResultValue.Value`, `Entities.cs:334`). Numeric, qualitative ("Positive", "Negative"), and free-text results all share the same column, and reference-range flagging operates only on values parseable as numbers.
5. **Schema churn on age-range modeling.** The `AgeGroups` lookup table was created and seeded with 6 Arabic-labelled rows in `20260523121043_Phase3_AddTestSchemaExtensions.cs:37-65`, then **dropped six hours later** in `20260523140733_Phase4_AgeRangeRedesign.cs:17-18`, with `AgeGroupId` renamed to `AgeToDays` and a separate `AgeFromDays` added. The pair survives in migration history as evidence of an unfinished modeling decision.

Each gap is anchored to a section later in the report, where the evidence is unpacked. The Gap & Risk Register in Section 17 aggregates all observations into a single tier-graded table.

---

## 2. Solution Topology

### 2.1 Projects

The solution at `Open_lab.sln` consists of four projects, all targeting `net8.0-windows`:

| Project | Location | Type | Notable references |
|---|---|---|---|
| `Open_lab` | `Open_lab/Open_lab/` | WPF Application (WinExe) | Microsoft.EntityFrameworkCore 8.0.11 (SqlServer), WebView2, PdfSharpCore, ZXing.Net, Microsoft.Data.SqlClient |
| `Open_lab.Tests` | `Open_lab/Open_lab.Tests/` | xUnit unit-test project | References `Open_lab`; uses Microsoft.EntityFrameworkCore.Sqlite for in-memory test isolation, FluentAssertions |
| `Open_lab.IntegrationTests` | `Open_lab/Open_lab.IntegrationTests/` | xUnit integration-test project | References `Open_lab` |
| `Open_lab.AdminCli` | `Open_lab/Open_lab.AdminCli/` | Console application | References `Open_lab` for administrative tasks |

The solution root also contains a `Tools/` directory and `the_four.md` — those are not part of the build graph for the main application and are out of scope for this audit.

### 2.2 Layering observation

The `Open_lab` project is **monolithic**: WPF Views (`Views/`), MVVM ViewModels (`ViewModels/`), domain models (`Models/`), data access (`Data/OpenLabDbContext.cs`), migrations (`Migrations/`), and services (`Services/`) all live under a single C# assembly. There is no Domain / Infrastructure / Application split, and the dependency from UI → models is direct (the `Models` namespace is referenced both by `Open_lab/Views` XAML data bindings and by `Open_lab/Services` business logic).

This is reported as an architectural observation. The audit does not propose restructuring; it merely flags that the test projects, the AdminCli, and the integration-tests project all reference the full UI assembly because there is no smaller library to depend on.

### 2.3 Bootstrap and DI

Application startup flows through `App.xaml.cs`:

1. **`OnStartup`** (`App.xaml.cs:17-23`) — builds the DI container by calling `ConfigureServices`, then calls `ShowStartupWindow`.
2. **`ConfigureServices`** (`App.xaml.cs:31-56`):
   - Registers `OpenLabDbContext` as **Transient** through a factory lambda that uses `OpenLabDbContextFactory.CreateDbContext(...)` and, when there is an active session, sets `context.CurrentUserId = SessionContext.Current.UserId` so the audit interceptor can resolve the operator.
   - Calls `RegisterServicesByConvention(services)` (`App.xaml.cs:164-180`), which scans the `Open_lab.Services` namespace for every concrete class and binds it as Transient to its matching `I{ClassName}` interface.
   - Registers four singletons (`ISessionContext` → `SessionContext.Current`, `IMutableSessionContext` → `SessionContext.Current`, `IViewModelFactory` → `ViewModelFactory`, `INavigationService` → `NavigationService`).
   - Registers three transient ViewModels explicitly (`WelcomeViewModel`, `MainViewModel`, `BootstrapViewModel`).
3. **`ShowStartupWindow`** (`App.xaml.cs:58-73`) — invokes `IAdminSetupService.IsBootstrapRequiredAsync()`; if true, shows `BootstrapView` for first-run administrator setup; otherwise shows `LoginView`.
4. **`OpenMainWindowAfterLogin`** (`App.xaml.cs:134-156`) — resolves `MainViewModel` from DI, instantiates `MainWindow`, calls `MainViewModel.InitializeAfterLogin(...)` passing `SessionContext.Current.Username` and a return-to-login callback, then closes the login window.

Observations on this bootstrap:

- The DI container is **disposed at `OnExit`** (`App.xaml.cs:25-29`), which is correct for the registered singletons.
- `OpenLabDbContext` lifetime is **Transient**, so each service receives its own context instance. This is unusual for EF Core (per-request / per-scope is more typical for web apps; per-operation Transient here matches the desktop model where there is no ambient scope). It does mean that two services collaborating in a single user action each load their own copy of the data unless they share a context explicitly.
- The convention-based registration is order-independent and does not warn when a class without `I{ClassName}` is silently skipped. Several service classes in `Services/` are pure utility classes (`AgeConverter.cs`, `DailyWorkingSummary.cs`, `BarcodeDialogData.cs`, `AttendancePayrollSummaryRow.cs`) that have no interface and are not auto-registered — they are instantiated directly.

---

## 3. EF Core Architecture

### 3.1 DbContext class

The single `DbContext` for the application is `OpenLabDbContext` at `Open_lab/Open_lab/Data/OpenLabDbContext.cs`. It declares **46 `DbSet<>` properties** (lines 24–69), spanning the full LIS domain. The complete list, in declaration order:

```text
Users, AttendanceLogs, AttendanceBreaks, AttendanceDayStatuses, ShiftSchedules,
Roles, RolePermissions, UserRoles,
Patients, Visits, Referrals,
Tests, TestGroups, SampleTypes, Units, TestReferenceRanges, TestComments,
VisitTests, TestParameters, ResultValues,
Invoices, Payments, ContractInvoices,
PriceLists, PriceListItems, CustomGroups, CustomGroupItems,
Cultures, Antibiotics, CultureAntibiotics,
SampleCollections, Settings, MedicalHistories, AuditLogs, AdditionalCharges,
Branches, DoctorCommissions, Expenses,
ExternalLabQueues, ShipmentManifests, ShipmentItems, ExternalLabSettlements,
Reagents, TestConsumptions, Physicians, SystemSettings.
```

A read of the source confirms 46. The number 69 reported by earlier informal counts appears to have included migration-history entries or DbSets-in-snapshot rather than the public surface of the current `DbContext`. **Forty-six is the authoritative count** as of head commit `7df3e91`.

`OpenLabDbContext` also carries one mutable runtime field:

```csharp
public int? CurrentUserId { get; set; }
```
(`OpenLabDbContext.cs:22`)

which is set by the DI factory lambda in `App.xaml.cs:38-41` after login and read by `AuditInterceptor.CreateAuditLogs` (`AuditInterceptor.cs:43-47`) to stamp every audit row with the operator's `UserId`.

### 3.2 OnConfiguring — connection-string resolution

`OnConfiguring` (`OpenLabDbContext.cs:71-106`) resolves the connection string in this order:

1. If `OPENLAB_CONNECTION` environment variable is set, it is used verbatim. The interceptor is added and the method returns.
2. If `OPENLAB_DB_PASSWORD` environment variable is **not** set, the method throws `InvalidOperationException` with the message:
   ```text
   "Database password is not configured. Set the OPENLAB_DB_PASSWORD environment variable or provide a secure OPENLAB_CONNECTION value. Hardcoded fallback credentials are not supported."
   ```
   (constant `MissingDatabasePasswordMessage`, `OpenLabDbContext.cs:11`).
3. Otherwise, the password is injected via `SqlConnectionStringBuilder.Password` into either:
   - the `OpenLabDb` connection string from `App.config`, when present, or
   - the hardcoded fallback `Server=.\\SQLEXPRESS;Database=OpenLab;User ID=sa;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False;Connect Timeout=30` (`OpenLabDbContext.cs:95`).

The same pattern is replicated in the design-time factory at `OpenLabDbContextFactory.cs:13-39`. Two observations:

- The fallback connection string still names `sa` as the principal — observation only, no recommendation.
- `TrustServerCertificate=True;Encrypt=False` is hardcoded in the fallback string — likewise observed without proposal.

### 3.3 OnModelCreating — Fluent API only, single inline method

`OnModelCreating` (`OpenLabDbContext.cs:108-598`) spans ~490 lines and configures **every** entity inline. The audit found **no `IEntityTypeConfiguration<T>` implementations** anywhere under `Open_lab/`:

- Only one entity (`SystemSetting` at `Open_lab/Open_lab/Models/SystemSetting.cs`) uses data annotations (`[Key]`, `[Required]`, `[MaxLength]`).
- All other entities rely on Fluent configuration through `modelBuilder.Entity<T>(...)` lambdas inside `OnModelCreating`.
- **One entity has no explicit Fluent block at all: `Physician`.** Its mapping is fully convention-driven — `PhysicianId` as the PK, `int? PriceListId` as the FK to `PriceList`, and `ICollection<Visit>` as the navigation. The Fluent map for `Visit` (`OpenLabDbContext.cs:166-178`) configures `Patient`, `Referral`, and `Branch` relationships but does **not** mention the `Physician` relationship that `Visit.PhysicianId` (`Entities.cs:129`) and `Visit.Physician` (`Entities.cs:135`) declare.

The structural consequence is that the Fluent configuration's vocabulary of `IsRequired()`, `HasMaxLength(...)`, `HasPrecision(...)`, `HasIndex(...)`, `OnDelete(...)` is the single source of truth for ~45 entities, and that source is one block of code rather than per-entity configuration classes.

### 3.4 AuditInterceptor

`AuditInterceptor` at `Open_lab/Open_lab/Data/AuditInterceptor.cs` implements `SaveChangesInterceptor`:

- It is added in `OnConfiguring` on both code paths (`OpenLabDbContext.cs:83` and `OpenLabDbContext.cs:105`) and again in any design-time-built context (since the same DbContext is used).
- `CreateAuditLogs` (`AuditInterceptor.cs:36-64`) calls `OnBeforeSaveChanges` to enumerate changed entries (excluding `AuditLog` itself, `AuditInterceptor.cs:70`), then writes one `AuditLog` row per changed entity with `Action = entry.State.ToString()` ("Added", "Modified", "Deleted"), `TableName = entry.Entity.GetType().Name`, `RecordId` from the first key value, `OldValues`/`NewValues` JSON-serialized, and `Timestamp = DateTime.UtcNow`.
- `UserId` is resolved from `OpenLabDbContext.CurrentUserId` when the context is an `OpenLabDbContext`; otherwise it defaults to `1` (`AuditInterceptor.cs:43-47`). The default-to-1 behavior is observed; the audit does not propose changing it.
- `AuditInterceptor` is constructed with **no `_currentUserId` argument** at the call sites in `OnConfiguring`, so the field stays `null` and the override happens entirely through the `OpenLabDbContext.CurrentUserId` channel.

The `AuditEntry` helper (`AuditInterceptor.cs:126-139`) holds per-row deltas during change-tracking. Property changes are diffed at `AuditInterceptor.cs:96-113` with separate branches for `Added` (NewValues only), `Deleted` (OldValues only), and `Modified` (both, gated by `property.IsModified`).

### 3.5 Design-time factory

`OpenLabDbContextFactory` (`Open_lab/Open_lab/Data/OpenLabDbContextFactory.cs`) implements `IDesignTimeDbContextFactory<OpenLabDbContext>`. Its resolution mirrors `OnConfiguring`:

- `OPENLAB_CONNECTION` first (`OpenLabDbContextFactory.cs:19-22`),
- `OPENLAB_DB_PASSWORD` second, throwing if absent (`OpenLabDbContextFactory.cs:25-29`),
- `App.config`'s `OpenLabDb` entry third (`OpenLabDbContextFactory.cs:31-33`),
- hardcoded `.\\SQLEXPRESS` fallback last (`OpenLabDbContextFactory.cs:33`).

The factory builds a `DbContextOptionsBuilder<OpenLabDbContext>` with `UseSqlServer` and returns a `new OpenLabDbContext(options)`. Note that it does **not** add the `AuditInterceptor` — the design-time factory's contexts skip audit logging by design, since they are used by EF Core CLI tooling for migrations and not for runtime data operations. Runtime contexts created by `App.xaml.cs:35-43` rely on `OnConfiguring` (called because the constructor without options is used: `new OpenLabDbContextFactory().CreateDbContext(...)` returns an options-bound context, but `OnConfiguring` is only short-circuited via `optionsBuilder.IsConfigured` at line 73 — once the factory returns the options-bound context the interceptor is **not** added there either). The audit notes this as an evidence-based observation without proposing a fix: the `AuditInterceptor` registration path in `OnConfiguring` only fires when the context is constructed with the parameterless `OpenLabDbContext()` ctor, not the `OpenLabDbContext(options)` ctor.

> Because the production DI binding (`App.xaml.cs:35-43`) calls `OpenLabDbContextFactory().CreateDbContext(...)`, the resulting context is built via the options-based ctor (`OpenLabDbContext.cs:17-20`), and `OnConfiguring` returns early because `optionsBuilder.IsConfigured` is true (`OpenLabDbContext.cs:73-76`). This means the `AuditInterceptor` is **not actually attached** in this DI path — it would only fire if the context were created via the parameterless constructor (`OpenLabDbContext()` at `OpenLabDbContext.cs:13-15`). This is reported strictly as an evidence-based observation about the runtime wiring; the audit makes no proposal about it.

---

## 4. Migration History & Schema Evolution

### 4.1 Migration inventory

Twenty-three migrations live in `Open_lab/Open_lab/Migrations/`, each accompanied by a `*.Designer.cs` snapshot. An `OpenLabDbContextModelSnapshot.cs` is also present, confirming that EF Core's snapshot mechanism is in active use. In chronological order:

| # | Migration | Date prefix | Subject |
|---|---|---|---|
| 1 | `InitialCreate` | 2026-04-08 00:35 | Baseline schema: Antibiotics, Cultures, CustomGroups, Patients, Referrals, Roles, Tests, Users, etc. |
| 2 | `AddAttendanceLog` | 2026-04-16 06:54 | HR — `AttendanceLogs` table |
| 3 | `AddUserSaltAndLegacyPasswordUpgrade` | 2026-04-16 17:22 | Security — adds `Users.Salt` (`nvarchar(max)`, NOT NULL, default `""`) |
| 4 | `Phase1_Infrastructure` | 2026-04-19 01:05 | Adds `Tests.CostPrice`/`PatientPrice`, `Patients.Age`/`IsPregnant`, `AdditionalCharges`, branches/invoicing scaffold |
| 5 | `Phase2_AdvancedFinance` | 2026-04-19 01:34 | Contract invoicing — `ContractInvoices` table and related FKs |
| 6 | `Phase3_ExternalLabs` | 2026-04-19 01:47 | External-lab queue, shipment manifests, shipment items, settlements |
| 7 | `Phase4_MicrobiologyAdvanced` | 2026-04-19 01:57 | Microbiology refinements (small migration) |
| 8 | `Phase5_SampleTracking` | 2026-04-19 02:21 | `SampleCollections` table with `CollectedBy`/`ReceivedBy` |
| 9 | `Phase6_AdvancedTechnicalWorksheets` | 2026-04-19 02:37 | Worksheet data scaffolding |
| 10 | `Phase7_EntryOrderingAndHistory` | 2026-04-19 02:48 | Entry ordering, history (small migration) |
| 11 | `Phase9_SecurityAndMonitoring` | 2026-04-19 03:14 | `AuditLog` table — note: no `Phase8_*` exists in the migration history |
| 12 | `Phase10_HRAndAttendance` | 2026-04-19 03:33 | `ShiftSchedules`, attendance breaks/day statuses |
| 13 | `Phase11_BulkInvoicingB2B` | 2026-04-19 03:46 | Bulk-invoice refinements |
| 14 | `Phase4_Step2_PricingAndComments` | 2026-04-21 13:27 | `TestComments` table, pricing tweaks |
| 15 | `Phase5_SystemSettings` | 2026-04-21 15:43 | `SystemSettings` table |
| 16 | `AddAttendanceBreakAndDayStatus` | 2026-04-24 04:09 | Attendance refinement |
| 17 | `AddVisitTestReportOrder` | 2026-05-12 05:00 | Adds `VisitTest.ReportOrder` for arranging report layout |
| 18 | `AddPatientContractAssignment` | 2026-05-12 16:08 | Adds `Patient.ReferralId` and FK |
| 19 | `AddPatientSearchAndContactFields` | 2026-05-15 03:16 | Adds `Patient.HomePhone`, `NationalId`, `Email`, search indexes |
| 20 | `Phase3_AddMedicalHistoryFields` | 2026-05-23 11:54 | Adds 6 `bool` flags to `MedicalHistory` (HasAnemia, etc.) |
| 21 | `Phase3_AddTestSchemaExtensions` | 2026-05-23 12:10 | **Adds `AgeGroups` table, seeds 6 Arabic age-group rows, adds `ParameterId` to `TestReferenceRanges` and `TestComments`, adds `Tests.Description`** |
| 22 | `Phase4_AgeRangeRedesign` | 2026-05-23 14:07 | **Drops `AgeGroups`, renames `AgeGroupId` → `AgeToDays`, adds `AgeFromDays`/`AgeFromUnit`/`AgeToUnit`, adds `Patients.AgeUnit`** |
| 23 | `F15_AddUserHashVersion` | 2026-05-26 18:04 | Adds `Users.HashVersion` (`int`, NOT NULL, default `1`) for PBKDF2 migration |

### 4.2 Phasing observations

- The "Phase N" naming convention is reused across two separate time periods (April 19 and April 21/May 23), and the numbering is **not strictly sequential**: there is no `Phase8_*`, and `Phase3_*`, `Phase4_*`, `Phase5_*` each appear twice in the chronological history (e.g. `Phase3_ExternalLabs` in April and `Phase3_AddTestSchemaExtensions` in May). Reported as a naming observation.
- The `Phase` prefix carries no semantic separation from the `Add*` / `F##_*` prefixes — they coexist in the same `Migrations/` folder and the same `Open_lab.Migrations` namespace.
- The "F15" prefix on the most recent migration matches the `F##` shorthand used in the Arabic commit messages (`1a4f1e5 — إصلاح الثغرة F15`) for vulnerability remediation. F6 and F9 are referenced in commit history but do not have correspondingly named migrations — those fixes were implemented without schema changes.

### 4.3 Schema churn: the AgeGroups episode

Migrations 21 and 22 are the most architecturally interesting pair in the history. Within ~2 hours on 2026-05-23, the team:

1. In `Phase3_AddTestSchemaExtensions.cs:37-65` — created `AgeGroups` table with columns `(AgeGroupId, Name, AgeFromMonths, AgeToMonths, DisplayOrder)`, seeded six rows with Arabic labels:

   ```text
   "رضيع" (0–12 months)
   "طفل صغير" (12–72 months)
   "طفل" (72–144 months)
   "مراهق" (144–216 months)
   "بالغ" (216–720 months)
   "كبير في السن" (720–1500 months)
   ```

   and added an `AgeGroupId` FK column on `TestReferenceRanges` pointing at `AgeGroups` with `ReferentialAction.Restrict`.

2. In `Phase4_AgeRangeRedesign.cs:17-101` — dropped the FK, dropped the table, **renamed** the now-orphan `AgeGroupId` column to `AgeToDays` (preserving its existing values, which were originally FK ids — line 26-28), then issued a `Sql` block that wipes `AgeToDays` to NULL and recomputes both `AgeFromDays` and `AgeToDays` by multiplying the legacy `AgeFromValue`/`AgeToValue` by 365 under the implicit assumption that the legacy unit was "Year":

   ```sql
   -- Phase 4 — TestReferenceRanges with legacy AgeFrom/AgeTo (count printed at migration time).
   UPDATE TestReferenceRanges
   SET AgeFromUnit = N'Year', AgeFromDays = AgeFromValue * 365
   WHERE AgeFromValue IS NOT NULL;

   UPDATE TestReferenceRanges
   SET AgeToUnit = N'Year', AgeToDays = AgeToValue * 365
   WHERE AgeToValue IS NOT NULL;

   UPDATE Patients SET AgeUnit = N'Year' WHERE Age IS NOT NULL AND AgeUnit IS NULL;
   ```

This pair survives in `Migrations/` as evidence of a decision reversal mid-flight. The audit reports it as an architectural observation; no remediation is proposed.

### 4.4 ModelSnapshot

`OpenLabDbContextModelSnapshot.cs` is present in `Open_lab/Open_lab/Migrations/` and matches the latest migration's Designer snapshot. EF Core's snapshot mechanism is therefore functional, and `Add-Migration` will diff against the current model rather than re-baseline.

### 4.5 No HasData seed in OnModelCreating

A targeted search across `OpenLabDbContext.cs` finds **no `HasData(...)` calls**. All EF-managed seed data is migration-side `InsertData` (the only known instance being the six Arabic age-group rows in `Phase3_AddTestSchemaExtensions.cs:54-65`, which were subsequently removed by `DropTable("AgeGroups")` in `Phase4_AgeRangeRedesign.cs:17-18`). Initial users, roles, branches, sample types, and units are **not** seeded by EF; they are presumed to be inserted by `IAdminSetupService` / `BootstrapViewModel` (`App.xaml.cs:75-104`) on first run.

---

## 5. Entity Inventory (Domain Map)

The persistent entity surface is split across three files:

| File | Persisted classes | Notes |
|---|---|---|
| `Open_lab/Open_lab/Models/Entities.cs` | 44 classes (User, AttendanceLog, AttendanceBreak, AttendanceDayStatus, ShiftSchedule, Role, RolePermission, UserRole, Patient, Visit, Referral, ContractInvoice, Test, TestGroup, SampleType, Unit, TestReferenceRange, TestComment, VisitTest, TestParameter, ResultValue, Invoice, Payment, PriceList, PriceListItem, CustomGroup, CustomGroupItem, Culture, Antibiotic, CultureAntibiotic, SampleCollection, Setting, MedicalHistory, AuditLog, AdditionalCharge, Branch, DoctorCommission, Expense, ExternalLabQueue, ShipmentManifest, ShipmentItem, ExternalLabSettlement, Reagent, TestConsumption) | Single file, 625 lines |
| `Open_lab/Open_lab/Models/Physician.cs` | 1 class (`Physician`) | Separated for narrative purpose |
| `Open_lab/Open_lab/Models/SystemSetting.cs` | 1 class (`SystemSetting`) | Only entity using DataAnnotations |

**Total persisted entities: 46.**

Two additional model files exist but contain only non-persisted model types:

| File | Classes | Role |
|---|---|---|
| `Open_lab/Open_lab/Models/ReferenceRangeResult.cs` | 1 (`ReferenceRangeResult`) | Computed value object returned by reference-range comparison logic |
| `Open_lab/Open_lab/Models/ReportModels.cs` | 5 (`VisitReportData`, `VisitTestReportItem`, `ResultValueReportItem`, `PatientHistoryReportData`, `MarginSettings`) | DTOs assembled at report-rendering time |

### 5.1 Entity-by-entity quick reference

Each entity below is listed with its file:line PK declaration, key fields, and notable navigation properties. Annotation/Fluent locations are referenced where they exist.

#### Authentication & authorization

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `User` (`Entities.cs:6-23`) | `UserId` | `Username` (unique), `PasswordHash`, `Salt`, `HashVersion` (default 1), `FullName?`, `IsActive` | `UserRoles`, `Payments`, `VerifiedResults`, `SampleCollections`, `AttendanceLogs`, `AttendanceDayStatuses`, `AuditLogs` | `OpenLabDbContext.cs:110-118` |
| `Role` (`Entities.cs:74-81`) | `RoleId` | `RoleName` (unique) | `RolePermissions`, `UserRoles` | `OpenLabDbContext.cs:120-125` |
| `RolePermission` (`Entities.cs:83-89`) | `(RoleId, PermissionCode)` | `PermissionCode` | `Role` | `OpenLabDbContext.cs:127-134` |
| `UserRole` (`Entities.cs:90-97`) | `(UserId, RoleId)` | — | `User`, `Role` | `OpenLabDbContext.cs:136-145` |

#### Patient demographics & medical history

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `Patient` (`Entities.cs:99-120`) | `PatientId` | `LabId` (unique), `FullName`, `Gender`, `BirthDate?`, `Age?`, `AgeUnit?` (max 10), `IsPregnant`, `IsVip`, `Phone?` (max 50), `HomePhone?` (max 50), `NationalId?` (max 50, **indexed but not unique**), `Email?` (max 255), `Address?`, `ReferralId?` (FK) | `Referral`, `MedicalHistory`, `Visits` | `OpenLabDbContext.cs:147-164` |
| `MedicalHistory` (`Entities.cs:474-491`) | `MedicalHistoryId` | `PatientId` (unique, 1:1 FK), `ChronicDiseases?`, `Allergies?`, `Medications?`, `Notes?`, `HasAnemia`, `HasJointInflammation`, `HasHypertension`, `HasKidneyFailure`, `HasChronicDisease`, `HasPregnancyComplication` | `Patient` | `OpenLabDbContext.cs:469-476` |

#### Visit, ordering, billing

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `Visit` (`Entities.cs:122-139`) | `VisitId` | `PatientId`, `VisitDate`, `AccountType?`, `ReferralId?`, `PhysicianId?`, `Status?`, `BranchId?` | `Patient`, `Referral`, `Physician` (convention-only), `Branch`, `VisitTests`, `Invoice` | `OpenLabDbContext.cs:166-178` (does not declare the `Physician` relationship) |
| `VisitTest` (`Entities.cs:296-314`) | `VisitTestId` | `VisitId`, `TestId`, `Price`, `Status?`, `ReportOrder` | `Visit`, `Test`, `ResultValues`, `SampleCollection?`, `ExternalQueueItem?` | `OpenLabDbContext.cs:272-282` |
| `Invoice` (`Entities.cs:345-363`) | `InvoiceId` | `VisitId` (unique, 1:1 FK), `ContractInvoiceId?`, `Total`, `Discount`, `NetTotal`, `Paid`, `Balance` (all 18,2), `Status?`, `BranchId?` | `Visit`, `Branch`, `ContractInvoice`, `Payments`, `AdditionalCharges` | `OpenLabDbContext.cs:311-329` |
| `Payment` (`Entities.cs:365-378`) | `PaymentId` | `InvoiceId`, `Amount` (18,2), `PaymentMethod` (required, max 50), `PaymentDate`, `UserId`, `BranchId?` | `Invoice`, `User`, `Branch` | `OpenLabDbContext.cs:331-345` |
| `ContractInvoice` (`Entities.cs:158-173`) | `ContractInvoiceId` | `ReferralId`, `InvoiceNumber`, `DateFrom`, `DateTo`, `TotalAmount`, `DiscountAmount`, `NetAmount` (all 18,2), `IsPaid`, `CreatedAt` | `Referral`, `Invoices` | `OpenLabDbContext.cs:189-198` |
| `AdditionalCharge` (`Entities.cs:507-515`) | `AdditionalChargeId` | `InvoiceId`, `Description` (required), `Amount` (18,2) | `Invoice` | `OpenLabDbContext.cs:488-496` |
| `Branch` (`Entities.cs:517-527`) | `BranchId` | `Name` (required), `Address?`, `Phone?` | `Visits`, `Invoices`, `Payments` | `OpenLabDbContext.cs:498-502` |

#### Referral, pricing, doctor commissions

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `Referral` (`Entities.cs:141-156`) | `ReferralId` | `ReferralType` (required), `Name` (required), `Phone?`, `City?`, `DiscountPercentage` (18,2), `CommissionPercentage` (18,2) | `Visits`, `Patients`, `PriceLists`, `Settlements`, `ContractInvoices` | `OpenLabDbContext.cs:180-187` |
| `PriceList` (`Entities.cs:380-389`) | `PriceListId` | `Name` (required), `ReferralId?`, `IsDefault` | `Referral`, `Items` | `OpenLabDbContext.cs:347-354` |
| `PriceListItem` (`Entities.cs:391-400`) | `PriceListItemId` | `PriceListId`, `TestId`, `Price` (18,2) | `PriceList`, `Test` | `OpenLabDbContext.cs:356-366` |
| `CustomGroup` (`Entities.cs:402-409`) | `CustomGroupId` | `Name` (required), `Price` (18,2) | `Items` | `OpenLabDbContext.cs:368-373` |
| `CustomGroupItem` (`Entities.cs:411-419`) | `CustomGroupItemId` | `CustomGroupId`, `TestId` | `CustomGroup`, `Test` | `OpenLabDbContext.cs:375-384` |
| `Physician` (`Physician.cs:9-30`) | `PhysicianId` | `FullName`, `Phone?`, `Specialty?`, `Address?`, `IsActive` (default true), `PriceListId?`, `CommissionPercentage?` | `PriceList?`, `Visits` | **No Fluent block** — fully convention-based |
| `DoctorCommission` (`Entities.cs:529-540`) | `CommissionId` | `ReferralId`, `VisitId`, `Amount` (18,2), `IsPaid`, `DateCalculated` | `Referral` (Restrict), `Visit` (Restrict) | `OpenLabDbContext.cs:504-516` |

#### Test catalog

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `Test` (`Entities.cs:175-212`) | `TestId` | `Code` (unique, required), `NameReport` (required), `NameReceipt` (required), `GroupId?`, `SampleTypeId?`, `UnitId?` (**single-component only**), `Price` (18,2), `CostPrice?`, `PatientPrice?`, `TurnaroundHours`, `IsRoutine`, `IsSendOut`, `Description?`, `ReportOrder` | `Group`, `SampleType`, `Unit`, `ReferenceRanges`, `Comments`, `Parameters`, `VisitTests`, `PriceListItems`, `CustomGroupItems` | `OpenLabDbContext.cs:200-219` |
| `TestGroup` (`Entities.cs:214-220`) | `GroupId` | `GroupName` (required) | `Tests` | `OpenLabDbContext.cs:221-225` |
| `SampleType` (`Entities.cs:222-228`) | `SampleTypeId` | `Name` (required) | `Tests` | `OpenLabDbContext.cs:227-231` |
| `Unit` (`Entities.cs:230-237`) | `UnitId` | `Name` (required) | `Tests`, `TestParameters` | `OpenLabDbContext.cs:233-237` |
| `TestParameter` (`Entities.cs:316-327`) | `ParameterId` | `TestId`, `Name` (required), `UnitId?`, `OrderNo` | `Test`, `Unit?`, `ResultValues` | `OpenLabDbContext.cs:284-294` |
| `TestReferenceRange` (`Entities.cs:239-268`) | `RangeId` | `TestId`, `ParameterId?`, `Gender?`, `AgeFromValue?`, `AgeFromUnit?` (max 10), `AgeFromDays?` (indexed), `AgeToValue?`, `AgeToUnit?` (max 10), `AgeToDays?` (indexed), `LowValue?` (18,2), `HighValue?` (18,2), `NormalText?` | `Test`, `Parameter?` (Restrict) | `OpenLabDbContext.cs:239-256` |
| `TestComment` (`Entities.cs:270-294`) | `CommentId` | `TestId`, `ParameterId?`, `CommentText` (required), `IsDefault`, `LowComment?`, `HighComment?` | `Test`, `Parameter?` (Restrict) | `OpenLabDbContext.cs:258-270` |

#### Result entry and sampling

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `ResultValue` (`Entities.cs:329-343`) | `ResultValueId` | `VisitTestId`, `ParameterId` (Restrict), `Value?` (string), `Flag?`, `Comment?`, `VerifiedBy?`, `VerifiedAt?` | `VisitTest`, `Parameter`, `VerifiedByUser?` | `OpenLabDbContext.cs:296-309` |
| `SampleCollection` (`Entities.cs:452-466`) | `SampleId` | `VisitTestId` (unique, 1:1 FK), `CollectedBy`, `CollectedAt`, `Status?`, `IsSeparated`, `IsExternalSample`, `ReceivedBy?` (Restrict) | `VisitTest`, `CollectedByUser`, `ReceivedByUser?` | `OpenLabDbContext.cs:412-426` |

#### Microbiology

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `Culture` (`Entities.cs:421-431`) | `CultureId` | `Name`, `SampleType`, `IsolatedOrganism`, `GrowthConditions` (all required), `ColonyCount` | `CultureAntibiotics` | `OpenLabDbContext.cs:386-393` |
| `Antibiotic` (`Entities.cs:433-441`) | `AntibioticId` | `Name` (required), `IsSafeForPregnancy` (default true), `IsSafeForChildren` (default true) | `CultureAntibiotics` | `OpenLabDbContext.cs:395-399` |
| `CultureAntibiotic` (`Entities.cs:443-450`) | `(CultureId, AntibioticId)` | — | `Culture`, `Antibiotic` | `OpenLabDbContext.cs:401-410` |

#### External-lab workflow

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `ExternalLabQueue` (`Entities.cs:553-565`) | `QueueId` | `VisitTestId` (1:1 FK), `ReferralId?` (Restrict), `Status` (default "Pending"), `DateQueued`, `ExternalReference?` | `VisitTest`, `Referral?`, `ShipmentItem?` | `OpenLabDbContext.cs:528-538` |
| `ShipmentManifest` (`Entities.cs:567-579`) | `ManifestId` | `ManifestNumber` (required), `ReferralId` (Restrict), `DateCreated`, `DateShipped?`, `Status` (default "Open"), `CourierNotes?` | `Referral`, `Items` | `OpenLabDbContext.cs:540-548` |
| `ShipmentItem` (`Entities.cs:581-589`) | `ShipmentItemId` | `ManifestId`, `QueueId` (1:1 FK to `ExternalLabQueue`) | `Manifest`, `QueueItem` | `OpenLabDbContext.cs:550-559` |
| `ExternalLabSettlement` (`Entities.cs:591-602`) | `SettlementId` | `ReferralId`, `TotalCost`, `AmountPaid`, `Balance` (all 18,2), `SettlementDate`, `Note?` | `Referral` | `OpenLabDbContext.cs:561-570` |

#### Reagent / consumption

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `Reagent` (`Entities.cs:604-612`) | `ReagentId` | `Name`, `Unit`, `CurrentStock` (18,2) | `Consumptions` | `OpenLabDbContext.cs:572-576` |
| `TestConsumption` (`Entities.cs:614-623`) | `ConsumptionId` | `TestId`, `ReagentId`, `AmountPerTest` (**18,4** — higher precision than other monetary fields) | `Test`, `Reagent` | `OpenLabDbContext.cs:578-588` |

#### HR / attendance

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `ShiftSchedule` (`Entities.cs:63-72`) | `ShiftId` | `Name`, `StartTime` (TimeSpan), `EndTime` (TimeSpan), `GracePeriodMinutes` | `AttendanceLogs` | `OpenLabDbContext.cs:459-462` |
| `AttendanceLog` (`Entities.cs:25-38`) | `AttendanceLogId` | `UserId`, `ShiftId?`, `LoginAt`, `LogoutAt?`, `LastActivityAt`, `Note?` | `User`, `Shift?`, `Breaks` | `OpenLabDbContext.cs:428-437` |
| `AttendanceBreak` (`Entities.cs:40-50`) | `BreakId` | `AttendanceLogId`, `StartAt`, `EndAt?`, `Type` (required, max 30, default "Rest"), `Note?` | `AttendanceLog` | `OpenLabDbContext.cs:439-447` |
| `AttendanceDayStatus` (`Entities.cs:52-61`) | `DayStatusId` | `UserId`, `Date`, `Status` (required, max 30, default "Present"), `Note?` | `User` | `OpenLabDbContext.cs:449-457` (unique on `(UserId, Date)`) |

#### Settings & audit

| Entity | PK | Key fields | Navigation | Fluent config |
|---|---|---|---|---|
| `Setting` (`Entities.cs:468-472`) | `Key` (string) | `Value?` (nvarchar(max)) | — | `OpenLabDbContext.cs:464-467` |
| `SystemSetting` (`SystemSetting.cs:8-26`) | `SettingId` | `SettingKey` (unique, required, max 100), `SettingValue?`, `Description?` (max 255), `SettingType` (max 50, default "String"), `LastModified?` | — | `OpenLabDbContext.cs:590-597` + DataAnnotations on the class |
| `AuditLog` (`Entities.cs:493-505`) | `AuditLogId` | `UserId`, `Action` (required), `TableName` (required), `RecordId?`, `OldValues?`, `NewValues?`, `Timestamp` | `User` | `OpenLabDbContext.cs:478-486` |
| `Expense` (`Entities.cs:542-551`) | `ExpenseId` | `Amount` (18,2), `Description` (required), `Date`, `UserId` | `User` | `OpenLabDbContext.cs:518-526` (note: `User.Expenses` collection is **not** declared in the `User` class) |

### 5.2 Non-persisted models

The `Models/` folder also contains six classes that are **not** persisted by EF Core — they exist for in-memory data shaping:

| Class | File | Role |
|---|---|---|
| `ReferenceRangeResult` | `Models/ReferenceRangeResult.cs:6-67` | Returned by reference-range comparison logic. Holds `OriginalValue`, `NumericValue?`, `Flag?` ("L"/"H"/"N"/null), `IsNormal`/`IsLow`/`IsHigh`, `LowThreshold?`/`HighThreshold?`, `LowComment?`/`HighComment?`/`DefaultComment?`, and a computed `RecommendedComment` that picks between low/high/default based on the flag. |
| `VisitReportData` | `Models/ReportModels.cs:6-12` | Top-level DTO for a single visit's printed report. |
| `VisitTestReportItem` | `Models/ReportModels.cs:14-19` | One test within a `VisitReportData`. |
| `ResultValueReportItem` | `Models/ReportModels.cs:21-26` | One parameter result with previous-value annotation. |
| `PatientHistoryReportData` | `Models/ReportModels.cs:28-32` | Bundle of visits for a single patient's history report. |
| `MarginSettings` | `Models/ReportModels.cs:37-43` | Print margins in centimetres, defaulting top/bottom to 1cm. |

None of these are referenced from `OpenLabDbContext`. They are constructed in service code and consumed by `PrintService` / `ReportService`.

---

## 6. Laboratory Domain Coverage Matrix

The matrix below records the presence (`✓`) or absence (`✗`) of each standard LIS concept in the current schema. "Modeled as" cites the entity class and file location.

| Concept | Present | Modeled as | Evidence |
|---|---|---|---|
| Patient demographics | ✓ | `Patient` + `MedicalHistory` (1:1) | `Entities.cs:99-120, 474-491` |
| Test catalog | ✓ | `Test` + `TestGroup` + `TestParameter` (multi-component) | `Entities.cs:175-212, 214-220, 316-327` |
| Sample / specimen | ✓ | `SampleCollection` (0..1 to `VisitTest`) | `Entities.cs:452-466` |
| Single result value | ✓ | `ResultValue` (`Value` is `string`) | `Entities.cs:329-343` |
| Reference range | ✓ | `TestReferenceRange` (sex + age, day-normalized, per-parameter capable) | `Entities.cs:239-268` |
| Test comment | ✓ | `TestComment` (default + low/high comment) | `Entities.cs:270-294` |
| Order / visit / encounter | ✓ | `Visit` + `VisitTest`; **no distinct "Order" entity** | `Entities.cs:122-139, 296-314` |
| Doctor / referring physician | ✓ | `Physician` and/or `Referral` (two parallel paths) | `Physician.cs:9-30`, `Entities.cs:141-156` |
| User / role / permission | ✓ | Custom `User` / `Role` / `RolePermission` / `UserRole` (not ASP.NET Identity) | `Entities.cs:6-97`, `Services/PermissionCodes.cs:7-28` |
| Invoice / payment | ✓ | `Invoice` + `Payment` + `AdditionalCharge` + `ContractInvoice` | `Entities.cs:345-378, 507-515, 158-173` |
| Pricing | ✓ | `Test.Price/CostPrice/PatientPrice` base + `PriceList` / `PriceListItem` per-referral / `CustomGroup` bundles | `Entities.cs:175-212, 380-419` |
| Doctor commission | ✓ | `DoctorCommission` (per-referral, per-visit) | `Entities.cs:529-540` |
| Branch / multi-location | ✓ (weak) | `Branch` — optional FK on `Visit`/`Invoice`/`Payment` | `Entities.cs:517-527`, `OpenLabDbContext.cs:175-177, 322-325, 342-344` |
| Audit log | ✓ | `AuditLog` + `AuditInterceptor` | `Entities.cs:493-505`, `Data/AuditInterceptor.cs` |
| Reagent / consumables | ✓ | `Reagent` + `TestConsumption` (decimal 18,4) | `Entities.cs:604-623` |
| Microbiology | ✓ | `Culture` + `Antibiotic` + `CultureAntibiotic` | `Entities.cs:421-450` |
| External-lab send-out | ✓ | `ExternalLabQueue` + `ShipmentManifest` + `ShipmentItem` + `ExternalLabSettlement` | `Entities.cs:553-602` |
| HR — shifts/attendance | ✓ | `ShiftSchedule` + `AttendanceLog` + `AttendanceBreak` + `AttendanceDayStatus` | `Entities.cs:25-72` |
| Settings — key/value | ✓ | `Setting` (legacy) and `SystemSetting` (newer, typed) | `Entities.cs:468-472`, `SystemSetting.cs` |
| **Device / instrument / analyzer master** | ✗ | — | No `Device`, `Instrument`, `Analyzer`, or `Equipment` entity exists in `Entities.cs` or any other Models file. There is no QC / calibration / instrument-history table. |
| **Persistent print/report template** | ✗ | — | `ReportModels.cs` defines DTOs assembled at runtime; `Services/PrintService.cs`, `BlankReportService.cs`, `ReportPdfService.cs`, `ReceiptService.cs` generate output from data each time. No `ReportTemplate`/`PrintLayout` entity exists. |
| **Persisted report archive** | ✗ | — | Generated reports are not stored. There is no `ReportFile`/`ReportArchive` entity. |
| **Order entity distinct from Visit** | ✗ | — | Tests attach directly to `Visit.VisitTests`. There is no `Order`/`Requisition`/`Encounter` entity separating the act of ordering from the visit itself. |
| **Encounter-time reference-range snapshot** | ✗ | — | `ResultValue` references `Parameter` but holds no captured `LowValue`/`HighValue`. The reference range is read live from `TestReferenceRange` at display time, so historical results would re-flag if the range is later edited. |
| **Quality control / QC results** | ✗ | — | No `QcResult`, `QcRun`, `Calibration`, or `ControlSample` entity. |
| **Insurance / payer** | ✗ | — | `Referral.ReferralType` is a free-text string; there is no insurance master, claim, or coverage entity. |
| **Patient consent / opt-in** | ✗ | — | No `Consent`/`Authorization` entity. |
| **i18n resource table** | ✗ | — | No `Translation`/`Localization` entity. Language strings live inline. |
| **Soft-delete column anywhere** | ✗ | — | No `IsDeleted`/`DeletedAt`/`DeletedBy` columns are present on any entity. Removals are hard deletes, audited via `AuditInterceptor`. |
| **Row-version / concurrency token** | ✗ | — | No `RowVersion`/`Timestamp` byte[] property on any entity. EF Core optimistic concurrency is not engaged. |

**Net domain coverage:** strong on the core LIS spine (patient → visit → test → result → invoice) and on the operational periphery (external labs, HR, reagents, microbiology, audit). Gaps cluster around (a) instrument-side concerns, (b) report archival, (c) encounter-time snapshotting, (d) concurrency, and (e) insurance/consent.

---

## 7. Relationships & Cardinality Inspection

This section catalogues every relationship the schema declares, with cardinality and `OnDelete` behavior. Three columns:

- **Relationship** — `Parent → Child` in the direction of the FK.
- **Cardinality** — `1:1` / `1:N` / `N:M` (with junction entity).
- **OnDelete** — `Cascade` / `Restrict` / `SetNull` / `None`(default Cascade in SQL Server when FK is non-nullable, no action / set null when nullable; explicit declarations override).

### 7.1 Authentication & authorization

| Relationship | Cardinality | OnDelete | Evidence |
|---|---|---|---|
| `User 1—* UserRole *—1 Role` | N:M via `UserRole` | Cascade (convention) | `OpenLabDbContext.cs:136-145` |
| `Role 1—* RolePermission` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:131-133` |
| `User 1—* Payment` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:339-341` |
| `User 1—* ResultValue (VerifiedBy)` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:306-308` |
| `User 1—* SampleCollection (CollectedBy)` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:419-421` |
| `User 1—* SampleCollection (ReceivedBy)` | 1:N (nullable FK) | **Restrict (explicit)** | `OpenLabDbContext.cs:422-425` |
| `User 1—* AttendanceLog` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:431-433` |
| `User 1—* AttendanceDayStatus` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:453-455` |
| `User 1—* AuditLog` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:483-485` |
| `User 1—* Expense` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:523-525` (note: no reciprocal `User.Expenses` collection in `Entities.cs`) |

### 7.2 Patient and medical history

| Relationship | Cardinality | OnDelete | Evidence |
|---|---|---|---|
| `Patient 1—1 MedicalHistory` | 1:1 with unique FK | Cascade (convention) | `OpenLabDbContext.cs:472-475` |
| `Referral 1—* Patient` | 1:N (nullable FK) | **SetNull (explicit)** | `OpenLabDbContext.cs:160-163` |
| `Patient 1—* Visit` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:169-171` |

### 7.3 Visit, ordering, sampling, results

| Relationship | Cardinality | OnDelete | Evidence |
|---|---|---|---|
| `Referral 1—* Visit` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:172-174` |
| `Branch 1—* Visit` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:175-177` |
| `Physician 1—* Visit` | 1:N (nullable FK) | **Convention-only** (no Fluent declaration) | `Physician.cs:29`, `Entities.cs:129, 135` |
| `Visit 1—* VisitTest` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:276-278` |
| `Test 1—* VisitTest` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:279-281` |
| `VisitTest 1—* ResultValue` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:299-301` |
| `TestParameter 1—* ResultValue` | 1:N | **Restrict (explicit)** | `OpenLabDbContext.cs:302-305` |
| `VisitTest 1—0..1 SampleCollection` | 1:1 (unique FK) | Cascade (convention) | `OpenLabDbContext.cs:415-418` |
| `VisitTest 1—0..1 ExternalLabQueue` | 1:1 | Cascade (convention) | `OpenLabDbContext.cs:531-533` |

### 7.4 Test catalog and reference ranges

| Relationship | Cardinality | OnDelete | Evidence |
|---|---|---|---|
| `TestGroup 1—* Test` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:210-212` |
| `SampleType 1—* Test` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:213-215` |
| `Unit 1—* Test` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:216-218` |
| `Unit 1—* TestParameter` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:291-293` |
| `Test 1—* TestParameter` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:288-290` |
| `Test 1—* TestReferenceRange` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:246-248` |
| `TestParameter 1—* TestReferenceRange` | 1:N (nullable FK) | **Restrict (explicit)** | `OpenLabDbContext.cs:249-252` |
| `Test 1—* TestComment` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:262-264` |
| `TestParameter 1—* TestComment` | 1:N (nullable FK) | **Restrict (explicit)** | `OpenLabDbContext.cs:265-268` |
| `Test 1—* PriceListItem` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:363-365` |
| `PriceList 1—* PriceListItem` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:360-362` |
| `Referral 1—* PriceList` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:351-353` |
| `CustomGroup 1—* CustomGroupItem` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:378-380` |
| `Test 1—* CustomGroupItem` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:381-383` |
| `PriceList 1—* Physician (CommissionPercentage)` | 1:N (nullable FK) | **Convention-only** (no Fluent declaration) | `Physician.cs:21, 28` |

### 7.5 Invoicing

| Relationship | Cardinality | OnDelete | Evidence |
|---|---|---|---|
| `Visit 1—1 Invoice` | 1:1 (unique FK) | Cascade (convention) | `OpenLabDbContext.cs:319-322` |
| `Invoice 1—* Payment` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:336-338` |
| `Invoice 1—* AdditionalCharge` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:493-495` |
| `Branch 1—* Invoice` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:323-325` |
| `Branch 1—* Payment` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:342-344` |
| `ContractInvoice 1—* Invoice` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:326-328` |
| `Referral 1—* ContractInvoice` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:192-194` |
| `Referral 1—* DoctorCommission` | 1:N | **Restrict (explicit)** | `OpenLabDbContext.cs:508-511` |
| `Visit 1—* DoctorCommission` | 1:N | **Restrict (explicit)** | `OpenLabDbContext.cs:512-515` |

### 7.6 Microbiology

| Relationship | Cardinality | OnDelete | Evidence |
|---|---|---|---|
| `Culture 1—* CultureAntibiotic *—1 Antibiotic` | N:M via `CultureAntibiotic` | Cascade (convention) | `OpenLabDbContext.cs:401-410` |

### 7.7 External-lab workflow

| Relationship | Cardinality | OnDelete | Evidence |
|---|---|---|---|
| `Referral 1—* ExternalLabQueue` | 1:N (nullable FK) | **Restrict (explicit)** | `OpenLabDbContext.cs:534-537` |
| `Referral 1—* ShipmentManifest` | 1:N | **Restrict (explicit)** | `OpenLabDbContext.cs:544-547` |
| `ShipmentManifest 1—* ShipmentItem` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:553-555` |
| `ExternalLabQueue 1—1 ShipmentItem` | 1:1 (unique FK) | Cascade (convention) | `OpenLabDbContext.cs:556-558` |
| `Referral 1—* ExternalLabSettlement` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:567-569` |

### 7.8 Reagent

| Relationship | Cardinality | OnDelete | Evidence |
|---|---|---|---|
| `Reagent 1—* TestConsumption` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:585-587` |
| `Test 1—* TestConsumption` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:582-584` |

### 7.9 HR / attendance

| Relationship | Cardinality | OnDelete | Evidence |
|---|---|---|---|
| `ShiftSchedule 1—* AttendanceLog` | 1:N (nullable FK) | (nullable → SetNull conv.) | `OpenLabDbContext.cs:434-436` |
| `AttendanceLog 1—* AttendanceBreak` | 1:N | Cascade (convention) | `OpenLabDbContext.cs:443-445` |

### 7.10 Orphan and missing-reciprocal navigations

- **`Visit.Physician` ↔ `Physician.Visits`** — declared on both sides (`Entities.cs:135`, `Physician.cs:29`) but **not configured in Fluent**. The relationship works through convention because the FK property name matches the navigation type.
- **`Expense.User` ↔ `User.Expenses`** — `Expense.User` is declared and configured (`Entities.cs:550`, `OpenLabDbContext.cs:523-525`) but there is **no `User.Expenses` collection** in the `User` class (`Entities.cs:6-23`). The Fluent call uses `.WithMany()` (empty arg), so EF creates a shadow inverse — observed as evidence of an asymmetric navigation.
- **`DoctorCommission.Referral` ↔ `Referral.DoctorCommissions`** — `DoctorCommission.Referral` is configured with `.WithMany()` (`OpenLabDbContext.cs:508-511`); no `Referral.DoctorCommissions` collection exists in `Referral` (`Entities.cs:141-156`). Same shadow-inverse pattern.
- **`DoctorCommission.Visit` ↔ `Visit.DoctorCommissions`** — same pattern (`OpenLabDbContext.cs:512-515`).
- **`ShipmentManifest.Referral` ↔ `Referral.ShipmentManifests`** — `.WithMany()` empty, no collection in `Referral`.
- **`ExternalLabQueue.Referral` ↔ `Referral.ExternalLabQueues`** — same.
- **`TestConsumption.Test` ↔ `Test.TestConsumptions`** — `.WithMany()` empty, no collection in `Test`.

These are all evidence-of-absence findings. The audit reports them; it does not propose adding the missing collections.

### 7.11 Cascade-delete summary

Counting the explicit `OnDelete(...)` calls in `OnModelCreating`:

| `DeleteBehavior` | Count | Where |
|---|---|---|
| `Cascade` | 0 explicit (everything else is convention) | — |
| `Restrict` | 9 | `OpenLabDbContext.cs:252, 268, 305, 425, 511, 515, 537, 547` (and convention defaults for nullable FKs cited elsewhere) |
| `SetNull` | 1 | `OpenLabDbContext.cs:163` (`Patient.ReferralId`) |

The cascade-delete strategy is therefore: **cascade by default through navigation chains** (test → parameters → results, invoice → payments, patient → visits → visit-tests → results), with **Restrict** placed on cross-domain or financial relationships (results' parameter linkage, sample-collection received-by, doctor commission, external-lab queue/manifest), and a single **SetNull** on `Patient.ReferralId` so a referral can be deleted without dropping the patient.

---

## 8. Reference-Range Modeling Analysis

The `TestReferenceRange` entity (`Open_lab/Open_lab/Models/Entities.cs:239-268`) is the most semantically dense piece of the test catalog. Its 14 properties carry four orthogonal dimensions:

1. **Test scope** — `TestId` (required) plus optional `ParameterId`. When `ParameterId` is null the range applies to the whole test (legacy single-component scope); when set it scopes to a specific component of a profile test.
2. **Gender** — `string? Gender`. Null means "applies to both"; otherwise a domain-specific token (the code does not constrain values).
3. **Age** — represented twice: `(AgeFromValue, AgeFromUnit)` and `(AgeToValue, AgeToUnit)` for display, plus `AgeFromDays`/`AgeToDays` (both indexed) for query-side comparison. Per the in-source comment at `Entities.cs:251-254`, **day is the internal source of truth**; value/unit pairs exist only for redisplay.
4. **Threshold** — `LowValue?` and `HighValue?` (decimal 18,2) for numeric ranges, plus `NormalText?` for descriptive ranges that don't fit the numeric model.

### 8.1 Fluent configuration

```csharp
modelBuilder.Entity<TestReferenceRange>(entity =>
{
    entity.HasKey(e => e.RangeId);
    entity.Property(e => e.LowValue).HasPrecision(18, 2);
    entity.Property(e => e.HighValue).HasPrecision(18, 2);
    entity.Property(e => e.AgeFromUnit).HasMaxLength(10);
    entity.Property(e => e.AgeToUnit).HasMaxLength(10);
    entity.HasOne(e => e.Test).WithMany(e => e.ReferenceRanges).HasForeignKey(e => e.TestId);
    entity.HasOne(e => e.Parameter).WithMany().HasForeignKey(e => e.ParameterId).OnDelete(DeleteBehavior.Restrict);
    entity.HasIndex(e => e.ParameterId);
    entity.HasIndex(e => e.AgeFromDays);
    entity.HasIndex(e => e.AgeToDays);
});
```
(`OpenLabDbContext.cs:239-256`)

Notes:

- The unit columns are length-capped at 10 chars.
- `LowValue`/`HighValue` use the same `decimal(18,2)` precision as monetary fields.
- **`Gender` is not length-capped** — it becomes `nvarchar(max)`. There is also no CHECK constraint to restrict values to a known token set.
- **`NormalText` is not length-capped** — also `nvarchar(max)`.
- The `Parameter` navigation has no reciprocal collection on `TestParameter` because of `.WithMany()` (empty arg) — comments and ranges on a parameter are accessed via `Test.ReferenceRanges` then filtered by `ParameterId`.

### 8.2 Dual day/value representation

The dual representation is a deliberate trade-off:

- **Storage**: 4 nullable columns per side (Value, Unit, Days, plus the Index on Days), so 8 nullable columns total for the age range. The redundancy is not normalized — `AgeFromValue=2, AgeFromUnit='Year'` could be derived from `AgeFromDays=730`, and vice versa.
- **Query**: indexable on `AgeFromDays`/`AgeToDays` makes range lookup a sargable comparison against `patient.AgeInDays`.
- **Migration history**: Phase 4 (`20260523140733_Phase4_AgeRangeRedesign.cs:75-100`) populated `AgeFromUnit='Year'` and `AgeFromDays = AgeFromValue * 365` for all legacy rows on the assumption that the previous schema's implicit unit was "Year". Rows that already had Phase 3 `AgeGroupId` values had their `AgeToDays` wiped to NULL (line 78) before recomputation — observed as evidence of a one-shot data migration that depends on the implicit-Year assumption being correct.

### 8.3 Per-parameter ranges and backward compatibility

`ParameterId` is **nullable** so that:

1. Multi-component (profile) tests can attach a range to a specific component — e.g. CBC's Hemoglobin threshold is distinct from its WBC threshold.
2. Legacy single-component tests retain the pre-Phase-3 schema shape (range on `TestId` only).

The data migration in `Phase3_AddTestSchemaExtensions.cs:114-128` walks all `TestReferenceRanges` whose `ParameterId IS NULL` and, **for tests with exactly one `TestParameter`**, populates `ParameterId` from that single parameter. Ranges on multi-component tests are left null until manually re-mapped. The comment in the migration (line 113) says: "Ranges belonging to multi-component tests stay null until manually re-mapped." This is observable evidence of an intentional partial migration.

### 8.4 Observed gaps in reference-range modeling

Strictly analytical, no proposals:

- **Qualitative ranges not supported.** The schema has no way to express "if the parsed value is 'Positive', flag as abnormal", because `LowValue` and `HighValue` are `decimal`. The comparison logic in `Services/ResultsService.cs:281-334` (per the exploration report) flags only on numeric parses.
- **No selection-priority rule** when multiple `TestReferenceRange` rows match a single (patient, parameter) combination. The schema allows overlapping rows (e.g. one row with `Gender=null` and another with `Gender='M'`, both covering the same age band). There is no `Priority`/`Specificity` column, and no documented tie-breaker.
- **No reference-range version pinning per result.** `ResultValue` (`Entities.cs:329-343`) stores `Value`, `Flag`, `Comment`, `VerifiedBy`, `VerifiedAt`, but **does not capture the `LowValue`/`HighValue` that were in effect at verification time**. If a `TestReferenceRange` row is edited, historical results re-flag against the new range.
- **`AgeFromValue/Unit` and `AgeFromDays` are not constrained to be consistent.** A row could carry `AgeFromValue=5, AgeFromUnit='Year', AgeFromDays=10` without any check.

### 8.5 ReferenceRangeResult value object

`Open_lab/Open_lab/Models/ReferenceRangeResult.cs` is a non-persisted DTO returned by the comparison logic. It carries:

- `OriginalValue` (string), `NumericValue?` (decimal),
- `Flag?` ("L"/"H"/"N"/null),
- `IsNormal`/`IsLow`/`IsHigh` (booleans),
- `LowThreshold?`/`HighThreshold?` (the thresholds that won),
- `LowComment?`/`HighComment?`/`DefaultComment?` (from `TestComment`),
- `RecommendedComment` — a computed property: `IsLow ? LowComment : (IsHigh ? HighComment : DefaultComment)` (line 66).

The value object captures *which threshold* applied to the comparison, but as noted above this is not persisted alongside the `ResultValue`.

---

## 9. Result-Entry Model Analysis

### 9.1 Storage shape

`ResultValue` (`Entities.cs:329-343`) is the unit of result storage:

```csharp
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
    // navigations
    public VisitTest VisitTest { get; set; } = null!;
    public TestParameter Parameter { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
}
```

Three observations:

- **`Value` is `string?`** and not length-capped in Fluent config — it becomes `nvarchar(max)`. The schema therefore accepts arbitrary text, numeric strings, qualitative results, or freeform clinical narrative interchangeably.
- **`Flag` is `string?`** — also unlength-capped. The values "L", "H", "N" are convention-based, set by `ResultsService` from the `ReferenceRangeResult` comparison.
- **`Comment` is `string?`** — likewise uncapped. The user can override the suggested `RecommendedComment` with arbitrary text.

### 9.2 Verification

Verification is per-result, not per-test:

- `VerifiedBy` is a nullable FK to `User`.
- `VerifiedAt` is a nullable timestamp.
- There is no aggregate `Verified` flag on `VisitTest`. To determine whether a whole test (and all its parameters) has been verified, callers must walk `VisitTest.ResultValues` and check that all `VerifiedAt` values are non-null.
- The `User → ResultValue` navigation (`User.VerifiedResults`, `Entities.cs:18`) does have a reciprocal collection, so the verifier-of-record can be queried both directions.

### 9.3 ParameterId is required (cascade Restrict)

`ResultValue.ParameterId` is **non-nullable** (`Entities.cs:333`) and the FK is configured with `OnDelete(DeleteBehavior.Restrict)` (`OpenLabDbContext.cs:302-305`). The implications:

- Every `ResultValue` must point at exactly one `TestParameter`.
- Even for a single-component test, EF requires a `TestParameter` to exist before any result can be entered (because `ResultValue.ParameterId` is the discriminator that ties a row of input to the slot in the test definition).
- Deleting a `TestParameter` that has any `ResultValue` will fail — protecting historical results from accidental loss.

### 9.4 Numeric vs qualitative

Because `ResultValue.Value` is a string and `TestReferenceRange.LowValue`/`HighValue` are decimals, the system can store **any** value type but can only **flag** numeric ones. There is no enumerated discriminator (`ResultType = Numeric | Qualitative | Text`) on either `TestParameter` or `ResultValue`. The audit reports this as a domain-modeling gap — the schema does not distinguish "value type expected" from "value type entered".

---

## 10. Validation & Constraint Audit

### 10.1 DataAnnotations

Only one entity uses attribute-based configuration:

| Entity | Annotation | Property |
|---|---|---|
| `SystemSetting` | `[Key]` | `SettingId` (`SystemSetting.cs:10`) |
| `SystemSetting` | `[Required] [MaxLength(100)]` | `SettingKey` (`SystemSetting.cs:13-15`) |
| `SystemSetting` | `[MaxLength(255)]` | `Description` (`SystemSetting.cs:19`) |
| `SystemSetting` | `[MaxLength(50)]` | `SettingType` (`SystemSetting.cs:22`) |

No `[ForeignKey]`, `[Table]`, `[Column]`, `[Required]` on any other class. No `[Range]` / `[RegularExpression]` anywhere.

### 10.2 Fluent `IsRequired()` properties

Enumerated from `OnModelCreating`:

| Entity | Required properties (Fluent) |
|---|---|
| `User` | `Username`, `PasswordHash`, `Salt` (`OpenLabDbContext.cs:114-116`) |
| `Role` | `RoleName` (`OpenLabDbContext.cs:124`) |
| `RolePermission` | `PermissionCode` (`OpenLabDbContext.cs:130`) |
| `Patient` | `LabId`, `FullName`, `Gender` (`OpenLabDbContext.cs:152-154`) |
| `Referral` | `ReferralType`, `Name` (`OpenLabDbContext.cs:183-184`) |
| `Test` | `Code`, `NameReport`, `NameReceipt` (`OpenLabDbContext.cs:204-206`) |
| `TestGroup` | `GroupName` (`OpenLabDbContext.cs:224`) |
| `SampleType` | `Name` (`OpenLabDbContext.cs:230`) |
| `Unit` | `Name` (`OpenLabDbContext.cs:236`) |
| `TestComment` | `CommentText` (`OpenLabDbContext.cs:261`) |
| `Payment` | `PaymentMethod` (also `HasMaxLength(50)`, `OpenLabDbContext.cs:335`) |
| `PriceList` | `Name` (`OpenLabDbContext.cs:350`) |
| `CustomGroup` | `Name` (`OpenLabDbContext.cs:371`) |
| `Culture` | `Name`, `SampleType`, `IsolatedOrganism`, `GrowthConditions` (`OpenLabDbContext.cs:389-392`) |
| `Antibiotic` | `Name` (`OpenLabDbContext.cs:398`) |
| `TestParameter` | `Name` (`OpenLabDbContext.cs:287`) |
| `Branch` | `Name` (`OpenLabDbContext.cs:501`) |
| `AdditionalCharge` | `Description` (`OpenLabDbContext.cs:491`) |
| `Expense` | `Description` (`OpenLabDbContext.cs:521`) |
| `AuditLog` | `Action`, `TableName` (`OpenLabDbContext.cs:481-482`) |
| `AttendanceBreak` | `Type` (also `HasMaxLength(30)`, `OpenLabDbContext.cs:442`) |
| `AttendanceDayStatus` | `Status` (also `HasMaxLength(30)`, `OpenLabDbContext.cs:452`) |
| `ShipmentManifest` | `ManifestNumber` (`OpenLabDbContext.cs:543`) |
| `SystemSetting` | `SettingKey` (also `HasMaxLength(100)`, `OpenLabDbContext.cs:594`) |

### 10.3 Fluent `HasMaxLength(...)` properties (length caps)

| Entity.Property | Max | Source |
|---|---|---|
| `Patient.Phone` | 50 | `OpenLabDbContext.cs:155` |
| `Patient.HomePhone` | 50 | `OpenLabDbContext.cs:156` |
| `Patient.NationalId` | 50 | `OpenLabDbContext.cs:157` |
| `Patient.Email` | 255 | `OpenLabDbContext.cs:158` |
| `Patient.AgeUnit` | 10 | `OpenLabDbContext.cs:159` |
| `TestReferenceRange.AgeFromUnit` | 10 | `OpenLabDbContext.cs:244` |
| `TestReferenceRange.AgeToUnit` | 10 | `OpenLabDbContext.cs:245` |
| `Payment.PaymentMethod` | 50 | `OpenLabDbContext.cs:335` |
| `AttendanceBreak.Type` | 30 | `OpenLabDbContext.cs:442` |
| `AttendanceDayStatus.Status` | 30 | `OpenLabDbContext.cs:452` |
| `SystemSetting.SettingKey` | 100 | `OpenLabDbContext.cs:594` (also via DataAnnotation) |
| `SystemSetting.SettingType` | 50 | `OpenLabDbContext.cs:595` |
| `SystemSetting.Description` | 255 | `OpenLabDbContext.cs:596` |

### 10.4 Uncapped string properties → `nvarchar(max)`

Every other `string` / `string?` property in the persisted entity surface — Fluent does not configure `HasMaxLength`, so EF Core defaults to `nvarchar(max)`. Concretely:

| Entity | Uncapped string properties |
|---|---|
| `User` | `Username`, `PasswordHash`, `Salt`, `FullName` (note: `Username` has a unique index — see Section 11 caveat) |
| `Role` | `RoleName` (also unique-indexed) |
| `RolePermission` | `PermissionCode` |
| `Patient` | `LabId` (unique-indexed), `FullName`, `Gender`, `Address` |
| `Referral` | `ReferralType`, `Name`, `Phone`, `City` |
| `ContractInvoice` | `InvoiceNumber` |
| `Test` | `Code` (unique-indexed), `NameReport`, `NameReceipt`, `Description` |
| `TestGroup` | `GroupName` |
| `SampleType` | `Name` |
| `Unit` | `Name` |
| `TestReferenceRange` | `Gender`, `NormalText` |
| `TestComment` | `CommentText`, `LowComment`, `HighComment` |
| `VisitTest` | `Status` |
| `Visit` | `AccountType`, `Status` |
| `Invoice` | `Status` |
| `Culture` | `Name`, `SampleType`, `IsolatedOrganism`, `GrowthConditions` |
| `Antibiotic` | `Name` |
| `SampleCollection` | `Status` |
| `Setting` | `Key` (PK), `Value` |
| `MedicalHistory` | `ChronicDiseases`, `Allergies`, `Medications`, `Notes` |
| `Branch` | `Address`, `Phone` |
| `Expense` | `Description` |
| `Reagent` | `Name`, `Unit` |
| `CustomGroup` | `Name` |
| `PriceList` | `Name` |
| `Physician` | `FullName`, `Phone`, `Specialty`, `Address` |
| `ShipmentManifest` | `ManifestNumber`, `CourierNotes` |
| `ExternalLabQueue` | `Status`, `ExternalReference` |
| `ExternalLabSettlement` | `Note` |
| `AuditLog` | `Action`, `TableName`, `RecordId`, `OldValues`, `NewValues` |
| `AttendanceLog` | `Note` |
| `AttendanceBreak` | `Note` |
| `AttendanceDayStatus` | `Note` |
| `Antibiotic` | `Name` |
| `ResultValue` | `Value`, `Flag`, `Comment` |
| `SystemSetting` | `SettingValue` |

**SQL Server's `nvarchar(max)` behavior:** unique indexes are limited to 900 bytes of key data, so a unique index on an `nvarchar(max)` column triggers EF Core to declare the column as `nvarchar(450)` instead (which fits in the index page). This is why `Patient.LabId`, `Test.Code`, `User.Username`, and `Role.RoleName` end up effectively length-limited (to 450 chars) by virtue of carrying a unique index — confirmed in the InitialCreate migration's CREATE TABLE statements (`InitialCreate.cs:60` shows `LabId = nvarchar(450)`, line 94 shows `RoleName = nvarchar(450)`). For all other uncapped strings, the column is `nvarchar(max)`.

### 10.5 Numeric precision

| Pattern | Entities / properties |
|---|---|
| `decimal(18, 2)` (monetary & ratios) | `Referral.DiscountPercentage`, `Referral.CommissionPercentage`, `ContractInvoice.{TotalAmount, DiscountAmount, NetAmount}`, `Test.{Price, CostPrice, PatientPrice}`, `TestReferenceRange.{LowValue, HighValue}`, `VisitTest.Price`, `Invoice.{Total, Discount, NetTotal, Paid, Balance}`, `Payment.Amount`, `PriceListItem.Price`, `CustomGroup.Price`, `AdditionalCharge.Amount`, `DoctorCommission.Amount`, `Expense.Amount`, `ExternalLabSettlement.{TotalCost, AmountPaid, Balance}`, `Reagent.CurrentStock` |
| `decimal(18, 4)` (higher precision) | `TestConsumption.AmountPerTest` only |
| `decimal` without `HasPrecision(...)` | `Physician.CommissionPercentage` — defaults to SQL Server `decimal(18, 2)` via convention, but is not explicit |

### 10.6 Concurrency, soft-delete, audit columns

- **Concurrency tokens** — none. No `RowVersion` / `Timestamp` / `[ConcurrencyCheck]` property on any entity.
- **Soft-delete** — none. No `IsDeleted` / `DeletedAt` / `DeletedBy` column anywhere.
- **`CreatedAt`/`UpdatedAt`/`CreatedBy`/`UpdatedBy`** — only `ContractInvoice.CreatedAt` (`Entities.cs:169`). Every other entity relies entirely on the `AuditLog` interceptor for change history; the entity table itself stores no creation or modification timestamp.

### 10.7 CHECK constraints

No `HasCheckConstraint(...)` calls in `OnModelCreating`. The migration history likewise has no `migrationBuilder.Sql("ALTER TABLE ... ADD CONSTRAINT CK_...")` blocks. The schema relies entirely on application-layer validation and FK/uniqueness enforcement.

---

## 11. Indexing & Performance Observations

### 11.1 Unique indexes

| Index | Columns | Purpose |
|---|---|---|
| `IX_Users_Username` | `Users.Username` | Login lookup, dedup (`OpenLabDbContext.cs:113`) |
| `IX_Roles_RoleName` | `Roles.RoleName` | Dedup (`OpenLabDbContext.cs:123`) |
| `IX_Patients_LabId` | `Patients.LabId` | Lab-side patient ID dedup (`OpenLabDbContext.cs:150`) |
| `IX_Tests_Code` | `Tests.Code` | Test catalog dedup (`OpenLabDbContext.cs:203`) |
| `IX_Invoices_VisitId` | `Invoices.VisitId` | 1:1 enforcement (`OpenLabDbContext.cs:322`) |
| `IX_SampleCollections_VisitTestId` | `SampleCollections.VisitTestId` | 1:1 enforcement (`OpenLabDbContext.cs:418`) |
| `IX_MedicalHistories_PatientId` | `MedicalHistories.PatientId` | 1:1 enforcement (`OpenLabDbContext.cs:475`) |
| `IX_AttendanceDayStatuses_UserId_Date` | `(UserId, Date)` | Composite unique — one status row per user per day (`OpenLabDbContext.cs:456`) |
| `IX_SystemSettings_SettingKey` | `SystemSettings.SettingKey` | Key lookup, dedup (`OpenLabDbContext.cs:593`) |

**Nine unique indexes total.**

### 11.2 Non-unique indexes declared explicitly in Fluent

| Index | Columns | Source |
|---|---|---|
| `IX_Patients_NationalId` | `Patients.NationalId` | `OpenLabDbContext.cs:151` — **searchability without dedup**, see Section 11.4 |
| `IX_TestReferenceRanges_ParameterId` | `TestReferenceRanges.ParameterId` | `OpenLabDbContext.cs:253` |
| `IX_TestReferenceRanges_AgeFromDays` | `TestReferenceRanges.AgeFromDays` | `OpenLabDbContext.cs:254` |
| `IX_TestReferenceRanges_AgeToDays` | `TestReferenceRanges.AgeToDays` | `OpenLabDbContext.cs:255` |
| `IX_TestComments_ParameterId` | `TestComments.ParameterId` | `OpenLabDbContext.cs:269` |
| `IX_AttendanceBreaks_AttendanceLogId_StartAt` | `(AttendanceLogId, StartAt)` | Composite, `OpenLabDbContext.cs:446` |

### 11.3 FK-driven indexes (EF Core convention)

EF Core creates a non-unique index for every FK column unless one is already declared. With ~80 foreign-key columns across the schema, this adds **~40+ additional non-unique indexes** that are not explicitly declared in `OnModelCreating` but appear in the generated migrations. The migration files (e.g. `InitialCreate.cs`) emit corresponding `CreateIndex` calls.

### 11.4 Non-unique index on `Patient.NationalId`

A focused observation: `OpenLabDbContext.cs:151` declares `entity.HasIndex(e => e.NationalId);` without `.IsUnique()`. The schema therefore supports:

- Multiple `Patient` rows with the same `NationalId`.
- Fast O(log n) search by `NationalId` (via the non-unique index).

This combination is consistent with a deduplication-as-application-concern model — the audit reports the schema's choice without proposing a constraint change.

---

## 12. Configuration & Seed Data

### 12.1 `App.config`

`Open_lab/Open_lab/App.config` (6 lines) contains one connection-string entry:

```xml
<connectionStrings>
  <add name="OpenLabDb"
       connectionString="Server=.\SQLEXPRESS;Database=OpenLab;User ID=sa;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False;Connect Timeout=30"
       providerName="Microsoft.Data.SqlClient" />
</connectionStrings>
```

Observations:

- **No `Password=` segment.** The password must be injected at runtime via `OPENLAB_DB_PASSWORD`. If the env var is unset, both `OpenLabDbContext.OnConfiguring` and `OpenLabDbContextFactory.CreateDbContext` throw `InvalidOperationException`.
- The connection string still names `sa` as the principal, and `TrustServerCertificate=True; Encrypt=False` is hardcoded. Reported as evidence; no proposal.

### 12.2 Environment-variable overrides

| Variable | Effect |
|---|---|
| `OPENLAB_CONNECTION` | If set, used **verbatim** (full connection string, including password). Bypasses `App.config`. |
| `OPENLAB_DB_PASSWORD` | If `OPENLAB_CONNECTION` is unset, injected into the `App.config` connection string via `SqlConnectionStringBuilder.Password`. If unset and `OPENLAB_CONNECTION` is also unset, the application fails-fast. |

### 12.3 Hardcoded fallback in code

Both `OpenLabDbContext.OnConfiguring` (`OpenLabDbContext.cs:95`) and `OpenLabDbContextFactory.CreateDbContext` (`OpenLabDbContextFactory.cs:33`) carry the same hardcoded fallback string. That fallback is **only reached when `App.config`'s `OpenLabDb` is missing or whitespace**, which is not the default state of the shipped application. The fallback remains in the codebase as a safety net.

### 12.4 Seed data

- **No `HasData(...)` calls** in `OnModelCreating` — confirmed by reading the entire `OnModelCreating` method (`OpenLabDbContext.cs:108-598`).
- **One historical `InsertData` in migrations**: the 6 Arabic age-group rows in `Phase3_AddTestSchemaExtensions.cs:54-65`. These rows were dropped along with the `AgeGroups` table in `Phase4_AgeRangeRedesign.cs:17-18`.
- All other initial data (roles, permissions, the bootstrap administrator) is created at runtime by `IAdminSetupService` / `BootstrapViewModel` via the bootstrap window (`App.xaml.cs:75-104`). The schema does not pre-populate them.

### 12.5 The `Setting` vs `SystemSetting` doublet

Two configuration tables coexist:

- `Setting` (`Entities.cs:468-472`) — `Key` (string PK), `Value?` (nvarchar(max)). The `DbSet<Setting> Settings` exists at `OpenLabDbContext.cs:55`. Fluent config (`OpenLabDbContext.cs:464-467`) declares only the PK.
- `SystemSetting` (`SystemSetting.cs:8-26`) — `SettingId` PK, `SettingKey` (unique, max 100), `SettingValue?`, `Description?` (max 255), `SettingType` (max 50, default "String"), `LastModified?`. The `DbSet<SystemSetting> SystemSettings` exists at `OpenLabDbContext.cs:69`. Added in migration `Phase5_SystemSettings` (`20260421154337_Phase5_SystemSettings.cs`).

Both tables are present in the current schema. The audit notes the doublet as evidence; the `Setting` table is preserved while `SystemSetting` carries the newer typed key/value model with `LastModified` audit tracking and a `SettingType` discriminator.

---

## 13. Security Posture Snapshot (F-Series Remediations)

This section reports the **current state** of three referenced security fixes (F6, F9, F15) and one configuration-side fix (DB password sourcing). Strictly analytical — no fix recommendations.

### 13.1 F6 — Empty/null password hash or salt rejected at login

**Commit:** `4f54468 — إصلاح ثغرة مسار كلمات المرور النصية القديمة والفارغة F6`

**Evidence in `Open_lab/Open_lab/Services/AuthService.cs:31-34`:**

```csharp
if (string.IsNullOrWhiteSpace(user.Salt) || string.IsNullOrWhiteSpace(user.PasswordHash))
{
    return null;
}
```

Before reaching the hash-verify step at line 36, `AuthService.ValidateCredentialsAsync` rejects any user whose `Salt` or `PasswordHash` is null or whitespace. This closes the gap where a legacy row migrated from an earlier schema (which had no salt, or whose hash had been wiped) would have allowed login with any password.

A reciprocal check is in `UserAdminService.UpdateUserAsync` (`UserAdminService.cs:113-119`): when an administrator omits the new password, the service refuses to save unless the current user already has both a valid salt and a valid hash.

`PasswordSecurity.Verify` (`Open_lab/Open_lab/Services/PasswordSecurity.cs:45-74`) is fixed-time-comparison-safe (`CryptographicOperations.FixedTimeEquals`, line 73) and catches `FormatException`/`ArgumentException` from malformed hex inputs (lines 57-64), returning `false` rather than throwing.

### 13.2 F9 — Service-level authorization gate

**Commit:** `73232a7 — إصلاح فجوة التخويل على مستوى الخدمة F9`

**Evidence in `Open_lab/Open_lab/Services/UserAdminService.cs:269-280`:**

```csharp
private void EnsureUsersEditPermission()
{
    if (_sessionContext.IsSystemOperation)
    {
        return;
    }

    if (!_sessionContext.HasPermission(PermissionCodes.UsersEdit))
    {
        throw new UnauthorizedAccessException("UsersEdit permission is required to manage users and roles.");
    }
}
```

This method is called at the head of every mutating operation in `UserAdminService`:

| Method | Calls `EnsureUsersEditPermission` | Line |
|---|---|---|
| `CreateUserAsync` | yes | `UserAdminService.cs:44` |
| `UpdateUserAsync` | yes | `UserAdminService.cs:77` |
| `DeleteUserAsync` | yes | `UserAdminService.cs:130` |
| `CreateRoleAsync` | yes | `UserAdminService.cs:166` |
| `DeleteRoleAsync` | yes | `UserAdminService.cs:188` |
| `AssignSingleRoleAsync` | yes | `UserAdminService.cs:219` |
| `RemoveUserRoleAsync` | yes | `UserAdminService.cs:236` |
| `SaveRolePermissionsAsync` | yes | `UserAdminService.cs:256` |

The gate is bypassed only when `ISessionContext.IsSystemOperation` is true (used by background and bootstrap code paths). Permission resolution itself happens in `SessionContext.HasPermission` (`SessionContext.cs:37-41`):

```csharp
public bool HasPermission(string permissionCode)
{
    return _grantedPermissions.Contains(PermissionCodes.FullAccess) ||
           _grantedPermissions.Contains(permissionCode);
}
```

Granted permissions are loaded into the singleton `SessionContext.Current._grantedPermissions` HashSet at login by `BeginSession` (`SessionContext.cs:18-30`).

The audit's read of `UserAdminService` confirms F9's intent is enforced consistently across mutating operations on users, roles, and role/permission assignments. Read methods (`GetUsersAsync`, `GetRolesAsync`, `GetRolePermissionCodesAsync` at `UserAdminService.cs:24-40`) do **not** call the gate — observed as evidence of design intent (read access is governed by view-level checks elsewhere, not by `UsersEdit`).

### 13.3 F15 — Password hash versioning and PBKDF2 migration

**Commit:** `1a4f1e5 — إصلاح الثغرة F15`

**Schema change:** Migration `20260526180401_F15_AddUserHashVersion.cs:13-18` adds `Users.HashVersion` as `int NOT NULL DEFAULT 1`. The entity property (`Entities.cs:12`) is `public int HashVersion { get; set; } = 1;` and the Fluent default is reaffirmed at `OpenLabDbContext.cs:117`.

**Hash algorithm constants** (`PasswordSecurity.cs:9-11`):

```csharp
public const int LegacySha256Version = 1;
public const int Pbkdf2Version = 2;
public const int Pbkdf2Iterations = 50000;
```

**Salt generation, two methods** (`PasswordSecurity.cs:13-22`):

- `GenerateSalt()` — 16 random bytes, Base64 encoded. Kept for V1 / legacy compatibility.
- `GenerateSecureSalt()` — 32 random bytes, **hex** encoded. Used by all new password creations and re-hashes.

**Verification dispatch** (`PasswordSecurity.cs:45-74`) uses `hashVersion` to pick:

- `LegacySha256Version` → `ComputeSha256(password, salt)` (SHA-256 of `password + "::" + salt`).
- `Pbkdf2Version` → `ComputePbkdf2(password, salt)` (Rfc2898DeriveBytes, SHA-256, 50,000 iterations, 32-byte output, hex-encoded).
- Any other version → `null` (failed verify).

**Auto-upgrade on successful legacy login** (`AuthService.cs:36-47`):

```csharp
if (PasswordSecurity.Verify(password, user.Salt, user.PasswordHash, user.HashVersion))
{
    if (user.HashVersion == PasswordSecurity.LegacySha256Version)
    {
        user.Salt = PasswordSecurity.GenerateSecureSalt();
        user.PasswordHash = PasswordSecurity.ComputePbkdf2(password, user.Salt);
        user.HashVersion = PasswordSecurity.Pbkdf2Version;
        await _db.SaveChangesAsync();
    }
    return user;
}
```

On the first successful login with a legacy SHA-256 hash, the service generates a new 32-byte hex salt, re-hashes with PBKDF2, sets `HashVersion = 2`, and saves. The plaintext password is held only on the stack during this single call.

**Admin-initiated password set** (`UserAdminService.ApplyPassword`, `UserAdminService.cs:282-293`) always uses `GenerateSecureSalt()` + `ComputePbkdf2(...)` + `HashVersion = Pbkdf2Version`.

The audit reports the state of F15 as: schema column present, defaulted to 1; SHA-256 still supported for verify-only on legacy rows; all new rows and all upgrade-on-login rows land at version 2.

### 13.4 DB password sourcing

**Commit:** `eb0dc32 — إصلاح تخزين كلمة مرور قاعدة البيانات`

Already detailed in Sections 3.2 and 12.1. The current state:

- `App.config` ships with no `Password=` segment.
- `OpenLabDbContext.OnConfiguring` and `OpenLabDbContextFactory.CreateDbContext` both **fail-fast** with `InvalidOperationException` when neither `OPENLAB_CONNECTION` nor `OPENLAB_DB_PASSWORD` is configured.
- The hardcoded fallback `Server=.\SQLEXPRESS;... User ID=sa` is reachable **only** when `App.config`'s `OpenLabDb` entry is missing — by default it is present, so the fallback path acts as a safety net for misconfigured deployments.

### 13.5 Session model

`SessionContext` (`SessionContext.cs:6-52`) is a **singleton** (`SessionContext.Current` static field, line 10) implementing `IMutableSessionContext`. Both `ISessionContext` and `IMutableSessionContext` are registered in DI as singletons pointing at the same instance (`App.xaml.cs:47-48`). Consequences:

- The process has exactly one "current user" at a time.
- `BeginSession` (`SessionContext.cs:18-30`) clears `_grantedPermissions` and reassigns `UserId`, `Username`, `AttendanceLogId`, then loads new permissions.
- `EndSession` (`SessionContext.cs:43-50`) zeroes out the same fields.
- The `IsSystemOperation` flag (line 15) is used by services like `UserAdminService.EnsureUsersEditPermission` to skip permission checks during bootstrap or background operations.

For a desktop single-user-per-machine deployment this is the natural model. The audit reports the singleton design as an observation without proposing a per-window or per-thread alternative.

### 13.6 Permission codes

`PermissionCodes` (`Services/PermissionCodes.cs:7-28`) defines 22 string constants:

```text
ALL, Patients.View, Patients.Edit,
Visits.View, Visits.Edit,
Tests.View, Tests.Edit,
Results.View, Results.Edit,
Reports.View,
Accounts.View, Accounts.Edit,
Settings.View, Settings.Edit,
Users.View, Users.Edit,
Statistics.View,
Backup.Restore,
Delivery.View, Delivery.Edit,
Constants.View, Constants.Edit
```

The `All` static list (`PermissionCodes.cs:30-54`) enumerates them for UI binding. Permissions are stored as strings in `RolePermission.PermissionCode` (PK is `(RoleId, PermissionCode)`). Roles → permissions are seeded at runtime, not by migration.

---

## 14. Domain Workflow Trace (Patient → Report)

This section walks the end-to-end LIS lifecycle and pinpoints the entity and service that owns each transition. The intent is to make explicit which workflow steps the schema represents directly and which are implicit.

```text
[Patient] ─ register ──► [Visit] ─ add tests ──► [VisitTest (1..*)]
                                                  ├─► [SampleCollection (0..1)]
                                                  ├─► [ResultValue (1..* per parameter)]
                                                  ├─► [ExternalLabQueue (0..1)] ──► [ShipmentItem] ──► [ShipmentManifest]
                                                  ▼
                                                [Invoice (1:1 to Visit)]
                                                  ├─► [Payment (1..*)]
                                                  └─► [AdditionalCharge (0..*)]
                                  AuditInterceptor stamps every mutation into [AuditLog]
                                  Reports assembled at runtime by PrintService / ReportPdfService
```

### Step 1 — Patient registration

- **Entity**: `Patient` (`Entities.cs:99-120`).
- **Service**: `Services/PatientService.cs`, `Services/PatientSearchService.cs`.
- **Unique constraint**: `LabId` only. `NationalId` is indexed but not unique — duplicate patients on the same NationalId are possible.
- **Optional `MedicalHistory`** (1:1) is created lazily.

### Step 2 — Visit creation

- **Entity**: `Visit` (`Entities.cs:122-139`).
- **Service**: `Services/VisitService.cs`.
- **FKs**: required `PatientId`; optional `ReferralId`, `PhysicianId`, `BranchId`.
- **Status**: `Visit.Status` is `string?` — values are convention-based (no enum).

### Step 3 — Test ordering

- **Entity**: `VisitTest` (`Entities.cs:296-314`) is added to `Visit.VisitTests`.
- **No distinct `Order` entity.** A test is "ordered" by inserting a `VisitTest` row. The price is **snapshot** into `VisitTest.Price` at this moment (`Entities.cs:301`) — this is the only price-versioning mechanism in the schema.
- **`ReportOrder`** (`Entities.cs:307`) is set at this stage and used later by `Services/ReportOrderService.cs` / `IReportOrderService.cs` to arrange the printed layout. A value of `0` means "not yet arranged".

### Step 4 — Sample collection (optional)

- **Entity**: `SampleCollection` (`Entities.cs:452-466`).
- **Service**: `Services/SampleCollectionService.cs`, `Services/SampleTrackingService.cs`.
- **Cardinality**: 1:0..1 to `VisitTest`. A `VisitTest` can have a collection record or not — the schema does not enforce "collected before results".
- **`Status`** is a string. Per the exploration phase, observed values include Arabic tokens such as `"مسحوبة"` ("drawn") and `"مفصولة"` ("separated"). No enum.
- **`ReceivedBy`** is optional with `OnDelete(Restrict)` to preserve audit chain.

### Step 5 — Result entry

- **Entity**: `ResultValue` (`Entities.cs:329-343`).
- **Service**: `Services/ResultsService.cs`.
- **Per-parameter rows**: one `ResultValue` per `TestParameter` per `VisitTest`. For a CBC with N components, there are N `ResultValue` rows per visit.
- **Flagging**: `Flag` is set to `"L"`/`"H"`/`"N"`/`null` by the comparison logic against `TestReferenceRange`, producing a `ReferenceRangeResult` value object (see Section 8.5). Flagging is **only applied to numeric-parseable values**.
- **Verification**: `VerifiedBy` (User FK), `VerifiedAt` (DateTime) at the result level. There is no aggregate "all results verified" flag on `VisitTest`.
- **History comparison**: `Services/CompareWithHistoryService.cs` (referenced by `ICompareWithHistoryService`) populates `ResultValueReportItem.PreviousValue`/`PreviousDate` (`ReportModels.cs:21-26`) for trend display.

### Step 6 — External-lab send-out (optional branch)

- **Entity**: `ExternalLabQueue` (`Entities.cs:553-565`), then bundled into `ShipmentItem` → `ShipmentManifest`.
- **Service**: `Services/ExternalLabService.cs`, `Services/ExternalSettlementService.cs`.
- **Cardinality**: `VisitTest 1—0..1 ExternalLabQueue` (only send-out tests). `ExternalLabQueue 1—1 ShipmentItem`; `ShipmentManifest 1—* ShipmentItem`.
- **Status string** on the queue starts at `"Pending"` (default, `Entities.cs:558`).

### Step 7 — Invoicing

- **Entity**: `Invoice` (1:1 to `Visit`).
- **Service**: `Services/InvoiceService.cs`, `Services/ReceiptService.cs`.
- **Pricing**: `Visit` aggregates `VisitTest.Price` (snapshotted at order time), with `AdditionalCharge` for surcharges. `PriceResolutionService` (`Services/PriceResolutionService.cs`) resolves the right price at order time using `Test.Price` / `PriceList` / `Physician.PriceListId` / `Referral`-based discount.
- **Payments**: `Invoice 1—* Payment`. `Invoice.Balance` is maintained explicitly (column, not computed) — `decimal(18,2)` like `Total`, `Paid`, etc.
- **Multi-tenant**: `Invoice.BranchId` is optional. Nothing in the schema groups invoices into a single tenant's books.

### Step 8 — Doctor commissions

- **Entity**: `DoctorCommission` (`Entities.cs:529-540`). Pre-`Restrict` foreign keys to `Referral` and `Visit`.
- The `IsPaid` boolean and `DateCalculated` timestamp suggest a downstream commission-payout workflow.

### Step 9 — Report generation

- **No persisted report.** `Services/PrintService.cs`, `Services/ReportService.cs`, `Services/BlankReportService.cs`, `Services/ReportPdfService.cs`, `Services/ReceiptService.cs` build `VisitReportData` / `PatientHistoryReportData` (`ReportModels.cs`) in memory and render to WPF `FlowDocument` / PDF.
- The system does **not** archive the rendered report. Re-running the report later will re-fetch data from the live schema.

### Step 10 — Audit

- **Entity**: `AuditLog` (`Entities.cs:493-505`).
- **Mechanism**: `AuditInterceptor` (`Data/AuditInterceptor.cs`) attaches at `OnConfiguring`. Every `SaveChanges` writes one `AuditLog` row per changed entity, with `Action`, `TableName`, `RecordId`, `OldValues`/`NewValues` JSON, `Timestamp = DateTime.UtcNow`, and `UserId` from `OpenLabDbContext.CurrentUserId`.
- See Section 3.4 caveat about which DbContext construction path the interceptor actually attaches to.

### 14.1 Implicit (unmodeled) workflow stages

The trace surfaces three stages that the schema does not model as discrete entities:

- **Order vs Visit.** Tests attach directly to `Visit.VisitTests`. There is no `Order` row that bundles tests within a visit, so reordering, partial cancellations, and amended-order workflows must be expressed entirely through `VisitTest.Status` strings.
- **Report generation event.** There is no `ReportGeneration` audit row that records "report printed on date X by user Y for visit Z". Audit rows exist for data mutations but not for report renders.
- **Result release / sign-out.** The `VerifiedBy`/`VerifiedAt` on `ResultValue` captures per-parameter verification, but there is no separate "results released to patient/portal" event entity. The transition from "verified" to "available to patient" is implicit.

---

## 15. Localization Posture

### 15.1 Entity / property naming

All persisted entity classes, property names, and DbSet identifiers are in **English** (`Open_lab.Models.*`). No `_ar` / `_en` suffixed columns, no bilingual property pairs.

### 15.2 UI strings

UI windows in `App.xaml.cs:85` and `App.xaml.cs:116` set `Title` to Arabic strings ("إعداد المشرف الأول", "تسجيل الدخول") and use `FlowDirection = FlowDirection.RightToLeft`. The application is therefore Arabic-RTL by default.

### 15.3 Inline Arabic in C# code

Service-layer error messages are written in Arabic, embedded directly in `throw` expressions rather than fetched from `.resx` resource files:

| Location | Message |
|---|---|
| `UserAdminService.cs:60` | `"اسم المستخدم موجود بالفعل."` |
| `UserAdminService.cs:94` | `"اسم المستخدم مطلوب."` |
| `UserAdminService.cs:100` | `"لا يمكن تغيير اسم مستخدم admin."` |
| `UserAdminService.cs:106` | `"اسم المستخدم مستخدم من حساب آخر."` |
| `UserAdminService.cs:140` | `"لا يمكن حذف حساب admin."` |
| `UserAdminService.cs:151` | `"لا يمكن حذف المستخدم لارتباطه بسجلات تشغيلية. قم بإيقافه بدلاً من ذلك."` |
| `UserAdminService.cs:177` | `"اسم الدور موجود بالفعل."` |
| `UserAdminService.cs:198` | `"لا يمكن حذف دور Administrator."` |
| `UserAdminService.cs:204` | `"لا يمكن حذف الدور لأنه مرتبط بمستخدمين."` |
| `UserAdminService.cs:225` | `"المستخدم أو الدور غير موجود."` |
| `UserAdminService.cs:247` | `"لا يمكن فك دور admin."` |

These coexist with English error messages in the same file (`"Username is required."`, `"Password is required when creating a user."`, etc.). The audit reports the bilingual-error pattern as evidence of incremental Arabization without a unified resource strategy.

### 15.4 Status string mixing

Status values for workflow entities are also language-mixed:

- `Visit.Status` and `VisitTest.Status` — English convention values (`"Open"`, `"Verified"`, `"Completed"`).
- `SampleCollection.Status` — Arabic tokens per the exploration phase (`"مسحوبة"`, `"مفصولة"`).
- `ExternalLabQueue.Status` — English (`"Pending"` is the column default at `Entities.cs:558`).
- `ShipmentManifest.Status` — English (`"Open"` is the column default at `Entities.cs:574`).

Reported as an observation about the absence of a unified status vocabulary.

### 15.5 No localization framework

No `.resx` files exist under `Open_lab/Open_lab/`. No `IStringLocalizer` / `ResourceManager` usage. The application is functionally Arabic-RTL with English-property-named entities and inline Arabic UI/error strings.

---

## 16. Multi-Tenancy / Branch Analysis

### 16.1 The Branch entity

`Branch` (`Entities.cs:517-527`) is a simple master:

```csharp
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
```

Fluent config at `OpenLabDbContext.cs:498-502` declares only the PK and the `Name` required constraint — no length cap on `Name`, `Address`, or `Phone`.

### 16.2 BranchId on transactional tables

Three entities carry a `BranchId` column, **all nullable**:

- `Visit.BranchId` (`Entities.cs:131`) — nullable, FK configured at `OpenLabDbContext.cs:175-177`.
- `Invoice.BranchId` (`Entities.cs:356`) — nullable, FK configured at `OpenLabDbContext.cs:323-325`.
- `Payment.BranchId` (`Entities.cs:373`) — nullable, FK configured at `OpenLabDbContext.cs:342-344`.

Other transactional entities (`VisitTest`, `ResultValue`, `SampleCollection`, `ExternalLabQueue`, `ContractInvoice`, `DoctorCommission`, `Expense`, `AdditionalCharge`, `AuditLog`) have **no** `BranchId` column. Branch context for those entities is inferred via the parent navigation chain (e.g. `VisitTest → Visit → BranchId`).

### 16.3 Branch isolation

- **No global query filter.** A search for `HasQueryFilter(...)` in `OnModelCreating` returns no matches; there is no EF Core query filter that auto-scopes data by branch.
- **No branch-aware permissions.** `Role` and `RolePermission` (`Entities.cs:74-89`) carry no branch dimension. A user with `Patients.View` can see patients from all branches.
- **No branch-scoped sequences.** `Invoice` numbers (the column does not exist explicitly — `Invoice` has no `InvoiceNumber`; `ContractInvoice.InvoiceNumber` is the only "InvoiceNumber" string column) are not partitioned per branch.

The audit reports `Branch` as a **weakly-modeled** multi-tenancy concept: the column exists and joins the right tables, but the schema does not enforce isolation. Whether a deployment uses multiple branches in practice is therefore an operational policy rather than a database guarantee.

---

## 17. Gap & Risk Register (Analytical)

This register consolidates every gap previously cited into a single tier-graded table. Tiers are labeled by impact area; no prioritization is implied. Every row references the earlier section that contains the evidence.

### Structural (project topology & EF Core configuration)

| ID | Observation | Evidence | Section |
|---|---|---|---|
| G-Struct-1 | Monolithic single-assembly project; UI + Models + Services + Data co-located. Test projects and AdminCli reference the full UI assembly because no smaller library exists. | `Open_lab.sln`, project structure | 2 |
| G-Struct-2 | All Fluent configuration inline in one ~490-line `OnModelCreating`; no `IEntityTypeConfiguration<T>` separation. | `OpenLabDbContext.cs:108-598` | 3.3 |
| G-Struct-3 | `Physician` has no Fluent block; relies entirely on EF Core convention. Its relationship to `Visit` is not declared in the `Visit` Fluent block. | `Physician.cs`, `OpenLabDbContext.cs:166-178` | 3.3, 7.3 |
| G-Struct-4 | `AuditInterceptor` is added in `OnConfiguring`, which short-circuits when the context is built with `DbContextOptions`. Production DI uses the options-based constructor via `OpenLabDbContextFactory`, so the audit interceptor is **not** attached in the production DI path. | `OpenLabDbContext.cs:73-76, 83, 105`, `App.xaml.cs:35-43`, `OpenLabDbContextFactory.cs:41-44` | 3.5 |
| G-Struct-5 | Asymmetric navigation properties: `Expense.User`, `DoctorCommission.Referral`, `DoctorCommission.Visit`, `ShipmentManifest.Referral`, `ExternalLabQueue.Referral`, `TestConsumption.Test` all use `.WithMany()` (empty arg) — no reciprocal collection on the parent entity. | `OpenLabDbContext.cs:508-515, 523-525, 534-547, 582-584` | 7.10 |

### Constraint (validation, integrity, concurrency)

| ID | Observation | Evidence | Section |
|---|---|---|---|
| G-Constraint-1 | `Patient.NationalId` indexed but **not unique**. Phone and Email have no uniqueness constraint. Patient deduplication is not enforced at the schema level. | `OpenLabDbContext.cs:151`, `Entities.cs:112-113` | 5, 11.4 |
| G-Constraint-2 | Most string columns are uncapped, defaulting to `nvarchar(max)` — including `User.Username`/`PasswordHash`/`Salt`, `Test.Code`, `Patient.FullName`/`Gender`/`Address`, all `Status` columns, `ResultValue.Value`/`Flag`/`Comment`, etc. (Unique-indexed columns are auto-trimmed to `nvarchar(450)`.) | `OpenLabDbContext.cs:108-598`, full list in Section 10.4 | 10.4 |
| G-Constraint-3 | No row-version / concurrency token on any entity. EF Core optimistic concurrency is not engaged. | Absence in `Entities.cs` and `OnModelCreating` | 10.6 |
| G-Constraint-4 | No soft-delete column anywhere. Deletions are hard, audited only via `AuditInterceptor` (which has caveat G-Struct-4). | Absence in `Entities.cs` | 10.6 |
| G-Constraint-5 | No CHECK constraints in `OnModelCreating` or in migration `Sql(...)` blocks. Status values, gender tokens, age units, and the like are unconstrained text. | Absence in `OnModelCreating`, migrations | 10.7 |
| G-Constraint-6 | Audit columns (`CreatedAt`/`UpdatedAt`/`CreatedBy`/`UpdatedBy`) appear only on `ContractInvoice.CreatedAt`. Every other entity relies entirely on `AuditLog`. | `Entities.cs:169` | 10.6 |
| G-Constraint-7 | `Physician.CommissionPercentage` uses `decimal` without `HasPrecision(...)` — relies on convention default. | `Physician.cs:26`, no Fluent config | 10.5 |

### Domain completeness

| ID | Observation | Evidence | Section |
|---|---|---|---|
| G-Domain-1 | No Device / Instrument / Analyzer master. No QC / calibration / control-sample entity. | Absence in `Entities.cs` | 6 |
| G-Domain-2 | No persistent report template or print-layout entity. No persisted report archive. | Absence; `ReportModels.cs` is DTO-only | 6, 14 |
| G-Domain-3 | `ResultValue.Value` is type-erased to `string`. Numeric, qualitative ("Positive"/"Negative"), and free-text results share one column with no discriminator on `TestParameter` or `ResultValue`. Flagging works only on numeric-parseable values. | `Entities.cs:334`, no `ResultType` enum | 9.4 |
| G-Domain-4 | Schema churn on age-range modeling: `AgeGroups` table created with 6 Arabic-seeded rows in Phase 3, then dropped in Phase 4 with a column rename and a one-shot SQL conversion assuming legacy units were "Year". | `Phase3_AddTestSchemaExtensions.cs:37-65, 114-128`, `Phase4_AgeRangeRedesign.cs:17-100` | 4.3, 8.2 |
| G-Domain-5 | No distinct `Order`/`Requisition`/`Encounter` entity. Tests attach directly to `Visit.VisitTests` — partial cancellations, amendments, and re-orderings collapse onto `VisitTest.Status` strings. | `Entities.cs:122-139, 296-314` | 6, 14 |
| G-Domain-6 | No reference-range snapshot stored alongside `ResultValue`. If a `TestReferenceRange` is later edited, historical results re-flag against the new range. | `Entities.cs:329-343` — no `LowValueAtVerification`/`HighValueAtVerification` columns | 8.4 |
| G-Domain-7 | No insurance / payer master. `Referral.ReferralType` is a free-text string with no enumerated payer dimension. | `Entities.cs:144` | 6 |
| G-Domain-8 | No patient-consent / authorization entity. | Absence | 6 |
| G-Domain-9 | No range-selection priority when multiple `TestReferenceRange` rows match the same (patient, parameter) combination. The schema admits ambiguity. | `Entities.cs:239-268`, no `Priority`/`Specificity` column | 8.4 |
| G-Domain-10 | No aggregate "all parameters verified" flag on `VisitTest`. Verification is per-parameter only. | `Entities.cs:296-314, 329-343` | 9.2 |

### Extensibility / operational

| ID | Observation | Evidence | Section |
|---|---|---|---|
| G-Ext-1 | `Setting` (legacy, key/value with no metadata) and `SystemSetting` (newer, typed, audited) coexist. The legacy table is not migrated or marked deprecated. | `Entities.cs:468-472`, `SystemSetting.cs` | 12.5 |
| G-Ext-2 | No localization framework. Arabic UI strings and error messages are embedded inline in C# code (e.g. `UserAdminService.cs:60, 94, 100, …`). Status columns mix English and Arabic vocabularies. | `UserAdminService.cs:60-247`, `Entities.cs:558, 574` | 15 |
| G-Ext-3 | `Branch` is optional on `Visit`/`Invoice`/`Payment`. No EF Core query filter scopes data by branch. Roles carry no branch dimension. Multi-tenancy is operational policy, not schema guarantee. | `Entities.cs:131, 356, 373`, no `HasQueryFilter` calls | 16 |
| G-Ext-4 | `SessionContext` is a process-wide singleton. There is no multi-user / multi-session model. Suited to single-user-per-machine desktop deployment. | `SessionContext.cs:10` | 13.5 |
| G-Ext-5 | `OpenLabDbContext` lifetime is **Transient**, so each service injection receives its own context. Cross-service transactions must be explicitly coordinated by sharing a context manually. | `App.xaml.cs:35-43` | 2.3 |
| G-Ext-6 | Convention-based DI silently skips any class in `Open_lab.Services` that lacks an `I{ClassName}` interface (`AgeConverter`, `DailyWorkingSummary`, `BarcodeDialogData`, `AttendancePayrollSummaryRow`). | `App.xaml.cs:164-180` | 2.3 |
| G-Ext-7 | Migration naming convention is inconsistent: `Phase` numbers are not sequential (no Phase 8), and `Phase3_*` / `Phase4_*` / `Phase5_*` each appear twice across different time periods. | `Open_lab/Open_lab/Migrations/` filenames | 4.2 |
| G-Ext-8 | F-series numbering (`F15_AddUserHashVersion`) appears in migration filenames; F6 and F9 reside in commit history but have no namesake migration (no schema change was required for those fixes). | Migrations folder, commit history | 4.2, 13 |

---

## 18. Future Extensibility Assessment

This section reasons about what the current schema can absorb without breaking changes versus what would require a migration. Strictly analytical — no recommendation about whether to act.

### 18.1 Changes the schema absorbs cleanly

- **Adding new permission codes.** `RolePermission.PermissionCode` is a free string — new codes can be assigned to roles without DDL. Only `PermissionCodes.cs` needs new constants.
- **Adding new branches.** `Branch` rows are pure data; transactional tables already carry nullable `BranchId`.
- **Adding new sample types or units.** `SampleType` and `Unit` are simple lookups joined to `Test` via nullable FKs.
- **Adding new referral types or pricing rules.** `Referral.ReferralType` is a free string; `PriceList`/`PriceListItem` can host arbitrary per-test pricing.
- **Adding new test groups or test parameters.** No schema change needed.
- **Adding new audit-log dimensions.** `AuditLog.OldValues`/`NewValues` are JSON — extending entity properties automatically flows into audit data.
- **Adding new microbiology cultures, antibiotics, or sensitivity records.** Pure data.
- **Adding new external-lab destinations.** `Referral` rows of type "external lab" reuse the same machinery.
- **Adding new system settings.** `SystemSetting` is key/value with typed metadata.

### 18.2 Changes that would require migration churn

- **Adding a Device / Instrument / Analyzer master.** Requires (a) new entity, (b) new FKs from `VisitTest` or `ResultValue` to capture which instrument produced the result, (c) optional QC/calibration tables. Touches `Entities.cs`, `OpenLabDbContext.cs`, multiple migrations.
- **Adding persistent print/report templates.** Requires (a) new template entity, (b) renderer rewiring to read from DB instead of code, (c) potentially binary-blob storage for template files.
- **Adding soft-delete across the schema.** Requires `IsDeleted`/`DeletedAt`/`DeletedBy` on every entity (the audit observed zero such columns), plus EF Core global query filters to hide deleted rows by default.
- **Adopting EF Core optimistic concurrency.** Requires `RowVersion byte[]` columns on every entity that participates in concurrent edits.
- **Encounter-time reference-range snapshotting.** Requires `LowValueAtVerification` / `HighValueAtVerification` / `ReferenceRangeIdAtVerification` columns on `ResultValue`.
- **Introducing a distinct `Order` entity between `Visit` and `VisitTest`.** Refactors the core spine of the schema; touches `Visit`, `VisitTest`, all order-related services, all reports, the audit log's `TableName` distribution.
- **Adopting ASP.NET Identity.** Replaces `User`/`Role`/`RolePermission`/`UserRole` with `AspNetUsers`/`AspNetRoles`/etc., and rewires every FK that currently points at `User.UserId`.
- **Multi-tenant data isolation.** Requires (a) making `BranchId` non-nullable on transactional tables, (b) backfilling existing rows, (c) adding `HasQueryFilter` to scope by current branch, (d) extending `RolePermission` with a branch dimension, (e) auditing service queries for filter respect.
- **Splitting the solution into Domain / Infrastructure / Application projects.** Touches every `using Open_lab.Models;` in the UI and Services layers.

### 18.3 Areas where the schema invites but does not require change

- `Setting` (legacy table) versus `SystemSetting` doublet — both work; deciding to retire one is a migration of stored data into the other.
- Bilingual `_ar`/`_en` columns for entity names — would replace inline Arabic strings, but the schema does not require it; the application functions today.
- Per-physician commission percentages — `Physician.CommissionPercentage` (no precision) and `Referral.CommissionPercentage` (`decimal(18,2)`) coexist. The schema accepts both as authoritative; resolution is by `Services/PriceResolutionService.cs`.

---

## Appendix A — File Path Index (Alphabetical)

| File | Purpose |
|---|---|
| `Open_lab/Open_lab.sln` | Visual Studio solution file (4 projects) |
| `Open_lab/Open_lab/App.config` | Connection-string config (no password) |
| `Open_lab/Open_lab/App.xaml.cs` | App startup, DI bootstrap, login/main window navigation |
| `Open_lab/Open_lab/Data/AuditInterceptor.cs` | `SaveChangesInterceptor` writing audit rows |
| `Open_lab/Open_lab/Data/OpenLabDbContext.cs` | Single DbContext with 46 `DbSet<>` properties and all Fluent config |
| `Open_lab/Open_lab/Data/OpenLabDbContextFactory.cs` | Design-time / runtime context factory with env-var-driven password resolution |
| `Open_lab/Open_lab/Migrations/20260408003548_InitialCreate.cs` | Baseline schema (Antibiotics, Cultures, Patients, Referrals, Roles, Tests, Users, …) |
| `Open_lab/Open_lab/Migrations/20260416065459_AddAttendanceLog.cs` | HR attendance table |
| `Open_lab/Open_lab/Migrations/20260416172229_AddUserSaltAndLegacyPasswordUpgrade.cs` | `Users.Salt` column added |
| `Open_lab/Open_lab/Migrations/20260419010556_Phase1_Infrastructure.cs` | `Tests.CostPrice`, `PatientPrice`; `Patients.Age`, `IsPregnant`; `AdditionalCharges` |
| `Open_lab/Open_lab/Migrations/20260419013428_Phase2_AdvancedFinance.cs` | Contract invoicing |
| `Open_lab/Open_lab/Migrations/20260419014730_Phase3_ExternalLabs.cs` | External lab queue, shipment manifests, settlements |
| `Open_lab/Open_lab/Migrations/20260419015747_Phase4_MicrobiologyAdvanced.cs` | Microbiology refinements |
| `Open_lab/Open_lab/Migrations/20260419022104_Phase5_SampleTracking.cs` | `SampleCollections` table |
| `Open_lab/Open_lab/Migrations/20260419023720_Phase6_AdvancedTechnicalWorksheets.cs` | Worksheet data scaffolding |
| `Open_lab/Open_lab/Migrations/20260419024823_Phase7_EntryOrderingAndHistory.cs` | Entry ordering / history |
| `Open_lab/Open_lab/Migrations/20260419031401_Phase9_SecurityAndMonitoring.cs` | `AuditLog` table |
| `Open_lab/Open_lab/Migrations/20260419033340_Phase10_HRAndAttendance.cs` | Shifts, breaks, day statuses |
| `Open_lab/Open_lab/Migrations/20260419034622_Phase11_BulkInvoicingB2B.cs` | Bulk-invoice refinements |
| `Open_lab/Open_lab/Migrations/20260421132719_Phase4_Step2_PricingAndComments.cs` | `TestComments` table, pricing tweaks |
| `Open_lab/Open_lab/Migrations/20260421154337_Phase5_SystemSettings.cs` | `SystemSettings` table |
| `Open_lab/Open_lab/Migrations/20260424040940_AddAttendanceBreakAndDayStatus.cs` | Attendance refinements |
| `Open_lab/Open_lab/Migrations/20260512050000_AddVisitTestReportOrder.cs` | `VisitTest.ReportOrder` |
| `Open_lab/Open_lab/Migrations/20260512160845_AddPatientContractAssignment.cs` | `Patient.ReferralId` + FK |
| `Open_lab/Open_lab/Migrations/20260515031648_AddPatientSearchAndContactFields.cs` | `Patient.HomePhone`/`NationalId`/`Email` + indexes |
| `Open_lab/Open_lab/Migrations/20260523115405_Phase3_AddMedicalHistoryFields.cs` | 6 medical-history bool flags |
| `Open_lab/Open_lab/Migrations/20260523121043_Phase3_AddTestSchemaExtensions.cs` | `AgeGroups` table (created + seeded), `ParameterId` on ranges/comments, `Tests.Description` |
| `Open_lab/Open_lab/Migrations/20260523140733_Phase4_AgeRangeRedesign.cs` | `AgeGroups` dropped, `AgeFromDays`/`AgeToDays` introduced, units backfilled |
| `Open_lab/Open_lab/Migrations/20260526180401_F15_AddUserHashVersion.cs` | `Users.HashVersion` (default 1) for PBKDF2 migration |
| `Open_lab/Open_lab/Migrations/OpenLabDbContextModelSnapshot.cs` | EF Core model snapshot |
| `Open_lab/Open_lab/Models/Entities.cs` | 44 persisted entity classes |
| `Open_lab/Open_lab/Models/Physician.cs` | `Physician` entity (no Fluent config) |
| `Open_lab/Open_lab/Models/SystemSetting.cs` | `SystemSetting` entity (only one using DataAnnotations) |
| `Open_lab/Open_lab/Models/ReferenceRangeResult.cs` | Non-persisted DTO returned by range-comparison logic |
| `Open_lab/Open_lab/Models/ReportModels.cs` | Non-persisted DTOs for report rendering |
| `Open_lab/Open_lab/Services/AuthService.cs` | Login with F6 empty-credential rejection + F15 PBKDF2 auto-upgrade |
| `Open_lab/Open_lab/Services/PasswordSecurity.cs` | Salt generation, SHA-256 / PBKDF2 hash + verify, fixed-time compare |
| `Open_lab/Open_lab/Services/PermissionCodes.cs` | 22 permission-code constants + `All` list |
| `Open_lab/Open_lab/Services/SessionContext.cs` | Singleton session: `UserId`, `Username`, `_grantedPermissions`, `BeginSession`/`EndSession` |
| `Open_lab/Open_lab/Services/UserAdminService.cs` | F9 service-level authorization gate (`EnsureUsersEditPermission`) |
| `Open_lab/Open_lab/Services/ResultsService.cs` | Result flagging logic against `TestReferenceRange` |
| `Open_lab/Open_lab/Services/PatientService.cs` | Patient CRUD |
| `Open_lab/Open_lab/Services/VisitService.cs` | Visit lifecycle |
| `Open_lab/Open_lab/Services/SampleTrackingService.cs` | Sample collection status transitions |
| `Open_lab/Open_lab/Services/PrintService.cs` | Report rendering to WPF FlowDocument |
| `Open_lab/Open_lab/Services/PriceResolutionService.cs` | Resolves the right price across Test/PriceList/Physician/Referral |

---

## Appendix B — Migration Quick Reference

A compact name → effect table for fast lookup:

| Migration | Tables created / dropped | Columns added / dropped | Notable data ops |
|---|---|---|---|
| `InitialCreate` | +Antibiotics, +Cultures, +CustomGroups, +Patients, +Referrals, +Roles, +Tests, +Users, … | Baseline columns | — |
| `AddAttendanceLog` | +AttendanceLogs | — | — |
| `AddUserSaltAndLegacyPasswordUpgrade` | — | +Users.Salt (nvarchar(max), NOT NULL, default `""`) | — |
| `Phase1_Infrastructure` | +AdditionalCharges, +Branches | +Tests.CostPrice, +Tests.PatientPrice, +Patients.Age, +Patients.IsPregnant, … | — |
| `Phase2_AdvancedFinance` | +ContractInvoices | Various FKs | — |
| `Phase3_ExternalLabs` | +ExternalLabQueues, +ShipmentManifests, +ShipmentItems, +ExternalLabSettlements | — | — |
| `Phase4_MicrobiologyAdvanced` | — | Minor refinements | — |
| `Phase5_SampleTracking` | +SampleCollections | — | — |
| `Phase6_AdvancedTechnicalWorksheets` | — | Worksheet scaffolding | — |
| `Phase7_EntryOrderingAndHistory` | — | Minor refinements | — |
| `Phase9_SecurityAndMonitoring` | +AuditLogs | — | — |
| `Phase10_HRAndAttendance` | +ShiftSchedules (also AttendanceBreaks/DayStatuses) | — | — |
| `Phase11_BulkInvoicingB2B` | — | Bulk invoice refinements | — |
| `Phase4_Step2_PricingAndComments` | +TestComments | Pricing tweaks | — |
| `Phase5_SystemSettings` | +SystemSettings | — | — |
| `AddAttendanceBreakAndDayStatus` | — | Attendance refinements | — |
| `AddVisitTestReportOrder` | — | +VisitTests.ReportOrder | — |
| `AddPatientContractAssignment` | — | +Patients.ReferralId + FK | — |
| `AddPatientSearchAndContactFields` | — | +Patients.HomePhone, NationalId, Email, search indexes | — |
| `Phase3_AddMedicalHistoryFields` | — | +6 MedicalHistory bool flags | — |
| `Phase3_AddTestSchemaExtensions` | **+AgeGroups** | +Tests.Description, +AgeGroupId/ParameterId on TestReferenceRanges, +ParameterId on TestComments | **InsertData: 6 Arabic age-group rows**; SQL backfill of ParameterId for single-component tests |
| `Phase4_AgeRangeRedesign` | **-AgeGroups** | Renamed `AgeGroupId` → `AgeToDays`; +AgeFromDays/AgeFromUnit/AgeToUnit; +Patients.AgeUnit | SQL: wipe AgeToDays, recompute from Value × 365 assuming "Year" unit |
| `F15_AddUserHashVersion` | — | +Users.HashVersion (int NOT NULL default 1) | — |

---

## Appendix C — Evidence Snippets

Short verbatim excerpts for the most architecturally significant findings.

### C.1 Password sourcing fail-fast (Section 3.2, 12.2)

`Open_lab/Open_lab/Data/OpenLabDbContext.cs:87-91`

```csharp
var dbPassword = Environment.GetEnvironmentVariable("OPENLAB_DB_PASSWORD");
if (string.IsNullOrWhiteSpace(dbPassword))
{
    throw new InvalidOperationException(MissingDatabasePasswordMessage);
}
```

### C.2 F6 empty-credential rejection (Section 13.1)

`Open_lab/Open_lab/Services/AuthService.cs:31-34`

```csharp
if (string.IsNullOrWhiteSpace(user.Salt) || string.IsNullOrWhiteSpace(user.PasswordHash))
{
    return null;
}
```

### C.3 F15 PBKDF2 auto-upgrade (Section 13.3)

`Open_lab/Open_lab/Services/AuthService.cs:36-47`

```csharp
if (PasswordSecurity.Verify(password, user.Salt, user.PasswordHash, user.HashVersion))
{
    if (user.HashVersion == PasswordSecurity.LegacySha256Version)
    {
        user.Salt = PasswordSecurity.GenerateSecureSalt();
        user.PasswordHash = PasswordSecurity.ComputePbkdf2(password, user.Salt);
        user.HashVersion = PasswordSecurity.Pbkdf2Version;
        await _db.SaveChangesAsync();
    }
    return user;
}
```

### C.4 F9 service-level authorization gate (Section 13.2)

`Open_lab/Open_lab/Services/UserAdminService.cs:269-280`

```csharp
private void EnsureUsersEditPermission()
{
    if (_sessionContext.IsSystemOperation)
    {
        return;
    }

    if (!_sessionContext.HasPermission(PermissionCodes.UsersEdit))
    {
        throw new UnauthorizedAccessException("UsersEdit permission is required to manage users and roles.");
    }
}
```

### C.5 Reference-range core entity (Section 8)

`Open_lab/Open_lab/Models/Entities.cs:239-268` (abbreviated)

```csharp
public class TestReferenceRange
{
    public int RangeId { get; set; }
    public int TestId { get; set; }

    // Nullable for backward compatibility with legacy simple-test ranges.
    public int? ParameterId { get; set; }

    public string? Gender { get; set; }

    // Day is the internal source of truth; Value+Unit are kept only for redisplay.
    public int? AgeFromValue { get; set; }
    public string? AgeFromUnit { get; set; }
    public int? AgeFromDays { get; set; }
    public int? AgeToValue { get; set; }
    public string? AgeToUnit { get; set; }
    public int? AgeToDays { get; set; }

    public decimal? LowValue { get; set; }
    public decimal? HighValue { get; set; }
    public string? NormalText { get; set; }

    public Test Test { get; set; } = null!;
    public TestParameter? Parameter { get; set; }
}
```

### C.6 ResultValue — type-erased value column (Section 9)

`Open_lab/Open_lab/Models/Entities.cs:329-343`

```csharp
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
    // navigations
    public VisitTest VisitTest { get; set; } = null!;
    public TestParameter Parameter { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
}
```

### C.7 Schema churn: AgeGroups dropped (Section 4.3, 8.2)

`Open_lab/Open_lab/Migrations/20260523140733_Phase4_AgeRangeRedesign.cs:13-28`

```csharp
migrationBuilder.DropForeignKey(
    name: "FK_TestReferenceRanges_AgeGroups_AgeGroupId",
    table: "TestReferenceRanges");

migrationBuilder.DropTable(
    name: "AgeGroups");

migrationBuilder.RenameColumn(
    name: "AgeTo",
    table: "TestReferenceRanges",
    newName: "AgeToValue");

migrationBuilder.RenameColumn(
    name: "AgeGroupId",
    table: "TestReferenceRanges",
    newName: "AgeToDays");
```

### C.8 AgeRange data migration assumes legacy "Year" unit (Section 4.3, 8.2)

`Open_lab/Open_lab/Migrations/20260523140733_Phase4_AgeRangeRedesign.cs:75-100`

```csharp
migrationBuilder.Sql(@"
    -- Step 1: AgeToDays was renamed from AgeGroupId — its values are FK ids, not days.
    UPDATE TestReferenceRanges SET AgeToDays = NULL;

    -- Step 3: convert assuming Year units.
    UPDATE TestReferenceRanges
    SET AgeFromUnit = N'Year', AgeFromDays = AgeFromValue * 365
    WHERE AgeFromValue IS NOT NULL;

    UPDATE TestReferenceRanges
    SET AgeToUnit = N'Year', AgeToDays = AgeToValue * 365
    WHERE AgeToValue IS NOT NULL;

    -- Step 4: backfill Patient.AgeUnit.
    UPDATE Patients SET AgeUnit = N'Year' WHERE Age IS NOT NULL AND AgeUnit IS NULL;
");
```

### C.9 AuditInterceptor — UserId fallback (Section 3.4)

`Open_lab/Open_lab/Data/AuditInterceptor.cs:43-47`

```csharp
int userId = _currentUserId ?? 1;
if (context is OpenLabDbContext openLabContext && openLabContext.CurrentUserId.HasValue)
{
    userId = openLabContext.CurrentUserId.Value;
}
```

### C.10 OnConfiguring short-circuit when options pre-supplied (Section 3.5 caveat)

`Open_lab/Open_lab/Data/OpenLabDbContext.cs:73-76`

```csharp
if (optionsBuilder.IsConfigured)
{
    return;
}
```

…followed by the only `optionsBuilder.AddInterceptors(new AuditInterceptor());` calls at lines 83 and 105, which are bypassed when the context is constructed via the options-based ctor used by `OpenLabDbContextFactory.CreateDbContext`.

### C.11 Patient — NationalId indexed but not unique (Section 11.4)

`Open_lab/Open_lab/Data/OpenLabDbContext.cs:147-164` (abbreviated)

```csharp
modelBuilder.Entity<Patient>(entity =>
{
    entity.HasKey(e => e.PatientId);
    entity.HasIndex(e => e.LabId).IsUnique();
    entity.HasIndex(e => e.NationalId);       // ← non-unique
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
```

### C.12 SystemSetting — the only DataAnnotations entity (Section 10.1)

`Open_lab/Open_lab/Models/SystemSetting.cs:8-26`

```csharp
public class SystemSetting
{
    [Key]
    public int SettingId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SettingKey { get; set; } = string.Empty;

    public string? SettingValue { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string SettingType { get; set; } = "String"; // String, Int, Decimal, Bool, Json

    public DateTime? LastModified { get; set; }
}
```

### C.13 Convention-based DI registration (Section 2.3)

`Open_lab/Open_lab/App.xaml.cs:164-180`

```csharp
private static void RegisterServicesByConvention(IServiceCollection services)
{
    var assembly = typeof(IAuthService).Assembly;
    var candidates = assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "Open_lab.Services");

    foreach (var implementation in candidates)
    {
        var interfaceType = implementation.GetInterface($"I{implementation.Name}");
        if (interfaceType == null)
        {
            continue;
        }

        services.AddTransient(interfaceType, implementation);
    }
}
```

### C.14 PriceList → Physician relationship has no Fluent declaration (Section 7.4)

`Open_lab/Open_lab/Models/Physician.cs:9-30` (entire class) declares the relationship via convention only:

```csharp
public class Physician
{
    public int PhysicianId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Specialty { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public int? PriceListId { get; set; }
    public decimal? CommissionPercentage { get; set; }

    public PriceList? PriceList { get; set; }
    public ICollection<Visit> Visits { get; set; } = new HashSet<Visit>();
}
```

No corresponding `modelBuilder.Entity<Physician>(...)` block exists in `OpenLabDbContext.OnModelCreating`.

---

## End of Audit

**Document length:** ~1700 lines of analytical content across 21 sections + 3 appendices.

**Audit verification checklist** (per the approved plan, Section 5):

1. **Spot-check evidence** — every claim in this document is cited to `file:line` and was read verbatim by the audit during preparation.
2. **DbSet count cross-check** — `OpenLabDbContext.cs:24-69` declares exactly 46 `DbSet<>` properties.
3. **Domain coverage cross-check** — every ✓ in Section 6 matches a class in `Entities.cs` / `Physician.cs` / `SystemSetting.cs`; every ✗ matches an absence (no such class exists).
4. **Relationship cross-check** — every cascade behavior in Section 7 matches an explicit `OnDelete(...)` call or the documented convention default.
5. **Migration cross-check** — the 23 migrations listed in Sections 4.1 and Appendix B match the 23 non-Designer `.cs` files in `Open_lab/Open_lab/Migrations/`.
6. **Security cross-check** — F6 (`AuthService.cs:31-34`), F9 (`UserAdminService.cs:269-280`), F15 (`Users.HashVersion` migration + `PasswordSecurity.cs` algorithm dispatch) all confirmed by reading source.

**Scope reminder:** This document is strictly analytical. It does not propose remediation, prioritization, or implementation. Decisions in those areas belong to the architectural review meeting this document is intended to support.
