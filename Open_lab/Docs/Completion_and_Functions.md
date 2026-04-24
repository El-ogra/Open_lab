# تقرير الاكتمال وتحليل الوظائف والاختبارات

## 1) مقدمة تحليلية

- تم تنفيذ التدقيق بالاعتماد على الكود الفعلي في المشروع وملفات الاختبارات فقط، دون افتراضات خارجية.
- مرجع الوظائف: `Docs/Open_lab_Modules_Documentation.md` (عدد الوظائف المستخرجة فعليًا: **97**).
- مرجع الربط الطبقي: `Open_lab.Tests/Services/DocumentationFunctionCoverageTests.cs` (ربط كل وظيفة بـ Model + Service + ViewModel).
- عدد حالات الاختبار المدققة: **386** (مطابق لنتيجة التشغيل: 386).
- التقييم المبدئي: البنية الخلفية واسعة التغطية، مع وجود نسبة معتبرة من اختبارات ضعيفة تحتاج تقوية لتقليل مخاطر الانحدار.

## 2) تحليل الوظائف (97 وظيفة)

### الموديول 1: موديول إدارة المرضى

#### 1.1 - إضافة مريض جديد
- Model: Present (Patient)
- ViewModel: Present (PatientRegistrationViewModel)
- Service: Present (PatientService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 1.2 - تعديل بيانات مريض
- Model: Present (Patient)
- ViewModel: Present (PatientRegistrationViewModel)
- Service: Present (PatientService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 1.3 - إضافة تحاليل للمريض
- Model: Present (Visit)
- ViewModel: Present (PatientTestsSelectionViewModel)
- Service: Present (VisitService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 3 | Real: 2 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 1.4 - حذف تحاليل
- Model: Present (VisitTest)
- ViewModel: Present (PatientTestsSelectionViewModel)
- Service: Present (VisitService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 2 | Real: 1 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 1.5 - البحث عن مريض
- Model: Present (Patient)
- ViewModel: Present (PatientSearchViewModel)
- Service: Present (PatientSearchService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 7 | Real: 4 | Weak: 3 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 1.6 - عرض التاريخ المرضي
- Model: Present (MedicalHistory)
- ViewModel: Present (PatientHistoryViewModel)
- Service: Present (VisitService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 1.7 - إضافة تاريخ طبي
- Model: Present (MedicalHistory)
- ViewModel: Present (PatientRegistrationViewModel)
- Service: Present (PatientService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 1.8 - إضافة مجموعة تحاليل
- Model: Present (CustomGroup)
- ViewModel: Present (PatientTestsSelectionViewModel)
- Service: Present (VisitService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

### الموديول 10: موديول إدارة المستخدمين والصلاحيات

#### 10.1 - إنشاء مستخدم
- Model: Present (User)
- ViewModel: Present (UsersPermissionsViewModel)
- Service: Present (UserAdminService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 10.2 - ضبط الصلاحيات
- Model: Present (RolePermission)
- ViewModel: Present (UsersPermissionsViewModel)
- Service: Present (UserAdminService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 10.3 - تعديل بيانات المستخدم
- Model: Present (User)
- ViewModel: Present (UsersPermissionsViewModel)
- Service: Present (UserAdminService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 10.4 - تسجيل الحضور
- Model: Present (AttendanceLog)
- ViewModel: Present (AttendanceLogViewModel)
- Service: Present (AttendanceService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 10.5 - تسجيل الانصراف
- Model: Present (AttendanceLog)
- ViewModel: Present (AttendanceLogViewModel)
- Service: Present (AttendanceService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 10.6 - عرض سجل نشاط المستخدم
- Model: Present (AuditLog)
- ViewModel: Present (UserActivityLogViewModel)
- Service: Present (UserActivityService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 10.7 - مراقبة استخدام النظام
- Model: Present (AttendanceLog)
- ViewModel: Present (SystemUsageMonitorViewModel)
- Service: Present (SystemMonitorService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 10.8 - تسجيل الخروج
- Model: Present (User)
- ViewModel: Present (LoginViewModel)
- Service: Present (AuthService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 8 | Real: 3 | Weak: 5 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

### الموديول 11: موديول الحضور والانصراف

#### 11.1 - تسجيل الحضور
- Model: Present (AttendanceLog)
- ViewModel: Present (AttendanceLogViewModel)
- Service: Present (AttendanceService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 11.2 - تسجيل الانصراف
- Model: Present (AttendanceLog)
- ViewModel: Present (AttendanceLogViewModel)
- Service: Present (AttendanceService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 11.3 - حساب ساعات العمل
- Model: Present (AttendanceLog)
- ViewModel: Present (AttendanceReportViewModel)
- Service: Present (AttendanceService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 2 | Real: 1 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 11.4 - حساب التأخير
- Model: Present (AttendanceLog)
- ViewModel: Present (AttendanceReportViewModel)
- Service: Present (TardinessService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 11.5 - إنشاء تقرير الحضور
- Model: Present (AttendanceLog)
- ViewModel: Present (AttendanceReportViewModel)
- Service: Present (AttendanceService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

### الموديول 12: موديول جهات التعاقد والإحالة

#### 12.1 - إنشاء جهة تعاقد
- Model: Present (Referral)
- ViewModel: Present (ReferralsViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 22 | Real: 22 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 12.2 - ربط قائمة أسعار بالجهة
- Model: Present (PriceList)
- ViewModel: Present (PriceListsViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 22 | Real: 22 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 12.3 - ضبط خصم الجهة
- Model: Present (Referral)
- ViewModel: Present (ReferralsViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 12.4 - ضبط عمولة الجهة
- Model: Present (Referral)
- ViewModel: Present (ReferralsViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 12.5 - ربط المريض بجهة التعاقد
- Model: Present (Visit)
- ViewModel: Present (PatientTestsSelectionViewModel)
- Service: Present (VisitService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 12.6 - إضافة طبيب محيل
- Model: Present (Physician)
- ViewModel: Present (PhysicianViewModel)
- Service: Present (PhysicianService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 9 | Real: 8 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 12.7 - ضبط قائمة أسعار الطبيب
- Model: Present (Physician)
- ViewModel: Present (PhysicianViewModel)
- Service: Present (PhysicianService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 9 | Real: 8 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 12.8 - إصدار فاتورة التعاقد
- Model: Present (ContractInvoice)
- ViewModel: Present (ContractInvoiceViewModel)
- Service: Present (ContractInvoiceService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 12.9 - تسوية حساب التعاقد
- Model: Present (ContractInvoice)
- ViewModel: Present (ContractInvoiceViewModel)
- Service: Present (ContractInvoiceService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

### الموديول 13: موديول إعدادات النظام والتكوين

#### 13.1 - ضبط هوامش التقرير
- Model: Present (Setting)
- ViewModel: Present (SettingsViewModel)
- Service: Present (SettingsService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 13.2 - ضبط حجم الورق
- Model: Present (Setting)
- ViewModel: Present (SettingsViewModel)
- Service: Present (SettingsService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 3 | Real: 1 | Weak: 2 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 13.3 - تكوين الترويسة والتذييل
- Model: Present (Setting)
- ViewModel: Present (SettingsViewModel)
- Service: Present (SettingsService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 13.4 - ضبط نوع الحساب الافتراضي
- Model: Present (Setting)
- ViewModel: Present (SettingsViewModel)
- Service: Present (SettingsService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 3 | Real: 1 | Weak: 2 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 13.5 - تكوين الطابعات
- Model: Present (Setting)
- ViewModel: Present (SettingsViewModel)
- Service: Present (SettingsService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 13.6 - إعدادات الفاتورة
- Model: Present (Setting)
- ViewModel: Present (SettingsViewModel)
- Service: Present (SettingsService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 13.7 - تكوين النسخ الاحتياطي
- Model: Present (Setting)
- ViewModel: Present (BackupRestoreViewModel)
- Service: Present (BackupRestoreService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 13.8 - ضبط كلمة مرور النظام
- Model: Present (Setting)
- ViewModel: Present (SystemSettingsViewModel)
- Service: Present (SystemSettingsService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 3 | Real: 3 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

### الموديول 2: موديول المحاسبة والمالية

#### 2.1 - حساب الإجمالي
- Model: Present (Invoice)
- ViewModel: Present (PatientBillingViewModel)
- Service: Present (InvoiceService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 2.2 - تطبيق خصم
- Model: Present (Invoice)
- ViewModel: Present (PatientBillingViewModel)
- Service: Present (InvoiceService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 3 | Real: 2 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 2.3 - تسجيل دفعة
- Model: Present (Invoice)
- ViewModel: Present (PatientBillingViewModel)
- Service: Present (InvoiceService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 33 | Real: 26 | Weak: 7 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 2.4 - تصفية الحساب
- Model: Present (Invoice)
- ViewModel: Present (PatientBillingViewModel)
- Service: Present (InvoiceService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 33 | Real: 26 | Weak: 7 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 2.5 - تعديل دفعة
- Model: Present (Invoice)
- ViewModel: Present (PatientBillingViewModel)
- Service: Present (InvoiceService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 2.6 - حذف دفعة
- Model: Present (Invoice)
- ViewModel: Present (PatientBillingViewModel)
- Service: Present (InvoiceService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 2.7 - إضافة رسوم إضافية
- Model: Present (Invoice)
- ViewModel: Present (PatientBillingViewModel)
- Service: Present (InvoiceService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 33 | Real: 26 | Weak: 7 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 2.8 - إصدار فاتورة
- Model: Present (Invoice)
- ViewModel: Present (PatientBillingViewModel)
- Service: Present (InvoiceService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 33 | Real: 26 | Weak: 7 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 2.9 - كشف حساب المريض
- Model: Present (Invoice)
- ViewModel: Present (PatientBillingViewModel)
- Service: Present (InvoiceService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 2.10 - تقرير الجرد المالي
- Model: Present (Invoice)
- ViewModel: Present (AccountsTreasuryViewModel)
- Service: Present (AccountsTreasuryService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 2.11 - جرد مالي للفرع
- Model: Present (Branch)
- ViewModel: Present (AccountsTreasuryViewModel)
- Service: Present (AccountsTreasuryService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 2 | Real: 1 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 2.12 - حساب الأطباء
- Model: Present (Physician)
- ViewModel: Present (AccountsTreasuryViewModel)
- Service: Present (AccountsTreasuryService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 2.13 - تصفية حسابات المعامل الخارجية
- Model: Present (ExternalLabSettlement)
- ViewModel: Present (ExternalLabManagementViewModel)
- Service: Present (ExternalSettlementService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 3 | Real: 1 | Weak: 2 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

### الموديول 3: موديول إدارة التحاليل والأسعار

#### 3.1 - إضافة تحليل جديد
- Model: Present (Test)
- ViewModel: Present (TestCatalogViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 3.2 - تعديل بيانات تحليل
- Model: Present (Test)
- ViewModel: Present (TestCatalogViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 3.3 - تحديد القيم المرجعية
- Model: Present (Test)
- ViewModel: Present (TestCatalogViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 4 | Real: 4 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 3.4 - إضافة تعليقات القيم المرتفعة/المنخفضة
- Model: Present (Test)
- ViewModel: Present (TestCatalogViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 2 | Real: 1 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 3.5 - إنشاء مجموعة مخصصة
- Model: Present (Test)
- ViewModel: Present (TestCatalogViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 2 | Real: 1 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 3.6 - إضافة تعليقات التحليل
- Model: Present (Test)
- ViewModel: Present (TestCatalogViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 3.7 - إنشاء قائمة أسعار
- Model: Present (Test)
- ViewModel: Present (TestCatalogViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 3.8 - تحديث الأسعار
- Model: Present (Test)
- ViewModel: Present (TestCatalogViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 2 | Real: 1 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 3.9 - تحديد تحليل كخارجي
- Model: Present (Test)
- ViewModel: Present (TestCatalogViewModel)
- Service: Present (TestCatalogService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 2 | Real: 1 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

### الموديول 4: موديول إدخال النتائج والتقارير

#### 4.1 - إدخال نتائج التحاليل
- Model: Present (ResultValue)
- ViewModel: Present (ResultsEntryViewModel)
- Service: Present (ResultsService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 4.2 - حفظ النتائج
- Model: Present (ResultValue)
- ViewModel: Present (ResultsEntryViewModel)
- Service: Present (ResultsService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 4.3 - تعديل النتائج
- Model: Present (ResultValue)
- ViewModel: Present (ResultsEntryViewModel)
- Service: Present (ResultsService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 4.4 - إنشاء تقرير مركّب
- Model: Present (ResultValue)
- ViewModel: Present (CombinedReportViewModel)
- Service: Present (ReportService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 3 | Real: 3 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 4.5 - ترتيب التقرير
- Model: Present (Test)
- ViewModel: Present (ReportViewerViewModel)
- Service: Present (ReportService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 3 | Real: 2 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 4.6 - معاينة التقرير
- Model: Present (ResultValue)
- ViewModel: Present (ReportViewerViewModel)
- Service: Present (ReportService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 4.7 - طباعة التقرير
- Model: Present (ResultValue)
- ViewModel: Present (ReportViewerViewModel)
- Service: Present (PrintService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 4.8 - طباعة تقرير فارغ
- Model: Present (ResultValue)
- ViewModel: Present (BlankReportViewModel)
- Service: Present (PrintService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 4.9 - المقارنة مع التاريخ
- Model: Present (ResultValue)
- ViewModel: Present (CompareWithHistoryViewModel)
- Service: Present (CompareWithHistoryService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

### الموديول 5: موديول المزارع والحساسية

#### 5.1 - إدخال بيانات المزرعة
- Model: Present (Culture)
- ViewModel: Present (CultureSensitivityViewModel)
- Service: Present (CultureSensitivityService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 5.2 - إضافة مضادات حيوية
- Model: Present (Culture)
- ViewModel: Present (CultureSensitivityViewModel)
- Service: Present (CultureSensitivityService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 5.3 - تسجيل الحساسية
- Model: Present (Culture)
- ViewModel: Present (CultureSensitivityViewModel)
- Service: Present (CultureSensitivityService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 5.4 - تصنيف الحساسية
- Model: Present (Culture)
- ViewModel: Present (CultureSensitivityViewModel)
- Service: Present (CultureSensitivityService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 24 | Real: 13 | Weak: 11 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 5.5 - تصفية مضادات الحوامل
- Model: Present (Culture)
- ViewModel: Present (CultureSensitivityViewModel)
- Service: Present (CultureSensitivityService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 5.6 - تصفية مضادات الأطفال
- Model: Present (Culture)
- ViewModel: Present (CultureSensitivityViewModel)
- Service: Present (CultureSensitivityService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 5.7 - طباعة تقرير المزرعة
- Model: Present (Culture)
- ViewModel: Present (CultureSensitivityViewModel)
- Service: Present (CultureSensitivityService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

### الموديول 6: موديول سحب العينات

#### 6.1 - تسجيل سحب العينة
- Model: Present (SampleCollection)
- ViewModel: Present (SampleCollectionViewModel)
- Service: Present (SampleCollectionService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 6.2 - تسجيل فصل العينة
- Model: Present (SampleCollection)
- ViewModel: Present (SampleCollectionViewModel)
- Service: Present (SampleCollectionService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 6.3 - متابعة حالة العينة
- Model: Present (SampleCollection)
- ViewModel: Present (SampleCollectionViewModel)
- Service: Present (SampleTrackingService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 6.4 - تعليم العينات الخارجية
- Model: Present (SampleCollection)
- ViewModel: Present (SampleCollectionViewModel)
- Service: Present (SampleCollectionService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 18 | Real: 15 | Weak: 3 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

### الموديول 7: موديول أوراق العمل

#### 7.1 - إنشاء ورقة عمل المرضى
- Model: Present (Visit)
- ViewModel: Present (WorkSheetByPatientViewModel)
- Service: Present (WorksheetService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 7.2 - إنشاء ورقة عمل التحليل
- Model: Present (VisitTest)
- ViewModel: Present (WorkSheetByTestViewModel)
- Service: Present (WorksheetService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 7.3 - إنشاء ورقة عمل المجموعة
- Model: Present (TestGroup)
- ViewModel: Present (GroupWorksheetViewModel)
- Service: Present (GroupWorksheetService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

#### 7.4 - سجل تصنيف التحاليل
- Model: Present (TestConsumption)
- ViewModel: Present (TestClassificationLogViewModel)
- Service: Present (TestClassificationService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

### الموديول 8: موديول المعامل الخارجية

#### 8.1 - تحديد تحليل كخارجي
- Model: Present (ExternalLabQueue)
- ViewModel: Present (ExternalLabManagementViewModel)
- Service: Present (ExternalLabService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 8.2 - تسجيل المريض للتحليل الخارجي
- Model: Present (ExternalLabQueue)
- ViewModel: Present (ExternalLabManagementViewModel)
- Service: Present (ExternalLabService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 8.3 - تجهيز العينة الخارجية
- Model: Present (ShipmentManifest)
- ViewModel: Present (ExternalLabManagementViewModel)
- Service: Present (ExternalLabService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 8.4 - متابعة حالة العينة الخارجية
- Model: Present (ExternalLabQueue)
- ViewModel: Present (ExternalLabManagementViewModel)
- Service: Present (ExternalLabService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 8.5 - إدخال نتيجة المعمل الخارجي
- Model: Present (ResultValue)
- ViewModel: Present (ExternalLabManagementViewModel)
- Service: Present (ExternalLabService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 3 | Real: 3 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 8.6 - طباعة تقرير المعمل الخارجي
- Model: Present (ResultValue)
- ViewModel: Present (ExternalLabManagementViewModel)
- Service: Present (PrintService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 2 | Real: 2 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 8.7 - تسوية حساب المعمل الخارجي
- Model: Present (ExternalLabSettlement)
- ViewModel: Present (ExternalLabManagementViewModel)
- Service: Present (ExternalSettlementService)
- اختبارات الوحدة: Weak (إجمالي مرتبط: 1 | Real: 0 | Weak: 1 | Fake: 0)
- نسبة الاكتمال: **75%**
- الفجوات: الوظيفة ممثلة عبر الطبقات الثلاث، لكن الاختبارات المرتبطة جزئية/ضعيفة ولا تكفي كحاجز انحدار كامل.

### الموديول 9: موديول الإحصائيات والتحليلات

#### 9.1 - توزيع المرضى حسب الجنس
- Model: Present (Visit)
- ViewModel: Present (StatisticsViewModel)
- Service: Present (StatisticsService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 9.2 - توزيع المرضى حسب الشهر
- Model: Present (Visit)
- ViewModel: Present (StatisticsViewModel)
- Service: Present (StatisticsService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 9.3 - تحليل الطلب على التحاليل
- Model: Present (Visit)
- ViewModel: Present (StatisticsViewModel)
- Service: Present (StatisticsService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 9.4 - عدد العينات سنوياً
- Model: Present (Visit)
- ViewModel: Present (StatisticsViewModel)
- Service: Present (StatisticsService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 9.5 - تحليل مصادر الإحالة
- Model: Present (Visit)
- ViewModel: Present (StatisticsViewModel)
- Service: Present (StatisticsService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

#### 9.6 - تقرير إنتاجية المستخدمين
- Model: Present (ResultValue)
- ViewModel: Present (StatisticsViewModel)
- Service: Present (UserProductivityService)
- اختبارات الوحدة: Real (إجمالي مرتبط: 1 | Real: 1 | Weak: 0 | Fake: 0)
- نسبة الاكتمال: **100%**
- الفجوات: لا توجد فجوات حرجة مرصودة لهذه الوظيفة وفق الأدلة الحالية من الكود والاختبارات.

## 3) تحليل اختبارات الوحدة (386 اختبار)

- Real Tests: **283**
- Weak Tests: **103**
- Fake Tests: **0**

### منهجية التصنيف المستخدمة
- Real: الاختبار يحتوي Assertions متعددة و/أو يتحقق من آثار جانبية على البيانات أو سلوك أخطاء/استثناءات أو تحقق Mock قوي.
- Weak: الاختبار يحتوي Assertions محدودة لا تكفي وحدها كحاجز انحدار قوي، أو يغطي جزءًا ضيقًا من السيناريو.
- Fake: لا يحتوي Assertions فعلية قابلة لكشف الانكسار المنطقي.

### ملخص الجودة حسب ملفات الاختبار
| الملف | الإجمالي | Real | Weak | Fake |
|---|---:|---:|---:|---:|
| AccountsTreasuryServiceTests.cs | 3 | 2 | 1 | 0 |
| AdditionalFunctionalityTests.cs | 28 | 16 | 12 | 0 |
| AppSessionTests.cs | 6 | 2 | 4 | 0 |
| AttendanceLogViewModelTests.cs | 1 | 1 | 0 | 0 |
| AttendancePayrollReportServiceTests.cs | 1 | 1 | 0 | 0 |
| AttendanceReportViewModelTests.cs | 2 | 2 | 0 | 0 |
| AttendanceServiceTests.cs | 6 | 4 | 2 | 0 |
| BackupRestoreServiceTests.cs | 3 | 3 | 0 | 0 |
| BaseViewModelTests.cs | 4 | 0 | 4 | 0 |
| BlankReportViewModelTests.cs | 1 | 1 | 0 | 0 |
| ContractInvoiceServiceTests.cs | 13 | 13 | 0 | 0 |
| CultureSensitivityServiceTests.cs | 19 | 9 | 10 | 0 |
| CultureSensitivityViewModelTests.cs | 5 | 4 | 1 | 0 |
| CustomGroupsViewModelTests.cs | 2 | 2 | 0 | 0 |
| DocumentationFunctionCoverageTests.cs | 4 | 2 | 2 | 0 |
| ExternalLabManagementViewModelTests.cs | 4 | 4 | 0 | 0 |
| ExternalLabServiceTests.cs | 10 | 10 | 0 | 0 |
| ExternalSettlementServiceTests.cs | 2 | 1 | 1 | 0 |
| FunctionCoverageGapTests.cs | 12 | 7 | 5 | 0 |
| GroupWorksheetServiceTests.cs | 1 | 1 | 0 | 0 |
| GroupWorksheetViewModelTests.cs | 2 | 2 | 0 | 0 |
| InvoiceServiceTests.cs | 23 | 20 | 3 | 0 |
| LoginViewModelTests.cs | 8 | 3 | 5 | 0 |
| MainViewModelTests.cs | 1 | 1 | 0 | 0 |
| PasswordSecurityTests.cs | 8 | 4 | 4 | 0 |
| PatientBillingViewModelTests.cs | 10 | 6 | 4 | 0 |
| PatientHistoryViewModelTests.cs | 2 | 1 | 1 | 0 |
| PatientRegistrationViewModelTests.cs | 17 | 11 | 6 | 0 |
| PatientSearchViewModelTests.cs | 4 | 1 | 3 | 0 |
| PatientServiceTests.cs | 13 | 12 | 1 | 0 |
| PatientTestsSelectionViewModelTests.cs | 4 | 2 | 2 | 0 |
| PhysicianServiceTests.cs | 7 | 7 | 0 | 0 |
| PhysicianViewModelTests.cs | 2 | 1 | 1 | 0 |
| PriceListsViewModelTests.cs | 2 | 2 | 0 | 0 |
| PrintServiceTests.cs | 3 | 3 | 0 | 0 |
| ReferenceRangesViewModelTests.cs | 2 | 2 | 0 | 0 |
| ReferralsViewModelTests.cs | 2 | 2 | 0 | 0 |
| RelayCommandTests.cs | 7 | 1 | 6 | 0 |
| ReportServiceTests.cs | 3 | 2 | 1 | 0 |
| ReportViewerViewModelTests.cs | 2 | 2 | 0 | 0 |
| ResultsEntryViewModelTests.cs | 9 | 8 | 1 | 0 |
| ResultsServiceTests.cs | 14 | 14 | 0 | 0 |
| SampleCollectionServiceTests.cs | 11 | 11 | 0 | 0 |
| SampleCollectionViewModelTests.cs | 7 | 4 | 3 | 0 |
| SampleTrackingServiceTests.cs | 1 | 1 | 0 | 0 |
| SettingsServiceTests.cs | 14 | 3 | 11 | 0 |
| SettingsViewModelTests.cs | 2 | 2 | 0 | 0 |
| StatisticsServiceTests.cs | 7 | 7 | 0 | 0 |
| StatisticsViewModelTests.cs | 1 | 1 | 0 | 0 |
| SystemMonitorServiceTests.cs | 1 | 1 | 0 | 0 |
| SystemSettingsServiceTests.cs | 3 | 1 | 2 | 0 |
| SystemSettingsViewModelTests.cs | 4 | 3 | 1 | 0 |
| SystemUsageMonitorViewModelTests.cs | 1 | 1 | 0 | 0 |
| TardinessServiceTests.cs | 1 | 1 | 0 | 0 |
| TestCatalogServiceTests.cs | 20 | 20 | 0 | 0 |
| TestCatalogViewModelTests.cs | 10 | 9 | 1 | 0 |
| TestClassificationLogViewModelTests.cs | 2 | 2 | 0 | 0 |
| TestClassificationServiceTests.cs | 1 | 1 | 0 | 0 |
| TestCommentsViewModelTests.cs | 2 | 2 | 0 | 0 |
| UserActivityLogViewModelTests.cs | 1 | 1 | 0 | 0 |
| UserActivityServiceTests.cs | 1 | 0 | 1 | 0 |
| UserAdminServiceTests.cs | 7 | 7 | 0 | 0 |
| UserProductivityServiceTests.cs | 1 | 1 | 0 | 0 |
| UsersPermissionsViewModelTests.cs | 3 | 3 | 0 | 0 |
| VisitServiceTests.cs | 7 | 4 | 3 | 0 |
| WorksheetServiceTests.cs | 2 | 1 | 1 | 0 |
| WorksheetViewModelTests.cs | 4 | 4 | 0 | 0 |

### تقييم عام للحماية من الانحدار
- الاختبارات الحالية توفر حماية جيدة في أجزاء كبيرة من النظام، لكنها ليست كافية بالكامل بسبب وجود اختبارات Weak تحتاج تعزيز Assertions وسيناريوهات فشل/حواف.

## 4) الملخص النهائي

- وظائف مصنفة 100%: **53**
- وظائف مصنفة 75%: **44**
- وظائف مصنفة 50%: **0**
- وظائف مصنفة 25%: **0**

- تقييم جودة المشروع: البنية الوظيفية مكتملة طبقيًا بدرجة عالية، لكن جودة جزء من الاختبارات تحتاج رفعًا للوصول إلى حماية انحدار أقوى وأكثر اتساقًا.
- أهم الفجوات الحرجة:
  - وجود نسبة اختبارات Weak في ملفات متعددة رغم اكتمال الطبقات.
  - الحاجة لزيادة اختبارات الفشل والحواف وربط بعض الوظائف باختبارات أكثر مباشرة بدل الربط غير المباشر عبر ملفات الخدمة/الـViewModel العامة.

- هل المشروع مستقر وقابل للتطوير؟ **YES**

