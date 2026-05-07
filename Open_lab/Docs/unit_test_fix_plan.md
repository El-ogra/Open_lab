
# خطة الإصلاح الشامل لمشروع اختبارات الوحدة — Open_lab.Tests

> **ملف مرجعي للوكلاء البرمجيين**
> المستودع: Open_lab | الفرع: Fo4uŕ | 
>
> **⚠️ تعليمات إلزامية للوكيل البرمجي قبل البدء:**
> 1. اقرأ هذا الملف بالكامل أولاً.
> 2. انظر إلى جدول **"حالة المراحل"** أدناه — نفّذ المرحلة المحددة في التكليف فقط.
> 3. اعمل دائماً على **آخر حالة للفرع Fo4uŕ** — لا تستعِد commit قديماً.
> 4. لا تنفّذ `dotnet build` أو `dotnet test` — المستخدم يتولى ذلك بنفسه.
> 5. بعد إنهاء المرحلة، حدّث خانة **"الحالة"** في الجدول إلى ✅ مكتملة.
> 6. **قاعدة تتبع الفشل (إلزامية في المراحل 5 و6):** إذا أدى تعديلك إلى فشل اختبار كان ناجحاً، لا تصلحه — سجّله في **"سجل الفشل المتراكم"** في نهاية هذا الملف. المرحلة 7 ستعالج كل الفشل دفعة واحدة.

---

## جدول حالة المراحل

| # | المرحلة | الحالة |
|---|---|---|
| 1 | إكمال بنية AAA للـ 13 اختباراً في Module5ViewModelTests_Additional | ✅ مكتملة |
| 2 | إصلاح الاختبار الوهمي الواحد | ⏳ لم تبدأ |
| 3 | استبدال الـ 255 Placeholder بأرقام وظائف حقيقية | ⏳ لم تبدأ |
| 4 | دمج الـ 7 أزواج المكررة | ⏳ لم تبدأ |
| 5 | تقوية الـ 56 اختباراً الضعيفة | ⏳ لم تبدأ |
| 6 | سد فجوات التغطية في الـ 22 وظيفة جزئية | ⏳ لم تبدأ |
| 7 | إصلاح الاختبارات الفاشلة وإصلاح ViewModels في الإنتاج | ⏳ لم تبدأ |
| 8 | التحقق النهائي والتقرير | ⏳ لم تبدأ |

---

## الصورة الإجمالية للمشروع

> على الوكيل التحقق من الكود الفعلي ولا يتقيد بالأرقام كحد أقصى.

| البند | القيمة |
|---|---|
| إجمالي ملفات الاختبارات | 108 (Services: 53، ViewModels: 54، Integration: 1) |
| إجمالي ميثودات الاختبار (`[Fact]` + `[Theory]`) | 1,505 (1,502 Fact + 3 Theory) |
| إجمالي حالات الاختبار الفعلية (تشمل InlineData) | 1,523 |
| اختبارات وهمية (Fake) | 1 |
| اختبارات ضعيفة | 56 |
| أزواج مكررة | 7 أزواج |
| Placeholder بدون رقم وظيفة حقيقي | 255 (16.9%) |
| اختبارات بدون AAA كاملة | 13 (كلها في ملف واحد) |
| استخدام `ExecuteAsync` المحظور | 0 ✅ |
| أسماء مخالفة لنمط `Function_State_Result` | 0 ✅ |
| وظائف مغطاة بالكامل (من 97) | 75 / 97 |
| وظائف مغطاة جزئياً | 22 / 97 |
| وظائف غير مغطاة كلياً | 0 ✅ |
| اختبارات فاشلة (نتيجة Visual Studio) | 21 |

---

## القيود الصارمة (تُطبَّق في جميع المراحل)

- ❌ ممنوع تعديل أي ملف داخل `Open_lab` الإنتاجي —
  **استثناء وحيد: المرحلة 7 فقط تشمل تعديل ViewModels وملف StatusMessages.cs**
- ❌ ممنوع تنفيذ `dotnet build` أو `dotnet test`
- ❌ ممنوع استخدام `ExecuteAsync` — استعمل `Execute(null)` ثم `await Task.Delay(100..200)`
- ❌ ممنوع `Assert.True(true)` أو `Assert.NotNull` لوحده أو `Verify` بلا Assert على قيمة أعمال
- ❌ ممنوع تكرار اختبار موجود
- ✅ كل اختبار: السطر الأول داخل الجسم `// Function: X.X — Function Name`
- ✅ كل اختبار: يحتوي `// Arrange` و `// Act` و `// Assert`
- ✅ كل اختبار: اسمه `[FunctionName]_[StateUnderTest]_[ExpectedResult]`

---

## المرحلة الأولى — إكمال بنية AAA للـ 13 اختباراً الناقصة

**الملف المستهدف الوحيد:** `ViewModels/Module5ViewModelTests_Additional.cs`

**المشكلة:** 13 اختباراً تفتقد تعليقات `// Arrange` و `// Act` و `// Assert`
صراحةً رغم وجود المنطق فعلاً.

**الإصلاح:** أضف التعليقات الثلاثة في المكان المناسب — لا تغيّر أي منطق،
فقط أضف التعليقات.

**مثال على المشكلة والإصلاح:**
```csharp
// ❌ قبل
public async Task AddCultureAsync_With_Empty_Name_Should_Set_Error_FailureGuard()
{
    // Function: 5.1 — Enter Culture Data
    _viewModel.NewCultureName = "";
    await _viewModel.AddCultureCommand.Execute(null);
    _viewModel.StatusMessage.Should().Contain("خطأ");
}

// ✅ بعد
public async Task AddCultureAsync_With_Empty_Name_Should_Set_Error_FailureGuard()
{
    // Function: 5.1 — Enter Culture Data
    // Arrange
    _viewModel.NewCultureName = "";
    // Act
    await _viewModel.AddCultureCommand.Execute(null);
    // Assert
    _viewModel.StatusMessage.Should().Contain("خطأ");
}
```

**الهدف:** 0 اختبارات بدون AAA، صفر تغيير في المنطق.

---

## المرحلة الثانية — إصلاح الاختبار الوهمي الواحد

**الملف:** `Services/BackupRestoreServiceTests.cs` — السطر 138

**الاسم الحالي:** `RestoreAsync_WithValidPath_ShouldNotThrowArgumentException_SuccessGuard`

**المشكلة:**
```csharp
var exception = await Record.ExceptionAsync(
    async () => await _service.RestoreAsync(validPath));
Assert.False(exception is ArgumentException, "...");
// ينجح حتى لو أُلقي NullReferenceException أو IOException
```

**الإصلاح:** استبدله باختبارَين:

```csharp
// الاختبار 1 — التحقق من رمي ArgumentException عند مسار غير صالح
[Fact]
public async Task RestoreAsync_WithEmptyPath_ShouldThrowArgumentException_FailureGuard()
{
    // Function: 13.7 — Configure Backup
    // Arrange
    var invalidPath = string.Empty;
    // Act
    var exception = await Record.ExceptionAsync(
        async () => await _service.RestoreAsync(invalidPath));
    // Assert
    exception.Should().BeOfType<ArgumentException>();
}

// الاختبار 2 — التحقق من عدم رمي ArgumentException عند مسار صالح
[Fact]
public async Task RestoreAsync_WithValidPath_ShouldNotThrowArgumentException_SuccessGuard()
{
    // Function: 13.7 — Configure Backup
    // Arrange
    var validPath = Path.Combine(Path.GetTempPath(), "test.bak");
    // Act
    var exception = await Record.ExceptionAsync(
        async () => await _service.RestoreAsync(validPath));
    // Assert
    (exception is ArgumentException).Should().BeFalse(
        "مسار صالح لا يجب أن يُسبب ArgumentException");
    (exception is ArgumentNullException).Should().BeFalse();
}
```

**الهدف:** 0 اختبارات وهمية.

---

## المرحلة الثالثة — استبدال الـ 255 Placeholder بأرقام وظائف حقيقية

**المشكلة:** 255 اختباراً يحتوي:
```csharp
// Function: X.X — To Be Determined
```

**الإصلاح:** استبدل كل placeholder بالرقم والاسم الصحيح مستعيناً بـ
`Open_lab/Docs/Open_lab_Modules_Documentation.md`.

**قاعدة الاستنباط — أمثلة:**

| بداية اسم الميثود | الرقم المقابل |
|---|---|
| `MarkCollectedAsync` | Function 6.1 |
| `MarkNotCollectedAsync` | Function 6.2 |
| `LoadVisitTests` | Function 4.1 |
| `SaveResultsAsync` | Function 4.3 |
| `VerifyResultsAsync` | Function 4.4 |
| `RecordAttendance` | Function 10.4 |
| `RecordDeparture` | Function 10.5 |

استند دائماً إلى التوثيق — لا تخمّن.

**الملفات ذات الأولوية (الأكثر placeholders):**
- `ViewModels/SampleCollectionViewModelTests.cs`
- `ViewModels/ResultsEntryViewModelTests.cs`
- `Services/AppSessionTests.cs`

**الهدف:** 0 placeholder، 100% ربط بالتوثيق.

---

## المرحلة الرابعة — دمج الـ 7 أزواج المكررة

**القاعدة:**
- إذا كان الزوجان يختبران نفس المنطق بمُدخلات مختلفة: ادمجهما في `[Theory]` مع `[InlineData]`.
- إذا كانا في ViewModels مختلفة يختبران كيانَين مختلفَين: أعد التسمية لإبراز الفرق بدلاً من الحذف.

### الأزواج السبعة:

**الزوج 1 — (Module5Tests_Additional) — دمج إلزامي:**
- `ClassifySensitivity_Should_Handle_Case_Insensitive_SuccessGuard` (9 InlineData)
- `ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard` (3 InlineData)
- جسماهما متطابقان حرفياً — الفرق في بيانات InlineData فقط.
- الإصلاح: ادمجهما في Theory واحد باسم
  `ClassifySensitivity_WithVariousFormats_ShouldReturnCorrectCode` مع 12 صف InlineData.

**الزوج 2 — (TestCatalog — Empty/Null validation) — دمج:**
- ثلاثة اختبارات: `WithEmptyCode` / `WithEmptyNameReport` / `WithEmptyNameReceipt`
- الإصلاح: Theory واحد مع InlineData لكل حقل.

**الزوج 3 — (CustomGroup — Empty/Null Name) — دمج:**
- `CreateCustomGroupAsync_WithEmptyName_…` و `WithNullName_…`
- الإصلاح: `[InlineData("")]` و `[InlineData(null)]` في Theory واحد.

**الزوج 4 — (PriceList — Empty/Null Name) — دمج:**
- نفس نهج الزوج 3.

**الزوج 5 — (AccountsTreasury — Weekly/Monthly) — إعادة تسمية:**
- `WeeklyCommand_…StartOfWeek` و `MonthlyCommand_…StartOfMonth`
- يختبران كيانَين مختلفَين — لا دمج. أعد التسمية لإبراز الدورية في الاسم.

**الزوج 6 — (Module4_Part3 — Zero/Negative VisitId) — دمج:**
- `PreviewReport_LoadCommand_With_Zero_VisitId_…`
  و `With_Negative_VisitId_…`
- الإصلاح: `[InlineData(0)]` و `[InlineData(-1)]` في Theory واحد.

**الزوج 7 — مُغطَّى ضمن الزوج 1 أعلاه.**

**الهدف:** تقليل 14 ميثوداً إلى 7، 0 تكرارات.

---

## المرحلة الخامسة — تقوية الـ 56 اختباراً الضعيفة

> ⚠️ **قاعدة تتبع الفشل:** إذا أدى إضافة Assertions جديدة إلى فشل اختبار كان ناجحاً،
> سجّله في **"سجل الفشل المتراكم"** ولا تصلحه.

### المجموعة أ — Verify-only في ViewModels (18 اختباراً):

**المشكلة:** `_xxxServiceMock.Verify(...)` بدون Assert على خصائص الـ ViewModel.

```csharp
// ❌ قبل
_patientServiceMock.Verify(x => x.DeleteAsync(patientId), Times.Once);

// ✅ بعد
_patientServiceMock.Verify(x => x.DeleteAsync(patientId), Times.Once);
_viewModel.StatusMessage.Should().NotBeNullOrEmpty();
// أو الخاصية الأنسب حسب السياق (Items.Count، SelectedItem، IsLoading...)
```

### المجموعة ب — BeNull/NotBeNull فقط في Services (36 اختباراً):

**المشكلة:** `result.Should().NotBeNull()` دون التحقق من القيم الفعلية.

```csharp
// ❌ قبل
var created = await _service.CreatePatientAsync(dto);
created.Should().NotBeNull();

// ✅ بعد
var created = await _service.CreatePatientAsync(dto);
created.Should().NotBeNull();
created.Name.Should().Be(dto.Name);
created.Id.Should().BeGreaterThan(0);
```

### الحالتان الإضافيتان (رقم 55 و56):

- `ViewModels/PatientHistoryViewModelTests.cs:108`
  — `PrintHistoryAsync_When_HistoryIsNull_Should_NotCall_PrintService_EdgeGuard`
  → أضف Assert على `StatusMessage` أو خاصية تعكس حالة الـ null.

- `ViewModels/ResultsEntryViewModelTests.cs:40`
  — `LoadVisitTestsAsync_Should_Call_Service`
  → استبدل `NotBeNull` بفحص عدد العناصر أو قيمة محددة.

**الهدف:** ≤ 10 اختبارات ضعيفة.

---

## المرحلة السادسة — سد فجوات التغطية في الـ 22 وظيفة جزئية

> ⚠️ **قاعدة تتبع الفشل:** الاختبارات الجديدة التي ستفشل بسبب سلوك ViewModel
> الحالي (مثل غياب StatusMessage) — سجّلها في **"سجل الفشل المتراكم"** ولا تصلحها.

**القاعدة:** كل وظيفة تحتاج: نجاح + فشل في Service، ونجاح + فشل في ViewModel.

| الوظيفة | الملف المستهدف | ما ينقص |
|---|---|---|
| 4.5 — ترتيب التقرير | `ViewModels/ResultsEntryViewModelTests.cs` | VM-failure |
| 4.9 — المقارنة مع التاريخ | `ViewModels/ResultsEntryViewModelTests.cs` | VM-success + VM-failure |
| 5.3 — تسجيل الحساسية | الملف المناسب في Services | S-failure |
| 5.4 — تصنيف الحساسية | `ViewModels/Module5ViewModelTests_Additional.cs` | VM-failure |
| 5.5 — تصفية الحوامل | Services + ViewModels/Module5 | S-failure + VM-failure |
| 5.6 — تصفية الأطفال | Services + ViewModels/Module5 | S-failure + VM-failure |
| 7.2 — ورقة عمل التحليل | الملف المناسب في Services | S-failure |
| 7.4 — سجل تصنيف التحاليل | الملف المناسب في Services | S-failure |
| 8.1 | `ViewModels/ExternalLabManagementViewModelTests.cs` | VM-failure |
| 8.2 | Services + ViewModels/ExternalLab | S-failure + VM-success + VM-failure |
| 8.3 | Services + ViewModels/ExternalLab | S-failure + VM-failure |
| 8.4 | Services + ViewModels/ExternalLab | S-failure + VM-failure |
| 8.7 | Services + ViewModels/ExternalLab | S-failure + VM-failure |
| 10.4 — تسجيل الحضور | الملف المناسب في Services | S-failure |
| 10.5 — تسجيل الانصراف | Services + ViewModels | S-failure + VM-success |
| 10.8 — تسجيل الخروج | الملف المناسب في ViewModels | VM-failure |
| 12.2 — ربط قائمة أسعار | `ViewModels/Module12ViewModelTests_Additional.cs` | VM-success + VM-failure |
| 12.5 — ربط المريض بالجهة | `ViewModels/Module12ViewModelTests_Additional.cs` | VM-success + VM-failure |
| 13.2 — Set Paper Size | الملف المناسب في ViewModels | VM-success + VM-failure |
| 13.4 — Set Default Account Type | الملف المناسب في ViewModels | S-failure + VM-failure |
| 13.5 — Configure Printers | الملف المناسب في ViewModels | S-failure + VM-failure |
| 13.6 — Set Invoice Settings | الملف المناسب في ViewModels | S-failure + VM-failure |

**الهدف:** 95+ وظيفة بتغطية كاملة من 97.

---

## المرحلة السابعة — إصلاح الاختبارات الفاشلة وإصلاح ViewModels

> ⚠️ **هذه المرحلة الوحيدة التي تشمل تعديل ملفات `Open_lab` الإنتاجية.**
> التعديل محصور في: **ViewModels** + ملف ثوابت **StatusMessages.cs** فقط.

**المدخل:** قائمة الفشل النهائية =
الـ 21 الأصلية + كل ما في "سجل الفشل المتراكم" من المراحل 5 و6.

### الخطوة الأولى — قرار الإطار (قبل أي تعديل)

| نمط الفشل | القرار |
|---|---|
| `StatusMessage` فارغ عند رفض مُدخل | **أصلح ViewModel** — أضف رسالة حالة |
| نص رسالة في الإنتاج يختلف عن المتوقع في الاختبار | **وحّد القاموس** — أنشئ `StatusMessages.cs` |
| توقع الاختبار غير واقعي (InMemory limitation) | **عدّل الاختبار** فقط |

### الخطوة الثانية — النمط الأول (16 اختباراً من الـ 21)

**المشكلة:** ViewModels تعود مبكراً بدون ضبط `StatusMessage`.

```csharp
// ❌ قبل — في ViewModel الإنتاجي
if (SelectedRow == null) return;

// ✅ بعد
if (SelectedRow == null)
{
    StatusMessage = "يرجى تحديد صف";
    return;
}
```

الـ 16 اختباراً المعنية (من الملفات التالية):
- `Module6ViewModelTests_Additional` — `MarkExternalCollectedAsync_With_Null_SelectedRow_…`
- `SampleCollectionViewModelTests` — `MarkCollectedAsync_With_Null_SelectedRow_…`
- `ReferenceRangesViewModelTests` — `DeleteAsync_When_NoSelection_…`
- `PatientBillingViewModelTests` — `DeletePaymentAsync_With_Null_…`، `AddChargeAsync_With_VisitId_Zero_…`، `PrintInvoiceAsync_When_NoInvoiceFound_…`
- `Module3ViewModelTests_Additional` — `UpdateListAsync_When_Null_…`، `DeleteAsync_When_Null_Selection_…`
- `LoginViewModelTests` — `LoginAsync_WithValidAdminCredentials_…`، `LoginAsync_WithRememberMe_…`، `LoginAsync_WithoutRememberMe_…`
- `Module4ViewModelCompletion_Part3` — `PrintReport_PrintCommand_When_No_Report_…`
- `SystemSettingsViewModelTests` — `ReloadCommand_When_Executed_…`
- `Module1And2ViewModelCompletionTests` — `ViewPatientHistory_MultipleVisits_…`، `SettleAccount_WhenVisitIdIsZero_…`
- (+ أي إدخالات إضافية من سجل الفشل المتراكم بنفس النمط)

### الخطوة الثالثة — النمط الثاني (5 اختبارات من الـ 21)

**المشكلة:** كود الإنتاج يضبط رسالة عامة، الاختبار ينتظر رسالة خاصة.

**الإصلاح:** أنشئ `Open_lab/Constants/StatusMessages.cs`:

```csharp
public static class StatusMessages
{
    public const string SampleMarkedNotCollected = "تم تعليم العينة كغير مجمعة";
    public const string CultureResultSaved       = "تم حفظ نتيجة المزرعة";
    public const string NoVisitTestSelected      = "لم يتم تحديد اختبار";
    public const string VisitIdZero              = "يرجى إدخال رقم زيارة صالح";
    public const string NoSelectionMade          = "يرجى تحديد صف";
}
```

استخدم هذه الثوابت في كلٍّ من الإنتاج والاختبارات بدلاً من السلاسل الحرفية.

الاختبارات الخمسة:
- `SampleCollectionViewModelTests` — `MarkNotCollectedAsync_With_Valid_Row_…`
- `ResultsEntryViewModelTests` — `SaveResultsAsync_With_Null_…`
- `ResultsEntryViewModelTests` — `VerifyResultsAsync_With_Null_…`
- `ResultsEntryViewModelTests` — `ReopenResultsAsync_With_Null_…`
- `Module5ViewModelTests_Additional` — `ClassifySensitivity_WithValidSIRValues_…`

**الهدف:** 0 اختبارات فاشلة — 1,523 / 1,523 ناجحة.

---

## المرحلة الثامنة — التحقق النهائي والتقرير

**العمل:**
1. تحقق من تحقق الأهداف النهائية أدناه.
2. حدّث قسم "الصورة الإجمالية" في هذا الملف بالأرقام الفعلية.
3. قدّم تقريراً ختامياً يوثّق كل ما تغيّر في كل مرحلة.

---

## الأهداف النهائية

| البند | قبل | الهدف |
|---|---|---|
| اختبارات وهمية | 1 | 0 |
| اختبارات ضعيفة | 56 | ≤ 10 |
| Placeholder (`To Be Determined`) | 255 | 0 |
| اختبارات بدون AAA | 13 | 0 |
| أزواج مكررة | 7 | 0 |
| وظائف مغطاة جزئياً | 22 | ≤ 2 |
| اختبارات فاشلة | 21 | 0 |
| حالات ناجحة / إجمالي | 1,502 / 1,523 | 1,523 / 1,523 |
| `ExecuteAsync` | 0 ✅ | 0 ✅ |

---

## سجل الفشل المتراكم

> **تعليمات للوكيل:** إذا ظهرت اختبارات فاشلة جديدة أثناء المراحل 5 أو 6،
> أضفها هنا بالصيغة التالية — لا تصلحها الآن.
>
> **الصيغة:**
> `[اسم الملف] :: [اسم الاختبار] — [سبب الفشل المتوقع] — [ظهر في المرحلة #]`

*(لا إدخالات حتى الآن)*
```

---

ثلاث ملاحظات على الملف:

**أولاً** — أضفت قسم "سجل الفشل المتراكم" في النهاية بصيغة موحدة يملؤها الوكيل تلقائياً أثناء المراحل 5 و6، وهذا يحل المشكلة التي كنا نناقشها.

**ثانياً** — المرحلة 7 تبدأ بـ "خطوة قرار الإطار" صراحةً قبل أي تعديل، حتى لا يبدأ الوكيل بالتعديل عشوائياً.

**ثالثاً** — القيود الصارمة تُذكّر الوكيل أن تعديل ملفات الإنتاج محظور في كل المراحل عدا السابعة.