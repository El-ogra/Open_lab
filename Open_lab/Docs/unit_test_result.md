========== Starting test discovery ==========
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:03.30]   Discovering: Open_lab.Tests
[xUnit.net 00:00:04.41]   Discovered:  Open_lab.Tests
========== Test discovery finished: 1558 Tests found in 7.7 sec ==========
========== Starting test run ==========
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:01.17]   Starting:    Open_lab.Tests
[xUnit.net 00:00:11.69]     Open_lab.Tests.ViewModels.ReferenceRangesViewModelTests.DeleteAsync_When_NoSelection_Should_NotCall_Service_EdgeGuard [FAIL]
[xUnit.net 00:00:11.70]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:11.70]       Stack Trace:
[xUnit.net 00:00:11.70]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:11.70]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:11.70]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:11.70]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:11.70]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:11.70]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:11.70]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:11.70]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ReferenceRangesViewModelTests.cs(116,0): at Open_lab.Tests.ViewModels.ReferenceRangesViewModelTests.DeleteAsync_When_NoSelection_Should_NotCall_Service_EdgeGuard()
[xUnit.net 00:00:11.70]         --- End of stack trace from previous location ---
[xUnit.net 00:00:16.68]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.DeletePaymentAsync_With_Null_SelectedPayment_Should_Return [FAIL]
[xUnit.net 00:00:16.68]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:16.68]       Stack Trace:
[xUnit.net 00:00:16.68]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:16.68]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:16.68]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:16.68]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:16.68]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:16.69]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:16.69]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:16.69]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(230,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.DeletePaymentAsync_With_Null_SelectedPayment_Should_Return()
[xUnit.net 00:00:16.69]         --- End of stack trace from previous location ---
[xUnit.net 00:00:17.05]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.AddChargeAsync_With_VisitId_Zero_Should_Return [FAIL]
[xUnit.net 00:00:17.05]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:17.06]       Stack Trace:
[xUnit.net 00:00:17.06]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:17.06]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:17.06]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:17.06]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:17.06]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:17.06]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:17.06]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:17.06]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(350,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.AddChargeAsync_With_VisitId_Zero_Should_Return()
[xUnit.net 00:00:17.06]         --- End of stack trace from previous location ---
[xUnit.net 00:00:18.06]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard [FAIL]
[xUnit.net 00:00:18.06]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:18.07]       Stack Trace:
[xUnit.net 00:00:18.07]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:18.07]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:18.07]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:18.07]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:18.07]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:18.07]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:18.07]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:18.07]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(504,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard()
[xUnit.net 00:00:18.07]         --- End of stack trace from previous location ---
[xUnit.net 00:00:39.04]     Open_lab.Tests.Module5ViewModelTests_Additional.ClassifySensitivity_WithValidSIRValues_ShouldClassifyEverySavedValue [FAIL]
[xUnit.net 00:00:39.04]       Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "تم حفظ نتيجة المزرعة".
[xUnit.net 00:00:39.04]       Stack Trace:
[xUnit.net 00:00:39.04]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:39.04]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:39.04]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:39.04]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:39.04]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module5ViewModelTests_Additional.cs(390,0): at Open_lab.Tests.Module5ViewModelTests_Additional.ClassifySensitivity_WithValidSIRValues_ShouldClassifyEverySavedValue()
[xUnit.net 00:00:39.04]         --- End of stack trace from previous location ---
[xUnit.net 00:00:40.35]     Open_lab.Tests.Module5ViewModelTests_Additional.SetSensitivity_WithSensitivityRows_ShouldSaveCultureResults [FAIL]
[xUnit.net 00:00:40.35]       Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "تم حفظ نتيجة المزرعة".
[xUnit.net 00:00:40.35]       Stack Trace:
[xUnit.net 00:00:40.35]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:40.35]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:40.35]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:40.35]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:40.35]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module5ViewModelTests_Additional.cs(308,0): at Open_lab.Tests.Module5ViewModelTests_Additional.SetSensitivity_WithSensitivityRows_ShouldSaveCultureResults()
[xUnit.net 00:00:40.35]         --- End of stack trace from previous location ---
[xUnit.net 00:01:01.90]     Open_lab.Tests.ViewModels.PatientHistoryViewModelTests.PrintHistoryAsync_When_HistoryIsNull_Should_NotCall_PrintService_EdgeGuard [FAIL]
[xUnit.net 00:01:01.90]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:01.90]       Stack Trace:
[xUnit.net 00:01:01.90]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:01.90]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:01.90]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:01.90]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:01.90]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientHistoryViewModelTests.cs(119,0): at Open_lab.Tests.ViewModels.PatientHistoryViewModelTests.PrintHistoryAsync_When_HistoryIsNull_Should_NotCall_PrintService_EdgeGuard()
[xUnit.net 00:01:01.90]         --- End of stack trace from previous location ---
[xUnit.net 00:01:03.76]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+PriceListsViewModel_AdditionalTests.UpdateListAsync_When_Null_Selected_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:01:03.76]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:03.76]       Stack Trace:
[xUnit.net 00:01:03.76]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:03.76]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:03.76]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:03.76]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:03.76]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(905,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.PriceListsViewModel_AdditionalTests.UpdateListAsync_When_Null_Selected_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:01:03.76]         --- End of stack trace from previous location ---
[xUnit.net 00:01:05.13]     Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service [FAIL]
[xUnit.net 00:01:05.13]       Expected _viewModel.StatusMessage "تم تحديث الحالة." to contain "تم تعليم العينة كغير مجمعة".
[xUnit.net 00:01:05.13]       Stack Trace:
[xUnit.net 00:01:05.13]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:05.13]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:05.13]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:05.13]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:05.13]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SampleCollectionViewModelTests.cs(227,0): at Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service()
[xUnit.net 00:01:05.13]         --- End of stack trace from previous location ---
[xUnit.net 00:01:39.26]     Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.ViewPatientHistory_MultipleVisits_ShouldPopulateVisitsList_SuccessGuard [FAIL]
[xUnit.net 00:01:39.26]       Expected vm.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:39.26]       Stack Trace:
[xUnit.net 00:01:39.26]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:39.26]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:39.26]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:39.26]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:39.26]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module1And2ViewModelCompletionTests.cs(256,0): at Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.ViewPatientHistory_MultipleVisits_ShouldPopulateVisitsList_SuccessGuard()
[xUnit.net 00:01:39.26]         --- End of stack trace from previous location ---
[xUnit.net 00:01:39.91]     Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.SettleAccount_WhenVisitIdIsZero_ShouldNotCallService_EdgeGuard [FAIL]
[xUnit.net 00:01:39.91]       Expected vm.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:39.91]       Stack Trace:
[xUnit.net 00:01:39.91]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:39.91]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:39.91]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:39.91]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:39.91]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module1And2ViewModelCompletionTests.cs(453,0): at Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.SettleAccount_WhenVisitIdIsZero_ShouldNotCallService_EdgeGuard()
[xUnit.net 00:01:39.91]         --- End of stack trace from previous location ---
[xUnit.net 00:01:53.80]   Finished:    Open_lab.Tests
========== Test run finished: 1558 Tests (1547 Passed, 11 Failed, 0 Skipped) run in 1.9 min ==========
