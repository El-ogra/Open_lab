========== Starting test discovery ==========
[xUnit.net 00:00:00.01] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:13.77]   Discovering: Open_lab.Tests
[xUnit.net 00:00:18.26]   Discovered:  Open_lab.Tests
========== Test discovery finished: 1321 Tests found in 1.5 min ==========
========== Starting test run ==========
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.26)
[xUnit.net 00:00:01.44]   Starting:    Open_lab.Tests
[xUnit.net 00:00:52.20]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+PriceListsViewModel_AdditionalTests.PrintListAsync_When_NoItems_Should_Not_Print_EdgeGuard [FAIL]
[xUnit.net 00:00:52.20]       Moq.MockException : 
[xUnit.net 00:00:52.20]       Expected invocation on the mock should never have been performed, but was 1 times: x => x.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<ObservableCollection<string>>(), It.IsAny<string>())
[xUnit.net 00:00:52.20]       
[xUnit.net 00:00:52.20]       Performed invocations:
[xUnit.net 00:00:52.20]       
[xUnit.net 00:00:52.20]          Mock<IPrintService:46> (x):
[xUnit.net 00:00:52.20]       
[xUnit.net 00:00:52.20]             IPrintService.PrintTextReportAsync("قائمة الأسعار", ObservableCollection<string>, "PriceList_2")
[xUnit.net 00:00:52.20]       
[xUnit.net 00:00:52.20]       Stack Trace:
[xUnit.net 00:00:52.20]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:00:52.20]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:00:52.20]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(731,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.PriceListsViewModel_AdditionalTests.PrintListAsync_When_NoItems_Should_Not_Print_EdgeGuard()
[xUnit.net 00:00:52.20]         --- End of stack trace from previous location ---
[xUnit.net 00:01:30.86]     Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_Should_Return_Only_Non_Separated_Samples_SuccessGuard [FAIL]
[xUnit.net 00:01:30.86]       Expected pending to contain 2 item(s), but found 3: Open_lab.Models.SampleCollection
[xUnit.net 00:01:30.86]           {
[xUnit.net 00:01:30.86]               CollectedAt = <2026-04-30 16:30:27.1241633>, 
[xUnit.net 00:01:30.86]               CollectedBy = 1, 
[xUnit.net 00:01:30.86]               CollectedByUser = <null>, 
[xUnit.net 00:01:30.86]               IsExternalSample = False, 
[xUnit.net 00:01:30.86]               IsSeparated = False, 
[xUnit.net 00:01:30.86]               ReceivedBy = <null>, 
[xUnit.net 00:01:30.86]               ReceivedByUser = <null>, 
[xUnit.net 00:01:30.86]               SampleId = 3, 
[xUnit.net 00:01:30.86]               Status = "مسحوبة", 
[xUnit.net 00:01:30.86]               VisitTest = Open_lab.Models.VisitTest
[xUnit.net 00:01:30.86]               {
[xUnit.net 00:01:30.86]                   ExternalQueueItem = <null>, 
[xUnit.net 00:01:30.86]                   Price = 10M, 
[xUnit.net 00:01:30.86]                   ResultValues = {empty}, 
[xUnit.net 00:01:30.86]                   SampleCollection = {Cyclic reference to type Open_lab.Models.SampleCollection detected}, 
[xUnit.net 00:01:30.86]                   Status = <null>, 
[xUnit.net 00:01:30.86]                   Test = Open_lab.Models.Test
[xUnit.net 00:01:30.86]                   {
[xUnit.net 00:01:30.86]                       Code = "T-ec1a854c", 
[xUnit.net 00:01:30.87]                       Comments = {empty}, 
[xUnit.net 00:01:30.87]                       CostPrice = <null>, 
[xUnit.net 00:01:30.87]                       CustomGroupItems = {empty}, 
[xUnit.net 00:01:30.87]                       Group = <null>, 
[xUnit.net 00:01:30.87]                       GroupId = <null>, 
[xUnit.net 00:01:30.87]                       IsRoutine = False, 
[xUnit.net 00:01:30.87]                       IsSendOut = False, 
[xUnit.net 00:01:30.87]                       NameReceipt = "", 
[xUnit.net 00:01:30.87]                       NameReport = "Test", 
[xUnit.net 00:01:30.87]                       Parameters = {empty}, 
[xUnit.net 00:01:30.87]                       PatientPrice = <null>, 
[xUnit.net 00:01:30.87]                       Price = 10M, 
[xUnit.net 00:01:30.87]                       PriceListItems = {empty}, 
[xUnit.net 00:01:30.87]                       ReferenceRanges = {empty}, 
[xUnit.net 00:01:30.87]                       ReportOrder = 0, 
[xUnit.net 00:01:30.87]                       SampleType = <null>, 
[xUnit.net 00:01:30.87]                       SampleTypeId = <null>, 
[xUnit.net 00:01:30.87]                       TestId = 3, 
[xUnit.net 00:01:30.87]                       TurnaroundHours = 0, 
[xUnit.net 00:01:30.87]                       Unit = <null>, 
[xUnit.net 00:01:30.87]                       UnitId = <null>, 
[xUnit.net 00:01:30.87]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:01:30.87]                   }, 
[xUnit.net 00:01:30.87]                   TestId = 3, 
[xUnit.net 00:01:30.87]                   Visit = Open_lab.Models.Visit
[xUnit.net 00:01:30.87]                   {
[xUnit.net 00:01:30.87]                       AccountType = <null>, 
[xUnit.net 00:01:30.87]                       Branch = <null>, 
[xUnit.net 00:01:30.87]                       BranchId = <null>, 
[xUnit.net 00:01:30.87]                       Invoice = <null>, 
[xUnit.net 00:01:30.87]                       Patient = Open_lab.Models.Patient
[xUnit.net 00:01:30.87]                       {
[xUnit.net 00:01:30.87]                           Address = <null>, 
[xUnit.net 00:01:30.87]                           Age = <null>, 
[xUnit.net 00:01:30.87]                           BirthDate = <null>, 
[xUnit.net 00:01:30.87]                           FullName = "Pending 3", 
[xUnit.net 00:01:30.87]                           Gender = "Male", 
[xUnit.net 00:01:30.87]                           IsPregnant = False, 
[xUnit.net 00:01:30.87]                           LabId = "L-PND3", 
[xUnit.net 00:01:30.87]                           MedicalHistory = <null>, 
[xUnit.net 00:01:30.87]                           PatientId = 3, 
[xUnit.net 00:01:30.87]                           Phone = <null>, 
[xUnit.net 00:01:30.87]                           Visits = {{Cyclic reference to type Open_lab.Models.Visit detected}}
[xUnit.net 00:01:30.87]                       }, 
[xUnit.net 00:01:30.87]                       PatientId = 3, 
[xUnit.net 00:01:30.87]                       Physician = <null>, 
[xUnit.net 00:01:30.87]                       PhysicianId = <null>, 
[xUnit.net 00:01:30.87]                       Referral = <null>, 
[xUnit.net 00:01:30.87]                       ReferralId = <null>, 
[xUnit.net 00:01:30.87]                       Status = <null>, 
[xUnit.net 00:01:30.87]                       VisitDate = <2026-04-30 16:30:27.1222468>, 
[xUnit.net 00:01:30.87]                       VisitId = 3, 
[xUnit.net 00:01:30.87]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:01:30.87]                   }, 
[xUnit.net 00:01:30.87]                   VisitId = 3, 
[xUnit.net 00:01:30.87]                   VisitTestId = 3
[xUnit.net 00:01:30.87]               }, 
[xUnit.net 00:01:30.87]               VisitTestId = 3
[xUnit.net 00:01:30.87]           }Open_lab.Models.SampleCollection
[xUnit.net 00:01:30.87]           {
[xUnit.net 00:01:30.87]               CollectedAt = <2026-04-30 16:30:27.1232218>, 
[xUnit.net 00:01:30.87]               CollectedBy = 1, 
[xUnit.net 00:01:30.87]               CollectedByUser = <null>, 
[xUnit.net 00:01:30.88]               IsExternalSample = False, 
[xUnit.net 00:01:30.88]               IsSeparated = False, 
[xUnit.net 00:01:30.88]               ReceivedBy = <null>, 
[xUnit.net 00:01:30.88]               ReceivedByUser = <null>, 
[xUnit.net 00:01:30.88]               SampleId = 2, 
[xUnit.net 00:01:30.88]               Status = "مسحوبة", 
[xUnit.net 00:01:30.88]               VisitTest = Open_lab.Models.VisitTest
[xUnit.net 00:01:30.88]               {
[xUnit.net 00:01:30.88]                   ExternalQueueItem = <null>, 
[xUnit.net 00:01:30.88]                   Price = 10M, 
[xUnit.net 00:01:30.88]                   ResultValues = {empty}, 
[xUnit.net 00:01:30.88]                   SampleCollection = {Cyclic reference to type Open_lab.Models.SampleCollection detected}, 
[xUnit.net 00:01:30.88]                   Status = <null>, 
[xUnit.net 00:01:30.88]                   Test = Open_lab.Models.Test
[xUnit.net 00:01:30.88]                   {
[xUnit.net 00:01:30.88]                       Code = "T-6ad727ec", 
[xUnit.net 00:01:30.88]       
[xUnit.net 00:01:30.88]       (Output has exceeded the maximum of 100 lines. Increase FormattingOptions.MaxLines on AssertionScope or AssertionOptions to include more lines.)
[xUnit.net 00:01:30.88]                       Comments = {empty}, .
[xUnit.net 00:01:30.88]       Stack Trace:
[xUnit.net 00:01:30.88]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:30.88]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:30.88]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:30.88]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:30.88]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:30.88]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module6Tests_Additional.cs(268,0): at Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_Should_Return_Only_Non_Separated_Samples_SuccessGuard()
[xUnit.net 00:01:30.88]         --- End of stack trace from previous location ---
[xUnit.net 00:01:31.02]     Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_With_All_Separated_Should_Return_Empty_EdgeGuard [FAIL]
[xUnit.net 00:01:31.02]       Expected pending to be empty, but found 
[xUnit.net 00:01:31.02]       {
[xUnit.net 00:01:31.02]           Open_lab.Models.SampleCollection
[xUnit.net 00:01:31.02]           {
[xUnit.net 00:01:31.02]               CollectedAt = <2026-04-30 16:30:27.5338908>, 
[xUnit.net 00:01:31.02]               CollectedBy = 1, 
[xUnit.net 00:01:31.02]               CollectedByUser = <null>, 
[xUnit.net 00:01:31.02]               IsExternalSample = False, 
[xUnit.net 00:01:31.02]               IsSeparated = True, 
[xUnit.net 00:01:31.02]               ReceivedBy = <null>, 
[xUnit.net 00:01:31.02]               ReceivedByUser = <null>, 
[xUnit.net 00:01:31.02]               SampleId = 1, 
[xUnit.net 00:01:31.02]               Status = "مفصولة - Centrifuge", 
[xUnit.net 00:01:31.02]               VisitTest = Open_lab.Models.VisitTest
[xUnit.net 00:01:31.02]               {
[xUnit.net 00:01:31.02]                   ExternalQueueItem = <null>, 
[xUnit.net 00:01:31.02]                   Price = 10M, 
[xUnit.net 00:01:31.02]                   ResultValues = {empty}, 
[xUnit.net 00:01:31.02]                   SampleCollection = {Cyclic reference to type Open_lab.Models.SampleCollection detected}, 
[xUnit.net 00:01:31.02]                   Status = <null>, 
[xUnit.net 00:01:31.02]                   Test = Open_lab.Models.Test
[xUnit.net 00:01:31.02]                   {
[xUnit.net 00:01:31.02]                       Code = "T-1e22e4c3", 
[xUnit.net 00:01:31.02]                       Comments = {empty}, 
[xUnit.net 00:01:31.02]                       CostPrice = <null>, 
[xUnit.net 00:01:31.02]                       CustomGroupItems = {empty}, 
[xUnit.net 00:01:31.02]                       Group = <null>, 
[xUnit.net 00:01:31.02]                       GroupId = <null>, 
[xUnit.net 00:01:31.02]                       IsRoutine = False, 
[xUnit.net 00:01:31.02]                       IsSendOut = False, 
[xUnit.net 00:01:31.02]                       NameReceipt = "", 
[xUnit.net 00:01:31.02]                       NameReport = "Test", 
[xUnit.net 00:01:31.02]                       Parameters = {empty}, 
[xUnit.net 00:01:31.02]                       PatientPrice = <null>, 
[xUnit.net 00:01:31.02]                       Price = 10M, 
[xUnit.net 00:01:31.02]                       PriceListItems = {empty}, 
[xUnit.net 00:01:31.02]                       ReferenceRanges = {empty}, 
[xUnit.net 00:01:31.02]                       ReportOrder = 0, 
[xUnit.net 00:01:31.02]                       SampleType = <null>, 
[xUnit.net 00:01:31.02]                       SampleTypeId = <null>, 
[xUnit.net 00:01:31.02]                       TestId = 1, 
[xUnit.net 00:01:31.02]                       TurnaroundHours = 0, 
[xUnit.net 00:01:31.02]                       Unit = <null>, 
[xUnit.net 00:01:31.02]                       UnitId = <null>, 
[xUnit.net 00:01:31.02]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:01:31.02]                   }, 
[xUnit.net 00:01:31.02]                   TestId = 1, 
[xUnit.net 00:01:31.02]                   Visit = Open_lab.Models.Visit
[xUnit.net 00:01:31.02]                   {
[xUnit.net 00:01:31.03]                       AccountType = <null>, 
[xUnit.net 00:01:31.03]                       Branch = <null>, 
[xUnit.net 00:01:31.03]                       BranchId = <null>, 
[xUnit.net 00:01:31.03]                       Invoice = <null>, 
[xUnit.net 00:01:31.03]                       Patient = Open_lab.Models.Patient
[xUnit.net 00:01:31.03]                       {
[xUnit.net 00:01:31.03]                           Address = <null>, 
[xUnit.net 00:01:31.03]                           Age = <null>, 
[xUnit.net 00:01:31.03]                           BirthDate = <null>, 
[xUnit.net 00:01:31.03]                           FullName = "All Separated", 
[xUnit.net 00:01:31.03]                           Gender = "Male", 
[xUnit.net 00:01:31.03]                           IsPregnant = False, 
[xUnit.net 00:01:31.03]                           LabId = "L-ALLSEP", 
[xUnit.net 00:01:31.03]                           MedicalHistory = <null>, 
[xUnit.net 00:01:31.03]                           PatientId = 1, 
[xUnit.net 00:01:31.03]                           Phone = <null>, 
[xUnit.net 00:01:31.03]                           Visits = {{Cyclic reference to type Open_lab.Models.Visit detected}}
[xUnit.net 00:01:31.03]                       }, 
[xUnit.net 00:01:31.03]                       PatientId = 1, 
[xUnit.net 00:01:31.03]                       Physician = <null>, 
[xUnit.net 00:01:31.03]                       PhysicianId = <null>, 
[xUnit.net 00:01:31.03]                       Referral = <null>, 
[xUnit.net 00:01:31.03]                       ReferralId = <null>, 
[xUnit.net 00:01:31.03]                       Status = <null>, 
[xUnit.net 00:01:31.03]                       VisitDate = <2026-04-30 16:30:27.5327318>, 
[xUnit.net 00:01:31.03]                       VisitId = 1, 
[xUnit.net 00:01:31.03]                       VisitTests = {{Cyclic reference to type Open_lab.Models.VisitTest detected}}
[xUnit.net 00:01:31.03]                   }, 
[xUnit.net 00:01:31.03]                   VisitId = 1, 
[xUnit.net 00:01:31.03]                   VisitTestId = 1
[xUnit.net 00:01:31.03]               }, 
[xUnit.net 00:01:31.03]               VisitTestId = 1
[xUnit.net 00:01:31.03]           }
[xUnit.net 00:01:31.03]       }.
[xUnit.net 00:01:31.03]       Stack Trace:
[xUnit.net 00:01:31.03]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:31.03]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:31.03]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:31.03]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:31.03]            at FluentAssertions.Execution.GivenSelector`1.FailWith(String message, Object[] args)
[xUnit.net 00:01:31.03]            at FluentAssertions.Collections.GenericCollectionAssertions`3.BeEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:01:31.03]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module6Tests_Additional.cs(287,0): at Open_lab.Tests.Module6ServiceTests_Additional.GetPendingTrackingSamplesAsync_With_All_Separated_Should_Return_Empty_EdgeGuard()
[xUnit.net 00:01:31.03]         --- End of stack trace from previous location ---
[xUnit.net 00:01:31.18]     Open_lab.Tests.Services.UserAdminServiceTests.EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation [FAIL]
[xUnit.net 00:01:31.18]       Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.
[xUnit.net 00:01:31.18]       Stack Trace:
[xUnit.net 00:01:31.18]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:31.18]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:31.18]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:31.18]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:31.18]            at FluentAssertions.Specialized.DelegateAssertionsBase`2.ThrowInternal[TException](Exception exception, String because, Object[] becauseArgs)
[xUnit.net 00:01:31.18]            at FluentAssertions.Specialized.AsyncFunctionAssertions`2.ThrowAsync[TException](String because, Object[] becauseArgs)
[xUnit.net 00:01:31.18]            at FluentAssertions.ExceptionAssertionsExtensions.WithMessage[TException](Task`1 task, String expectedWildcardPattern, String because, Object[] becauseArgs)
[xUnit.net 00:01:31.18]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\UserAdminServiceTests.cs(349,0): at Open_lab.Tests.Services.UserAdminServiceTests.EditUserData_WhenChangingAdminUsername_ShouldThrowInvalidOperation()
[xUnit.net 00:01:31.18]         --- End of stack trace from previous location ---
[xUnit.net 00:01:35.99]     Open_lab.Tests.Module5ViewModelTests_Additional.SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard [FAIL]
[xUnit.net 00:01:35.99]       Expected _viewModel.StatusMessage "تم تحميل 0 طلب مزرعة." to contain "لا توجد نتائج".
[xUnit.net 00:01:35.99]       Stack Trace:
[xUnit.net 00:01:35.99]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:35.99]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:35.99]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:35.99]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:35.99]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:35.99]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module5Tests_Additional.cs(739,0): at Open_lab.Tests.Module5ViewModelTests_Additional.SaveResultAsync_With_Empty_ResultRows_Should_Set_Warning_EdgeGuard()
[xUnit.net 00:01:35.99]         --- End of stack trace from previous location ---
[xUnit.net 00:01:59.21]     Open_lab.Tests.Module4ViewModelTests_Additional.PrintBlankAsync_Should_Call_PrintService_SuccessGuard [FAIL]
[xUnit.net 00:01:59.21]       Expected viewModel.StatusMessage "تم إرسال التقرير الفارغ للطباعة." to contain "تم إرسال النموذج".
[xUnit.net 00:01:59.21]       Stack Trace:
[xUnit.net 00:01:59.21]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:59.21]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:59.21]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:59.21]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:59.21]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:59.21]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(718,0): at Open_lab.Tests.Module4ViewModelTests_Additional.PrintBlankAsync_Should_Call_PrintService_SuccessGuard()
[xUnit.net 00:01:59.21]         --- End of stack trace from previous location ---
[xUnit.net 00:01:59.81]     Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_Multiple_Results_Should_Group_By_Visit_EdgeGuard [FAIL]
[xUnit.net 00:01:59.81]       Expected viewModel.HistoryResults to contain 3 item(s), but found 0: {empty}.
[xUnit.net 00:01:59.81]       Stack Trace:
[xUnit.net 00:01:59.82]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:59.82]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:59.82]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:59.82]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:59.82]            at FluentAssertions.Collections.GenericCollectionAssertions`3.HaveCount(Int32 expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:59.82]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(769,0): at Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_Multiple_Results_Should_Group_By_Visit_EdgeGuard()
[xUnit.net 00:01:59.82]         --- End of stack trace from previous location ---
[xUnit.net 00:01:59.84]     Open_lab.Tests.Module4ViewModelTests_Additional.ReopenResultsAsync_With_Verified_Status_Should_Call_Service_SuccessGuard [FAIL]
[xUnit.net 00:01:59.84]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تم فتح التحليل".
[xUnit.net 00:01:59.84]       Stack Trace:
[xUnit.net 00:01:59.84]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:01:59.84]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:01:59.84]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:01:59.84]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:01:59.84]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:01:59.84]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(647,0): at Open_lab.Tests.Module4ViewModelTests_Additional.ReopenResultsAsync_With_Verified_Status_Should_Call_Service_SuccessGuard()
[xUnit.net 00:01:59.84]         --- End of stack trace from previous location ---
[xUnit.net 00:02:00.11]     Open_lab.Tests.Module4ViewModelTests_Additional.LoadAsync_With_Null_Referral_Should_Handle_EdgeGuard [FAIL]
[xUnit.net 00:02:00.11]       Expected viewModel.ReferralName to be empty, but found "—".
[xUnit.net 00:02:00.11]       Stack Trace:
[xUnit.net 00:02:00.11]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:02:00.11]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:02:00.11]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:02:00.11]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:02:00.11]            at FluentAssertions.Primitives.StringAssertions`1.BeEmpty(String because, Object[] becauseArgs)
[xUnit.net 00:02:00.11]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(732,0): at Open_lab.Tests.Module4ViewModelTests_Additional.LoadAsync_With_Null_Referral_Should_Handle_EdgeGuard()
[xUnit.net 00:02:00.11]         --- End of stack trace from previous location ---
[xUnit.net 00:02:01.35]     Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_No_History_Should_Show_Empty_State_EdgeGuard [FAIL]
[xUnit.net 00:02:01.35]       Expected viewModel.StatusMessage "تم تحميل 0 نتيجة من 0 زيارات سابقة." to contain "لا توجد".
[xUnit.net 00:02:01.35]       Stack Trace:
[xUnit.net 00:02:01.35]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:02:01.35]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:02:01.35]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:02:01.35]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:02:01.35]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:02:01.35]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module4Tests_Additional.cs(798,0): at Open_lab.Tests.Module4ViewModelTests_Additional.LoadHistoryCommand_With_No_History_Should_Show_Empty_State_EdgeGuard()
[xUnit.net 00:02:01.35]         --- End of stack trace from previous location ---
[xUnit.net 00:02:25.25]     Open_lab.Tests.Module7ViewModelTests_Additional.GroupWorksheet_LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:02:25.25]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:02:25.25]       Stack Trace:
[xUnit.net 00:02:25.25]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:02:25.25]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:02:25.25]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:02:25.25]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:02:25.25]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:02:25.25]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module7Tests_Additional.cs(793,0): at Open_lab.Tests.Module7ViewModelTests_Additional.GroupWorksheet_LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:02:25.25]         --- End of stack trace from previous location ---
[xUnit.net 00:02:26.56]     Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:02:26.56]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:02:26.56]       Stack Trace:
[xUnit.net 00:02:26.56]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:02:26.56]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:02:26.56]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:02:26.56]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:02:26.56]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:02:26.56]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module7Tests_Additional.cs(770,0): at Open_lab.Tests.Module7ViewModelTests_Additional.LoadCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:02:26.56]         --- End of stack trace from previous location ---
[xUnit.net 00:02:26.62]     Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional+TestCommentsViewModel_AdditionalTests.SaveAsync_With_Empty_Comment_Text_Should_NotCall_Service_EdgeGuard [FAIL]
[xUnit.net 00:02:26.62]       Moq.MockException : 
[xUnit.net 00:02:26.62]       Expected invocation on the mock should never have been performed, but was 1 times: x => x.CreateTestCommentAsync(It.IsAny<TestComment>())
[xUnit.net 00:02:26.62]       
[xUnit.net 00:02:26.62]       Performed invocations:
[xUnit.net 00:02:26.62]       
[xUnit.net 00:02:26.62]          Mock<ITestCatalogService:204> (x):
[xUnit.net 00:02:26.62]       
[xUnit.net 00:02:26.62]             ITestCatalogService.GetAllTestsAsync()
[xUnit.net 00:02:26.62]             ITestCatalogService.GetTestCommentsAsync(1)
[xUnit.net 00:02:26.62]             ITestCatalogService.CreateTestCommentAsync(TestComment)
[xUnit.net 00:02:26.62]       
[xUnit.net 00:02:26.62]       Stack Trace:
[xUnit.net 00:02:26.62]         /_/src/Moq/Mock.cs(331,0): at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
[xUnit.net 00:02:26.62]         /_/src/Moq/Mock`1.cs(1033,0): at Moq.Mock`1.Verify[TResult](Expression`1 expression, Func`1 times)
[xUnit.net 00:02:26.62]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\ViewModels\Module3ViewModelTests_Additional.cs(859,0): at Open_lab.Tests.ViewModels.Module3ViewModelTests_Additional.TestCommentsViewModel_AdditionalTests.SaveAsync_With_Empty_Comment_Text_Should_NotCall_Service_EdgeGuard()
[xUnit.net 00:02:26.62]         --- End of stack trace from previous location ---
[xUnit.net 00:02:32.66]     Open_lab.Tests.Module8ViewModelTests_Additional.LoadSettlementCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:02:32.66]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:02:32.66]       Stack Trace:
[xUnit.net 00:02:32.66]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:02:32.66]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:02:32.66]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:02:32.66]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:02:32.66]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:02:32.66]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module8Tests_Additional.cs(758,0): at Open_lab.Tests.Module8ViewModelTests_Additional.LoadSettlementCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:02:32.66]         --- End of stack trace from previous location ---
[xUnit.net 00:02:32.95]     Open_lab.Tests.Module8ViewModelTests_Additional.LoadQueueCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard [FAIL]
[xUnit.net 00:02:32.95]       Expected viewModel.StatusMessage "خطأ: Object reference not set to an instance of an object." to contain "تسجيل الدخول".
[xUnit.net 00:02:32.95]       Stack Trace:
[xUnit.net 00:02:32.95]            at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
[xUnit.net 00:02:32.95]            at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
[xUnit.net 00:02:32.95]            at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
[xUnit.net 00:02:32.95]            at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
[xUnit.net 00:02:32.95]            at FluentAssertions.Primitives.StringAssertions`1.Contain(String expected, String because, Object[] becauseArgs)
[xUnit.net 00:02:32.95]         C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests\Services\Module8Tests_Additional.cs(729,0): at Open_lab.Tests.Module8ViewModelTests_Additional.LoadQueueCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
[xUnit.net 00:02:32.95]         --- End of stack trace from previous location ---
[xUnit.net 00:02:34.46]   Finished:    Open_lab.Tests
========== Test run finished: 1321 Tests (1306 Passed, 15 Failed, 0 Skipped) run in 2.6 min ==========
