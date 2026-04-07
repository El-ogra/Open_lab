Executive Summary
  هذا المستند يعيد هندسة النظام المرجعي لإدارة معامل التحاليل الطبية بالكامل ويحوّله إلى تصميم حديث لنظام جديد باسم Open_lab مبني على SQL Server + WPF + .NET 8 + MVVM مع استبعاد التكامل مع الأجهزة الطبية
  واستلام النتائج عبر الإنترنت. التحليل استخلص الوحدات والكيانات وسير العمل والشاشات، ثم حوّلها إلى نموذج بيانات مُطبّع وتصميم معماري واضح قابل للتنفيذ.

  ———

  Modules Breakdown

  - إدارة المرضى والزيارات
  - إدارة التحاليل والكتالوج المرجعي
  - إدخال النتائج والتحقق والطباعة
  - التقارير (فردي، مجمّع، فارغ، تاريخ مرضي)
  - ورقة العمل (Work Sheet)
  - الحسابات والفواتير والتحصيل
  - الجهات المحولة/المتعاقدة (أطباء، شركات، معامل)
  - مجموعات التحاليل (Packages/Custom Groups)
  - المزارع والمضادات الحيوية (Culture & Sensitivity)
  - الإحصائيات
  - إدارة المستخدمين والصلاحيات
  - إعدادات النظام والتقارير والطابعات
  - النسخ الاحتياطي وصيانة البيانات
  - سحب العينات وتتبع حالتها

  ———

  User Roles & Permissions
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

  UI Screens Catalog

  - Screen: Login
  - الوصف: تسجيل الدخول واختيار المستخدم
  - العناصر: اسم مستخدم، كلمة مرور، تذكرني
  - العمليات: دخول، خروج
  - Screen: Main Dashboard
  - الوصف: القائمة الرئيسية للوحدات
  - العناصر: أزرار وحدات (Patients, Laboratory, Work Sheet, Statistics, Accounts, Users, System, Settings, Tools)
  - العمليات: تنقل للوحدات
  - Screen: Patient Registration
  - الوصف: إضافة/تعديل بيانات مريض
  - العناصر: بيانات شخصية، رقم Lab ID، جهة الإحالة، نوع الحساب، هاتف، العمر/النوع
  - العمليات: حفظ، تعديل، حذف (حسب الصلاحية)
  - Screen: Patient Tests Selection
  - الوصف: اختيار التحاليل للمريض
  - العناصر: قائمة تحاليل، مجموعات جاهزة، بحث، إجمالي السعر
  - العمليات: إضافة/حذف تحليل، إضافة مجموعة، حفظ الطلب
  - Screen: Patient Billing / Account
  - الوصف: حساب المريض والمدفوعات
  - العناصر: إجمالي التحاليل، خصم، المدفوع، المتبقي
  - العمليات: تعديل مدفوع، حذف عملية دفع، تسوية الحساب
  - Screen: Barcode/Receipt Printing
  - الوصف: طباعة الباركود والإيصال
  - العناصر: اختيار نوع الطباعة، رقم الزيارة
  - العمليات: طباعة باركود/إيصال
  - Screen: Results Entry
  - الوصف: إدخال نتائج التحاليل
  - العناصر: قائمة المرضى اليوم، تفاصيل التحليل، القيم، الوحدات، النطاق المرجعي
  - العمليات: حفظ نتيجة، مراجعة، اعتماد
  - Screen: Report Viewer/Print
  - الوصف: عرض وطباعة التقرير
  - العناصر: بيانات المريض، النتائج، التعليقات، توقيع الطبيب
  - العمليات: طباعة، إعادة طباعة
  - Screen: Combined Report
  - الوصف: تقرير مجمّع لأكثر من تحليل
  - العناصر: قائمة التحاليل المختارة، أسهم ترتيب
  - العمليات: إعادة ترتيب، طباعة
  - Screen: Blank Report
  - الوصف: تقرير بيانات مريض فقط
  - العناصر: بيانات المريض
  - العمليات: طباعة
  - Screen: Culture & Sensitivity
  - الوصف: إدخال نتائج المزرعة والمضادات
  - العناصر: اسم المزرعة، قائمة المضادات، حساسية
  - العمليات: إضافة/تعديل، حفظ
  - Screen: Patient Search
  - الوصف: البحث عن مريض وزياراته
  - العناصر: بحث بالاسم/الجهة/الفترة، قائمة زيارات
  - العمليات: فتح زيارة، عرض نتائج، حسابات
  - Screen: Patient History
  - الوصف: تاريخ مرضي وتحاليل سابقة
  - العناصر: قائمة زيارات، فترة تاريخية
  - العمليات: طباعة تاريخ مرضي منفصل/مدمج
  - Screen: Work Sheet (By Patient)
  - الوصف: ورقة عمل بأسماء المرضى
  - العناصر: فترة زمنية، قائمة مرضى
  - العمليات: طباعة
  - Screen: Work Sheet (By Test)
  - الوصف: ورقة عمل بالتحاليل
  - العناصر: فترة زمنية، قائمة تحاليل
  - العمليات: طباعة
  - Screen: Test Catalog
  - الوصف: تعريف التحاليل
  - العناصر: اسم التحليل في التقرير/الإيصال، الوحدة، المجموعة، العينة، الوقت، السعر، باركود
  - العمليات: إضافة/تعديل/حذف
  - Screen: Reference Ranges
  - الوصف: القيم المرجعية حسب العمر/النوع
  - العناصر: مدى طبيعي، أعلام Low/High
  - العمليات: إضافة/تعديل/حذف
  - Screen: Test Comments
  - الوصف: تعليقات ثابتة مرتبطة بالتحليل
  - العناصر: قائمة تعليقات
  - العمليات: إضافة/تعديل/حذف، اختيار تعليق للتقرير
  - Screen: Price Lists
  - الوصف: قوائم الأسعار العامة أو للجهات
  - العناصر: قائمة تحاليل وأسعار
  - العمليات: إنشاء قائمة جديدة، طباعة قائمة
  - Screen: Custom Groups
  - الوصف: إنشاء مجموعات تحاليل بسعر محدد
  - العناصر: اسم المجموعة، تحاليل، سعر
  - العمليات: إضافة/تعديل/حذف، إضافة مجموعة لمريض
  - Screen: Referrals & External Entities
  - الوصف: أطباء، شركات، معامل
  - العناصر: بيانات الجهة، نوع الجهة، أسعار خاصة
  - العمليات: إضافة/تعديل/حذف
  - Screen: Users & Permissions
  - الوصف: إدارة المستخدمين وصلاحياتهم
  - العناصر: قائمة مستخدمين، صلاحيات
  - العمليات: إضافة/تعديل/حذف، حفظ
  - Screen: Attendance Log
  - الوصف: حضور/انصراف المستخدمين
  - العناصر: قائمة الدخول والخروج
  - العمليات: عرض تقارير
  - Screen: Statistics
  - الوصف: إحصائيات حسب الفترة/الجنس/الجهة
  - العناصر: فلاتر تاريخية، تقارير
  - العمليات: عرض/طباعة
  - Screen: Accounts / Treasury
  - الوصف: جرد وحسابات المعمل
  - العناصر: إجماليات فترة، حساب مستخدمين، حساب جهات
  - العمليات: جرد يومي/أسبوعي/شهري، طباعة
  - Screen: System Settings
  - الوصف: إعدادات التقارير والطابعات والإيصال
  - العناصر: هوامش، رأس/ذيل، ألوان، طابعة افتراضية
  - العمليات: حفظ إعدادات
  - Screen: Backup & Restore
  - الوصف: نسخ احتياطي واسترجاع
  - العناصر: مسار النسخ، قائمة النسخ
  - العمليات: إنشاء نسخة، استرجاع
  - Screen: Sample Collection
  - الوصف: تتبع سحب العينات
  - العناصر: قائمة مرضى اليوم، حالة العينة
  - العمليات: تعليم مسحوبة/غير مسحوبة

  ———

  Workflow Scenarios

  1. تسجيل مريض جديد
  2. فتح شاشة المرضى، إدخال البيانات الأساسية
  3. توليد Lab ID وطباعة باركود
  4. اختيار التحاليل أو مجموعة تحاليل
  5. احتساب التكلفة، خصم/مدفوع، إصدار إيصال
  6. إدخال نتائج وتحويلها لتقرير
  7. اختيار المريض من قائمة اليوم
  8. إدخال نتائج التحاليل وربطها بالنطاق المرجعي
  9. مراجعة النتائج واعتمادها
  10. طباعة التقرير وتسليم النتائج
  11. تقرير مجمّع
  12. اختيار عدة تحاليل لنفس الزيارة
  13. ترتيبها يدوياً
  14. طباعة تقرير موحّد
  15. تقرير تاريخ مرضي
  16. فتح تاريخ المريض
  17. تحديد الفترة
  18. طباعة تاريخ مرضي منفصل أو دمجه
  19. ورقة عمل
  20. اختيار نوع ورقة العمل (مرضى أو تحاليل)
  21. تحديد الفترة
  22. طباعة ورقة العمل
  23. جرد وحساب المعمل
  24. تحديد الفترة
  25. عرض الإجماليات حسب المستخدم/الجهة
  26. طباعة تقرير الجرد

  ———

  Database Design

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

  العلاقات (وصف ERD)

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

  ———

  Normalization Explanation

  - 1NF: كل جدول يحتوي أعمدة ذرّية دون مجموعات متكررة.
  - 2NF: كل الحقول غير المفتاحية تعتمد كليًا على المفتاح الأساسي (مثلاً نتائج التحاليل مرتبطة بـ VisitTestId وليس بزيارة فقط).
  - 3NF: لا توجد تبعيات انتقالية؛ بيانات الجهات منفصلة عن الزيارات، الأسعار في جداول مستقلة، وإعدادات النظام في جدول مستقل.

  ———

  Project Architecture (WPF + MVVM)

  - Open_lab.UI: Views, Windows, XAML Resources
  - Open_lab.ViewModels: ViewModels, Commands, Navigation
  - Open_lab.Models: Entities, DTOs
  - Open_lab.Data: DbContext, EF Core Repositories
  - Open_lab.Services: Business Logic
  - Open_lab.Infrastructure (اختياري): Logging, Configuration, Helpers

  توزيع المسؤوليات:

  - UI: عرض وربط بيانات فقط
  - ViewModels: حالة الشاشة + أوامر + تحقق
  - Services: منطق الأعمال
  - Data: الوصول لقاعدة البيانات

  ———

  ViewModels Design

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

  ———

  Implementation Roadmap

  1. المرحلة 1 (Core)
  2. قاعدة البيانات الأساسية + EF Core
  3. تسجيل المرضى + اختيار التحاليل + الحسابات
  4. إدخال النتائج + طباعة التقرير
  5. المرحلة 2
  6. كتالوج التحاليل والقيم المرجعية
  7. مجموعات التحاليل وقوائم الأسعار
  8. ورقة العمل + تاريخ مرضي
  9. المرحلة 3
  10. الإحصائيات والحسابات المتقدمة
  11. إعدادات التقارير والطابعات
  12. النسخ الاحتياطي وصيانة البيانات

  ———

  Assumptions & Ambiguities

  - بعض الصفحات كانت شعارات فقط أو نصها ضعيف بسبب طبيعة الصور؛ تم التعامل معها كصفحات بلا محتوى وظيفي.
  - بعض أجزاء OCR قد تحتوي أخطاء بسبب جودة الصور، وتم الاعتماد على الاستنتاج المنطقي الهندسي.
  - تم استبعاد التكامل مع الأجهزة الطبية واستلام النتائج عبر الإنترنت كما هو مطلوب.
  - أجزاء خاصة بمنصة نتائج خارجية تم تجاهلها بالكامل في التصميم.

  ———

  إذا أردت، أبدأ بتحويل هذا التصميم إلى مشروع فعلي بـ .NET 8 + WPF مع هيكل الحل والموديلات وDbContext مبدئيًا.