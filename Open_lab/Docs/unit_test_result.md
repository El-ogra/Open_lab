========== Starting test run ==========
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:00.84]   Starting:    Open_lab.Tests
[xUnit.net 00:00:09.04]     Open_lab.Tests.Module6ViewModelTests_Additional.MarkExternalCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:00:09.04]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:09.04]       Stack Trace:
[xUnit.net 00:00:09.04]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:09.04]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:09.04]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:09.04]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.04]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.04]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:09.04]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:09.04]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module6ViewModelTests_Additional.cs(325,0): at Open_lab.Tests.Module6ViewModelTests_Additional.MarkExternalCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:00:09.05]         --- End of stack trace from previous location ---
[xUnit.net 00:00:30.80]     Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service [FAIL]
[xUnit.net 00:00:30.80]       Expected _viewModel.StatusMessage "تم تحديث الحالة." to contain "تم تعليم العينة كغير مجمعة".
[xUnit.net 00:00:30.80]       Stack Trace:
[xUnit.net 00:00:30.80]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:30.80]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:30.80]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:30.80]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:30.80]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SampleCollectionViewModelTests.cs(227,0): at Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service()
[xUnit.net 00:00:30.80]         --- End of stack trace from previous location ---
[xUnit.net 00:00:30.81]     Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:30.81]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:30.81]       Stack Trace:
[xUnit.net 00:00:30.81]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:30.81]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:30.81]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:30.81]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:30.81]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SampleCollectionViewModelTests.cs(119,0): at Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing()
[xUnit.net 00:00:30.81]         --- End of stack trace from previous location ---
[xUnit.net 00:00:39.47]     Open_lab.Tests.ViewModels.ReferenceRangesViewModelTests.DeleteAsync_When_NoSelection_Should_NotCall_Service_EdgeGuard [FAIL]
[xUnit.net 00:00:39.47]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:39.47]       Stack Trace:
[xUnit.net 00:00:39.47]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:39.47]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:39.47]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:39.47]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:39.47]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ReferenceRangesViewModelTests.cs(116,0): at Open_lab.Tests.ViewModels.ReferenceRangesViewModelTests.DeleteAsync_When_NoSelection_Should_NotCall_Service_EdgeGuard()
[xUnit.net 00:00:39.47]         --- End of stack trace from previous location ---
[xUnit.net 00:00:41.95]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.VerifyResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:41.95]       Expected (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")) to be true, but found False.
[xUnit.net 00:00:41.95]       Stack Trace:
[xUnit.net 00:00:41.95]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:41.95]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:41.95]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:41.95]            at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
[xUnit.net 00:00:41.95]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(108,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.VerifyResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
[xUnit.net 00:00:41.95]         --- End of stack trace from previous location ---
[xUnit.net 00:00:41.97]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.ReopenResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:41.97]       Expected (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")) to be true, but found False.
[xUnit.net 00:00:41.97]       Stack Trace:
[xUnit.net 00:00:41.97]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:41.97]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:41.97]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:41.97]            at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
[xUnit.net 00:00:41.97]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(174,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.ReopenResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
[xUnit.net 00:00:41.97]         --- End of stack trace from previous location ---
[xUnit.net 00:00:41.98]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.SaveResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:41.98]       Expected (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")) to be true, but found False.
[xUnit.net 00:00:41.98]       Stack Trace:
[xUnit.net 00:00:41.98]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:41.98]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:41.98]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:41.98]            at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
[xUnit.net 00:00:41.98]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(77,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.SaveResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
[xUnit.net 00:00:41.98]         --- End of stack trace from previous location ---
[xUnit.net 00:01:10.96]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.DeletePaymentAsync_With_Null_SelectedPayment_Should_Return [FAIL]
[xUnit.net 00:01:10.96]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:10.96]       Stack Trace:
[xUnit.net 00:01:10.96]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:10.96]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:10.96]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:10.96]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:10.96]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(230,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.DeletePaymentAsync_With_Null_SelectedPayment_Should_Return()
[xUnit.net 00:01:10.97]         --- End of stack trace from previous location ---
[xUnit.net 00:01:11.46]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.AddChargeAsync_With_VisitId_Zero_Should_Return [FAIL]
[xUnit.net 00:01:11.46]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:11.46]       Stack Trace:
[xUnit.net 00:01:11.46]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:11.46]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:11.46]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:11.46]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:11.46]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(350,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.AddChargeAsync_With_VisitId_Zero_Should_Return()
[xUnit.net 00:01:11.46]         --- End of stack trace from previous location ---
[xUnit.net 00:01:12.37]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard [FAIL]
[xUnit.net 00:01:12.37]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:12.37]       Stack Trace:
[xUnit.net 00:01:12.37]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:12.37]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:12.37]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:12.37]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:12.37]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(504,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard()
[xUnit.net 00:01:12.37]         --- End of stack trace from previous location ---
[xUnit.net 00:01:21.96]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+PriceListsViewModel_AdditionalTests.UpdateListAsync_When_Null_Selected_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:01:21.96]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:21.96]       Stack Trace:
[xUnit.net 00:01:21.96]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:21.96]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:21.96]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:21.96]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:21.96]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(905,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.PriceListsViewModel_AdditionalTests.UpdateListAsync_When_Null_Selected_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:01:21.96]         --- End of stack trace from previous location ---
[xUnit.net 00:01:22.05]     Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithValidAdminCredentials_ShouldSetSessionAndCreateAttendanceRecord [FAIL]
[xUnit.net 00:01:22.05]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:22.05]       Stack Trace:
[xUnit.net 00:01:22.05]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:22.05]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:22.05]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:22.05]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:22.05]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\LoginViewModelTests.cs(165,0): at Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithValidAdminCredentials_ShouldSetSessionAndCreateAttendanceRecord()
[xUnit.net 00:01:22.05]         --- End of stack trace from previous location ---
[xUnit.net 00:01:22.06]     Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithRememberMe_ShouldSaveUsername [FAIL]
[xUnit.net 00:01:22.06]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:22.06]       Stack Trace:
[xUnit.net 00:01:22.06]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:22.06]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:22.06]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:22.06]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:22.06]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\LoginViewModelTests.cs(195,0): at Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithRememberMe_ShouldSaveUsername()
[xUnit.net 00:01:22.06]         --- End of stack trace from previous location ---
[xUnit.net 00:01:22.06]     Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithoutRememberMe_ShouldClearSavedUsername [FAIL]
[xUnit.net 00:01:22.07]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:22.07]       Stack Trace:
[xUnit.net 00:01:22.07]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:22.07]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:22.07]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:22.07]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:22.07]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\LoginViewModelTests.cs(225,0): at Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithoutRememberMe_ShouldClearSavedUsername()
[xUnit.net 00:01:22.07]         --- End of stack trace from previous location ---
[xUnit.net 00:01:25.89]     Open_lab.Tests.ViewModels.Module4ViewModelCompletion_Part3.PrintReport_PrintCommand_When_No_Report_Should_Do_Nothing_Edge [FAIL]
[xUnit.net 00:01:25.89]       Expected viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:25.89]       Stack Trace:
[xUnit.net 00:01:25.89]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:25.89]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:25.89]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:25.89]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:25.89]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module4ViewModelCompletion_Part3.cs(235,0): at Open_lab.Tests.ViewModels.Module4ViewModelCompletion_Part3.PrintReport_PrintCommand_When_No_Report_Should_Do_Nothing_Edge()
[xUnit.net 00:01:25.89]         --- End of stack trace from previous location ---
[xUnit.net 00:01:28.89]     Open_lab.Tests.Module5ViewModelTests_Additional.ClassifySensitivity_WithValidSIRValues_ShouldClassifyEverySavedValue [FAIL]
[xUnit.net 00:01:28.89]       Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "تم حفظ نتيجة المزرعة".
[xUnit.net 00:01:28.89]       Stack Trace:
[xUnit.net 00:01:28.89]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:28.89]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:28.89]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:28.89]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:28.89]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module5ViewModelTests_Additional.cs(370,0): at Open_lab.Tests.Module5ViewModelTests_Additional.ClassifySensitivity_WithValidSIRValues_ShouldClassifyEverySavedValue()
[xUnit.net 00:01:28.89]         --- End of stack trace from previous location ---
[xUnit.net 00:01:29.65]     Open_lab.Tests.Module5ViewModelTests_Additional.SetSensitivity_WithSensitivityRows_ShouldSaveCultureResults [FAIL]
[xUnit.net 00:01:29.65]       Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "تم حفظ نتيجة المزرعة".
[xUnit.net 00:01:29.65]       Stack Trace:
[xUnit.net 00:01:29.65]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:29.65]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:29.65]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:29.65]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:29.65]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module5ViewModelTests_Additional.cs(288,0): at Open_lab.Tests.Module5ViewModelTests_Additional.SetSensitivity_WithSensitivityRows_ShouldSaveCultureResults()
[xUnit.net 00:01:29.65]         --- End of stack trace from previous location ---
[xUnit.net 00:01:29.89]     Open_lab.Tests.ViewModels.SystemSettingsViewModelTests.ReloadCommand_When_Executed_Should_Refresh_Profile_And_Settings_Success [FAIL]
[xUnit.net 00:01:29.89]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:29.89]       Stack Trace:
[xUnit.net 00:01:29.89]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:29.89]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:29.89]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:29.89]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:29.89]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SystemSettingsViewModelTests.cs(125,0): at Open_lab.Tests.ViewModels.SystemSettingsViewModelTests.ReloadCommand_When_Executed_Should_Refresh_Profile_And_Settings_Success()
[xUnit.net 00:01:29.89]         --- End of stack trace from previous location ---
[xUnit.net 00:01:30.75]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+TestCommentsViewModel_AdditionalTests.DeleteAsync_When_Null_Selection_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:01:30.75]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:30.75]       Stack Trace:
[xUnit.net 00:01:30.75]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:30.75]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:30.75]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:30.75]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:30.75]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(1031,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.TestCommentsViewModel_AdditionalTests.DeleteAsync_When_Null_Selection_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:01:30.75]         --- End of stack trace from previous location ---
[xUnit.net 00:01:36.22]     Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.ViewPatientHistory_MultipleVisits_ShouldPopulateVisitsList_SuccessGuard [FAIL]
[xUnit.net 00:01:36.22]       Expected vm.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:36.22]       Stack Trace:
[xUnit.net 00:01:36.22]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:36.22]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:36.22]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:36.22]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:36.22]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module1And2ViewModelCompletionTests.cs(256,0): at Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.ViewPatientHistory_MultipleVisits_ShouldPopulateVisitsList_SuccessGuard()
[xUnit.net 00:01:36.22]         --- End of stack trace from previous location ---
[xUnit.net 00:01:36.89]     Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.SettleAccount_WhenVisitIdIsZero_ShouldNotCallService_EdgeGuard [FAIL]
[xUnit.net 00:01:36.89]       Expected vm.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:36.89]       Stack Trace:
[xUnit.net 00:01:36.89]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:36.89]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:36.89]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:36.89]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:36.89]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module1And2ViewModelCompletionTests.cs(453,0): at Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.SettleAccount_WhenVisitIdIsZero_ShouldNotCallService_EdgeGuard()
[xUnit.net 00:01:36.89]         --- End of stack trace from previous location ---
[xUnit.net 00:01:43.45]   Finished:    Open_lab.Tests
========== Test run finished: 1523 Tests (1502 Passed, 21 Failed, 0 Skipped) run in 1.7 min ==========
