========== Starting test discovery ==========
[xUnit.net 00:00:00.01] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:04.78]   Discovering: Open_lab.Tests
[xUnit.net 00:00:08.01]   Discovered:  Open_lab.Tests
========== Test discovery finished: 1322 Tests found in 18.2 sec ==========
========== Starting test run ==========
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:01.02]   Starting:    Open_lab.Tests
[xUnit.net 00:00:23.43]     Open_lab.Tests.ViewModels.Module12ViewModelTests_Additional.AddReferringPhysician_WithValidFullName_ShouldCallCreateAndShowStatus_SuccessGuard [FAIL]
[xUnit.net 00:00:23.43]       Expected viewModel.StatusMessage "تم تحميل 0 طبيب." to contain "تم إنشاء الطبيب".
[xUnit.net 00:00:23.44]       Stack Trace:
[xUnit.net 00:00:23.44]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:23.44]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:23.44]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:23.44]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:23.44]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:23.44]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:23.44]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:23.44]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module12ViewModelTests_Additional.cs(317,0): at Open_lab.Tests.ViewModels.Module12ViewModelTests_Additional.AddReferringPhysician_WithValidFullName_ShouldCallCreateAndShowStatus_SuccessGuard()
[xUnit.net 00:00:23.44]         --- End of stack trace from previous location ---
[xUnit.net 00:00:28.82]     Open_lab.Tests.ViewModels.Module12ViewModelTests_Additional.SettleContractAccount_WithSelectedUnpaidInvoice_ShouldCallServiceAndShowStatus_SuccessGuard [FAIL]
[xUnit.net 00:00:28.82]       Expected viewModel.StatusMessage "تم تحميل 1 فاتورة تعاقد سابقة." to contain "تم تسوية".
[xUnit.net 00:00:28.82]       Stack Trace:
[xUnit.net 00:00:28.82]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:28.82]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:28.82]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:28.82]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:28.82]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module12ViewModelTests_Additional.cs(769,0): at Open_lab.Tests.ViewModels.Module12ViewModelTests_Additional.SettleContractAccount_WithSelectedUnpaidInvoice_ShouldCallServiceAndShowStatus_SuccessGuard()
[xUnit.net 00:00:28.82]         --- End of stack trace from previous location ---
[xUnit.net 00:00:30.52]     Open_lab.Tests.ViewModels.Module12ViewModelTests_Additional.SetPhysicianPriceList_WhenSelectedExisting_ShouldCallUpdateWithPriceListId_SuccessGuard [FAIL]
[xUnit.net 00:00:30.52]       Expected viewModel.StatusMessage "تم تحميل 1 طبيب." to contain "تم تحديث".
[xUnit.net 00:00:30.52]       Stack Trace:
[xUnit.net 00:00:30.52]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:30.52]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:30.52]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:30.52]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:30.52]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module12ViewModelTests_Additional.cs(455,0): at Open_lab.Tests.ViewModels.Module12ViewModelTests_Additional.SetPhysicianPriceList_WhenSelectedExisting_ShouldCallUpdateWithPriceListId_SuccessGuard()
[xUnit.net 00:00:30.52]         --- End of stack trace from previous location ---
[xUnit.net 00:00:42.51]     Open_lab.Tests.Module8ServiceTests_Additional.GetPendingBalanceAsync_With_Existing_Settlements_Should_Calculate_Correctly_EdgeGuard [FAIL]
[xUnit.net 00:00:42.51]       Expected balance to be 20M, but found -30M (difference of -50).
[xUnit.net 00:00:42.51]       Stack Trace:
[xUnit.net 00:00:42.51]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:42.51]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:42.51]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:42.51]            at FluentAssertions.Numeric.NumericAssertions`2.Be(T expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:42.51]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module8Tests_Additional.cs(624,0): at Open_lab.Tests.Module8ServiceTests_Additional.GetPendingBalanceAsync_With_Existing_Settlements_Should_Calculate_Correctly_EdgeGuard()
[xUnit.net 00:00:42.51]         --- End of stack trace from previous location ---
[xUnit.net 00:00:42.69]     Open_lab.Tests.Module8ServiceTests_Additional.GetTotalProfitAsync_With_Multiple_Queue_Items_Should_Sum_All_Profits_SuccessGuard [FAIL]
[xUnit.net 00:00:42.69]       Expected profit to be 300M, but found 200M (difference of -100).
[xUnit.net 00:00:42.69]       Stack Trace:
[xUnit.net 00:00:42.69]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:42.69]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:42.69]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:42.69]            at FluentAssertions.Numeric.NumericAssertions`2.Be(T expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:42.69]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module8Tests_Additional.cs(589,0): at Open_lab.Tests.Module8ServiceTests_Additional.GetTotalProfitAsync_With_Multiple_Queue_Items_Should_Sum_All_Profits_SuccessGuard()
[xUnit.net 00:00:42.69]         --- End of stack trace from previous location ---
[xUnit.net 00:00:51.41]     Open_lab.Tests.Module4ServiceTests_Additional.ValidateResultAsync_With_Value_Above_Range_Should_Set_High_Flag_SuccessGuard [FAIL]
[xUnit.net 00:00:51.41]       Expected result.Flag to be "H", but found <null>.
[xUnit.net 00:00:51.41]       Stack Trace:
[xUnit.net 00:00:51.41]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:51.41]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:51.41]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:51.41]            at FluentAssertions.Primitives.StringValidator.Validate()
[xUnit.net 00:00:51.41]            at FluentAssertions.Primitives.StringAssertions`1.Be(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:51.41]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(225,0): at Open_lab.Tests.Module4ServiceTests_Additional.ValidateResultAsync_With_Value_Above_Range_Should_Set_High_Flag_SuccessGuard()
[xUnit.net 00:00:51.41]         --- End of stack trace from previous location ---
[xUnit.net 00:00:51.97]     Open_lab.Tests.Module4ServiceTests_Additional.GetResultsForVisitTestAsync_Should_Return_All_Results_Ordered_By_Parameter_SuccessGuard [FAIL]
[xUnit.net 00:00:51.97]       Expected results[0].ParameterId to be 60, but found 62 (difference of 2).
[xUnit.net 00:00:51.97]       Stack Trace:
[xUnit.net 00:00:51.97]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:51.97]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:51.97]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:51.97]            at FluentAssertions.Numeric.NumericAssertions`2.Be(T expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:51.97]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(469,0): at Open_lab.Tests.Module4ServiceTests_Additional.GetResultsForVisitTestAsync_Should_Return_All_Results_Ordered_By_Parameter_SuccessGuard()
[xUnit.net 00:00:51.97]         --- End of stack trace from previous location ---
[xUnit.net 00:00:58.33]     Open_lab.Tests.ViewModels.Module11ViewModelTests_Additional.ClockOutCommand_WithOpenLog_ShouldClearAppSessionAndShowSuccess_SuccessGuard [FAIL]
[xUnit.net 00:00:58.33]       Expected vm.StatusMessage "لا يوجد سجل حضور مفتوح." to contain "تم تسجيل الانصراف".
[xUnit.net 00:00:58.33]       Stack Trace:
[xUnit.net 00:00:58.33]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:58.33]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:58.33]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:58.33]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:58.33]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module11ViewModelTests_Additional.cs(179,0): at Open_lab.Tests.ViewModels.Module11ViewModelTests_Additional.ClockOutCommand_WithOpenLog_ShouldClearAppSessionAndShowSuccess_SuccessGuard()
[xUnit.net 00:00:58.33]         --- End of stack trace from previous location ---
[xUnit.net 00:01:02.57]     Open_lab.Tests.ViewModels.Module11ViewModelTests_Additional.ClockInCommand_WhenAlreadyHasOpenLog_ShouldStillReportSuccess_EdgeGuard [FAIL]
[xUnit.net 00:01:02.57]       Expected vm.StatusMessage "لا يوجد سجل حضور مفتوح." to contain "100".
[xUnit.net 00:01:02.57]       Stack Trace:
[xUnit.net 00:01:02.57]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:02.57]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:02.57]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:02.57]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:02.57]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module11ViewModelTests_Additional.cs(154,0): at Open_lab.Tests.ViewModels.Module11ViewModelTests_Additional.ClockInCommand_WhenAlreadyHasOpenLog_ShouldStillReportSuccess_EdgeGuard()
[xUnit.net 00:01:02.57]         --- End of stack trace from previous location ---
[xUnit.net 00:01:02.78]     Open_lab.Tests.ViewModels.Module11ViewModelTests_Additional.ClockInCommand_WithValidUser_ShouldUpdateAppSessionAndStatusMessage_SuccessGuard [FAIL]
[xUnit.net 00:01:02.78]       Expected vm.StatusMessage "لا يوجد سجل حضور مفتوح." to contain "تم تسجيل الحضور".
[xUnit.net 00:01:02.78]       Stack Trace:
[xUnit.net 00:01:02.78]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:02.78]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:02.78]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:02.78]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:02.78]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module11ViewModelTests_Additional.cs(91,0): at Open_lab.Tests.ViewModels.Module11ViewModelTests_Additional.ClockInCommand_WithValidUser_ShouldUpdateAppSessionAndStatusMessage_SuccessGuard()
[xUnit.net 00:01:02.78]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.79]     Open_lab.Tests.Module5ViewModelTests_Additional.SearchCommand_With_NonExistent_LabId_Should_Show_NotFound_EdgeGuard [FAIL]
[xUnit.net 00:01:07.79]       System.Reflection.TargetException : Method 'SearchCommand' not found on type 'CultureSensitivityViewModel'.
[xUnit.net 00:01:07.79]       Stack Trace:
[xUnit.net 00:01:07.79]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Infrastructure\ReflectionHelper.cs(12,0): at Open_lab.Tests.Infrastructure.ReflectionHelper.InvokePrivateAsync(Object instance, String methodName, Object[] parameters)
[xUnit.net 00:01:07.79]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(958,0): at Open_lab.Tests.Module5ViewModelTests_Additional.SearchCommand_With_NonExistent_LabId_Should_Show_NotFound_EdgeGuard()
[xUnit.net 00:01:07.79]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.81]     Open_lab.Tests.Module5ViewModelTests_Additional.SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard [FAIL]
[xUnit.net 00:01:07.81]       Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "لا توجد نتائج".
[xUnit.net 00:01:07.81]       Stack Trace:
[xUnit.net 00:01:07.81]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:07.81]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:07.81]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:07.81]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:07.81]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(729,0): at Open_lab.Tests.Module5ViewModelTests_Additional.SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard()
[xUnit.net 00:01:07.81]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.83]     Open_lab.Tests.Module5ViewModelTests_Additional.AddAntibioticAsync_With_Empty_Name_Should_Set_Error_FailureGuard [FAIL]
[xUnit.net 00:01:07.83]       Expected _viewModel.StatusMessage "أدخل اسم المضاد الحيوي." to contain "خطأ:".
[xUnit.net 00:01:07.83]       Stack Trace:
[xUnit.net 00:01:07.83]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:07.83]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:07.83]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:07.83]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:07.83]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(680,0): at Open_lab.Tests.Module5ViewModelTests_Additional.AddAntibioticAsync_With_Empty_Name_Should_Set_Error_FailureGuard()
[xUnit.net 00:01:07.83]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.85]     Open_lab.Tests.Module5ViewModelTests_Additional.AddCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard [FAIL]
[xUnit.net 00:01:07.85]       Moq.MockException : 
[xUnit.net 00:01:07.85]       Expected invocation on the mock once, but was 0 times: s => s.CreateCultureAsync(It.Is<Culture>(c => c.ColonyCount == 0))
[xUnit.net 00:01:07.85]       
[xUnit.net 00:01:07.85]       Performed invocations:
[xUnit.net 00:01:07.85]       
[xUnit.net 00:01:07.85]          Mock<ICultureSensitivityService:9> (s):
[xUnit.net 00:01:07.85]       
[xUnit.net 00:01:07.85]             ICultureSensitivityService.GetCulturesAsync()
[xUnit.net 00:01:07.85]             ICultureSensitivityService.GetAntibioticsAsync()
[xUnit.net 00:01:07.85]       
[xUnit.net 00:01:07.85]       Stack Trace:
[xUnit.net 00:01:07.85]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:01:07.85]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:01:07.85]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(663,0): at Open_lab.Tests.Module5ViewModelTests_Additional.AddCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard()
[xUnit.net 00:01:07.85]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.86]     Open_lab.Tests.Module5ViewModelTests_Additional.AddCultureAsync_With_Empty_Name_Should_Set_Error_FailureGuard [FAIL]
[xUnit.net 00:01:07.86]       Expected _viewModel.StatusMessage "أدخل اسم المزرعة." to contain "خطأ:".
[xUnit.net 00:01:07.86]       Stack Trace:
[xUnit.net 00:01:07.86]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:07.86]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:07.86]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:07.86]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:07.86]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(645,0): at Open_lab.Tests.Module5ViewModelTests_Additional.AddCultureAsync_With_Empty_Name_Should_Set_Error_FailureGuard()
[xUnit.net 00:01:07.86]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.87]     Open_lab.Tests.Module5ViewModelTests_Additional.SearchCommand_With_Valid_LabId_Should_Load_Results_SuccessGuard [FAIL]
[xUnit.net 00:01:07.87]       System.Reflection.TargetException : Method 'SearchCommand' not found on type 'CultureSensitivityViewModel'.
[xUnit.net 00:01:07.87]       Stack Trace:
[xUnit.net 00:01:07.87]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Infrastructure\ReflectionHelper.cs(12,0): at Open_lab.Tests.Infrastructure.ReflectionHelper.InvokePrivateAsync(Object instance, String methodName, Object[] parameters)
[xUnit.net 00:01:07.87]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(944,0): at Open_lab.Tests.Module5ViewModelTests_Additional.SearchCommand_With_Valid_LabId_Should_Load_Results_SuccessGuard()
[xUnit.net 00:01:07.87]         --- End of stack trace from previous location ---
[xUnit.net 00:01:09.69]     Open_lab.Tests.Module8ViewModelTests_Additional.LoadSettlementCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:01:09.69]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:01:09.70]       Stack Trace:
[xUnit.net 00:01:09.70]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:09.70]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:09.70]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:09.70]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:09.70]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module8Tests_Additional.cs(758,0): at Open_lab.Tests.Module8ViewModelTests_Additional.LoadSettlementCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:01:09.70]         --- End of stack trace from previous location ---
[xUnit.net 00:01:10.17]     Open_lab.Tests.Module8ViewModelTests_Additional.LoadQueueCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:01:10.17]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:01:10.17]       Stack Trace:
[xUnit.net 00:01:10.17]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:10.17]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:10.17]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:10.17]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:10.17]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module8Tests_Additional.cs(729,0): at Open_lab.Tests.Module8ViewModelTests_Additional.LoadQueueCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:01:10.17]         --- End of stack trace from previous location ---
[xUnit.net 00:01:17.52]     Open_lab.Tests.Services.Module11ServiceTests_Additional.GenerateAttendanceReport_WithLogsButNoStatuses_ShouldCountDaysWithLogsAsPresent_SuccessGuard [FAIL]
[xUnit.net 00:01:17.52]       Expected rows[0].PresentDays to be 1 because Day with login record but no explicit status should count as present, but found 0 (difference of -1).
[xUnit.net 00:01:17.52]       Stack Trace:
[xUnit.net 00:01:17.52]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:17.52]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:17.52]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:17.52]            at FluentAssertions.Numeric.NumericAssertions`2.Be(T expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:17.52]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module11ServiceTests_Additional.cs(862,0): at Open_lab.Tests.Services.Module11ServiceTests_Additional.GenerateAttendanceReport_WithLogsButNoStatuses_ShouldCountDaysWithLogsAsPresent_SuccessGuard()
[xUnit.net 00:01:17.52]         --- End of stack trace from previous location ---
[xUnit.net 00:01:24.40]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+TestCommentsViewModel_AdditionalTests.SaveAsync_With_Empty_Comment_Text_Should_NotCall_Service_EdgeGuard [FAIL]
[xUnit.net 00:01:24.40]       Moq.MockException : 
[xUnit.net 00:01:24.40]       Expected invocation on the mock should never have been performed, but was 1 times: x => x.CreateTestCommentAsync(It.IsAny<TestComment>())
[xUnit.net 00:01:24.40]       
[xUnit.net 00:01:24.40]       Performed invocations:
[xUnit.net 00:01:24.40]       
[xUnit.net 00:01:24.40]          Mock<ITestCatalogService:134> (x):
[xUnit.net 00:01:24.40]       
[xUnit.net 00:01:24.40]             ITestCatalogService.GetAllTestsAsync()
[xUnit.net 00:01:24.40]             ITestCatalogService.GetTestCommentsAsync(1)
[xUnit.net 00:01:24.40]             ITestCatalogService.CreateTestCommentAsync(TestComment)
[xUnit.net 00:01:24.40]       
[xUnit.net 00:01:24.41]       Stack Trace:
[xUnit.net 00:01:24.41]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:01:24.41]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:01:24.41]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(859,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.TestCommentsViewModel_AdditionalTests.SaveAsync_With_Empty_Comment_Text_Should_NotCall_Service_EdgeGuard()
[xUnit.net 00:01:24.41]         --- End of stack trace from previous location ---
[xUnit.net 00:01:37.36]     Open_lab.Tests.Services.UserAdminServiceTests.EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation [FAIL]
[xUnit.net 00:01:37.36]       Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.
[xUnit.net 00:01:37.36]       Stack Trace:
[xUnit.net 00:01:37.36]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:37.36]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:37.36]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:37.36]            at FluentAssertions.Specialized.DelegateAssertionsBase`2.ThrowInternal[TException](Exception exception, String because, Object[] becauseArgs)
[xUnit.net 00:01:37.36]            at FluentAssertions.Specialized.AsyncFunctionAssertions`2.ThrowAsync[TException](String because, Object[] becauseArgs)
[xUnit.net 00:01:37.36]            at FluentAssertions.ExceptionAssertionsExtensions.WithMessage[TException](Task`1 task, String expectedWildcardPattern, String because, Object[] becauseArgs)
[xUnit.net 00:01:37.36]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\UserAdminServiceTests.cs(349,0): at Open_lab.Tests.Services.UserAdminServiceTests.EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation()
[xUnit.net 00:01:37.36]         --- End of stack trace from previous location ---
[xUnit.net 00:01:40.11]     Open_lab.Tests.Module7ViewModelTests_Additional.GroupWorksheet_LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:01:40.11]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:01:40.12]       Stack Trace:
[xUnit.net 00:01:40.12]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:40.12]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:40.12]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:40.12]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:40.12]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module7Tests_Additional.cs(789,0): at Open_lab.Tests.Module7ViewModelTests_Additional.GroupWorksheet_LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:01:40.12]         --- End of stack trace from previous location ---
[xUnit.net 00:01:40.44]     Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_With_Specific_Date_Range_Should_Pass_Range_To_Service_SuccessGuard [FAIL]
[xUnit.net 00:01:40.44]       Moq.MockException : 
[xUnit.net 00:01:40.44]       Expected invocation on the mock once, but was 0 times: x => x.GetWorksheetByPatientAsync(23/04/2026 12:00:00 ص, 30/04/2026 12:00:00 ص)
[xUnit.net 00:01:40.44]       
[xUnit.net 00:01:40.44]       Performed invocations:
[xUnit.net 00:01:40.44]       
[xUnit.net 00:01:40.44]          Mock<IWorksheetService:22> (x):
[xUnit.net 00:01:40.44]       
[xUnit.net 00:01:40.44]             IWorksheetService.GetWorksheetByPatientAsync(23/04/2026 12:00:00 ص, 30/04/2026 11:59:59 م)
[xUnit.net 00:01:40.44]       
[xUnit.net 00:01:40.44]       Stack Trace:
[xUnit.net 00:01:40.44]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:01:40.44]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:01:40.44]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module7Tests_Additional.cs(516,0): at Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_With_Specific_Date_Range_Should_Pass_Range_To_Service_SuccessGuard()
[xUnit.net 00:01:40.44]         --- End of stack trace from previous location ---
[xUnit.net 00:01:41.11]     Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:01:41.11]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:01:41.11]       Stack Trace:
[xUnit.net 00:01:41.11]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:41.11]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:41.11]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:41.11]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:41.11]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module7Tests_Additional.cs(766,0): at Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:01:41.11]         --- End of stack trace from previous location ---
[xUnit.net 00:01:44.12]     Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_Should_Return_Only_Non_Separated_Samples_SuccessGuard [FAIL]
[xUnit.net 00:01:44.12]       Expected pending to contain 2 item(s), but found 3: Open_lab.Models.SampleCollection
[xUnit.net 00:01:44.12]           {
[xUnit.net 00:01:44.12]               CollectedAt = <2026-04-30 14:44:34.5637508>, 
[xUnit.net 00:01:44.12]               CollectedBy = 1, 
[xUnit.net 00:01:44.12]               CollectedByUser = <null>, 
[xUnit.net 00:01:44.12]               IsExternalSample = False, 
[xUnit.net 00:01:44.12]               IsSeparated = False, 
[xUnit.net 00:01:44.12]               ReceivedBy = <null>, 
[xUnit.net 00:01:44.12]               ReceivedByUser = <null>, 
[xUnit.net 00:01:44.12]               SampleId = 3, 
[xUnit.net 00:01:44.12]               Status = "مسحوبة", 
[xUnit.net 00:01:44.12]               VisitTest = Open_lab.Models.VisitTest
[xUnit.net 00:01:44.12]               {
[xUnit.net 00:01:44.13]                   ExternalQueueItem = <null>, 
[xUnit.net 00:01:44.13]                   Price = 10M, 
[xUnit.net 00:01:44.13]                   ResultValues = {empty}, 
[xUnit.net 00:01:44.13]                   SampleCollection = {Cyclic reference to type Open_lab.Models.SampleCollection detected}, 
[xUnit.net 00:01:44.13]                   Status = <null>, 
[xUnit.net 00:01:44.13]                   Test = Open_lab.Models.Test
[xUnit.net 00:01:44.13]                   {
[xUnit.net 00:01:44.13]                       Code = "T-22f21496", 
[xUnit.net 00:01:44.13]                       Comments = {empty}, 
[xUnit.net 00:01:44.13]                       CostPrice = <null>, 
[xUnit.net 00:01:44.13]                       CustomGroupItems = {empty}, 
[xUnit.net 00:01:44.13]                       Group = <null>, 
[xUnit.net 00:01:44.13]                       GroupId = <null>, 
[xUnit.net 00:01:44.13]                       IsRoutine = False, 
[xUnit.net 00:01:44.13]                       IsSendOut = False, 
[xUnit.net 00:01:44.13]                       NameReceipt = "", 
[xUnit.net 00:01:44.13]                       NameReport = "Test", 
[xUnit.net 00:01:44.13]                       Parameters = {empty}, 
[xUnit.net 00:01:44.13]                       PatientPrice = <null>, 
[xUnit.net 00:01:44.13]                       Price = 10M, 
[xUnit.net 00:01:44.13]                       PriceListItems = {empty}, 
[xUnit.net 00:01:44.13]                       ReferenceRanges = {empty}, 
[xUnit.net 00:01:44.13]                       ReportOrder = 0, 
[xUnit.net 00:01:44.13]                       SampleType = <null>, 
[xUnit.net 00:01:44.13]                       SampleTypeId = <null>, 
[xUnit.net 00:01:44.13]                       TestId = 3, 
[xUnit.net 00:01:44.13]                       TurnaroundHours = 0, 
[xUnit.net 00:01:44.13]                       Unit = <null>, 
[xUnit.net 00:01:44.13]                       UnitId = <null>, 
[xUnit.net 00:01:44.13]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:01:44.13]                   }, 
[xUnit.net 00:01:44.13]                   TestId = 3, 
[xUnit.net 00:01:44.13]                   Visit = Open_lab.Models.Visit
[xUnit.net 00:01:44.13]                   {
[xUnit.net 00:01:44.13]                       AccountType = <null>, 
[xUnit.net 00:01:44.13]                       Branch = <null>, 
[xUnit.net 00:01:44.13]                       BranchId = <null>, 
[xUnit.net 00:01:44.13]                       Invoice = <null>, 
[xUnit.net 00:01:44.13]                       Patient = Open_lab.Models.Patient
[xUnit.net 00:01:44.13]                       {
[xUnit.net 00:01:44.13]                           Address = <null>, 
[xUnit.net 00:01:44.13]                           Age = <null>, 
[xUnit.net 00:01:44.13]                           BirthDate = <null>, 
[xUnit.net 00:01:44.13]                           FullName = "Pending 3", 
[xUnit.net 00:01:44.13]                           Gender = "Male", 
[xUnit.net 00:01:44.13]                           IsPregnant = False, 
[xUnit.net 00:01:44.13]                           LabId = "L-PND3", 
[xUnit.net 00:01:44.13]                           MedicalHistory = <null>, 
[xUnit.net 00:01:44.13]                           PatientId = 3, 
[xUnit.net 00:01:44.13]                           Phone = <null>, 
[xUnit.net 00:01:44.13]                           Visits = {{Cyclic reference to type Open_lab.Models.Visit detected}}
[xUnit.net 00:01:44.13]                       }, 
[xUnit.net 00:01:44.13]                       PatientId = 3, 
[xUnit.net 00:01:44.13]                       Physician = <null>, 
[xUnit.net 00:01:44.13]                       PhysicianId = <null>, 
[xUnit.net 00:01:44.13]                       Referral = <null>, 
[xUnit.net 00:01:44.13]                       ReferralId = <null>, 
[xUnit.net 00:01:44.13]                       Status = <null>, 
[xUnit.net 00:01:44.13]                       VisitDate = <2026-04-30 14:44:34.558770>, 
[xUnit.net 00:01:44.13]                       VisitId = 3, 
[xUnit.net 00:01:44.13]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:01:44.13]                   }, 
[xUnit.net 00:01:44.13]                   VisitId = 3, 
[xUnit.net 00:01:44.13]                   VisitTestId = 3
[xUnit.net 00:01:44.13]               }, 
[xUnit.net 00:01:44.13]               VisitTestId = 3
[xUnit.net 00:01:44.13]           }Open_lab.Models.SampleCollection
[xUnit.net 00:01:44.13]           {
[xUnit.net 00:01:44.13]               CollectedAt = <2026-04-30 14:44:34.5633462>, 
[xUnit.net 00:01:44.13]               CollectedBy = 1, 
[xUnit.net 00:01:44.13]               CollectedByUser = <null>, 
[xUnit.net 00:01:44.13]               IsExternalSample = False, 
[xUnit.net 00:01:44.13]               IsSeparated = False, 
[xUnit.net 00:01:44.13]               ReceivedBy = <null>, 
[xUnit.net 00:01:44.13]               ReceivedByUser = <null>, 
[xUnit.net 00:01:44.13]               SampleId = 2, 
[xUnit.net 00:01:44.13]               Status = "مسحوبة", 
[xUnit.net 00:01:44.13]               VisitTest = Open_lab.Models.VisitTest
[xUnit.net 00:01:44.13]               {
[xUnit.net 00:01:44.13]                   ExternalQueueItem = <null>, 
[xUnit.net 00:01:44.13]                   Price = 10M, 
[xUnit.net 00:01:44.13]                   ResultValues = {empty}, 
[xUnit.net 00:01:44.13]                   SampleCollection = {Cyclic reference to type Open_lab.Models.SampleCollection detected}, 
[xUnit.net 00:01:44.13]                   Status = <null>, 
[xUnit.net 00:01:44.13]                   Test = Open_lab.Models.Test
[xUnit.net 00:01:44.13]                   {
[xUnit.net 00:01:44.13]                       Code = "T-75beae97", 
[xUnit.net 00:01:44.13]       
[xUnit.net 00:01:44.13]       (Output has exceeded the maximum of 100 lines. Increase FormattingOptions.MaxLines on AssertionScope or AssertionOptions to include more lines.)
[xUnit.net 00:01:44.13]                       Comments = {empty}, .
[xUnit.net 00:01:44.13]       Stack Trace:
[xUnit.net 00:01:44.13]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:44.13]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:44.13]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:44.13]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:44.13]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module6Tests_Additional.cs(268,0): at Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_Should_Return_Only_Non_Separated_Samples_SuccessGuard()
[xUnit.net 00:01:44.13]         --- End of stack trace from previous location ---
[xUnit.net 00:01:44.19]     Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_With_All_Separated_Should_Return_Empty_EdgeGuard [FAIL]
[xUnit.net 00:01:44.19]       Expected pending to be empty, but found 
[xUnit.net 00:01:44.19]       {
[xUnit.net 00:01:44.19]           Open_lab.Models.SampleCollection
[xUnit.net 00:01:44.19]           {
[xUnit.net 00:01:44.19]               CollectedAt = <2026-04-30 14:44:34.6577135>, 
[xUnit.net 00:01:44.19]               CollectedBy = 1, 
[xUnit.net 00:01:44.19]               CollectedByUser = <null>, 
[xUnit.net 00:01:44.19]               IsExternalSample = False, 
[xUnit.net 00:01:44.19]               IsSeparated = True, 
[xUnit.net 00:01:44.19]               ReceivedBy = <null>, 
[xUnit.net 00:01:44.19]               ReceivedByUser = <null>, 
[xUnit.net 00:01:44.19]               SampleId = 1, 
[xUnit.net 00:01:44.19]               Status = "مفصولة - Centrifuge", 
[xUnit.net 00:01:44.19]               VisitTest = Open_lab.Models.VisitTest
[xUnit.net 00:01:44.19]               {
[xUnit.net 00:01:44.19]                   ExternalQueueItem = <null>, 
[xUnit.net 00:01:44.19]                   Price = 10M, 
[xUnit.net 00:01:44.19]                   ResultValues = {empty}, 
[xUnit.net 00:01:44.19]                   SampleCollection = {Cyclic reference to type Open_lab.Models.SampleCollection detected}, 
[xUnit.net 00:01:44.19]                   Status = <null>, 
[xUnit.net 00:01:44.19]                   Test = Open_lab.Models.Test
[xUnit.net 00:01:44.19]                   {
[xUnit.net 00:01:44.19]                       Code = "T-7211aa50", 
[xUnit.net 00:01:44.19]                       Comments = {empty}, 
[xUnit.net 00:01:44.19]                       CostPrice = <null>, 
[xUnit.net 00:01:44.19]                       CustomGroupItems = {empty}, 
[xUnit.net 00:01:44.19]                       Group = <null>, 
[xUnit.net 00:01:44.19]                       GroupId = <null>, 
[xUnit.net 00:01:44.19]                       IsRoutine = False, 
[xUnit.net 00:01:44.19]                       IsSendOut = False, 
[xUnit.net 00:01:44.19]                       NameReceipt = "", 
[xUnit.net 00:01:44.19]                       NameReport = "Test", 
[xUnit.net 00:01:44.19]                       Parameters = {empty}, 
[xUnit.net 00:01:44.19]                       PatientPrice = <null>, 
[xUnit.net 00:01:44.19]                       Price = 10M, 
[xUnit.net 00:01:44.19]                       PriceListItems = {empty}, 
[xUnit.net 00:01:44.19]                       ReferenceRanges = {empty}, 
[xUnit.net 00:01:44.19]                       ReportOrder = 0, 
[xUnit.net 00:01:44.19]                       SampleType = <null>, 
[xUnit.net 00:01:44.19]                       SampleTypeId = <null>, 
[xUnit.net 00:01:44.19]                       TestId = 1, 
[xUnit.net 00:01:44.19]                       TurnaroundHours = 0, 
[xUnit.net 00:01:44.19]                       Unit = <null>, 
[xUnit.net 00:01:44.19]                       UnitId = <null>, 
[xUnit.net 00:01:44.19]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:01:44.19]                   }, 
[xUnit.net 00:01:44.19]                   TestId = 1, 
[xUnit.net 00:01:44.19]                   Visit = Open_lab.Models.Visit
[xUnit.net 00:01:44.19]                   {
[xUnit.net 00:01:44.19]                       AccountType = <null>, 
[xUnit.net 00:01:44.19]                       Branch = <null>, 
[xUnit.net 00:01:44.19]                       BranchId = <null>, 
[xUnit.net 00:01:44.19]                       Invoice = <null>, 
[xUnit.net 00:01:44.19]                       Patient = Open_lab.Models.Patient
[xUnit.net 00:01:44.19]                       {
[xUnit.net 00:01:44.19]                           Address = <null>, 
[xUnit.net 00:01:44.19]                           Age = <null>, 
[xUnit.net 00:01:44.19]                           BirthDate = <null>, 
[xUnit.net 00:01:44.19]                           FullName = "All Separated", 
[xUnit.net 00:01:44.19]                           Gender = "Male", 
[xUnit.net 00:01:44.19]                           IsPregnant = False, 
[xUnit.net 00:01:44.19]                           LabId = "L-ALLSEP", 
[xUnit.net 00:01:44.19]                           MedicalHistory = <null>, 
[xUnit.net 00:01:44.19]                           PatientId = 1, 
[xUnit.net 00:01:44.19]                           Phone = <null>, 
[xUnit.net 00:01:44.19]                           Visits = {{Cyclic reference to type Open_lab.Models.Visit detected}}
[xUnit.net 00:01:44.19]                       }, 
[xUnit.net 00:01:44.19]                       PatientId = 1, 
[xUnit.net 00:01:44.19]                       Physician = <null>, 
[xUnit.net 00:01:44.19]                       PhysicianId = <null>, 
[xUnit.net 00:01:44.19]                       Referral = <null>, 
[xUnit.net 00:01:44.19]                       ReferralId = <null>, 
[xUnit.net 00:01:44.19]                       Status = <null>, 
[xUnit.net 00:01:44.20]                       VisitDate = <2026-04-30 14:44:34.6566213>, 
[xUnit.net 00:01:44.20]                       VisitId = 1, 
[xUnit.net 00:01:44.20]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:01:44.20]                   }, 
[xUnit.net 00:01:44.20]                   VisitId = 1, 
[xUnit.net 00:01:44.20]                   VisitTestId = 1
[xUnit.net 00:01:44.20]               }, 
[xUnit.net 00:01:44.20]               VisitTestId = 1
[xUnit.net 00:01:44.20]           }
[xUnit.net 00:01:44.20]       }.
[xUnit.net 00:01:44.20]       Stack Trace:
[xUnit.net 00:01:44.20]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:44.20]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:44.20]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:44.20]            at FluentAssertions.Execution.GivenSelector`1.FailWith(String message, Object[] args)
[xUnit.net 00:01:44.20]            at FluentAssertions.Collections.GenericCollectionAssertions`3.BeEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:44.20]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module6Tests_Additional.cs(287,0): at Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_With_All_Separated_Should_Return_Empty_EdgeGuard()
[xUnit.net 00:01:44.20]         --- End of stack trace from previous location ---
[xUnit.net 00:01:44.77]     Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(raw: "مقاومة", expected: "R") [FAIL]
[xUnit.net 00:01:44.77]       System.InvalidOperationException : قيمة الحساسية غير مدعومة. استخدم S أو I أو R.
[xUnit.net 00:01:44.77]       Stack Trace:
[xUnit.net 00:01:44.77]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(227,0): at Open_lab.Services.CultureSensitivityService.ClassifySensitivity(String rawSensitivity)
[xUnit.net 00:01:44.77]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(288,0): at Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(String raw, String expected)
[xUnit.net 00:01:44.77]            at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
[xUnit.net 00:01:44.77]            at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs(Object obj, Span`1 copyOfArgs, BindingFlags invokeAttr)
[xUnit.net 00:01:44.77]     Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(raw: "حساس جدا", expected: "S") [FAIL]
[xUnit.net 00:01:44.77]       System.InvalidOperationException : قيمة الحساسية غير مدعومة. استخدم S أو I أو R.
[xUnit.net 00:01:44.77]       Stack Trace:
[xUnit.net 00:01:44.77]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(227,0): at Open_lab.Services.CultureSensitivityService.ClassifySensitivity(String rawSensitivity)
[xUnit.net 00:01:44.77]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(288,0): at Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(String raw, String expected)
[xUnit.net 00:01:44.77]            at InvokeStub_Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(Object, Span`1)
[xUnit.net 00:01:44.77]            at System.Reflection.MethodBaseInvoker.InvokeWithFewArgs(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
[xUnit.net 00:01:44.78]     Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(raw: "مستعمرات قليله", expected: "S") [FAIL]
[xUnit.net 00:01:44.78]       System.InvalidOperationException : قيمة الحساسية غير مدعومة. استخدم S أو I أو R.
[xUnit.net 00:01:44.78]       Stack Trace:
[xUnit.net 00:01:44.78]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(227,0): at Open_lab.Services.CultureSensitivityService.ClassifySensitivity(String rawSensitivity)
[xUnit.net 00:01:44.78]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(288,0): at Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(String raw, String expected)
[xUnit.net 00:01:44.78]            at InvokeStub_Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(Object, Span`1)
[xUnit.net 00:01:44.78]            at System.Reflection.MethodBaseInvoker.InvokeWithFewArgs(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
[xUnit.net 00:01:44.78]     Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(raw: "متوسط الحساسية", expected: "I") [FAIL]
[xUnit.net 00:01:44.78]       System.InvalidOperationException : قيمة الحساسية غير مدعومة. استخدم S أو I أو R.
[xUnit.net 00:01:44.78]       Stack Trace:
[xUnit.net 00:01:44.78]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(227,0): at Open_lab.Services.CultureSensitivityService.ClassifySensitivity(String rawSensitivity)
[xUnit.net 00:01:44.78]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(288,0): at Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(String raw, String expected)
[xUnit.net 00:01:44.78]            at InvokeStub_Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(Object, Span`1)
[xUnit.net 00:01:44.78]            at System.Reflection.MethodBaseInvoker.InvokeWithFewArgs(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
[xUnit.net 00:01:44.81]     Open_lab.Tests.Module5ServiceTests_Additional.GetFilteredAntibioticsAsync_Combined_Filter_Pregnant_And_Child_Should_Apply_Both_SuccessGuard [FAIL]
[xUnit.net 00:01:44.81]       Expected filtered to contain 1 item(s), but found 2: 
[xUnit.net 00:01:44.81]       {
[xUnit.net 00:01:44.81]           Open_lab.Models.Antibiotic
[xUnit.net 00:01:44.81]           {
[xUnit.net 00:01:44.81]               AntibioticId = 1, 
[xUnit.net 00:01:44.81]               CultureAntibiotics = {empty}, 
[xUnit.net 00:01:44.81]               IsSafeForChildren = True, 
[xUnit.net 00:01:44.81]               IsSafeForPregnancy = True, 
[xUnit.net 00:01:44.81]               Name = "SafeForBoth"
[xUnit.net 00:01:44.81]           }, 
[xUnit.net 00:01:44.81]           Open_lab.Models.Antibiotic
[xUnit.net 00:01:44.81]           {
[xUnit.net 00:01:44.81]               AntibioticId = 3, 
[xUnit.net 00:01:44.81]               CultureAntibiotics = {empty}, 
[xUnit.net 00:01:44.81]               IsSafeForChildren = False, 
[xUnit.net 00:01:44.81]               IsSafeForPregnancy = True, 
[xUnit.net 00:01:44.81]               Name = "UnsafeChildOnly"
[xUnit.net 00:01:44.81]           }
[xUnit.net 00:01:44.81]       }.
[xUnit.net 00:01:44.81]       Stack Trace:
[xUnit.net 00:01:44.81]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:44.81]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:44.81]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:44.81]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:44.81]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(476,0): at Open_lab.Tests.Module5ServiceTests_Additional.GetFilteredAntibioticsAsync_Combined_Filter_Pregnant_And_Child_Should_Apply_Both_SuccessGuard()
[xUnit.net 00:01:44.81]         --- End of stack trace from previous location ---
[xUnit.net 00:01:44.96]     Open_lab.Tests.Module5ServiceTests_Additional.SaveCultureResultAsync_With_Multiple_Sensitivities_Should_Classify_Each_SuccessGuard [FAIL]
[xUnit.net 00:01:44.96]       Expected values to contain 3 item(s), but found 4: 
[xUnit.net 00:01:44.96]       Open_lab.Models.ResultValue
[xUnit.net 00:01:44.96]           {
[xUnit.net 00:01:44.96]               Comment = <null>, 
[xUnit.net 00:01:44.96]               Flag = <null>, 
[xUnit.net 00:01:44.96]               Parameter = Open_lab.Models.TestParameter
[xUnit.net 00:01:44.96]               {
[xUnit.net 00:01:44.96]                   Name = "Culture", 
[xUnit.net 00:01:44.96]                   OrderNo = 1, 
[xUnit.net 00:01:44.96]                   ParameterId = 1, 
[xUnit.net 00:01:44.96]                   ResultValues = {{Cyclic reference to type Open_lab.Models.ResultValue detected}}, 
[xUnit.net 00:01:44.96]                   Test = Open_lab.Models.Test
[xUnit.net 00:01:44.96]                   {
[xUnit.net 00:01:44.96]                       Code = "CULT-SENS", 
[xUnit.net 00:01:44.96]                       Comments = {empty}, 
[xUnit.net 00:01:44.96]                       CostPrice = <null>, 
[xUnit.net 00:01:44.96]                       CustomGroupItems = {empty}, 
[xUnit.net 00:01:44.96]                       Group = <null>, 
[xUnit.net 00:01:44.96]                       GroupId = <null>, 
[xUnit.net 00:01:44.96]                       IsRoutine = False, 
[xUnit.net 00:01:44.96]                       IsSendOut = False, 
[xUnit.net 00:01:44.96]                       NameReceipt = "Culture", 
[xUnit.net 00:01:44.96]                       NameReport = "Culture Sensitivity", 
[xUnit.net 00:01:44.96]                       {
[xUnit.net 00:01:44.96]                       Parameters = {Cyclic reference to type Open_lab.Models.TestParameter detected}, 
[xUnit.net 00:01:44.96]                           Open_lab.Models.TestParameter
[xUnit.net 00:01:44.96]                           {
[xUnit.net 00:01:44.96]                               Name = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               OrderNo = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               ParameterId = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               ResultValues = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               Test = {Cyclic reference to type Open_lab.Models.Test detected}, 
[xUnit.net 00:01:44.96]                               TestId = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               Unit = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               UnitId = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           }, 
[xUnit.net 00:01:44.96]                           Open_lab.Models.TestParameter
[xUnit.net 00:01:44.96]                           {
[xUnit.net 00:01:44.96]                               Name = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               OrderNo = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               ParameterId = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               ResultValues = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               Test = {Cyclic reference to type Open_lab.Models.Test detected}, 
[xUnit.net 00:01:44.96]                               TestId = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               Unit = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.96]                               UnitId = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           }, 
[xUnit.net 00:01:44.96]                           Open_lab.Models.TestParameter
[xUnit.net 00:01:44.96]                           {
[xUnit.net 00:01:44.96]                               Name = 
[xUnit.net 00:01:44.96]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.96]                           , 
[xUnit.net 00:01:44.97]                               OrderNo = 
[xUnit.net 00:01:44.97]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.97]                           , 
[xUnit.net 00:01:44.97]                               ParameterId = 
[xUnit.net 00:01:44.97]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.97]                           , 
[xUnit.net 00:01:44.97]                               ResultValues = 
[xUnit.net 00:01:44.97]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.97]                           , 
[xUnit.net 00:01:44.97]                               Test = {Cyclic reference to type Open_lab.Models.Test detected}, 
[xUnit.net 00:01:44.97]                               TestId = 
[xUnit.net 00:01:44.97]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.97]                           , 
[xUnit.net 00:01:44.97]                               Unit = 
[xUnit.net 00:01:44.97]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.97]                           , 
[xUnit.net 00:01:44.97]                               UnitId = 
[xUnit.net 00:01:44.97]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:01:44.97]                                           }
[xUnit.net 00:01:44.97]                       }, 
[xUnit.net 00:01:44.97]                       PatientPrice = <null>, 
[xUnit.net 00:01:44.97]                       Price = 10M, 
[xUnit.net 00:01:44.97]                       PriceListItems = {empty}, 
[xUnit.net 00:01:44.97]                       ReferenceRanges = {empty}, 
[xUnit.net 00:01:44.97]                       ReportOrder = 0, 
[xUnit.net 00:01:44.97]       
[xUnit.net 00:01:44.97]       (Output has exceeded the maximum of 100 lines. Increase FormattingOptions.MaxLines on AssertionScope or AssertionOptions to include more lines.)
[xUnit.net 00:01:44.97]                       SampleType = <null>, .
[xUnit.net 00:01:44.97]       Stack Trace:
[xUnit.net 00:01:44.97]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:44.97]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:44.97]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:44.97]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:44.97]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(253,0): at Open_lab.Tests.Module5ServiceTests_Additional.SaveCultureResultAsync_With_Multiple_Sensitivities_Should_Classify_Each_SuccessGuard()
[xUnit.net 00:01:44.97]         --- End of stack trace from previous location ---
[xUnit.net 00:01:45.04]     Open_lab.Tests.Module5ServiceTests_Additional.CreateCultureAsync_With_Null_Organism_Should_Succeed_EdgeGuard [FAIL]
[xUnit.net 00:01:45.04]       System.ArgumentException : الكائن الدقيق المعزول مطلوب. (Parameter 'culture')
[xUnit.net 00:01:45.04]       Stack Trace:
[xUnit.net 00:01:45.04]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(38,0): at Open_lab.Services.CultureSensitivityService.CreateCultureAsync(Culture culture)
[xUnit.net 00:01:45.04]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(77,0): at Open_lab.Tests.Module5ServiceTests_Additional.CreateCultureAsync_With_Null_Organism_Should_Succeed_EdgeGuard()
[xUnit.net 00:01:45.04]         --- End of stack trace from previous location ---
[xUnit.net 00:01:45.05]     Open_lab.Tests.Module5ServiceTests_Additional.CreateCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard [FAIL]
[xUnit.net 00:01:45.05]       System.ArgumentException : عدد المستعمرات يجب أن يكون أكبر من صفر. (Parameter 'culture')
[xUnit.net 00:01:45.05]       Stack Trace:
[xUnit.net 00:01:45.05]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(46,0): at Open_lab.Services.CultureSensitivityService.CreateCultureAsync(Culture culture)
[xUnit.net 00:01:45.05]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(55,0): at Open_lab.Tests.Module5ServiceTests_Additional.CreateCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard()
[xUnit.net 00:01:45.05]         --- End of stack trace from previous location ---
[xUnit.net 00:01:45.13]     Open_lab.Tests.Module4ViewModelTests_Additional.PrintBlankAsync_Should_Call_PrintService_SuccessGuard [FAIL]
[xUnit.net 00:01:45.13]       Expected viewModel.StatusMessage "تم إرسال التقرير الفارغ للطباعة." to contain "تم إرسال النموذج".
[xUnit.net 00:01:45.13]       Stack Trace:
[xUnit.net 00:01:45.13]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:45.13]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:45.13]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:45.13]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:45.14]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(715,0): at Open_lab.Tests.Module4ViewModelTests_Additional.PrintBlankAsync_Should_Call_PrintService_SuccessGuard()
[xUnit.net 00:01:45.14]         --- End of stack trace from previous location ---
[xUnit.net 00:01:45.28]     Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_Multiple_Results_Should_Group_By_Visit_EdgeGuard [FAIL]
[xUnit.net 00:01:45.28]       Expected viewModel.HistoryResults to contain 3 item(s), but found 0: {empty}.
[xUnit.net 00:01:45.28]       Stack Trace:
[xUnit.net 00:01:45.28]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:45.28]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:45.28]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:45.28]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:45.28]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(766,0): at Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_Multiple_Results_Should_Group_By_Visit_EdgeGuard()
[xUnit.net 00:01:45.28]         --- End of stack trace from previous location ---
[xUnit.net 00:01:45.28]     Open_lab.Tests.Module4ViewModelTests_Additional.ReopenResultsAsync_With_Verified_Status_Should_Call_Service_SuccessGuard [FAIL]
[xUnit.net 00:01:45.28]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تم فتح التحليل".
[xUnit.net 00:01:45.28]       Stack Trace:
[xUnit.net 00:01:45.28]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:45.28]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:45.28]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:45.28]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:45.28]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(644,0): at Open_lab.Tests.Module4ViewModelTests_Additional.ReopenResultsAsync_With_Verified_Status_Should_Call_Service_SuccessGuard()
[xUnit.net 00:01:45.28]         --- End of stack trace from previous location ---
[xUnit.net 00:01:45.36]     Open_lab.Tests.Module4ViewModelTests_Additional.LoadAsync_With_Null_Referral_Should_Handle_EdgeGuard [FAIL]
[xUnit.net 00:01:45.37]       Expected viewModel.ReferralName to be empty, but found "—".
[xUnit.net 00:01:45.37]       Stack Trace:
[xUnit.net 00:01:45.37]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:45.37]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:45.37]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:45.37]            at FluentAssertions.Primitives.StringAssertions`1.BeEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:45.37]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(729,0): at Open_lab.Tests.Module4ViewModelTests_Additional.LoadAsync_With_Null_Referral_Should_Handle_EdgeGuard()
[xUnit.net 00:01:45.37]         --- End of stack trace from previous location ---
[xUnit.net 00:01:45.61]     Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_No_History_Should_Show_Empty_State_EdgeGuard [FAIL]
[xUnit.net 00:01:45.61]       Expected viewModel.StatusMessage "تم تحميل 0 نتيجة من 0 زيارات سابقة." to contain "لا توجد".
[xUnit.net 00:01:45.61]       Stack Trace:
[xUnit.net 00:01:45.61]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:45.61]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:45.61]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:45.61]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:45.61]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(795,0): at Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_No_History_Should_Show_Empty_State_EdgeGuard()
[xUnit.net 00:01:45.61]         --- End of stack trace from previous location ---
[xUnit.net 00:01:50.82]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+PriceListsViewModel_AdditionalTests.PrintListAsync_When_NoItems_Should_Not_Print_EdgeGuard [FAIL]
[xUnit.net 00:01:50.82]       Moq.MockException : 
[xUnit.net 00:01:50.82]       Expected invocation on the mock should never have been performed, but was 1 times: x => x.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>())
[xUnit.net 00:01:50.82]       
[xUnit.net 00:01:50.82]       Performed invocations:
[xUnit.net 00:01:50.82]       
[xUnit.net 00:01:50.82]          Mock<IPrintService:229> (x):
[xUnit.net 00:01:50.82]       
[xUnit.net 00:01:50.82]             IPrintService.PrintTextReportAsync("قائمة الأسعار", ObservableCollection<string>, "PriceList_2")
[xUnit.net 00:01:50.82]       
[xUnit.net 00:01:50.82]       Stack Trace:
[xUnit.net 00:01:50.82]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:01:50.82]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:01:50.82]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(731,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.PriceListsViewModel_AdditionalTests.PrintListAsync_When_NoItems_Should_Not_Print_EdgeGuard()
[xUnit.net 00:01:50.82]         --- End of stack trace from previous location ---
[xUnit.net 00:01:53.90]   Finished:    Open_lab.Tests
========== Test run finished: 1322 Tests (1282 Passed, 40 Failed, 0 Skipped) run in 1.9 min ==========
