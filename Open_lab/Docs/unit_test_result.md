========== Starting test run ==========
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:00.74]   Starting:    Open_lab.Tests
[xUnit.net 00:00:08.81]     Open_lab.Tests.ViewModels.ReferenceRangesViewModelTests.DeleteAsync_When_NoSelection_Should_NotCall_Service_EdgeGuard [FAIL]
[xUnit.net 00:00:08.82]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:08.82]       Stack Trace:
[xUnit.net 00:00:08.82]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:08.82]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:08.82]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:08.82]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:08.82]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:08.82]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:08.82]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:08.82]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ReferenceRangesViewModelTests.cs(113,0): at Open_lab.Tests.ViewModels.ReferenceRangesViewModelTests.DeleteAsync_When_NoSelection_Should_NotCall_Service_EdgeGuard()
[xUnit.net 00:00:08.82]         --- End of stack trace from previous location ---
[xUnit.net 00:00:09.55]     Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithValidAdminCredentials_ShouldSetSessionAndCreateAttendanceRecord [FAIL]
[xUnit.net 00:00:09.55]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:09.55]       Stack Trace:
[xUnit.net 00:00:09.55]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:09.56]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:09.56]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:09.56]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.56]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.56]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:09.56]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:09.56]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\LoginViewModelTests.cs(164,0): at Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithValidAdminCredentials_ShouldSetSessionAndCreateAttendanceRecord()
[xUnit.net 00:00:09.56]         --- End of stack trace from previous location ---
[xUnit.net 00:00:09.59]     Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithRememberMe_ShouldSaveUsername [FAIL]
[xUnit.net 00:00:09.59]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:09.59]       Stack Trace:
[xUnit.net 00:00:09.59]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:09.59]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:09.59]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:09.59]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.59]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.59]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:09.59]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:09.59]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\LoginViewModelTests.cs(194,0): at Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithRememberMe_ShouldSaveUsername()
[xUnit.net 00:00:09.59]         --- End of stack trace from previous location ---
[xUnit.net 00:00:09.61]     Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithoutRememberMe_ShouldClearSavedUsername [FAIL]
[xUnit.net 00:00:09.61]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:09.61]       Stack Trace:
[xUnit.net 00:00:09.61]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:09.61]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:09.61]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:09.61]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.61]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.61]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:09.61]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:09.61]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\LoginViewModelTests.cs(224,0): at Open_lab.Tests.ViewModels.LoginViewModelTests.LoginAsync_WithoutRememberMe_ShouldClearSavedUsername()
[xUnit.net 00:00:09.61]         --- End of stack trace from previous location ---
[xUnit.net 00:00:11.20]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.VerifyResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:11.20]       Expected (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")) to be true, but found False.
[xUnit.net 00:00:11.20]       Stack Trace:
[xUnit.net 00:00:11.20]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:11.20]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:11.20]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:11.20]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:11.20]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:11.20]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:11.20]            at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
[xUnit.net 00:00:11.20]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(84,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.VerifyResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
[xUnit.net 00:00:11.20]         --- End of stack trace from previous location ---
[xUnit.net 00:00:11.21]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.ReopenResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:11.21]       Expected (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")) to be true, but found False.
[xUnit.net 00:00:11.21]       Stack Trace:
[xUnit.net 00:00:11.21]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:11.21]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:11.21]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:11.21]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:11.21]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:11.21]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:11.21]            at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
[xUnit.net 00:00:11.21]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(134,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.ReopenResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
[xUnit.net 00:00:11.21]         --- End of stack trace from previous location ---
[xUnit.net 00:00:11.23]     Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.SaveResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing [FAIL]
[xUnit.net 00:00:11.23]       Expected (_viewModel.StatusMessage.Contains("لم يتم تحديد") || _viewModel.StatusMessage.Contains("اختبار")) to be true, but found False.
[xUnit.net 00:00:11.23]       Stack Trace:
[xUnit.net 00:00:11.23]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:11.23]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:11.23]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:11.23]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:11.23]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:11.23]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:11.23]            at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
[xUnit.net 00:00:11.23]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\ResultsEntryViewModelTests.cs(61,0): at Open_lab.Tests.ViewModels.ResultsEntryViewModelTests.SaveResultsAsync_With_Null_SelectedVisitTest_Should_Do_Nothing()
[xUnit.net 00:00:11.23]         --- End of stack trace from previous location ---
[xUnit.net 00:00:53.51]     Open_lab.Tests.Module6ViewModelTests_Additional.MarkExternalCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:00:53.51]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:53.51]       Stack Trace:
[xUnit.net 00:00:53.51]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:53.51]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:53.51]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:53.51]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:53.51]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:53.51]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module6Tests_Additional.cs(597,0): at Open_lab.Tests.Module6ViewModelTests_Additional.MarkExternalCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:00:53.51]         --- End of stack trace from previous location ---
[xUnit.net 00:00:59.23]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+PriceListsViewModel_AdditionalTests.UpdateListAsync_When_Null_Selected_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:00:59.23]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:59.23]       Stack Trace:
[xUnit.net 00:00:59.23]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:59.23]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:59.23]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:59.23]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:59.23]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:59.23]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(885,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.PriceListsViewModel_AdditionalTests.UpdateListAsync_When_Null_Selected_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:00:59.23]         --- End of stack trace from previous location ---
[xUnit.net 00:00:59.71]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+TestCommentsViewModel_AdditionalTests.DeleteAsync_When_Null_Selection_Should_Do_Nothing_EdgeGuard [FAIL]
[xUnit.net 00:00:59.71]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:00:59.71]       Stack Trace:
[xUnit.net 00:00:59.71]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:59.71]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:59.71]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:59.71]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:59.71]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:59.71]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(1007,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.TestCommentsViewModel_AdditionalTests.DeleteAsync_When_Null_Selection_Should_Do_Nothing_EdgeGuard()
[xUnit.net 00:00:59.71]         --- End of stack trace from previous location ---
[xUnit.net 00:01:03.32]     Open_lab.Tests.ViewModels.SystemSettingsViewModelTests.ReloadCommand_When_Executed_Should_Refresh_Profile_And_Settings_Success [FAIL]
[xUnit.net 00:01:03.32]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:03.32]       Stack Trace:
[xUnit.net 00:01:03.32]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:03.32]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:03.32]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:03.32]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:03.32]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:03.32]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SystemSettingsViewModelTests.cs(111,0): at Open_lab.Tests.ViewModels.SystemSettingsViewModelTests.ReloadCommand_When_Executed_Should_Refresh_Profile_And_Settings_Success()
[xUnit.net 00:01:03.32]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.08]     Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.ViewPatientHistory_MultipleVisits_ShouldPopulateVisitsList_SuccessGuard [FAIL]
[xUnit.net 00:01:07.08]       Expected vm.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:07.08]       Stack Trace:
[xUnit.net 00:01:07.08]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:07.08]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:07.09]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:07.09]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:07.09]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:07.09]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module1And2ViewModelCompletionTests.cs(254,0): at Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.ViewPatientHistory_MultipleVisits_ShouldPopulateVisitsList_SuccessGuard()
[xUnit.net 00:01:07.09]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.70]     Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.SettleAccount_WhenVisitIdIsZero_ShouldNotCallService_EdgeGuard [FAIL]
[xUnit.net 00:01:07.70]       Expected vm.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:07.70]       Stack Trace:
[xUnit.net 00:01:07.70]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:07.70]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:07.70]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:07.70]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:07.70]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:07.70]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module1And2ViewModelCompletionTests.cs(451,0): at Open_lab.Tests.ViewModels.Module1And2ViewModelCompletionTests.SettleAccount_WhenVisitIdIsZero_ShouldNotCallService_EdgeGuard()
[xUnit.net 00:01:07.70]         --- End of stack trace from previous location ---
[xUnit.net 00:01:08.96]     Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service [FAIL]
[xUnit.net 00:01:08.96]       Expected _viewModel.StatusMessage "تم تحديث الحالة." to contain "تم تعليم العينة كغير مجمعة".
[xUnit.net 00:01:08.96]       Stack Trace:
[xUnit.net 00:01:08.96]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:08.96]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:08.96]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:08.96]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:08.96]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:08.96]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SampleCollectionViewModelTests.cs(188,0): at Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkNotCollectedAsync_With_Valid_Row_Should_Call_Service()
[xUnit.net 00:01:08.96]         --- End of stack trace from previous location ---
[xUnit.net 00:01:08.97]     Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing [FAIL]
[xUnit.net 00:01:08.97]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:08.97]       Stack Trace:
[xUnit.net 00:01:08.97]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:08.97]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:08.97]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:08.97]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:08.97]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:08.97]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\SampleCollectionViewModelTests.cs(101,0): at Open_lab.Tests.ViewModels.SampleCollectionViewModelTests.MarkCollectedAsync_With_Null_SelectedRow_Should_Do_Nothing()
[xUnit.net 00:01:08.97]         --- End of stack trace from previous location ---
[xUnit.net 00:01:11.91]     Open_lab.Tests.ViewModels.Module4ViewModelCompletion_Part3.PrintReport_PrintCommand_When_No_Report_Should_Do_Nothing_Edge [FAIL]
[xUnit.net 00:01:11.91]       Expected viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:11.91]       Stack Trace:
[xUnit.net 00:01:11.91]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:11.91]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:11.91]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:11.91]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:11.91]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:11.91]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module4ViewModelCompletion_Part3.cs(235,0): at Open_lab.Tests.ViewModels.Module4ViewModelCompletion_Part3.PrintReport_PrintCommand_When_No_Report_Should_Do_Nothing_Edge()
[xUnit.net 00:01:11.91]         --- End of stack trace from previous location ---
[xUnit.net 00:01:20.36]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.DeletePaymentAsync_With_Null_SelectedPayment_Should_Return [FAIL]
[xUnit.net 00:01:20.37]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:20.37]       Stack Trace:
[xUnit.net 00:01:20.37]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:20.37]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:20.37]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:20.37]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:20.37]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:20.37]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(226,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.DeletePaymentAsync_With_Null_SelectedPayment_Should_Return()
[xUnit.net 00:01:20.37]         --- End of stack trace from previous location ---
[xUnit.net 00:01:20.72]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.AddChargeAsync_With_VisitId_Zero_Should_Return [FAIL]
[xUnit.net 00:01:20.72]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:20.72]       Stack Trace:
[xUnit.net 00:01:20.72]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:20.72]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:20.72]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:20.72]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:20.72]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:20.72]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(346,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.AddChargeAsync_With_VisitId_Zero_Should_Return()
[xUnit.net 00:01:20.72]         --- End of stack trace from previous location ---
[xUnit.net 00:01:21.38]     Open_lab.Tests.ViewModels.PatientBillingViewModelTests.PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard [FAIL]
[xUnit.net 00:01:21.38]       Expected _viewModel.StatusMessage not to be <null> or empty, but found "".
[xUnit.net 00:01:21.38]       Stack Trace:
[xUnit.net 00:01:21.38]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:21.38]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:21.38]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:21.38]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:21.38]            at FluentAssertions.Primitives.StringAssertions`1.NotBeNullOrEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:21.38]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\PatientBillingViewModelTests.cs(500,0): at Open_lab.Tests.ViewModels.PatientBillingViewModelTests.PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard()
[xUnit.net 00:01:21.38]         --- End of stack trace from previous location ---
[xUnit.net 00:01:22.49]   Finished:    Open_lab.Tests
========== Test run finished: 1483 Tests (1464 Passed, 19 Failed, 0 Skipped) run in 1.4 min ==========
