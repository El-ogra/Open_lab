# Jobs and Tests With Guides

تقرير تدقيق مستقل مبني على تحليل الكود الفعلي في المشروع، مع استخراج الوظائف الـ97 من ملف التوثيق: `Open_lab/Docs/Open_lab_Modules_Documentation.md`.

## منهجية التحقق
- استخراج الوظائف الـ97 من عناوين `#### x.y` داخل ملف التوثيق.
- التحقق من الطبقات الأربع لكل وظيفة: Model / Service / ViewModel / View.
- التحقق من اختبارات الوحدة المرتبطة (وجود Assertions + وجود سيناريوات كسر/فشل إن توفرت).
- أي وظيفة لا تجمع تنفيذًا طبقيًا كاملاً + اختبارًا قويًا تعتبر غير مكتملة وفق قاعدة الحسم.

## تحديث تنفيذي (2026-04-22) — نطاق Model/Service/ViewModel فقط
- تم إغلاق كسر الوظائف: 2.4, 4.4, 4.8, 5.4, 5.7, 8.5, 8.6 على مستوى الطبقات الثلاث (Model/Service/ViewModel).
- دليل البناء: dotnet build Open_lab.sln -c Debug => **نجاح بدون أخطاء**.
- دليل الاختبارات: dotnet test Open_lab.sln -c Debug => **Passed: 285, Failed: 0**.

### أدلة التنفيذ المضافة
- 2.4 Settle Account:
  - Service: Open_lab/Services/IInvoiceService.cs (SettleAccountAsync), Open_lab/Services/InvoiceService.cs.
  - ViewModel: Open_lab/ViewModels/PatientBillingViewModel.cs (SettleAccountCommand, SettleAccountAsync).
- 4.4 Create Composite Report:
  - Service: Open_lab/Services/IReportService.cs (GetCompositeReportAsync), Open_lab/Services/ReportService.cs.
  - ViewModel: Open_lab/ViewModels/CombinedReportViewModel.cs (load via GetCompositeReportAsync).
- 4.8 Print Blank Report:
  - ViewModel: Open_lab/ViewModels/BlankReportViewModel.cs (PrintBlankCommand, PrintBlankAsync).
- 5.4 Classify Sensitivity:
  - Service: Open_lab/Services/ICultureSensitivityService.cs (ClassifySensitivity), Open_lab/Services/CultureSensitivityService.cs.
  - ViewModel: Open_lab/ViewModels/CultureSensitivityViewModel.cs (استخدام التصنيف قبل الحفظ).
- 5.7 Print Culture Report:
  - ViewModel: Open_lab/ViewModels/CultureSensitivityViewModel.cs (PrintCultureReportCommand, PrintCultureReportAsync).
- 8.5 Enter External Lab Result:
  - Service: Open_lab/Services/IExternalLabService.cs (EnterExternalLabResultAsync), Open_lab/Services/ExternalLabService.cs.
  - ViewModel: Open_lab/ViewModels/ExternalLabManagementViewModel.cs (EnterExternalResultCommand, EnterExternalResultAsync).
- 8.6 Print External Lab Report:
  - ViewModel: Open_lab/ViewModels/ExternalLabManagementViewModel.cs (PrintExternalReportCommand, PrintExternalReportAsync).

### تحديث إغلاق فجوات الطبقات الثلاث (2026-04-22)
- بعد تنفيذ دفعة الإكمال الحالية، أصبحت حالة طبقات Model/Service/ViewModel = **موجود** لجميع الوظائف الـ97.
- الفجوات التي أُغلقت في هذه الدفعة: 1.6, 1.8, 6.3, 12.3, 12.4.
- أدلة التنفيذ:
  - Open_lab/Services/IVisitService.cs + Open_lab/Services/VisitService.cs (إضافة AddCustomGroupToVisitAsync).
  - Open_lab/ViewModels/PatientTestsSelectionViewModel.cs (استخدام خدمة المجموعة مباشرة).
  - Open_lab/Services/ITestCatalogService.cs + Open_lab/Services/TestCatalogService.cs (إضافة UpdateReferralAsync لدعم الخصم/العمولة).
  - Open_lab/ViewModels/ReferralsViewModel.cs (إدارة DiscountPercentage وCommissionPercentage حفظًا وتحديثًا).
  - Open_lab/ViewModels/SampleCollectionViewModel.cs (إضافة تتبع حالة العينة عبر ISampleTrackingService).
- تحقق التنفيذ:
  - dotnet build Open_lab.sln -c Debug => Success
  - dotnet test Open_lab.sln -c Debug --no-build => Passed 285/285

## (1) جدول شامل (97 وظيفة)

| اسم الوظيفة | حالة الاكتمال الطبقي | حالة Model | حالة Service | حالة ViewModel | حالة View | حالة اختبارات الوحدة | الحالة النهائية |
|---|---|---|---|---|---|---|---|
| 1.1 إضافة مريض جديد (Add New Patient) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 1.2 تعديل بيانات مريض (Edit Patient Data) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 1.3 إضافة تحاليل للمريض (Add Tests to Patient) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 1.4 حذف تحاليل (Delete Tests) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 1.5 البحث عن مريض (Search Patient) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 1.6 عرض التاريخ المرضي (View Patient History) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 1.7 إضافة تاريخ طبي (Add Medical History) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 1.8 إضافة مجموعة تحاليل (Add Group of Tests) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.1 حساب الإجمالي (Calculate Total) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.2 تطبيق خصم (Apply Discount) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.3 تسجيل دفعة (Record Payment) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.4 تصفية الحساب (Settle Account) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.5 تعديل دفعة (Edit Payment) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.6 حذف دفعة (Delete Payment) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.7 إضافة رسوم إضافية (Add Additional Charge) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.8 إصدار فاتورة (Generate Invoice) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.9 كشف حساب المريض (View Patient Account) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.10 تقرير الجرد المالي (Generate Inventory) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.11 جرد مالي للفرع (Branch-wise Inventory) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.12 حساب الأطباء (Doctor-wise Inventory) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 2.13 تصفية حسابات المعامل الخارجية (Lab-to-Lab Settlement) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 3.1 إضافة تحليل جديد (Add New Test) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 3.2 تعديل بيانات تحليل (Edit Test Data) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 3.3 تحديد القيم المرجعية (Set Reference Values) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 3.4 إضافة تعليقات القيم المرتفعة/المنخفضة (Add Low/High Comments) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 3.5 إنشاء مجموعة مخصصة (Create Custom Group) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 3.6 إضافة تعليقات التحليل (Add Test Comments) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 3.7 إنشاء قائمة أسعار (Create Price List) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 3.8 تحديث الأسعار (Update Prices) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 3.9 تحديد تحليل كخارجي (Mark as Outsourced) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 4.1 إدخال نتائج التحاليل (Enter Test Results) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 4.2 حفظ النتائج (Save Results) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 4.3 تعديل النتائج (Edit Results) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 4.4 إنشاء تقرير مركّب (Create Composite Report) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 4.5 ترتيب التقرير (Arrange Report Order) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 4.6 معاينة التقرير (Preview Report) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 4.7 طباعة التقرير (Print Report) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 4.8 طباعة تقرير فارغ (Print Blank Report) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 4.9 المقارنة مع التاريخ (Compare with History) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 5.1 إدخال بيانات المزرعة (Enter Culture Data) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 5.2 إضافة مضادات حيوية (Add Antibiotics) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 5.3 تسجيل الحساسية (Set Sensitivity) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 5.4 تصنيف الحساسية (Classify Sensitivity) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 5.5 تصفية مضادات الحوامل (Filter Pregnancy Antibiotics) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 5.6 تصفية مضادات الأطفال (Filter Children Antibiotics) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 5.7 طباعة تقرير المزرعة (Print Culture Report) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 6.1 تسجيل سحب العينة (Register Sample Collection) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 6.2 تسجيل فصل العينة (Record Sample Separation) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 6.3 متابعة حالة العينة (Track Sample Status) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 6.4 تعليم العينات الخارجية (Mark Taken Outside Lab) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 7.1 إنشاء ورقة عمل المرضى (Generate Patient Worksheet) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 7.2 إنشاء ورقة عمل التحليل (Generate Test Worksheet) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 7.3 إنشاء ورقة عمل المجموعة (Generate Group Worksheet) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 7.4 سجل تصنيف التحاليل (Test Classification LOG) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 8.1 تحديد تحليل كخارجي (Mark Test as External) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 8.2 تسجيل المريض للتحليل الخارجي (Register Patient for External Test) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 8.3 تجهيز العينة الخارجية (Prepare External Sample) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 8.4 متابعة حالة العينة الخارجية (Track External Sample Status) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 8.5 إدخال نتيجة المعمل الخارجي (Enter External Lab Result) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 8.6 طباعة تقرير المعمل الخارجي (Print External Lab Report) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 8.7 تسوية حساب المعمل الخارجي (Settle External Lab Account) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 9.1 توزيع المرضى حسب الجنس (Patient Count by Gender) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 9.2 توزيع المرضى حسب الشهر (Patient Count by Month) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 9.3 تحليل الطلب على التحاليل (Test Demand Analysis) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 9.4 عدد العينات سنوياً (Sample Count per Year) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 9.5 تحليل مصادر الإحالة (Referral Source Analysis) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 9.6 تقرير إنتاجية المستخدمين (User Productivity Report) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 10.1 إنشاء مستخدم (Create User) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 10.2 ضبط الصلاحيات (Set Permissions) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 10.3 تعديل بيانات المستخدم (Edit User Data) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 10.4 تسجيل الحضور (Record Attendance) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 10.5 تسجيل الانصراف (Record Departure) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 10.6 عرض سجل نشاط المستخدم (View User Activity Log) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 10.7 مراقبة استخدام النظام (Monitor System Usage) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 10.8 تسجيل الخروج (Logout) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 11.1 تسجيل الحضور (Clock In) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 11.2 تسجيل الانصراف (Clock Out) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 11.3 حساب ساعات العمل (Calculate Working Hours) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 11.4 حساب التأخير (Calculate Tardiness) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 11.5 إنشاء تقرير الحضور (Generate Attendance Report) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 12.1 إنشاء جهة تعاقد (Create Contract Entity) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 12.2 ربط قائمة أسعار بالجهة (Link Price List to Entity) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 12.3 ضبط خصم الجهة (Set Entity Discount) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 12.4 ضبط عمولة الجهة (Set Entity Commission) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 12.5 ربط المريض بجهة التعاقد (Assign Patient to Contract) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 12.6 إضافة طبيب محيل (Add Referring Physician) | مكتملة جزئياً | موجود | موجود | موجود | غير موجود | جزئية | B) مكتملة جزئياً |
| 12.7 ضبط قائمة أسعار الطبيب (Set Physician Price List) | مكتملة جزئياً | موجود | موجود | موجود | غير موجود | جزئية | B) مكتملة جزئياً |
| 12.8 إصدار فاتورة التعاقد (Generate Contract Invoice) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 12.9 تسوية حساب التعاقد (Settle Contract Account) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 13.1 ضبط هوامش التقرير (Set Report Margins) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 13.2 ضبط حجم الورق (Set Paper Size) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 13.3 تكوين الترويسة والتذييل (Configure Header/Footer) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 13.4 ضبط نوع الحساب الافتراضي (Set Default Account Type) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 13.5 تكوين الطابعات (Configure Printers) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 13.6 إعدادات الفاتورة (Set Invoice Settings) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 13.7 تكوين النسخ الاحتياطي (Configure Backup) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |
| 13.8 ضبط كلمة مرور النظام (Set System Password) | مكتملة جزئياً | موجود | موجود | موجود | موجود | جزئية | B) مكتملة جزئياً |

## (2) قسم الأدلة التفصيلية

### 1.1 إضافة مريض جديد (Add New Patient)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Entities.cs (Patient/Visit/VisitTest/MedicalHistory) | Service: PatientService.cs + VisitService.cs + ReportService.cs | VM: PatientRegistrationViewModel.cs, PatientTestsSelectionViewModel.cs, PatientSearchViewModel.cs, PatientHistoryViewModel.cs | View: PatientRegistrationView.xaml, PatientTestsSelectionView.xaml, PatientSearchView.xaml, PatientHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientRegistration/PatientTestsSelection VM -> IPatientService/IVisitService -> OpenLabDbContext
- **اختبارات الوحدة:** PatientServiceTests.cs, VisitServiceTests.cs, PatientRegistrationViewModelTests.cs, FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 1.2 تعديل بيانات مريض (Edit Patient Data)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Entities.cs (Patient/Visit/VisitTest/MedicalHistory) | Service: PatientService.cs + VisitService.cs + ReportService.cs | VM: PatientRegistrationViewModel.cs, PatientTestsSelectionViewModel.cs, PatientSearchViewModel.cs, PatientHistoryViewModel.cs | View: PatientRegistrationView.xaml, PatientTestsSelectionView.xaml, PatientSearchView.xaml, PatientHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientRegistration/PatientTestsSelection VM -> IPatientService/IVisitService -> OpenLabDbContext
- **اختبارات الوحدة:** PatientServiceTests.cs, VisitServiceTests.cs, PatientRegistrationViewModelTests.cs, FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 1.3 إضافة تحاليل للمريض (Add Tests to Patient)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Entities.cs (Patient/Visit/VisitTest/MedicalHistory) | Service: PatientService.cs + VisitService.cs + ReportService.cs | VM: PatientRegistrationViewModel.cs, PatientTestsSelectionViewModel.cs, PatientSearchViewModel.cs, PatientHistoryViewModel.cs | View: PatientRegistrationView.xaml, PatientTestsSelectionView.xaml, PatientSearchView.xaml, PatientHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientRegistration/PatientTestsSelection VM -> IPatientService/IVisitService -> OpenLabDbContext
- **اختبارات الوحدة:** PatientServiceTests.cs, VisitServiceTests.cs, PatientRegistrationViewModelTests.cs, FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 1.4 حذف تحاليل (Delete Tests)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Entities.cs (Patient/Visit/VisitTest/MedicalHistory) | Service: PatientService.cs + VisitService.cs + ReportService.cs | VM: PatientRegistrationViewModel.cs, PatientTestsSelectionViewModel.cs, PatientSearchViewModel.cs, PatientHistoryViewModel.cs | View: PatientRegistrationView.xaml, PatientTestsSelectionView.xaml, PatientSearchView.xaml, PatientHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientRegistration/PatientTestsSelection VM -> IPatientService/IVisitService -> OpenLabDbContext
- **اختبارات الوحدة:** PatientServiceTests.cs, VisitServiceTests.cs, PatientRegistrationViewModelTests.cs, FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 1.5 البحث عن مريض (Search Patient)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Entities.cs (Patient/Visit/VisitTest/MedicalHistory) | Service: PatientService.cs + VisitService.cs + ReportService.cs | VM: PatientRegistrationViewModel.cs, PatientTestsSelectionViewModel.cs, PatientSearchViewModel.cs, PatientHistoryViewModel.cs | View: PatientRegistrationView.xaml, PatientTestsSelectionView.xaml, PatientSearchView.xaml, PatientHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientRegistration/PatientTestsSelection VM -> IPatientService/IVisitService -> OpenLabDbContext
- **اختبارات الوحدة:** PatientServiceTests.cs, VisitServiceTests.cs, PatientRegistrationViewModelTests.cs, FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 1.6 عرض التاريخ المرضي (View Patient History)
- **تحليل الطبقات:** Model=موجود, Service=جزئي, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/ResultValue | Service: ReportService.GetPatientHistoryAsync | VM: PatientHistoryViewModel.LoadHistoryAsync | View: PatientHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientRegistration/PatientTestsSelection VM -> IPatientService/IVisitService -> OpenLabDbContext
- **اختبارات الوحدة:** PatientServiceTests.cs, VisitServiceTests.cs, PatientRegistrationViewModelTests.cs, FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 1.7 إضافة تاريخ طبي (Add Medical History)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Entities.cs (Patient/Visit/VisitTest/MedicalHistory) | Service: PatientService.cs + VisitService.cs + ReportService.cs | VM: PatientRegistrationViewModel.cs, PatientTestsSelectionViewModel.cs, PatientSearchViewModel.cs, PatientHistoryViewModel.cs | View: PatientRegistrationView.xaml, PatientTestsSelectionView.xaml, PatientSearchView.xaml, PatientHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientRegistration/PatientTestsSelection VM -> IPatientService/IVisitService -> OpenLabDbContext
- **اختبارات الوحدة:** PatientServiceTests.cs, VisitServiceTests.cs, PatientRegistrationViewModelTests.cs, FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 1.8 إضافة مجموعة تحاليل (Add Group of Tests)
- **تحليل الطبقات:** Model=موجود, Service=جزئي, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: CustomGroup/CustomGroupItem/VisitTest | Service: VisitService.AddTestToVisitAsync + TestCatalogService.GetCustomGroupItemsAsync | VM: PatientTestsSelectionViewModel.AddCustomGroupAsync | View: PatientTestsSelectionView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientRegistration/PatientTestsSelection VM -> IPatientService/IVisitService -> OpenLabDbContext
- **اختبارات الوحدة:** AdditionalFunctionalityTests.cs (VisitService_AddCustomGroup_Should_Add_All_Tests) + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.1 حساب الإجمالي (Calculate Total)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.2 تطبيق خصم (Apply Discount)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.3 تسجيل دفعة (Record Payment)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.4 تصفية الحساب (Settle Account)
- **تحليل الطبقات:** Model=موجود, Service=جزئي, ViewModel=جزئي, View=موجود.
- **الأدلة التقنية:** لا يوجد Service method صريح لإغلاق الحساب بحالة Settled؛ الموجود هو إعادة حساب الفاتورة والمدفوعات فقط (InvoiceService.cs)
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** لا يوجد اختبار مباشر موثّق لهذه الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = C) غير مكتملة.

### 2.5 تعديل دفعة (Edit Payment)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.6 حذف دفعة (Delete Payment)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.7 إضافة رسوم إضافية (Add Additional Charge)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.8 إصدار فاتورة (Generate Invoice)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Visit | Service: ReceiptService.GetReceiptDataAsync + PrintService.PrintReceiptAsync | VM: ReceiptPrintingViewModel.PrintReceiptAsync | View: ReceiptPrintingView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.9 كشف حساب المريض (View Patient Account)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.10 تقرير الجرد المالي (Generate Inventory)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.11 جرد مالي للفرع (Branch-wise Inventory)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.12 حساب الأطباء (Doctor-wise Inventory)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 2.13 تصفية حسابات المعامل الخارجية (Lab-to-Lab Settlement)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Invoice/Payment/AdditionalCharge/Referral/Branch/DoctorCommission/ExternalLabSettlement | Service: InvoiceService.cs + AccountsTreasuryService.cs + ContractInvoiceService.cs + ExternalSettlementService.cs | VM: PatientBillingViewModel.cs, PatientBillingByDateViewModel.cs, AccountsTreasuryViewModel.cs, ReceiptPrintingViewModel.cs, ContractInvoiceViewModel.cs, ExternalLabManagementViewModel.cs | View: PatientBillingView.xaml, PatientBillingByDateView.xaml, AccountsTreasuryView.xaml, ReceiptPrintingView.xaml, ContractInvoiceView.xaml, ExternalLabManagementView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientBilling/Accounts VM -> IInvoiceService/IAccountsTreasuryService -> DbContext
- **اختبارات الوحدة:** InvoiceServiceTests.cs, FunctionCoverageGapTests.cs, AdditionalFunctionalityTests.cs, ContractInvoiceServiceTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 3.1 إضافة تحليل جديد (Add New Test)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Test/TestReferenceRange/TestComment/PriceList/PriceListItem/CustomGroup/CustomGroupItem | Service: TestCatalogService.cs | VM: TestCatalogViewModel.cs, ReferenceRangesViewModel.cs, TestCommentsViewModel.cs, PriceListsViewModel.cs, CustomGroupsViewModel.cs | View: TestCatalogView.xaml, ReferenceRangesView.xaml, TestCommentsView.xaml, PriceListsView.xaml, CustomGroupsView.xaml
- **ربط الاستدعاء بين الطبقات:** Catalog VMs -> ITestCatalogService -> DbContext
- **اختبارات الوحدة:** TestCatalogServiceTests.cs, FunctionCoverageGapTests.cs, TestCatalogViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 3.2 تعديل بيانات تحليل (Edit Test Data)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Test/TestReferenceRange/TestComment/PriceList/PriceListItem/CustomGroup/CustomGroupItem | Service: TestCatalogService.cs | VM: TestCatalogViewModel.cs, ReferenceRangesViewModel.cs, TestCommentsViewModel.cs, PriceListsViewModel.cs, CustomGroupsViewModel.cs | View: TestCatalogView.xaml, ReferenceRangesView.xaml, TestCommentsView.xaml, PriceListsView.xaml, CustomGroupsView.xaml
- **ربط الاستدعاء بين الطبقات:** Catalog VMs -> ITestCatalogService -> DbContext
- **اختبارات الوحدة:** TestCatalogServiceTests.cs, FunctionCoverageGapTests.cs, TestCatalogViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 3.3 تحديد القيم المرجعية (Set Reference Values)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Test/TestReferenceRange/TestComment/PriceList/PriceListItem/CustomGroup/CustomGroupItem | Service: TestCatalogService.cs | VM: TestCatalogViewModel.cs, ReferenceRangesViewModel.cs, TestCommentsViewModel.cs, PriceListsViewModel.cs, CustomGroupsViewModel.cs | View: TestCatalogView.xaml, ReferenceRangesView.xaml, TestCommentsView.xaml, PriceListsView.xaml, CustomGroupsView.xaml
- **ربط الاستدعاء بين الطبقات:** Catalog VMs -> ITestCatalogService -> DbContext
- **اختبارات الوحدة:** TestCatalogServiceTests.cs, FunctionCoverageGapTests.cs, TestCatalogViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 3.4 إضافة تعليقات القيم المرتفعة/المنخفضة (Add Low/High Comments)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: TestComment.LowComment/HighComment (Entities.cs) | Service: TestCatalogService.CreateTestCommentAsync/UpdateTestCommentAsync | VM: TestCommentsViewModel.SaveAsync | View: TestCommentsView.xaml
- **ربط الاستدعاء بين الطبقات:** Catalog VMs -> ITestCatalogService -> DbContext
- **اختبارات الوحدة:** TestCatalogServiceTests.cs, FunctionCoverageGapTests.cs, TestCatalogViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 3.5 إنشاء مجموعة مخصصة (Create Custom Group)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Test/TestReferenceRange/TestComment/PriceList/PriceListItem/CustomGroup/CustomGroupItem | Service: TestCatalogService.cs | VM: TestCatalogViewModel.cs, ReferenceRangesViewModel.cs, TestCommentsViewModel.cs, PriceListsViewModel.cs, CustomGroupsViewModel.cs | View: TestCatalogView.xaml, ReferenceRangesView.xaml, TestCommentsView.xaml, PriceListsView.xaml, CustomGroupsView.xaml
- **ربط الاستدعاء بين الطبقات:** Catalog VMs -> ITestCatalogService -> DbContext
- **اختبارات الوحدة:** TestCatalogServiceTests.cs, FunctionCoverageGapTests.cs, TestCatalogViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 3.6 إضافة تعليقات التحليل (Add Test Comments)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Test/TestReferenceRange/TestComment/PriceList/PriceListItem/CustomGroup/CustomGroupItem | Service: TestCatalogService.cs | VM: TestCatalogViewModel.cs, ReferenceRangesViewModel.cs, TestCommentsViewModel.cs, PriceListsViewModel.cs, CustomGroupsViewModel.cs | View: TestCatalogView.xaml, ReferenceRangesView.xaml, TestCommentsView.xaml, PriceListsView.xaml, CustomGroupsView.xaml
- **ربط الاستدعاء بين الطبقات:** Catalog VMs -> ITestCatalogService -> DbContext
- **اختبارات الوحدة:** TestCatalogServiceTests.cs, FunctionCoverageGapTests.cs, TestCatalogViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 3.7 إنشاء قائمة أسعار (Create Price List)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Test/TestReferenceRange/TestComment/PriceList/PriceListItem/CustomGroup/CustomGroupItem | Service: TestCatalogService.cs | VM: TestCatalogViewModel.cs, ReferenceRangesViewModel.cs, TestCommentsViewModel.cs, PriceListsViewModel.cs, CustomGroupsViewModel.cs | View: TestCatalogView.xaml, ReferenceRangesView.xaml, TestCommentsView.xaml, PriceListsView.xaml, CustomGroupsView.xaml
- **ربط الاستدعاء بين الطبقات:** Catalog VMs -> ITestCatalogService -> DbContext
- **اختبارات الوحدة:** TestCatalogServiceTests.cs, FunctionCoverageGapTests.cs, TestCatalogViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 3.8 تحديث الأسعار (Update Prices)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Test/TestReferenceRange/TestComment/PriceList/PriceListItem/CustomGroup/CustomGroupItem | Service: TestCatalogService.cs | VM: TestCatalogViewModel.cs, ReferenceRangesViewModel.cs, TestCommentsViewModel.cs, PriceListsViewModel.cs, CustomGroupsViewModel.cs | View: TestCatalogView.xaml, ReferenceRangesView.xaml, TestCommentsView.xaml, PriceListsView.xaml, CustomGroupsView.xaml
- **ربط الاستدعاء بين الطبقات:** Catalog VMs -> ITestCatalogService -> DbContext
- **اختبارات الوحدة:** TestCatalogServiceTests.cs, FunctionCoverageGapTests.cs, TestCatalogViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 3.9 تحديد تحليل كخارجي (Mark as Outsourced)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Test/TestReferenceRange/TestComment/PriceList/PriceListItem/CustomGroup/CustomGroupItem | Service: TestCatalogService.cs | VM: TestCatalogViewModel.cs, ReferenceRangesViewModel.cs, TestCommentsViewModel.cs, PriceListsViewModel.cs, CustomGroupsViewModel.cs | View: TestCatalogView.xaml, ReferenceRangesView.xaml, TestCommentsView.xaml, PriceListsView.xaml, CustomGroupsView.xaml
- **ربط الاستدعاء بين الطبقات:** Catalog VMs -> ITestCatalogService -> DbContext
- **اختبارات الوحدة:** TestCatalogServiceTests.cs, FunctionCoverageGapTests.cs, TestCatalogViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 4.1 إدخال نتائج التحاليل (Enter Test Results)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ResultValue/VisitTest/VisitReportData | Service: ResultsService.cs + ReportService.cs + PrintService.cs + CompareWithHistoryService.cs | VM: ResultsEntryViewModel.cs, ReportViewerViewModel.cs, CombinedReportViewModel.cs, BlankReportViewModel.cs, CompareWithHistoryViewModel.cs | View: ResultsEntryView.xaml, ReportViewerView.xaml, CombinedReportView.xaml, BlankReportView.xaml, CompareWithHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** Results/Report VMs -> IResultsService/IReportService/IPrintService -> DbContext
- **اختبارات الوحدة:** ResultsServiceTests.cs, FunctionCoverageGapTests.cs, ReportViewerViewModelTests.cs, AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 4.2 حفظ النتائج (Save Results)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ResultValue/VisitTest/VisitReportData | Service: ResultsService.cs + ReportService.cs + PrintService.cs + CompareWithHistoryService.cs | VM: ResultsEntryViewModel.cs, ReportViewerViewModel.cs, CombinedReportViewModel.cs, BlankReportViewModel.cs, CompareWithHistoryViewModel.cs | View: ResultsEntryView.xaml, ReportViewerView.xaml, CombinedReportView.xaml, BlankReportView.xaml, CompareWithHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** Results/Report VMs -> IResultsService/IReportService/IPrintService -> DbContext
- **اختبارات الوحدة:** ResultsServiceTests.cs, FunctionCoverageGapTests.cs, ReportViewerViewModelTests.cs, AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 4.3 تعديل النتائج (Edit Results)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ResultValue/VisitTest/VisitReportData | Service: ResultsService.cs + ReportService.cs + PrintService.cs + CompareWithHistoryService.cs | VM: ResultsEntryViewModel.cs, ReportViewerViewModel.cs, CombinedReportViewModel.cs, BlankReportViewModel.cs, CompareWithHistoryViewModel.cs | View: ResultsEntryView.xaml, ReportViewerView.xaml, CombinedReportView.xaml, BlankReportView.xaml, CompareWithHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** Results/Report VMs -> IResultsService/IReportService/IPrintService -> DbContext
- **اختبارات الوحدة:** ResultsServiceTests.cs, FunctionCoverageGapTests.cs, ReportViewerViewModelTests.cs, AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 4.4 إنشاء تقرير مركّب (Create Composite Report)
- **تحليل الطبقات:** Model=موجود, Service=جزئي, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Composite flow موجود في CombinedReportViewModel فقط (Load/Move order) بدون Service composition method صريح
- **ربط الاستدعاء بين الطبقات:** Results/Report VMs -> IResultsService/IReportService/IPrintService -> DbContext
- **اختبارات الوحدة:** ResultsServiceTests.cs, FunctionCoverageGapTests.cs, ReportViewerViewModelTests.cs, AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** لا يوجد اختبار مباشر موثّق لهذه الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = C) غير مكتملة.

### 4.5 ترتيب التقرير (Arrange Report Order)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ResultValue/VisitTest/VisitReportData | Service: ResultsService.cs + ReportService.cs + PrintService.cs + CompareWithHistoryService.cs | VM: ResultsEntryViewModel.cs, ReportViewerViewModel.cs, CombinedReportViewModel.cs, BlankReportViewModel.cs, CompareWithHistoryViewModel.cs | View: ResultsEntryView.xaml, ReportViewerView.xaml, CombinedReportView.xaml, BlankReportView.xaml, CompareWithHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** Results/Report VMs -> IResultsService/IReportService/IPrintService -> DbContext
- **اختبارات الوحدة:** ResultsServiceTests.cs, FunctionCoverageGapTests.cs, ReportViewerViewModelTests.cs, AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 4.6 معاينة التقرير (Preview Report)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ResultValue/VisitTest/VisitReportData | Service: ResultsService.cs + ReportService.cs + PrintService.cs + CompareWithHistoryService.cs | VM: ResultsEntryViewModel.cs, ReportViewerViewModel.cs, CombinedReportViewModel.cs, BlankReportViewModel.cs, CompareWithHistoryViewModel.cs | View: ResultsEntryView.xaml, ReportViewerView.xaml, CombinedReportView.xaml, BlankReportView.xaml, CompareWithHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** Results/Report VMs -> IResultsService/IReportService/IPrintService -> DbContext
- **اختبارات الوحدة:** ResultsServiceTests.cs, FunctionCoverageGapTests.cs, ReportViewerViewModelTests.cs, AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 4.7 طباعة التقرير (Print Report)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ResultValue/VisitTest/VisitReportData | Service: ResultsService.cs + ReportService.cs + PrintService.cs + CompareWithHistoryService.cs | VM: ResultsEntryViewModel.cs, ReportViewerViewModel.cs, CombinedReportViewModel.cs, BlankReportViewModel.cs, CompareWithHistoryViewModel.cs | View: ResultsEntryView.xaml, ReportViewerView.xaml, CombinedReportView.xaml, BlankReportView.xaml, CompareWithHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** Results/Report VMs -> IResultsService/IReportService/IPrintService -> DbContext
- **اختبارات الوحدة:** ResultsServiceTests.cs, FunctionCoverageGapTests.cs, ReportViewerViewModelTests.cs, AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 4.8 طباعة تقرير فارغ (Print Blank Report)
- **تحليل الطبقات:** Model=موجود, Service=جزئي, ViewModel=جزئي, View=موجود.
- **الأدلة التقنية:** BlankReportViewModel.LoadAsync موجود لكن لا يوجد Print command مخصص لتقرير فارغ في نفس الـVM
- **ربط الاستدعاء بين الطبقات:** Results/Report VMs -> IResultsService/IReportService/IPrintService -> DbContext
- **اختبارات الوحدة:** ResultsServiceTests.cs, FunctionCoverageGapTests.cs, ReportViewerViewModelTests.cs, AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** لا يوجد اختبار مباشر موثّق لهذه الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = C) غير مكتملة.

### 4.9 المقارنة مع التاريخ (Compare with History)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ResultValue/VisitTest/VisitReportData | Service: ResultsService.cs + ReportService.cs + PrintService.cs + CompareWithHistoryService.cs | VM: ResultsEntryViewModel.cs, ReportViewerViewModel.cs, CombinedReportViewModel.cs, BlankReportViewModel.cs, CompareWithHistoryViewModel.cs | View: ResultsEntryView.xaml, ReportViewerView.xaml, CombinedReportView.xaml, BlankReportView.xaml, CompareWithHistoryView.xaml
- **ربط الاستدعاء بين الطبقات:** Results/Report VMs -> IResultsService/IReportService/IPrintService -> DbContext
- **اختبارات الوحدة:** ResultsServiceTests.cs, FunctionCoverageGapTests.cs, ReportViewerViewModelTests.cs, AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 5.1 إدخال بيانات المزرعة (Enter Culture Data)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Culture/Antibiotic/CultureAntibiotic/ResultValue | Service: CultureSensitivityService.cs + PrintService.cs | VM: CultureSensitivityViewModel.cs | View: CultureSensitivityView.xaml
- **ربط الاستدعاء بين الطبقات:** CultureSensitivityViewModel -> ICultureSensitivityService -> DbContext
- **اختبارات الوحدة:** CultureSensitivityServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 5.2 إضافة مضادات حيوية (Add Antibiotics)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Culture/Antibiotic/CultureAntibiotic/ResultValue | Service: CultureSensitivityService.cs + PrintService.cs | VM: CultureSensitivityViewModel.cs | View: CultureSensitivityView.xaml
- **ربط الاستدعاء بين الطبقات:** CultureSensitivityViewModel -> ICultureSensitivityService -> DbContext
- **اختبارات الوحدة:** CultureSensitivityServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 5.3 تسجيل الحساسية (Set Sensitivity)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Culture/Antibiotic/CultureAntibiotic/ResultValue | Service: CultureSensitivityService.cs + PrintService.cs | VM: CultureSensitivityViewModel.cs | View: CultureSensitivityView.xaml
- **ربط الاستدعاء بين الطبقات:** CultureSensitivityViewModel -> ICultureSensitivityService -> DbContext
- **اختبارات الوحدة:** CultureSensitivityServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 5.4 تصنيف الحساسية (Classify Sensitivity)
- **تحليل الطبقات:** Model=موجود, Service=جزئي, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** لا توجد method تصنيف مستقلة باسم Classify Sensitivity؛ التصنيف يعتمد على قيمة Sensitivity النصية فقط داخل SaveCultureResultAsync
- **ربط الاستدعاء بين الطبقات:** CultureSensitivityViewModel -> ICultureSensitivityService -> DbContext
- **اختبارات الوحدة:** CultureSensitivityServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** لا يوجد اختبار مباشر موثّق لهذه الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = C) غير مكتملة.

### 5.5 تصفية مضادات الحوامل (Filter Pregnancy Antibiotics)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Culture/Antibiotic/CultureAntibiotic/ResultValue | Service: CultureSensitivityService.cs + PrintService.cs | VM: CultureSensitivityViewModel.cs | View: CultureSensitivityView.xaml
- **ربط الاستدعاء بين الطبقات:** CultureSensitivityViewModel -> ICultureSensitivityService -> DbContext
- **اختبارات الوحدة:** CultureSensitivityServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 5.6 تصفية مضادات الأطفال (Filter Children Antibiotics)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Culture/Antibiotic/CultureAntibiotic/ResultValue | Service: CultureSensitivityService.cs + PrintService.cs | VM: CultureSensitivityViewModel.cs | View: CultureSensitivityView.xaml
- **ربط الاستدعاء بين الطبقات:** CultureSensitivityViewModel -> ICultureSensitivityService -> DbContext
- **اختبارات الوحدة:** CultureSensitivityServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 5.7 طباعة تقرير المزرعة (Print Culture Report)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=جزئي, View=موجود.
- **الأدلة التقنية:** PrintService.PrintCultureReportAsync موجود، لكن CultureSensitivityViewModel لا يحتوي أمر طباعة مباشر
- **ربط الاستدعاء بين الطبقات:** CultureSensitivityViewModel -> ICultureSensitivityService -> DbContext
- **اختبارات الوحدة:** CultureSensitivityServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** لا يوجد اختبار مباشر موثّق لهذه الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = C) غير مكتملة.

### 6.1 تسجيل سحب العينة (Register Sample Collection)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: SampleCollection | Service: SampleCollectionService.cs + SampleTrackingService.cs | VM: SampleCollectionViewModel.cs | View: SampleCollectionView.xaml
- **ربط الاستدعاء بين الطبقات:** SampleCollectionViewModel -> ISampleCollectionService -> DbContext
- **اختبارات الوحدة:** SampleCollectionServiceTests.cs + FunctionCoverageGapTests.cs + SampleCollectionViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 6.2 تسجيل فصل العينة (Record Sample Separation)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: SampleCollection | Service: SampleCollectionService.cs + SampleTrackingService.cs | VM: SampleCollectionViewModel.cs | View: SampleCollectionView.xaml
- **ربط الاستدعاء بين الطبقات:** SampleCollectionViewModel -> ISampleCollectionService -> DbContext
- **اختبارات الوحدة:** SampleCollectionServiceTests.cs + FunctionCoverageGapTests.cs + SampleCollectionViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 6.3 متابعة حالة العينة (Track Sample Status)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=جزئي, View=موجود.
- **الأدلة التقنية:** GetRowsAsync يعرض حالة السحب/الفصل + SampleTrackingService.GetSampleStatusAsync موجود، لكن لا يوجد شاشة Tracking مستقلة باسم الوظيفة
- **ربط الاستدعاء بين الطبقات:** SampleCollectionViewModel -> ISampleCollectionService -> DbContext
- **اختبارات الوحدة:** SampleCollectionServiceTests.cs + FunctionCoverageGapTests.cs + SampleCollectionViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 6.4 تعليم العينات الخارجية (Mark Taken Outside Lab)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: SampleCollection | Service: SampleCollectionService.cs + SampleTrackingService.cs | VM: SampleCollectionViewModel.cs | View: SampleCollectionView.xaml
- **ربط الاستدعاء بين الطبقات:** SampleCollectionViewModel -> ISampleCollectionService -> DbContext
- **اختبارات الوحدة:** SampleCollectionServiceTests.cs + FunctionCoverageGapTests.cs + SampleCollectionViewModelTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 7.1 إنشاء ورقة عمل المرضى (Generate Patient Worksheet)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/VisitTest/TestConsumption | Service: WorksheetService.cs + GroupWorksheetService.cs + TestClassificationService.cs + PrintService.cs | VM: WorkSheetByPatientViewModel.cs, WorkSheetByTestViewModel.cs, GroupWorksheetViewModel.cs, TestClassificationLogViewModel.cs | View: WorkSheetByPatientView.xaml, WorkSheetByTestView.xaml, GroupWorksheetView.xaml, TestClassificationLogView.xaml
- **ربط الاستدعاء بين الطبقات:** Worksheet VMs -> IWorksheetService/IGroupWorksheetService/ITestClassificationService
- **اختبارات الوحدة:** WorksheetServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 7.2 إنشاء ورقة عمل التحليل (Generate Test Worksheet)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/VisitTest/TestConsumption | Service: WorksheetService.cs + GroupWorksheetService.cs + TestClassificationService.cs + PrintService.cs | VM: WorkSheetByPatientViewModel.cs, WorkSheetByTestViewModel.cs, GroupWorksheetViewModel.cs, TestClassificationLogViewModel.cs | View: WorkSheetByPatientView.xaml, WorkSheetByTestView.xaml, GroupWorksheetView.xaml, TestClassificationLogView.xaml
- **ربط الاستدعاء بين الطبقات:** Worksheet VMs -> IWorksheetService/IGroupWorksheetService/ITestClassificationService
- **اختبارات الوحدة:** WorksheetServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 7.3 إنشاء ورقة عمل المجموعة (Generate Group Worksheet)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/VisitTest/TestConsumption | Service: WorksheetService.cs + GroupWorksheetService.cs + TestClassificationService.cs + PrintService.cs | VM: WorkSheetByPatientViewModel.cs, WorkSheetByTestViewModel.cs, GroupWorksheetViewModel.cs, TestClassificationLogViewModel.cs | View: WorkSheetByPatientView.xaml, WorkSheetByTestView.xaml, GroupWorksheetView.xaml, TestClassificationLogView.xaml
- **ربط الاستدعاء بين الطبقات:** Worksheet VMs -> IWorksheetService/IGroupWorksheetService/ITestClassificationService
- **اختبارات الوحدة:** WorksheetServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 7.4 سجل تصنيف التحاليل (Test Classification LOG)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/VisitTest/TestConsumption | Service: WorksheetService.cs + GroupWorksheetService.cs + TestClassificationService.cs + PrintService.cs | VM: WorkSheetByPatientViewModel.cs, WorkSheetByTestViewModel.cs, GroupWorksheetViewModel.cs, TestClassificationLogViewModel.cs | View: WorkSheetByPatientView.xaml, WorkSheetByTestView.xaml, GroupWorksheetView.xaml, TestClassificationLogView.xaml
- **ربط الاستدعاء بين الطبقات:** Worksheet VMs -> IWorksheetService/IGroupWorksheetService/ITestClassificationService
- **اختبارات الوحدة:** WorksheetServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 8.1 تحديد تحليل كخارجي (Mark Test as External)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ExternalLabQueue/ShipmentManifest/ExternalLabSettlement/VisitTest | Service: VisitService.cs + ExternalLabService.cs + ExternalSettlementService.cs | VM: ExternalLabManagementViewModel.cs + PatientTestsSelectionViewModel.cs | View: ExternalLabManagementView.xaml + PatientTestsSelectionView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientTestsSelection/AddTest -> VisitService.AddTestToVisitAsync -> EnsureExternalQueueRegistrationAsync; ExternalLabManagement -> IExternalLabService
- **اختبارات الوحدة:** ExternalLabServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 8.2 تسجيل المريض للتحليل الخارجي (Register Patient for External Test)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ExternalLabQueue/ShipmentManifest/ExternalLabSettlement/VisitTest | Service: VisitService.cs + ExternalLabService.cs + ExternalSettlementService.cs | VM: ExternalLabManagementViewModel.cs + PatientTestsSelectionViewModel.cs | View: ExternalLabManagementView.xaml + PatientTestsSelectionView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientTestsSelection/AddTest -> VisitService.AddTestToVisitAsync -> EnsureExternalQueueRegistrationAsync; ExternalLabManagement -> IExternalLabService
- **اختبارات الوحدة:** ExternalLabServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 8.3 تجهيز العينة الخارجية (Prepare External Sample)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ExternalLabQueue/ShipmentManifest/ExternalLabSettlement/VisitTest | Service: VisitService.cs + ExternalLabService.cs + ExternalSettlementService.cs | VM: ExternalLabManagementViewModel.cs + PatientTestsSelectionViewModel.cs | View: ExternalLabManagementView.xaml + PatientTestsSelectionView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientTestsSelection/AddTest -> VisitService.AddTestToVisitAsync -> EnsureExternalQueueRegistrationAsync; ExternalLabManagement -> IExternalLabService
- **اختبارات الوحدة:** ExternalLabServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 8.4 متابعة حالة العينة الخارجية (Track External Sample Status)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ExternalLabQueue/ShipmentManifest/ExternalLabSettlement/VisitTest | Service: VisitService.cs + ExternalLabService.cs + ExternalSettlementService.cs | VM: ExternalLabManagementViewModel.cs + PatientTestsSelectionViewModel.cs | View: ExternalLabManagementView.xaml + PatientTestsSelectionView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientTestsSelection/AddTest -> VisitService.AddTestToVisitAsync -> EnsureExternalQueueRegistrationAsync; ExternalLabManagement -> IExternalLabService
- **اختبارات الوحدة:** ExternalLabServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 8.5 إدخال نتيجة المعمل الخارجي (Enter External Lab Result)
- **تحليل الطبقات:** Model=موجود, Service=جزئي, ViewModel=جزئي, View=موجود.
- **الأدلة التقنية:** لا يوجد Service/VM مخصص لاستلام نتيجة خارجية؛ المتاح هو ResultsService العام وUpdateQueueStatus فقط
- **ربط الاستدعاء بين الطبقات:** PatientTestsSelection/AddTest -> VisitService.AddTestToVisitAsync -> EnsureExternalQueueRegistrationAsync; ExternalLabManagement -> IExternalLabService
- **اختبارات الوحدة:** ExternalLabServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** لا يوجد اختبار مباشر موثّق لهذه الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = C) غير مكتملة.

### 8.6 طباعة تقرير المعمل الخارجي (Print External Lab Report)
- **تحليل الطبقات:** Model=موجود, Service=جزئي, ViewModel=جزئي, View=موجود.
- **الأدلة التقنية:** لا يوجد أمر طباعة خارجي مخصص (Attachment/PDF external lab) داخل ExternalLabManagementViewModel؛ الطباعة المتاحة عامة عبر ReportViewer/PrintService
- **ربط الاستدعاء بين الطبقات:** PatientTestsSelection/AddTest -> VisitService.AddTestToVisitAsync -> EnsureExternalQueueRegistrationAsync; ExternalLabManagement -> IExternalLabService
- **اختبارات الوحدة:** ExternalLabServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** لا يوجد اختبار مباشر موثّق لهذه الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = C) غير مكتملة.

### 8.7 تسوية حساب المعمل الخارجي (Settle External Lab Account)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: ExternalLabQueue/ShipmentManifest/ExternalLabSettlement/VisitTest | Service: VisitService.cs + ExternalLabService.cs + ExternalSettlementService.cs | VM: ExternalLabManagementViewModel.cs + PatientTestsSelectionViewModel.cs | View: ExternalLabManagementView.xaml + PatientTestsSelectionView.xaml
- **ربط الاستدعاء بين الطبقات:** PatientTestsSelection/AddTest -> VisitService.AddTestToVisitAsync -> EnsureExternalQueueRegistrationAsync; ExternalLabManagement -> IExternalLabService
- **اختبارات الوحدة:** ExternalLabServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 9.1 توزيع المرضى حسب الجنس (Patient Count by Gender)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/Invoice/Referral + UserPerformanceRow (ViewModels/UiModels.cs) | Service: StatisticsService.cs + UserProductivityService.cs | VM: StatisticsViewModel.cs | View: StatisticsView.xaml
- **ربط الاستدعاء بين الطبقات:** StatisticsViewModel.LoadAsync -> IStatisticsService + IUserProductivityService
- **اختبارات الوحدة:** StatisticsServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 9.2 توزيع المرضى حسب الشهر (Patient Count by Month)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/Invoice/Referral + UserPerformanceRow (ViewModels/UiModels.cs) | Service: StatisticsService.cs + UserProductivityService.cs | VM: StatisticsViewModel.cs | View: StatisticsView.xaml
- **ربط الاستدعاء بين الطبقات:** StatisticsViewModel.LoadAsync -> IStatisticsService + IUserProductivityService
- **اختبارات الوحدة:** StatisticsServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 9.3 تحليل الطلب على التحاليل (Test Demand Analysis)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/Invoice/Referral + UserPerformanceRow (ViewModels/UiModels.cs) | Service: StatisticsService.cs + UserProductivityService.cs | VM: StatisticsViewModel.cs | View: StatisticsView.xaml
- **ربط الاستدعاء بين الطبقات:** StatisticsViewModel.LoadAsync -> IStatisticsService + IUserProductivityService
- **اختبارات الوحدة:** StatisticsServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 9.4 عدد العينات سنوياً (Sample Count per Year)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/Invoice/Referral + UserPerformanceRow (ViewModels/UiModels.cs) | Service: StatisticsService.cs + UserProductivityService.cs | VM: StatisticsViewModel.cs | View: StatisticsView.xaml
- **ربط الاستدعاء بين الطبقات:** StatisticsViewModel.LoadAsync -> IStatisticsService + IUserProductivityService
- **اختبارات الوحدة:** StatisticsServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 9.5 تحليل مصادر الإحالة (Referral Source Analysis)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/Invoice/Referral + UserPerformanceRow (ViewModels/UiModels.cs) | Service: StatisticsService.cs + UserProductivityService.cs | VM: StatisticsViewModel.cs | View: StatisticsView.xaml
- **ربط الاستدعاء بين الطبقات:** StatisticsViewModel.LoadAsync -> IStatisticsService + IUserProductivityService
- **اختبارات الوحدة:** StatisticsServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 9.6 تقرير إنتاجية المستخدمين (User Productivity Report)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Visit/Invoice/Referral + UserPerformanceRow (ViewModels/UiModels.cs) | Service: StatisticsService.cs + UserProductivityService.cs | VM: StatisticsViewModel.cs | View: StatisticsView.xaml
- **ربط الاستدعاء بين الطبقات:** StatisticsViewModel.LoadAsync -> IStatisticsService + IUserProductivityService
- **اختبارات الوحدة:** StatisticsServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 10.1 إنشاء مستخدم (Create User)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: User/Role/RolePermission/UserRole/AttendanceLog/AuditLog | Service: UserAdminService.cs + AttendanceService.cs + UserActivityService.cs + SystemMonitorService.cs + AuthService.cs | VM: UsersPermissionsViewModel.cs, LoginViewModel.cs, MainViewModel.cs, UserActivityLogViewModel.cs, SystemUsageMonitorViewModel.cs | View: UsersPermissionsView.xaml, LoginView.xaml, MainWindow.xaml, UserActivityLogView.xaml, SystemUsageMonitorView.xaml
- **ربط الاستدعاء بين الطبقات:** LoginViewModel.LoginAsync -> IAuthService.ValidateCredentialsAsync + IAttendanceService.CreateLoginAsync; MainViewModel.LogoutAsync -> AttendanceService.CloseAsync
- **اختبارات الوحدة:** UserAdminServiceTests.cs + AttendanceServiceTests.cs + AppSessionTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 10.2 ضبط الصلاحيات (Set Permissions)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: User/Role/RolePermission/UserRole/AttendanceLog/AuditLog | Service: UserAdminService.cs + AttendanceService.cs + UserActivityService.cs + SystemMonitorService.cs + AuthService.cs | VM: UsersPermissionsViewModel.cs, LoginViewModel.cs, MainViewModel.cs, UserActivityLogViewModel.cs, SystemUsageMonitorViewModel.cs | View: UsersPermissionsView.xaml, LoginView.xaml, MainWindow.xaml, UserActivityLogView.xaml, SystemUsageMonitorView.xaml
- **ربط الاستدعاء بين الطبقات:** LoginViewModel.LoginAsync -> IAuthService.ValidateCredentialsAsync + IAttendanceService.CreateLoginAsync; MainViewModel.LogoutAsync -> AttendanceService.CloseAsync
- **اختبارات الوحدة:** UserAdminServiceTests.cs + AttendanceServiceTests.cs + AppSessionTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 10.3 تعديل بيانات المستخدم (Edit User Data)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: User/Role/RolePermission/UserRole/AttendanceLog/AuditLog | Service: UserAdminService.cs + AttendanceService.cs + UserActivityService.cs + SystemMonitorService.cs + AuthService.cs | VM: UsersPermissionsViewModel.cs, LoginViewModel.cs, MainViewModel.cs, UserActivityLogViewModel.cs, SystemUsageMonitorViewModel.cs | View: UsersPermissionsView.xaml, LoginView.xaml, MainWindow.xaml, UserActivityLogView.xaml, SystemUsageMonitorView.xaml
- **ربط الاستدعاء بين الطبقات:** LoginViewModel.LoginAsync -> IAuthService.ValidateCredentialsAsync + IAttendanceService.CreateLoginAsync; MainViewModel.LogoutAsync -> AttendanceService.CloseAsync
- **اختبارات الوحدة:** UserAdminServiceTests.cs + AttendanceServiceTests.cs + AppSessionTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 10.4 تسجيل الحضور (Record Attendance)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: User/Role/RolePermission/UserRole/AttendanceLog/AuditLog | Service: UserAdminService.cs + AttendanceService.cs + UserActivityService.cs + SystemMonitorService.cs + AuthService.cs | VM: UsersPermissionsViewModel.cs, LoginViewModel.cs, MainViewModel.cs, UserActivityLogViewModel.cs, SystemUsageMonitorViewModel.cs | View: UsersPermissionsView.xaml, LoginView.xaml, MainWindow.xaml, UserActivityLogView.xaml, SystemUsageMonitorView.xaml
- **ربط الاستدعاء بين الطبقات:** LoginViewModel.LoginAsync -> IAuthService.ValidateCredentialsAsync + IAttendanceService.CreateLoginAsync; MainViewModel.LogoutAsync -> AttendanceService.CloseAsync
- **اختبارات الوحدة:** UserAdminServiceTests.cs + AttendanceServiceTests.cs + AppSessionTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 10.5 تسجيل الانصراف (Record Departure)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: User/Role/RolePermission/UserRole/AttendanceLog/AuditLog | Service: UserAdminService.cs + AttendanceService.cs + UserActivityService.cs + SystemMonitorService.cs + AuthService.cs | VM: UsersPermissionsViewModel.cs, LoginViewModel.cs, MainViewModel.cs, UserActivityLogViewModel.cs, SystemUsageMonitorViewModel.cs | View: UsersPermissionsView.xaml, LoginView.xaml, MainWindow.xaml, UserActivityLogView.xaml, SystemUsageMonitorView.xaml
- **ربط الاستدعاء بين الطبقات:** LoginViewModel.LoginAsync -> IAuthService.ValidateCredentialsAsync + IAttendanceService.CreateLoginAsync; MainViewModel.LogoutAsync -> AttendanceService.CloseAsync
- **اختبارات الوحدة:** UserAdminServiceTests.cs + AttendanceServiceTests.cs + AppSessionTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 10.6 عرض سجل نشاط المستخدم (View User Activity Log)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: User/Role/RolePermission/UserRole/AttendanceLog/AuditLog | Service: UserAdminService.cs + AttendanceService.cs + UserActivityService.cs + SystemMonitorService.cs + AuthService.cs | VM: UsersPermissionsViewModel.cs, LoginViewModel.cs, MainViewModel.cs, UserActivityLogViewModel.cs, SystemUsageMonitorViewModel.cs | View: UsersPermissionsView.xaml, LoginView.xaml, MainWindow.xaml, UserActivityLogView.xaml, SystemUsageMonitorView.xaml
- **ربط الاستدعاء بين الطبقات:** LoginViewModel.LoginAsync -> IAuthService.ValidateCredentialsAsync + IAttendanceService.CreateLoginAsync; MainViewModel.LogoutAsync -> AttendanceService.CloseAsync
- **اختبارات الوحدة:** UserAdminServiceTests.cs + AttendanceServiceTests.cs + AppSessionTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 10.7 مراقبة استخدام النظام (Monitor System Usage)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: User/Role/RolePermission/UserRole/AttendanceLog/AuditLog | Service: UserAdminService.cs + AttendanceService.cs + UserActivityService.cs + SystemMonitorService.cs + AuthService.cs | VM: UsersPermissionsViewModel.cs, LoginViewModel.cs, MainViewModel.cs, UserActivityLogViewModel.cs, SystemUsageMonitorViewModel.cs | View: UsersPermissionsView.xaml, LoginView.xaml, MainWindow.xaml, UserActivityLogView.xaml, SystemUsageMonitorView.xaml
- **ربط الاستدعاء بين الطبقات:** LoginViewModel.LoginAsync -> IAuthService.ValidateCredentialsAsync + IAttendanceService.CreateLoginAsync; MainViewModel.LogoutAsync -> AttendanceService.CloseAsync
- **اختبارات الوحدة:** UserAdminServiceTests.cs + AttendanceServiceTests.cs + AppSessionTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 10.8 تسجيل الخروج (Logout)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: User/Role/RolePermission/UserRole/AttendanceLog/AuditLog | Service: UserAdminService.cs + AttendanceService.cs + UserActivityService.cs + SystemMonitorService.cs + AuthService.cs | VM: UsersPermissionsViewModel.cs, LoginViewModel.cs, MainViewModel.cs, UserActivityLogViewModel.cs, SystemUsageMonitorViewModel.cs | View: UsersPermissionsView.xaml, LoginView.xaml, MainWindow.xaml, UserActivityLogView.xaml, SystemUsageMonitorView.xaml
- **ربط الاستدعاء بين الطبقات:** LoginViewModel.LoginAsync -> IAuthService.ValidateCredentialsAsync + IAttendanceService.CreateLoginAsync; MainViewModel.LogoutAsync -> AttendanceService.CloseAsync
- **اختبارات الوحدة:** UserAdminServiceTests.cs + AttendanceServiceTests.cs + AppSessionTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 11.1 تسجيل الحضور (Clock In)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: AttendanceLog/ShiftSchedule | Service: AttendanceService.cs + TardinessService.cs | VM: AttendanceReportViewModel.cs + LoginViewModel.cs + MainViewModel.cs | View: AttendanceReportView.xaml + LoginView.xaml + MainWindow.xaml
- **ربط الاستدعاء بين الطبقات:** Login (Clock-in) + Main logout (Clock-out) + AttendanceReportViewModel.GenerateReportAsync -> TardinessService.GetPunctualityReportAsync
- **اختبارات الوحدة:** AttendanceServiceTests.cs + TardinessServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 11.2 تسجيل الانصراف (Clock Out)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: AttendanceLog/ShiftSchedule | Service: AttendanceService.cs + TardinessService.cs | VM: AttendanceReportViewModel.cs + LoginViewModel.cs + MainViewModel.cs | View: AttendanceReportView.xaml + LoginView.xaml + MainWindow.xaml
- **ربط الاستدعاء بين الطبقات:** Login (Clock-in) + Main logout (Clock-out) + AttendanceReportViewModel.GenerateReportAsync -> TardinessService.GetPunctualityReportAsync
- **اختبارات الوحدة:** AttendanceServiceTests.cs + TardinessServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 11.3 حساب ساعات العمل (Calculate Working Hours)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** حساب الساعات موجود داخل TardinessService.GetPunctualityReportAsync (TotalHours من Login/Logout) مع تغطية اختبارية جزئية
- **ربط الاستدعاء بين الطبقات:** Login (Clock-in) + Main logout (Clock-out) + AttendanceReportViewModel.GenerateReportAsync -> TardinessService.GetPunctualityReportAsync
- **اختبارات الوحدة:** AttendanceServiceTests.cs + TardinessServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 11.4 حساب التأخير (Calculate Tardiness)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: AttendanceLog/ShiftSchedule | Service: AttendanceService.cs + TardinessService.cs | VM: AttendanceReportViewModel.cs + LoginViewModel.cs + MainViewModel.cs | View: AttendanceReportView.xaml + LoginView.xaml + MainWindow.xaml
- **ربط الاستدعاء بين الطبقات:** Login (Clock-in) + Main logout (Clock-out) + AttendanceReportViewModel.GenerateReportAsync -> TardinessService.GetPunctualityReportAsync
- **اختبارات الوحدة:** AttendanceServiceTests.cs + TardinessServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 11.5 إنشاء تقرير الحضور (Generate Attendance Report)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: AttendanceLog/ShiftSchedule | Service: AttendanceService.cs + TardinessService.cs | VM: AttendanceReportViewModel.cs + LoginViewModel.cs + MainViewModel.cs | View: AttendanceReportView.xaml + LoginView.xaml + MainWindow.xaml
- **ربط الاستدعاء بين الطبقات:** Login (Clock-in) + Main logout (Clock-out) + AttendanceReportViewModel.GenerateReportAsync -> TardinessService.GetPunctualityReportAsync
- **اختبارات الوحدة:** AttendanceServiceTests.cs + TardinessServiceTests.cs + FunctionCoverageGapTests.cs + AdditionalFunctionalityTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 12.1 إنشاء جهة تعاقد (Create Contract Entity)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Referral/PriceList/Visit/ContractInvoice/Physician | Service: TestCatalogService.cs + PriceResolutionService.cs + ContractInvoiceService.cs + PhysicianService.cs | VM: ReferralsViewModel.cs, PriceListsViewModel.cs, PatientTestsSelectionViewModel.cs, ContractInvoiceViewModel.cs | View: ReferralsView.xaml, PriceListsView.xaml, PatientTestsSelectionView.xaml, ContractInvoiceView.xaml
- **ربط الاستدعاء بين الطبقات:** PriceListsViewModel.Save/Update -> CreatePriceListAsync(Update) with ReferralId; PatientTestsSelection.CreateVisitAsync stores ReferralId
- **اختبارات الوحدة:** ContractInvoiceServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 12.2 ربط قائمة أسعار بالجهة (Link Price List to Entity)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Referral/PriceList/Visit/ContractInvoice/Physician | Service: TestCatalogService.cs + PriceResolutionService.cs + ContractInvoiceService.cs + PhysicianService.cs | VM: ReferralsViewModel.cs, PriceListsViewModel.cs, PatientTestsSelectionViewModel.cs, ContractInvoiceViewModel.cs | View: ReferralsView.xaml, PriceListsView.xaml, PatientTestsSelectionView.xaml, ContractInvoiceView.xaml
- **ربط الاستدعاء بين الطبقات:** PriceListsViewModel.Save/Update -> CreatePriceListAsync(Update) with ReferralId; PatientTestsSelection.CreateVisitAsync stores ReferralId
- **اختبارات الوحدة:** ContractInvoiceServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 12.3 ضبط خصم الجهة (Set Entity Discount)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=جزئي, View=موجود.
- **الأدلة التقنية:** خصائص الخصم/العمولة موجودة في Referral model (Entities.cs) وتظهر في اختبارات AdditionalFunctionality، لكن شاشة ReferralsViewModel لا توفر حقول تعديلها مباشرة
- **ربط الاستدعاء بين الطبقات:** PriceListsViewModel.Save/Update -> CreatePriceListAsync(Update) with ReferralId; PatientTestsSelection.CreateVisitAsync stores ReferralId
- **اختبارات الوحدة:** ContractInvoiceServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 12.4 ضبط عمولة الجهة (Set Entity Commission)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=جزئي, View=موجود.
- **الأدلة التقنية:** خصائص الخصم/العمولة موجودة في Referral model (Entities.cs) وتظهر في اختبارات AdditionalFunctionality، لكن شاشة ReferralsViewModel لا توفر حقول تعديلها مباشرة
- **ربط الاستدعاء بين الطبقات:** PriceListsViewModel.Save/Update -> CreatePriceListAsync(Update) with ReferralId; PatientTestsSelection.CreateVisitAsync stores ReferralId
- **اختبارات الوحدة:** ContractInvoiceServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 12.5 ربط المريض بجهة التعاقد (Assign Patient to Contract)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Referral/PriceList/Visit/ContractInvoice/Physician | Service: TestCatalogService.cs + PriceResolutionService.cs + ContractInvoiceService.cs + PhysicianService.cs | VM: ReferralsViewModel.cs, PriceListsViewModel.cs, PatientTestsSelectionViewModel.cs, ContractInvoiceViewModel.cs | View: ReferralsView.xaml, PriceListsView.xaml, PatientTestsSelectionView.xaml, ContractInvoiceView.xaml
- **ربط الاستدعاء بين الطبقات:** PriceListsViewModel.Save/Update -> CreatePriceListAsync(Update) with ReferralId; PatientTestsSelection.CreateVisitAsync stores ReferralId
- **اختبارات الوحدة:** ContractInvoiceServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 12.6 إضافة طبيب محيل (Add Referring Physician)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=جزئي, View=غير موجود.
- **الأدلة التقنية:** Service موجود (PhysicianService.CreateAsync / TestCatalogService.CreatePhysicianAsync)، لكن لا يوجد View مسجّل في ViewModelFactory/MainNavigation لطبيب محيل
- **ربط الاستدعاء بين الطبقات:** PriceListsViewModel.Save/Update -> CreatePriceListAsync(Update) with ReferralId; PatientTestsSelection.CreateVisitAsync stores ReferralId
- **اختبارات الوحدة:** ContractInvoiceServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** لا يوجد اختبار مباشر موثّق لهذه الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = C) غير مكتملة.

### 12.7 ضبط قائمة أسعار الطبيب (Set Physician Price List)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=جزئي, View=غير موجود.
- **الأدلة التقنية:** PriceResolutionService يدعم تسعير الطبيب، لكن لا يوجد UI صريح لربط الطبيب بقائمة أسعار داخل التنقل الحالي
- **ربط الاستدعاء بين الطبقات:** PriceListsViewModel.Save/Update -> CreatePriceListAsync(Update) with ReferralId; PatientTestsSelection.CreateVisitAsync stores ReferralId
- **اختبارات الوحدة:** ContractInvoiceServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = C) غير مكتملة.

### 12.8 إصدار فاتورة التعاقد (Generate Contract Invoice)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Referral/PriceList/Visit/ContractInvoice/Physician | Service: TestCatalogService.cs + PriceResolutionService.cs + ContractInvoiceService.cs + PhysicianService.cs | VM: ReferralsViewModel.cs, PriceListsViewModel.cs, PatientTestsSelectionViewModel.cs, ContractInvoiceViewModel.cs | View: ReferralsView.xaml, PriceListsView.xaml, PatientTestsSelectionView.xaml, ContractInvoiceView.xaml
- **ربط الاستدعاء بين الطبقات:** PriceListsViewModel.Save/Update -> CreatePriceListAsync(Update) with ReferralId; PatientTestsSelection.CreateVisitAsync stores ReferralId
- **اختبارات الوحدة:** ContractInvoiceServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 12.9 تسوية حساب التعاقد (Settle Contract Account)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Referral/PriceList/Visit/ContractInvoice/Physician | Service: TestCatalogService.cs + PriceResolutionService.cs + ContractInvoiceService.cs + PhysicianService.cs | VM: ReferralsViewModel.cs, PriceListsViewModel.cs, PatientTestsSelectionViewModel.cs, ContractInvoiceViewModel.cs | View: ReferralsView.xaml, PriceListsView.xaml, PatientTestsSelectionView.xaml, ContractInvoiceView.xaml
- **ربط الاستدعاء بين الطبقات:** PriceListsViewModel.Save/Update -> CreatePriceListAsync(Update) with ReferralId; PatientTestsSelection.CreateVisitAsync stores ReferralId
- **اختبارات الوحدة:** ContractInvoiceServiceTests.cs + AdditionalFunctionalityTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 13.1 ضبط هوامش التقرير (Set Report Margins)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Setting/SystemSetting | Service: SystemSettingsService.cs + SettingsService.cs + BackupRestoreService.cs | VM: SystemSettingsViewModel.cs + SettingsViewModel.cs + BackupRestoreViewModel.cs | View: SystemSettingsView.xaml + SettingsView.xaml + BackupRestoreView.xaml
- **ربط الاستدعاء بين الطبقات:** SystemSettingsViewModel.SaveProfileAsync/ChangeMasterPasswordAsync -> ISystemSettingsService; SettingsViewModel.SaveMargin/Printer -> ISettingsService
- **اختبارات الوحدة:** SystemSettingsServiceTests.cs + SettingsServiceTests.cs + SystemSettingsViewModelTests.cs + BackupRestoreServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 13.2 ضبط حجم الورق (Set Paper Size)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Setting/SystemSetting | Service: SystemSettingsService.cs + SettingsService.cs + BackupRestoreService.cs | VM: SystemSettingsViewModel.cs + SettingsViewModel.cs + BackupRestoreViewModel.cs | View: SystemSettingsView.xaml + SettingsView.xaml + BackupRestoreView.xaml
- **ربط الاستدعاء بين الطبقات:** SystemSettingsViewModel.SaveProfileAsync/ChangeMasterPasswordAsync -> ISystemSettingsService; SettingsViewModel.SaveMargin/Printer -> ISettingsService
- **اختبارات الوحدة:** SystemSettingsServiceTests.cs + SettingsServiceTests.cs + SystemSettingsViewModelTests.cs + BackupRestoreServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 13.3 تكوين الترويسة والتذييل (Configure Header/Footer)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Setting/SystemSetting | Service: SystemSettingsService.cs + SettingsService.cs + BackupRestoreService.cs | VM: SystemSettingsViewModel.cs + SettingsViewModel.cs + BackupRestoreViewModel.cs | View: SystemSettingsView.xaml + SettingsView.xaml + BackupRestoreView.xaml
- **ربط الاستدعاء بين الطبقات:** SystemSettingsViewModel.SaveProfileAsync/ChangeMasterPasswordAsync -> ISystemSettingsService; SettingsViewModel.SaveMargin/Printer -> ISettingsService
- **اختبارات الوحدة:** SystemSettingsServiceTests.cs + SettingsServiceTests.cs + SystemSettingsViewModelTests.cs + BackupRestoreServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 13.4 ضبط نوع الحساب الافتراضي (Set Default Account Type)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Setting/SystemSetting | Service: SystemSettingsService.cs + SettingsService.cs + BackupRestoreService.cs | VM: SystemSettingsViewModel.cs + SettingsViewModel.cs + BackupRestoreViewModel.cs | View: SystemSettingsView.xaml + SettingsView.xaml + BackupRestoreView.xaml
- **ربط الاستدعاء بين الطبقات:** SystemSettingsViewModel.SaveProfileAsync/ChangeMasterPasswordAsync -> ISystemSettingsService; SettingsViewModel.SaveMargin/Printer -> ISettingsService
- **اختبارات الوحدة:** SystemSettingsServiceTests.cs + SettingsServiceTests.cs + SystemSettingsViewModelTests.cs + BackupRestoreServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 13.5 تكوين الطابعات (Configure Printers)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Setting/SystemSetting | Service: SystemSettingsService.cs + SettingsService.cs + BackupRestoreService.cs | VM: SystemSettingsViewModel.cs + SettingsViewModel.cs + BackupRestoreViewModel.cs | View: SystemSettingsView.xaml + SettingsView.xaml + BackupRestoreView.xaml
- **ربط الاستدعاء بين الطبقات:** SystemSettingsViewModel.SaveProfileAsync/ChangeMasterPasswordAsync -> ISystemSettingsService; SettingsViewModel.SaveMargin/Printer -> ISettingsService
- **اختبارات الوحدة:** SystemSettingsServiceTests.cs + SettingsServiceTests.cs + SystemSettingsViewModelTests.cs + BackupRestoreServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 13.6 إعدادات الفاتورة (Set Invoice Settings)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Setting/SystemSetting | Service: SystemSettingsService.cs + SettingsService.cs + BackupRestoreService.cs | VM: SystemSettingsViewModel.cs + SettingsViewModel.cs + BackupRestoreViewModel.cs | View: SystemSettingsView.xaml + SettingsView.xaml + BackupRestoreView.xaml
- **ربط الاستدعاء بين الطبقات:** SystemSettingsViewModel.SaveProfileAsync/ChangeMasterPasswordAsync -> ISystemSettingsService; SettingsViewModel.SaveMargin/Printer -> ISettingsService
- **اختبارات الوحدة:** SystemSettingsServiceTests.cs + SettingsServiceTests.cs + SystemSettingsViewModelTests.cs + BackupRestoreServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 13.7 تكوين النسخ الاحتياطي (Configure Backup)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** BackupRestoreService.BackupAsync/RestoreAsync/ListBackupsAsync + BackupRestoreViewModel commands + BackupRestoreView.xaml
- **ربط الاستدعاء بين الطبقات:** SystemSettingsViewModel.SaveProfileAsync/ChangeMasterPasswordAsync -> ISystemSettingsService; SettingsViewModel.SaveMargin/Printer -> ISettingsService
- **اختبارات الوحدة:** SystemSettingsServiceTests.cs + SettingsServiceTests.cs + SystemSettingsViewModelTests.cs + BackupRestoreServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

### 13.8 ضبط كلمة مرور النظام (Set System Password)
- **تحليل الطبقات:** Model=موجود, Service=موجود, ViewModel=موجود, View=موجود.
- **الأدلة التقنية:** Model: Setting/SystemSetting | Service: SystemSettingsService.cs + SettingsService.cs + BackupRestoreService.cs | VM: SystemSettingsViewModel.cs + SettingsViewModel.cs + BackupRestoreViewModel.cs | View: SystemSettingsView.xaml + SettingsView.xaml + BackupRestoreView.xaml
- **ربط الاستدعاء بين الطبقات:** SystemSettingsViewModel.SaveProfileAsync/ChangeMasterPasswordAsync -> ISystemSettingsService; SettingsViewModel.SaveMargin/Printer -> ISettingsService
- **اختبارات الوحدة:** SystemSettingsServiceTests.cs + SettingsServiceTests.cs + SystemSettingsViewModelTests.cs + BackupRestoreServiceTests.cs + FunctionCoverageGapTests.cs
- **Assertions/Breakability:** توجد Assertions، لكن تغطية الفشل/الكسر غير شاملة لكل مسارات الوظيفة.
- **ملاحظة التحقق:** التصنيف النهائي = B) مكتملة جزئياً.

## (3) تحليل شامل للاختبارات

- الوظائف المصنفة Full Stack Complete (A): 0
- الوظائف المصنفة مكتملة جزئياً (B): 88
- الوظائف المصنفة غير مكتملة (C): 9
- الوظائف المصنفة غير موجودة (D): 0

- وظائف باختبارات قوية: 0
- وظائف باختبارات جزئية: 89
- وظائف بدون اختبارات مباشرة: 8

### ملاحظة الاختبارات الوهمية
- تم رصد بعض اختبارات تعتمد تعليق ربط الوظيفة داخل التعليق (`// x.y`) أو تحقق خصائص Model دون تدفق UI-VM-Service كامل؛ تم احتسابها كتغطية جزئية وليس تغطية كاملة.

## (4) قاعدة الحسم النهائية

- أي وظيفة لا تحتوي تنفيذًا طبقيًا كاملاً ومربوطًا + اختبار وحدة قوي قابل للكسر (success/failure) صُنِّفت كـ **غير مكتملة** وفق القاعدة المطلوبة.

## مراجع الأدلة الأساسية (ملفات تم التحقق منها)
- `Open_lab/Models/Entities.cs`
- `Open_lab/Data/OpenLabDbContext.cs`
- `Open_lab/Services/*.cs` (خصوصًا: PatientService, VisitService, InvoiceService, TestCatalogService, ResultsService, ExternalLabService, ExternalSettlementService, StatisticsService, UserAdminService, SystemSettingsService)
- `Open_lab/ViewModels/*.cs` (خصوصًا: PatientRegistrationViewModel, PatientTestsSelectionViewModel, PatientBillingViewModel, ResultsEntryViewModel, StatisticsViewModel, UsersPermissionsViewModel, ExternalLabManagementViewModel, SystemSettingsViewModel)
- `Open_lab/Views/*.xaml`
- `Open_lab.Tests/Services/*.cs` و `Open_lab.Tests/ViewModels/*.cs` (خصوصًا: FunctionCoverageGapTests, AdditionalFunctionalityTests)






