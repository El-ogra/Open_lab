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
| 1.1 | إضافة مريض جديد — Add New Patient | ✅ | ✅ | ✅ مكتمل | AddNewPatient_WithValidData_ShouldCreatePatientAndGenerateLabId, AddNewPatient_WithInvalidGender_ShouldThrowArgumentException, AddNewPatient_WithMissingFullName_ShouldThrowArgumentException, AddNewPatient_WithWhitespacePhone_ShouldStoreNullPhone_EdgeCase, SaveAsync_With_New_Patient_Should_Create_New_Patient_LogicGuard, SaveAsync_When_No_FullName_Should_Show_Error_LogicGuard |
| 1.2 | تعديل بيانات مريض — Edit Patient Data | ✅ | ✅ | ✅ مكتمل | UpdateAsync_Should_Update_All_Fields, UpdateAsync_DuplicateLabId_Should_Throw, UpdateAsync_FutureBirthDate_Should_Throw, DeleteAsync_With_Visits_Should_Throw, SaveAsync_With_Existing_Patient_Should_Update_Patient_LogicGuard, LoadByLabIdAsync_With_Existing_LabId_Should_Load_Patient_Data, DeleteAsync_When_DeleteServiceThrows_Should_Set_ErrorMessage_FailureGuard |
| 1.3 | إضافة تحاليل للمريض — Add Tests to Patient | ✅ | ✅ | ✅ مكتمل | AddTestToVisitAsync_Should_Add_VisitTest_With_Price, AddTestToVisitAsync_Duplicate_Should_Throw, AddTestToVisitAsync_Visit_Closed_Should_Throw, AddTestsToPatient_When_ReferralAndDefaultPriceListExist_Should_PrioritizeReferralPriceList, AddTestsToPatient_When_NoPriceListsExist_Should_Use_BaseTestPrice_EdgeGuard, AddTestCommand_Should_Add_Test_To_Visit_LogicGuard, AddTestCommand_When_Service_Throws_Should_Set_Error_StatusMessage |
| 1.4 | حذف تحاليل — Delete Tests | ✅ | ✅ | ✅ مكتمل | RemoveVisitTestAsync_Pending_Should_Delete_Successfully, RemoveVisitTestAsync_Verified_Should_Throw, RemoveVisitTestAsync_Visit_Closed_Should_Throw, RemoveVisitTestAsync_VisitTest_Not_Found_Should_Not_Throw, RemoveTestCommand_Should_Remove_Test_LogicGuard, RemoveTestCommand_When_Service_Throws_Should_Set_Error_StatusMessage |
| 1.5 | البحث عن مريض — Search Patient | ✅ | ✅ | ✅ مكتمل | SearchPatient_WithMatchingLabId_ShouldReturnPatient, SearchPatient_WhenNoMatch_ShouldReturnEmptyList, SearchPatient_WhenAllFiltersEmpty_ShouldReturnAllOrderedByName, SearchAsync_With_NameAndPhone_Should_Return_Matching_Patients_SuccessGuard, SearchCommand_Should_Search_By_All_Criteria_And_Populate_Results, SearchAsync_Should_Populate_Results_LogicGuard, SearchAsync_With_No_Results_Should_Return_Empty_LogicGuard |
| 1.6 | عرض التاريخ المرضي — View Patient History | ✅ | ✅ | ✅ مكتمل | ViewPatientHistory_WhenVisitsExist_ShouldReturnDescendingByVisitDate, ViewPatientHistory_WhenNoVisits_ShouldReturnEmptyList, ViewPatientHistory_WhenPatientNotFound_ShouldThrowInvalidOperationException, ViewPatientHistory_WithExistingVisits_ShouldReturnVisitsWithinRange, LoadHistoryCommand_Should_Populate_Visits_LogicGuard, SelectedPatient_Setter_Should_Load_Visits, PatientHistoryViewModel_Should_Expose_ReadOnly_Design_Without_EditCommand_DesignEnforcement |
| 1.7 | إضافة تاريخ طبي — Add Medical History | ✅ | ✅ | ✅ مكتمل | SaveMedicalHistoryAsync_Should_Create_New_History, SaveMedicalHistoryAsync_Should_Update_Existing_History, SaveMedicalHistoryAsync_NonExistentPatient_Should_Throw, AddMedicalHistory_WithNullHistory_ShouldThrowArgumentNullException, AddMedicalHistory_WithWhitespaceFields_ShouldStoreNulls_EdgeCase, SaveAsync_With_MedicalHistory_Should_Save_Complete_History_LogicGuard, SaveAsync_When_MedicalHistorySaveFails_Should_Set_Error_Message_FailureGuard |
| 1.8 | إضافة مجموعة تحاليل — Add Group of Tests | ✅ | ✅ | ✅ مكتمل | AddCustomGroupToVisitAsync_Visit_Not_Found_Should_Throw, AddCustomGroupToVisitAsync_CustomGroup_Has_No_Tests_Should_Throw, AddGroupOfTests_WithValidCustomGroup_Should_AddAllTestsToVisit, AddGroupOfTests_WhenSomeTestsAlreadyInVisit_Should_AddOnlyMissingOnes_EdgeGuard, AddCustomGroupCommand_Should_Add_All_Tests_In_Group_LogicGuard, AddCustomGroupCommand_When_Service_Throws_Should_Set_Error_StatusMessage |

---

## الموديول 2 — المحاسبة والمالية | Financial Accounting

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 2.1 | حساب الإجمالي — Calculate Total | ✅ | ✅ | ✅ مكتمل | GetVisitTotalAsync_WithCharges_Returns_Sum_LogicGuard, GetVisitTotalAsync_When_VisitNotFound_Should_Return_Zero_FailureGuard, GetVisitTotalAsync_With_NoTestsOrCharges_Should_Return_Zero_EdgeGuard, CalculateTotalAsync_Should_Call_GetVisitTotalAsync_Directly, LoadVisitCommand_When_VisitNotFound_Should_Show_Error_FailureGuard, LoadVisitCommand_When_VisitHasNoTests_Should_Return_Zero_EdgeGuard, Finance_Gap_Functions_Should_Work |
| 2.2 | تطبيق خصم — Apply Discount | ✅ | ✅ | ✅ مكتمل | CreateOrUpdateInvoiceAsync_WithZeroDiscount_Should_Calculate_Correctly, CreateOrUpdateInvoiceAsync_WithLargeValues_Should_Handle_Correctly, CreateOrUpdateInvoiceAsync_NegativeDiscount_Should_Throw, CreateOrUpdateInvoiceAsync_DiscountExceedsTotal_Should_Throw_LogicGuard, CalculateReferralDiscountAsync_With_Valid_Referral_Should_Apply_Discount_LogicGuard, CalculateReferralDiscountAsync_When_ReferralMissing_Should_Return_Zero_EdgeGuard, ApplyDiscountAsync_Should_Update_NetTotal_Directly, ApplyDiscountAsync_When_DiscountExceedsTotal_Should_Throw_FailureGuard, ApplyDiscountAsync_With_ZeroDiscount_Should_Calculate_Correctly_EdgeGuard |
| 2.3 | تسجيل دفعة — Record Payment | ✅ | ✅ | ✅ مكتمل | CreateOrUpdateInvoiceAsync_NegativePaid_Should_Throw, CreateOrUpdateInvoiceAsync_PartialPayment_Should_Set_Status_To_Partial, AddPaymentAsync_InvalidAmount_Should_Throw_LogicGuard, AddPaymentAsync_PartialPayment_Should_Update_Balance_Correctly, AddPaymentAsync_When_PaymentExceedsBalance_Should_Allow_Overpayment_EdgeGuard, AddPaymentAsync_With_Paid_Zero_Should_Set_StatusMessage, AddPaymentAsync_When_ServiceThrows_Should_Show_Error_FailureGuard, AddPaymentAsync_With_MaximumAmount_Should_Handle_LargeValue_EdgeGuard, AddPaymentAsync_With_ValidAmount_Should_RecordPayment_SuccessGuard, AddPaymentAsync_With_ValidPayment_Should_RecordPayment_SuccessGuard |
| 2.4 | تصفية الحساب — Settle Account | ✅ | ✅ | ✅ مكتمل | CreateOrUpdateInvoiceAsync_FullPayment_Should_Set_Status_To_Paid, SettleAccountAsync_When_BalanceRemaining_Should_Throw_FailureGuard, SettleAccountAsync_When_FullyPaid_Should_CloseVisit_EdgeGuard, SettleAccountAsync_When_AlreadySettled_Should_Handle_Gracefully_EdgeGuard, SettleAccountAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard, SettleAccountAsync_With_FullyPaidInvoice_Should_Settle_SuccessGuard |
| 2.5 | تعديل دفعة — Edit Payment | ✅ | ✅ | ✅ مكتمل | EditPaymentAsync_Should_Update_Amount_And_Recalculate_Invoice_LogicGuard, EditPaymentAsync_When_PaymentNotFound_Should_Throw_FailureGuard, EditPaymentAsync_When_ReasonWhitespace_Should_Throw_EdgeGuard, EditPaymentAsync_When_EditingToSameAmount_Should_Handle_NoChange_EdgeGuard, EditPaymentAsync_When_ServiceThrows_Should_Show_Error_FailureGuard, EditPaymentAsync_With_InvalidAmount_Should_Set_ValidationMessage_FailureGuard, EditPaymentAsync_With_ValidAmount_Should_EditPayment_SuccessGuard |
| 2.6 | حذف دفعة — Delete Payment | ✅ | ✅ | ✅ مكتمل | DeletePaymentAsync_Should_Recalculate_Balance, DeletePaymentAsync_Without_Reason_Should_Throw, DeletePaymentAsync_NonExistent_Should_Return_Without_Throwing, DeletePaymentAsync_With_Null_SelectedPayment_Should_Return, DeletePaymentAsync_With_Valid_SelectedPayment_Should_Delete, DeletePaymentAsync_When_ServiceThrows_Should_Show_Error_FailureGuard, DeletePaymentAsync_When_DeletingLastPayment_Should_UpdateBalanceCorrectly_EdgeGuard |
| 2.7 | إضافة رسوم إضافية — Add Additional Charge | ✅ | ✅ | ✅ مكتمل | AddAdditionalChargeAsync_Should_Create_Charge_Record, AddAdditionalChargeAsync_InvalidDescription_Should_Throw_LogicGuard, AddAdditionalChargeAsync_NegativeAmount_Should_Throw, AddAdditionalChargeAsync_With_DescriptionWhitespaceAround_Should_Trim_EdgeGuard, AddChargeAsync_With_VisitId_Zero_Should_Return, AddChargeAsync_With_Valid_Data_Should_Add_Charge, AddChargeAsync_When_ServiceThrows_Should_Show_Error_FailureGuard, AddChargeAsync_With_ZeroAmount_Should_Handle_Gracefully_EdgeGuard |
| 2.8 | إصدار فاتورة — Generate Invoice | ✅ | ✅ | ✅ مكتمل | CreateOrUpdateInvoiceAsync_Should_Create_Invoice_With_Correct_Totals_LogicGuard, LogInvoicePrintedAsync_Should_Create_AuditLog_Entry, LogInvoicePrintedAsync_With_InvalidInvoiceId_Should_Still_Create_Log_FailureGuard, LogInvoicePrintedAsync_With_ZeroUserId_Should_Create_Log_EdgeGuard, SaveInvoiceAsync_With_VisitId_Zero_Should_Set_StatusMessage, SaveInvoiceAsync_With_Valid_Visit_Should_Save, PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard, PrintInvoiceAsync_When_LogThrows_Should_Set_ErrorStatus_FailureGuard |
| 2.9 | كشف حساب المريض — View Patient Account | ✅ | ✅ | ✅ مكتمل | GetPatientAccount_ByDate_Should_Return_Invoices_And_Payments_For_Same_Patient_LogicGuard, GetPatientAccount_When_PatientNotFound_Should_Return_Empty_FailureGuard, GetPatientAccount_With_DateRangeOutside_VisitDate_Should_Return_Empty_EdgeGuard, Commands_When_Admin_Should_Be_Enabled, LoadVisitAsync_With_VisitId_Zero_Should_Set_StatusMessage, LoadVisitAsync_With_Existing_Invoice_Should_Load_Data, LoadVisitAsync_When_VisitHasNoInvoice_Should_Create_New_EdgeGuard, LoadVisitAsync_When_ServiceThrows_Should_Set_StatusMessage_FailureGuard |
| 2.10 | تقرير الجرد المالي — Generate Inventory | ✅ | ✅ | ✅ مكتمل | GetSnapshotAsync_Overall_Should_Aggregate_Invoices_And_Payments_LogicGuard, GetSnapshotAsync_WithInvalidDates_ShouldThrowException_FailureGuard, GetSnapshotAsync_When_NoDataInRange_Should_Return_ZeroedSnapshot_EdgeGuard, LoadCommand_Should_SetIsLoading_And_LoadData_SuccessGuard, LoadCommand_Failure_Should_HandleException_FailureGuard, LoadCommand_With_EmptySnapshot_Should_Keep_ZeroTotals_EdgeGuard, DailyCommand_Should_LoadDataForToday_SuccessGuard, DailyCommand_When_ServiceThrows_Should_Set_ErrorMessage_FailureGuard, WeeklyCommand_Should_CalculateStartOfWeek_SuccessGuard, MonthlyCommand_Should_CalculateStartOfMonth_SuccessGuard, PrintCommand_Should_CallPrintService_SuccessGuard, PrintCommand_Failure_Should_HandleException_FailureGuard |
| 2.11 | جرد مالي للفرع — Branch-wise Inventory | ✅ | ✅ | ✅ مكتمل | GetSnapshotAsync_With_BranchFilter_Should_Isolate_Branch_Data_LogicGuard, GetSnapshotAsync_With_NonExistentBranch_Should_Return_ZeroedSnapshot_FailureGuard, GetSnapshotAsync_With_BranchFilter_And_NoData_Should_Return_ZeroedSnapshot_EdgeGuard, LoadCommand_With_BranchFilter_Should_Call_GetSnapshotAsync_With_BranchId, LoadCommand_With_BranchFilter_When_ServiceThrows_Should_Show_Error_FailureGuard, LoadCommand_With_NoBranchSelected_Should_Load_AllBranches_EdgeGuard, StatisticsService_BranchWiseInventory_Should_Filter_By_Branch |
| 2.12 | حساب الأطباء — Doctor-wise Inventory | ✅ | ✅ | ✅ مكتمل | GetSnapshotAsync_Should_Calculate_Doctor_Commissions_LogicGuard, GetSnapshotAsync_With_ZeroCommissionPercentage_Should_Calculate_ZeroCommission_EdgeGuard, GetSnapshotAsync_DoctorWise_When_DateRangeInvalid_Should_Throw_FailureGuard, LoadCommand_Should_Calculate_Doctor_Commissions_Directly, LoadCommand_When_ServiceThrows_DoctorCommissions_Should_Show_Error_FailureGuard, LoadCommand_With_NoDoctorCommissions_Should_Return_EmptyList_EdgeGuard |
| 2.13 | تصفية حسابات المعامل الخارجية — Lab-to-Lab Settlement | ✅ | ✅ | ✅ مكتمل | GetTotalProfitAsync_Should_Calculate_PatientPrice_Minus_CostPrice_LogicGuard, CreateSettlementAsync_Should_Update_Balance_LogicGuard, GetPendingBalanceAsync_When_NoQueueOrPayments_Should_Return_Zero_FailureGuard, CreateSettlementAsync_When_AmountPaid_Exceeds_Balance_Should_Store_NegativeBalance_EdgeGuard, LoadReferralsCommand_When_Executed_Should_Load_ExternalLab_Referrals_Success, LoadReferralsCommand_When_Service_Throws_Should_Set_Error_Message_Failure, LoadQueueAsync_Should_Map_Row_With_VisitTest_For_Printing, LoadQueueCommand_When_No_Pending_Items_Should_Clear_Queue_Edge, LoadQueueCommand_When_Service_Throws_Should_Set_Error_Message_Failure, CreateManifestCommand_With_Valid_Data_Should_Create_Manifest_Success, CreateManifestCommand_When_Referral_Missing_Should_Not_Call_Service_Failure, CreateManifestCommand_When_Queue_Selection_Empty_Should_Not_Call_Service_Edge, PrintExternalReportAsync_Should_Print_When_Visit_Is_Available, EnterExternalResultAsync_Should_Call_Service_And_Reset_Input, UpdateStatusCommand_When_SelectedItem_Exists_Should_Call_Service_Success, UpdateStatusCommand_When_No_Selected_Item_Should_Not_Call_Service_Edge, UpdateStatusCommand_When_Service_Throws_Should_Set_Error_Message_Failure, LoadManifestsCommand_When_Executed_Should_Load_Manifest_List_Success, LoadManifestsCommand_When_Service_Throws_Should_Set_Error_Message_Failure, LoadSettlementCommand_When_Referral_Selected_Should_Load_Settlement_Success, LoadSettlementCommand_When_Referral_Not_Selected_Should_Not_Call_Service_Edge, LoadSettlementCommand_When_Service_Throws_Should_Set_Error_Message_Failure, LoadSettlementAsync_Should_Load_History_And_TotalProfit, CreateSettlementCommand_When_Valid_Data_Should_Create_And_Reset_Amount_Success, CreateSettlementCommand_When_Service_Throws_Should_Set_Error_Message_Failure, CreateSettlementAsync_When_AmountIsZero_Should_NotCall_Service_EdgeGuard, ExternalSettlementService_Should_Calculate_Correct_Balance, PrintExternalReportCommand_When_VisitId_Invalid_Should_Set_User_Message_Failure, PrintExternalReportCommand_When_Report_Not_Found_Should_Set_NotFound_Message_Edge, EnterExternalResultCommand_When_Result_Value_Empty_Should_Not_Call_Service_Edge, EnterExternalResultAsync_When_ServiceThrows_Should_Set_ErrorMessage_FailureGuard |

---

## الموديول 3 — إدارة التحاليل والأسعار | Test & Price Management

| رقم الوظيفة | اسم الوظيفة | Service ✅/❌ | ViewModel ✅/❌ | الحالة | أسماء الاختبارات المكتوبة |
|-------------|-------------|--------------|----------------|--------|--------------------------|
| 3.1 | إضافة تحليل جديد — Add New Test | ❌ | ❌ | ❌ لم يبدأ | |
| 3.2 | تعديل بيانات تحليل — Edit Test Data | ❌ | ❌ | ❌ لم يبدأ | |
| 3.3 | تحديد القيم المرجعية — Set Reference Values | ❌ | ❌ | ❌ لم يبدأ | |
| 3.4 | إضافة تعليقات القيم المرتفعة/المنخفضة — Add Low/High Comments | ❌ | ❌ | ❌ لم يبدأ | |
| 3.5 | إنشاء مجموعة مخصصة — Create Custom Group | ❌ | ❌ | ❌ لم يبدأ | |
| 3.6 | إضافة تعليقات التحليل — Add Test Comments | ❌ | ❌ | ❌ لم يبدأ | |
| 3.7 | إنشاء قائمة أسعار — Create Price List | ❌ | ❌ | ❌ لم يبدأ | |
| 3.8 | تحديث الأسعار — Update Prices | ❌ | ❌ | ❌ لم يبدأ | |
| 3.9 | تحديد تحليل كخارجي — Mark as Outsourced | ❌ | ❌ | ❌ لم يبدأ | |

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
| 10.1 | إنشاء مستخدم — Create User | ❌ | ❌ | ❌ لم يبدأ | |
| 10.2 | ضبط الصلاحيات — Set Permissions | ❌ | ❌ | ❌ لم يبدأ | |
| 10.3 | تعديل بيانات المستخدم — Edit User Data | ❌ | ❌ | ❌ لم يبدأ | |
| 10.4 | تسجيل الحضور — Record Attendance | ❌ | ❌ | ❌ لم يبدأ | |
| 10.5 | تسجيل الانصراف — Record Departure | ❌ | ❌ | ❌ لم يبدأ | |
| 10.6 | عرض سجل نشاط المستخدم — View User Activity Log | ❌ | ❌ | ❌ لم يبدأ | |
| 10.7 | مراقبة استخدام النظام — Monitor System Usage | ❌ | ❌ | ❌ لم يبدأ | |
| 10.8 | تسجيل الخروج — Logout | ❌ | ❌ | ❌ لم يبدأ | |

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
| 1 — إدارة المرضى | 8 | 8 | 0 | 0 | 0 |
| 2 — المحاسبة والمالية | 13 | 13 | 0 | 0 | 0 |
| 3 — إدارة التحاليل والأسعار | 9 | 0 | 0 | 9 | 0 |
| 4 — إدخال النتائج والتقارير | 9 | 0 | 0 | 9 | 0 |
| 5 — المزارع والحساسية | 7 | 0 | 0 | 7 | 0 |
| 6 — سحب العينات | 4 | 0 | 0 | 4 | 0 |
| 7 — أوراق العمل | 4 | 0 | 0 | 4 | 0 |
| 8 — المعامل الخارجية | 7 | 0 | 0 | 7 | 0 |
| 9 — الإحصائيات والتحليلات | 6 | 0 | 0 | 6 | 0 |
| 10 — إدارة المستخدمين | 8 | 0 | 0 | 8 | 0 |
| 11 — الحضور والانصراف | 5 | 0 | 0 | 5 | 0 |
| 12 — جهات التعاقد والإحالة | 9 | 0 | 0 | 9 | 0 |
| 13 — إعدادات النظام | 8 | 0 | 0 | 8 | 0 |
| **الإجمالي** | **97** | **21** | **0** | **76** | **0** |

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
