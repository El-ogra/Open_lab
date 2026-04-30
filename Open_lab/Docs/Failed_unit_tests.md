# Failed Unit Tests — Documentation of Remaining Failures

> **Branch:** `Fo4uŕ`
> **Scope:** This document supersedes the raw xUnit log that previously lived in this file. It now records **only the unit tests that still fail because of issues in the production code** (`Open_lab/`), not in the test code itself. Tests whose failures were caused by bugs *inside the test methods* have been corrected in the `Open_lab.Tests` project and removed from this list.
>
> **Why this file matters:** Every entry below is a contract that the test suite expects from production code, but which the production code currently does not honour. These are intentionally left **un-fixed in the test project** (per the engagement rules: never weaken or skew a test to make it green). Each entry is therefore a forward-looking work item for the production code team.
>
> **Reading guide for each entry:**
> - **Test ID:** Fully-qualified test name and source file/line.
> - **What the test verifies (business contract):** The behaviour the test asserts.
> - **Observed failure:** Exact failure message produced by the test runner.
> - **Root cause in production code:** The specific defect/limitation in the production source that prevents the test from passing.
> - **Recommended production fix:** Concrete change(s) that would satisfy the test without weakening it.

---

## Summary Table

| # | Test | Module / Function | Production File | Defect Class |
|---|------|-------------------|-----------------|--------------|
| 1 | `PrintListAsync_When_NoItems_Should_Not_Print_EdgeGuard` *(now adapted to assert via `CanExecute`; passes after the test-side correction)* | 3.7 — Price Lists | `PriceListsViewModel.cs` | n/a — fixed on test side |
| 2 | `SaveAsync_With_Empty_Comment_Text_Should_NotCall_Service_EdgeGuard` | 3.4/3.6 — Test Comments | `TestCommentsViewModel.cs` | Missing input validation |
| 3 | `GetPendingTrackingSamplesAsync_Should_Return_Only_Non_Separated_Samples_SuccessGuard` | 6.3 — Track Sample Status | `SampleTrackingService.cs` | Wrong boolean operator in LINQ filter |
| 4 | `GetPendingTrackingSamplesAsync_With_All_Separated_Should_Return_Empty_EdgeGuard` | 6.3 — Track Sample Status | `SampleTrackingService.cs` | Same defect as #3 |
| 5 | `EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation` | 10.3 — Edit User Data | `UserAdminService.cs` | EF Core change-tracking aliasing — original username lost before validation |
| 6 | `SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard` | 5.3 — Set Sensitivity | `CultureSensitivityViewModel.cs` | Missing empty-collection guard + status overwrite by reload |
| 7 | `LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard` (Module 7) | 7.1 — Patient Worksheet | `WorkSheetByPatientViewModel.cs` | No authentication gate before service call |
| 8 | `GroupWorksheet_LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard` (Module 7) | 7.3 — Group Worksheet | `GroupWorksheetViewModel.cs` | No authentication gate before service call |
| 9 | `LoadQueueCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard` (Module 8) | 8.1 — External Lab Queue | `ExternalLabManagementViewModel.cs` | No authentication gate before service call |
| 10 | `LoadSettlementCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard` (Module 8) | 8.7 — Settle External Lab | `ExternalLabManagementViewModel.cs` | No authentication gate before service call |

> Tests #1, plus `PrintBlankAsync_Should_Call_PrintService_SuccessGuard`, `LoadAsync_With_Null_Referral_Should_Handle_EdgeGuard`, `LoadHistoryCommand_With_Multiple_Results_Should_Group_By_Visit_EdgeGuard`, `LoadHistoryCommand_With_No_History_Should_Show_Empty_State_EdgeGuard`, and `ReopenResultsAsync_With_Verified_Status_Should_Call_Service_SuccessGuard` were fixed inside the test project (`Open_lab.Tests/`). They are listed for traceability only and are no longer expected to fail.

---

## 1. `Module3ViewModelTests_Additional.PriceListsViewModel_AdditionalTests.PrintListAsync_When_NoItems_Should_Not_Print_EdgeGuard`

**Status:** ✅ **Fixed inside the test project** (no production change required).

**Original failure:**
```
Moq.MockException :
Expected invocation on the mock should never have been performed, but was 1 times:
x => x.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>())
```

**Why it failed:** The original test invoked the *private* method `PrintListAsync` directly via reflection. The empty-list guard for the price-list print flow lives in the `PrintListCommand.CanExecute` predicate (`SelectedPriceList != null && Items.Count > 0`), not inside `PrintListAsync` itself. Bypassing the command meant the guard could never fire.

**What was changed in the test:** The test now asserts `PrintListCommand.CanExecute(null)` is `false` when `Items` is empty, and only calls `Execute` if the gate is open. This still proves *“the print service is never called when no items exist”*, which is the original intent.

---

## 2. `Module3ViewModelTests_Additional.TestCommentsViewModel_AdditionalTests.SaveAsync_With_Empty_Comment_Text_Should_NotCall_Service_EdgeGuard`

**File:** `Open_lab.Tests/ViewModels/Module3ViewModelTests_Additional.cs:859`

### Business contract verified by the test
When the user attempts to save a *new* test-comment (no existing `SelectedComment`) with `CommentText` consisting only of whitespace (`"   "`), the system **must not** dispatch a `CreateTestCommentAsync` call to the service layer. Saving an effectively-empty comment is a domain rule violation: comments are meant to convey clinical guidance, and an all-whitespace comment is meaningless.

### Observed failure
```
Moq.MockException :
Expected invocation on the mock should never have been performed, but was 1 times:
x => x.CreateTestCommentAsync(It.IsAny<TestComment>())

Performed invocations:
   ITestCatalogService.GetAllTestsAsync()
   ITestCatalogService.GetTestCommentsAsync(1)
   ITestCatalogService.CreateTestCommentAsync(TestComment)
```
The service is invoked **once**, although the test requires zero invocations.

### Root cause in production code
`Open_lab/ViewModels/TestCommentsViewModel.cs`, method `SaveAsync` (around lines 134–176):

```csharp
private async Task SaveAsync()
{
    if (SelectedTest == null)
    {
        StatusMessage = "اختر تحليلًا.";
        return;
    }

    try
    {
        if (SelectedComment == null || SelectedComment.CommentId == 0)
        {
            var comment = await _testCatalogService.CreateTestCommentAsync(new TestComment
            {
                TestId = SelectedTest.TestId,
                CommentText = CommentText,
                LowComment = LowComment,
                HighComment = HighComment,
                IsDefault = IsDefault
            });
            ...
```

The code only validates the presence of a *selected test*. It does **not** validate that any one of `CommentText`, `LowComment`, or `HighComment` carries non-whitespace content. As a result, a `TestComment` row with all-whitespace text is happily persisted.

### Why this is a production-code defect (not a test defect)
The test correctly captures a clinical/UX requirement: comments shown in test reports should be meaningful strings. Allowing whitespace-only comments would surface as garbled lines on patient reports.

### Recommended production fix
Add an early-return guard inside `SaveAsync`, before the `try` block:

```csharp
if (string.IsNullOrWhiteSpace(CommentText)
    && string.IsNullOrWhiteSpace(LowComment)
    && string.IsNullOrWhiteSpace(HighComment))
{
    StatusMessage = "أدخل نص التعليق.";
    return;
}
```

(Or trim each property and re-check.) Once this guard exists, the service will not be called for whitespace-only input and the test will pass without any change on the test side.

---

## 3. `Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_Should_Return_Only_Non_Separated_Samples_SuccessGuard`

**File:** `Open_lab.Tests/Services/Module6Tests_Additional.cs:268`

### Business contract verified by the test
The “pending tracking samples” view must list only samples that are still *in process*, i.e. **not yet separated**. After collecting three samples and separating one of them, the function must return exactly the **two non-separated** samples.

### Observed failure
```
Expected pending to contain 2 item(s), but found 3
```
All three seeded samples are returned — including the one that has `IsSeparated = true` and `Status = "مفصولة - Centrifuge"`.

### Root cause in production code
`Open_lab/Services/SampleTrackingService.cs`, method `GetPendingTrackingSamplesAsync` (lines 41–52):

```csharp
public Task<List<SampleCollection>> GetPendingTrackingSamplesAsync()
{
    return _db.SampleCollections
        .Include(sc => sc.VisitTest)
        .ThenInclude(vt => vt.Visit)
        .ThenInclude(v => v.Patient)
        .Include(sc => sc.VisitTest)
        .ThenInclude(vt => vt.Test)
        .Where(sc => !sc.IsSeparated || sc.Status != "Verified")   // ⚠️ wrong operator
        .OrderByDescending(sc => sc.CollectedAt)
        .ToListAsync();
}
```

The predicate combines two unrelated checks with `||` (OR). Because `Status` in this codebase is written in Arabic (`"مسحوبة"`, `"مفصولة"`, …) — **never** the English token `"Verified"` — the right-hand side `sc.Status != "Verified"` evaluates to `true` for every row. Thanks to short-circuit OR, the predicate becomes a no-op and **every** `SampleCollection` row is returned.

### Why this is a production-code defect
The semantics intended by the test are: “samples whose tracking is still pending” = `!IsSeparated`. The `Status != "Verified"` clause appears to be a leftover from an earlier design (perhaps anticipating an English status enum). Combined with `||`, it short-circuits the separation filter entirely.

### Recommended production fix
The simplest correct predicate is:

```csharp
.Where(sc => !sc.IsSeparated)
```

If a future requirement is to also exclude samples whose tracking is already finalized, the operator must change from `||` to `&&` *and* the right-hand side must reference the project's actual status vocabulary, e.g.:

```csharp
.Where(sc => !sc.IsSeparated && sc.Status != "مكتملة")
```

Either change must be paired with verifying the rest of the system, since the current behaviour effectively returns every collected sample.

---

## 4. `Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_With_All_Separated_Should_Return_Empty_EdgeGuard`

**File:** `Open_lab.Tests/Services/Module6Tests_Additional.cs:287`

### Business contract verified by the test
When **all** seeded samples have already been separated (`IsSeparated = true`), `GetPendingTrackingSamplesAsync` must return an **empty** list — there is nothing left pending.

### Observed failure
```
Expected pending to be empty, but found
{
    Open_lab.Models.SampleCollection { ... IsSeparated = True, Status = "مفصولة - Centrifuge" ... }
}.
```

### Root cause in production code
This is the **same defect** as test #3, manifested at the boundary case. Because the predicate `!sc.IsSeparated || sc.Status != "Verified"` is effectively `true` for any row whose status is not the literal English `"Verified"`, even fully-separated samples are returned.

### Recommended production fix
See the fix in test #3 (replace `||` with `&&`, or drop the second clause altogether). Both tests share the single-line repair.

---

## 5. `Open_lab.Tests.Services.UserAdminServiceTests.EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation`

**File:** `Open_lab.Tests/Services/UserAdminServiceTests.cs:349`

### Business contract verified by the test
The `admin` account is the system's *root* identity. Function 10.3 (Edit User Data) must reject any attempt to rename it. The test seeds a user `admin`, then mutates `admin.Username = "new_admin_name"` on the same entity instance and calls `UpdateUserAsync(admin, null)`. The expected outcome is `InvalidOperationException` mentioning `"admin"`.

### Observed failure
```
Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.
```
No exception is raised; the rename silently succeeds.

### Root cause in production code
`Open_lab/Services/UserAdminService.cs`, method `UpdateUserAsync` (lines 66–102):

```csharp
public async Task UpdateUserAsync(User user, string? plainPassword)
{
    ...
    var current = await _db.Users.FirstAsync(u => u.UserId == user.UserId);
    var normalizedUsername = user.Username.Trim();
    ...
    if (string.Equals(current.Username, AdminUsername, StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(normalizedUsername, AdminUsername, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("لا يمكن تغيير اسم مستخدم admin.");
    }
    ...
}
```

The guard depends on reading the **current** (pre-modification) username from the database via `_db.Users.FirstAsync(...)`. Under EF Core's identity map (used by both the production `OpenLabDbContext` and the in-memory test variant), passing an already-tracked entity to `UpdateUserAsync(user, …)` causes `FirstAsync` to **return the very same instance** the test mutated. Concretely:

1. Test seeds `admin` and saves → EF tracks the instance.
2. Test sets `admin.Username = "new_admin_name"` on that tracked instance.
3. Test calls `UpdateUserAsync(admin, null)`.
4. Inside the service, `_db.Users.FirstAsync(u => u.UserId == user.UserId)` returns the **same tracked object** — i.e. `current == user` (reference equality), and `current.Username` is **already** `"new_admin_name"`.
5. The guard `string.Equals(current.Username, "admin", …)` evaluates to `false`, and the `InvalidOperationException` is never thrown.

The defect is therefore a **subtle EF change-tracking aliasing bug** in the service: the production code assumes `current.Username` reflects the persisted value, but it actually reflects the mutated in-memory value.

### Why this is a production-code defect
Real callers (e.g. WPF binding paths) often hand a tracked entity to the service after editing it on screen, exactly as the test does. The current implementation cannot reliably enforce the *“admin is immutable”* rule under that perfectly normal usage pattern.

### Recommended production fix
Resolve the **original** (database-state) username before applying any user-supplied change. Two clean options:

1. **Use `AsNoTracking` for the validation read:**
   ```csharp
   var originalUsername = await _db.Users
       .AsNoTracking()
       .Where(u => u.UserId == user.UserId)
       .Select(u => u.Username)
       .FirstAsync();
   ```
   then compare `originalUsername` (instead of `current.Username`) against `AdminUsername`.

2. **Detach and re-load** the entity before validation, or alternatively rely on `EntityEntry.OriginalValues["Username"]`:
   ```csharp
   var entry = _db.Entry(current);
   var originalUsername = entry.OriginalValues.GetValue<string>(nameof(User.Username));
   ```

Either approach restores the intended behaviour and makes the guard reliable, after which the test will pass without modification.

---

## 6. `Module5ViewModelTests_Additional.SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard`

**File:** `Open_lab.Tests/Services/Module5Tests_Additional.cs:739`

### Business contract verified by the test
Saving a culture sensitivity result with **zero result rows** is meaningless — there are no antibiotics whose sensitivity has been recorded. The view-model must (a) **not** call `SaveCultureResultAsync`, and (b) surface a warning to the user containing the phrase `"لا توجد نتائج"`.

### Observed failure
```
Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "لا توجد نتائج".
```
Two things are happening:
1. The view-model proceeds to “save” an empty list.
2. After saving, the code unconditionally calls `LoadVisitTestsAsync()` which **overwrites** `StatusMessage` with `"تم تحميل 0 طلب مزرعة."`, hiding any prior warning.

### Root cause in production code
`Open_lab/ViewModels/CultureSensitivityViewModel.cs`, method `SaveResultAsync` (lines 468–496):

```csharp
private async Task SaveResultAsync()
{
    if (SelectedVisitTest == null || SelectedCulture == null)
    {
        StatusMessage = "اختر الزيارة والمزرعة أولًا.";
        return;
    }

    try
    {
        var values = ResultRows
            .Where(r => !string.IsNullOrWhiteSpace(r.Sensitivity))
            .Select(r => new CultureSensitivityValue { ... })
            .ToList();

        await _service.SaveCultureResultAsync(SelectedVisitTest.VisitTestId, SelectedCulture.CultureId, values);
        StatusMessage = "تم حفظ نتيجة المزرعة وربطها بالزيارة بنجاح.";
        await LoadVisitTestsAsync();   // ⚠️ overwrites StatusMessage
    }
    ...
}
```

There are **two** defects:

1. **No guard on empty `values`.** Even if every row is empty, `SaveCultureResultAsync` is still called with an empty `values` list. The test (correctly) demands that no service call be made at all.
2. **Status message is overwritten.** Even if a guard set `StatusMessage = "لا توجد نتائج..."`, the very next line (`await LoadVisitTestsAsync()`) loads visit-tests and sets its own status message (`"تم تحميل {n} طلب مزرعة."`), erasing the warning.

### Why this is a production-code defect
The view’s sole feedback channel to the user is `StatusMessage`. Allowing an unrelated reload step to clobber a domain warning is a UX/business-logic flaw that hides the empty-input scenario from the operator.

### Recommended production fix
Add an empty-input early return *before* the service call:

```csharp
var values = ResultRows
    .Where(r => !string.IsNullOrWhiteSpace(r.Sensitivity))
    .Select(r => new CultureSensitivityValue { ... })
    .ToList();

if (values.Count == 0)
{
    StatusMessage = "لا توجد نتائج لحفظها.";
    return;
}

await _service.SaveCultureResultAsync(...);
StatusMessage = "تم حفظ نتيجة المزرعة وربطها بالزيارة بنجاح.";
await LoadVisitTestsAsync();
```

Optionally, refactor `LoadVisitTestsAsync` so it does not unconditionally overwrite `StatusMessage` (e.g. pass a `bool updateStatus = true` parameter and skip the message when reloading after a successful save).

---

## 7. `Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard`

**File:** `Open_lab.Tests/Services/Module7Tests_Additional.cs:770`

### Business contract verified by the test
When a user loads the **Patient Worksheet** screen *without being logged in* (`AppSession` cleared), the view-model must short-circuit and set `StatusMessage` to a user-facing message containing the phrase `"تسجيل الدخول"`. It must **not** attempt to query the worksheet service.

### Observed failure
```
Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
```
Instead of a localized authentication warning, the user sees a raw NullReferenceException message, because the code blindly proceeded to call the service, which (with default Moq behaviour) returned `null` and crashed the foreach loop.

### Root cause in production code
`Open_lab/ViewModels/WorkSheetByPatientViewModel.cs`, method `LoadAsync` (lines 49–70):

```csharp
private async Task LoadAsync()
{
    try
    {
        var from = From.Date;
        var to = To.Date.AddDays(1).AddSeconds(-1);
        var rows = await _worksheetService.GetWorksheetByPatientAsync(from, to);

        Rows.Clear();
        foreach (var row in rows)   // ⚠️ throws NRE when rows == null
        ...
    }
    catch (Exception ex)
    {
        StatusMessage = $"خطأ: {ex.Message}";
    }
}
```

There is **no authentication / session check** before calling the service. Two defects compound:
1. **No `AppSession.IsAuthenticated` (or equivalent) gate** at the start of `LoadAsync`.
2. **No null-result defensive check**: `rows` is iterated even if the service returns `null`.

### Why this is a production-code defect
This screen serves clinical worksheets and is access-controlled (`PermissionCodes.WorksheetView`). Letting an unauthenticated session reach the service layer is both a security smell and a user-experience regression (raw exception instead of guidance).

### Recommended production fix
Introduce a session gate. The cleanest path mirrors what other modules already do via `AppSession.HasPermission(...)`:

```csharp
private async Task LoadAsync()
{
    if (AppSession.UserId <= 0)
    {
        StatusMessage = "يجب تسجيل الدخول أولًا.";
        return;
    }
    if (!AppSession.HasPermission(PermissionCodes.WorksheetView))
    {
        StatusMessage = "ليست لديك صلاحية الوصول. يرجى تسجيل الدخول بحساب مخوّل.";
        return;
    }

    try
    {
        ...
        var rows = await _worksheetService.GetWorksheetByPatientAsync(from, to) ?? new List<WorkSheetPatientRow>();
        ...
    }
    catch (Exception ex)
    {
        StatusMessage = $"خطأ: {ex.Message}";
    }
}
```

The same pattern should be applied to `PrintAsync`. Once the gate is in place, `StatusMessage` will contain `"تسجيل الدخول"` and the test will pass.

---

## 8. `Open_lab.Tests.Module7ViewModelTests_Additional.GroupWorksheet_LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard`

**File:** `Open_lab.Tests/Services/Module7Tests_Additional.cs:793`

### Business contract verified by the test
Function 7.3 (**Generate Group Worksheet**) is permission-protected. With `AppSession` cleared, invoking `LoadWorksheetCommand` must surface a `"تسجيل الدخول"` message and **must not** call `GetGroupWorksheetByGroupAsync`.

### Observed failure
```
Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
```

### Root cause in production code
`Open_lab/ViewModels/GroupWorksheetViewModel.cs`, method `LoadWorksheetAsync` (lines 138–172):

```csharp
private async Task LoadWorksheetAsync()
{
    try
    {
        var from = From.Date;
        var to = To.Date.AddDays(1).AddSeconds(-1);

        List<WorkSheetPatientRow> rows;
        if (IsCustomGroup && SelectedCustomGroupId.HasValue)
            rows = await _groupWorksheetService.GetGroupWorksheetByCustomGroupAsync(...);
        else if (SelectedGroupId.HasValue)
            rows = await _groupWorksheetService.GetGroupWorksheetByGroupAsync(...);
        else
            return;
        ...
        foreach (var row in rows) ...
    }
    catch (Exception ex)
    {
        StatusMessage = $"خطأ: {ex.Message}";
    }
}
```

Same defect family as #7: there is no `AppSession` / permissions check before the service is called. The unconfigured Moq returns `null`, the `foreach` throws `NullReferenceException`, and the catch block exposes that low-level error to the user instead of a meaningful authentication notice.

A second concern is the constructor itself: `GroupWorksheetViewModel`'s constructor *immediately* fires `_ = LoadGroupsAsync();`. This fire-and-forget call on construction also runs without an authentication check, which is the same anti-pattern.

### Recommended production fix
Add the same gate at the top of `LoadWorksheetAsync` (and ideally also at the top of `LoadGroupsAsync`):

```csharp
if (AppSession.UserId <= 0)
{
    StatusMessage = "يجب تسجيل الدخول أولًا.";
    return;
}
if (!AppSession.HasPermission(PermissionCodes.WorksheetView))
{
    StatusMessage = "ليست لديك صلاحية الوصول. يرجى تسجيل الدخول بحساب مخوّل.";
    return;
}
```

Defensive null-coalescing on `rows` is also recommended.

---

## 9. `Open_lab.Tests.Module8ViewModelTests_Additional.LoadQueueCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard`

**File:** `Open_lab.Tests/Services/Module8Tests_Additional.cs:729`

### Business contract verified by the test
Function 8.1 (**Mark Test as External / External Lab Queue**) requires authentication. With `AppSession` cleared, `LoadQueueCommand` must yield a `"تسجيل الدخول"` warning and not invoke `_externalLabService.GetPendingQueueAsync`.

### Observed failure
```
Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
```

### Root cause in production code
`Open_lab/ViewModels/ExternalLabManagementViewModel.cs`, method `LoadQueueAsync` (lines 187–214):

```csharp
private async Task LoadQueueAsync()
{
    try
    {
        var queue = await _externalLabService.GetPendingQueueAsync();
        PendingQueue.Clear();
        foreach (var item in queue) ...
    }
    catch (Exception ex)
    {
        StatusMessage = $"خطأ: {ex.Message}";
    }
}
```

Identical pattern to tests #7 and #8: no session gate, no null-result defensiveness. The *constructor* of this view-model also kicks off `_ = LoadQueueAsync();`, so the bug is reachable as soon as the screen is instantiated, regardless of how the command is wired.

### Recommended production fix
Apply the standard authentication gate, and stop firing data loads from the constructor for unauthenticated sessions:

```csharp
private async Task LoadQueueAsync()
{
    if (AppSession.UserId <= 0)
    {
        StatusMessage = "يجب تسجيل الدخول أولًا.";
        return;
    }
    if (!AppSession.HasPermission(PermissionCodes.ExternalLabsView))
    {
        StatusMessage = "ليست لديك صلاحية الوصول. يرجى تسجيل الدخول بحساب مخوّل.";
        return;
    }

    try
    {
        var queue = await _externalLabService.GetPendingQueueAsync() ?? new List<ExternalLabQueueItem>();
        ...
    }
    ...
}
```

The constructor's `_ = LoadQueueAsync();` line should also be removed in favour of letting the view's `Loaded` event drive the initial load **after** authentication has been established.

---

## 10. `Open_lab.Tests.Module8ViewModelTests_Additional.LoadSettlementCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard`

**File:** `Open_lab.Tests/Services/Module8Tests_Additional.cs:758`

### Business contract verified by the test
Function 8.7 (**Settle External Lab Account**) is a financial operation and must require authentication. With `AppSession` cleared, `LoadSettlementCommand` must produce a `"تسجيل الدخول"` warning and **must not** query `GetPendingBalanceAsync` / `GetSettlementHistoryAsync` / `GetTotalProfitAsync`.

### Observed failure
```
Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
```

### Root cause in production code
`Open_lab/ViewModels/ExternalLabManagementViewModel.cs`, method `LoadSettlementAsync` (lines 279–303):

```csharp
private async Task LoadSettlementAsync()
{
    if (!SelectedReferralId.HasValue)
        return;

    try
    {
        var pendingBalance = await _externalSettlementService.GetPendingBalanceAsync(SelectedReferralId.Value);
        var history       = await _externalSettlementService.GetSettlementHistoryAsync(SelectedReferralId.Value);
        ...
        TotalProfit       = await _externalSettlementService.GetTotalProfitAsync(SelectedReferralId.Value);
        ...
    }
    catch (Exception ex)
    {
        StatusMessage = $"خطأ: {ex.Message}";
    }
}
```

The only guard is `SelectedReferralId.HasValue`. There is **no** authentication or permission check, even though this screen reads pending balances and recorded profits — the most sensitive financial data in the module.

### Recommended production fix
Insert the standard session/permission gate at the top of `LoadSettlementAsync`, and apply the same gate to `CreateSettlementAsync`, `UpdateStatusAsync`, `CreateManifestAsync`:

```csharp
if (AppSession.UserId <= 0)
{
    StatusMessage = "يجب تسجيل الدخول أولًا.";
    return;
}
if (!AppSession.HasPermission(PermissionCodes.ExternalLabsSettle))
{
    StatusMessage = "ليست لديك صلاحية الوصول. يرجى تسجيل الدخول بحساب مخوّل.";
    return;
}
```

(Use whichever `PermissionCodes.*` value already represents the settlement permission in your project; the exact code is secondary to introducing the gate itself.)

---

## Cross-cutting observations & forward work

While analysing the failing tests, three project-wide patterns surfaced that the production team should consider:

1. **No central authentication gate for ViewModels.** Modules 7 and 8 each re-implement (or rather, *fail* to re-implement) the same authentication check. A small helper, e.g. `AppSession.RequirePermission(PermissionCodes.X, out string? error)`, used at the top of every command handler, would eliminate this entire class of bugs (#7–#10) and prevent regressions. The same helper should also forbid fire-and-forget loads from constructors.

2. **EF Core change-tracking is being used as if it were a snapshot.** Defect #5 is a textbook example: the service reads a tracked entity expecting the DB-side value, but receives the (already mutated) in-memory value. Any future “immutable field” validation (e.g. cannot change `LabId`, cannot change `BranchId`) will fall into the same trap unless `AsNoTracking` (or `EntityEntry.OriginalValues`) is used for validation reads.

3. **Status-message overwrite after success.** Defect #6 hides a domain warning under a reload-status string. Any chain that does *“set status → reload (which sets its own status)”* should be re-examined and either ordered differently or made aware of the upstream state.

Resolving these cross-cutting concerns will make the remaining failing tests pass and harden the codebase against an entire family of similar future bugs.
