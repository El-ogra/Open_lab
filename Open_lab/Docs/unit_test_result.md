# سجل تغطية اختبارات الوحدة — مشروع Open_lab
## Unit Test Coverage Tracker

> **إجمالي الوظائف:** 97 وظيفة | **إجمالي الموديولات:** 13 موديول
> **آخر تحديث:** يُحدَّث من قِبَل الوكيل البرمجي بعد كل جلسة عمل
> **قاعدة:** لا يُعدَّل هذا الملف يدوياً — يُحدَّث فقط بعد كتابة الاختبارات الفعلية والتحقق من نجاحها

---

## دليل الحالات

| الرمز | المعنى |
|-------|--------|
| ❌ | لم يبدأ — لا توجد اختبارات لهذه الوظيفة |
| 🔄 | جزئي — توجد اختبارات لكنها غير مكتملة |
| ✅ | مكتمل — تغطية كاملة لـ Service و ViewModel |
| ⚠️ | يحتاج مراجعة — اختبارات موجودة لكنها ضعيفة أو وهمية |

---

## الموديول 1 — إدارة المرضى | Patient Management

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 1.1 | إضافة مريض جديد — Add New Patient | ✅ | ✅ | ✅ مكتمل | PatientServiceTests + PatientRegistrationViewModelTests (Success/Failure/Edge) |
| 1.2 | تعديل بيانات مريض — Edit Patient Data | ✅ | ✅ | ⚠️ يحتاج مراجعة | Production Code Issue - Requires Manual Fix (BR-SEC-002 Audit Trail missing in production update flow) |
| 1.3 | إضافة تحاليل للمريض — Add Tests to Patient | ✅ | ✅ | ✅ مكتمل | VisitServiceTests + PatientTestsSelectionViewModelTests (Success/Failure/Edge) |
| 1.4 | حذف تحاليل — Delete Tests | ✅ | ✅ | ✅ مكتمل | VisitServiceTests + PatientTestsSelectionViewModelTests (Success/Failure/Edge) |
| 1.5 | البحث عن مريض — Search Patient | ✅ | ✅ | ✅ مكتمل | PatientServiceTests + PatientSearchViewModelTests + PatientRegistrationViewModelTests |
| 1.6 | عرض التاريخ المرضي — View Patient History | ✅ | ✅ | ✅ مكتمل | PatientServiceTests + PatientHistoryViewModelTests + PatientSearchViewModelTests |
| 1.7 | إضافة تاريخ طبي — Add Medical History | ✅ | ✅ | ✅ مكتمل | PatientServiceTests + PatientRegistrationViewModelTests (Success/Failure/Edge) |
| 1.8 | إضافة مجموعة تحاليل — Add Group of Tests | ✅ | ✅ | ✅ مكتمل | VisitServiceTests + PatientTestsSelectionViewModelTests (Success/Failure/Edge) |

---

## الموديول 2 — المحاسبة والمالية | Financial Accounting

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 2.1 | حساب الإجمالي — Calculate Total | ✅ | ✅ | ✅ مكتمل | InvoiceServiceTests + PatientBillingViewModelTests (Success/Failure/Edge) |
| 2.2 | تطبيق خصم — Apply Discount | ✅ | ✅ | ✅ مكتمل | InvoiceServiceTests + PatientBillingViewModelTests (Success/Failure/Edge) |
| 2.3 | تسجيل دفعة — Record Payment | ✅ | ✅ | ✅ مكتمل | InvoiceServiceTests + PatientBillingViewModelTests (Success/Failure/Edge) |
| 2.4 | تصفية الحساب — Settle Account | ✅ | ✅ | ✅ مكتمل | InvoiceServiceTests + PatientBillingViewModelTests (Success/Failure/Edge) |
| 2.5 | تعديل دفعة — Edit Payment | ✅ | ✅ | ✅ مكتمل | InvoiceServiceTests + PatientBillingViewModelTests (Success/Failure/Edge) |
| 2.6 | حذف دفعة — Delete Payment | ✅ | ✅ | ✅ مكتمل | InvoiceServiceTests + PatientBillingViewModelTests (Success/Failure/Edge) |
| 2.7 | إضافة رسوم إضافية — Add Additional Charge | ✅ | ✅ | ✅ مكتمل | InvoiceServiceTests + PatientBillingViewModelTests (Success/Failure/Edge) |
| 2.8 | إصدار فاتورة — Generate Invoice | ✅ | ✅ | ✅ مكتمل | InvoiceServiceTests + PatientBillingViewModelTests (Success/Failure/Edge) |
| 2.9 | كشف حساب المريض — View Patient Account | ✅ | ✅ | ✅ مكتمل | InvoiceServiceTests + PatientBillingViewModelTests (Success/Failure/Edge) |
| 2.10 | تقرير الجرد المالي — Generate Inventory | ✅ | ✅ | ✅ مكتمل | AccountsTreasuryServiceTests + AccountsTreasuryViewModelTests (Success/Failure/Edge) |
| 2.11 | جرد مالي للفرع — Branch-wise Inventory | ✅ | ✅ | ✅ مكتمل | AccountsTreasuryServiceTests + AccountsTreasuryViewModelTests (Success/Failure/Edge) |
| 2.12 | حساب الأطباء — Doctor-wise Inventory | ✅ | ✅ | ✅ مكتمل | AccountsTreasuryServiceTests + AccountsTreasuryViewModelTests (Success/Failure/Edge) |
| 2.13 | تصفية حسابات المعامل الخارجية — Lab-to-Lab Settlement | ✅ | ✅ | ✅ مكتمل | ExternalSettlementServiceTests + ExternalLabManagementViewModelTests (Success/Failure/Edge) |

---

## الموديول 3 — إدارة التحاليل والأسعار | Test & Price Management

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 3.1 | إضافة تحليل جديد — Add New Test | ✅ | ✅ | ✅ مكتمل | TestCatalogServiceTests_Additional: CreateTestAsync_WithAllFields_ShouldPersistCorrectly, CreateTestAsync_WithNullFields_ShouldSucceed, CreateTestAsync_WithEmptyCode_ShouldThrow, CreateTestAsync_WithEmptyName_ShouldThrow, CreateTestAsync_WithDuplicateCode_ShouldThrow, CreateTestAsync_WithZeroPrice_ShouldSucceed, CreateTestAsync_WithNegativePrice_ShouldThrow + TestCatalogViewModelTests: SaveAsync_Without_SelectedTest_Should_Create + Module3ViewModelTests_Additional: SaveAsync_With_AllFields_Should_Persist_Correctly |
| 3.2 | تعديل بيانات تحليل — Edit Test Data | ✅ | ✅ | ✅ مكتمل (BR-VAL-005) | TestCatalogServiceTests: UpdateTestAsync_WithValidData_ShouldUpdateSuccessfully, UpdateTestAsync_WithNullData_ShouldThrow, UpdateTestAsync_WithNonExistentId_ShouldThrow, UpdateTestAsync_WithDuplicateCode_ShouldThrow + TestCatalogViewModelTests: SaveAsync_With_SelectedTest_Should_Update + Module3ViewModelTests_Additional: UpdateTestAsync_Should_Modify_SelectedTest_Fields_SuccessGuard |
| 3.3 | تحديد القيم المرجعية — Set Reference Values | ✅ | ✅ | ✅ مكتمل (BR-MED-001) | TestCatalogServiceTests: CreateReferenceRangeAsync_WithValidRange_ShouldSucceed, CreateReferenceRangeAsync_WithNullTestId_ShouldThrow, CreateReferenceRangeAsync_WithInvalidRange_ShouldThrow, GetReferenceRangesAsync_WithValidTestId_ShouldReturnRanges, UpdateReferenceRangeAsync_WithValidData_ShouldSucceed, DeleteReferenceRangeAsync_WithValidId_ShouldSucceed + ReferenceRangesViewModelTests: SaveAsync_NewRange_Should_Call_CreateRange, SaveAsync_Without_SelectedTest_Should_NotCall_Service_FailureGuard + Module3ViewModelTests_Additional: LoadRangesAsync_Should_Populate_Ranges_SuccessGuard, UpdateRangeAsync_Should_Call_Update_Service_SuccessGuard, DeleteAsync_Should_Remove_Range_And_Clear_Selection_SuccessGuard |
| 3.4 | إضافة تعليقات القيم المرتفعة/المنخفضة — Add Low/High Comments | ✅ | ✅ | ✅ مكتمل (BR-MED-002) | TestCatalogServiceTests: CreateTestCommentAsync_WithValidData_ShouldSucceed, CreateTestCommentAsync_WithEmptyText_ShouldThrow, GetTestCommentsAsync_ShouldReturnComments, DeleteTestCommentAsync_WithValidId_ShouldSucceed + TestCommentsViewModelTests: SaveAsync_Should_Persist_Low_High_Comments_LogicGuard + Module3ViewModelTests_Additional: SaveAsync_Update_Existing_Comment_Should_Call_Update_Service_SuccessGuard |
| 3.5 | إنشاء مجموعة مخصصة — Create Custom Group | ✅ | ✅ | ✅ مكتمل | TestCatalogServiceTests: CreateCustomGroupAsync_WithValidData_ShouldSucceed, CreateCustomGroupAsync_WithEmptyName_ShouldThrow, CreateCustomGroupAsync_WithDuplicateName_ShouldThrow, CreateCustomGroupAsync_WithZeroPrice_ShouldSucceed + CustomGroupsViewModelTests: SaveGroupAsync_Should_Create_Group, SaveGroupAsync_With_EmptyName_Should_NotCall_Service_FailureGuard + Module3ViewModelTests_Additional: SaveGroupAsync_With_Zero_Price_Should_Allow_EdgeGuard, AddItemAsync_With_Existing_Item_Should_Set_Test_Reference_SuccessGuard |
| 3.6 | إضافة تعليقات التحليل — Add Test Comments | ✅ | ✅ | ✅ مكتمل | (Shared with 3.4 - same service methods) |
| 3.7 | إنشاء قائمة أسعار — Create Price List | ✅ | ✅ | ✅ مكتمل | TestCatalogServiceTests: CreatePriceListAsync_WithValidData_ShouldSucceed, CreatePriceListAsync_WithEmptyName_ShouldThrow, CreatePriceListAsync_WithNullReferral_ShouldSucceed, GetPriceListsAsync_ShouldReturnLists, UpdatePriceListAsync_WithValidData_ShouldSucceed, DeletePriceListAsync_WithValidId_ShouldSucceed + PriceListsViewModelTests: SaveListAsync_Should_Create_PriceList, SaveListAsync_With_EmptyName_Should_NotCall_Service_FailureGuard + Module3ViewModelTests_Additional: UpdateListAsync_Should_Call_Update_Service_SuccessGuard, PrintListAsync_Should_Call_PrintService_SuccessGuard |
| 3.8 | تحديث الأسعار — Update Prices | ✅ | ✅ | ✅ مكتمل | TestCatalogServiceTests: AddPriceListItemAsync_WithValidData_ShouldSucceed, AddPriceListItemAsync_WithNegativePrice_ShouldThrow, AddPriceListItemAsync_WithDuplicateItem_ShouldThrow, UpdatePriceListItemAsync_WithValidData_ShouldSucceed, GetPriceListItemsAsync_ShouldReturnItems, DeletePriceListItemAsync_WithValidId_ShouldSucceed + PriceListsViewModelTests: AddItemAsync_Should_Add_Test_To_PriceList, AddItemAsync_When_PriceZero_Should_Fallback_To_TestPrice_EdgeGuard + Module3ViewModelTests_Additional: UpdateItemAsync_Should_Change_Price_SuccessGuard, DeleteItemAsync_Should_Remove_Item_SuccessGuard |
| 3.9 | تحديد تحليل كخارجي — Mark as Outsourced | ✅ | ✅ | ✅ مكتمل (BR-ACC-007) | TestCatalogServiceTests: CreateTestAsync_WithIsSendOutTrue_ShouldPersistFlags, CreateTestAsync_WithNullCostAndPatientPrices_ShouldSucceed + TestCatalogViewModelTests: SaveAsync_With_OutsourceData_Should_Persist_Pricing_LogicGuard + Module3ViewModelTests_Additional: SaveAsync_With_IsSendOut_True_Should_Include_Pricing_Fields_BR_ACC_007 |

---

## الموديول 4 — إدخال النتائج والتقارير | Result Entry & Reporting

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 4.1 | إدخال نتائج التحاليل — Enter Test Results | ❌ | ❌ | ❌ لم يبدأ | |
| 4.2 | حفظ النتائج — Save Results | ❌ | ❌ | ❌ لم يبدأ | |
| 4.3 | تعديل النتائج — Edit Results | ❌ | ❌ | ❌ لم يبدأ | |
| 4.4 | إنشاء تقرير مركّب — Create Composite Report | ❌ | ❌ | ❌ لم يبدأ | |
| 4.5 | ترتيب التقرير — Arrange Report Order | ❌ | ❌ | ❌ لم يبدأ | |
| 4.6 | معاينة التقرير — Preview Report | ❌ | ❌ | ❌ لم يبدأ | |
| 4.7 | طباعة التقرير — Print Report | ❌ | ❌ | ❌ لم يبدأ | |
| 4.8 | طباعة تقرير فارغ — Print Blank Report | ❌ | ❌ | ❌ لم يبدأ | |
| 4.9 | المقارنة مع التاريخ — Compare with History | ❌ | ❌ | ❌ لم يبدأ | |

---

## الموديول 5 — المزارع والحساسية | Culture & Sensitivity

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 5.1 | إدخال بيانات المزرعة — Enter Culture Data | ❌ | ❌ | ❌ لم يبدأ | |
| 5.2 | إضافة مضادات حيوية — Add Antibiotics | ❌ | ❌ | ❌ لم يبدأ | |
| 5.3 | تسجيل الحساسية — Set Sensitivity | ❌ | ❌ | ❌ لم يبدأ | |
| 5.4 | تصنيف الحساسية — Classify Sensitivity | ❌ | ❌ | ❌ لم يبدأ | |
| 5.5 | تصفية مضادات الحوامل — Filter Pregnancy Antibiotics | ❌ | ❌ | ❌ لم يبدأ | |
| 5.6 | تصفية مضادات الأطفال — Filter Children Antibiotics | ❌ | ❌ | ❌ لم يبدأ | |
| 5.7 | طباعة تقرير المزرعة — Print Culture Report | ❌ | ❌ | ❌ لم يبدأ | |

---

## الموديول 6 — سحب العينات | Sample Collection

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 6.1 | تسجيل سحب العينة — Register Sample Collection | ❌ | ❌ | ❌ لم يبدأ | |
| 6.2 | تسجيل فصل العينة — Record Sample Separation | ❌ | ❌ | ❌ لم يبدأ | |
| 6.3 | متابعة حالة العينة — Track Sample Status | ❌ | ❌ | ❌ لم يبدأ | |
| 6.4 | تعليم العينات الخارجية — Mark Taken Outside Lab | ❌ | ❌ | ❌ لم يبدأ | |

---

## الموديول 7 — أوراق العمل | Work Sheets

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 7.1 | إنشاء ورقة عمل المرضى — Generate Patient Worksheet | ❌ | ❌ | ❌ لم يبدأ | |
| 7.2 | إنشاء ورقة عمل التحليل — Generate Test Worksheet | ❌ | ❌ | ❌ لم يبدأ | |
| 7.3 | إنشاء ورقة عمل المجموعة — Generate Group Worksheet | ❌ | ❌ | ❌ لم يبدأ | |
| 7.4 | سجل تصنيف التحاليل — Test Classification LOG | ❌ | ❌ | ❌ لم يبدأ | |

---

## الموديول 8 — المعامل الخارجية | Lab-to-Lab

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 8.1 | تحديد تحليل كخارجي — Mark Test as External | ❌ | ❌ | ❌ لم يبدأ | |
| 8.2 | تسجيل المريض للتحليل الخارجي — Register Patient for External Test | ❌ | ❌ | ❌ لم يبدأ | |
| 8.3 | تجهيز العينة الخارجية — Prepare External Sample | ❌ | ❌ | ❌ لم يبدأ | |
| 8.4 | متابعة حالة العينة الخارجية — Track External Sample Status | ❌ | ❌ | ❌ لم يبدأ | |
| 8.5 | إدخال نتيجة المعمل الخارجي — Enter External Lab Result | ❌ | ❌ | ❌ لم يبدأ | |
| 8.6 | طباعة تقرير المعمل الخارجي — Print External Lab Report | ❌ | ❌ | ❌ لم يبدأ | |
| 8.7 | تسوية حساب المعمل الخارجي — Settle External Lab Account | ❌ | ❌ | ❌ لم يبدأ | |

---

## الموديول 9 — الإحصائيات والتحليلات | Statistics & Analytics

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 9.1 | توزيع المرضى حسب الجنس — Patient Count by Gender | ❌ | ❌ | ❌ لم يبدأ | |
| 9.2 | توزيع المرضى حسب الشهر — Patient Count by Month | ❌ | ❌ | ❌ لم يبدأ | |
| 9.3 | تحليل الطلب على التحاليل — Test Demand Analysis | ❌ | ❌ | ❌ لم يبدأ | |
| 9.4 | عدد العينات سنوياً — Sample Count per Year | ❌ | ❌ | ❌ لم يبدأ | |
| 9.5 | تحليل مصادر الإحالة — Referral Source Analysis | ❌ | ❌ | ❌ لم يبدأ | |
| 9.6 | تقرير إنتاجية المستخدمين — User Productivity Report | ❌ | ❌ | ❌ لم يبدأ | |

---

## الموديول 10 — إدارة المستخدمين والصلاحيات | User Management & Security

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 10.1 | إنشاء مستخدم — Create User | ✅ | ✅ | ✅ مكتمل | CreateUser_WithValidData_ShouldCreateUserAndHashPassword · CreateUser_WithDuplicateUsername_ShouldThrowInvalidOperation · CreateUser_WithEmptyUsername_ShouldThrowArgumentException · CreateUser_WithNullUser_ShouldThrowArgumentNullException · CreateUser_WithNullPassword_ShouldCreateUserWithEmptyHash · GetUsers_ShouldReturnUsersOrderedByUsername · GetUsers_WhenNoUsers_ShouldReturnEmpty · DeleteUser_WhenAdminUser_ShouldThrowInvalidOperation · DeleteUser_WhenUserNotFound_ShouldNotThrow · DeleteUser_ShouldRemoveUserAndUserRoles · SaveUserCommand_WhenNewUserWithValidData_ShouldCreateUserAndAddToList · SaveUserCommand_WhenUsernameIsEmpty_ShouldSetValidationMessage · SaveUserCommand_WhenServiceThrows_ShouldSetErrorMessage · DeleteUserCommand_WhenSelectedUserExists_ShouldCallServiceAndShowSuccess · DeleteUserCommand_WhenServiceThrows_ShouldSetErrorMessage · ReloadCommand_ShouldLoadUsersAndRoles |
| 10.2 | ضبط الصلاحيات — Set Permissions | ✅ | ✅ | ✅ مكتمل (BR-SEC-001) | SetPermissions_WithValidRoleAndCodes_ShouldPersistGrantedPermissions · SetPermissions_WhenReplacingOldPermissions_ShouldRemoveOldAndAddNew · SetPermissions_WithDuplicateCodes_ShouldSaveDistinctOnly · GetRolePermissions_WhenNoPermissions_ShouldReturnEmpty · GetRolePermissions_WithAssignedPermissions_ShouldReturnCorrectCodes · AssignRole_ToUser_ShouldReplaceOldRoles · AssignRole_WhenUserOrRoleMissing_ShouldThrowInvalidOperation · RemoveRole_WhenLinkNotFound_ShouldNotThrow · RemoveRole_WhenAdminUser_ShouldThrowInvalidOperation · RemoveRole_WhenLinkExists_ShouldRemoveSuccessfully · CreateRole_WithValidName_ShouldCreateRoleSuccessfully · CreateRole_WithDuplicateName_ShouldThrowInvalidOperation · CreateRole_WithWhitespaceName_ShouldThrowArgumentException · DeleteRole_WhenRoleAssignedToUser_ShouldThrowInvalidOperation · DeleteRole_ShouldRemoveRoleAndPermissions · DeleteRole_WhenRoleNotFound_ShouldNotThrow · GetRoles_ShouldReturnRolesOrderedByName · GetRoles_WhenNoRoles_ShouldReturnEmpty · SaveRolePermissionsCommand_WhenRoleSelected_ShouldSendGrantedCodes · SaveRolePermissionsCommand_WhenNoRoleSelected_ShouldNotCallService · AssignRoleCommand_WhenUserAndRoleSelected_ShouldCallServiceAndShowSuccess · AssignRoleCommand_WhenServiceThrows_ShouldSetErrorMessage · UnassignRoleCommand_WhenSelectedUserAndRole_ShouldCallServiceAndShowSuccess · UnassignRoleCommand_WhenServiceThrows_ShouldSetErrorMessage · SaveRoleCommand_WithValidRoleName_ShouldCreateRoleAndShowSuccess · SaveRoleCommand_WhenRoleNameIsEmpty_ShouldSetValidationMessage · SaveRoleCommand_WhenServiceThrows_ShouldSetErrorMessage · DeleteRoleCommand_WhenSelectedRoleExists_ShouldCallServiceAndShowSuccess · DeleteRoleCommand_WhenServiceThrows_ShouldSetErrorMessage |
| 10.3 | تعديل بيانات المستخدم — Edit User Data | ✅ | ✅ | ✅ مكتمل | EditUserData_WithValidData_ShouldUpdateFieldsAndHashNewPassword · EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation · EditUserData_WithDuplicateUsername_ShouldThrowInvalidOperation · EditUserData_WithEmptyNewUsername_ShouldThrowInvalidOperation · EditUserData_WithNoNewPassword_ShouldNotChangeExistingHash · SaveUserCommand_WhenExistingUserSelected_ShouldUpdateUserData · SaveUserCommand_WhenUpdateServiceThrows_ShouldSetErrorMessage |
| 10.4 | تسجيل الحضور — Record Attendance | ✅ | ✅ | ✅ مكتمل (BR-SEC-004) | RecordAttendance_WithValidUserId_ShouldCreateLogWithTimestamp · RecordAttendance_WhenOpenLogAlreadyExists_ShouldReturnExistingLog · RecordAttendance_ShouldPersistLogInDatabase · RecordAttendance_GetOpenLog_WhenOpenLogExists_ShouldReturnIt · RecordAttendance_GetOpenLog_WhenNoOpenLog_ShouldReturnNull · RecordAttendance_WithCustomTime_ShouldUseProvidedTime · StartBreak_WhenNoOpenLog_ShouldReturnNull · StartBreak_WhenOpenBreakAlreadyExists_ShouldReturnSameBreak · LoginAsync_WithEmptyCredentials_ShouldSetValidationMessage · LoginAsync_WithInvalidCredentials_ShouldSetFailureMessage · LoginAsync_WithValidAdminCredentials_ShouldSetSessionAndCreateAttendanceRecord · LoginAsync_WithRememberMe_ShouldSaveUsername · LoginCommand_WhenAuthServiceThrows_ShouldSetErrorMessage · LoginCommand_WhenAdminSetupThrows_ShouldSetErrorMessage · Constructor_WhenRememberedUsernameExists_ShouldSetUsernameAndRememberMe |
| 10.5 | تسجيل الانصراف — Record Departure | ✅ | ✅ | ✅ مكتمل (BR-SEC-004) | RecordDeparture_WhenOpenLogExists_ShouldSetLogoutAt · RecordDeparture_WhenNoOpenLog_ShouldReturnNull · RecordDeparture_WhenOpenLogExists_ShouldCloseCorrectLog · RecordDeparture_WithCustomLogoutTime_ShouldUseProvidedTime · RecordDeparture_GetLogsAsync_ShouldFilterByDateRange · LoginAsync_WithoutRememberMe_ShouldClearSavedUsername |
| 10.6 | عرض سجل نشاط المستخدم — View User Activity Log | ✅ | ✅ | ✅ مكتمل (BR-SEC-002) | GetUserActivityLog_WithValidRequest_ShouldReturnRecentActivities · GetUserActivityLog_WhenServiceError_ShouldReturnEmptyOrThrow · GetUserActivityLog_WithUserFilter_ShouldReturnOnlySelectedUserRows · GetUserActivityLog_ShouldRespectCountLimit · SimplifyAuditLog_WithFieldChanges_ShouldDescribeChanges · SimplifyAuditLog_WhenLogNotFound_ShouldReturnNotFoundMessage · LoadCommand_WithValidMaxCount_ShouldPopulateItemsAndShowCount · LoadCommand_WhenMaxCountIsInvalid_ShouldRequestDefault100 · LoadCommand_WhenServiceThrows_ShouldSetErrorMessage · LoadCommand_WhenServiceReturnsEmpty_ShouldShowZeroCount · LoadCommand_WithMultipleRecords_ShouldDisplayAllRecordsWithCorrectData |
| 10.7 | مراقبة استخدام النظام — Monitor System Usage | ✅ | ✅ | ✅ مكتمل | MonitorSystemUsage_GetActiveSessions_ShouldReturnOnlyOpenSessions · MonitorSystemUsage_WhenNoOpenSessions_ShouldReturnEmpty · MonitorSystemUsage_MarkActivity_ShouldUpdateMostRecentOpenSession · MonitorSystemUsage_MarkActivity_WhenNoOpenSession_ShouldNotModifyClosedSession · RefreshCommand_WhenActiveSessions_ShouldPopulateSessionsAndShowCount · RefreshCommand_WhenServiceThrows_ShouldSetErrorMessage · RefreshCommand_WhenNoActiveSessions_ShouldShowZeroCount · RefreshAsync_WhenCalled_ShouldPopulateSessionsCorrectly · RefreshCommand_WithMultipleActiveSessions_ShouldDisplayAllSessions |
| 10.8 | تسجيل الخروج — Logout | ✅ | ✅ | ✅ مكتمل | ValidateCredentials_WithValidHashedPassword_ShouldReturnUser · ValidateCredentials_WhenUserNotFound_ShouldReturnNull · ValidateCredentials_WithLegacyPlainText_ShouldMigrateToHash · ValidateCredentials_WhenUserInactiveAndNotAdmin_ShouldReturnNull · ValidateCredentials_AdminDevelopmentFallback_ShouldResetHashAndReturnUser · ValidateCredentials_WithWrongPassword_ShouldReturnNull · TogglePasswordVisibilityCommand_ShouldToggleIsPasswordVisible · TogglePasswordVisibilityCommand_WhenExecutedTwice_ShouldReturnToInitialState |

---

## الموديول 11 — الحضور والانصراف | HR & Attendance

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 11.1 | تسجيل الحضور — Clock In | ❌ | ❌ | ❌ لم يبدأ | |
| 11.2 | تسجيل الانصراف — Clock Out | ❌ | ❌ | ❌ لم يبدأ | |
| 11.3 | حساب ساعات العمل — Calculate Working Hours | ❌ | ❌ | ❌ لم يبدأ | |
| 11.4 | حساب التأخير — Calculate Tardiness | ❌ | ❌ | ❌ لم يبدأ | |
| 11.5 | إنشاء تقرير الحضور — Generate Attendance Report | ❌ | ❌ | ❌ لم يبدأ | |

---

## الموديول 12 — جهات التعاقد والإحالة | Contracts & Referrals

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 12.1 | إنشاء جهة تعاقد — Create Contract Entity | ❌ | ❌ | ❌ لم يبدأ | |
| 12.2 | ربط قائمة أسعار بالجهة — Link Price List to Entity | ❌ | ❌ | ❌ لم يبدأ | |
| 12.3 | ضبط خصم الجهة — Set Entity Discount | ❌ | ❌ | ❌ لم يبدأ | |
| 12.4 | ضبط عمولة الجهة — Set Entity Commission | ❌ | ❌ | ❌ لم يبدأ | |
| 12.5 | ربط المريض بجهة التعاقد — Assign Patient to Contract | ❌ | ❌ | ❌ لم يبدأ | |
| 12.6 | إضافة طبيب محيل — Add Referring Physician | ❌ | ❌ | ❌ لم يبدأ | |
| 12.7 | ضبط قائمة أسعار الطبيب — Set Physician Price List | ❌ | ❌ | ❌ لم يبدأ | |
| 12.8 | إصدار فاتورة التعاقد — Generate Contract Invoice | ❌ | ❌ | ❌ لم يبدأ | |
| 12.9 | تسوية حساب التعاقد — Settle Contract Account | ❌ | ❌ | ❌ لم يبدأ | |

---

## الموديول 13 — إعدادات النظام والتكوين | System Settings & Configuration

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 13.1 | ضبط هوامش التقرير — Set Report Margins | ❌ | ❌ | ❌ لم يبدأ | |
| 13.2 | ضبط حجم الورق — Set Paper Size | ❌ | ❌ | ❌ لم يبدأ | |
| 13.3 | تكوين الترويسة والتذييل — Configure Header/Footer | ❌ | ❌ | ❌ لم يبدأ | |
| 13.4 | ضبط نوع الحساب الافتراضي — Set Default Account Type | ❌ | ❌ | ❌ لم يبدأ | |
| 13.5 | تكوين الطابعات — Configure Printers | ❌ | ❌ | ❌ لم يبدأ | |
| 13.6 | إعدادات الفاتورة — Set Invoice Settings | ❌ | ❌ | ❌ لم يبدأ | |
| 13.7 | تكوين النسخ الاحتياطي — Configure Backup | ❌ | ❌ | ❌ لم يبدأ | |
| 13.8 | ضبط كلمة مرور النظام — Set System Password | ❌ | ❌ | ❌ لم يبدأ | |

---

## ملخص التقدم الإجمالي

| الموديول | إجمالي الوظائف | مكتمل ✅ | جزئي 🔄 | لم يبدأ ❌ | يحتاج مراجعة ⚠️ |
|----------|---------------|---------|---------|-----------|----------------|
| 1 — إدارة المرضى | 8 | 7 | 0 | 0 | 1 |
| 2 — المحاسبة والمالية | 13 | 13 | 0 | 0 | 0 |
| 3 — إدارة التحاليل والأسعار | 9 | 9 | 0 | 0 | 0 |
| 4 — إدخال النتائج والتقارير | 9 | 0 | 0 | 9 | 0 |
| 5 — المزارع والحساسية | 7 | 0 | 0 | 7 | 0 |
| 6 — سحب العينات | 4 | 0 | 0 | 4 | 0 |
| 7 — أوراق العمل | 4 | 0 | 0 | 4 | 0 |
| 8 — المعامل الخارجية | 7 | 0 | 0 | 7 | 0 |
| 9 — الإحصائيات والتحليلات | 6 | 0 | 0 | 6 | 0 |
| 10 — إدارة المستخدمين | 8 | 8 | 8 | 0 | 8 |
| 11 — الحضور والانصراف | 5 | 0 | 0 | 5 | 0 |
| 12 — جهات التعاقد والإحالة | 9 | 0 | 0 | 9 | 0 |
| 13 — إعدادات النظام | 8 | 0 | 0 | 8 | 0 |
| **الإجمالي** | **97** | **37** | **8** | **59** | **9** |

---

## تعليمات الوكيل البرمجي لتحديث هذا الملف

بعد الانتهاء من كتابة اختبارات أي وظيفة قم بما يلي:

**الخطوة الأولى:** حدِّث خانة Service بـ ✅ إذا كتبت اختبارات النجاح والفشل والحالات الحدية للـ Service.

**الخطوة الثانية:** حدِّث خانة ViewModel بـ ✅ إذا كتبت اختبارات النجاح والفشل والحالات الحدية للـ ViewModel.

**الخطوة الثالثة:** حدِّث عمود الحالة:
- ✅ مكتمل إذا كانت Service و ViewModel كلتاهما ✅
- 🔄 جزئي إذا كانت إحداهما فقط ✅
- ⚠️ يحتاج مراجعة إذا وجدت اختبارات ضعيفة أو وهمية

**الخطوة الرابعة:** أضف أسماء الاختبارات التي كتبتها في عمود "أسماء الاختبارات المكتوبة".

**الخطوة الخامسة:** حدِّث جدول الملخص الإجمالي في نهاية الملف.
