# خطة الإصلاح الشامل لمشروع اختبارات الوحدة — Open_lab.Tests

> **ملف مرجعي للوكلاء البرمجيين**
> المستودع: Open_lab | الفرع: Fo4uŕ
>
> **⚠️ تعليمات إلزامية للوكيل البرمجي قبل البدء:**
> 1. اقرأ هذا الملف بالكامل أولاً.
> 2. انظر إلى جدول **"حالة المراحل"** أدناه — نفّذ المرحلة المحددة في التكليف فقط.
> 3. اعمل دائماً على **آخر حالة للفرع Fo4uŕ** — لا تستعِد commit قديماً.
> 4. لا تنفّذ `dotnet build` أو `dotnet test` — المستخدم يتولى ذلك بنفسه.
> 5. بعد إنهاء المرحلة، حدّث خانة **"الحالة"** في الجدول إلى ✅ مكتملة.

---

## جدول حالة المراحل

| # | المرحلة | الحالة |
|---|---|---|
| 1 | إصلاح الاختبارَين بدون أي Assert | ✅ مكتملة |
| 2 | إصلاح 117 اختباراً وهمياً (Verify فقط) | ✅ مكتملة |
| 3 | حذف ودمج 20 نسخة مكررة | ⏳ لم تبدأ |
| 4 | إصلاح المخالفة المعمارية (نقل فئات ViewModel) | ⏳ لم تبدأ |
| 5 | إضافة تعليق // Function: X.X للـ 418 اختبار | ⏳ لم تبدأ |
| 6 | إكمال بنية AAA للـ 417 اختبار | ⏳ لم تبدأ |
| 7 | إعادة تسمية 28 اختباراً مخالفاً | ⏳ لم تبدأ |
| 8 | سد فجوات التغطية في 32 وظيفة جزئية | ⏳ لم تبدأ |

---

## الصورة الإجمالية للمشروع

> **ملاحظة:** تشغيل الاختبارات الفعلي يُظهر **1,483 اختباراً** (كلها ناجحة). التدقيق المستقل حلّل حالة سابقة ووجد 1,465 — الفارق (18 اختباراً) يعني أن اختبارات أُضيفت لاحقاً. أرقام المشاكل أدناه مبنية على التدقيق؛ على الوكيل التحقق من الكود الفعلي ولا يتقيد بالأرقام كحد أقصى.

| البند | القيمة |
|---|---|
| إجمالي الاختبارات (الفعلي) | 1,483 |
| اختبارات وهمية (Fake) | 119 |
| اختبارات ضعيفة | 0 ✅ |
| مجموعات أسماء مكررة | 14 مجموعة — 20 نسخة زائدة |
| اختبارات بدون // Function: | 418 (28.5%) |
| اختبارات بدون AAA كاملة | 417 (28.5%) |
| استخدام ExecuteAsync المحظور | 0 ✅ |
| أسماء مخالفة لنمط Function_State_Result | 28 |
| وظائف مغطاة بالكامل (من 97) | 65 / 97 |
| وظائف مغطاة جزئياً | 32 / 97 |
| وظائف غير مغطاة كلياً | 0 ✅ |

---

## القيود الصارمة (تُطبَّق في جميع المراحل)

- ❌ ممنوع تعديل أي ملف داخل `Open_lab` الإنتاجي — التعديلات حصراً على `Open_lab.Tests`
- ❌ ممنوع تنفيذ `dotnet build` أو `dotnet test`
- ❌ ممنوع استخدام `ExecuteAsync` — استعمل `Execute(null)` ثم `await Task.Delay(100..200)`
- ❌ ممنوع `Assert.True(true)` أو `Assert.NotNull` لوحده أو Verify بلا Assert على قيمة أعمال
- ❌ ممنوع تكرار اختبار موجود
- ✅ كل اختبار: السطر الأول `// Function: X.X — Function Name`
- ✅ كل اختبار: يحتوي `// Arrange` و `// Act` و `// Assert`
- ✅ كل اختبار: اسمه `[FunctionName]_[StateUnderTest]_[ExpectedResult]`

---

## المرحلة الأولى — إصلاح الاختبارَين بدون أي Assert

**الملفات المستهدفة:** ملفان، اختباران فقط.

### الاختبار الأول
- **الملف:** `Services/BackupRestoreServiceTests.cs` — السطر 130
- **الاسم:** `RestoreAsync_ShouldExecuteSqlCommand_SuccessGuard`
- **المشكلة:** `try { await _service.RestoreAsync(validPath); } catch { ... }` بلا Assert — ينجح دائماً حتى لو الكود معطوب.
- **الإصلاح:** أعد كتابته ليتأكد من استدعاء الأمر الصحيح أو رمي `ArgumentException` عند مسار غير صالح، مع Assert فعلي.

### الاختبار الثاني
- **الملف:** `Services/Module6Tests_Additional.cs` — السطر 509
- **الاسم:** `MarkSeparatedCommand_CanExecute_When_No_SeparationType_Should_Return_False_EdgeGuard`
- **المشكلة:** تعليق `"may or may not be enabled..."` بلا أي Assert.
- **الإصلاح:** `Assert.False(_viewModel.MarkSeparatedCommand.CanExecute(null))` أو Assert على `StatusMessage`.

---

## المرحلة الثانية — إصلاح 117 اختباراً وهمياً (Verify فقط)

**المشكلة:** تحتوي `Verify(...)` لكن لا تتحقق من خصائص ViewModel.
**الإصلاح:** أضف بعد `Verify` تأكيدات على `StatusMessage` / `ErrorMessage` / الخصائص المتغيرة.

| الملف | العدد |
|---|---|
| `ViewModels/Module3ViewModelTests_Additional.cs` | 14 |
| `ViewModels/Module13ViewModelTests_Additional.cs` | 13 |
| `ViewModels/PatientBillingViewModelTests.cs` | 9 |
| `ViewModels/ResultsEntryViewModelTests.cs` | 7 |
| `ViewModels/ExternalLabManagementViewModelTests.cs` | 6 |
| `Services/Module6Tests_Additional.cs` | 6 |
| `Services/Module7Tests_Additional.cs` | 6 |
| `ViewModels/Module1And2ViewModelCompletionTests.cs` | 5 |
| `ViewModels/Module3ViewModelCompletionTests.cs` | 5 |
| `ViewModels/Module9ViewModelTests_Additional.cs` | 5 |
| `Services/Module4Tests_Additional.cs` | 4 |
| `ViewModels/Module12ViewModelTests_Additional.cs` | 3 |
| `ViewModels/PatientTestsSelectionViewModelTests.cs` | 3 |
| `ViewModels/PriceListsViewModelTests.cs` | 3 |
| `ViewModels/SettingsViewModelTests.cs` | 3 |
| `ViewModels/TestCatalogViewModelTests.cs` | 3 |
| `Services/Module8Tests_Additional.cs` | 2 |
| `ViewModels/GroupWorksheetViewModelTests.cs` | 2 |
| `ViewModels/LoginViewModelTests.cs` | 2 |
| `ViewModels/Module4ViewModelCompletion_Part1.cs` | 2 |
| `ViewModels/Module4ViewModelCompletion_Part3.cs` | 2 |
| `ViewModels/SampleCollectionViewModelTests.cs` | 2 |
| `ViewModels/SystemSettingsViewModelTests.cs` | 2 |
| `ViewModels/WorksheetViewModelTests.cs` | 2 |
| `ViewModels/AttendanceLogViewModelTests.cs` | 1 |
| `ViewModels/ContractInvoiceViewModelTests.cs` | 1 |
| `ViewModels/Module4_Function4_8_Tests.cs` | 1 |
| `ViewModels/PhysicianViewModelTests.cs` | 1 |
| `ViewModels/ReferenceRangesViewModelTests.cs` | 1 |
| `ViewModels/TestClassificationLogViewModelTests.cs` | 1 |
| `ViewModels/UsersPermissionsViewModelTests.cs` | 1 |
| **المجموع** | **117** |

---

## المرحلة الثالثة — حذف ودمج 20 نسخة مكررة

**القاعدة:** أبقِ نسخة واحدة قوية؛ أعد تسمية المكررات في ViewModels مختلفة بدلاً من حذفها.

| # | الاسم المكرر | التكرارات | المواقع |
|---|---|---|---|
| 1 | `Commands_When_Admin_Should_Be_Enabled` | 4 | `PatientBillingVMTests:28`، `ResultsEntryVMTests:28`، `SampleCollectionVMTests:34`، `TestCatalogVMTests:30` |
| 2 | `PrintCommand_When_No_Rows_Should_Be_Disabled_Edge` | 4 | `WorkSheetByPatientVMTests:61`، `WorkSheetByTestVMTests:51`، `WorksheetVMTests:119`، `WorksheetVMTests:240` |
| 3 | `PrintCommand_When_PrintService_Fails_Should_Set_Error_Message_Failure` | 3 | `GroupWorksheetVMTests:152`، `WorksheetVMTests:102`، `WorksheetVMTests:223` |
| 4 | `SaveAsync_When_ServiceThrows_Should_Show_Error_FailureGuard` | 3 | `Module3VMTests_Additional:136`، `:468`، `:1028` |
| 5 | `LoadCommand_When_Service_Throws_Should_Set_Error_Message_FailureGuard` | 2 | `Module7Tests_Additional:706`، `CombinedReportVMTests:55` |
| 6 | `UpdateAsync_Should_Update_All_Fields` | 2 | `PatientServiceTests:148`، `PhysicianServiceTests:94` — أعد التسمية ليعكس الكيان |
| 7 | `DeleteAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard` | 2 | `Module3VMTests_Additional:239`، `TestCommentsVMTests:132` |
| 8 | `DeleteItemAsync_Should_Remove_Item_SuccessGuard` | 2 | `Module3VMTests_Additional:577`، `:751` |
| 9 | `DeleteItemAsync_When_ServiceThrows_Should_Show_Error_FailureGuard` | 2 | `Module3VMTests_Additional:598`، `:768` |
| 10 | `SetEntityCommission_WithExactly100Percent_ShouldAllow_EdgeGuard_BR_ACC_008` | 2 | `Module12ServiceTests:411`، `Module12VMTests:266` — أعد التسمية إلى Service… و ViewModel… |
| 11 | `LoadCommand_Should_Load_Rows_Success` | 2 | `WorkSheetByPatientVMTests:21`، `WorkSheetByTestVMTests:15` |
| 12 | `LoadCommand_When_Service_Throws_Should_Set_Error_Failure` | 2 | `WorkSheetByPatientVMTests:41`، `WorkSheetByTestVMTests:33` |
| 13 | `LoadCommand_When_Service_Fails_Should_Set_Error_Message_Failure` | 2 | `WorksheetVMTests:69`، `:190` |
| 14 | `LoadCommand_When_Service_Returns_Empty_Should_Set_Zero_Status_Edge` | 2 | `WorksheetVMTests:85`، `:206` |

---

## المرحلة الرابعة — إصلاح المخالفة المعمارية (نقل فئات ViewModel)

**⚠️ يجب تنفيذها قبل المرحلة الثامنة.**

| الملف الأصلي | ما يبقى فيه | الملف الجديد |
|---|---|---|
| `Services/Module5Tests_Additional.cs` | `Module5ServiceTests_Additional` فقط | `ViewModels/Module5ViewModelTests_Additional.cs` |
| `Services/Module6Tests_Additional.cs` | `Module6ServiceTests_Additional` فقط | `ViewModels/Module6ViewModelTests_Additional.cs` |
| `Services/Module7Tests_Additional.cs` | `Module7ServiceTests_Additional` فقط | `ViewModels/Module7ViewModelTests_Additional.cs` |
| `Services/Module8Tests_Additional.cs` | `Module8ServiceTests_Additional` فقط | `ViewModels/Module8ViewModelTests_Additional.cs` |

---

## المرحلة الخامسة — إضافة تعليق // Function: X.X للـ 418 اختبار

**القاعدة:** السطر الأول داخل كل اختبار:
```csharp
// Function: X.X — Function Name
```
استنبط الرقم من سياق الملف واعتمد على `Open_lab_Modules_Documentation.md`.

---

## المرحلة السادسة — إكمال بنية AAA للـ 417 اختبار

**القاعدة:** كل اختبار يجب أن يحتوي:
```csharp
// Arrange
...
// Act
...
// Assert
...
```

---

## المرحلة السابعة — إعادة تسمية 28 اختباراً مخالفاً

**القاعدة:** ثلاثة أجزاء على الأقل: `[FunctionName]_[StateUnderTest]_[ExpectedResult]`

| الاسم الحالي | الاسم المقترح | الملف |
|---|---|---|
| `RecordAttendance_ShouldPersistLogInDatabase` | `RecordAttendance_WithValidUser_ShouldPersistLogInDatabase` | `AttendanceServiceTests.cs` |
| `TogglePasswordVisibilityCommand_ShouldToggleIsPasswordVisible` | `TogglePasswordVisibilityCommand_WhenInvoked_ShouldToggleIsPasswordVisible` | `LoginViewModelTests.cs` |
| `ReloadCommand_ShouldLoadUsersAndRoles` | `ReloadCommand_WhenInvoked_ShouldLoadUsersAndRoles` | `UsersPermissionsViewModelTests.cs` |
| `DeleteUser_ShouldRemoveUserAndUserRoles` | `DeleteUser_WithExistingUser_ShouldRemoveUserAndUserRoles` | `UserAdminServiceTests.cs` |
| `DeleteRole_ShouldRemoveRoleAndPermissions` | `DeleteRole_WithExistingRole_ShouldRemoveRoleAndPermissions` | `UserAdminServiceTests.cs` |
| `GetRoles_ShouldReturnRolesOrderedByName` | `GetRoles_WhenCalled_ShouldReturnRolesOrderedByName` | `UserAdminServiceTests.cs` |
| `GetUsers_ShouldReturnUsersOrderedByUsername` | `GetUsers_WhenCalled_ShouldReturnUsersOrderedByUsername` | `UserAdminServiceTests.cs` |
| `GetUserActivityLog_ShouldRespectCountLimit` | `GetUserActivityLog_WithCountLimit_ShouldRespectLimit` | `UserActivityServiceTests.cs` |
| `UpdateReferenceRangeAsync_ShouldPersistChanges` | `UpdateReferenceRangeAsync_WithValidRange_ShouldPersistChanges` | `TestCatalogServiceTests_Additional.cs` |
| `DeleteReferenceRangeAsync_ShouldRemoveRange` | `DeleteReferenceRangeAsync_WithExistingRange_ShouldRemoveRange` | `TestCatalogServiceTests_Additional.cs` |
| `CreateTestCommentAsync_ShouldTrimWhitespace` | `CreateTestCommentAsync_WithWhitespaceInput_ShouldTrimWhitespace` | `TestCatalogServiceTests_Additional.cs` |
| `UpdatePriceListAsync_ShouldUpdateAllFields` | `UpdatePriceListAsync_WithValidData_ShouldUpdateAllFields` | `TestCatalogServiceTests_Additional.cs` |
| `DeletePriceListItemAsync_ShouldRemoveItem` | `DeletePriceListItemAsync_WithExistingItem_ShouldRemoveItem` | `TestCatalogServiceTests_Additional.cs` |
| `GetAllTestsAsync_ShouldReturnOrderedByNameReport` | `GetAllTestsAsync_WhenCalled_ShouldReturnOrderedByName` | `TestCatalogServiceTests_Additional.cs` |
| `CreateTestGroupAsync_ShouldCreateGroup` | `CreateTestGroupAsync_WithValidData_ShouldCreateGroup` | `TestCatalogServiceTests_Additional.cs` |
| `CreateSampleTypeAsync_ShouldCreateSampleType` | `CreateSampleTypeAsync_WithValidData_ShouldCreateSampleType` | `TestCatalogServiceTests_Additional.cs` |
| `CreateUnitAsync_ShouldCreateUnit` | `CreateUnitAsync_WithValidData_ShouldCreateUnit` | `TestCatalogServiceTests_Additional.cs` |
| `PatientCountByMonth_ShouldIncludeArabicMonthNames` | `PatientCountByMonth_WhenLoaded_ShouldIncludeArabicMonthNames` | `Module9ServiceTests_Additional.cs` |
| `TestDemandAnalysis_ShouldCalculateTotalRevenuePerTest` | `TestDemandAnalysis_WhenLoaded_ShouldCalculateTotalRevenuePerTest` | `Module9ServiceTests_Additional.cs` |
| `SampleCountPerYear_ShouldReturnRowsOrderedByYearAscending` | `SampleCountPerYear_WhenLoaded_ShouldReturnRowsOrderedByYearAscending` | `Module9ServiceTests_Additional.cs` |
| `GetReferralsAsync_ShouldReturnAllReferralsOrderedByName` | `GetReferralsAsync_WhenCalled_ShouldReturnAllReferralsOrderedByName` | `Module9ServiceTests_Additional.cs` |
| `UserProductivityReport_ShouldReturnRowsOrderedByCompletedTestsDescending` | `UserProductivityReport_WhenLoaded_ShouldReturnRowsOrderedByCompletedTestsDescending` | `Module9ServiceTests_Additional.cs` |
| `GetSnapshotAsync_ShouldCalculateTotalPatientsAndVisitsSeparately` | `GetSnapshotAsync_WhenCalled_ShouldCalculateTotalPatientsAndVisitsSeparately` | `Module9ServiceTests_Additional.cs` |
| (+ 5 أسماء مماثلة في `TestCatalogServiceTests_Additional.cs`) | راجع كل اسم بجزأين فقط | — |

---

## المرحلة الثامنة — سد فجوات التغطية في 32 وظيفة جزئية

**⚠️ نفّذ بعد اكتمال المرحلة الرابعة حتماً.**
**القاعدة:** كل وظيفة: نجاح + فشل في Service، ونجاح + فشل في ViewModel.

| الوظيفة | الملف المستهدف | ما ينقص |
|---|---|---|
| 4.5 — Arrange Report Order | `ViewModels/ResultsEntryViewModelTests.cs` | VM نجاح + فشل |
| 4.9 — Compare with History | `ViewModels/ResultsEntryViewModelTests.cs` | VM نجاح + فشل |
| 5.1 → 5.7 (7 وظائف) | `ViewModels/Module5ViewModelTests_Additional.cs` | VM نجاح + فشل لكل |
| 6.1 → 6.4 (4 وظائف) | `ViewModels/Module6ViewModelTests_Additional.cs` | مراجعة وإكمال |
| 7.1 → 7.4 (4 وظائف) | `ViewModels/Module7ViewModelTests_Additional.cs` | مراجعة وإكمال |
| 8.1 → 8.7 (7 وظائف) | `ViewModels/Module8ViewModelTests_Additional.cs` | مراجعة وإكمال |
| 10.5 — Record Departure | الملف المناسب | VM نجاح + فشل |
| 10.8 — Logout | الملف المناسب | اختبار فشل VM |
| 12.2 — Link Price List to Entity | `ViewModels/Module12ViewModelTests_Additional.cs` | VM نجاح + فشل |
| 12.5 — Assign Patient to Contract | `ViewModels/Module12ViewModelTests_Additional.cs` | VM نجاح + فشل |
| 13.2 — Set Paper Size | الملف المناسب | إكمال VM |
| 13.4 — Set Default Account Type | الملف المناسب | إكمال VM |
| 13.5 — Configure Printers | الملف المناسب | إكمال VM |
| 13.6 — Set Invoice Settings | الملف المناسب | إكمال VM |

---

## الأهداف النهائية

| البند | قبل | الهدف |
|---|---|---|
| اختبارات وهمية | 119 | 0 |
| نسخ مكررة | 20 | 0 |
| بدون // Function: | 418 | 0 |
| بدون AAA | 417 | 0 |
| أسماء مخالفة | 28 | 0 |
| وظائف مغطاة بالكامل | 65/97 | 97/97 |
| ExecuteAsync | 0 ✅ | 0 ✅ |
