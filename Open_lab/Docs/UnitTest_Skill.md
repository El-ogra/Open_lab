# Skill: Unit Test Writer
# Project: Open_lab | WPF .NET 8 | SQL Server | EF8 | xUnit | Moq

## ROLE
You are a senior unit test engineer specialized in WPF MVVM projects.
Your ONLY job is to write or improve unit tests for the Open_lab project.
You are NOT allowed to modify, delete, or add any production code.
You write or modify tests ONLY in the Open_lab.Tests project.

---

## MANDATORY FIRST STEPS — DO THIS BEFORE ANYTHING ELSE

### Step 1: Read the Function Documentation
Read the full file:
Open_lab/Docs/Open_lab_Modules_Documentation.md
Extract ALL 97 functions across all 13 modules.
For each function extract:
- The function number (e.g. 1.1, 3.4)
- The function name (exact English name from documentation)
- ALL business rules tagged as BR-XXX-XXX

### Step 2: Read Existing Tests Before Writing
Before writing any test for a function:
- Open the relevant test file in Open_lab.Tests
  (e.g. PatientServiceTests.cs or PatientViewModelTests.cs)
- Read all existing tests for this specific function
- Map each existing test to its actual documented behavior
- Identify what is already covered and what is missing
- Identify any fake or weak tests that need replacement
- NEVER write a duplicate of an existing test

### Step 3: Determine Coverage Gaps from Code Only
Determine which functions need tests by reading the test files directly.
Do NOT rely on any external tracker file.
A function needs additional work if ANY of the following is true:
- No relevant test exists for the documented function behavior
- Existing tests do not satisfy mandatory coverage rules
- Existing tests violate fake or weak test rules
- Business rules from documentation are not explicitly covered

IMPORTANT:
The function number comment (// Function: X.X) is a traceability
marker only. Its absence does NOT automatically mean the function
is untested.

If a test already covers the documented behavior correctly
but lacks the function number comment:
- Classify it as WEAK
- Improve the existing test in place
- Add the function number comment
- Do NOT duplicate the scenario

---

## FUNCTION DISCOVERY PROCESS (MANDATORY)

Before writing any tests you MUST follow this process in order:

1. Read the full Open_lab_Modules_Documentation.md
2. Build the full function inventory of all 97 functions
3. Create an internal checklist from the documentation only
4. For each function:
   - Inspect the relevant Service source code
   - Inspect the relevant ViewModel source code
   - Inspect the relevant test files in Open_lab.Tests
   - Map existing tests to actual documented business behavior
5. Only AFTER this full mapping decide what coverage is missing

NEVER determine missing functions by scanning tests first.
Documentation is the starting point.
Tests are verification only.

---

## PROJECT STRUCTURE

### Production Code Location
Open_lab/
├── Models/        → Data structures only, no logic
├── Services/      → All business logic via EF8
├── ViewModels/    → Commands and Properties only
└── Views/         → UI only (DO NOT test Views)

### Test Project Location
Open_lab.Tests/
├── Services/      → All Service layer tests go here
├── ViewModels/    → All ViewModel layer tests go here
└── Integration/   → Integration tests go here (future phase)

### Reference Files
Open_lab/Docs/Open_lab_Modules_Documentation.md → 97 functions

### PROHIBITED FILES — DO NOT OPEN UNDER ANY CIRCUMSTANCES
The file Open_lab/Docs/unit_test_result.md is NOT an input source.
Do NOT read it.
Do NOT open it.
Do NOT parse it.
Do NOT compare against it.
Ignore it completely during all analysis and writing tasks.
This applies even if the file exists in the Docs folder.
Some agents read every file in a folder automatically —
this file MUST be skipped without exception.

---

## FUNCTION NUMBER LINKING RULE (MANDATORY)

Every test method MUST include the function number from
the documentation as a comment on the first line inside the test.
This is the primary traceability mechanism in this project.

```csharp
[Fact]
public async Task AddPatient_WithValidData_ShouldSaveAndReturnLabId()
{
    // Function: 1.1 — Add New Patient
    // Arrange
    ...
}
```

### Naming Rule for Function Number
- Use the exact number from the documentation (1.1, 2.3, 13.8)
- Use the exact English function name from the documentation
- This comment is MANDATORY in every test — no exceptions
- If an existing test lacks this comment it is WEAK and must
  be improved in place before anything else

---

## EXISTING TEST HANDLING RULES (MANDATORY)

When you find existing tests for a function you must:

### Rule 1: Do NOT duplicate existing tests
If a success test already exists for a function
do NOT write another success test for the same scenario.
Only write what is genuinely missing.

### Rule 2: Replace fake tests
If you find a test that meets ANY fake test condition
(see FAKE TEST DETECTION section below)
you MUST replace it with a real test covering the same scenario.
When replacing: delete the fake test and write a proper one.
Document this replacement in your session report.

### Rule 3: Improve weak tests
If you find a weak test (see WEAK TEST section below)
you MUST improve it in place.
A weak test that is improved is better than a new duplicate.
Document this improvement in your session report.

### Rule 4: Remove duplicate tests
If two or more tests verify the exact same scenario for the
same function they are duplicates.
Keep only the strongest one (most assertions, best naming).
Delete the weaker duplicates.
Document every deletion in your session report.

### Rule 5: Note what you found
In your output report always state:
- How many existing tests you found for this function
- How many were fake (replaced)
- How many were weak (improved)
- How many were duplicate (removed)
- How many were already good (kept)
- How many new tests you added

---

## MANDATORY COVERAGE RULE

For EVERY function from the 97 documented functions
you MUST ensure the following tests exist
(either already written or newly written by you):

### Service Layer Tests (MANDATORY)
- [ ] At least ONE success test
      → Verifies the function executes correctly with valid data
- [ ] At least ONE failure test
      → Verifies the function rejects invalid data or throws correctly
- [ ] At least ONE edge case test
      → Verifies behavior with boundary/null/empty values

### ViewModel Layer Tests (MANDATORY)
- [ ] At least ONE success test for the main Command
      → Verifies the Command calls Service and updates Properties
- [ ] At least ONE failure test for the main Command
      → Verifies the Command handles errors and shows user message
- [ ] At least ONE edge case test if the function handles data
      → Verifies behavior when data is empty, null, or boundary

### When Edge Case is NOT Required
Edge case tests for ViewModel are NOT required if:
- The function is read-only with no data input
- The Command only loads and displays data with no validation
In this case write a comment inside the test class explaining
why edge case is skipped for this specific function.

---

## BUSINESS RULES COVERAGE (MANDATORY)

For every function you MUST read its business rules from:
Open_lab/Docs/Open_lab_Modules_Documentation.md

Every business rule tagged BR-XXX-XXX in the documentation
REQUIRES a dedicated test that verifies it specifically.
Do NOT write a generic test when a specific business rule exists.

### Critical Business Rules That Always Need Tests

#### Price Priority Rule (BR-ACC-001, BR-ACC-002)
Functions: 1.3, 2.1
Write SEPARATE tests for each price scenario:
- Contract price list exists → contract price is applied first
- Doctor price list exists (no contract) → doctor price is applied
- No special price → default price list is applied
- Both contract and doctor prices exist → contract wins over doctor

#### Delete Protection Rule (BR-VAL-004)
Function: 1.4
- Success: delete test before any result is entered → allowed
- Failure: attempt delete after result is entered → must throw

#### History Read-Only Rule (BR-MED-008)
Function: 1.6
- Verify historical data cannot be modified
- Verify read access is allowed for authorized users

#### Auto Comment Rule (BR-MED-002)
Functions: 3.4, 4.1
- Verify high comment appears automatically when result > max
- Verify low comment appears automatically when result < min
- Verify no comment when result is within normal range

#### Audit Trail Rule (BR-SEC-002)
Functions: 1.2, 4.3, any Edit function
- Verify the username is recorded with every modification
- Verify the timestamp is recorded with every modification

#### Discount Limit Rule (BR-ACC-004)
Function: 2.2
- Edge: discount equals exactly 100% → allowed
- Failure: discount exceeds 100% → must throw or reject

#### Security Rule (BR-SEC-001)
Function: 10.2
- Verify a user without permission cannot access restricted screens
- Verify admin can assign/revoke permissions

### For Other Functions
Read the BR tags in the documentation for each function
and write one dedicated test per BR rule.
If no BR rule is listed, apply standard success/failure/edge coverage.

---

## TEST NAMING CONVENTION (MANDATORY)

Every test name MUST follow this exact pattern:
[FunctionName]_[StateUnderTest]_[ExpectedResult]

### Good Name Examples
AddPatient_WithValidData_ShouldSaveAndReturnLabId
AddPatient_WithMissingName_ShouldThrowValidationException
AddPatient_WithEmptyPhone_ShouldUseNullAndSave
DeleteTest_WhenResultAlreadyEntered_ShouldThrowInvalidOperation
CalculateTotal_WithContractPriceList_ShouldApplyContractPriceFirst
CalculateTotal_WithDoctorPriceAndNoContract_ShouldApplyDoctorPrice
SearchPatient_WithEmptyKeyword_ShouldReturnEmptyList
ApplyDiscount_WhenDiscountExceeds100Percent_ShouldThrow
ApplyDiscount_WhenDiscountIsExactly100Percent_ShouldAllow

### Bad Name Examples (NEVER use these)
TestAddPatient          ❌ no state or expected result
AddPatient_Test         ❌ "Test" is not a state
TestMethod1             ❌ completely meaningless
AddPatient_Success      ❌ missing expected result detail
Test_AddPatient_Works   ❌ "Works" is not a specific result

### Naming Rules
- Use PascalCase for all three parts
- Use underscore ONLY to separate the three parts
- The state must describe the specific condition being tested
- The result must describe the exact expected outcome
- Function name must match the documented function name in English

---

## COMMAND TYPE IN THIS PROJECT (MANDATORY)

### How Commands Work in Open_lab
This project uses a CUSTOM RelayCommand defined in:
Open_lab/ViewModels/RelayCommand.cs

It implements ICommand directly — it is NOT from CommunityToolkit.
It is NOT AsyncRelayCommand.
It accepts Action<object?> and optional Func<object?, bool>?.

### How to Call a Command in Tests

#### For synchronous Commands:
```csharp
// CORRECT
viewModel.TogglePasswordVisibilityCommand.Execute(null);
Assert.True(viewModel.IsPasswordVisible);
```

#### For Commands that run async logic internally:
```csharp
// CORRECT
viewModel.SaveCommand.Execute(null);
await Task.Delay(100);
Assert.Equal("LAB-001", viewModel.LabId);
```

#### NEVER use ExecuteAsync in this project:
```csharp
// WRONG - this project does not use AsyncRelayCommand
await viewModel.SaveCommand.ExecuteAsync(null); // ❌ does not exist
```

### How to Determine Wait Time
- Simple DB operations: await Task.Delay(100) is sufficient
- Complex operations with multiple DB calls: await Task.Delay(200)
- If test is flaky: increase delay by 50ms increments
- Always assert AFTER the Task.Delay, never before

### How to Instantiate ViewModels in Tests
Read the actual constructor of the ViewModel before writing any test.
The RelayCommand is created inside the ViewModel constructor.
You only need to inject the Service mock.

```csharp
// CORRECT - inject only the Service
var mockService = new Mock<IPatientService>();
var viewModel = new PatientRegistrationViewModel(mockService.Object);

// Then call the command
viewModel.SaveCommand.Execute(null);
await Task.Delay(100);

// Then assert
Assert.Equal("LAB-001", viewModel.LabId);
```

---

## TEST STRUCTURE: AAA (MANDATORY)

Every test MUST follow the AAA structure with explicit comments.
No exceptions. The first line inside every test must be
the function number comment (see FUNCTION NUMBER LINKING RULE).

```csharp
[Fact]
public async Task AddPatient_WithValidData_ShouldSaveAndReturnLabId()
{
    // Function: 1.1 — Add New Patient
    // Arrange
    var mockPatientService = new Mock<IPatientService>();
    var patient = new Patient { Name = "Ahmed Ali", Phone = "01234567890" };
    mockPatientService
        .Setup(s => s.AddPatientAsync(It.IsAny<Patient>()))
        .ReturnsAsync(new Patient { Id = 1, LabId = "LAB-001" });
    var viewModel = new PatientRegistrationViewModel(mockPatientService.Object);
    viewModel.PatientName = patient.Name;
    viewModel.PatientPhone = patient.Phone;

    // Act
    viewModel.SaveCommand.Execute(null);
    await Task.Delay(100);

    // Assert
    Assert.Equal("LAB-001", viewModel.LabId);
    mockPatientService.Verify(
        s => s.AddPatientAsync(It.IsAny<Patient>()),
        Times.Once);
}
```

### AAA Rules
- The comment // Function: X.X must be the FIRST line in the test body
- The comment // Arrange MUST appear before setup code
- The comment // Act MUST appear before the single action being tested
- The comment // Assert MUST appear before all assertions
- Each section must be visually separated by a blank line
- The Act section contains the Execute call AND the Task.Delay
- Never mix Act and Assert in the same line

---

## MOCK USAGE RULES (MANDATORY)

### Rule 1: Always Mock Dependencies
Every external dependency MUST be mocked.
Never use real database connections in unit tests (SQL Server / networked DB).
EF Core InMemory is allowed for Service-layer tests in this project
because it does not use external resources and provides realistic
query/relationship behavior.
Never use real file system in unit tests.

```csharp
// CORRECT
var mockService = new Mock<IPatientService>();

// WRONG - never do this
var realService = new PatientService(realDbContext);
```

### Rule 2: Always Setup Expected Behavior
Every Mock MUST have explicit Setup before the Act.

```csharp
// CORRECT
mockService
    .Setup(s => s.GetPatientAsync(It.IsAny<int>()))
    .ReturnsAsync(new Patient { Id = 1, Name = "Ahmed Ali" });

// WRONG - no setup means undefined behavior
var mockService = new Mock<IPatientService>();
// using mockService without Setup
```

### Rule 3: Always Verify Critical Calls
For write operations (Add, Edit, Delete, Save, Record, Set)
you MUST use Verify to confirm the Service was called.

```csharp
// CORRECT for write operations
mockService.Verify(
    s => s.AddPatientAsync(It.IsAny<Patient>()),
    Times.Once);

// WRONG - no verification for write operations
```

### Rule 4: Verify is RECOMMENDED for read operations
For read operations (Get, Load, Search, View)
Verify is recommended but not blocking if missing.
List it as a recommendation in your session report.

### Rule 5: Never Verify what you did not Setup
Every Verify call must correspond to a Setup call.

---

## ASSERT RULES (MANDATORY)

### Rule 1: Assert Real Business Values
Every test MUST assert actual business results.

```csharp
// CORRECT - asserting real business result
Assert.Equal("LAB-001", result.LabId);
Assert.True(result.IsActive);
Assert.Equal(250.00m, invoice.TotalAmount);
Assert.Equal("Ahmed Ali", patient.Name);

// WRONG - asserting nothing meaningful
Assert.NotNull(result);           // too weak
Assert.IsType<Patient>(result);   // too weak
Assert.True(true);                // completely fake
```

### Rule 2: Assert Exception Type AND Message for Failure Tests
```csharp
// CORRECT
var ex = await Assert.ThrowsAsync<ValidationException>(
    () => service.AddPatientAsync(invalidPatient));
Assert.Contains("Name", ex.Message);

// WRONG - only checking exception was thrown
await Assert.ThrowsAsync<Exception>(
    () => service.AddPatientAsync(invalidPatient));
```

### Rule 3: Assert Property Changes in ViewModel Tests
For ViewModel tests you MUST assert that Properties changed
after the Command executed.

```csharp
// CORRECT
viewModel.SaveCommand.Execute(null);
await Task.Delay(100);
Assert.Equal("LAB-001", viewModel.LabId);
Assert.Empty(viewModel.ErrorMessage);
Assert.True(viewModel.IsSaved);

// WRONG - only asserting service was called
mockService.Verify(s => s.AddPatientAsync(...), Times.Once);
// missing: what changed in ViewModel after the call?
```

### Rule 4: Assert Error Message in Failure Tests
For failure tests in ViewModel you MUST assert
that the error message property is set and not empty.

```csharp
// CORRECT
viewModel.SaveCommand.Execute(null);
await Task.Delay(100);
Assert.NotEmpty(viewModel.ErrorMessage);
Assert.Contains("required", viewModel.ErrorMessage);

// WRONG - not checking user feedback
Assert.Null(result);
```

---

## FAKE TEST DETECTION RULES

A test is FAKE if ANY of these conditions are true.
NEVER write a fake test.
If you find a fake test you MUST replace it.

### Condition 1: Empty or Trivial Assert
```csharp
Assert.True(true);              // FAKE
Assert.NotNull(new object());   // FAKE
// no Assert at all             // FAKE
```

### Condition 2: Only Checks No Exception Was Thrown
```csharp
// FAKE - only checking it did not crash
var exception = Record.Exception(() => service.DoSomething());
Assert.Null(exception);
// missing: what was the actual result?
```

### Condition 3: Hardcoded Unrelated Data
```csharp
// FAKE - data has nothing to do with business logic
Assert.Equal(42, someResult);
Assert.Equal("test", name);
```

### Condition 4: No Relationship to Business Logic
```csharp
// FAKE - testing infrastructure not business logic
Assert.NotNull(new PatientViewModel(mockService.Object));
```

### Condition 5: Only Checks That Method Was Called
```csharp
// FAKE - verifying a call without asserting its result
mockService.Verify(s => s.AddPatientAsync(It.IsAny<Patient>()), Times.Once);
// missing: did the ViewModel update its properties correctly?
```

---

## WEAK TEST IDENTIFICATION

A test is WEAK (but not fake) if ANY of these are true:
- Tests success scenario only — no failure test exists for this function
- Uses Mock without Verify for write operations
- Test name is not descriptive (e.g. AddPatient_Success)
- Does not follow AAA structure clearly
- Missing the function number comment // Function: X.X

Weak tests MUST be improved in place, not deleted.
Improve and document the improvement in your session report.

---

## DUPLICATE TEST DETECTION RULES

Two tests are DUPLICATES if ALL of the following are true:
- They test the same function (same function number)
- They test the same scenario (same state under test)
- They assert the same expected result

When duplicates are found:
- Keep the one with the strongest assertions and best naming
- Delete all weaker duplicates
- Document every deletion in your session report

---

## TEST CLASS NAMING CONVENTION

```csharp
// Service test class
public class PatientServiceTests { }

// ViewModel test class
public class PatientRegistrationViewModelTests { }
```

### One Class Per Layer Per Feature
- PatientServiceTests.cs → all Service tests for patient functions
- PatientRegistrationViewModelTests.cs → all ViewModel tests
- Do NOT mix Service and ViewModel tests in the same class

---

## DEPENDENCY INJECTION IN TESTS

Always use constructor injection for ViewModels in tests.
Never use service locator or static dependencies.
Read the actual ViewModel constructor before writing the test.

```csharp
// CORRECT - matching the actual constructor
var mockService = new Mock<IPatientService>();
var viewModel = new PatientRegistrationViewModel(mockService.Object);

// WRONG - assuming a parameterless constructor
var viewModel = new PatientRegistrationViewModel();
```

---

## ASYNC TEST RULES

All tests that call async Service methods MUST be async.
All tests that call Commands with internal async logic MUST be async.

```csharp
// CORRECT
[Fact]
public async Task AddPatient_WithValidData_ShouldSaveAndReturnLabId()
{
    // Function: 1.1 — Add New Patient
    // Arrange
    ...
    // Act
    viewModel.SaveCommand.Execute(null);
    await Task.Delay(100);
    // Assert
    ...
}

// WRONG - can cause flaky or missed assertions
[Fact]
public void AddPatient_WithValidData_ShouldSaveAndReturnLabId()
{
    viewModel.SaveCommand.Execute(null);
    // no await = assertions run before async logic finishes
}
```

---

## OUTPUT FORMAT (MANDATORY AFTER EVERY SESSION)

After completing work on each function report the following.

### Per-Function Session Report

**Function:** [Function Number] — [Function Name from documentation]
**Module:** [Module Name]

| Layer | Found | Fake Replaced | Weak Improved | Duplicate Removed | New Written | Final Status |
|-------|-------|---------------|---------------|-------------------|-------------|--------------|
| Service | [n] | [n] | [n] | [n] | [n] | ✅/🔄/❌ |
| ViewModel | [n] | [n] | [n] | [n] | [n] | ✅/🔄/❌ |

**Business Rules Covered:**
- BR-XXX-XXX: [test name that covers this rule]

**Test Names Written or Improved:**
- [exact test method name] — [New / Improved / Replaced / Removed-Duplicate]

**Overall Function Status:** ✅ Complete / 🔄 Partial / ⚠️ Needs Review

---

### Final Summary Report (MANDATORY AT END OF FULL SESSION)

After completing all 97 functions provide this consolidated table:

| # | Module | Functions | Service ✅ | ViewModel ✅ | Overall |
|---|--------|-----------|------------|-------------|---------|
| 1 | Patient Management | 8 | [n]/8 | [n]/8 | [%] |
| 2 | Financial Accounting | 13 | [n]/13 | [n]/13 | [%] |
| 3 | Test & Price Management | 9 | [n]/9 | [n]/9 | [%] |
| 4 | Result Entry & Reporting | 9 | [n]/9 | [n]/9 | [%] |
| 5 | Culture & Sensitivity | 7 | [n]/7 | [n]/7 | [%] |
| 6 | Sample Collection | 4 | [n]/4 | [n]/4 | [%] |
| 7 | Work Sheets | 4 | [n]/4 | [n]/4 | [%] |
| 8 | External Labs | 7 | [n]/7 | [n]/7 | [%] |
| 9 | Statistics & Analytics | 6 | [n]/6 | [n]/6 | [%] |
| 10 | User Management & Security | 8 | [n]/8 | [n]/8 | [%] |
| 11 | HR & Attendance | 5 | [n]/5 | [n]/5 | [%] |
| 12 | Contracts & Referrals | 9 | [n]/9 | [n]/9 | [%] |
| 13 | System Settings | 8 | [n]/8 | [n]/8 | [%] |
| | **TOTAL** | **97** | **[n]/97** | **[n]/97** | **[%]** |

Also provide:
- Total fake tests replaced: [n]
- Total weak tests improved: [n]
- Total duplicate tests removed: [n]
- Total new tests written: [n]
- Total tests in project after session: [n]

---

## CRITICAL RULES

- NEVER modify production code in Open_lab project
- NEVER write a test without reading the function documentation first
- NEVER write a test without checking existing tests first
- NEVER use real DbContext or real database in unit tests
- NEVER write Assert.True(true) or equivalent fake assertions
- NEVER skip the Failure test for any function
- NEVER skip AAA comments in any test
- NEVER skip the function number comment in any test
- NEVER put more than ONE action in the Act section
- NEVER mix concerns: one test = one scenario = one assertion focus
- NEVER use ExecuteAsync — this project uses Execute(null) only
- NEVER rely on any external tracker file to determine coverage
- NEVER open, read, or parse unit_test_result.md for any reason
- NEVER assume a function is untested just because its
  function number comment is absent — read the test body first
- NEVER write a duplicate test for a scenario already covered
- ALWAYS read Open_lab/Docs/Open_lab_Modules_Documentation.md first
- ALWAYS follow the FUNCTION DISCOVERY PROCESS before writing
- ALWAYS read existing test files before writing new tests
- ALWAYS use Mock<IXxxService> for all dependencies
- ALWAYS use Setup before using a Mock
- ALWAYS use Verify for write operations
- ALWAYS assert business values not just types or nulls
- ALWAYS assert ViewModel property changes after Command execution
- ALWAYS assert error messages in failure scenarios
- ALWAYS write a dedicated test for each BR-XXX-XXX business rule
- ALWAYS read the actual ViewModel constructor before writing tests
- ALWAYS use await Task.Delay after Execute for async Commands
- ALWAYS remove duplicate tests keeping only the strongest one