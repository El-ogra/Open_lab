========== Starting test discovery ==========
[xUnit.net 00:00:00.01] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:05.62]   Discovering: Open_lab.Tests
[xUnit.net 00:00:07.44]   Discovered:  Open_lab.Tests
========== Test discovery finished: 1322 Tests found in 19.6 sec ==========
========== Starting test run ==========
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:01.18]   Starting:    Open_lab.Tests
[xUnit.net 00:00:06.08]     Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(raw: "مقاومة", expected: "R") [FAIL]
[xUnit.net 00:00:06.08]       System.InvalidOperationException : قيمة الحساسية غير مدعومة. استخدم S أو I أو R.
[xUnit.net 00:00:06.08]       Stack Trace:
[xUnit.net 00:00:06.09]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(227,0): at Open_lab.Services.CultureSensitivityService.ClassifySensitivity(String rawSensitivity)
[xUnit.net 00:00:06.09]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(288,0): at Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(String raw, String expected)
[xUnit.net 00:00:06.09]            at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
[xUnit.net 00:00:06.09]            at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs(Object obj, Span`1 copyOfArgs, BindingFlags invokeAttr)
[xUnit.net 00:00:06.09]     Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(raw: "حساس جدا", expected: "S") [FAIL]
[xUnit.net 00:00:06.09]       System.InvalidOperationException : قيمة الحساسية غير مدعومة. استخدم S أو I أو R.
[xUnit.net 00:00:06.09]       Stack Trace:
[xUnit.net 00:00:06.09]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(227,0): at Open_lab.Services.CultureSensitivityService.ClassifySensitivity(String rawSensitivity)
[xUnit.net 00:00:06.09]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(288,0): at Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(String raw, String expected)
[xUnit.net 00:00:06.09]            at InvokeStub_Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(Object, Span`1)
[xUnit.net 00:00:06.09]            at System.Reflection.MethodBaseInvoker.InvokeWithFewArgs(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
[xUnit.net 00:00:06.09]     Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(raw: "مستعمرات قليله", expected: "S") [FAIL]
[xUnit.net 00:00:06.09]       System.InvalidOperationException : قيمة الحساسية غير مدعومة. استخدم S أو I أو R.
[xUnit.net 00:00:06.09]       Stack Trace:
[xUnit.net 00:00:06.09]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(227,0): at Open_lab.Services.CultureSensitivityService.ClassifySensitivity(String rawSensitivity)
[xUnit.net 00:00:06.09]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(288,0): at Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(String raw, String expected)
[xUnit.net 00:00:06.09]            at InvokeStub_Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(Object, Span`1)
[xUnit.net 00:00:06.09]            at System.Reflection.MethodBaseInvoker.InvokeWithFewArgs(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
[xUnit.net 00:00:06.09]     Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(raw: "متوسط الحساسية", expected: "I") [FAIL]
[xUnit.net 00:00:06.09]       System.InvalidOperationException : قيمة الحساسية غير مدعومة. استخدم S أو I أو R.
[xUnit.net 00:00:06.09]       Stack Trace:
[xUnit.net 00:00:06.09]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(227,0): at Open_lab.Services.CultureSensitivityService.ClassifySensitivity(String rawSensitivity)
[xUnit.net 00:00:06.09]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(288,0): at Open_lab.Tests.Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(String raw, String expected)
[xUnit.net 00:00:06.09]            at InvokeStub_Module5ServiceTests_Additional.ClassifySensitivity_Should_Handle_Arabic_Input_SuccessGuard(Object, Span`1)
[xUnit.net 00:00:06.09]            at System.Reflection.MethodBaseInvoker.InvokeWithFewArgs(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
[xUnit.net 00:00:09.23]     Open_lab.Tests.Module5ServiceTests_Additional.GetFilteredAntibioticsAsync_Combined_Filter_Pregnant_And_Child_Should_Apply_Both_SuccessGuard [FAIL]
[xUnit.net 00:00:09.23]       Expected filtered to contain 1 item(s), but found 2: 
[xUnit.net 00:00:09.23]       {
[xUnit.net 00:00:09.23]           Open_lab.Models.Antibiotic
[xUnit.net 00:00:09.23]           {
[xUnit.net 00:00:09.23]               AntibioticId = 1, 
[xUnit.net 00:00:09.23]               CultureAntibiotics = {empty}, 
[xUnit.net 00:00:09.23]               IsSafeForChildren = True, 
[xUnit.net 00:00:09.23]               IsSafeForPregnancy = True, 
[xUnit.net 00:00:09.23]               Name = "SafeForBoth"
[xUnit.net 00:00:09.23]           }, 
[xUnit.net 00:00:09.23]           Open_lab.Models.Antibiotic
[xUnit.net 00:00:09.24]           {
[xUnit.net 00:00:09.24]               AntibioticId = 3, 
[xUnit.net 00:00:09.24]               CultureAntibiotics = {empty}, 
[xUnit.net 00:00:09.24]               IsSafeForChildren = False, 
[xUnit.net 00:00:09.24]               IsSafeForPregnancy = True, 
[xUnit.net 00:00:09.24]               Name = "UnsafeChildOnly"
[xUnit.net 00:00:09.24]           }
[xUnit.net 00:00:09.24]       }.
[xUnit.net 00:00:09.24]       Stack Trace:
[xUnit.net 00:00:09.24]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:09.24]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:09.24]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:09.24]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.24]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:09.24]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:09.24]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:09.24]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(476,0): at Open_lab.Tests.Module5ServiceTests_Additional.GetFilteredAntibioticsAsync_Combined_Filter_Pregnant_And_Child_Should_Apply_Both_SuccessGuard()
[xUnit.net 00:00:09.24]         --- End of stack trace from previous location ---
[xUnit.net 00:00:10.07]     Open_lab.Tests.Module5ServiceTests_Additional.SaveCultureResultAsync_With_Multiple_Sensitivities_Should_Classify_Each_SuccessGuard [FAIL]
[xUnit.net 00:00:10.07]       Expected values to contain 3 item(s), but found 4: 
[xUnit.net 00:00:10.07]       Open_lab.Models.ResultValue
[xUnit.net 00:00:10.07]           {
[xUnit.net 00:00:10.07]               Comment = <null>, 
[xUnit.net 00:00:10.07]               Flag = <null>, 
[xUnit.net 00:00:10.07]               Parameter = Open_lab.Models.TestParameter
[xUnit.net 00:00:10.07]               {
[xUnit.net 00:00:10.07]                   Name = "Culture", 
[xUnit.net 00:00:10.07]                   OrderNo = 1, 
[xUnit.net 00:00:10.07]                   ParameterId = 1, 
[xUnit.net 00:00:10.07]                   ResultValues = {{Cyclic reference to type Open_lab.Models.ResultValue detected}}, 
[xUnit.net 00:00:10.07]                   Test = Open_lab.Models.Test
[xUnit.net 00:00:10.07]                   {
[xUnit.net 00:00:10.07]                       Code = "CULT-SENS", 
[xUnit.net 00:00:10.07]                       Comments = {empty}, 
[xUnit.net 00:00:10.07]                       CostPrice = <null>, 
[xUnit.net 00:00:10.07]                       CustomGroupItems = {empty}, 
[xUnit.net 00:00:10.07]                       Group = <null>, 
[xUnit.net 00:00:10.07]                       GroupId = <null>, 
[xUnit.net 00:00:10.07]                       IsRoutine = False, 
[xUnit.net 00:00:10.07]                       IsSendOut = False, 
[xUnit.net 00:00:10.07]                       NameReceipt = "Culture", 
[xUnit.net 00:00:10.07]                       NameReport = "Culture Sensitivity", 
[xUnit.net 00:00:10.07]                       {
[xUnit.net 00:00:10.07]                       Parameters = {Cyclic reference to type Open_lab.Models.TestParameter detected}, 
[xUnit.net 00:00:10.07]                           Open_lab.Models.TestParameter
[xUnit.net 00:00:10.07]                           {
[xUnit.net 00:00:10.07]                               Name = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           , 
[xUnit.net 00:00:10.07]                               OrderNo = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           , 
[xUnit.net 00:00:10.07]                               ParameterId = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           , 
[xUnit.net 00:00:10.07]                               ResultValues = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           , 
[xUnit.net 00:00:10.07]                               Test = {Cyclic reference to type Open_lab.Models.Test detected}, 
[xUnit.net 00:00:10.07]                               TestId = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           , 
[xUnit.net 00:00:10.07]                               Unit = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           , 
[xUnit.net 00:00:10.07]                               UnitId = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           }, 
[xUnit.net 00:00:10.07]                           Open_lab.Models.TestParameter
[xUnit.net 00:00:10.07]                           {
[xUnit.net 00:00:10.07]                               Name = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           , 
[xUnit.net 00:00:10.07]                               OrderNo = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           , 
[xUnit.net 00:00:10.07]                               ParameterId = 
[xUnit.net 00:00:10.07]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.07]                           , 
[xUnit.net 00:00:10.07]                               ResultValues = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           , 
[xUnit.net 00:00:10.08]                               Test = {Cyclic reference to type Open_lab.Models.Test detected}, 
[xUnit.net 00:00:10.08]                               TestId = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           , 
[xUnit.net 00:00:10.08]                               Unit = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           , 
[xUnit.net 00:00:10.08]                               UnitId = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           }, 
[xUnit.net 00:00:10.08]                           Open_lab.Models.TestParameter
[xUnit.net 00:00:10.08]                           {
[xUnit.net 00:00:10.08]                               Name = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           , 
[xUnit.net 00:00:10.08]                               OrderNo = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           , 
[xUnit.net 00:00:10.08]                               ParameterId = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           , 
[xUnit.net 00:00:10.08]                               ResultValues = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           , 
[xUnit.net 00:00:10.08]                               Test = {Cyclic reference to type Open_lab.Models.Test detected}, 
[xUnit.net 00:00:10.08]                               TestId = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           , 
[xUnit.net 00:00:10.08]                               Unit = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                           , 
[xUnit.net 00:00:10.08]                               UnitId = 
[xUnit.net 00:00:10.08]                           Maximum recursion depth of 5 was reached.  Increase MaxDepth on AssertionScope or AssertionOptions to get more details.
[xUnit.net 00:00:10.08]                                           }
[xUnit.net 00:00:10.08]                       }, 
[xUnit.net 00:00:10.08]                       PatientPrice = <null>, 
[xUnit.net 00:00:10.08]                       Price = 10M, 
[xUnit.net 00:00:10.08]                       PriceListItems = {empty}, 
[xUnit.net 00:00:10.08]                       ReferenceRanges = {empty}, 
[xUnit.net 00:00:10.08]                       ReportOrder = 0, 
[xUnit.net 00:00:10.08]       
[xUnit.net 00:00:10.08]       (Output has exceeded the maximum of 100 lines. Increase FormattingOptions.MaxLines on AssertionScope or AssertionOptions to include more lines.)
[xUnit.net 00:00:10.08]                       SampleType = <null>, .
[xUnit.net 00:00:10.08]       Stack Trace:
[xUnit.net 00:00:10.08]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:10.08]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:10.08]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:10.08]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:10.08]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:10.08]            at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
[xUnit.net 00:00:10.08]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:10.08]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(253,0): at Open_lab.Tests.Module5ServiceTests_Additional.SaveCultureResultAsync_With_Multiple_Sensitivities_Should_Classify_Each_SuccessGuard()
[xUnit.net 00:00:10.08]         --- End of stack trace from previous location ---
[xUnit.net 00:00:10.33]     Open_lab.Tests.Module5ServiceTests_Additional.CreateCultureAsync_With_Null_Organism_Should_Succeed_EdgeGuard [FAIL]
[xUnit.net 00:00:10.33]       System.ArgumentException : الكائن الدقيق المعزول مطلوب. (Parameter 'culture')
[xUnit.net 00:00:10.33]       Stack Trace:
[xUnit.net 00:00:10.33]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(38,0): at Open_lab.Services.CultureSensitivityService.CreateCultureAsync(Culture culture)
[xUnit.net 00:00:10.33]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(77,0): at Open_lab.Tests.Module5ServiceTests_Additional.CreateCultureAsync_With_Null_Organism_Should_Succeed_EdgeGuard()
[xUnit.net 00:00:10.33]         --- End of stack trace from previous location ---
[xUnit.net 00:00:10.34]     Open_lab.Tests.Module5ServiceTests_Additional.CreateCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard [FAIL]
[xUnit.net 00:00:10.34]       System.ArgumentException : عدد المستعمرات يجب أن يكون أكبر من صفر. (Parameter 'culture')
[xUnit.net 00:00:10.34]       Stack Trace:
[xUnit.net 00:00:10.34]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab\Services\CultureSensitivityService.cs(46,0): at Open_lab.Services.CultureSensitivityService.CreateCultureAsync(Culture culture)
[xUnit.net 00:00:10.34]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(55,0): at Open_lab.Tests.Module5ServiceTests_Additional.CreateCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard()
[xUnit.net 00:00:10.34]         --- End of stack trace from previous location ---
[xUnit.net 00:00:29.15]     Open_lab.Tests.Module8ViewModelTests_Additional.LoadSettlementCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:00:29.15]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:00:29.15]       Stack Trace:
[xUnit.net 00:00:29.15]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:29.15]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:29.15]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:29.15]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:29.15]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:29.15]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module8Tests_Additional.cs(758,0): at Open_lab.Tests.Module8ViewModelTests_Additional.LoadSettlementCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:00:29.15]         --- End of stack trace from previous location ---
[xUnit.net 00:00:29.86]     Open_lab.Tests.Module8ViewModelTests_Additional.LoadQueueCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:00:29.86]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:00:29.86]       Stack Trace:
[xUnit.net 00:00:29.87]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:29.87]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:29.87]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:29.87]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:29.87]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:29.87]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module8Tests_Additional.cs(729,0): at Open_lab.Tests.Module8ViewModelTests_Additional.LoadQueueCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:00:29.87]         --- End of stack trace from previous location ---
[xUnit.net 00:00:36.30]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+TestCommentsViewModel_AdditionalTests.SaveAsync_With_Empty_Comment_Text_Should_NotCall_Service_EdgeGuard [FAIL]
[xUnit.net 00:00:36.30]       Moq.MockException : 
[xUnit.net 00:00:36.30]       Expected invocation on the mock should never have been performed, but was 1 times: x => x.CreateTestCommentAsync(It.IsAny<TestComment>())
[xUnit.net 00:00:36.30]       
[xUnit.net 00:00:36.30]       Performed invocations:
[xUnit.net 00:00:36.30]       
[xUnit.net 00:00:36.30]          Mock<ITestCatalogService:105> (x):
[xUnit.net 00:00:36.30]       
[xUnit.net 00:00:36.30]             ITestCatalogService.GetAllTestsAsync()
[xUnit.net 00:00:36.30]             ITestCatalogService.GetTestCommentsAsync(1)
[xUnit.net 00:00:36.30]             ITestCatalogService.CreateTestCommentAsync(TestComment)
[xUnit.net 00:00:36.30]       
[xUnit.net 00:00:36.30]       Stack Trace:
[xUnit.net 00:00:36.30]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:00:36.30]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:00:36.30]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(859,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.TestCommentsViewModel_AdditionalTests.SaveAsync_With_Empty_Comment_Text_Should_NotCall_Service_EdgeGuard()
[xUnit.net 00:00:36.30]         --- End of stack trace from previous location ---
[xUnit.net 00:00:38.72]     Open_lab.Tests.Services.Module11ServiceTests_Additional.GenerateAttendanceReport_WithLogsButNoStatuses_ShouldCountDaysWithLogsAsPresent_SuccessGuard [FAIL]
[xUnit.net 00:00:38.72]       Expected rows[0].PresentDays to be 1 because Day with login record but no explicit status should count as present, but found 0 (difference of -1).
[xUnit.net 00:00:38.72]       Stack Trace:
[xUnit.net 00:00:38.72]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:38.72]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:38.72]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:38.72]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:38.72]            at FluentAssertions.Numeric.NumericAssertions`2.Be(T expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:38.72]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module11ServiceTests_Additional.cs(862,0): at Open_lab.Tests.Services.Module11ServiceTests_Additional.GenerateAttendanceReport_WithLogsButNoStatuses_ShouldCountDaysWithLogsAsPresent_SuccessGuard()
[xUnit.net 00:00:38.72]         --- End of stack trace from previous location ---
[xUnit.net 00:00:45.38]     Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_Should_Return_Only_Non_Separated_Samples_SuccessGuard [FAIL]
[xUnit.net 00:00:45.38]       Expected pending to contain 2 item(s), but found 3: Open_lab.Models.SampleCollection
[xUnit.net 00:00:45.38]           {
[xUnit.net 00:00:45.38]               CollectedAt = <2026-04-30 16:03:41.2676992>, 
[xUnit.net 00:00:45.38]               CollectedBy = 1, 
[xUnit.net 00:00:45.38]               CollectedByUser = <null>, 
[xUnit.net 00:00:45.38]               IsExternalSample = False, 
[xUnit.net 00:00:45.38]               IsSeparated = False, 
[xUnit.net 00:00:45.38]               ReceivedBy = <null>, 
[xUnit.net 00:00:45.38]               ReceivedByUser = <null>, 
[xUnit.net 00:00:45.38]               SampleId = 3, 
[xUnit.net 00:00:45.38]               Status = "مسحوبة", 
[xUnit.net 00:00:45.38]               VisitTest = Open_lab.Models.VisitTest
[xUnit.net 00:00:45.38]               {
[xUnit.net 00:00:45.38]                   ExternalQueueItem = <null>, 
[xUnit.net 00:00:45.38]                   Price = 10M, 
[xUnit.net 00:00:45.38]                   ResultValues = {empty}, 
[xUnit.net 00:00:45.38]                   SampleCollection = {Cyclic reference to type Open_lab.Models.SampleCollection detected}, 
[xUnit.net 00:00:45.38]                   Status = <null>, 
[xUnit.net 00:00:45.38]                   Test = Open_lab.Models.Test
[xUnit.net 00:00:45.38]                   {
[xUnit.net 00:00:45.38]                       Code = "T-c9102428", 
[xUnit.net 00:00:45.38]                       Comments = {empty}, 
[xUnit.net 00:00:45.38]                       CostPrice = <null>, 
[xUnit.net 00:00:45.38]                       CustomGroupItems = {empty}, 
[xUnit.net 00:00:45.38]                       Group = <null>, 
[xUnit.net 00:00:45.38]                       GroupId = <null>, 
[xUnit.net 00:00:45.38]                       IsRoutine = False, 
[xUnit.net 00:00:45.38]                       IsSendOut = False, 
[xUnit.net 00:00:45.38]                       NameReceipt = "", 
[xUnit.net 00:00:45.38]                       NameReport = "Test", 
[xUnit.net 00:00:45.38]                       Parameters = {empty}, 
[xUnit.net 00:00:45.38]                       PatientPrice = <null>, 
[xUnit.net 00:00:45.38]                       Price = 10M, 
[xUnit.net 00:00:45.38]                       PriceListItems = {empty}, 
[xUnit.net 00:00:45.38]                       ReferenceRanges = {empty}, 
[xUnit.net 00:00:45.38]                       ReportOrder = 0, 
[xUnit.net 00:00:45.38]                       SampleType = <null>, 
[xUnit.net 00:00:45.38]                       SampleTypeId = <null>, 
[xUnit.net 00:00:45.38]                       TestId = 3, 
[xUnit.net 00:00:45.38]                       TurnaroundHours = 0, 
[xUnit.net 00:00:45.38]                       Unit = <null>, 
[xUnit.net 00:00:45.38]                       UnitId = <null>, 
[xUnit.net 00:00:45.38]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:00:45.38]                   }, 
[xUnit.net 00:00:45.38]                   TestId = 3, 
[xUnit.net 00:00:45.38]                   Visit = Open_lab.Models.Visit
[xUnit.net 00:00:45.38]                   {
[xUnit.net 00:00:45.38]                       AccountType = <null>, 
[xUnit.net 00:00:45.38]                       Branch = <null>, 
[xUnit.net 00:00:45.38]                       BranchId = <null>, 
[xUnit.net 00:00:45.38]                       Invoice = <null>, 
[xUnit.net 00:00:45.38]                       Patient = Open_lab.Models.Patient
[xUnit.net 00:00:45.38]                       {
[xUnit.net 00:00:45.38]                           Address = <null>, 
[xUnit.net 00:00:45.38]                           Age = <null>, 
[xUnit.net 00:00:45.38]                           BirthDate = <null>, 
[xUnit.net 00:00:45.38]                           FullName = "Pending 3", 
[xUnit.net 00:00:45.38]                           Gender = "Male", 
[xUnit.net 00:00:45.38]                           IsPregnant = False, 
[xUnit.net 00:00:45.38]                           LabId = "L-PND3", 
[xUnit.net 00:00:45.38]                           MedicalHistory = <null>, 
[xUnit.net 00:00:45.38]                           PatientId = 3, 
[xUnit.net 00:00:45.38]                           Phone = <null>, 
[xUnit.net 00:00:45.38]                           Visits = {{Cyclic reference to type Open_lab.Models.Visit detected}}
[xUnit.net 00:00:45.38]                       }, 
[xUnit.net 00:00:45.38]                       PatientId = 3, 
[xUnit.net 00:00:45.38]                       Physician = <null>, 
[xUnit.net 00:00:45.38]                       PhysicianId = <null>, 
[xUnit.net 00:00:45.38]                       Referral = <null>, 
[xUnit.net 00:00:45.38]                       ReferralId = <null>, 
[xUnit.net 00:00:45.38]                       Status = <null>, 
[xUnit.net 00:00:45.38]                       VisitDate = <2026-04-30 16:03:41.2661615>, 
[xUnit.net 00:00:45.38]                       VisitId = 3, 
[xUnit.net 00:00:45.38]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:00:45.38]                   }, 
[xUnit.net 00:00:45.38]                   VisitId = 3, 
[xUnit.net 00:00:45.38]                   VisitTestId = 3
[xUnit.net 00:00:45.38]               }, 
[xUnit.net 00:00:45.38]               VisitTestId = 3
[xUnit.net 00:00:45.38]           }Open_lab.Models.SampleCollection
[xUnit.net 00:00:45.38]           {
[xUnit.net 00:00:45.38]               CollectedAt = <2026-04-30 16:03:41.2672826>, 
[xUnit.net 00:00:45.38]               CollectedBy = 1, 
[xUnit.net 00:00:45.38]               CollectedByUser = <null>, 
[xUnit.net 00:00:45.38]               IsExternalSample = False, 
[xUnit.net 00:00:45.38]               IsSeparated = False, 
[xUnit.net 00:00:45.38]               ReceivedBy = <null>, 
[xUnit.net 00:00:45.38]               ReceivedByUser = <null>, 
[xUnit.net 00:00:45.38]               SampleId = 2, 
[xUnit.net 00:00:45.38]               Status = "مسحوبة", 
[xUnit.net 00:00:45.38]               VisitTest = Open_lab.Models.VisitTest
[xUnit.net 00:00:45.38]               {
[xUnit.net 00:00:45.38]                   ExternalQueueItem = <null>, 
[xUnit.net 00:00:45.38]                   Price = 10M, 
[xUnit.net 00:00:45.38]                   ResultValues = {empty}, 
[xUnit.net 00:00:45.38]                   SampleCollection = {Cyclic reference to type Open_lab.Models.SampleCollection detected}, 
[xUnit.net 00:00:45.38]                   Status = <null>, 
[xUnit.net 00:00:45.38]                   Test = Open_lab.Models.Test
[xUnit.net 00:00:45.38]                   {
[xUnit.net 00:00:45.38]                       Code = "T-25c45a74", 
[xUnit.net 00:00:45.38]       
[xUnit.net 00:00:45.38]       (Output has exceeded the maximum of 100 lines. Increase FormattingOptions.MaxLines on AssertionScope or AssertionOptions to include more lines.)
[xUnit.net 00:00:45.38]                       Comments = {empty}, .
[xUnit.net 00:00:45.38]       Stack Trace:
[xUnit.net 00:00:45.38]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:45.38]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:45.38]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:45.38]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:45.38]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:00:45.38]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module6Tests_Additional.cs(268,0): at Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_Should_Return_Only_Non_Separated_Samples_SuccessGuard()
[xUnit.net 00:00:45.38]         --- End of stack trace from previous location ---
[xUnit.net 00:00:45.55]     Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_With_All_Separated_Should_Return_Empty_EdgeGuard [FAIL]
[xUnit.net 00:00:45.55]       Expected pending to be empty, but found 
[xUnit.net 00:00:45.55]       {
[xUnit.net 00:00:45.55]           Open_lab.Models.SampleCollection
[xUnit.net 00:00:45.55]           {
[xUnit.net 00:00:45.55]               CollectedAt = <2026-04-30 16:03:41.6194493>, 
[xUnit.net 00:00:45.55]               CollectedBy = 1, 
[xUnit.net 00:00:45.55]               CollectedByUser = <null>, 
[xUnit.net 00:00:45.55]               IsExternalSample = False, 
[xUnit.net 00:00:45.55]               IsSeparated = True, 
[xUnit.net 00:00:45.55]               ReceivedBy = <null>, 
[xUnit.net 00:00:45.55]               ReceivedByUser = <null>, 
[xUnit.net 00:00:45.55]               SampleId = 1, 
[xUnit.net 00:00:45.55]               Status = "مفصولة - Centrifuge", 
[xUnit.net 00:00:45.55]               VisitTest = Open_lab.Models.VisitTest
[xUnit.net 00:00:45.55]               {
[xUnit.net 00:00:45.55]                   ExternalQueueItem = <null>, 
[xUnit.net 00:00:45.55]                   Price = 10M, 
[xUnit.net 00:00:45.55]                   ResultValues = {empty}, 
[xUnit.net 00:00:45.55]                   SampleCollection = {Cyclic reference to type Open_lab.Models.SampleCollection detected}, 
[xUnit.net 00:00:45.55]                   Status = <null>, 
[xUnit.net 00:00:45.55]                   Test = Open_lab.Models.Test
[xUnit.net 00:00:45.55]                   {
[xUnit.net 00:00:45.55]                       Code = "T-000d97fe", 
[xUnit.net 00:00:45.55]                       Comments = {empty}, 
[xUnit.net 00:00:45.55]                       CostPrice = <null>, 
[xUnit.net 00:00:45.55]                       CustomGroupItems = {empty}, 
[xUnit.net 00:00:45.55]                       Group = <null>, 
[xUnit.net 00:00:45.55]                       GroupId = <null>, 
[xUnit.net 00:00:45.55]                       IsRoutine = False, 
[xUnit.net 00:00:45.55]                       IsSendOut = False, 
[xUnit.net 00:00:45.55]                       NameReceipt = "", 
[xUnit.net 00:00:45.55]                       NameReport = "Test", 
[xUnit.net 00:00:45.55]                       Parameters = {empty}, 
[xUnit.net 00:00:45.55]                       PatientPrice = <null>, 
[xUnit.net 00:00:45.55]                       Price = 10M, 
[xUnit.net 00:00:45.55]                       PriceListItems = {empty}, 
[xUnit.net 00:00:45.55]                       ReferenceRanges = {empty}, 
[xUnit.net 00:00:45.55]                       ReportOrder = 0, 
[xUnit.net 00:00:45.55]                       SampleType = <null>, 
[xUnit.net 00:00:45.55]                       SampleTypeId = <null>, 
[xUnit.net 00:00:45.55]                       TestId = 1, 
[xUnit.net 00:00:45.55]                       TurnaroundHours = 0, 
[xUnit.net 00:00:45.55]                       Unit = <null>, 
[xUnit.net 00:00:45.55]                       UnitId = <null>, 
[xUnit.net 00:00:45.55]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:00:45.55]                   }, 
[xUnit.net 00:00:45.55]                   TestId = 1, 
[xUnit.net 00:00:45.55]                   Visit = Open_lab.Models.Visit
[xUnit.net 00:00:45.55]                   {
[xUnit.net 00:00:45.55]                       AccountType = <null>, 
[xUnit.net 00:00:45.55]                       Branch = <null>, 
[xUnit.net 00:00:45.55]                       BranchId = <null>, 
[xUnit.net 00:00:45.55]                       Invoice = <null>, 
[xUnit.net 00:00:45.55]                       Patient = Open_lab.Models.Patient
[xUnit.net 00:00:45.55]                       {
[xUnit.net 00:00:45.55]                           Address = <null>, 
[xUnit.net 00:00:45.55]                           Age = <null>, 
[xUnit.net 00:00:45.55]                           BirthDate = <null>, 
[xUnit.net 00:00:45.55]                           FullName = "All Separated", 
[xUnit.net 00:00:45.55]                           Gender = "Male", 
[xUnit.net 00:00:45.55]                           IsPregnant = False, 
[xUnit.net 00:00:45.55]                           LabId = "L-ALLSEP", 
[xUnit.net 00:00:45.55]                           MedicalHistory = <null>, 
[xUnit.net 00:00:45.55]                           PatientId = 1, 
[xUnit.net 00:00:45.55]                           Phone = <null>, 
[xUnit.net 00:00:45.55]                           Visits = {{Cyclic reference to type Open_lab.Models.Visit detected}}
[xUnit.net 00:00:45.55]                       }, 
[xUnit.net 00:00:45.55]                       PatientId = 1, 
[xUnit.net 00:00:45.55]                       Physician = <null>, 
[xUnit.net 00:00:45.55]                       PhysicianId = <null>, 
[xUnit.net 00:00:45.55]                       Referral = <null>, 
[xUnit.net 00:00:45.55]                       ReferralId = <null>, 
[xUnit.net 00:00:45.55]                       Status = <null>, 
[xUnit.net 00:00:45.55]                       VisitDate = <2026-04-30 16:03:41.6188102>, 
[xUnit.net 00:00:45.55]                       VisitId = 1, 
[xUnit.net 00:00:45.55]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:00:45.55]                   }, 
[xUnit.net 00:00:45.55]                   VisitId = 1, 
[xUnit.net 00:00:45.55]                   VisitTestId = 1
[xUnit.net 00:00:45.55]               }, 
[xUnit.net 00:00:45.55]               VisitTestId = 1
[xUnit.net 00:00:45.55]           }
[xUnit.net 00:00:45.55]       }.
[xUnit.net 00:00:45.55]       Stack Trace:
[xUnit.net 00:00:45.55]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:00:45.55]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:00:45.55]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:00:45.55]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:00:45.55]            at FluentAssertions.Execution.GivenSelector`1.FailWith(String message, Object[] args)
[xUnit.net 00:00:45.55]            at FluentAssertions.Collections.GenericCollectionAssertions`3.BeEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:00:45.55]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module6Tests_Additional.cs(287,0): at Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_With_All_Separated_Should_Return_Empty_EdgeGuard()
[xUnit.net 00:00:45.55]         --- End of stack trace from previous location ---
[xUnit.net 00:01:03.78]     Open_lab.Tests.Module7ViewModelTests_Additional.GroupWorksheet_LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:01:03.78]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:01:03.78]       Stack Trace:
[xUnit.net 00:01:03.78]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:03.78]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:03.78]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:03.78]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:03.78]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:03.78]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module7Tests_Additional.cs(789,0): at Open_lab.Tests.Module7ViewModelTests_Additional.GroupWorksheet_LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:01:03.78]         --- End of stack trace from previous location ---
[xUnit.net 00:01:04.43]     Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_With_Specific_Date_Range_Should_Pass_Range_To_Service_SuccessGuard [FAIL]
[xUnit.net 00:01:04.43]       Moq.MockException : 
[xUnit.net 00:01:04.43]       Expected invocation on the mock once, but was 0 times: x => x.GetWorksheetByPatientAsync(23/04/2026 12:00:00 ص, 30/04/2026 12:00:00 ص)
[xUnit.net 00:01:04.43]       
[xUnit.net 00:01:04.43]       Performed invocations:
[xUnit.net 00:01:04.43]       
[xUnit.net 00:01:04.43]          Mock<IWorksheetService:15> (x):
[xUnit.net 00:01:04.43]       
[xUnit.net 00:01:04.44]             IWorksheetService.GetWorksheetByPatientAsync(23/04/2026 12:00:00 ص, 30/04/2026 11:59:59 م)
[xUnit.net 00:01:04.44]       
[xUnit.net 00:01:04.44]       Stack Trace:
[xUnit.net 00:01:04.44]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:01:04.44]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:01:04.44]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module7Tests_Additional.cs(516,0): at Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_With_Specific_Date_Range_Should_Pass_Range_To_Service_SuccessGuard()
[xUnit.net 00:01:04.44]         --- End of stack trace from previous location ---
[xUnit.net 00:01:05.21]     Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:01:05.21]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:01:05.21]       Stack Trace:
[xUnit.net 00:01:05.21]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:05.21]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:05.21]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:05.21]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:05.21]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:05.21]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module7Tests_Additional.cs(766,0): at Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:01:05.21]         --- End of stack trace from previous location ---
[xUnit.net 00:01:06.44]     Open_lab.Tests.Module4ServiceTests_Additional.GetResultsForVisitTestAsync_Should_Return_All_Results_Ordered_By_Parameter_SuccessGuard [FAIL]
[xUnit.net 00:01:06.44]       Expected results[0].ParameterId to be 60, but found 62 (difference of 2).
[xUnit.net 00:01:06.44]       Stack Trace:
[xUnit.net 00:01:06.44]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:06.44]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:06.44]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:06.44]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:06.44]            at FluentAssertions.Numeric.NumericAssertions`2.Be(T expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:06.44]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(471,0): at Open_lab.Tests.Module4ServiceTests_Additional.GetResultsForVisitTestAsync_Should_Return_All_Results_Ordered_By_Parameter_SuccessGuard()
[xUnit.net 00:01:06.44]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.84]     Open_lab.Tests.Module5ViewModelTests_Additional.SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard [FAIL]
[xUnit.net 00:01:07.84]       Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "لا توجد نتائج".
[xUnit.net 00:01:07.84]       Stack Trace:
[xUnit.net 00:01:07.84]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:07.84]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:07.84]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:07.84]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:07.84]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:07.84]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(732,0): at Open_lab.Tests.Module5ViewModelTests_Additional.SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard()
[xUnit.net 00:01:07.84]         --- End of stack trace from previous location ---
[xUnit.net 00:01:07.88]     Open_lab.Tests.Module5ViewModelTests_Additional.AddCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard [FAIL]
[xUnit.net 00:01:07.88]       Moq.MockException : 
[xUnit.net 00:01:07.88]       Expected invocation on the mock once, but was 0 times: s => s.CreateCultureAsync(It.Is<Culture>(c => c.ColonyCount == 0))
[xUnit.net 00:01:07.88]       
[xUnit.net 00:01:07.88]       Performed invocations:
[xUnit.net 00:01:07.88]       
[xUnit.net 00:01:07.88]          Mock<ICultureSensitivityService:16> (s):
[xUnit.net 00:01:07.88]       
[xUnit.net 00:01:07.88]             ICultureSensitivityService.GetCulturesAsync()
[xUnit.net 00:01:07.88]             ICultureSensitivityService.GetAntibioticsAsync()
[xUnit.net 00:01:07.88]       
[xUnit.net 00:01:07.88]       Stack Trace:
[xUnit.net 00:01:07.88]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:01:07.88]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:01:07.88]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(665,0): at Open_lab.Tests.Module5ViewModelTests_Additional.AddCultureAsync_With_Zero_ColonyCount_Should_Succeed_EdgeGuard()
[xUnit.net 00:01:07.88]         --- End of stack trace from previous location ---
[xUnit.net 00:01:13.62]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+PriceListsViewModel_AdditionalTests.PrintListAsync_When_NoItems_Should_Not_Print_EdgeGuard [FAIL]
[xUnit.net 00:01:13.62]       Moq.MockException : 
[xUnit.net 00:01:13.62]       Expected invocation on the mock should never have been performed, but was 1 times: x => x.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>())
[xUnit.net 00:01:13.62]       
[xUnit.net 00:01:13.62]       Performed invocations:
[xUnit.net 00:01:13.62]       
[xUnit.net 00:01:13.62]          Mock<IPrintService:111> (x):
[xUnit.net 00:01:13.62]       
[xUnit.net 00:01:13.62]             IPrintService.PrintTextReportAsync("قائمة الأسعار", ObservableCollection<string>, "PriceList_2")
[xUnit.net 00:01:13.62]       
[xUnit.net 00:01:13.62]       Stack Trace:
[xUnit.net 00:01:13.62]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:01:13.62]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:01:13.62]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(731,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.PriceListsViewModel_AdditionalTests.PrintListAsync_When_NoItems_Should_Not_Print_EdgeGuard()
[xUnit.net 00:01:13.62]         --- End of stack trace from previous location ---
[xUnit.net 00:01:23.83]     Open_lab.Tests.Module4ViewModelTests_Additional.PrintBlankAsync_Should_Call_PrintService_SuccessGuard [FAIL]
[xUnit.net 00:01:23.83]       Expected viewModel.StatusMessage "تم إرسال التقرير الفارغ للطباعة." to contain "تم إرسال النموذج".
[xUnit.net 00:01:23.83]       Stack Trace:
[xUnit.net 00:01:23.83]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:23.83]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:23.83]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:23.83]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:23.83]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:23.83]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(717,0): at Open_lab.Tests.Module4ViewModelTests_Additional.PrintBlankAsync_Should_Call_PrintService_SuccessGuard()
[xUnit.net 00:01:23.83]         --- End of stack trace from previous location ---
[xUnit.net 00:01:24.01]     Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_Multiple_Results_Should_Group_By_Visit_EdgeGuard [FAIL]
[xUnit.net 00:01:24.01]       Expected viewModel.HistoryResults to contain 3 item(s), but found 0: {empty}.
[xUnit.net 00:01:24.01]       Stack Trace:
[xUnit.net 00:01:24.01]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:24.01]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:24.01]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:24.01]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:24.01]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:24.01]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(768,0): at Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_Multiple_Results_Should_Group_By_Visit_EdgeGuard()
[xUnit.net 00:01:24.01]         --- End of stack trace from previous location ---
[xUnit.net 00:01:24.02]     Open_lab.Tests.Module4ViewModelTests_Additional.ReopenResultsAsync_With_Verified_Status_Should_Call_Service_SuccessGuard [FAIL]
[xUnit.net 00:01:24.02]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تم فتح التحليل".
[xUnit.net 00:01:24.02]       Stack Trace:
[xUnit.net 00:01:24.02]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:24.02]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:24.02]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:24.02]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:24.02]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:24.02]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(646,0): at Open_lab.Tests.Module4ViewModelTests_Additional.ReopenResultsAsync_With_Verified_Status_Should_Call_Service_SuccessGuard()
[xUnit.net 00:01:24.02]         --- End of stack trace from previous location ---
[xUnit.net 00:01:24.10]     Open_lab.Tests.Module4ViewModelTests_Additional.LoadAsync_With_Null_Referral_Should_Handle_EdgeGuard [FAIL]
[xUnit.net 00:01:24.10]       Expected viewModel.ReferralName to be empty, but found "—".
[xUnit.net 00:01:24.10]       Stack Trace:
[xUnit.net 00:01:24.10]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:24.10]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:24.10]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:24.10]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:24.10]            at FluentAssertions.Primitives.StringAssertions`1.BeEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:24.10]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(731,0): at Open_lab.Tests.Module4ViewModelTests_Additional.LoadAsync_With_Null_Referral_Should_Handle_EdgeGuard()
[xUnit.net 00:01:24.10]         --- End of stack trace from previous location ---
[xUnit.net 00:01:24.35]     Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_No_History_Should_Show_Empty_State_EdgeGuard [FAIL]
[xUnit.net 00:01:24.35]       Expected viewModel.StatusMessage "تم تحميل 0 نتيجة من 0 زيارات سابقة." to contain "لا توجد".
[xUnit.net 00:01:24.35]       Stack Trace:
[xUnit.net 00:01:24.35]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:24.35]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:24.35]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:24.35]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:24.35]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:24.35]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(797,0): at Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_No_History_Should_Show_Empty_State_EdgeGuard()
[xUnit.net 00:01:24.35]         --- End of stack trace from previous location ---
[xUnit.net 00:01:49.79]     Open_lab.Tests.Services.UserAdminServiceTests.EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation [FAIL]
[xUnit.net 00:01:49.79]       Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.
[xUnit.net 00:01:49.80]       Stack Trace:
[xUnit.net 00:01:49.80]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:49.80]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:49.80]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:49.80]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:49.80]            at FluentAssertions.Specialized.DelegateAssertionsBase`2.ThrowInternal[TException](Exception exception, String because, Object[] becauseArgs)
[xUnit.net 00:01:49.80]            at FluentAssertions.Specialized.AsyncFunctionAssertions`2.ThrowAsync[TException](String because, Object[] becauseArgs)
[xUnit.net 00:01:49.80]            at FluentAssertions.ExceptionAssertionsExtensions.WithMessage[TException](Task`1 task, String expectedWildcardPattern, String because, Object[] becauseArgs)
[xUnit.net 00:01:49.80]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\UserAdminServiceTests.cs(349,0): at Open_lab.Tests.Services.UserAdminServiceTests.EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation()
[xUnit.net 00:01:49.80]         --- End of stack trace from previous location ---
[xUnit.net 00:01:51.19]   Finished:    Open_lab.Tests
========== Test run finished: 1322 Tests (1295 Passed, 27 Failed, 0 Skipped) run in 1.9 min ==========
