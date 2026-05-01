# Open_lab Unit Test Coverage (Fix + Complete) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** جعل جميع اختبارات الوحدة تمر بنجاح (0 Failed) ثم استكمال تغطية وظائف النظام الـ97 وفق UnitTest_Skill.md مع تحديث unit_test_result.md ليعكس الحالة الحقيقية.

**Architecture:** نثبت .NET SDK 8 محلياً داخل الـworkspace لتشغيل `dotnet test`. نبدأ بإصلاح الاختبارات الفاشلة الحالية (13 Failed حسب تشغيل المستخدم) حتى تصبح المجموعة خضراء، ثم نستخدم فحصاً آلياً يعتمد على تعليقات `// Function: x.y` داخل الاختبارات لاكتشاف الفجوات بدقة، وبعدها نُكمل التغطية موديول-موديول مع تشغيل الاختبارات وتحديث unit_test_result.md بعد كل دفعة.

**Tech Stack:** .NET 8 SDK (local install), xUnit, Moq, FluentAssertions, EF Core InMemory, WPF MVVM (RelayCommand).

---

## قيود إلزامية
- ممنوع تعديل/إضافة/حذف أي كود داخل مشروع الإنتاج: `/workspace/Open_lab/**`
- مسموح فقط:
  - تعديل/إضافة اختبارات داخل: `/workspace/Open_lab.Tests/**`
  - تحديث التتبع داخل: `/workspace/Open_lab/Docs/unit_test_result.md`
- الالتزام الصارم بملف: `/workspace/Open_lab/Docs/UnitTest_Skill.md`

---

## مراجع قبل أي تعديل
- التوثيق: `/workspace/Open_lab/Docs/Open_lab_Modules_Documentation.md`
- تعليمات الاختبارات: `/workspace/Open_lab/Docs/UnitTest_Skill.md`
- تتبع النتائج: `/workspace/Open_lab/Docs/unit_test_result.md` (يُعامل كملف ناتج يتم تصحيحه وليس كمصدر حقيقة)

---

## Task 1: تجهيز تشغيل الاختبارات داخل بيئة العمل (تثبيت .NET SDK 8 محلياً)

**Files:**
- Create (optional): `/workspace/.local-dotnet/` (directory)

- [ ] **Step 1: تنزيل dotnet-install.sh**

Run:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /workspace/.local-dotnet/dotnet-install.sh
chmod +x /workspace/.local-dotnet/dotnet-install.sh
```

- [ ] **Step 2: تثبيت .NET SDK 8 داخل workspace**

Run:

```bash
/workspace/.local-dotnet/dotnet-install.sh --version 8.0.204 --install-dir /workspace/.local-dotnet/sdk
```

- [ ] **Step 3: التأكد من dotnet**

Run:

```bash
export PATH="/workspace/.local-dotnet/sdk:$PATH"
dotnet --info
```

Expected: ظهور معلومات .NET 8.

---

## Task 2: إعادة إنتاج الـ13 فشل وتجميع قائمة الاختبارات الفاشلة

**Files:**
- None

- [ ] **Step 1: تشغيل كل الاختبارات وتسجيل الفشل**

Run:

```bash
export PATH="/workspace/.local-dotnet/sdk:$PATH"
dotnet test /workspace/Open_lab.sln -c Debug --nologo
```

Expected: تقرير يوضح قائمة الاختبارات الفاشلة (أسماء + StackTrace).

- [ ] **Step 2: استخراج قائمة (اسم الاختبار + الملف) لكل فشل**

ملاحظة: إذا لم تُظهر المخرجات مسار الملف بوضوح، أعد التشغيل مع:

```bash
dotnet test /workspace/Open_lab.sln -c Debug --nologo -v normal
```

---

## Task 3: إصلاح الاختبارات الفاشلة فقط (بدون تعديل كود الإنتاج)

**Files:**
- Modify: `/workspace/Open_lab.Tests/**/*.cs`

- [ ] **Step 1: لكل اختبار فاشل، اقرأ الاختبار والـArrange وتأكد من مطابقته لوحدة العمل**
- [ ] **Step 2: أصلح سبب الفشل داخل الاختبار (Seed/Setup/Assertions/Flakiness)**
- [ ] **Step 3: شغّل كل الاختبارات للتأكد أن الفشل اختفى**

Run:

```bash
export PATH="/workspace/.local-dotnet/sdk:$PATH"
dotnet test /workspace/Open_lab.sln -c Debug --nologo
```

- [ ] **Step 4: شغّل كل الاختبارات للتأكد من 0 Failed**

Run:

```bash
export PATH="/workspace/.local-dotnet/sdk:$PATH"
dotnet test /workspace/Open_lab.sln -c Debug --nologo
```

Expected: `Failed: 0`.

---

## Task 4: تدقيق التغطية بشكل آلي بدل الاعتماد على unit_test_result.md

**Files:**
- Create: `/workspace/Open_lab.Tests/Services/FunctionIdTraceabilityTests.cs`

- [ ] **Step 1: إنشاء اختبار يقرأ كل ملفات الاختبارات ويستخرج Function IDs من التعليقات**

Create file:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class FunctionIdTraceabilityTests
    {
        private static readonly Regex FunctionIdRegex = new Regex(@"//\\s*Function:\\s*(\\d+\\.\\d+)", RegexOptions.Compiled);
        private static readonly Regex DocFunctionIdRegex = new Regex(@"^####\\s+(\\d+\\.\\d+)", RegexOptions.Multiline | RegexOptions.Compiled);

        [Fact]
        public void Every_Documented_Function_Should_Have_AtLeastOne_TestCommentReference()
        {
            var documented = ExtractDocumentedIds();
            var referenced = ExtractReferencedIdsFromTests();

            var missing = documented.Except(referenced).OrderBy(x => x).ToList();
            missing.Should().BeEmpty("missing Function ID references in tests: " + string.Join(", ", missing));
        }

        [Fact]
        public void Each_Function_Should_Have_Service_And_ViewModel_Test_References()
        {
            var documented = ExtractDocumentedIds();
            var references = ExtractReferencesWithLayer();

            var missingPairs = new List<string>();

            foreach (var id in documented)
            {
                var hasService = references.Any(r => r.FunctionId == id && r.Layer == "Service");
                var hasViewModel = references.Any(r => r.FunctionId == id && r.Layer == "ViewModel");

                if (!hasService || !hasViewModel)
                {
                    missingPairs.Add($"{id}: {(hasService ? "" : "Service ")}{(hasViewModel ? "" : "ViewModel")}".Trim());
                }
            }

            missingPairs.Should().BeEmpty("missing layer references: " + string.Join(" | ", missingPairs));
        }

        private static IReadOnlyList<string> ExtractDocumentedIds()
        {
            var doc = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "Open_lab", "Docs", "Open_lab_Modules_Documentation.md"));
            return DocFunctionIdRegex.Matches(doc).Select(m => m.Groups[1].Value).Distinct().ToList();
        }

        private static IReadOnlyList<string> ExtractReferencedIdsFromTests()
        {
            return ExtractAllTestFiles()
                .SelectMany(f => FunctionIdRegex.Matches(File.ReadAllText(f)).Select(m => m.Groups[1].Value))
                .Distinct()
                .ToList();
        }

        private static IReadOnlyList<(string FunctionId, string Layer)> ExtractReferencesWithLayer()
        {
            var root = GetRepositoryRoot();
            return ExtractAllTestFiles()
                .SelectMany(f =>
                {
                    var layer = f.Contains(Path.Combine(root, "Open_lab.Tests", "Services") + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                        ? "Service"
                        : "ViewModel";

                    var ids = FunctionIdRegex.Matches(File.ReadAllText(f)).Select(m => m.Groups[1].Value);
                    return ids.Select(id => (FunctionId: id, Layer: layer));
                })
                .Distinct()
                .ToList();
        }

        private static IReadOnlyList<string> ExtractAllTestFiles()
        {
            var root = GetRepositoryRoot();
            var testRoot = Path.Combine(root, "Open_lab.Tests");
            return Directory.GetFiles(testRoot, "*.cs", SearchOption.AllDirectories)
                .Where(p => !p.Contains(Path.Combine(testRoot, "obj") + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .ToList();
        }

        private static string GetRepositoryRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "Open_lab.sln")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate repository root (Open_lab.sln).");
        }
    }
}
```

- [ ] **Step 2: تشغيل الاختبار الجديد**

Run:

```bash
export PATH="/workspace/.local-dotnet/sdk:$PATH"
dotnet test /workspace/Open_lab.sln -c Debug --filter FullyQualifiedName~FunctionIdTraceabilityTests --nologo
```

Expected:
- PASS إذا كانت كل الوظائف 97 لها مراجع في الاختبارات.
- FAIL مع قائمة IDs ناقصة إذا كان هناك فجوات حقيقية (وهذا ما سنعالجّه في المهام التالية).

---

## Task 5: استكمال التغطية موديول-موديول (5 ثم 6 ثم 7 ثم 8 ثم 10 ثم 12 ثم 13)

**Files:**
- Modify/Create: `/workspace/Open_lab.Tests/Services/*.cs`
- Modify/Create: `/workspace/Open_lab.Tests/ViewModels/*.cs`
- Modify: `/workspace/Open_lab/Docs/unit_test_result.md`

**قاعدة العمل لكل وظيفة x.y:**
- [ ] اقرأ وصف الوظيفة و BR-XXX-XXX من التوثيق.
- [ ] اقرأ الاختبارات الحالية للـService والـViewModel المتعلقة بالوظيفة (لا تكرر).
- [ ] أكمل الناقص:
  - Service: Success + Failure + Edge
  - ViewModel: Success + Failure + Edge (أو سبب واضح داخل class إذا لا ينطبق)
  - BR: اختبار مستقل لكل BR
- [ ] شغّل `dotnet test` (فلترة على ملف/namespace عند الإمكان).
- [ ] حدث `unit_test_result.md` فوراً بعد نجاح الدفعة.

**تشغيل بعد كل موديول:**

```bash
export PATH="/workspace/.local-dotnet/sdk:$PATH"
dotnet test /workspace/Open_lab.sln -c Debug --nologo
```

---

## Task 6: تصحيح unit_test_result.md ليصبح “المصدر الرسمي”

**Files:**
- Modify: `/workspace/Open_lab/Docs/unit_test_result.md`

- [ ] **Step 1: بعد اكتمال كل موديول، عدّل صفوف الوظائف بما يطابق الاختبارات الموجودة فعلياً**
- [ ] **Step 2: حدّث جدول الملخص في نهاية الملف**
- [ ] **Step 3: شغّل FunctionIdTraceabilityTests كحاجز جودة**

Run:

```bash
export PATH="/workspace/.local-dotnet/sdk:$PATH"
dotnet test /workspace/Open_lab.sln -c Debug --filter FullyQualifiedName~FunctionIdTraceabilityTests --nologo
```

---

## Task 7: التحقق النهائي (قبل التسليم)

**Files:**
- None

- [ ] **Step 1: تشغيل كل الاختبارات**
- [ ] **Step 2: التأكد من 0 Failed**
- [ ] **Step 3: التأكد أن جميع الوظائف 97 لها Service + ViewModel references وأن tracker محدث**

Run:

```bash
export PATH="/workspace/.local-dotnet/sdk:$PATH"
dotnet test /workspace/Open_lab.sln -c Debug --nologo
```
