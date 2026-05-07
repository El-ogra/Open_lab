========== Starting test discovery ==========
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:05.10]   Discovering: Open_lab.Tests
[xUnit.net 00:00:06.46]   Discovered:  Open_lab.Tests
========== Test discovery finished: 1558 Tests found in 9.4 sec ==========
========== Starting test run ==========
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:01.48]   Starting:    Open_lab.Tests
[xUnit.net 00:00:07.52]     Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service [FAIL]
[xUnit.net 00:00:07.52]       Expected _viewModel.StatusMessage "تم تحديث الحالة." to contain "تم تعليم العينة كغير مجمعة".
[xUnit.net 00:00:07.53]       Stack Trace:
[xUnit.net 00:00:07.53]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:07.53]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:07.53]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:07.53]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:07.53]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:07.53]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:07.53]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:07.53]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SampleCollectionViewModelTests.cs(227,0): at Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service()
[xUnit.net 00:00:07.53]         --- End of stack trace from previous location ---
[xUnit.net 00:00:07.54]     Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:07.54]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:07.54]       Stack Trace:
[xUnit.net 00:00:07.54]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:07.54]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:07.54]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:07.54]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:07.54]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:07.54]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:07.54]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:07.54]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SampleCollectionViewModelTests.cs(119,0): at Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing()
[xUnit.net 00:00:07.54]         --- End of stack trace from previous location ---
[xUnit.net 00:00:12.60]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+PriceListsViewModel_AdditionalTests.UpdateListAsync_When_Null_Selected_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:00:12.60]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:12.60]       Stack Trace:
[xUnit.net 00:00:12.60]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:12.60]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:12.60]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:12.60]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:12.60]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:12.60]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:12.60]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:12.60]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(905,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.PriceListsViewModel_AdditionalTests.UpdateListAsync_When_Null_Selected_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:00:12.60]         --- End of stack trace from previous location ---
[xUnit.net 00:00:19.16]     Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.ViewPatientHistory_MultipleVisits_ShouldPopulateVisitsList_SuccessGuard [FAIL]
[xUnit.net 00:00:19.16]       Expected vm.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:19.16]       Stack Trace:
[xUnit.net 00:00:19.16]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:19.16]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:19.16]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:19.16]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:19.16]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:19.16]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:19.16]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:19.16]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module1And2ViewModelCompletionTests.cs(256,0): at Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.ViewPatientHistory_MultipleVisits_ShouldPopulateVisitsList_SuccessGuard()
[xUnit.net 00:00:19.16]         --- End of stack trace from previous location ---
[xUnit.net 00:00:20.13]     Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.SettleAccount_WhenVisitIdIsZero_ShouldNotCallService_EdgeGuard [FAIL]
[xUnit.net 00:00:20.13]       Expected vm.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:20.13]       Stack Trace:
[xUnit.net 00:00:20.13]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:20.13]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:20.13]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:20.13]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:20.14]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:20.14]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:20.14]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:20.14]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module1And2ViewModelCompletionTests.cs(453,0): at Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.SettleAccount_WhenVisitIdIsZero_ShouldNotCallService_EdgeGuard()
[xUnit.net 00:00:20.14]         --- End of stack trace from previous location ---
[xUnit.net 00:00:30.92]     Open_lab.Tests.ViewModels.Module4ViewModelCompletion_Part3.PrintReport_PrintCommand_When_No_Report_Should_Do_Nothing_Edge [FAIL]
[xUnit.net 00:00:30.92]       Expected viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:30.92]       Stack Trace:
[xUnit.net 00:00:30.92]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:30.92]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:30.92]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:30.92]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:30.92]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:30.92]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:30.92]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:30.92]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module4ViewModelCompletion_Part3.cs(221,0): at Open_lab.Tests.ViewModels.Module4ViewModelCompletion_Part3.PrintReport_PrintCommand_When_No_Report_Should_Do_Nothing_Edge()
[xUnit.net 00:00:30.92]         --- End of stack trace from previous location ---
[xUnit.net 00:00:35.05]     Open_lab.Tests.Module5ViewModelTests_Additional.FilterPregnancyAntibiotics_When_Exception_Should_Set_Error_FailureGuard [FAIL]
[xUnit.net 00:00:35.05]       System.Exception : filter_preg_error
[xUnit.net 00:00:35.05]       Stack Trace:
[xUnit.net 00:00:35.05]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\ViewModels\CultureSensitivityViewModel.cs(266,0): at Open_lab.ViewModels.CultureSensitivityViewModel.BuildResultRowsAsync()
[xUnit.net 00:00:35.05]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module5ViewModelTests_Additional.cs(772,0): at Open_lab.Tests.Module5ViewModelTests_Additional.FilterPregnancyAntibiotics_When_Exception_Should_Set_Error_FailureGuard()
[xUnit.net 00:00:35.05]         --- End of stack trace from previous location ---
[xUnit.net 00:00:35.59]     Open_lab.Tests.Module5ViewModelTests_Additional.ClassifySensitivity_WithValidSIRValues_ShouldClassifyEverySavedValue [FAIL]
[xUnit.net 00:00:35.59]       Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "تم حفظ نتيجة المزرعة".
[xUnit.net 00:00:35.59]       Stack Trace:
[xUnit.net 00:00:35.59]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:35.59]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:35.59]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:35.59]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:35.59]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:35.59]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:35.59]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:35.59]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module5ViewModelTests_Additional.cs(390,0): at Open_lab.Tests.Module5ViewModelTests_Additional.ClassifySensitivity_WithValidSIRValues_ShouldClassifyEverySavedValue()
[xUnit.net 00:00:35.59]         --- End of stack trace from previous location ---
[xUnit.net 00:00:35.91]     Open_lab.Tests.Module5ViewModelTests_Additional.FilterChildrenAntibiotics_When_Exception_Should_Set_Error_FailureGuard [FAIL]
[xUnit.net 00:00:35.91]       System.Exception : filter_child_error
[xUnit.net 00:00:35.91]       Stack Trace:
[xUnit.net 00:00:35.91]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\ViewModels\CultureSensitivityViewModel.cs(266,0): at Open_lab.ViewModels.CultureSensitivityViewModel.BuildResultRowsAsync()
[xUnit.net 00:00:35.91]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module5ViewModelTests_Additional.cs(789,0): at Open_lab.Tests.Module5ViewModelTests_Additional.FilterChildrenAntibiotics_When_Exception_Should_Set_Error_FailureGuard()
[xUnit.net 00:00:35.91]         --- End of stack trace from previous location ---
[xUnit.net 00:00:36.57]     Open_lab.Tests.Module5ViewModelTests_Additional.SetSensitivity_WithSensitivityRows_ShouldSaveCultureResults [FAIL]
[xUnit.net 00:00:36.57]       Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "تم حفظ نتيجة المزرعة".
[xUnit.net 00:00:36.57]       Stack Trace:
[xUnit.net 00:00:36.57]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:36.57]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:36.57]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:36.57]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:36.57]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:36.57]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:36.57]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:36.57]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module5ViewModelTests_Additional.cs(308,0): at Open_lab.Tests.Module5ViewModelTests_Additional.SetSensitivity_WithSensitivityRows_ShouldSaveCultureResults()
[xUnit.net 00:00:36.57]         --- End of stack trace from previous location ---
[xUnit.net 00:00:39.21]     Open_lab.Tests.Module6ViewModelTests_Additional.MarkExternalCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:00:39.21]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:39.21]       Stack Trace:
[xUnit.net 00:00:39.21]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:39.21]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:39.21]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:39.21]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:39.21]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:39.21]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:39.21]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:39.21]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module6ViewModelTests_Additional.cs(325,0): at Open_lab.Tests.Module6ViewModelTests_Additional.MarkExternalCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:00:39.21]         --- End of stack trace from previous location ---
[xUnit.net 00:00:40.78]     Open_lab.Tests.ViewModels.ReferenceRangesViewModelTests.DeleteAsync_When_NoSelection_Should_NotCall_Service_EdgeGuard [FAIL]
[xUnit.net 00:00:40.78]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:40.78]       Stack Trace:
[xUnit.net 00:00:40.78]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:40.78]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:40.78]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:40.78]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:40.78]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ReferenceRangesViewModelTests.cs(116,0): at Open_lab.Tests.ViewModels.ReferenceRangesViewModelTests.DeleteAsync_When_NoSelection_Should_NotCall_Service_EdgeGuard()
[xUnit.net 00:00:40.78]         --- End of stack trace from previous location ---
[xUnit.net 00:00:44.15]     Open_lab.Tests.ViewModels.PatientHistoryViewModelTests.PrintHistoryAsync_When_HistoryIsNull_Should_NotCall_PrintService_EdgeGuard [FAIL]
[xUnit.net 00:00:44.15]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:44.15]       Stack Trace:
[xUnit.net 00:00:44.15]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:44.15]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:44.15]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:44.15]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:44.15]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientHistoryViewModelTests.cs(119,0): at Open_lab.Tests.ViewModels.PatientHistoryViewModelTests.PrintHistoryAsync_When_HistoryIsNull_Should_NotCall_PrintService_EdgeGuard()
[xUnit.net 00:00:44.15]         --- End of stack trace from previous location ---
[xUnit.net 00:00:51.37]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.LoadVisitTestsAsync_Should_Call_Service [FAIL]
[xUnit.net 00:00:51.37]       Expected _viewModel.VisitTests to contain 1 item(s), but found 0: {empty}.
[xUnit.net 00:00:51.37]       Stack Trace:
[xUnit.net 00:00:51.37]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:51.37]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:51.37]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:51.37]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:51.37]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(51,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.LoadVisitTestsAsync_Should_Call_Service()
[xUnit.net 00:00:51.37]         --- End of stack trace from previous location ---
[xUnit.net 00:00:51.42]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.VerifyResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:51.42]       Expected (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")) to be true, but found False.
[xUnit.net 00:00:51.42]       Stack Trace:
[xUnit.net 00:00:51.42]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:51.42]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:51.42]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:51.42]            at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
[xUnit.net 00:00:51.42]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(109,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.VerifyResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
[xUnit.net 00:00:51.42]         --- End of stack trace from previous location ---
[xUnit.net 00:00:51.43]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.ReopenResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:51.43]       Expected (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")) to be true, but found False.
[xUnit.net 00:00:51.43]       Stack Trace:
[xUnit.net 00:00:51.43]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:51.43]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:51.43]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:51.43]            at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
[xUnit.net 00:00:51.43]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(175,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.ReopenResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
[xUnit.net 00:00:51.44]         --- End of stack trace from previous location ---
[xUnit.net 00:00:51.51]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.SaveResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:51.51]       Expected (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")) to be true, but found False.
[xUnit.net 00:00:51.51]       Stack Trace:
[xUnit.net 00:00:51.51]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:51.51]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:51.51]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:51.51]            at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
[xUnit.net 00:00:51.51]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(78,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.SaveResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
[xUnit.net 00:00:51.51]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.75]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.DeletePaymentAsync_With_Null_SelectedPayment_Should_Return [FAIL]
[xUnit.net 00:01:07.75]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:07.75]       Stack Trace:
[xUnit.net 00:01:07.75]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:07.75]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:07.75]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:07.75]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:07.75]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(230,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.DeletePaymentAsync_With_Null_SelectedPayment_Should_Return()
[xUnit.net 00:01:07.75]         --- End of stack trace from previous location ---
[xUnit.net 00:01:08.10]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.AddChargeAsync_With_VisitId_Zero_Should_Return [FAIL]
[xUnit.net 00:01:08.10]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:08.10]       Stack Trace:
[xUnit.net 00:01:08.10]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:08.10]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:08.10]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:08.10]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:08.10]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(350,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.AddChargeAsync_With_VisitId_Zero_Should_Return()
[xUnit.net 00:01:08.10]         --- End of stack trace from previous location ---
[xUnit.net 00:01:08.79]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard [FAIL]
[xUnit.net 00:01:08.79]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:08.79]       Stack Trace:
[xUnit.net 00:01:08.79]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:08.79]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:08.79]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:08.79]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:08.79]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(504,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard()
[xUnit.net 00:01:08.79]         --- End of stack trace from previous location ---
[xUnit.net 00:01:09.16]     Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithValidAdminCredentials_ShouldSetSessionAndCreateAttendanceRecord [FAIL]
[xUnit.net 00:01:09.16]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:09.16]       Stack Trace:
[xUnit.net 00:01:09.16]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:09.16]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:09.16]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:09.16]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:09.16]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\LoginViewModelTests.cs(165,0): at Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithValidAdminCredentials_ShouldSetSessionAndCreateAttendanceRecord()
[xUnit.net 00:01:09.16]         --- End of stack trace from previous location ---
[xUnit.net 00:01:09.18]     Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithRememberMe_ShouldSaveUsername [FAIL]
[xUnit.net 00:01:09.18]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:09.18]       Stack Trace:
[xUnit.net 00:01:09.18]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:09.18]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:09.18]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:09.18]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:09.18]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\LoginViewModelTests.cs(195,0): at Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithRememberMe_ShouldSaveUsername()
[xUnit.net 00:01:09.18]         --- End of stack trace from previous location ---
[xUnit.net 00:01:09.20]     Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithoutRememberMe_ShouldClearSavedUsername [FAIL]
[xUnit.net 00:01:09.20]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:09.20]       Stack Trace:
[xUnit.net 00:01:09.20]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:09.20]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:09.20]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:09.20]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:09.20]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\LoginViewModelTests.cs(225,0): at Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithoutRememberMe_ShouldClearSavedUsername()
[xUnit.net 00:01:09.20]         --- End of stack trace from previous location ---
[xUnit.net 00:01:14.49]     Open_lab.Tests.ViewModels.SystemSettingsViewModelTests.ReloadCommand_When_Executed_Should_Refresh_Profile_And_Settings_Success [FAIL]
[xUnit.net 00:01:14.49]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:14.49]       Stack Trace:
[xUnit.net 00:01:14.49]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:14.49]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:14.49]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:14.49]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:14.49]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SystemSettingsViewModelTests.cs(125,0): at Open_lab.Tests.ViewModels.SystemSettingsViewModelTests.ReloadCommand_When_Executed_Should_Refresh_Profile_And_Settings_Success()
[xUnit.net 00:01:14.49]         --- End of stack trace from previous location ---
[xUnit.net 00:01:16.12]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+TestCommentsViewModel_AdditionalTests.DeleteAsync_When_Null_Selection_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:01:16.12]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:16.12]       Stack Trace:
[xUnit.net 00:01:16.12]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:16.12]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:16.12]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:16.12]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:16.12]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(1031,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.TestCommentsViewModel_AdditionalTests.DeleteAsync_When_Null_Selection_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:01:16.12]         --- End of stack trace from previous location ---
[xUnit.net 00:01:18.49]     Open_lab.Tests.ViewModels.UsersPermissionsViewModelTests.SaveRolePermissionsCommand_WhenNoRoleSelected_ShouldNotCallService [FAIL]
[xUnit.net 00:01:18.49]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:18.49]       Stack Trace:
[xUnit.net 00:01:18.49]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:18.49]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:18.49]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:18.49]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:18.49]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\UsersPermissionsViewModelTests.cs(274,0): at Open_lab.Tests.ViewModels.UsersPermissionsViewModelTests.SaveRolePermissionsCommand_WhenNoRoleSelected_ShouldNotCallService()
[xUnit.net 00:01:18.49]         --- End of stack trace from previous location ---
[xUnit.net 00:01:44.58]   Finished:    Open_lab.Tests
========== Test run finished: 1558 Tests (1532 Passed, 26 Failed, 0 Skipped) run in 1.7 min ==========
