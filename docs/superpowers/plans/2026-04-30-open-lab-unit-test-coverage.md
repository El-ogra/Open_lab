# Open_lab Unit Test Coverage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** تغطية جميع الوظائف الـ97 الموثقة باختبارات (Service + ViewModel) وفق UnitTest_Skill.md وتحديث unit_test_result.md بناءً على التنفيذ الفعلي ونجاح الاختبارات.

**Architecture:** اختبارات الـViewModel تعتمد على Moq لواجهات الخدمات. اختبارات الـService تعتمد على EF Core InMemory لتشغيل منطق EF/Queries بدون SQL Server حقيقي. تتبع التغطية يتم تحديثه بعد كل دفعة مع تشغيل dotnet test.

**Tech Stack:** .NET 8 (net8.0-windows), xUnit, Moq, FluentAssertions, EF Core InMemory, WPF MVVM.

---

## مراجع إلزامية قبل التنفيذ
- التوثيق (97 وظيفة): [Open_lab_Modules_Documentation.md](file:///workspace/Open_lab/Docs/Open_lab_Modules_Documentation.md)
- تعليمات الاختبارات: [UnitTest_Skill.md](file:///workspace/Open_lab/Docs/UnitTest_Skill.md)
- تتبع التغطية: [unit_test_result.md](file:///workspace/Open_lab/Docs/unit_test_result.md)
- خرائط الربط بين الوظائف والطبقات: [DocumentationFunctionCoverageTests.cs](file:///workspace/Open_lab.Tests/Services/DocumentationFunctionCoverageTests.cs)

## ملف/مجلدات سيتم العمل عليها
**تعديل/إضافة داخل مشروع الاختبارات فقط:**
- Modify: [unit_test_result.md](file:///workspace/Open_lab/Docs/unit_test_result.md)
- Modify/Create: `Open_lab.Tests/Services/*.cs`
- Modify/Create: `Open_lab.Tests/ViewModels/*.cs`
- Reuse: [InMemoryDbContextFactory.cs](file:///workspace/Open_lab.Tests/Infrastructure/InMemoryDbContextFactory.cs)

## أوامر التحقق القياسية
- تشغيل كل الاختبارات:
  - Run: `dotnet test /workspace/Open_lab.sln -c Debug`
  - Expected: `Passed: <n>, Failed: 0`
- تشغيل فئة واحدة (اختياري):
  - Run: `dotnet test /workspace/Open_lab.sln -c Debug --filter FullyQualifiedName~Open_lab.Tests.Services`

---

### Task 1: تثبيت “تعريف التغطية” كنقطة فحص آلية

**Files:**
- Modify: [unit_test_result.md](file:///workspace/Open_lab/Docs/unit_test_result.md)
- Create: `/workspace/Open_lab.Tests/Services/UnitTestCoverageEnforcementTests.cs`

- [ ] **Step 1: إضافة اختبار يضمن أن ملف التوثيق يحتوي 97 وظيفة**

هذا موجود فعلاً في [DocumentationFunctionCoverageTests.cs](file:///workspace/Open_lab.Tests/Services/DocumentationFunctionCoverageTests.cs#L20-L28) ولا يحتاج تغيير. الهدف هنا هو الاعتماد عليه كـGuard.

- [ ] **Step 2: إنشاء اختبار Enforcement يقرأ unit_test_result.md ويتأكد أن أي وظيفة معلَّمة ✅ لديها أسماء اختبارات غير فارغة**

```csharp
using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class UnitTestCoverageEnforcementTests
    {
        [Fact]
        public void CoverageTracker_When_FunctionMarkedComplete_Should_List_TestNames()
        {
            var path = GetCoverageTrackerPath();
            var lines = File.ReadAllLines(path);

            var completeRows = lines
                .Where(l => l.Contains("|") && l.Contains("✅") && !l.Contains("رقم الوظيفة"))
                .ToList();

            foreach (var row in completeRows)
            {
                var parts = row.Split('|').Select(p => p.Trim()).ToArray();
                parts.Length.Should().BeGreaterThan(6, $"row should have all columns: {row}");

                var functionId = parts[1];
                var statusColumn = parts[5];
                var testsColumn = parts[6];

                if (statusColumn.StartsWith("✅", StringComparison.Ordinal))
                {
                    testsColumn.Should().NotBeNullOrWhiteSpace($"function {functionId} is ✅ but has no listed tests");
                }
            }
        }

        private static string GetCoverageTrackerPath()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var sln = Path.Combine(dir.FullName, "Open_lab.sln");
                if (File.Exists(sln))
                {
                    return Path.Combine(dir.FullName, "Open_lab", "Docs", "unit_test_result.md");
                }

                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate repository root (Open_lab.sln).");
        }
    }
}
```

- [ ] **Step 3: تشغيل الاختبارات للتأكد أن الاختبار الجديد يمر**

Run: `dotnet test /workspace/Open_lab.sln -c Debug --filter FullyQualifiedName~UnitTestCoverageEnforcementTests`  
Expected: `Passed: 1, Failed: 0`

---

### Task 2: تثبيت قوالب اختبارات ViewModel (Moq) + Service (EF InMemory)

**Files:**
- Reuse: [InMemoryDbContextFactory.cs](file:///workspace/Open_lab.Tests/Infrastructure/InMemoryDbContextFactory.cs)
- Create: `/workspace/Open_lab.Tests/Infrastructure/TestSeed.cs`
- Reference existing patterns: [PatientRegistrationViewModelTests.cs](file:///workspace/Open_lab.Tests/ViewModels/PatientRegistrationViewModelTests.cs)

- [ ] **Step 1: إنشاء TestSeed لتجميع بيانات Seed متكررة**

```csharp
using System;
using Open_lab.Models;

namespace Open_lab.Tests.Infrastructure
{
    public static class TestSeed
    {
        public static Patient Patient(string labId = "LAB-001")
        {
            return new Patient
            {
                LabId = labId,
                FullName = "Test Patient",
                Gender = "Male",
                Phone = "01000000000",
                BirthDate = new DateTime(1990, 1, 1)
            };
        }

        public static Test Test(string code = "T-001", decimal price = 50m)
        {
            return new Test
            {
                Code = code,
                NameReport = code,
                NameReceipt = code,
                Price = price
            };
        }
    }
}
```

- [ ] **Step 2: إنشاء قالب Service Test باستخدام EF InMemory**

مثال عملي (يُستخدم كنمط عند كتابة بقية اختبارات الخدمات):

```csharp
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class ServiceTestTemplateTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public ServiceTestTemplateTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateAsync_WithValidPatient_Should_CreatePatient_SuccessTemplate()
        {
            // Function: 1.1 — Add New Patient
            // Arrange
            var service = new PatientService(_db);
            var patient = TestSeed.Patient();

            // Act
            var created = await service.CreateAsync(patient);

            // Assert
            created.PatientId.Should().BeGreaterThan(0);
            created.LabId.Should().Be("LAB-001");
        }
    }
}
```

- [ ] **Step 3: التأكد أن القالب يمر**

Run: `dotnet test /workspace/Open_lab.sln -c Debug --filter FullyQualifiedName~ServiceTestTemplateTests`  
Expected: `Passed: 1, Failed: 0`

---

### Task 3: تنفيذ التغطية Module-by-Module (تكرار نفس النمط مع تغيير الملفات/السيناريوهات)

**Files:**
- Modify: [unit_test_result.md](file:///workspace/Open_lab/Docs/unit_test_result.md)
- Modify/Create: `Open_lab.Tests/Services/*ServiceTests*.cs`
- Modify/Create: `Open_lab.Tests/ViewModels/*ViewModelTests*.cs`

**قاعدة العمل لكل وظيفة x.y:**
- [ ] قراءة وصف الوظيفة والـBR tags من توثيق الوظائف.
- [ ] قراءة tests الحالية للـService والـViewModel المرتبطة.
- [ ] كتابة/تحسين:
  - Service: Success + Failure + Edge
  - ViewModel: Success + Failure + Edge (إذا ينطبق)
  - BR: اختبار مستقل لكل BR
- [ ] تشغيل dotnet test (أو فلترة على الملف/الـnamespace).
- [ ] تحديث صف الوظيفة في unit_test_result.md (Service/ViewModel/Status + أسماء الاختبارات).

**ملاحظة تنظيمية:** الربط بين الوظيفة وملفات الخدمة/الـViewModel موجود في [DocumentationFunctionCoverageTests.cs](file:///workspace/Open_lab.Tests/Services/DocumentationFunctionCoverageTests.cs#L123-L218). يُستخدم لتحديد الملفات بسرعة.

---

### Task 4: تحديث unit_test_result.md بطريقة ثابتة بعد كل دفعة

**Files:**
- Modify: [unit_test_result.md](file:///workspace/Open_lab/Docs/unit_test_result.md)

- [ ] **Step 1: تحديث صف الوظيفة بعد نجاح الاختبارات**

نموذج تحديث (مثال فقط على الشكل):
- Service ✅ إذا (Success + Failure + Edge) مكتملة
- ViewModel ✅ إذا (Success + Failure + Edge إن لزم) مكتملة
- Status ✅ إذا Service/ViewModel كلاهما ✅
- “أسماء الاختبارات المكتوبة”: قائمة أسماء الـtest methods التي تغطي الوظيفة

- [ ] **Step 2: تحديث جدول الملخص في نهاية الملف**

تحديث أرقام (مكتمل/جزئي/لم يبدأ/يحتاج مراجعة) بما يطابق الصفوف.

- [ ] **Step 3: تشغيل Enforcement test**

Run: `dotnet test /workspace/Open_lab.sln -c Debug --filter FullyQualifiedName~UnitTestCoverageEnforcementTests`  
Expected: `Passed: 1, Failed: 0`

---

## مراجعة ذاتية للخطة (تم)
1) تغطية المتطلبات: الخطة تشمل كتابة اختبارات ViewModel بـMoq وService بـInMemory وتحديث unit_test_result.md والتحقق بـdotnet test.  
2) فحص الـPlaceholders: لا يوجد TBD/TODO. يوجد نماذج كود فعلية وأوامر تشغيل/نتائج متوقعة.  
3) اتساق التسمية: المسارات والأسماء تشير إلى ملفات موجودة في الريبو، وأسماء الاختبارات تتبع نمط المشروع.

---

## تسليم التنفيذ
تم حفظ الخطة في: `docs/superpowers/plans/2026-04-30-open-lab-unit-test-coverage.md`.

خياران للتنفيذ:
1) **Subagent-Driven (موصى به)** — أُنجز المهام واحدة واحدة مع مراجعة بعد كل مهمة.
2) **Inline Execution** — تنفيذ مباشر في هذه الجلسة بخطوات متتابعة مع نقاط توقف للمراجعة.

أي خيار تفضّل؟

