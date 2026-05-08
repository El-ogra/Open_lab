# Unit Test Coverage Tracker
## Open_lab Project — 97 Functions × 2 Layers (Service + ViewModel)

**آخر تحديث:** 2026-05-08
**إجمالي الاختبارات:** 1,539 اختبار ناجح (0 فاشل)
**نسبة النجاح:** 100%

---

## ملخص التغطية

| الموديول | عدد الوظائف | Service ✅ | ViewModel ✅ | الحالة |
|:---|:---:|:---:|:---:|:---:|
| 1. إدارة المرضى | 8 | 8 | 8 | ✅ |
| 2. المحاسبة والمالية | 13 | 13 | 13 | ✅ |
| 3. إدارة التحاليل والأسعار | 9 | 9 | 9 | ✅ |
| 4. إدخال النتائج والتقارير | 9 | 9 | 9 | ✅ |
| 5. المزارع والحساسية | 7 | 7 | 7 | ✅ |
| 6. سحب العينات | 4 | 4 | 4 | ✅ |
| 7. أوراق العمل | 4 | 4 | 4 | ✅ |
| 8. المعامل الخارجية | 7 | 7 | 6 | ⚠️ |
| 9. الإحصائيات والتحليلات | 6 | 6 | 6 | ✅ |
| 10. إدارة المستخدمين والصلاحيات | 8 | 8 | 8 | ✅ |
| 11. الحضور والانصراف | 5 | 5 | 5 | ✅ |
| 12. جهات التعاقد والإحالة | 9 | 9 | 7 | ⚠️ |
| 13. إعدادات النظام والتكوين | 8 | 8 | 8 | ✅ |
| **الإجمالي** | **97** | **97** | **94** | — |

---

## التفصيل الكامل

### Module 1 — إدارة المرضى (Patient Management)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 1.1 | Add New Patient | ✅ | ✅ | PatientServiceTests, FunctionCoverageGapTests, ServiceTestTemplateTests | PatientRegistrationViewModelTests |
| 1.2 | Edit Patient Data | ✅ | ✅ | PatientServiceTests, Module1And2ServiceCompletionTests | PatientRegistrationViewModelTests, Module1And2ViewModelCompletionTests |
| 1.3 | Add Tests to Patient | ✅ | ✅ | VisitServiceTests, Module1And2ServiceCompletionTests | PatientTestsSelectionViewModelTests, Module1And2ViewModelCompletionTests |
| 1.4 | Delete Tests | ✅ | ✅ | VisitServiceTests, Module1And2ServiceCompletionTests | PatientTestsSelectionViewModelTests, Module1And2ViewModelCompletionTests |
| 1.5 | Search Patient | ✅ | ✅ | PatientServiceTests, PatientSearchServiceTests, Module1And2ServiceCompletionTests | PatientSearchViewModelTests, PatientRegistrationViewModelTests, Module1And2ViewModelCompletionTests |
| 1.6 | View Patient History | ✅ | ✅ | PatientServiceTests, PatientSearchServiceTests, ReportServiceTests | PatientHistoryViewModelTests, PatientSearchViewModelTests, Module1And2ViewModelCompletionTests |
| 1.7 | Add Medical History | ✅ | ✅ | PatientServiceTests, Module1And2ServiceCompletionTests | PatientRegistrationViewModelTests, Module1And2ViewModelCompletionTests |
| 1.8 | Add Group of Tests | ✅ | ✅ | VisitServiceTests, AdditionalFunctionalityTests, Module1And2ServiceCompletionTests | PatientTestsSelectionViewModelTests, Module1And2ViewModelCompletionTests |

---

### Module 2 — المحاسبة والمالية (Financial Accounting)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 2.1 | Calculate Total | ✅ | ✅ | InvoiceServiceTests, FunctionCoverageGapTests | PatientBillingViewModelTests, Module1And2ViewModelCompletionTests |
| 2.2 | Apply Discount | ✅ | ✅ | InvoiceServiceTests | PatientBillingViewModelTests, Module1And2ViewModelCompletionTests |
| 2.3 | Record Payment | ✅ | ✅ | InvoiceServiceTests, Module1And2ServiceCompletionTests | PatientBillingViewModelTests, Module1And2ViewModelCompletionTests |
| 2.4 | Settle Account | ✅ | ✅ | InvoiceServiceTests | PatientBillingViewModelTests, Module1And2ViewModelCompletionTests |
| 2.5 | Edit Payment | ✅ | ✅ | InvoiceServiceTests, Module1And2ServiceCompletionTests | PatientBillingViewModelTests |
| 2.6 | Delete Payment | ✅ | ✅ | InvoiceServiceTests, Module1And2ServiceCompletionTests | PatientBillingViewModelTests |
| 2.7 | Add Additional Charge | ✅ | ✅ | InvoiceServiceTests | PatientBillingViewModelTests |
| 2.8 | Generate Invoice | ✅ | ✅ | InvoiceServiceTests | PatientBillingViewModelTests, Module1And2ViewModelCompletionTests |
| 2.9 | View Patient Account | ✅ | ✅ | InvoiceServiceTests | PatientBillingViewModelTests, Module1And2ViewModelCompletionTests |
| 2.10 | Generate Inventory | ✅ | ✅ | AccountsTreasuryServiceTests | AccountsTreasuryViewModelTests |
| 2.11 | Branch-wise Inventory | ✅ | ✅ | AccountsTreasuryServiceTests, AdditionalFunctionalityTests | AccountsTreasuryViewModelTests |
| 2.12 | Doctor-wise Inventory | ✅ | ✅ | AccountsTreasuryServiceTests | AccountsTreasuryViewModelTests |
| 2.13 | Lab-to-Lab Settlement | ✅ | ✅ | ExternalSettlementServiceTests, AdditionalFunctionalityTests | ExternalLabManagementViewModelTests |

---

### Module 3 — إدارة التحاليل والأسعار (Test & Price Management)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 3.1 | Add New Test | ✅ | ✅ | TestCatalogServiceTests, TestCatalogServiceTests_Additional, FunctionCoverageGapTests, Module3ServiceCompletionTests | Module3ViewModelCompletionTests, Module3ViewModelTests_Additional |
| 3.2 | Edit Test Data | ✅ | ✅ | TestCatalogServiceTests, TestCatalogServiceTests_Additional, Module3ServiceCompletionTests | Module3ViewModelTests_Additional |
| 3.3 | Set Reference Values | ✅ | ✅ | TestCatalogServiceTests, TestCatalogServiceTests_Additional, ResultsServiceTests, Module3ServiceCompletionTests | Module3ViewModelCompletionTests, Module3ViewModelTests_Additional, ReferenceRangesViewModelTests |
| 3.4 | Add Low/High Comments | ✅ | ✅ | TestCatalogServiceTests_Additional | Module3ViewModelTests_Additional, TestCommentsViewModelTests |
| 3.5 | Create Custom Group | ✅ | ✅ | TestCatalogServiceTests, TestCatalogServiceTests_Additional, Module3ServiceCompletionTests | CustomGroupsViewModelTests, Module3ViewModelCompletionTests, Module3ViewModelTests_Additional |
| 3.6 | Add Test Comments | ✅ | ✅ | Module3ServiceCompletionTests | Module3ViewModelCompletionTests |
| 3.7 | Create Price List | ✅ | ✅ | TestCatalogServiceTests, TestCatalogServiceTests_Additional, Module3ServiceCompletionTests | PriceListsViewModelTests, Module3ViewModelCompletionTests, Module3ViewModelTests_Additional |
| 3.8 | Update Prices | ✅ | ✅ | TestCatalogServiceTests, TestCatalogServiceTests_Additional, Module3ServiceCompletionTests | PriceListsViewModelTests, Module3ViewModelCompletionTests, Module3ViewModelTests_Additional |
| 3.9 | Mark as Outsourced | ✅ | ✅ | TestCatalogServiceTests_Additional | TestCatalogViewModelTests, Module3ViewModelCompletionTests, Module3ViewModelTests_Additional |

---

### Module 4 — إدخال النتائج والتقارير (Result Entry & Reporting)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 4.1 | Enter Test Results | ✅ | ✅ | ResultsServiceTests, FunctionCoverageGapTests, Module4Tests_Additional | ResultsEntryViewModelTests, Module4ViewModelCompletion_Part1 |
| 4.2 | Save Results | ✅ | ✅ | Module4Tests_Additional | Module4ViewModelCompletion_Part1 |
| 4.3 | Edit Results | ✅ | ✅ | ResultsServiceTests, Module4Tests_Additional | ResultsEntryViewModelTests, Module4ViewModelCompletion_Part1 |
| 4.4 | Create Composite Report | ✅ | ✅ | ReportServiceTests, ResultsServiceTests, Module4ServiceCompletionTests, Module4Tests_Additional | CombinedReportViewModelTests, ReportViewerViewModelTests, ResultsEntryViewModelTests, Module4ViewModelCompletion_Part2 |
| 4.5 | Arrange Report Order | ✅ | ✅ | ReportServiceTests, AdditionalFunctionalityTests, Module4ServiceCompletionTests, Module4Tests_Additional | Module4ViewModelCompletion_Part2 |
| 4.6 | Preview Report | ✅ | ✅ | Module4ServiceCompletionTests, Module4Tests_Additional | Module4ViewModelCompletion_Part3 |
| 4.7 | Print Report | ✅ | ✅ | ResultsServiceTests, Module4ServiceCompletionTests, Module4Tests_Additional | ReportViewerViewModelTests, Module4ViewModelCompletion_Part3 |
| 4.8 | Print Blank Report | ✅ | ✅ | Module4ServiceCompletionTests, Module4Tests_Additional | BlankReportViewModelTests, Module4_Function4_8_Tests |
| 4.9 | Compare with History | ✅ | ✅ | ReportServiceTests, AdditionalFunctionalityTests, Module4ServiceCompletionTests, Module4Tests_Additional | CompareWithHistoryViewModelTests |

---

### Module 5 — المزارع والحساسية (Culture & Sensitivity)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 5.1 | Enter Culture Data | ✅ | ✅ | CultureSensitivityServiceTests, FunctionCoverageGapTests, Module5Tests_Additional | CultureSensitivityViewModelTests, Module5ViewModelTests_Additional |
| 5.2 | Add Antibiotics | ✅ | ✅ | CultureSensitivityServiceTests, Module5Tests_Additional | CultureSensitivityViewModelTests, Module5ViewModelTests_Additional |
| 5.3 | Set Sensitivity | ✅ | ✅ | CultureSensitivityServiceTests, Module5Tests_Additional | CultureSensitivityViewModelTests, Module5ViewModelTests_Additional |
| 5.4 | Classify Sensitivity | ✅ | ✅ | CultureSensitivityServiceTests, Module5Tests_Additional | Module5ViewModelTests_Additional |
| 5.5 | Filter Pregnancy Antibiotics | ✅ | ✅ | CultureSensitivityServiceTests, Module5Tests_Additional | CultureSensitivityViewModelTests, Module5ViewModelTests_Additional |
| 5.6 | Filter Children Antibiotics | ✅ | ✅ | CultureSensitivityServiceTests, Module5Tests_Additional | Module5ViewModelTests_Additional |
| 5.7 | Print Culture Report | ✅ | ✅ | PrintServiceTests, Module5Tests_Additional | CultureSensitivityViewModelTests, Module5ViewModelTests_Additional |

---

### Module 6 — سحب العينات (Sample Collection)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 6.1 | Register Sample Collection | ✅ | ✅ | SampleCollectionServiceTests, FunctionCoverageGapTests, Module6Tests_Additional | SampleCollectionViewModelTests, Module6ViewModelTests_Additional |
| 6.2 | Record Sample Separation | ✅ | ✅ | SampleCollectionServiceTests, Module6Tests_Additional | SampleCollectionViewModelTests, Module6ViewModelTests_Additional |
| 6.3 | Track Sample Status | ✅ | ✅ | SampleCollectionServiceTests, SampleTrackingServiceTests, Module6Tests_Additional | SampleCollectionViewModelTests, Module6ViewModelTests_Additional |
| 6.4 | Mark Taken Outside Lab | ✅ | ✅ | SampleCollectionServiceTests, Module6Tests_Additional | SampleCollectionViewModelTests, Module6ViewModelTests_Additional |

---

### Module 7 — أوراق العمل (Work Sheets)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 7.1 | Generate Patient Worksheet | ✅ | ✅ | WorksheetServiceTests, FunctionCoverageGapTests, Module7Tests_Additional | WorkSheetByPatientViewModelTests, WorksheetViewModelTests, Module7ViewModelTests_Additional |
| 7.2 | Generate Test Worksheet | ✅ | ✅ | WorksheetServiceTests, Module7Tests_Additional | WorkSheetByTestViewModelTests, WorksheetViewModelTests, Module7ViewModelTests_Additional |
| 7.3 | Generate Group Worksheet | ✅ | ✅ | GroupWorksheetServiceTests, AdditionalFunctionalityTests, Module7Tests_Additional | GroupWorksheetViewModelTests, Module7ViewModelTests_Additional |
| 7.4 | Test Classification LOG | ✅ | ✅ | TestClassificationServiceTests, AdditionalFunctionalityTests, Module7Tests_Additional | TestClassificationLogViewModelTests, Module7ViewModelTests_Additional |

---

### Module 8 — المعامل الخارجية (Lab-to-Lab)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 8.1 | Mark Test as External | ✅ | ✅ | FunctionCoverageGapTests, Module8Tests_Additional | Module8ViewModelTests_Additional |
| 8.2 | Register Patient for External Test | ✅ | ⚠️ | Module8Tests_Additional | — |
| 8.3 | Prepare External Sample | ✅ | ✅ | Module8Tests_Additional | ExternalLabManagementViewModelTests, Module8ViewModelTests_Additional |
| 8.4 | Track External Sample Status | ✅ | ✅ | Module8Tests_Additional | ExternalLabManagementViewModelTests |
| 8.5 | Enter External Lab Result | ✅ | ✅ | ExternalLabServiceTests, Module8Tests_Additional | ExternalLabManagementViewModelTests, Module8ViewModelTests_Additional |
| 8.6 | Print External Lab Report | ✅ | ✅ | ExternalLabServiceTests, PrintServiceTests, Module8Tests_Additional | ExternalLabManagementViewModelTests, Module8ViewModelTests_Additional |
| 8.7 | Settle External Lab Account | ✅ | ✅ | Module8Tests_Additional | Module8ViewModelTests_Additional |

---

### Module 9 — الإحصائيات والتحليلات (Statistics & Analytics)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 9.1 | Patient Count by Gender | ✅ | ✅ | StatisticsServiceTests, FunctionCoverageGapTests, Module9ServiceTests_Additional | StatisticsViewModelTests, Module9ViewModelTests_Additional |
| 9.2 | Patient Count by Month | ✅ | ✅ | StatisticsServiceTests, Module9ServiceTests_Additional | Module9ViewModelTests_Additional |
| 9.3 | Test Demand Analysis | ✅ | ✅ | StatisticsServiceTests, Module9ServiceTests_Additional | Module9ViewModelTests_Additional |
| 9.4 | Sample Count per Year | ✅ | ✅ | StatisticsServiceTests, Module9ServiceTests_Additional | Module9ViewModelTests_Additional |
| 9.5 | Referral Source Analysis | ✅ | ✅ | StatisticsServiceTests, Module9ServiceTests_Additional | StatisticsViewModelTests, Module9ViewModelTests_Additional |
| 9.6 | User Productivity Report | ✅ | ✅ | UserProductivityServiceTests, AdditionalFunctionalityTests, Module9ServiceTests_Additional | StatisticsViewModelTests, Module9ViewModelTests_Additional |

---

### Module 10 — إدارة المستخدمين والصلاحيات (User Management & Security)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 10.1 | Create User | ✅ | ✅ | UserAdminServiceTests, FunctionCoverageGapTests | UsersPermissionsViewModelTests |
| 10.2 | Set Permissions | ✅ | ✅ | UserAdminServiceTests, AppSessionTests | UsersPermissionsViewModelTests |
| 10.3 | Edit User Data | ✅ | ✅ | UserAdminServiceTests | UsersPermissionsViewModelTests |
| 10.4 | Record Attendance | ✅ | ✅ | AttendanceServiceTests | AttendanceLogViewModelTests, LoginViewModelTests |
| 10.5 | Record Departure | ✅ | ✅ | AttendanceServiceTests | AttendanceLogViewModelTests |
| 10.6 | View User Activity Log | ✅ | ✅ | UserActivityServiceTests, AdditionalFunctionalityTests | UserActivityLogViewModelTests |
| 10.7 | Monitor System Usage | ✅ | ✅ | SystemMonitorServiceTests, AdditionalFunctionalityTests | SystemUsageMonitorViewModelTests |
| 10.8 | Logout | ✅ | ✅ | AuthServiceTests | LoginViewModelTests, MainViewModelTests |

---

### Module 11 — الحضور والانصراف (HR & Attendance)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 11.1 | Clock In | ✅ | ✅ | Module11ServiceTests_Additional | Module11ViewModelTests_Additional |
| 11.2 | Clock Out | ✅ | ✅ | Module11ServiceTests_Additional | Module11ViewModelTests_Additional |
| 11.3 | Calculate Working Hours | ✅ | ✅ | AdditionalFunctionalityTests, Module11ServiceTests_Additional | Module11ViewModelTests_Additional |
| 11.4 | Calculate Tardiness | ✅ | ✅ | TardinessServiceTests, Module11ServiceTests_Additional | Module11ViewModelTests_Additional |
| 11.5 | Generate Attendance Report | ✅ | ✅ | AttendancePayrollReportServiceTests, Module11ServiceTests_Additional | AttendanceReportViewModelTests, Module11ViewModelTests_Additional |

---

### Module 12 — جهات التعاقد والإحالة (Contracts & Referrals)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 12.1 | Create Contract Entity | ✅ | ✅ | TestCatalogServiceTests, Module12ServiceTests_Additional | ReferralsViewModelTests, Module12ViewModelTests_Additional |
| 12.2 | Link Price List to Entity | ✅ | ⚠️ | Module12ServiceTests_Additional | — |
| 12.3 | Set Entity Discount | ✅ | ✅ | TestCatalogServiceTests, ContractInvoiceServiceTests, AdditionalFunctionalityTests, Module12ServiceTests_Additional | ReferralsViewModelTests, Module12ViewModelTests_Additional |
| 12.4 | Set Entity Commission | ✅ | ✅ | Module12ServiceTests_Additional | Module12ViewModelTests_Additional |
| 12.5 | Assign Patient to Contract | ✅ | ⚠️ | ContractInvoiceServiceTests, AdditionalFunctionalityTests, Module12ServiceTests_Additional | — |
| 12.6 | Add Referring Physician | ✅ | ✅ | PhysicianServiceTests, Module12ServiceTests_Additional | PhysicianViewModelTests, Module12ViewModelTests_Additional |
| 12.7 | Set Physician Price List | ✅ | ✅ | PhysicianServiceTests, Module12ServiceTests_Additional | Module12ViewModelTests_Additional |
| 12.8 | Generate Contract Invoice | ✅ | ✅ | ContractInvoiceServiceTests, FunctionCoverageGapTests, Module12ServiceTests_Additional | ContractInvoiceViewModelTests, Module12ViewModelTests_Additional |
| 12.9 | Settle Contract Account | ✅ | ✅ | ContractInvoiceServiceTests, Module12ServiceTests_Additional | ContractInvoiceViewModelTests, Module12ViewModelTests_Additional |

---

### Module 13 — إعدادات النظام والتكوين (System Settings & Configuration)

| # | الوظيفة | Service | ViewModel | ملفات اختبار Service | ملفات اختبار ViewModel |
|:---|:---|:---:|:---:|:---|:---|
| 13.1 | Set Report Margins | ✅ | ✅ | FunctionCoverageGapTests, Module13ServiceTests_Additional | SettingsViewModelTests, Module13ViewModelTests_Additional |
| 13.2 | Set Paper Size | ✅ | ✅ | SystemSettingsServiceTests, Module13ServiceTests_Additional | SystemSettingsViewModelTests, Module13ViewModelTests_Additional |
| 13.3 | Configure Header/Footer | ✅ | ✅ | SettingsServiceTests, SystemSettingsServiceTests, Module13ServiceTests_Additional | Module13ViewModelTests_Additional |
| 13.4 | Set Default Account Type | ✅ | ✅ | SettingsServiceTests, SystemSettingsServiceTests, Module13ServiceTests_Additional | Module13ViewModelTests_Additional |
| 13.5 | Configure Printers | ✅ | ✅ | SystemSettingsServiceTests, AdditionalFunctionalityTests, Module13ServiceTests_Additional | Module13ViewModelTests_Additional |
| 13.6 | Set Invoice Settings | ✅ | ✅ | SettingsServiceTests, SystemSettingsServiceTests, Module13ServiceTests_Additional | Module13ViewModelTests_Additional |
| 13.7 | Configure Backup | ✅ | ✅ | BackupRestoreServiceTests, Module13ServiceTests_Additional | BackupRestoreViewModelTests, Module13ViewModelTests_Additional |
| 13.8 | Set System Password | ✅ | ✅ | SystemSettingsServiceTests, PasswordSecurityTests, Module13ServiceTests_Additional | SystemSettingsViewModelTests, Module13ViewModelTests_Additional |

---

## ملاحظات

### وظائف بدون اختبار ViewModel مباشر (⚠️)
الوظائف التالية مغطاة على مستوى Service لكن لم يتم العثور على تعليق `// Function:` مخصص لها في ملفات ViewModel:

| الوظيفة | السبب |
|:---|:---|
| 8.2 Register Patient for External Test | الوظيفة تنفذ تلقائياً عند إضافة تحليل خارجي (ضمن PatientTestsSelection logic) |
| 12.2 Link Price List to Entity | الوظيفة مدمجة ضمن شاشة إدارة جهات التعاقد |
| 12.5 Assign Patient to Contract | الوظيفة مدمجة ضمن شاشة تسجيل المريض |

### قواعد العمل (Business Rules) المغطاة

| القاعدة | الوصف | الحالة | ملف الاختبار |
|:---|:---|:---:|:---|
| BR-ACC-001 | أولوية سعر التعاقد | ✅ | InvoiceServiceTests |
| BR-ACC-002 | أولوية سعر الطبيب | ✅ | InvoiceServiceTests |
| BR-ACC-003 | تسجيل الدفع فور التأكيد | ✅ | BusinessRulesCoverageTests |
| BR-ACC-004 | الخصم لا يتجاوز 100% | ✅ | InvoiceServiceTests |
| BR-ACC-005 | توثيق التعديلات المالية | ✅ | BusinessRulesCoverageTests |
| BR-ACC-006 | شمول الخصومات في التقارير | ✅ | BusinessRulesCoverageTests |
| BR-ACC-007 | حساب تكلفة المعامل الخارجية | ✅ | ExternalSettlementServiceTests |
| BR-ACC-008 | حساب العمولات | ✅ | Module12ServiceTests_Additional |
| BR-MED-001 | مقارنة النتائج بالقيم المرجعية | ✅ | ResultsServiceTests |
| BR-MED-002 | التعليقات التلقائية للقيم الشاذة | ✅ | TestCatalogServiceTests_Additional |
| BR-MED-004 | بيانات المزرعة | ✅ | CultureSensitivityServiceTests |
| BR-MED-005 | تصنيف الحساسية | ✅ | CultureSensitivityServiceTests |
| BR-MED-006 | تصفية مضادات الحوامل | ✅ | CultureSensitivityServiceTests |
| BR-MED-007 | تصفية مضادات الأطفال | ✅ | CultureSensitivityServiceTests |
| BR-MED-008 | البيانات التاريخية للقراءة فقط | ✅ | PatientServiceTests |
| BR-OPS-002 | مراجعة الرصيد قبل الإغلاق | ✅ | BusinessRulesCoverageTests |
| BR-OPS-004 | حساب الربح من المعامل الخارجية | ✅ | BusinessRulesCoverageTests |
| BR-SEC-001 | صلاحيات الوصول | ✅ | UserAdminServiceTests |
| BR-SEC-002 | سجل التدقيق (Audit Trail) | ✅ | BusinessRulesCoverageTests |
| BR-SEC-003 | كلمة مرور النظام | ✅ | PasswordSecurityTests |
| BR-SEC-004 | تسجيل الحضور والانصراف | ✅ | AttendanceServiceTests |
| BR-VAL-001 | البيانات الأساسية إلزامية | ✅ | BusinessRulesCoverageTests |
| BR-VAL-004 | منع الحذف بعد تسجيل النتيجة | ✅ | BusinessRulesCoverageTests |
| BR-VAL-005 | التعديل يؤثر على الحالات الجديدة فقط | ✅ | TestCatalogServiceTests |

**الإجمالي:** 24 / 24 مغطاة (100%) ✅

---

## آخر تشغيل للاختبارات

**التاريخ:** 2026-05-08

```
========== Test discovery finished: 1556 Tests found in 7.6 sec ==========
========== Test run finished: 1556 Tests (1556 Passed, 0 Failed, 0 Skipped) run in 1.6 min ==========
```
