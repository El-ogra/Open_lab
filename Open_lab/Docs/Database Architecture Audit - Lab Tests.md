# Database Architecture Audit - Lab Tests

> نطاق هذا التقرير هو الحالة الحالية للملفات الموجودة فعلاً داخل الحل البرمجي فقط. لم يتم الاعتماد على أي ملف محذوف أو غير موجود حالياً.

## 1. قائمة الملفات والجداول التي تم تحليلها

### الملفات الحالية ذات الصلة

| النوع | الملف | سبب الإدراج |
|---|---|---|
| Models/Entities | `Open_lab/Models/Entities.cs` | يحتوي كيانات `Test`, `TestParameter`, `TestReferenceRange`, `Unit`, `TestGroup`, `SampleType`, `ResultValue`, `PriceList`, `CustomGroup` في `Entities.cs:174-341` و`379-417`. |
| DbContext | `Open_lab/Data/OpenLabDbContext.cs` | يحتوي `DbSet` للجداول المرتبطة بالتحاليل في `OpenLabDbContext.cs:29-47`، وتكوين العلاقات والقيود في `184-367`. |
| EF Snapshot | `Open_lab/Migrations/OpenLabDbContextModelSnapshot.cs` | يمثل نموذج EF الحالي؛ توجد كيانات `Test`, `TestParameter`, `TestReferenceRange` في `1126`, `1276`, `1306`، وعلاقاتها في `1875-1957`. |
| Migrations | `Open_lab/Migrations/20260523121043_Phase3_AddTestSchemaExtensions.cs` | أضاف `ParameterId` للمعدلات والتعليقات، وفهارس/علاقات `ParameterId` في `26-32`, `73-80`, `88-108`. |
| Migrations | `Open_lab/Migrations/20260523140733_Phase4_AgeRangeRedesign.cs` | أعاد تصميم أعمار المعدلات إلى `AgeFromValue/Unit/Days` و`AgeToValue/Unit/Days` في `23-54`, وأضاف فهرس `AgeFromDays` في `68-70`. |
| Service interface | `Open_lab/Services/ITestCatalogService.cs` | يعرف عمليات إنشاء/تعديل/حذف الاختبارات والمعدلات والقوائم في `9-60`. |
| Service | `Open_lab/Services/TestCatalogService.cs` | ينفذ عمليات الكتالوج على `_db.Tests`, `_db.TestParameters`, `_db.TestReferenceRanges`, `_db.PriceLists`, `_db.CustomGroups` في `20-525`. |
| Results service | `Open_lab/Services/IResultsService.cs`, `Open_lab/Services/ResultsService.cs` | يستخدم `TestParameter` و`TestReferenceRange` عند إدخال/تقييم النتائج في `IResultsService.cs:12-24` و`ResultsService.cs:47-85`, `281-334`. |
| ViewModels | `Open_lab/ViewModels/TestCatalogViewModel.cs` | يدير بيانات الاختبار الأساسية فقط: الاسم، السعر، المجموعة، الوحدة، الوصف في `83-155`, ويحفظ/يعدل في `244-289`. |
| ViewModels | `Open_lab/ViewModels/ReferenceRangesViewModel.cs` | يدير المعدلات الطبيعية ويربطها اختيارياً بـ `SelectedParameter` في `38-52`, `193-202`, `220-250`. |
| ViewModels | `Open_lab/ViewModels/ResultsEntryViewModel.cs` | يحمّل مكوّنات التحليل ونتائجها عبر `GetParametersForTestAsync` و`GetResultsForVisitTestAsync` في `324-333`, ويحفظ النتائج في `400-402`. |
| Navigation/UI | `Open_lab/App.xaml`, `Open_lab/Views` | `App.xaml` لا يحتوي `DataTemplate` لـ `TestCatalogViewModel` أو `ReferenceRangesViewModel`; القوالب الحالية تنتهي عند وحدات عامة مثل `SystemDataModuleViewModel` في `App.xaml:70-78` و`80-94`. ملفات Views الحالية المرتبطة بالكتالوج المباشر غير موجودة حسب `rg --files Open_lab/Views` الذي أظهر فقط `SystemDataModuleView.xaml` عند البحث عن test/reference/price/custom. |

### الجداول الحالية المرتبطة بأنواع التحاليل

من `OpenLabDbContext.cs:29-47`:

`Tests`, `TestGroups`, `SampleTypes`, `Units`, `TestReferenceRanges`, `TestComments`, `VisitTests`, `TestParameters`, `ResultValues`, `PriceLists`, `PriceListItems`, `CustomGroups`, `CustomGroupItems`.

## 2. توثيق هيكل الجداول الحالية

> الأنواع أدناه موثقة من CLR/EF الحالي. حيث وُجدت دقة SQL صريحة، تم ذكرها من `OpenLabDbContext`.

### Tests

مصدر الأعمدة: `Entities.cs:174-210`. التكوين: `OpenLabDbContext.cs:184-202`.

| العمود | النوع | Nullable | FK | مرجع |
|---|---|---:|---|---|
| `TestId` | int | لا | PK | `Entities.cs:176`, `OpenLabDbContext.cs:186` |
| `Code` | string | لا | لا | `Entities.cs:177`, Required + Unique Index في `OpenLabDbContext.cs:187-188` |
| `NameReport` | string | لا | لا | `Entities.cs:178`, Required في `OpenLabDbContext.cs:189` |
| `NameReceipt` | string | لا | لا | `Entities.cs:179`, Required في `OpenLabDbContext.cs:190` |
| `GroupId` | int? | نعم | `TestGroups.GroupId` | `Entities.cs:180`, FK في `OpenLabDbContext.cs:194-196` |
| `SampleTypeId` | int? | نعم | `SampleTypes.SampleTypeId` | `Entities.cs:181`, FK في `OpenLabDbContext.cs:197-199` |
| `UnitId` | int? | نعم | `Units.UnitId` | `Entities.cs:183-186`, FK في `OpenLabDbContext.cs:200-202` |
| `Price` | decimal | لا | لا | `Entities.cs:187`, precision 18,2 في `OpenLabDbContext.cs:191` |
| `CostPrice` | decimal? | نعم | لا | `Entities.cs:188`, precision 18,2 في `OpenLabDbContext.cs:192` |
| `PatientPrice` | decimal? | نعم | لا | `Entities.cs:189`, precision 18,2 في `OpenLabDbContext.cs:193` |
| `TurnaroundHours` | int | لا | لا | `Entities.cs:190` |
| `IsRoutine` | bool | لا | لا | `Entities.cs:191` |
| `IsSendOut` | bool | لا | لا | `Entities.cs:192` |
| `Description` | string? | نعم | لا | `Entities.cs:194`; migration أضافه في `20260523121043...cs:14` |
| `ReportOrder` | int | لا | لا | `Entities.cs:196-200` |

العلاقات: `TestGroup 1-* Tests`, `SampleType 1-* Tests`, `Unit 1-* Tests`, `Test 1-* TestParameters`, `Test 1-* TestReferenceRanges`, `Test 1-* TestComments`, `Test 1-* VisitTests`, `Test 1-* PriceListItems`, `Test 1-* CustomGroupItems` موثقة في `Entities.cs:202-210` و`OpenLabDbContext.cs:194-202`, `230-232`, `246-248`, `263-265`, `272-277`, `347-349`, `365-367`.

### TestParameters

مصدر الأعمدة: `Entities.cs:315-325`. التكوين: `OpenLabDbContext.cs:268-277`.

| العمود | النوع | Nullable | FK | مرجع |
|---|---|---:|---|---|
| `ParameterId` | int | لا | PK | `Entities.cs:317`, `OpenLabDbContext.cs:270` |
| `TestId` | int | لا | `Tests.TestId` | `Entities.cs:318`, `OpenLabDbContext.cs:272-274` |
| `Name` | string | لا | لا | `Entities.cs:319`, Required في `OpenLabDbContext.cs:271` |
| `UnitId` | int? | نعم | `Units.UnitId` | `Entities.cs:320`, `OpenLabDbContext.cs:275-277` |
| `OrderNo` | int | لا | لا | `Entities.cs:321` |

العلاقة: `Test 1-* TestParameters`, و`Unit 1-* TestParameters` في `Entities.cs:323-325`.

### TestReferenceRanges

مصدر الأعمدة: `Entities.cs:238-266`. التكوين: `OpenLabDbContext.cs:223-239`.

| العمود | النوع | Nullable | FK | مرجع |
|---|---|---:|---|---|
| `RangeId` | int | لا | PK | `Entities.cs:240`, `OpenLabDbContext.cs:225` |
| `TestId` | int | لا | `Tests.TestId` | `Entities.cs:241`, `OpenLabDbContext.cs:230-232` |
| `ParameterId` | int? | نعم | `TestParameters.ParameterId` | `Entities.cs:243-246`, FK Restrict + index في `OpenLabDbContext.cs:233-237` |
| `Gender` | string? | نعم | لا | `Entities.cs:248` |
| `AgeFromValue` | int? | نعم | لا | `Entities.cs:254`, snapshot `OpenLabDbContextModelSnapshot.cs:1321` |
| `AgeFromUnit` | string? | نعم | لا | `Entities.cs:255`, max length 10 في `OpenLabDbContext.cs:228` |
| `AgeFromDays` | int? | نعم | لا | `Entities.cs:256`, index في `OpenLabDbContext.cs:238` |
| `AgeToValue` | int? | نعم | لا | `Entities.cs:257`, snapshot `OpenLabDbContextModelSnapshot.cs:1331` |
| `AgeToUnit` | string? | نعم | لا | `Entities.cs:258`, max length 10 في `OpenLabDbContext.cs:229` |
| `AgeToDays` | int? | نعم | لا | `Entities.cs:259`, index في `OpenLabDbContext.cs:239` |
| `LowValue` | decimal? | نعم | لا | `Entities.cs:261`, precision 18,2 في `OpenLabDbContext.cs:226` |
| `HighValue` | decimal? | نعم | لا | `Entities.cs:262`, precision 18,2 في `OpenLabDbContext.cs:227` |
| `NormalText` | string? | نعم | لا | `Entities.cs:263` |

العلاقات: `Test 1-* TestReferenceRanges`، و`TestParameter 1-* TestReferenceRanges` اختيارية من جهة `ParameterId`. الفهارس الحالية: `ParameterId`, `AgeFromDays`, `AgeToDays` في `OpenLabDbContext.cs:237-239`.

### Units

مصدر الأعمدة: `Entities.cs:229-235`. التكوين: `OpenLabDbContext.cs:217-220`.

| العمود | النوع | Nullable | FK | مرجع |
|---|---|---:|---|---|
| `UnitId` | int | لا | PK | `Entities.cs:231`, `OpenLabDbContext.cs:219` |
| `Name` | string | لا | لا | `Entities.cs:232`, Required في `OpenLabDbContext.cs:220` |

العلاقات: `Unit 1-* Tests` و`Unit 1-* TestParameters` في `Entities.cs:234-235`.

### TestGroups و SampleTypes

| الجدول | الأعمدة | العلاقات | مرجع |
|---|---|---|---|
| `TestGroups` | `GroupId int PK`, `GroupName string required` | `TestGroup 1-* Tests` | `Entities.cs:213-218`, `OpenLabDbContext.cs:205-208` |
| `SampleTypes` | `SampleTypeId int PK`, `Name string required` | `SampleType 1-* Tests` | `Entities.cs:221-226`, `OpenLabDbContext.cs:211-214` |

### ResultValues و VisitTests

| الجدول | الأعمدة الأساسية | العلاقات | مرجع |
|---|---|---|---|
| `VisitTests` | `VisitTestId`, `VisitId`, `TestId`, `Price decimal`, `Status string?`, `ReportOrder int` | `Visit 1-* VisitTests`, `Test 1-* VisitTests` | `Entities.cs:295-312`, `OpenLabDbContext.cs:256-265` |
| `ResultValues` | `ResultValueId`, `VisitTestId`, `ParameterId`, `Value`, `Flag`, `Comment`, `VerifiedBy`, `VerifiedAt` | `VisitTest 1-* ResultValues`, `TestParameter 1-* ResultValues`, `User 1-* VerifiedResults` | `Entities.cs:328-341`, `OpenLabDbContext.cs:280-292` |

### PriceLists و CustomGroups

| الجدول | الأعمدة الأساسية | العلاقات | مرجع |
|---|---|---|---|
| `PriceLists` | `PriceListId`, `Name`, `ReferralId?`, `IsDefault` | `Referral 1-* PriceLists` | `Entities.cs:379-387`, `OpenLabDbContext.cs:331-337` |
| `PriceListItems` | `PriceListItemId`, `PriceListId`, `TestId`, `Price` | `PriceList 1-* Items`, `Test 1-* PriceListItems` | `Entities.cs:390-398`, `OpenLabDbContext.cs:340-349` |
| `CustomGroups` | `CustomGroupId`, `Name`, `Price` | `CustomGroup 1-* CustomGroupItems` | `Entities.cs:401-407`, `OpenLabDbContext.cs:352-356` |
| `CustomGroupItems` | `CustomGroupItemId`, `CustomGroupId`, `TestId` | `CustomGroup 1-* Items`, `Test 1-* CustomGroupItems` | `Entities.cs:410-417`, `OpenLabDbContext.cs:359-367` |

## 3. جدول تقييم القدرة

| السؤال | الحالة | الشرح والمرجع |
|---|---|---|
| هل يمكن تخزين نوع تحليل بسيط مكوّن واحد؟ | ✅ مدعوم | `Tests` يحتوي اسم، كود، وحدة، سعر، وصف. `UnitId` مخصص للاختبارات البسيطة حسب تعليق الكود في `Entities.cs:183-186`. |
| هل يمكن تخزين نوع تحليل مركّب 20+ مكوّن فرعي؟ | ✅ مدعوم بنيوياً | `Test` يحتوي `ICollection<TestParameter>` في `Entities.cs:207`، وكل `TestParameter` له `TestId`, `Name`, `UnitId`, `OrderNo` في `315-325`. لا يوجد حد عددي في النموذج. |
| هل لكل مكوّن فرعي وحدة قياس مستقلة؟ | ✅ مدعوم | `TestParameter.UnitId` موجود في `Entities.cs:320` ومربوط بـ `Units` في `OpenLabDbContext.cs:275-277`. |
| هل يمكن تخزين معدلات طبيعية متعددة لنفس المكوّن؟ | ✅ مدعوم | `TestReferenceRanges` جدول مستقل، ويرتبط اختيارياً بـ `ParameterId` في `Entities.cs:243-246`، ويمكن إنشاء عدة صفوف لنفس `TestId/ParameterId` لأنه لا يوجد Unique constraint يمنع ذلك في `OpenLabDbContext.cs:223-239`. |
| هل المعدلات الطبيعية مرتبطة بجنس المريض؟ | ✅ مدعوم | `TestReferenceRange.Gender` موجود في `Entities.cs:248` ويستخدم في التحقق في `ResultsService.cs:299`. |
| هل المعدلات الطبيعية مرتبطة بالفئة العمرية؟ | ✅ مدعوم جزئياً | توجد حدود عمرية رقمية ووحدات وأيام: `AgeFromValue/Unit/Days`, `AgeToValue/Unit/Days` في `Entities.cs:254-259`، ويستخدمها التحقق في `ResultsService.cs:300-301`. لكنها ليست مربوطة بجدول فئات عمرية مسمّاة reusable. |
| هل يمكن تعريف فئات عمرية مرنة ليست ثابتة؟ | ⚠️ مدعوم جزئياً | يمكن إدخال نطاقات مرنة لكل معدل عبر value/unit/days في `Entities.cs:250-259`، لكن لا يوجد جدول `AgeGroups` حالي في `DbSet` ضمن `OpenLabDbContext.cs:21-66`، ولا توجد علاقة FK لفئة عمرية مسماة. |
| هل يمكن تخزين سعر كل نوع تحليل؟ | ✅ مدعوم | `Test.Price` موجود في `Entities.cs:187` بدقة 18,2 في `OpenLabDbContext.cs:191`. توجد أيضاً أسعار بديلة `PatientPrice`, `CostPrice` في `Entities.cs:188-189`. |
| هل يمكن تصنيف أنواع التحاليل في مجموعات؟ | ✅ مدعوم | `Test.GroupId` في `Entities.cs:180` مرتبط بـ `TestGroup` في `OpenLabDbContext.cs:194-196`. |
| هل يمكن إضافة أنواع تحاليل جديدة من واجهة المستخدم؟ | ⚠️ مدعوم جزئياً | `TestCatalogViewModel` ينشئ اختباراً عبر `CreateTestAsync` في `TestCatalogViewModel.cs:244-268`، والخدمة تدعم `CreateTestAsync` في `ITestCatalogService.cs:11` و`TestCatalogService.cs:35-51`. لكن `App.xaml` الحالي لا يحتوي `DataTemplate` لـ `TestCatalogViewModel` في القوالب الحالية `App.xaml:34-94`، ولا توجد ملفات View مباشرة للكتالوج ضمن `Open_lab/Views` عند الحصر الحالي. |
| هل يمكن تعديل بيانات أنواع التحاليل الموجودة؟ | ⚠️ مدعوم جزئياً | `TestCatalogViewModel` يعدّل حقول الاختبار الأساسية في `TestCatalogViewModel.cs:274-289`، والخدمة تدعم `UpdateTestAsync` في `ITestCatalogService.cs:12` و`TestCatalogService.cs:56-83`. لكن لا توجد واجهة فعلية مربوطة حالياً في `App.xaml`، ولا يدير هذا الـ ViewModel المكوّنات أو المعدلات. |
| هل يمكن إضافة مكوّنات فرعية جديدة من واجهة المستخدم؟ | ❌ غير مدعوم حالياً من الواجهة | الخدمة تحتوي `CreateParameterAsync` في `ITestCatalogService.cs:24` و`TestCatalogService.cs:189-215`، لكن البحث الحالي لا يظهر أي ViewModel يستخدمها؛ تظهر فقط في الخدمة و`ReferenceRangesViewModel` يقرأ المكوّنات عبر `GetParametersByTestAsync` في `ReferenceRangesViewModel.cs:164`. |
| هل يمكن تعديل المعدلات الطبيعية لأي مكوّن؟ | ⚠️ مدعوم جزئياً | `ReferenceRangesViewModel` ينشئ/يحدث/يحذف المعدلات ويربطها بـ `SelectedParameter` في `220-250` و`272-274`. لكن لا توجد `DataTemplate`/View حالية ظاهرة لـ `ReferenceRangesViewModel` في `App.xaml:34-94`. |

## 4. قائمة الفجوات والمشاكل المكتشفة

| الأولوية | الفجوة | الجدول/العمود | الدليل | الأثر العملي |
|---|---|---|---|---|
| P1 | لا توجد واجهة فعلية حالية لإدارة كتالوج التحاليل رغم وجود ViewModels. | ليست فجوة جدول، بل ربط UI | `ViewModelFactory` ينشئ `TestCatalogViewModel` و`ReferenceRangesViewModel` في `ViewModelFactory.cs:32-33`، لكن `App.xaml` لا يحتوي DataTemplate لهما ضمن `34-94`، وملفات Views الحالية لا تحتوي `TestCatalogView.xaml` أو `ReferenceRangesView.xaml`. | المستخدم لا يستطيع الوصول لشاشة حقيقية لإضافة/تعديل التحاليل والمعدلات عبر UI الحالي، حتى لو كانت الخدمات موجودة. |
| P1 | لا توجد واجهة لإضافة/تعديل/حذف مكوّنات التحليل `TestParameter`. | `TestParameters.Name`, `UnitId`, `OrderNo` | الخدمة تدعم الإنشاء فقط في `ITestCatalogService.cs:24` و`TestCatalogService.cs:189-215`، والبحث الحالي لا يظهر استخدام `CreateParameterAsync` في ViewModels أو Views. | لا يمكن للمستخدم بناء CBC أو أي profile من 20+ مكوّن من داخل الواجهة الحالية. |
| P1 | لا توجد دوال خدمة لتعديل/حذف `TestParameter`. | `TestParameters` | `ITestCatalogService.cs:24-25` يحتوي فقط `CreateParameterAsync` و`GetParametersByTestAsync` ولا يحتوي `UpdateParameterAsync` أو `DeleteParameterAsync`. | حتى لو أُضيفت شاشة، لن يمكن تعديل اسم/وحدة/ترتيب مكوّن أو حذفه دون إضافة API جديد. |
| P2 | الفئات العمرية ليست كيانات مستقلة قابلة لإعادة الاستخدام. | `TestReferenceRanges.AgeFromValue/AgeFromUnit/AgeFromDays/AgeToValue/AgeToUnit/AgeToDays` | الأعمار مخزنة مباشرة على كل range في `Entities.cs:254-259`، ولا يوجد `DbSet<AgeGroup>` في `OpenLabDbContext.cs:21-66`. | تكرار نفس فئات العمر داخل كل مكوّن؛ صعوبة توحيد تعريف "طفل/بالغ/كبير سن" عبر النظام. |
| P2 | `ParameterId` في `TestReferenceRanges` اختياري، ما يسمح بمعدل عام غير مربوط بمكوّن في test مركب. | `TestReferenceRanges.ParameterId` | `int? ParameterId` في `Entities.cs:243-246`، والعلاقة اختيارية في `OpenLabDbContext.cs:233-237`. | في التحاليل المركبة قد تُسجّل معدلات عامة على test بدلاً من المكوّن، فينشأ التباس بين Hemoglobin وWBC مثلاً. |
| P2 | لا يوجد constraint يمنع تكرار مكوّن بنفس الاسم داخل نفس التحليل على مستوى قاعدة البيانات. | `TestParameters.TestId`, `Name` | الخدمة تمنع duplicate برمجياً في `TestCatalogService.cs:209-212`، لكن `OpenLabDbContext.cs:268-277` لا يعرّف unique index على `(TestId, Name)`. | بيانات غير متسقة إذا تم الإدخال من مسار آخر غير الخدمة أو migration/manual SQL. |
| P2 | لا يوجد constraint يمنع تكرار أسعار نفس الاختبار داخل نفس قائمة السعر. | `PriceListItems.PriceListId`, `TestId` | العلاقة والفهارس الأساسية فقط في `OpenLabDbContext.cs:340-349`، ولا يظهر unique index. الخدمة تفحص وجود السعر في `TestCatalogService.cs:389-414`. | احتمال وجود أكثر من سعر لنفس test في نفس price list إذا تم تجاوز الخدمة. |
| P3 | `Test.UnitId` و`TestParameter.UnitId` موجودان معاً وقد يسببان لبساً وظيفياً. | `Tests.UnitId`, `TestParameters.UnitId` | تعليق الكود يوضح أن `Tests.UnitId` للاختبارات البسيطة فقط وأن المركبة تستخدم `TestParameter.UnitId` في `Entities.cs:183-186`. | يحتاج UI/validation واضح حتى لا يضع المستخدم وحدة عامة لا تُستخدم في profile مركب. |

## 5. التصميم المقترح للإصلاح

### أولوية 1: تمكين إدارة المكوّنات

إضافة شاشة/ViewModel/API لإدارة `TestParameters`:

| الجدول الحالي | التعديل المقترح |
|---|---|
| `TestParameters` | الإبقاء على الأعمدة الحالية: `ParameterId`, `TestId`, `Name`, `UnitId`, `OrderNo`. |
| `ITestCatalogService` | إضافة `UpdateParameterAsync(TestParameter parameter)` و`DeleteParameterAsync(int parameterId)`. |
| `OpenLabDbContext` | إضافة Unique Index مقترح: `(TestId, Name)` ويفضل `(TestId, OrderNo)` إذا كان الترتيب يجب أن يكون فريداً. |
| UI | إضافة View حقيقي لإدارة مكوّنات التحليل المختار: إضافة/تعديل/حذف/ترتيب/وحدة. |

سبب التعديل: الجداول الحالية تكفي بنيوياً للـ profiles، لكن لا توجد واجهة ولا API مكتمل للتعديل والحذف.

### أولوية 2: ربط ViewModels الحالية بواجهات فعلية

إضافة DataTemplates وViews للـ ViewModels الحالية:

| ViewModel | المطلوب |
|---|---|
| `TestCatalogViewModel` | View لإدارة `Tests` الأساسية. |
| `ReferenceRangesViewModel` | View لإدارة `TestReferenceRanges` حسب `TestParameter`. |
| `TestCommentsViewModel` | View لإدارة التعليقات حسب test/parameter. |
| `PriceListsViewModel` | View لإدارة قوائم الأسعار. |
| `CustomGroupsViewModel` | View لإدارة مجموعات التحاليل المخصصة. |

سبب التعديل: `ViewModelFactory.cs:32-36` ينشئ هذه ViewModels، لكن `App.xaml:34-94` لا يربطها بـ Views حالية.

### أولوية 3: ضبط القيود على مستوى قاعدة البيانات

| الجدول | القيد المقترح | السبب |
|---|---|---|
| `TestParameters` | Unique `(TestId, Name)` | منع تكرار نفس المكوّن داخل نفس التحليل. |
| `PriceListItems` | Unique `(PriceListId, TestId)` | منع تعدد أسعار نفس التحليل داخل نفس القائمة. |
| `CustomGroupItems` | Unique `(CustomGroupId, TestId)` | منع تكرار نفس التحليل داخل نفس المجموعة. |
| `TestReferenceRanges` | Index مركب `(TestId, ParameterId, Gender, AgeFromDays, AgeToDays)` | تحسين البحث ومنع/كشف التعارضات في معدلات نفس المكوّن. |

### أولوية 4: قرار معماري حول الفئات العمرية

الخيار الحالي يخزن العمر مباشرة داخل `TestReferenceRanges`، وهو مرن لكنه يكرر التعريفات. إذا كان المطلوب تعريف فئات عمرية reusable باسم واضح، أضف:

| جدول مقترح | الأعمدة | العلاقة |
|---|---|---|
| `AgeGroups` | `AgeGroupId`, `Name`, `AgeFromValue`, `AgeFromUnit`, `AgeFromDays`, `AgeToValue`, `AgeToUnit`, `AgeToDays`, `SortOrder` | `AgeGroups 1-* TestReferenceRanges` عبر `AgeGroupId?` |

سبب التعديل: يتيح تعريف "رضيع/طفل/بالغ/60+" مرة واحدة واستخدامه عبر كل المكوّنات، مع بقاء حقول العمر المباشرة كخيار override إذا أراد النظام مرونة كاملة.

### أولوية 5: قاعدة تمييز الاختبار البسيط والمركب

لا يوجد حالياً عمود `TestType` أو `IsProfile`. يمكن استنتاج المركب من وجود `TestParameters`, لكن الاستنتاج غير صريح. المقترح:

| الجدول | العمود المقترح | السبب |
|---|---|---|
| `Tests` | `TestKind` أو `IsProfile` | يجعل UI والـ validation واضحين: الاختبار البسيط يحتاج مكوّناً واحداً أو `UnitId`; المركب يحتاج مكوّنات متعددة في `TestParameters`. |

هذا الاقتراح لا ينفذ الآن؛ هو فقط خطة مراجعة مبنية على الفجوات أعلاه.
