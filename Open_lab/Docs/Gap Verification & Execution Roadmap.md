
# Gap Verification & Execution Roadmap

## 1. Repository Verification

| الحقل | القيمة |
|---|---|
| **Actual Commit Hash** | `227815f5666e044412bff03058b95fef23c8839f` |
| **Actual Commit Message** | `تحسينات شاشات المرضي 7` |
| **Expected Commit Message** | `تحسينات شاشات المرضى 7` |
| **Branch** | `Fi5ve` |
| **Commit Date** | 2026-05-21 12:44:35 +0300 |
| **Author** | medowemado |
| **Verification Status** | ✅ تطابق (مع ملاحظة فرق إملائي طفيف "المرضي" بدل "المرضى" — الجوهر مطابق ولا يوجد إيقاف) |

ملف التدقيق المُستخرَج منه: `Open_lab/Docs/Medical_Catalog_Readiness_Audit.md` (345 سطر، نُسخته الحالية على رأس الفرع تشير إلى Commit سابق `dea0ed8b` كقاعدة تحليل — هذا طبيعي لأن التقرير كُتب قبل آخر كوميت).

---

## 2. Extracted Gaps from Audit File

القائمة الكاملة كما وردت حرفياً في الجدول `Detected Gaps` (الأسطر 317–336) من ملف `Docs/Medical_Catalog_Readiness_Audit.md`:

| # | الفجوة | الملف المُشار إليه في التقرير | السطر |
|---|---|---|---|
| G1 | لا يوجد Production Seeder للـ Tests/Units/Ranges | `Open_lab/Data/DevDataSeeder.cs` | 1 |
| G2 | الـ DevSeeder لا يضيف أي TestReferenceRange | `DevDataSeeder.cs` | 97–122 |
| G3 | Reference Range مرتبط بـ Test وليس Parameter | `Models/Entities.cs` | 232–243 |
| G4 | لا يوجد دعم IsPregnant في Range | `Models/Entities.cs` | 231–243 |
| G5 | لا يوجد Age Unit (months/years) | `Models/Entities.cs` | 236–237 |
| G6 | الـ Flag غير مُنَمَّط (Enum) | `ResultsService.cs` vs `DevDataSeeder.cs` | 295,300 / 161–168 |
| G7 | التقرير المطبوع لا يعرض Unit ولا Range | `Services/PrintService.cs` | 77–87 |
| G8 | VisitReportData لا يحوي Range/Unit | `Models/ReportModels.cs` | 14–19 |
| G9 | لا يوجد Bulk Insert / Transaction للاستيراد | `Services/TestCatalogService.cs` | 51–53 |
| G10 | لا يوجد Upsert API على TestCatalogService | `Services/TestCatalogService.cs` | 45–49 |
| G11 | لا توجد NuGet Package لقراءة Excel/CSV/JSON | `Open_lab.csproj` | 13–22 |
| G12 | لا يوجد ParameterId في ResultsService.ValidateResultAsync | `Services/ResultsService.cs` | 271–285 |
| G13 | لا يوجد IsActive على Test/Range (Soft-Delete) | `Models/Entities.cs` | 173–204 |
| G14 | لا توجد شاشات منفصلة لـ Groups/Units/SampleTypes | `ViewModels/` | — |
| G15 | لا يوجد فهرس فريد على DB لـ TestGroup.GroupName | `Data/OpenLabDbContext.cs` | 204–208 |
| G16 | لا يُحفظ RangeId المستخدم وقت إدخال النتيجة | `Models/Entities.cs` (ResultValue) | 298–312 |
| G17 | ResultEntryItem لا يحوي Unit ولا Range Min/Max | `ViewModels/UiModels.cs` | 93–127 |

---

## 3. Gap Verification Matrix

(جدول التحقق المستقل بعد فحص الكود الحالي مباشرة — كل دليل موثَّق بالملف والسطر مع مقتطف حرفي)

| # | الوصف المختصر | الحالة | الدليل المباشر من الكود الحالي | التأثير التشغيلي/السريري | اعتماديات | ملاحظة |
|---|---|---|---|---|---|---|
| **G1** | لا يوجد Production Seeder | ✅ **مؤكدة** | `Data/DevDataSeeder.cs:1` ⇒ `#if DEBUG` يُغلِّف كامل الكلاس + `App.xaml.cs:32` يستدعيه فقط في DEBUG | لا توجد بيانات كتالوج عند نشر النظام في معمل جديد | يعتمد على G3, G9, G10, G11 | — |
| **G2** | DevSeeder بلا Ranges | ✅ **مؤكدة** | `grep "TestReferenceRange" Data/DevDataSeeder.cs` ⇒ صفر نتائج. الـ Seeder يضيف Tests + Parameters + ResultValues فقط | اختبار AutoValidate لا يعمل سريرياً على بيانات الـ DEBUG | يعتمد على G3 | — |
| **G3** | Range مرتبطة بـ Test وليس Parameter | ✅ **مؤكدة** | `Models/Entities.cs:231-243`:<br>`public class TestReferenceRange { public int RangeId; public int TestId; ... }`<br>**لا يوجد ParameterId**.<br>`OpenLabDbContext.cs:221-230`:<br>`entity.HasOne(e => e.Test).WithMany(e => e.ReferenceRanges).HasForeignKey(e => e.TestId);` | CBC/Lipid: نفس المعدل يُطبَّق على HGB و WBC و RBC = خطأ سريري جسيم | يعتمد عليه: G2, G12, G16, G17 (سريرياً)، G7+G8 (للعرض الصحيح) | **القرار المعماري 1** |
| **G4** | لا IsPregnant في Range | ✅ **مؤكدة** | `Models/Entities.cs:231-243` لا يحوي `IsPregnant`. الحقل موجود على المريض فقط: `Models/Entities.cs:106` ⇒ `public bool IsPregnant { get; set; }` (داخل Patient) | معدلات بعض التحاليل (β-hCG, TSH, Hb) تختلف للحامل | تُدمَج مع Migration G3 (نفس الجدول) | — |
| **G5** | لا AgeUnit (شهور/سنوات) | ✅ **مؤكدة** | `Models/Entities.cs:236-237`:<br>`public int? AgeFrom { get; set; }`<br>`public int? AgeTo { get; set; }`<br>بدون وحدة قياس | حديثو الولادة (0–28 يوم) و الرضّع (1–12 شهر) لا يمكن تمييزهم | تُدمَج مع Migration G3 | — |
| **G6** | Flag غير موحد (Enum) | ✅ **مؤكدة** | `Models/Entities.cs:303` ⇒ `public string? Flag { get; set; }`<br>`Services/ResultsService.cs:295` ⇒ `result.Flag = "L";`<br>`Services/ResultsService.cs:300` ⇒ `result.Flag = "H";`<br>`Data/DevDataSeeder.cs:161` ⇒ `Flag = "Normal"`<br>`grep -n "ResultFlag\|enum.*Flag"` ⇒ صفر نتائج | تضارب قيم نصية ⇒ منطق الطباعة والتصفية والإحصاء يفشل | يجب تنفيذها قبل G12 | **القرار المعماري 3** |
| **G7** | الطباعة بلا Unit ولا Range | ✅ **مؤكدة** | `Services/PrintService.cs:80`:<br>`var resultText = $"- {result.Parameter.Name}: {result.Value ?? "-"} {result.Flag}";` | تقرير غير مطابق لمعايير ISO 15189 / CLSI | يعتمد على G3 + G8 | — |
| **G8** | VisitTestReportItem بلا Range/Unit | ✅ **مؤكدة** | `Models/ReportModels.cs:14-19`:<br>`public class VisitTestReportItem { public VisitTest VisitTest; public Test Test; public List<ResultValueReportItem> Results; }`<br>**لا يوجد LowValue/HighValue/Unit/NormalText**.<br>`Services/ReportService.cs:50-87`: لا يجلب TestReferenceRanges ولا Parameter.Unit | بنية البيانات لا تدعم تقرير LIS احترافي | يعتمد على G3 | — |
| **G9** | لا Transaction في TestCatalogService | ✅ **مؤكدة** | `grep "BeginTransaction" Services/TestCatalogService.cs` ⇒ صفر نتائج.<br>كل `CreateXxxAsync` يستدعي `SaveChangesAsync()` منفرداً (مثال `TestCatalogService.cs:51-53`) | استيراد ضخم: فشل في المنتصف يترك بيانات نصفية | مستقل، يخدم G1 | — |
| **G10** | لا يوجد Upsert API | ✅ **مؤكدة** | `grep "Upsert" Services/TestCatalogService.cs` ⇒ صفر نتائج. توجد `CreateTestAsync` و `UpdateTestAsync` منفصلتان فقط (`Services/TestCatalogService.cs:35,55`) | تعذُّر إعادة الاستيراد بدون حذف يدوي مسبق | مستقل، يخدم G1 | — |
| **G11** | لا حزمة قراءة Excel/CSV/JSON | ✅ **مؤكدة** | `Open_lab.csproj:20-30`: الحزم الموجودة هي EFCore + WebView2 + PdfSharpCore + ZXing.Net + ConfigManager فقط. **لا** ClosedXML/EPPlus/CsvHelper/Newtonsoft | الـ Importer لا يستطيع قراءة ملف Seed | مستقل، يخدم G1 | — |
| **G12** | ValidateResultAsync بلا ParameterId | ✅ **مؤكدة** | `Services/IResultsService.cs:22`:<br>`Task<ReferenceRangeResult> ValidateResultAsync(int testId, string value, string gender, int age);`<br>`Services/ResultsService.cs:280-285`:<br>`.Where(r => r.TestId == testId)` فقط.<br>المُستدعي `ViewModels/ResultsEntryViewModel.cs:363-368` يمرر `TestId` فقط | كل بارامتر داخل التحليل يتلقى نفس Flag — أخطر فجوة سريرية مع G3 | يعتمد على G3 + G6 | **القرار المعماري 1** |
| **G13** | لا IsActive على Test/Range | ✅ **مؤكدة** | `grep -n "IsActive" Models/Entities.cs` ⇒ السطر 13 (User) و 106 (Patient via Migration) فقط، **لا شيء** في `Test` (173–204) ولا `TestReferenceRange` (231–243).<br>`Services/TestCatalogService.cs:108-111`:<br>`if (isUsed) { throw new InvalidOperationException("Cannot delete test that is already used."); }`<br>`Services/TestCatalogService.cs:113`:<br>`_db.Tests.Remove(test);` (Hard Delete) | لا توجد طريقة لإخفاء تحليل قديم محفوظ في زيارات سابقة | تُدمَج مع Migration G3 | **القرار المعماري 2** |
| **G14** | لا شاشات Groups/Units/SampleTypes | ✅ مؤكدة | `ls ViewModels/ | grep -iE "TestGroup|Units|SampleType"` ⇒ صفر نتائج (فقط `CustomGroupsViewModel` غير مرتبط) | المستخدم لا يضيف Groups/Units من الواجهة | لا يدخل في خطة العمل | ⛔ **مستبعدة وفق القرار المعماري 4** (موديول "بيانات النظام" المؤجل) |
| **G15** | لا فهرس فريد على TestGroup.GroupName | ✅ **مؤكدة** | `Data/OpenLabDbContext.cs:204-208`:<br>`entity.HasKey(e => e.GroupId);`<br>`entity.Property(e => e.GroupName).IsRequired();`<br>**لا `HasIndex().IsUnique()`**. الفحص بـ `Service` فقط في `TestCatalogService.cs:125`. | تكرار TestGroup ممكن من خارج الخدمة (Seeder/Migration/SQL مباشر) | تُدمَج مع Migration G3 (تغيير صغير) | — |
| **G16** | ResultValue بلا AppliedRangeId | ✅ **مؤكدة** | `Models/Entities.cs:298-312` (ResultValue):<br>الحقول: `ResultValueId, VisitTestId, ParameterId, Value, Flag?, Comment?, VerifiedBy?, VerifiedAt?` — **لا AppliedRangeId** | فقدان إمكانية تتبُّع أي Range طُبِّق على نتيجة قديمة | يعتمد على G3 | — |
| **G17** | ResultEntryItem بلا Unit/Min/Max | ✅ **مؤكدة** | `ViewModels/UiModels.cs:93-126`:<br>`public int ParameterId; public string ParameterName; public string? Value; public string? Flag; public string? Comment;`<br>**لا Unit/LowValue/HighValue**. | الفني يدخل النتيجة "أعمى" بدون مرجع بصري | يعتمد على G3 + G12 | — |

> **ملاحظة منهجية:** لم أعتمد على أي استنتاج للتقرير دون فحص الكود الحالي. التقرير الأصلي مبني على Commit `dea0ed8b` وقد فحصتُ الكوميت الحالي `227815f` وجدتُ أن **جميع الفجوات لا تزال قائمة كما هي** ولم تُصلح أي منها على هذا الفرع.

---

## 4. Confirmed Gaps List

القائمة النهائية للفجوات المؤكَّدة التي **تدخل** في خطة العمل:

```
G1  — Production Seeder للكتالوج
G2  — تحديث DevDataSeeder ليولّد TestReferenceRange
G3  — ربط TestReferenceRange بـ TestParameter (ParameterId)        [القرار 1]
G4  — إضافة IsPregnant إلى TestReferenceRange
G5  — إضافة AgeUnit إلى TestReferenceRange
G6  — توحيد ResultFlag كـ Enum                                       [القرار 3]
G7  — عرض Unit و Range في التقرير المطبوع
G8  — توسعة VisitTestReportItem ليحوي Unit و Range
G9  — Transaction Support في TestCatalogService
G10 — Upsert API (UpsertTestAsync, UpsertReferenceRangeAsync ...)
G11 — إضافة حزمة قراءة Excel/CSV/JSON
G12 — تحديث ValidateResultAsync ليعتمد ParameterId                   [القرار 1]
G13 — Soft Delete (IsActive) لـ Test و TestReferenceRange            [القرار 2]
G15 — Unique Index على TestGroup.GroupName
G16 — حفظ AppliedRangeId داخل ResultValue
G17 — توسعة ResultEntryItem (Unit / LowValue / HighValue / NormalText)
```

⛔ **المستبعدة صراحةً من هذه الخطة:**
- **G14** — شاشات إدارة `TestGroup` / `Unit` / `SampleType` المستقلة. وفقاً للقرار المعماري 4، هذه الشاشات مؤجلة بالكامل لموديول مستقبلي **"بيانات النظام" (System Data Module)**. الإدارة المؤقتة لهذه الكيانات تتم عبر:
  - الاستيراد من ملف Seed (Sheet `Departments`, `Units`, `SampleTypes` ضمن مهمة G1)، أو
  - توسعة بسيطة لواجهة التحاليل الحالية عند الحاجة (دون شاشة مستقلة).

---

## 5. Dependency Analysis

### 5.1 الاعتماديات المُلزمة (Critical Path)

```
                  ┌───────────────────────────┐
                  │  Step 1: Models (Domain)  │
                  │  G3 + G4 + G5 + G6 + G13  │
                  │  + AppliedRangeId (G16)   │
                  └─────────────┬─────────────┘
                                │ (موديل صار جاهز)
                                ▼
                  ┌───────────────────────────┐
                  │ Step 2: DbContext Config  │
                  │ علاقة Range↔Parameter +   │
                  │ HasIndex GroupName (G15) +│
                  │ GlobalQueryFilter IsActive│
                  └─────────────┬─────────────┘
                                │
                                ▼
                  ┌───────────────────────────┐
                  │ Step 3: EF Migration واحد │
                  │ (Schema + Data Migration) │
                  └─────────────┬─────────────┘
                                │ (DB صارت جاهزة)
              ┌─────────────────┼─────────────────┐
              ▼                 ▼                 ▼
       ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
       │ Step 4:      │  │ Step 5:      │  │ Step 6:      │
       │ ResultsSvc   │  │ TestCatalog  │  │ Report       │
       │ G12+G6+G16   │  │ G13+G10+G9   │  │ G7+G8        │
       └──────┬───────┘  └──────┬───────┘  └──────┬───────┘
              │                 │                 │
              ▼                 │                 │
       ┌──────────────┐         │                 │
       │ Step 7:      │         │                 │
       │ Result Entry │         │                 │
       │ UI  (G17)    │         │                 │
       └──────┬───────┘         │                 │
              │                 │                 │
              ▼                 │                 │
       ┌──────────────┐         │                 │
       │ Step 8:      │         │                 │
       │ Ranges UI    │         │                 │
       │ (Parameter   │         │                 │
       │  selector +  │         │                 │
       │  IsActive +  │         │                 │
       │  IsPregnant) │         │                 │
       └──────────────┘         │                 │
                                ▼                 │
                         ┌──────────────┐         │
                         │ Step 9:      │         │
                         │ DevSeeder G2 │         │
                         └──────────────┘         │
                                                  │
                  ┌───────────────────────────────┘
                  ▼
            ┌──────────────────────────┐
            │ Step 10 (متوازٍ من البداية):│
            │  G11 — NuGet packages    │
            └──────────────┬───────────┘
                           │
                           ▼
            ┌──────────────────────────┐
            │ Step 11: MedicalCatalog  │
            │ Importer + Embedded Seed │
            │ File (G1) — الخطوة الأخيرة│
            └──────────────────────────┘
```

### 5.2 شرح الاعتماديات (سبب تقني لكل علاقة)

1. **G3 يجب أن يسبق G12** — استحالة استخدام `ParameterId` في `ValidateResultAsync` قبل وجود الحقل في كيان `TestReferenceRange`.
2. **G3 يجب أن يسبق G2** — الـ Seeder لا يستطيع إنشاء `TestReferenceRange` بدون قيمة لـ `ParameterId` بعد إضافته.
3. **G3 يجب أن يسبق G7 + G8** — `VisitTestReportItem` الجديد سيحمل بيانات `Range` على مستوى الـ Parameter، لذا تصميم الـ DTO نفسه يعتمد على شكل العلاقة الجديدة.
4. **G3 يجب أن يسبق G16** — `AppliedRangeId` يشير إلى Range محدَّد لـ Parameter محدَّد؛ بدون G3 يعود إلى الخطأ السابق (Range على مستوى Test).
5. **G3 يجب أن يسبق G17** — `ResultEntryItem` الجديد سيعرض `LowValue/HighValue/Unit/NormalText` التي تأتي من Range المرتبط بالـ Parameter.
6. **G6 يجب أن يسبق G12** — `ValidateResultAsync` المُحدَّث يجب أن يُعيد قيم `ResultFlag` enum بدلاً من `"L"/"H"`. لذا الـ enum يجب أن يكون مُعرَّفاً قبل تعديل الخدمة.
7. **G6 يسبق G7** — منطق الطباعة سيحوِّل `ResultFlag` إلى نص محلي ("منخفض/مرتفع"). لا يمكن تحقيق ذلك دون الـ enum.
8. **G6 يسبق G17** — UI يجب أن يربط لون الخلية بقيم `ResultFlag`، لذا الـ enum مطلوب أولاً.
9. **G13 (Soft Delete) يجب أن يكون في نفس Migration G3** — لأنه يضيف عموداً (`IsActive`) إلى نفس الجدول `TestReferenceRange` (وإلى جدول `Tests`). إنشاء migration واحد يقلل خطر تشتيت Schema.
10. **G13 يسبق G5/G11 (في Seeder/Service)** — أي query تجلب Tests/Ranges بعد G13 يجب أن تُصفِّي `IsActive == true` (Global Query Filter)، بما فيها استعلامات `MedicalCatalogImporter` و `ResultsService`.
11. **G15 (Unique Index) يُضاف داخل نفس Migration الكبير** — تغيير صغير لا يستحق Migration مستقل، لكنه يجب أن يأتي بعد فحص البيانات الموجودة (التأكد من عدم وجود تكرار في `TestGroup.GroupName`).
12. **G8 يسبق G7** — `PrintService` لا يمكنه طباعة Unit/Range قبل أن تُضاف هذه الحقول إلى `VisitTestReportItem` ويُملأها `ReportService`.
13. **G11 + G9 + G10 يسبقون G1** — `MedicalCatalogImporter` يحتاج: حزمة قراءة الملف (G11)، Transaction واحد للاستيراد كاملاً (G9)، و Upsert APIs (G10) لإعادة الاستيراد بأمان.
14. **G2 يسبق G1 منطقياً** — لكنه ليس اعتمادية تقنية صارمة. مفيد أن نختبر تدفق الـ Validation الكامل على DevSeeder قبل الانتقال للـ Importer الإنتاجي.

### 5.3 الفجوات المتوازية (يمكن تنفيذها معاً)

- **G11 (NuGet packages)** مستقلة تماماً ⇒ يمكن إضافتها في أي وقت قبل الخطوة 11.
- **G9 + G10 (Transaction + Upsert)** مستقلتان عن سلسلة Models/Migration، يمكن تنفيذهما بالتوازي مع الخطوات 4–7.
- **G15 (Unique Index)** صغيرة جداً، تُدمج في Migration G3 مباشرةً بدون عمل إضافي.

### 5.4 الـ Migrations المطلوبة (موقعها في السلسلة)

- **Migration واحد فقط** بعد الخطوة 2 يجمع كل تغييرات Schema:
  - **الجداول المتأثرة:** `TestReferenceRanges`, `Tests`, `ResultValues`, `TestGroups`.
  - **الأعمدة الجديدة:**
    - `TestReferenceRanges.ParameterId` (int NULL, FK ⇒ `TestParameters.ParameterId`) — G3.
    - `TestReferenceRanges.IsPregnant` (bit NULL) — G4.
    - `TestReferenceRanges.AgeUnit` (nvarchar(10) NULL، قيم: "Day"/"Month"/"Year") — G5.
    - `TestReferenceRanges.IsActive` (bit NOT NULL DEFAULT 1) — G13.
    - `Tests.IsActive` (bit NOT NULL DEFAULT 1) — G13.
    - `ResultValues.AppliedRangeId` (int NULL, FK ⇒ `TestReferenceRanges.RangeId` مع `OnDelete: NoAction` لمنع cascade تخريبية) — G16.
  - **الفهارس الجديدة:**
    - `IX_TestReferenceRanges_ParameterId`.
    - `IX_TestGroups_GroupName UNIQUE` — G15.
    - `IX_Tests_IsActive`, `IX_TestReferenceRanges_IsActive` (فهارس فلترة).
  - **Data Migration مطلوب — نعم:** خطوتان داخل نفس الـ migration:
    1. ملء `Tests.IsActive = 1` و `TestReferenceRanges.IsActive = 1` (تم تلقائياً عبر DEFAULT).
    2. **ترحيل Ranges الموجودة:** لكل سجل في `TestReferenceRanges` حالياً (TestId مرتبط بـ Test متعدد البارامترات)، نملأ `ParameterId` بأول Parameter (أقل OrderNo) من نفس التحليل. هذه قاعدة تقريبية محافِظة (لمنع كسر السلوك الحالي). إذا كان التحليل أحادي البارامتر، الربط تلقائي ومثالي. هذا الترحيل يُكتب كـ `migrationBuilder.Sql(...)` داخل `Up()`.
  - **التطبيق الآمن:**
    - أخذ Backup كامل (`.bak`) للقاعدة الإنتاجية الحالية قبل تنفيذ `dotnet ef database update`.
    - تنفيذ على نسخة QA أولاً والتحقق من نتائج عينة (10 سجلات Range متنوعة).
    - تنفيذ على الإنتاج بعد ساعات العمل بإطار صيانة مُعلَن.

---

## 6. Execution Roadmap

> الترتيب أدناه يتبع سلسلة الاعتماديات الفعلية المُثبَتة في القسم 5، **وليس تجميعاً وصفياً**.

---

### الخطوة 1: تعديل النماذج (Domain Models)

- **الفجوات المعالجة:** G3, G4, G5, G6 (تعريف enum)، G13 (إضافة الحقل في الموديل)، G16 (إضافة الحقل في الموديل).
- **سبب هذا الترتيب:** كل خطوة لاحقة (DbContext، Migration، Services، UI) تعتمد مباشرةً على شكل النماذج. لا يمكن كتابة Migration دون تعديل الـ Entities أولاً. ولا يمكن استبدال `string Flag` بـ `ResultFlag enum` في `ValidateResultAsync` قبل تعريف الـ enum.
- **الملفات المتأثرة:**
  - Models: `Models/Entities.cs` — تعديل `TestReferenceRange` (إضافة `ParameterId int?`, `IsPregnant bool?`, `AgeUnit string?`, `IsActive bool`)، تعديل `Test` (إضافة `IsActive bool`)، تعديل `ResultValue` (إضافة `AppliedRangeId int?`).
  - Models (ملف جديد): `Models/ResultFlag.cs` — تعريف الـ enum:
    ```
    public enum ResultFlag { Normal, Low, High, Critical, NotApplicable }
    ```
  - DbContext: لا تعديل في هذه الخطوة (الخطوة 2).
  - Services / ViewModels / XAML: لا تعديل بعد.
- **هل تتطلب Migration؟** ❌ ليس بعد. الـ Migration يتولَّد في الخطوة 3 بعد اكتمال الـ DbContext config.
- **الخطوات الفرعية:**
  أ. إضافة `public int? ParameterId { get; set; }` و navigation `public TestParameter? Parameter { get; set; }` إلى `TestReferenceRange` مع الإبقاء على `TestId` كـ shortcut (للحالات الأحادية كـ Glucose) — متوافق مع نص القرار 1: "إضافة حقل ParameterId بدلاً من الاعتماد على TestId فقط".
  ب. إضافة `public bool? IsPregnant { get; set; }` (Tri-state: null = لا فرق، true = للحامل، false = لغير الحامل).
  ج. إضافة `public string? AgeUnit { get; set; }` (يُقيَّد لاحقاً بالـ enum أو constraint).
  د. إضافة `public bool IsActive { get; set; } = true;` إلى `Test` و `TestReferenceRange`.
  هـ. إضافة `public int? AppliedRangeId { get; set; }` و navigation property إلى `ResultValue`.
  و. إنشاء ملف `Models/ResultFlag.cs` بـ enum `ResultFlag`.
- **معايير القبول:**
  - يتم Build بنجاح (`dotnet build`) دون كسر أي مرجع.
  - استعراض `Models/Entities.cs` يُظهر جميع الحقول الجديدة.
  - `grep "ResultFlag" Models/` يُرجع نتائج.
  - لا تعديل على الـ services بعد ⇒ السلوك الحالي للنظام لم يتغير.
- **تحذيرات:**
  - **لا تُغيِّر** نوع `Flag` في `ResultValue` من `string?` إلى `ResultFlag` مباشرةً — هذا يكسر القراءة من البيانات القديمة. الـ enum يُستخدم فقط داخل الـ services + UI، و persistence يبقى string عبر `Value Conversion` تُعرَّف في DbContext (الخطوة 2). هذا الأسلوب يحافظ على بيانات Flag الموجودة دون Data Migration معقد.

---

### الخطوة 2: تعديل DbContext + علاقات EF

- **الفجوات المعالجة:** G3 (علاقة Range↔Parameter)، G15 (Unique Index)، G13 (Global Query Filter)، G6 (Value Conversion للـ enum)، G16 (علاقة AppliedRange).
- **سبب هذا الترتيب:** يجب اكتمال الموديل أولاً (الخطوة 1)، وقبل توليد Migration. كذلك يجب تعريف Global Query Filter قبل أن تستخدمه الخدمات، وإلا ستحتاج كل خدمة إلى تصفية يدوية.
- **الملفات المتأثرة:**
  - Models: لا تعديل.
  - DbContext: `Data/OpenLabDbContext.cs` — في `OnModelCreating`:
    - إضافة العلاقة الجديدة `TestReferenceRange.Parameter ↔ TestParameter`.
    - تغيير سلوك Cascade على `TestReferenceRange.Test` إلى `Restrict` بعد إضافة ParameterId (لمنع cascade مزدوج).
    - `HasIndex(e => e.GroupName).IsUnique()` على `TestGroup`.
    - `HasIndex(e => e.IsActive)` على `Test` و `TestReferenceRange`.
    - `HasQueryFilter(t => t.IsActive)` على `Test` و `TestReferenceRange`.
    - `HasConversion<string>()` للـ enum إذا أُضيف لاحقاً، **لكن في هذه الخطوة Flag يبقى string** — التحويل يتم في الـ services.
    - علاقة `ResultValue.AppliedRange ↔ TestReferenceRange` بـ `OnDelete: NoAction` (لمنع كسر النتائج التاريخية إذا حُذفت Range).
  - Services: لا تعديل.
  - ViewModels / XAML: لا تعديل.
- **هل تتطلب Migration؟** ❌ ليس بعد — الـ Migration يُولَّد بعد هذه الخطوة في الخطوة 3.
- **الخطوات الفرعية:**
  أ. إضافة قسم تكوين `TestReferenceRange` الجديد للـ `Parameter` (مع `IsRequired(false)`).
  ب. إضافة Global Query Filter لـ `Test` و `TestReferenceRange`.
  ج. إضافة Unique Index على `TestGroups.GroupName`.
  د. تعريف علاقة `ResultValue.AppliedRange`.
  هـ. مراجعة `OnDelete` على Range ⇒ ChangeBehavior إلى `Restrict` لمنع cascade خاطئ.
- **معايير القبول:**
  - `dotnet ef migrations add Phase12_CatalogReadiness --dry-run` (إذا متوفر) لا يُبلِغ عن خطأ Model.
  - `dotnet build` ينجح.
- **تحذيرات:**
  - **Global Query Filter** قد يؤثر على شاشات تستخدم EF Include على Tests. يجب فحص كل ViewModel يستدعي `_db.Tests.Where(...)` للتأكد من أن السلوك الجديد (إخفاء التحاليل غير النشطة) مقصود في كل المواضع. **استثناء واحد مهم:** عند عرض الزيارات التاريخية، يجب استخدام `IgnoreQueryFilters()` لإظهار التحاليل المُعطَّلة لاحقاً.
  - بعد إضافة العلاقتين الجديدتين، قد يلزم تعطيل أحد Cascade لتجنب خطأ "multiple cascade paths" من SQL Server.

---

### الخطوة 3: توليد وتطبيق Migration واحد كبير

- **الفجوات المعالجة:** G3 + G4 + G5 + G13 + G15 + G16 (كل ما يحتاج DDL).
- **سبب هذا الترتيب:** يأتي مباشرةً بعد اكتمال Model + DbContext لأن Migration يقرأ منهما. ولا يمكن تنفيذ الخطوات 4 فما بعد (التي تستهلك ParameterId/IsActive في Linq queries) قبل تطبيق الـ Migration على القاعدة.
- **الملفات المتأثرة:**
  - Migrations: ملف جديد `Migrations/202605xx_Phase12_CatalogReadiness.cs` + `.Designer.cs` + تحديث `OpenLabDbContextModelSnapshot.cs`.
  - DbContext: لا تعديل (يُحدِّث الـ snapshot تلقائياً عبر CLI).
  - Services / ViewModels / XAML: لا تعديل بعد.
- **هل تتطلب Migration؟** ✅ **نعم — Migration رئيسي**.
  - **الجداول المتأثرة:**
    - `Tests`: عمود جديد `IsActive bit NOT NULL DEFAULT 1`.
    - `TestReferenceRanges`: 4 أعمدة جديدة (`ParameterId int NULL`, `IsPregnant bit NULL`, `AgeUnit nvarchar(10) NULL`, `IsActive bit NOT NULL DEFAULT 1`) + FK نحو `TestParameters` + index على `ParameterId` + index على `IsActive`.
    - `ResultValues`: عمود جديد `AppliedRangeId int NULL` + FK نحو `TestReferenceRanges` بـ `OnDelete: NoAction`.
    - `TestGroups`: `UNIQUE INDEX (GroupName)`.
  - **التطبيق الآمن:**
    - Backup كامل: `BACKUP DATABASE OpenLab TO DISK = 'OpenLab_Pre_Phase12.bak' WITH COMPRESSION;`.
    - تنفيذ على DB QA أولاً، التحقق بفحص `SELECT TOP 10 ParameterId FROM TestReferenceRanges` بعد الترحيل.
    - تنفيذ على Production في إطار صيانة (10–15 دقيقة).
  - **Data Migration المطلوب:** نعم. داخل نفس الـ migration `Up()`:
    ```
    migrationBuilder.Sql(@"
      UPDATE r
      SET r.ParameterId = (
        SELECT TOP 1 p.ParameterId
        FROM TestParameters p
        WHERE p.TestId = r.TestId
        ORDER BY p.OrderNo, p.ParameterId
      )
      FROM TestReferenceRanges r
      WHERE r.ParameterId IS NULL
        AND EXISTS (SELECT 1 FROM TestParameters p WHERE p.TestId = r.TestId);
    ");
    ```
    هذا يربط كل Range قديمة بأول Parameter للتحليل (سلوك محافظ يحاكي السلوك السابق دون كسر سريري فوري).
    قبل تنفيذ الـ migration، يجب فحص يدوي للقاعدة الحالية:
    ```
    SELECT GroupName, COUNT(*) FROM TestGroups GROUP BY GroupName HAVING COUNT(*) > 1;
    ```
    إذا وُجد تكرار، يجب توحيده يدوياً قبل تطبيق Unique Index (G15) وإلا الـ migration سيفشل.
- **الخطوات الفرعية:**
  أ. تشغيل `dotnet ef migrations add Phase12_CatalogReadiness -p Open_lab -s Open_lab`.
  ب. مراجعة ملف الـ migration المُولَّد يدوياً + إضافة بلوك `migrationBuilder.Sql(...)` لترحيل البيانات.
  ج. إضافة كود `Down()` يعكس كل العمليات (لإمكانية الـ rollback).
  د. تطبيق `dotnet ef database update` على DB التطوير (الأساسية الحالية المُستخدمة).
  هـ. التحقق بعد التطبيق:
    - `SELECT COUNT(*) FROM TestReferenceRanges WHERE ParameterId IS NULL;` يجب أن يكون 0 لكل Range مرتبطة بـ Test يحوي parameters.
    - `SELECT IsActive FROM Tests` كله = 1.
- **معايير القبول:**
  - Migration يطبَّق بنجاح بدون أخطاء.
  - الأعمدة الجديدة موجودة في DB (تحقُّق عبر `INFORMATION_SCHEMA.COLUMNS`).
  - البيانات الموجودة سليمة (لا فقدان Tests/Ranges/Results).
  - تشغيل النظام يعمل بدون أخطاء runtime (لأن الخدمات لم تتغير بعد ⇒ Backward compatible).
- **تحذيرات:**
  - **ممنوع** تطبيق هذا الـ migration على Production قبل اختباره على نسخة Restore من Backup الإنتاجي الحالي.
  - إذا فشل بلوك الـ Data Migration (مثلاً TestReferenceRange يشير إلى TestId لا يوجد له Parameters)، يجب اتخاذ قرار: حذف هذه السجلات اليتيمة أم تركها بـ `ParameterId = NULL`. التوصية: تركها NULL لأنها قابلة للإصلاح يدوياً لاحقاً عبر شاشة الـ Ranges.
  - السياق الحالي: **القاعدة الإنتاجية الحقيقية** قيد الاستخدام. لذا أي خلل في الـ Data Migration يجب أن يقف فوراً على QA دون لمس Production.

---

### الخطوة 4: تحديث ResultsService (validation core)

- **الفجوات المعالجة:** G12 (ParameterId)، G6 (استخدام ResultFlag enum بدلاً من "L"/"H"/literal)، G16 (كتابة AppliedRangeId).
- **سبب هذا الترتيب:** تعتمد على وجود `ParameterId` في الـ entity (الخطوة 1) و في الـ DB (الخطوة 3). كذلك تعتمد على وجود الـ enum (الخطوة 1). تأتي قبل تعديل UI لأن الـ UI سيُغذِّى بمخرجات هذه الخدمة.
- **الملفات المتأثرة:**
  - Models: `Models/ReferenceRangeResult.cs` — تعديل `Flag` ليكون من نوع `ResultFlag?` بدلاً من `string?` (أو إضافة خاصية `FlagEnum` بجانب الـ string الحالية لتفادي كسر مستهلكين آخرين).
  - DbContext: لا تعديل.
  - Services: `Services/IResultsService.cs` (تغيير signature: `ValidateResultAsync(int testId, int parameterId, string value, string gender, int age, bool isPregnant)`)، `Services/ResultsService.cs` (تحديث الاستعلام ليصفِّي `r.ParameterId == parameterId || (r.ParameterId == null && r.TestId == testId)` للسماح بالـ fallback، + استخدام `ResultFlag.Low/High/Normal` بدلاً من النصوص، + ملء `AppliedRangeId` على `ResultValue` عند الحفظ في `SaveResultCoreAsync`).
  - ViewModels: تعديل `ResultsEntryViewModel.cs:363-368` لتمرير `item.ParameterId` و `SelectedVisitTest.PatientIsPregnant`.
  - XAML: لا تعديل.
- **هل تتطلب Migration؟** ❌ لا (Schema لم يتغير).
- **الخطوات الفرعية:**
  أ. تعديل توقيع `ValidateResultAsync` في `IResultsService` + كل تنفيذاته.
  ب. تعديل الـ Linq query ليفلتر بـ `ParameterId == parameterId` أولاً، ثم `OrderByDescending` يفضل Range الـ ParameterId المحدَّد على الـ NULL.
  ج. إضافة شرط `IsPregnant`: `r.IsPregnant == null || r.IsPregnant == isPregnant`.
  د. إضافة شرط `AgeUnit` (تحويل العمر إلى وحدة الـ Range قبل المقارنة).
  هـ. استبدال `result.Flag = "L"` بـ `result.Flag = ResultFlag.Low`.
  و. في `SaveResultCoreAsync` (السطر 71)، حفظ `AppliedRangeId = range.RangeId` على `ResultValue` الجديد.
  ز. تحديث `ResultsEntryViewModel.AutoValidateResultAsync` ليمرر `parameterId` و `isPregnant`.
- **معايير القبول:**
  - Unit test (يدوي أو آلي): إنشاء CBC بـ 4 parameters، 4 ranges مختلفة لكل parameter، إدخال 4 قيم ⇒ يجب الحصول على 4 flags صحيحة، كل واحدة من range مختلفة.
  - `SELECT AppliedRangeId FROM ResultValues WHERE ResultValueId = ?` يعيد قيمة غير NULL بعد الـ Save.
  - `grep '"L"\|"H"' Services/ResultsService.cs` ⇒ صفر نتائج.
- **تحذيرات:**
  - الـ fallback للـ `ParameterId == null` يبقى لـ backward compatibility مع Ranges القديمة التي لم تُربَط بـ Parameter بعد. هذا مقصود وجوهري لمنع كسر التحاليل الأحادية كـ Glucose.
  - يجب أن يكون `ResultsEntryViewModel` لديه access إلى `Patient.IsPregnant`. حالياً `VisitTestRow` لا يحوي هذا الحقل — يجب إضافته كحقل عرض من Patient.

---

### الخطوة 5: تحديث TestCatalogService (Soft Delete + Upsert + Transaction)

- **الفجوات المعالجة:** G13 (تحويل Delete إلى Soft)، G10 (Upsert APIs)، G9 (Transaction Support).
- **سبب هذا الترتيب:** يعتمد على وجود `IsActive` في الـ entity + DB (خطوات 1+3). لا يعتمد على الخطوة 4 ⇒ **يمكن تنفيذها بالتوازي مع الخطوة 4**.
- **الملفات المتأثرة:**
  - Models: لا تعديل.
  - DbContext: تم بالفعل (Global Query Filter في الخطوة 2).
  - Services: `Services/ITestCatalogService.cs` + `Services/TestCatalogService.cs`:
    - تغيير `DeleteTestAsync` إلى Soft (تعيين `IsActive = false`).
    - تغيير `DeleteReferenceRangeAsync` إلى Soft.
    - إضافة `ReactivateTestAsync(int testId)` و `ReactivateReferenceRangeAsync(int rangeId)`.
    - إضافة `GetAllTestsAsync(bool includeInactive = false)` (مع `IgnoreQueryFilters()` عند includeInactive=true).
    - إضافة `UpsertTestAsync(Test)` يستخدم `Code` كمفتاح طبيعي.
    - إضافة `UpsertReferenceRangeAsync(TestReferenceRange)` بمفتاح طبيعي: `(TestId, ParameterId, Gender, IsPregnant, AgeFrom, AgeTo, AgeUnit)`.
    - إضافة `UpsertManyAsync(...)` يفتح `BeginTransactionAsync` ويُغلِّف كل العمليات.
  - ViewModels: لا تعديل بعد (الخطوة 8 ستتولى تحديث الـ UI).
  - XAML: لا تعديل بعد.
- **هل تتطلب Migration؟** ❌ لا.
- **الخطوات الفرعية:**
  أ. تعديل `DeleteTestAsync` ⇒ `test.IsActive = false; await SaveChangesAsync();` بدلاً من `Remove`. + إزالة شرط `isUsed` المُلقي للـ exception.
  ب. تعديل `DeleteReferenceRangeAsync` بنفس الأسلوب.
  ج. تعديل `GetAllTestsAsync` ليأخذ معامل اختياري `bool includeInactive`.
  د. كتابة `UpsertTestAsync` و `UpsertReferenceRangeAsync`.
  هـ. إضافة دالة عامة تستخدم `_db.Database.BeginTransactionAsync()` للاستيراد الجماعي.
- **معايير القبول:**
  - حذف تحليل مُستخدم في زيارات سابقة لا يرمي exception، بل يُخفيه فقط.
  - `SELECT * FROM Tests WHERE IsActive = 0` يُظهر السجلات المحذوفة.
  - شاشة اختيار التحاليل في زيارة جديدة لا تعرض المحذوفين (Global Query Filter يعمل).
  - شاشات الـ History تعرضهم بشكل صحيح (بعد استخدام `IgnoreQueryFilters` حيث يلزم).
- **تحذيرات:**
  - يجب فحص **كل** Linq query في كل خدمة (`grep "_db.Tests"` و `"_db.TestReferenceRanges"`) للتأكد من سلوك Global Query Filter. مواضع التقارير التاريخية والإحصاءات يجب أن تستخدم `IgnoreQueryFilters()`.
  - Upsert على Range يجب أن يحترم Unique Logical Key المركَّب. يلزم اتفاق صريح على ما يعتبر "نفس Range" (نفس المفتاح المركَّب أعلاه).

---

### الخطوة 6: توسعة Print/Report Pipeline (G7 + G8)

- **الفجوات المعالجة:** G8 (إضافة Unit/Range إلى DTO)، G7 (طباعة فعلية).
- **سبب هذا الترتيب:** تعتمد على G3 (Range موجود على مستوى Parameter في DB، الخطوة 3). مستقلة عن خطوات 4 + 5 ⇒ **يمكن تنفيذها بالتوازي معهما**.
- **الملفات المتأثرة:**
  - Models: `Models/ReportModels.cs` — إضافة إلى `ResultValueReportItem` الحقول: `string? Unit`, `decimal? LowValue`, `decimal? HighValue`, `string? NormalText`, `ResultFlag Flag`, و إضافة `RangeDisplay` (نص جاهز للطباعة مثل "12.0 – 16.0").
  - DbContext: لا تعديل.
  - Services: `Services/ReportService.cs` — تعديل `BuildVisitReportAsync` ليُحمِّل لكل `ResultValue` الـ Parameter + Parameter.Unit + الـ Range المُطبَّق (عبر `AppliedRangeId` إن وُجد، وإلا يعيد الحساب من `ValidateResultAsync` بدون حفظ). `Services/PrintService.cs:80` — تعديل سطر `resultText` ليُضمِّن Unit و RangeDisplay. `Services/ReportPdfService.cs` — نفس التحديث للـ PDF.
  - ViewModels: لا تعديل.
  - XAML: لا تعديل (PrintService يستخدم FlowDocument برمجياً، لا XAML خارجي).
- **هل تتطلب Migration؟** ❌ لا.
- **الخطوات الفرعية:**
  أ. توسعة `ResultValueReportItem` (و `VisitTestReportItem` إن لزم) بالحقول الجديدة.
  ب. في `ReportService.BuildVisitReportAsync`، عند بناء كل `ResultValueReportItem`، جلب Parameter.Unit من `_db.TestParameters.Include(p => p.Unit)`.
  ج. جلب Range المطبَّق: إذا `rv.AppliedRangeId != null` نُحمِّله مباشرةً عبر `Include` أو استعلام ثانوي؛ وإلا fallback نستدعي `IResultsService.ValidateResultAsync` (في حالة النتائج القديمة قبل خطوة 4).
  د. تعديل `PrintService` ليطبع: `{ParameterName}\t{Value}\t{Unit}\t{RangeDisplay}\t{FlagLabel}` بتنسيق جدولي.
  هـ. نفس التعديل في `ReportPdfService` (PdfSharpCore Table).
- **معايير القبول:**
  - طباعة تقرير لزيارة CBC تُظهر لكل بارامتر: القيمة، الوحدة (g/dL, ×10³/μL, ...)، الـ Range، و Flag.
  - PDF يحوي نفس الأعمدة.
  - تقارير الزيارات القديمة (قبل تطبيق Phase12) لا تنكسر — تستخدم fallback لإعادة حساب Range وقت الطباعة.
- **تحذيرات:**
  - الـ `FlowDocument` في PrintService يستخدم `Paragraph` و `Run` نصياً. التنسيق الجدولي يستلزم استخدام `Table`, `TableRowGroup`, `TableCell` — هذا تغيير ملموس في طبقة التخطيط، يحتاج مراجعة بصرية على بيئة Windows.
  - تقارير التاريخ المرضي (`PrintPatientHistoryAsync`) قد تحتاج نفس التحديث للاتساق.

---

### الخطوة 7: تحديث شاشة Result Entry (G17)

- **الفجوات المعالجة:** G17 (عرض Unit + LowValue + HighValue + NormalText للفني).
- **سبب هذا الترتيب:** تعتمد على نتيجة `ValidateResultAsync` الجديدة (الخطوة 4) التي تعيد Range بناءً على ParameterId. كذلك تعتمد على `Parameter.Unit` (موجود في الموديل أصلاً).
- **الملفات المتأثرة:**
  - Models: لا تعديل.
  - DbContext: لا تعديل.
  - Services: لا تعديل.
  - ViewModels: `ViewModels/UiModels.cs` — توسعة `ResultEntryItem` بـ `string? Unit`, `decimal? LowValue`, `decimal? HighValue`, `string? NormalText`, `string RangeDisplay`, `ResultFlag FlagEnum`. `ViewModels/ResultsEntryViewModel.cs` — عند بناء `ResultEntryItem` (السطر 328 وما بعده)، جلب Parameter.Unit، و جلب أنسب Range حسب gender/age/isPregnant لعرضه قبل الإدخال.
  - XAML: شاشة `ResultsEntryView.xaml` (الـ DataGrid أو الـ ItemsControl المرتبط بـ `ResultItems`) — إضافة أعمدة جديدة: Unit, Range, Flag (بلون).
- **هل تتطلب Migration؟** ❌ لا.
- **الخطوات الفرعية:**
  أ. توسعة `ResultEntryItem`.
  ب. عند تحميل البارامترات في `ResultsEntryViewModel.LoadParametersAsync` (تقريباً السطر 320–340)، استدعاء `ResolveDisplayRangeAsync(parameterId, gender, age, isPregnant)` (دالة جديدة على `IResultsService` تُعيد Range فقط بدون مقارنة قيمة — تُستخدم للعرض المسبق).
  ج. إضافة `RangeDisplay` computed property: `LowValue.HasValue && HighValue.HasValue ? $"{LowValue} – {HighValue}" : NormalText ?? "-"`.
  د. تحديث XAML بإضافة أعمدة DataGridTextColumn جديدة.
  هـ. إضافة Style Trigger يلوِّن صف القيمة حسب `FlagEnum` (أحمر لـ High, أزرق لـ Low, أخضر لـ Normal).
- **معايير القبول:**
  - فتح شاشة إدخال نتائج CBC ⇒ كل بارامتر يعرض الوحدة و Range قبل الإدخال.
  - بعد إدخال قيمة شاذة ⇒ الصف يتلون فوراً ويظهر Flag صحيح.
- **تحذيرات:**
  - عدم استخدام Binding على Test.Range أو شيء مشابه — `ResultEntryItem` هو الـ DTO الصحيح وكل البيانات يجب أن تكون عليه.

---

### الخطوة 8: تحديث ReferenceRangesViewModel + شاشتها (Parameter selector + IsPregnant + AgeUnit + Soft Delete UI)

- **الفجوات المعالجة:** G3 + G4 + G5 + G13 (الجانب UI لكل الفجوات السابقة).
- **سبب هذا الترتيب:** تأتي بعد ثبات Domain + DB + Services ليكون التغيير UI فقط بدون فوضى.
- **الملفات المتأثرة:**
  - Models: لا.
  - DbContext: لا.
  - Services: قد يتطلب إضافة `GetParametersForTestAsync(int testId)` في `TestCatalogService` إن لم تكن موجودة.
  - ViewModels: `ViewModels/ReferenceRangesViewModel.cs` — إضافة `SelectedTest`, `SelectedParameter`, `Parameters` (ObservableCollection يتم تحميله عند تغيير SelectedTest)، إضافة `IsPregnant?` و `AgeUnit` بـ ComboBox، إضافة `ShowInactive` toggle مع زر "تفعيل/تعطيل" بدل "حذف".
  - XAML: `ReferenceRangesView.xaml` — إضافة ComboBox للـ Parameter (يفلتر بناءً على Test المختار)، CheckBox للـ IsPregnant (Tri-State)، ComboBox للـ AgeUnit (Day/Month/Year)، CheckBox "إظهار غير النشطة".
- **هل تتطلب Migration؟** ❌ لا.
- **الخطوات الفرعية:**
  أ. إضافة عقد للـ ComboBox الجديد + binding.
  ب. تحميل Parameters عند تغيير Test.
  ج. منع حفظ Range دون Parameter (Validation).
  د. تحويل زر Delete إلى Soft Delete (يستدعي Service الجديد).
  هـ. إضافة فلتر "إظهار غير النشطة" — عند تفعيله، استخدام `GetAllRangesAsync(includeInactive: true)`.
- **معايير القبول:**
  - فتح شاشة Ranges، اختيار CBC، اختيار HGB، إضافة Range (Gender=F, AgeFrom=20, AgeTo=50, Low=12, High=16) ⇒ يُحفظ بنجاح، يظهر في الجدول.
  - زر "حذف" يُغير `IsActive = false` ولا يحذف فعلياً.
- **تحذيرات:**
  - شاشة إدخال النتائج (الخطوة 7) يجب أن تختار Range الصحيح لكل بارامتر بناءً على المفتاح المركَّب — أي اختلاف في القواعد بين شاشة Ranges Editor و logic الاختيار في ResultsService = bug سريري.

---

### الخطوة 9: تحديث DevDataSeeder (G2)

- **الفجوات المعالجة:** G2 (إضافة TestReferenceRange سريرية صحيحة) + استخدام ResultFlag enum.
- **سبب هذا الترتيب:** يأتي بعد ثبات Schema + Services حتى يولِّد بيانات صحيحة للاختبار. مستقل عن الخطوة 11 (الـ Importer الإنتاجي).
- **الملفات المتأثرة:**
  - Models: لا.
  - DbContext: لا.
  - Services: لا.
  - ViewModels: لا.
  - Data: `Data/DevDataSeeder.cs` — بعد إنشاء كل Parameter (أسطر تقريباً 80–115)، إنشاء TestReferenceRange مرتبط بـ ParameterId الصحيح. تحديث ResultValues (السطور 161–168) لاستخدام `Flag = ResultFlag.Normal.ToString()` بدلاً من `"Normal"` (للاتساق).
  - XAML: لا.
- **هل تتطلب Migration؟** ❌ لا.
- **الخطوات الفرعية:**
  أ. إضافة TestReferenceRange لكل Parameter في CBC (HGB, WBC, RBC, PLT) + Glucose + ALT + AST + ALB، كل واحد بـ Gender + Age + Low + High طبية صحيحة.
  ب. توحيد قيم `Flag` لتطابق ResultFlag enum.
- **معايير القبول:**
  - تشغيل النظام في DEBUG ⇒ شاشة Result Entry تعرض Ranges صحيحة لكل بارامتر.
- **تحذيرات:**
  - يجب التأكد من أن DevDataSeeder يعمل فقط في DEBUG (لا يتسرب لـ Production).

---

### الخطوة 10: إضافة NuGet Packages (G11) — يمكن البدء بها بالتوازي

- **الفجوات المعالجة:** G11.
- **سبب هذا الترتيب:** مستقلة تماماً ⇒ يمكن إضافتها في أي لحظة قبل الخطوة 11. أُدرجت بشكل صريح لتجنُّب نسيانها.
- **الملفات المتأثرة:**
  - Project file: `Open_lab/Open_lab.csproj` — إضافة `<PackageReference Include="ClosedXML" Version="0.102.x" />` (الأنسب لقراءة Excel) **أو** `EPPlus` (لكن يحتاج license تجاري بعد v4)، **أو** `CsvHelper` إن قُرِّر استخدام CSV بدل Excel، **أو** `System.Text.Json` (موجود في .NET 8) إن قُرِّر JSON.
  - **التوصية:** ClosedXML (مفتوح، MIT) + System.Text.Json (مُضمَّن).
- **هل تتطلب Migration؟** ❌ لا.
- **الخطوات الفرعية:**
  أ. تحديد صيغة Seed File (الموصى به وفق التقرير Section "Recommended Data Schema": Excel متعدد Sheets).
  ب. إضافة Package.
  ج. `dotnet restore` + `dotnet build`.
- **معايير القبول:**
  - `dotnet build` ينجح.
  - `using ClosedXML.Excel;` يعمل في ملف اختباري بسيط.
- **تحذيرات:**
  - بعض Packages تحمل تبعيات System.IO.Packaging قد تتعارض مع PdfSharpCore. يجب التحقق بعد الإضافة.

---

### الخطوة 11: بناء MedicalCatalogImporter + ربطه في App.xaml.cs (G1)

- **الفجوات المعالجة:** G1 (الـ Production Seeder النهائي).
- **سبب هذا الترتيب:** آخر خطوة لأنها تعتمد على **كل** ما سبق: Domain (G3-G5)، DB (Migration)، ResultsService (G12)، TestCatalogService (G9 + G10)، NuGet packages (G11).
- **الملفات المتأثرة:**
  - Models: لا (يستهلك الموجود).
  - DbContext: لا.
  - Services: ملفان جديدان `Services/IMedicalCatalogImporter.cs` + `Services/MedicalCatalogImporter.cs`. التسجيل في DI تلقائي عبر convention في `App.xaml.cs:130-146`.
  - Resources: ملف Excel جديد `Resources/medical_catalog_seed.xlsx` يُضاف كـ `<EmbeddedResource>` في الـ csproj، بـ 6–7 sheets كما هي موصوفة في تقرير التدقيق.
  - App: `App.xaml.cs` — إضافة استدعاء `MedicalCatalogImporter.SeedOnFirstRunAsync(db)` بعد الـ Migrate وقبل أي UI (داخل bracket عام، **خارج** `#if DEBUG`)، بشرط `if (!await db.Tests.AnyAsync(t => t.IsActive))` لمنع تكرار الـ Seed.
  - ViewModels / XAML: لا.
- **هل تتطلب Migration؟** ❌ لا (لكن يستهلك Schema الذي أنشأته الخطوة 3).
- **الخطوات الفرعية:**
  أ. تصميم Excel template بـ 7 sheets (Departments, Units, SampleTypes, Tests, Parameters, ReferenceRanges, Comments) وفق المواصفات في القسم "Recommended Data Schema" من التقرير، مع تعديل واحد إلزامي: Sheet ReferenceRanges يجب أن يحوي عمود `ParameterName` (FK نحو Parameters عبر TestCode + ParameterName).
  ب. كتابة `MedicalCatalogImporter` يقرأ الـ sheets بترتيب FK-safe (Groups ⇒ Units ⇒ SampleTypes ⇒ Tests ⇒ Parameters ⇒ Ranges ⇒ Comments).
  ج. كل خطوة تستخدم `UpsertXxxAsync` من خدمة الكتالوج (موجود بعد الخطوة 5).
  د. كل العملية داخل `BeginTransactionAsync` واحد (موجود بعد الخطوة 5).
  هـ. تسجيل تفصيلي عبر `ILogger` (إضافة dependency إذا غير موجود — `Microsoft.Extensions.Logging`).
  و. ربط في `App.xaml.cs` خارج DEBUG، مع `if (!await db.Tests.AnyAsync())`.
- **معايير القبول:**
  - تشغيل النظام لأول مرة على DB فارغة ⇒ بعد الـ Migration يُملأ التلقائي ~30 تحليل و ~120 Range.
  - تشغيل ثاني ⇒ لا يحدث Seed مكرر (idempotent).
  - تعديل الـ Excel + استبدال الـ embedded resource + إعادة Build + تشغيل ⇒ Upsert يعمل (يضيف الجديد، يحدِّث الموجود حسب Code).
- **تحذيرات:**
  - يجب أن يكون الـ Importer **مقاوماً للفشل الجزئي**: في حال فشل أي صف، التراجع الكلي عن الترانزاكشن مع log واضح.
  - لا يُنشَّط في DEBUG لتجنب تعارض مع `DevDataSeeder` الحالي. الشرط في `App.xaml.cs` يكفي: `if (!await db.Tests.AnyAsync())`.

---

## 7. Risk Assessment

| الخطر المحتمل | الاحتمال | التأثير | خطة التخفيف |
|---|---|---|---|
| الـ Migration يفشل على Production بسبب تكرار TestGroup.GroupName موجود | متوسط | عالي (يوقف الترقية) | فحص يدوي مسبق `SELECT GroupName, COUNT(*) FROM TestGroups GROUP BY GroupName HAVING COUNT(*) > 1;` وتوحيد التكرار قبل تطبيق Migration |
| Global Query Filter يُخفي تحاليل تاريخية في تقارير الزيارات القديمة | عالي | عالي (المريض يفقد بياناته القديمة) | فحص شامل لكل query على Tests/Ranges في كل خدمة وإضافة `IgnoreQueryFilters()` في كل مواضع التاريخ |
| ربط Range موجودة بـ ParameterId خاطئ في الـ Data Migration | متوسط | عالي سريرياً (تفسير خاطئ لنتائج قديمة) | الـ Data Migration يربط بأول Parameter فقط كحل محافظ — جلسة مراجعة سريرية يدوية للمعدلات بعد التطبيق |
| كسر `OnDelete: Cascade` على Range بعد إضافة العلاقة الثانية يُسبب multiple cascade paths | عالي | متوسط (Build error من EF) | تغيير علاقة Range↔Test إلى `Restrict` + الإبقاء على Cascade من Test مرة واحدة فقط |
| FlowDocument الجديد بأعمدة Table يكسر تخطيط الورقة الحالي | متوسط | متوسط (شكل التقرير سيئ) | اختبار بصري على Windows فعلي قبل الإنتاج، ضبط Column Widths |
| ClosedXML يدخل تعارض مع PdfSharpCore (System.IO.Packaging) | منخفض | متوسط | اختبار `dotnet build` بعد الإضافة، استخدام binding redirect إن لزم |
| `ResultsEntryViewModel` لا يعرف `Patient.IsPregnant` لتمريرها لـ ValidateResultAsync | عالي | متوسط (Range الحوامل لن يُختار) | إضافة الحقل إلى `VisitTestRow` (UiModels.cs:30) كحقل عرض + ملؤه من `Patient` في `LoadVisitTestsAsync` |
| Soft Delete على Range يخفي Range الوحيد لتحليل ⇒ AutoValidate يعطي Null لكل النتائج بعدها | متوسط | عالي | في `TestCatalogService.DeleteReferenceRangeAsync`، تحذير عند محاولة تعطيل آخر Range نشطة |
| Embedded Excel resource يضخّم حجم الـ assembly | منخفض | منخفض | الملف الموصى به ~50–100 KB، مقبول |
| القاعدة الإنتاجية الحالية تحوي ResultValues بـ Flag = null/فارغ ⇒ بعد G6 لن يتم تفسيرها | منخفض | منخفض | الـ enum يبقى string في التخزين، NULL/empty يبقى مقبول كقيمة "غير محدد" |

---

## 8. Final Recommendation

**هل الخطة آمنة للتنفيذ؟** ✅ **نعم — بشروط محددة:**
1. أخذ Backup كامل للقاعدة قبل تطبيق Migration الخطوة 3.
2. تنفيذ كل خطوة على بيئة QA أولاً (مرآة من الإنتاج)، التحقق من معايير القبول، ثم النقل للإنتاج.
3. تنفيذ Migration الخطوة 3 في إطار صيانة معلَن، خارج ساعات العمل.
4. مراجعة سريرية يدوية لـ Ranges بعد الـ Data Migration للتأكد من أن ربط `ParameterId` تلقائياً أنتج نتائج سريرية معقولة (خاصة CBC و Lipid).

**ما هي الخطوة الأولى التي يجب البدء بها فوراً؟**
👉 **الخطوة 1 — تعديل النماذج (Models)**. السبب: كل خطوة لاحقة (DbContext، Migration، Services، UI) تعتمد مباشرةً عليها. هي خطوة "آمنة" تماماً (لا تمس قاعدة بيانات الإنتاج ولا تكسر السلوك الحالي حتى يُطبَّق Migration الخطوة 3). تنفيذها أولاً يُتيح فحص استمرار البناء (`dotnet build`) والاتفاق على الـ Domain Model قبل أي عمل أعمق.

**تحذيرات معمارية أخيرة قبل التنفيذ:**

1. **حول القرار 1 (G3/G12):** التوصية المعمارية الأمتن — إضافة `ParameterId` **كعمود اختياري (`int?`)** مع إبقاء `TestId` (للتحاليل الأحادية المعامل، حيث لا حاجة لـ Parameter منفصل أو حيث Range "عام" يُطبَّق على كل البارامترات كـ default). منطق `ValidateResultAsync` يفضل ParameterId المحدَّد، ثم يعود إلى Range الـ NULL كـ fallback. هذا يحفظ التوافق مع البيانات القديمة ويحقق الاستقلالية للتحاليل المركَّبة.

2. **حول القرار 2 (G13):** Global Query Filter قوي لكنه يحمل خطر "بيانات مخفية" في كل تقرير تاريخي. **يجب** مراجعة كل query في الخدمات التالية وإضافة `IgnoreQueryFilters()` حيث يلزم:
   - `ReportService.BuildVisitReportAsync` (تقارير قديمة قد تحوي تحاليل عُطِّلت لاحقاً).
   - `CompareWithHistoryService` (مقارنة مع نتائج قديمة).
   - أي query في الإحصاءات والمراجعات.

3. **حول القرار 3 (G6):** عدم تغيير نوع العمود `Flag` في DB من `nvarchar` إلى enum. الإبقاء على string في DB + التحويل في طبقة الخدمة يحفظ التوافق مع البيانات القديمة ويتجنب Data Migration للنتائج الموجودة.

4. **حول القرار 4 (G14):** ✅ مستبعدة صراحةً من هذه الخطة. كل ما يخص شاشات إدارة `TestGroup` و `Unit` و `SampleType` المستقلة يُؤجَّل لموديول "بيانات النظام (System Data Module)" المنفصل. حالياً تتم إدارة هذه الكيانات حصراً عبر Sheets `Departments` و `Units` و `SampleTypes` في ملف الـ Seed (Excel) عبر `MedicalCatalogImporter` (الخطوة 11)، أو عبر التوسعة البسيطة لواجهة التحاليل الحالية عند الحاجة الملحَّة.

5. **حول البيئة الإنتاجية الحالية:** بما أن النظام يعمل على القاعدة الإنتاجية الحقيقية، **يجب** اعتبار كل خطوة فيها Migration (الخطوة 3 فقط) خطوة عمليات حقيقية لها إطار صيانة. باقي الخطوات (4–11) تغييرات كود فقط، يمكن نشرها بأي بناء عادي.

---

📌 **تذكير ختامي:** لم يُعدَّل أي ملف في المستودع، ولم يُكتب أي كود تنفيذي. كل ادعاء أعلاه مدعوم بـ ملف + سطر + مقتطف من الكوميت الحالي `227815f`. شاشات إدارة `TestGroup` و `Unit` و `SampleType` المستقلة (G14) مستبعدة صراحةً من هذه الخطة وستُنفَّذ لاحقاً ضمن موديول "بيانات النظام". الخطة جاهزة للانتقال إلى مرحلة التنفيذ بمجرد إقرارك للقرارات المعمارية الأربعة وتأكيد البدء.