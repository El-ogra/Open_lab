 # Open_lab System Architecture

  ## 1. Overview

  - النظام هو منصة مكتبية لإدارة معامل التحاليل الطبية باسم Open_lab، تعمل على SQL Server + WPF + .NET 8 مع التزام صارم بـ MVVM.
  - الهدف العام: إدارة دورة العمل الكاملة داخل المعمل من تسجيل المرضى والطلبات، مرورًا بإدخال النتائج والتحقق والطباعة، وصولًا للحسابات والإحصائيات والإعدادات.
  - نطاق المشروع: يشمل إدارة المرضى والزيارات والتحاليل والنتائج والتقارير والمحاسبة والإحصائيات والإعدادات، مع استبعاد التكامل مع أجهزة المختبر واستلام النتائج عبر الإنترنت.

  ———

  ## 2. Modules

  - إدارة المرضى والزيارات: تسجيل بيانات المرضى، توليد Lab ID، إدارة الزيارات وربطها بالتحاليل.
  - إدارة التحاليل والكتالوج المرجعي: تعريف التحاليل، الوحدات، العينات، الأسعار، الوقت المرجعي.
  - إدخال النتائج والتحقق والطباعة: إدخال القيم، مراجعتها، اعتمادها، وطباعة التقارير.
  - التقارير: تقرير فردي، تقرير مجمّع، تقرير فارغ، تاريخ مرضي منفصل.
  - ورقة العمل (Work Sheet): طباعة ورقة عمل حسب المرضى أو حسب التحاليل خلال فترة زمنية.
  - الحسابات والفواتير والتحصيل: حساب التكلفة، الخصومات، المدفوع، والمتبقي.
  - الجهات المحولة/المتعاقدة: أطباء، شركات، ومعامل خارجية وإدارتها.
  - مجموعات التحاليل (Custom Groups): إنشاء مجموعات تحاليل بأسعار محددة وإضافتها للمرضى.
  - المزارع والمضادات الحيوية (Culture & Sensitivity): إدارة المزارع وربطها بالمضادات.
  - الإحصائيات: تقارير إحصائية حسب الفترة، الجنس، الجهة.
  - إدارة المستخدمين والصلاحيات: إنشاء المستخدمين وتحديد صلاحياتهم وإدارة الدخول والخروج.
  - إعدادات النظام والتقارير والطابعات: ضبط الهوامش والألوان والطابعات والإيصال والظرف.
  - النسخ الاحتياطي وصيانة البيانات: إنشاء نسخ احتياطية واسترجاعها.
  - سحب العينات وتتبع حالتها: تأكيد سحب العينة ومتابعتها.

  ———

  ## 3. User Roles

  - مدير النظام: إدارة المستخدمين والصلاحيات، إعدادات النظام، النسخ الاحتياطي، الوصول الكامل للتقارير والإحصائيات.
  - موظف استقبال: تسجيل المرضى، تعديل البيانات، اختيار التحاليل، إصدار الإيصال/الباركود.
  - فني مختبر: إدخال النتائج، مراجعة واعتماد النتائج، طباعة التقارير.
  - مشرف تقارير: إعداد التقارير المجمعة/الفارغة والتاريخ المرضي والطباعة.
  - محاسب/خزينة: التحصيل، الخصومات، تصفية الحسابات، والجرد الدوري.
  - مسؤول إحصائيات: إعداد تقارير إحصائية حسب الفترات والجهات والجنس.
  - مسؤول سحب عينات: متابعة حالات العينات وتأكيد السحب.

  ———

  ## 4. Database Design

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

  ERD نصيًا

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

  ## 5. UI Screens

  - Login
      - الهدف: تسجيل الدخول
      - العناصر: اسم مستخدم، كلمة مرور، تذكرني
      - العمليات: دخول، خروج
  - Main Dashboard
      - الهدف: التنقل بين وحدات النظام
      - العناصر: أزرار الوحدات
      - العمليات: فتح الوحدة
  - Patient Registration
      - الهدف: إضافة/تعديل بيانات مريض
      - العناصر: بيانات شخصية، Lab ID، جهة إحالة، نوع حساب
      - العمليات: حفظ، تعديل، حذف
  - Patient Tests Selection
      - الهدف: اختيار التحاليل للزيارة
      - العناصر: قائمة التحاليل، بحث، مجموعات، إجمالي السعر
      - العمليات: إضافة/حذف تحليل، إضافة مجموعة
  - Patient Billing / Account
      - الهدف: حساب المريض
      - العناصر: إجمالي، خصم، مدفوع، متبقي
      - العمليات: تعديل مدفوع، تسوية الحساب
  - Barcode/Receipt Printing
      - الهدف: إصدار باركود وإيصال
      - العناصر: بيانات المريض، خيارات الطباعة
      - العمليات: طباعة
  - Results Entry
      - الهدف: إدخال النتائج
      - العناصر: قائمة مرضى اليوم، تفاصيل التحليل، القيم
      - العمليات: حفظ، مراجعة، اعتماد
  - Report Viewer/Print
      - الهدف: عرض وطباعة التقرير
      - العناصر: بيانات المريض، النتائج، التعليقات
      - العمليات: طباعة، إعادة طباعة
  - Combined Report
      - الهدف: تقرير مجمّع
      - العناصر: قائمة تحاليل مرتبة
      - العمليات: إعادة ترتيب، طباعة
  - Blank Report
      - الهدف: تقرير بيانات فقط
      - العناصر: بيانات المريض
      - العمليات: طباعة
  - Culture & Sensitivity
      - الهدف: نتائج المزارع والمضادات
      - العناصر: مزرعة، مضادات، حساسية
      - العمليات: إضافة/تعديل
  - Patient Search
      - الهدف: البحث عن المرضى
      - العناصر: فلاتر بحث، قائمة زيارات
      - العمليات: فتح زيارة
  - Patient History
      - الهدف: تاريخ مرضي
      - العناصر: زيارات وفترات
      - العمليات: طباعة تاريخ
  - Work Sheet (By Patient)
      - الهدف: ورقة عمل بالمرضى
      - العناصر: فترة زمنية، قائمة مرضى
      - العمليات: طباعة
  - Work Sheet (By Test)
      - الهدف: ورقة عمل بالتحاليل
      - العناصر: فترة زمنية، قائمة تحاليل
      - العمليات: طباعة
  - Test Catalog
      - الهدف: إدارة التحاليل
      - العناصر: اسم التحليل، سعر، وحدة، عينة
      - العمليات: إضافة/تعديل/حذف
  - Reference Ranges
      - الهدف: القيم المرجعية
      - العناصر: مدى طبيعي حسب السن/النوع
      - العمليات: إضافة/تعديل
  - Test Comments
      - الهدف: تعليقات ثابتة
      - العناصر: قائمة تعليقات
      - العمليات: إضافة/تعديل/حذف
  - Price Lists
      - الهدف: قوائم أسعار
      - العناصر: قائمة التحاليل وأسعارها
      - العمليات: إنشاء قائمة، طباعة
  - Custom Groups
      - الهدف: مجموعات تحاليل
      - العناصر: اسم المجموعة، التحاليل، السعر
      - العمليات: إضافة/تعديل/حذف
  - Referrals & External Entities
      - الهدف: إدارة الجهات
      - العناصر: بيانات الجهة ونوعها
      - العمليات: إضافة/تعديل/حذف
  - Users & Permissions
      - الهدف: إدارة المستخدمين
      - العناصر: قائمة مستخدمين، صلاحيات
      - العمليات: إضافة/تعديل/حذف
  - Attendance Log
      - الهدف: حضور وانصراف
      - العناصر: سجل الدخول والخروج
      - العمليات: عرض
  - Statistics
      - الهدف: إحصائيات
      - العناصر: فلاتر زمنية وتقارير
      - العمليات: توليد تقرير
  - Accounts / Treasury
      - الهدف: جرد وحسابات
      - العناصر: إجماليات وفترات
      - العمليات: طباعة جرد
  - System Settings
      - الهدف: إعدادات التقارير والطباعة
      - العناصر: هوامش، ألوان، طابعة افتراضية
      - العمليات: حفظ
  - Backup & Restore
      - الهدف: نسخ احتياطي واسترجاع
      - العناصر: مسار نسخ، قائمة نسخ
      - العمليات: إنشاء، استرجاع
  - Sample Collection
      - الهدف: تتبع سحب العينات
      - العناصر: قائمة مرضى، حالة عينة
      - العمليات: تحديث الحالة

  ———

  ## 6. Workflow

  - تسجيل مريض
      - فتح شاشة المرضى
      - إدخال البيانات الأساسية
      - توليد Lab ID
      - حفظ وطباعة باركود
  - طلب تحليل
      - اختيار المريض/الزيارة
      - اختيار تحاليل أو مجموعة
      - احتساب التكلفة
      - إصدار إيصال
  - إدخال النتائج
      - اختيار المريض من قائمة اليوم
      - إدخال القيم وربطها بالنطاق المرجعي
      - مراجعة واعتماد النتائج
  - إصدار تقرير
      - فتح التقرير
      - التأكد من النتائج المعتمدة
      - طباعة التقرير وتسليمه

  ———

  ## 7. Project Architecture (MVVM)

  - هيكل المشروع
      - Open_lab.UI
      - Open_lab.ViewModels
      - Open_lab.Models
      - Open_lab.Data
      - Open_lab.Services
      - Open_lab.Infrastructure (اختياري)
  - توزيع الطبقات
      - Models: الكيانات وDTOs
      - Views: شاشات WPF
      - ViewModels: حالة الشاشة، أوامر، تحقق
      - Data: DbContext وRepositories
      - Services: منطق الأعمال
  - تطبيق MVVM
      - Views تعرض البيانات فقط عبر Binding
      - ViewModels تحتوي Properties وCommands وValidation
      - Models تمثل البيانات
      - Services تنفذ منطق الأعمال
      - Data تعزل التخزين عن واجهة المستخدم

  ———

  # Execution Plan

  ## Phase 1 - Setup

  - إنشاء Solution ومشاريع الطبقات
  - إعداد بيئة .NET 8 وWPF
  - إعداد اتصال SQL Server

  ———

  ## Phase 2 - Models & Database

  - إنشاء الكيانات وفق الجداول المذكورة
  - إعداد DbContext
  - إنشاء المهاجرات (Migrations) وبناء قاعدة البيانات

  ———

  ## Phase 3 - Views & ViewModels

  - إنشاء الواجهات الأساسية
  - ربط كل View بـ ViewModel
  - إعداد Commands والتحقق الأولي

  ———

  ## Phase 4 - Logic

  - بناء Services الخاصة بإدارة المرضى والتحاليل والنتائج
  - تطبيق Business Rules
  - تكامل مع Data Layer

  ———

  ## Phase 5 - Integration

  - ربط التنقل بين الشاشات
  - تكامل كامل للـ UI مع المنطق والبيانات
  - تحسين الأداء وتجربة المستخدم