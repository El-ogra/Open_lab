# Open_lab — الوثيقة التوثيقية الموحدة للمشروع

> هذا الملف هو المرجع الوحيد والشامل لمشروع Open_lab.
> تم توحيده من ثلاثة ملفات توثيقية سابقة (Open_lab_Plan.md, Open_lab_Master_Plan.md, New-Item Open_lab_Architecture.md)
> بتاريخ: 16 أبريل 2026

———

## 1. نظرة عامة على النظام (System Overview)

- النظام هو منصة مكتبية لإدارة معامل التحاليل الطبية باسم Open_lab، تعمل على SQL Server + WPF + .NET 8 مع التزام صارم بـ MVVM.
- الهدف العام: إدارة دورة العمل الكاملة داخل المعمل من تسجيل المرضى والطلبات، مرورًا بإدخال النتائج والتحقق والطباعة، وصولًا للحسابات والإحصائيات والإعدادات.
- نطاق المشروع: يشمل إدارة المرضى والزيارات والتحاليل والنتائج والتقارير والمحاسبة والإحصائيات والإعدادات، مع استبعاد التكامل مع أجهزة المختبر واستلام النتائج عبر الإنترنت.

———

## 2. الوحدات الوظيفية (Modules Breakdown)

1. إدارة المرضى والزيارات: تسجيل بيانات المرضى، توليد Lab ID، إدارة الزيارات وربطها بالتحاليل.
2. إدارة التحاليل والكتالوج المرجعي: تعريف التحاليل، الوحدات، العينات، الأسعار، الوقت المرجعي.
3. إدخال النتائج والتحقق والطباعة: إدخال القيم، مراجعتها، اعتمادها، وطباعة التقارير.
4. التقارير: تقرير فردي، تقرير مجمّع، تقرير فارغ، تاريخ مرضي منفصل.
5. ورقة العمل (Work Sheet): طباعة ورقة عمل حسب المرضى أو حسب التحاليل خلال فترة زمنية.
6. الحسابات والفواتير والتحصيل: حساب التكلفة، الخصومات، المدفوع، والمتبقي.
7. الجهات المحولة/المتعاقدة: أطباء، شركات، ومعامل خارجية وإدارتها.
8. مجموعات التحاليل (Custom Groups): إنشاء مجموعات تحاليل بأسعار محددة وإضافتها للمرضى.
9. المزارع والمضادات الحيوية (Culture & Sensitivity): إدارة المزارع وربطها بالمضادات.
10. الإحصائيات: تقارير إحصائية حسب الفترة، الجنس، الجهة.
11. إدارة المستخدمين والصلاحيات: إنشاء المستخدمين وتحديد صلاحياتهم وإدارة الدخول والخروج.
12. إعدادات النظام والتقارير والطابعات: ضبط الهوامش والألوان والطابعات والإيصال والظرف.
13. النسخ الاحتياطي وصيانة البيانات: إنشاء نسخ احتياطية واسترجاعها.
14. سحب العينات وتتبع حالتها: تأكيد سحب العينة ومتابعتها.

———

## 3. أدوار المستخدمين (User Roles & Permissions)

| الدور | الصلاحيات الرئيسية |
|---|---|
| مدير النظام | إدارة المستخدمين والصلاحيات، إعدادات النظام، النسخ الاحتياطي، كل التقارير والإحصائيات |
| موظف استقبال | تسجيل مرضى، تعديل بيانات، إصدار إيصال/باركود، اختيار التحاليل |
| فني مختبر | إدخال نتائج، مراجعة/اعتماد نتائج، طباعة تقارير |
| مشرف تقارير | إعداد تقارير مجمّعة/فارغة، تاريخ مرضي، طباعة |
| محاسب/خزينة | تحصيل، خصومات، تصفية حسابات، جرد يومي/أسبوعي/شهري |
| مسؤول إحصائيات | تقارير إحصائية حسب الفترة/الجنس/جهة الإحالة |
| مسؤول سحب عينات | تأكيد سحب العينات وتتبعها |

———

## 4. معمارية المشروع (Project Architecture — MVVM)

### هيكل المشروع

- Open_lab.UI: Views, Windows, XAML Resources
- Open_lab.ViewModels: ViewModels, Commands, Navigation
- Open_lab.Models: Entities, DTOs
- Open_lab.Data: DbContext, EF Core Repositories
- Open_lab.Services: Business Logic
- Open_lab.Infrastructure (اختياري): Logging, Configuration, Helpers

### توزيع المسؤوليات

- Models: الكيانات وDTOs
- Views: شاشات WPF — عرض وربط بيانات فقط عبر Binding
- ViewModels: حالة الشاشة، أوامر (Commands)، تحقق (Validation)
- Services: منطق الأعمال — لا يوجد منطق أعمال داخل ViewModels
- Data: DbContext وRepositories — تعزل التخزين عن واجهة المستخدم

### قواعد MVVM الصارمة

1. Views تعرض البيانات فقط عبر Binding — لا يوجد Code-behind منطقي
2. ViewModels تحتوي Properties وCommands وValidation
3. Models تمثل البيانات فقط
4. Services تنفذ منطق الأعمال — ممنوع وضع منطق أعمال داخل ViewModels
5. BaseViewModel مع INotifyPropertyChanged مطلوب
6. RelayCommand كـ base class مطلوب
7. NavigationService للتبديل بين الشاشات مطلوب

———

# خارطة الطريق التنفيذية (Execution Roadmap)

> سأعتمد فقط على المخرجات المعتمدة ولن أضيف وظائف جديدة خارج النطاق المعتمد.

———

## المرحلة 0: التحضير والتخطيط

- الهدف: تثبيت الأساسيات وتجهيز بيئة العمل وبنية المستودع.
- خطوات التنفيذ:
  - إنشاء Solution بالمجلدات التالية: Views, ViewModels, Models, Data, Services
  - تثبيت .NET 8 وتهيئة WPF.
  - تجهيز SQL Server وإنشاء قاعدة بيانات فارغة.
  - إعداد Git وملفات الإعداد الأساسية.

### نقاط التحقق

- بناء الحل بدون أخطاء.
- تشغيل تطبيق WPF يعرض نافذة فارغة.
- وجود هيكل المشاريع كما هو محدد.

———

## المرحلة 1: نمذجة البيانات وقاعدة البيانات

- الهدف: بناء نموذج البيانات بالكامل طبقًا للجداول المذكورة.
- خطوات التنفيذ:
  - إنشاء جميع الكيانات في Open_lab.Models وفق الجداول المحددة أدناه.
  - إعداد DbContext في Open_lab.Data.
  - إضافة العلاقات والمفاتيح الأساسية والأجنبية.
  - إنشاء Migration أولي وتطبيقه على SQL Server.

### تصميم قاعدة البيانات (Database Schema)

- Users
    - UserId (PK, int, identity)
    - Username (nvarchar, unique, not null)
    - PasswordHash (nvarchar, not null)
    - FullName (nvarchar)
    - IsActive (bit)
- Roles
    - RoleId (PK)
    - RoleName (nvarchar, unique)
- RolePermissions
    - RoleId (FK)
    - PermissionCode (nvarchar)
- UserRoles
    - UserId (FK)
    - RoleId (FK)
- Patients
    - PatientId (PK)
    - LabId (nvarchar, unique)
    - FullName (nvarchar)
    - Gender (char(1))
    - BirthDate (date, nullable)
    - Phone (nvarchar)
    - Address (nvarchar)
- Visits
    - VisitId (PK)
    - PatientId (FK)
    - VisitDate (datetime2)
    - AccountType (nvarchar)
    - ReferralId (FK, nullable)
    - Status (nvarchar)
- Referrals
    - ReferralId (PK)
    - ReferralType (nvarchar)
    - Name (nvarchar)
    - Phone (nvarchar)
    - City (nvarchar)
- Tests
    - TestId (PK)
    - Code (nvarchar, unique)
    - NameReport (nvarchar)
    - NameReceipt (nvarchar)
    - GroupId (FK)
    - SampleTypeId (FK)
    - UnitId (FK)
    - Price (decimal)
    - TurnaroundHours (int)
    - IsRoutine (bit)
    - IsSendOut (bit)
- TestGroups
    - GroupId (PK)
    - GroupName (nvarchar)
- SampleTypes
    - SampleTypeId (PK)
    - Name (nvarchar)
- Units
    - UnitId (PK)
    - Name (nvarchar)
- TestReferenceRanges
    - RangeId (PK)
    - TestId (FK)
    - Gender (char(1), nullable)
    - AgeFrom (int)
    - AgeTo (int)
    - LowValue (decimal, nullable)
    - HighValue (decimal, nullable)
    - NormalText (nvarchar)
- TestComments
    - CommentId (PK)
    - TestId (FK)
    - CommentText (nvarchar)
    - IsDefault (bit)
- VisitTests
    - VisitTestId (PK)
    - VisitId (FK)
    - TestId (FK)
    - Price (decimal)
    - Status (nvarchar)
- TestParameters
    - ParameterId (PK)
    - TestId (FK)
    - Name (nvarchar)
    - UnitId (FK)
    - OrderNo (int)
- ResultValues
    - ResultValueId (PK)
    - VisitTestId (FK)
    - ParameterId (FK)
    - Value (nvarchar)
    - Flag (nvarchar)
    - Comment (nvarchar)
    - VerifiedBy (FK Users)
    - VerifiedAt (datetime2)
- Invoices
    - InvoiceId (PK)
    - VisitId (FK)
    - Total (decimal)
    - Discount (decimal)
    - NetTotal (decimal)
    - Paid (decimal)
    - Balance (decimal)
    - Status (nvarchar)
- Payments
    - PaymentId (PK)
    - InvoiceId (FK)
    - Amount (decimal)
    - PaymentDate (datetime2)
    - UserId (FK)
- PriceLists
    - PriceListId (PK)
    - Name (nvarchar)
    - ReferralId (FK, nullable)
    - IsDefault (bit)
- PriceListItems
    - PriceListItemId (PK)
    - PriceListId (FK)
    - TestId (FK)
    - Price (decimal)
- CustomGroups
    - CustomGroupId (PK)
    - Name (nvarchar)
    - Price (decimal)
- CustomGroupItems
    - CustomGroupItemId (PK)
    - CustomGroupId (FK)
    - TestId (FK)
- Cultures
    - CultureId (PK)
    - Name (nvarchar)
- Antibiotics
    - AntibioticId (PK)
    - Name (nvarchar)
- CultureAntibiotics
    - CultureId (FK)
    - AntibioticId (FK)
- SampleCollections
    - SampleId (PK)
    - VisitTestId (FK)
    - CollectedBy (FK Users)
    - CollectedAt (datetime2)
    - Status (nvarchar)
- Settings
    - Key (PK, nvarchar)
    - Value (nvarchar)

### العلاقات بين الجداول (ERD نصيًا)

- Patient 1-N Visits
- Visit 1-N VisitTests
- Test 1-N TestParameters
- VisitTest 1-N ResultValues
- Visit 1-1 Invoice
- Invoice 1-N Payments
- Referral 1-N Visits
- TestGroup 1-N Tests
- SampleType 1-N Tests
- Unit 1-N Tests و TestParameters
- Test 1-N ReferenceRanges
- Test 1-N TestComments
- PriceList 1-N PriceListItems
- CustomGroup 1-N CustomGroupItems
- Culture N-N Antibiotic عبر CultureAntibiotics

### شرح التسوية (Normalization)

- 1NF: كل جدول يحتوي أعمدة ذرّية دون مجموعات متكررة.
- 2NF: كل الحقول غير المفتاحية تعتمد كليًا على المفتاح الأساسي (مثلاً نتائج التحاليل مرتبطة بـ VisitTestId وليس بزيارة فقط).
- 3NF: لا توجد تبعيات انتقالية؛ بيانات الجهات منفصلة عن الزيارات، الأسعار في جداول مستقلة، وإعدادات النظام في جدول مستقل.

### مطالبات للوكلاء البرمجيين

- "أنشئ الكيانات لكل الجداول في المخرجات السابقة مع العلاقات والمفاتيح."
- "ابنِ DbContext مع DbSet لكل جدول وطبّق Migration."

### نقاط التحقق

- إنشاء قاعدة البيانات بنجاح.
- تطابق الجداول والأعمدة مع التصميم السابق.
- العلاقات (FK) مضبوطة وتعمل.

———

## المرحلة 2: طبقة الخدمات (Business Logic)

- الهدف: تنفيذ منطق الأعمال بعيدًا عن الواجهة.
- خطوات التنفيذ:
  - بناء Services للعمليات الأساسية: المرضى، الزيارات، التحاليل، النتائج، التقارير، الحسابات.
  - تضمين التحقق الأساسي للبيانات والمنطق الحسابي للفواتير.
  - تعريف واجهات (Interfaces) للخدمات لاستخدامها في ViewModels.

### مطالبات للوكلاء البرمجيين

- "ابنِ Services لإدارة المرضى والزيارات والتحاليل وفق تصميم النظام."
- "أضف منطق الحساب للفواتير والخصومات والمدفوعات."

### نقاط التحقق

- جميع الخدمات تعمل عبر اختبارات بسيطة (CRUD).
- عدم وجود منطق أعمال داخل ViewModels.

———

## المرحلة 3: البنية الأساسية لـ MVVM والتنقل

- الهدف: تجهيز بنية MVVM والتصفح بين الشاشات.
- خطوات التنفيذ:
  - إنشاء BaseViewModel مع INotifyPropertyChanged.
  - إنشاء Command base (RelayCommand).
  - إنشاء Navigation Service بسيط للتبديل بين الشاشات.
  - ربط MainWindow بـ MainViewModel.

### تصميم الـ ViewModels

- LoginViewModel: Username, Password, LoginCommand, Validation
- MainViewModel: MenuItems, NavigateCommand, CurrentView
- PatientViewModel: Patient fields, Save/Update/Delete, Validate
- VisitTestsViewModel: AvailableTests, SelectedTests, Add/Remove, CalculateTotal
- InvoiceViewModel: Total, Discount, Paid, Balance, AddPayment
- ResultsEntryViewModel: SelectedPatient, TestItems, SaveResult, VerifyResult
- ReportViewModel: ReportData, PrintCommand, ReprintCommand
- WorkSheetViewModel: DateFrom/To, Type, PrintCommand
- TestCatalogViewModel: Test CRUD, ReferenceRanges CRUD
- CustomGroupsViewModel: Group CRUD, AddTestToGroup
- ReferralsViewModel: Referral CRUD
- UsersViewModel: User CRUD, Permissions
- StatisticsViewModel: Filters, GenerateReport
- SettingsViewModel: ReportLayout, PrinterSettings, Save
- BackupViewModel: BackupPath, CreateBackup, Restore

### مطالبات للوكلاء البرمجيين

- "أنشئ BaseViewModel وRelayCommand وNavigationService."
- "هيّئ MainViewModel لعرض شاشة افتراضية."

### نقاط التحقق

- تغيير الشاشة داخل التطبيق يعمل بدون أخطاء.
- لا يوجد Code-behind منطقي خارج MVVM.

———

## المرحلة 4: بناء الشاشات الأساسية (Core Screens)

- الهدف: الشاشات الجوهرية لتشغيل النظام.
- خطوات التنفيذ:

### Login
- الوصف: تسجيل الدخول واختيار المستخدم
- العناصر: اسم مستخدم، كلمة مرور، تذكرني
- العمليات: دخول، خروج

### Main Dashboard
- الوصف: القائمة الرئيسية للوحدات
- العناصر: أزرار وحدات (Patients, Laboratory, Work Sheet, Statistics, Accounts, Users, System, Settings, Tools)
- العمليات: تنقل للوحدات

### Patient Registration
- الوصف: إضافة/تعديل بيانات مريض
- العناصر: بيانات شخصية، رقم Lab ID، جهة الإحالة، نوع الحساب، هاتف، العمر/النوع
- العمليات: حفظ، تعديل، حذف (حسب الصلاحية)

### Patient Tests Selection
- الوصف: اختيار التحاليل للمريض
- العناصر: قائمة تحاليل، مجموعات جاهزة، بحث، إجمالي السعر
- العمليات: إضافة/حذف تحليل، إضافة مجموعة، حفظ الطلب

### Patient Billing / Account
- الوصف: حساب المريض والمدفوعات
- العناصر: إجمالي التحاليل، خصم، المدفوع، المتبقي
- العمليات: تعديل مدفوع، حذف عملية دفع، تسوية الحساب

### Barcode/Receipt Printing
- الوصف: طباعة الباركود والإيصال
- العناصر: اختيار نوع الطباعة، رقم الزيارة
- العمليات: طباعة باركود/إيصال

### Results Entry
- الوصف: إدخال نتائج التحاليل
- العناصر: قائمة المرضى اليوم، تفاصيل التحليل، القيم، الوحدات، النطاق المرجعي
- العمليات: حفظ نتيجة، مراجعة، اعتماد

### Report Viewer/Print
- الوصف: عرض وطباعة التقرير
- العناصر: بيانات المريض، النتائج، التعليقات، توقيع الطبيب
- العمليات: طباعة، إعادة طباعة

### Combined Report
- الوصف: تقرير مجمّع لأكثر من تحليل
- العناصر: قائمة التحاليل المختارة، أسهم ترتيب
- العمليات: إعادة ترتيب، طباعة

### Blank Report
- الوصف: تقرير بيانات مريض فقط
- العناصر: بيانات المريض
- العمليات: طباعة

### Culture & Sensitivity
- الوصف: إدخال نتائج المزرعة والمضادات
- العناصر: اسم المزرعة، قائمة المضادات، حساسية
- العمليات: إضافة/تعديل، حفظ

### مطالبات للوكلاء البرمجيين

- "أنشئ الشاشات الأساسية مع ViewModels وربطها بالخدمات."
- "ضمن أن عمليات إضافة مريض، اختيار تحليل، حساب الفاتورة تعمل من واجهة المستخدم."

### نقاط التحقق

- يمكن إضافة مريض من الواجهة وحفظه فعليًا في DB.
- يمكن اختيار تحاليل وربطها بزيارة.
- يمكن إدخال نتائج وحفظها وربطها بالزيارة.

———

## المرحلة 5: الشاشات المساندة

- الهدف: إضافة بقية الوحدات حسب المخرجات السابقة.
- خطوات التنفيذ:

### Patient Search
- الوصف: البحث عن مريض وزياراته
- العناصر: بحث بالاسم/الجهة/الفترة، قائمة زيارات
- العمليات: فتح زيارة، عرض نتائج، حسابات

### Patient History
- الوصف: تاريخ مرضي وتحاليل سابقة
- العناصر: قائمة زيارات، فترة تاريخية
- العمليات: طباعة تاريخ مرضي منفصل/مدمج

### Work Sheet (By Patient)
- الوصف: ورقة عمل بأسماء المرضى
- العناصر: فترة زمنية، قائمة مرضى
- العمليات: طباعة

### Work Sheet (By Test)
- الوصف: ورقة عمل بالتحاليل
- العناصر: فترة زمنية، قائمة تحاليل
- العمليات: طباعة

### Test Catalog
- الوصف: تعريف التحاليل
- العناصر: اسم التحليل في التقرير/الإيصال، الوحدة، المجموعة، العينة، الوقت، السعر، باركود
- العمليات: إضافة/تعديل/حذف

### Reference Ranges
- الوصف: القيم المرجعية حسب العمر/النوع
- العناصر: مدى طبيعي، أعلام Low/High
- العمليات: إضافة/تعديل/حذف

### Test Comments
- الوصف: تعليقات ثابتة مرتبطة بالتحليل
- العناصر: قائمة تعليقات
- العمليات: إضافة/تعديل/حذف، اختيار تعليق للتقرير

### Price Lists
- الوصف: قوائم الأسعار العامة أو للجهات
- العناصر: قائمة تحاليل وأسعار
- العمليات: إنشاء قائمة جديدة، طباعة قائمة

### Custom Groups
- الوصف: إنشاء مجموعات تحاليل بسعر محدد
- العناصر: اسم المجموعة، تحاليل، سعر
- العمليات: إضافة/تعديل/حذف، إضافة مجموعة لمريض

### Referrals & External Entities
- الوصف: أطباء، شركات، معامل
- العناصر: بيانات الجهة، نوع الجهة، أسعار خاصة
- العمليات: إضافة/تعديل/حذف

### مطالبات للوكلاء البرمجيين

- "أنشئ شاشات الكتالوج المرجعي (تحاليل، قيم مرجعية، تعليقات) وربطها بالخدمات."
- "أنشئ شاشات Work Sheet والتاريخ المرضي."

### نقاط التحقق

- CRUD كامل للتحاليل والقيم المرجعية.
- إمكانية طباعة/عرض Work Sheet والتاريخ المرضي.

———

## المرحلة 6: الإدارة والإحصائيات والإعدادات

- الهدف: إكمال الوظائف الإدارية.
- خطوات التنفيذ:

### Users & Permissions
- الوصف: إدارة المستخدمين وصلاحياتهم
- العناصر: قائمة مستخدمين، صلاحيات
- العمليات: إضافة/تعديل/حذف، حفظ

### Attendance Log
- الوصف: حضور/انصراف المستخدمين
- العناصر: قائمة الدخول والخروج
- العمليات: عرض تقارير

### Statistics
- الوصف: إحصائيات حسب الفترة/الجنس/الجهة
- العناصر: فلاتر تاريخية، تقارير
- العمليات: عرض/طباعة

### Accounts / Treasury
- الوصف: جرد وحسابات المعمل
- العناصر: إجماليات فترة، حساب مستخدمين، حساب جهات
- العمليات: جرد يومي/أسبوعي/شهري، طباعة

### System Settings
- الوصف: إعدادات التقارير والطابعات والإيصال
- العناصر: هوامش، رأس/ذيل، ألوان، طابعة افتراضية
- العمليات: حفظ إعدادات

### Backup & Restore
- الوصف: نسخ احتياطي واسترجاع
- العناصر: مسار النسخ، قائمة النسخ
- العمليات: إنشاء نسخة، استرجاع

### Sample Collection
- الوصف: تتبع سحب العينات
- العناصر: قائمة مرضى اليوم، حالة العينة
- العمليات: تعليم مسحوبة/غير مسحوبة

### مطالبات للوكلاء البرمجيين

- "أنشئ إدارة المستخدمين والصلاحيات وربطها بنظام الدخول."
- "أنشئ شاشات الإحصائيات والحسابات مع فلاتر زمنية."

### نقاط التحقق

- صلاحيات المستخدمين تمنع الوصول غير المصرح به.
- الإحصائيات تنتج بيانات صحيحة من DB.
- إعدادات التقرير والطباعة قابلة للحفظ والاسترجاع.

———

## المرحلة 7: التكامل النهائي والجودة

- الهدف: ربط كل المكونات وتحسين الاستقرار.
- خطوات التنفيذ:
  - توحيد التنقل بين جميع الشاشات.
  - معالجة الأخطاء والاستثناءات.
  - تحسين الأداء في الاستعلامات الثقيلة.
  - مراجعة UX.

### سيناريوهات العمل (Workflow Scenarios)

#### سيناريو 1: تسجيل مريض جديد
1. فتح شاشة المرضى
2. إدخال البيانات الأساسية
3. توليد Lab ID وطباعة باركود

#### سيناريو 2: طلب تحليل وإصدار إيصال
4. اختيار التحاليل أو مجموعة تحاليل
5. احتساب التكلفة، خصم/مدفوع، إصدار إيصال

#### سيناريو 3: إدخال نتائج وإصدار تقرير
6. اختيار المريض من قائمة اليوم
7. إدخال نتائج التحاليل وربطها بالنطاق المرجعي
8. مراجعة النتائج واعتمادها
9. طباعة التقرير وتسليم النتائج

#### سيناريو 4: تقرير مجمّع
10. اختيار عدة تحاليل لنفس الزيارة
11. ترتيبها يدوياً
12. طباعة تقرير موحّد

#### سيناريو 5: تقرير تاريخ مرضي
13. فتح تاريخ المريض
14. تحديد الفترة
15. طباعة تاريخ مرضي منفصل أو دمجه

#### سيناريو 6: ورقة عمل
16. اختيار نوع ورقة العمل (مرضى أو تحاليل)
17. تحديد الفترة
18. طباعة ورقة العمل

#### سيناريو 7: جرد وحساب المعمل
19. تحديد الفترة
20. عرض الإجماليات حسب المستخدم/الجهة
21. طباعة تقرير الجرد

### مطالبات للوكلاء البرمجيين

- "اربط كل الشاشات ضمن نظام تنقل واحد."
- "أضف معالجة أخطاء موحدة ورسائل واضحة للمستخدم."

### نقاط التحقق

- جميع الشاشات متاحة من Dashboard.
- لا توجد أخطاء غير معالجة أثناء الاستخدام.
- أداء مقبول في التقارير والإحصائيات.
- تنفيذ جميع سيناريوهات العمل أعلاه بنجاح.

———

## المرحلة 8: التسليم

- الهدف: تجهيز المشروع للاستخدام الفعلي.
- خطوات التنفيذ:
  - إعداد Seed Data بسيطة (اختياري).
  - توثيق خطوات التشغيل.
  - اختبار سيناريوهات العمل الأساسية.

### مطالبات للوكلاء البرمجيين

- "جهّز حزمة تشغيل أولية مع بيانات تجريبية."
- "وثّق طريقة الإعداد والتشغيل."

### نقاط التحقق

- تشغيل النظام من الصفر بدون تدخل يدوي إضافي.
- تنفيذ سيناريوهات: تسجيل مريض، طلب تحليل، إدخال نتيجة، إصدار تقرير.

———

## ملاحظات واستثناءات (Assumptions & Exclusions)

- تم استبعاد التكامل مع الأجهزة الطبية واستلام النتائج عبر الإنترنت.
- أجزاء خاصة بمنصة نتائج خارجية تم تجاهلها بالكامل في التصميم.
