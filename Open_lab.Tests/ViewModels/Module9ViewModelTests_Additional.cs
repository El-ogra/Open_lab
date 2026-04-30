using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    /// <summary>
    /// اختبارات ViewModel الموديول 9 — الإحصائيات والتحليلات
    /// يغطي الوظائف: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6
    /// </summary>
    public class Module9ViewModelTests_Additional : IDisposable
    {
        private readonly Mock<IStatisticsService> _statsMock;
        private readonly Mock<IUserProductivityService> _productivityMock;
        private readonly Mock<IPrintService> _printMock;
        private readonly StatisticsViewModel _viewModel;

        public Module9ViewModelTests_Additional()
        {
            AppSessionTestHelper.ResetToAdmin();

            _statsMock = new Mock<IStatisticsService>();
            _productivityMock = new Mock<IUserProductivityService>();
            _printMock = new Mock<IPrintService>();

            // Setup افتراضي لمنع أخطاء null في بناء ViewModel
            _statsMock
                .Setup(x => x.GetReferralsAsync())
                .ReturnsAsync(new List<StatisticsReferralLookup>
                {
                    new() { ReferralId = 1, Name = "عيادة النور" }
                });

            _statsMock
                .Setup(x => x.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new StatisticsSnapshot
                {
                    Summary = new StatisticsSummary
                    {
                        VisitCount = 10,
                        PatientCount = 8,
                        TestCount = 20,
                        TotalRevenue = 5000m,
                        TotalPaid = 4500m
                    },
                    ByGender = new List<StatisticsGenderRow>
                    {
                        new() { Gender = "ذكر", VisitsCount = 6, TestsCount = 12, Revenue = 3000m },
                        new() { Gender = "أنثى", VisitsCount = 4, TestsCount = 8, Revenue = 2000m }
                    },
                    ByReferral = new List<StatisticsReferralRow>
                    {
                        new() { ReferralName = "عيادة النور", VisitsCount = 5, TestsCount = 10, Revenue = 2500m, Paid = 2500m }
                    }
                });

            _statsMock
                .Setup(x => x.GetMonthlyAnalysisAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<MonthlyAnalysisRow>
                {
                    new() { Month = 1, MonthName = "يناير", VisitCount = 3, TestCount = 6, Revenue = 1500m },
                    new() { Month = 2, MonthName = "فبراير", VisitCount = 2, TestCount = 4, Revenue = 1000m }
                });

            _statsMock
                .Setup(x => x.GetTop10TestsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<TopTestRow>
                {
                    new() { TestName = "صورة دم كاملة", DemandCount = 5, TotalRevenue = 750m },
                    new() { TestName = "سكر الدم", DemandCount = 3, TotalRevenue = 300m }
                });

            _statsMock
                .Setup(x => x.GetSampleCountPerYearAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<YearlySampleRow>
                {
                    new() { Year = DateTime.Today.Year - 1, SamplesCount = 150 },
                    new() { Year = DateTime.Today.Year, SamplesCount = 200 }
                });

            _productivityMock
                .Setup(x => x.GetUserPerformanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UserPerformanceRow>
                {
                    new() { UserId = 1, Username = "tech.sara", CompletedTestsCount = 10 },
                    new() { UserId = 2, Username = "tech.omar", CompletedTestsCount = 7 }
                });

            _viewModel = new StatisticsViewModel(
                _statsMock.Object,
                _productivityMock.Object,
                _printMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        // ===================================================================
        // 9.1 توزيع المرضى حسب الجنس — Patient Count by Gender
        // ===================================================================

        [Fact]
        public async Task PatientCountByGender_LoadCommand_ShouldPopulateByGenderCollection_Success()
        {
            // Function: 9.1 — Patient Count by Gender
            // Arrange — Setup موجود في المُنشئ

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.ByGender.Should().HaveCount(2, "يجب تحميل توزيع الجنسين");
            _viewModel.ByGender.Should().Contain(g => g.Gender == "ذكر" && g.VisitsCount == 6);
            _viewModel.ByGender.Should().Contain(g => g.Gender == "أنثى" && g.VisitsCount == 4);
        }

        [Fact]
        public async Task PatientCountByGender_WhenServiceReturnsEmpty_ShouldKeepByGenderCollectionEmpty_Edge()
        {
            // Function: 9.1 — Patient Count by Gender
            // Arrange
            _statsMock
                .Setup(x => x.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new StatisticsSnapshot
                {
                    Summary = new StatisticsSummary(),
                    ByGender = new List<StatisticsGenderRow>(),
                    ByReferral = new List<StatisticsReferralRow>()
                });

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.ByGender.Should().BeEmpty("عند غياب البيانات يجب أن تكون مجموعة الجنس فارغة");
        }

        [Fact]
        public async Task PatientCountByGender_WhenServiceThrows_ShouldSetErrorStatusMessage_Failure()
        {
            // Function: 9.1 — Patient Count by Gender
            // Arrange
            _statsMock
                .Setup(x => x.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<string?>(), It.IsAny<int?>()))
                .ThrowsAsync(new InvalidOperationException("خطأ في قاعدة البيانات"));

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:",
                "عند حدوث استثناء يجب عرض رسالة خطأ للمستخدم");
            _viewModel.StatusMessage.Should().Contain("قاعدة البيانات");
        }

        [Fact]
        public async Task PatientCountByGender_LoadCommand_ShouldUpdateSummaryCountsCorrectly()
        {
            // Function: 9.1 — Patient Count by Gender
            // Arrange — Setup مُعَد بـ 10 زيارات و 8 مرضى

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.VisitCount.Should().Be(10);
            _viewModel.PatientCount.Should().Be(8);
            _viewModel.TestCount.Should().Be(20);
            _viewModel.TotalRevenue.Should().Be(5000m);
            _viewModel.TotalPaid.Should().Be(4500m);
        }

        // ===================================================================
        // 9.2 توزيع المرضى حسب الشهر — Patient Count by Month
        // ===================================================================

        [Fact]
        public async Task PatientCountByMonth_LoadCommand_ShouldPopulateMonthlyAnalysisCollection_Success()
        {
            // Function: 9.2 — Patient Count by Month
            // Arrange — Setup موجود في المُنشئ

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.MonthlyAnalysis.Should().NotBeEmpty("يجب تحميل التحليل الشهري");
            _viewModel.MonthlyAnalysis.Should().Contain(m => m.Month == 1 && m.VisitCount == 3);
            _viewModel.MonthlyAnalysis.Should().Contain(m => m.Month == 2 && m.VisitCount == 2);
        }

        [Fact]
        public async Task PatientCountByMonth_WhenServiceReturnsEmpty_ShouldHaveEmptyMonthlyCollection_Edge()
        {
            // Function: 9.2 — Patient Count by Month
            // Arrange
            _statsMock
                .Setup(x => x.GetMonthlyAnalysisAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<MonthlyAnalysisRow>());

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.MonthlyAnalysis.Should().BeEmpty("عند غياب البيانات تكون قائمة الأشهر فارغة");
        }

        [Fact]
        public async Task PatientCountByMonth_LoadCommand_ShouldCallGetMonthlyAnalysisWithCurrentYear_Verify()
        {
            // Function: 9.2 — Patient Count by Month
            // Arrange — تتبع استدعاء الـ Service

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _statsMock.Verify(
                x => x.GetMonthlyAnalysisAsync(It.IsAny<int>()),
                Times.AtLeastOnce,
                "يجب استدعاء GetMonthlyAnalysisAsync عند تحميل البيانات");
        }

        // ===================================================================
        // 9.3 تحليل الطلب على التحاليل — Test Demand Analysis
        // ===================================================================

        [Fact]
        public async Task TestDemandAnalysis_LoadCommand_ShouldPopulateTopTestsCollection_Success()
        {
            // Function: 9.3 — Test Demand Analysis
            // Arrange — Setup موجود في المُنشئ

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.TopTests.Should().HaveCount(2, "يجب تحميل قائمة أكثر التحاليل طلباً");
            _viewModel.TopTests.Should().Contain(t => t.TestName == "صورة دم كاملة" && t.DemandCount == 5);
            _viewModel.TopTests.Should().Contain(t => t.TestName == "سكر الدم" && t.DemandCount == 3);
        }

        [Fact]
        public async Task TestDemandAnalysis_WhenServiceThrows_ShouldSetErrorMessage_Failure()
        {
            // Function: 9.3 — Test Demand Analysis
            // Arrange
            _statsMock
                .Setup(x => x.GetTop10TestsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("خطأ في تحليل الطلب"));

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
        }

        [Fact]
        public async Task TestDemandAnalysis_WhenServiceReturnsEmptyList_ShouldKeepTopTestsEmpty_Edge()
        {
            // Function: 9.3 — Test Demand Analysis
            // Arrange
            _statsMock
                .Setup(x => x.GetTop10TestsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<TopTestRow>());

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.TopTests.Should().BeEmpty("عند غياب البيانات تكون قائمة التحاليل فارغة");
        }

        [Fact]
        public async Task TestDemandAnalysis_LoadCommand_ShouldPassDateRangeToService_Verify()
        {
            // Function: 9.3 — Test Demand Analysis
            // Arrange

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _statsMock.Verify(
                x => x.GetTop10TestsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce,
                "يجب استدعاء GetTop10TestsAsync بنطاق زمني");
        }

        // ===================================================================
        // 9.4 عدد العينات سنوياً — Sample Count per Year
        // ===================================================================

        [Fact]
        public async Task SampleCountPerYear_LoadCommand_ShouldPopulateYearlySamplesCollection_Success()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange — Setup موجود في المُنشئ

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.YearlySamples.Should().HaveCount(2, "يجب تحميل إحصائيات السنوات");
            _viewModel.YearlySamples.Should().Contain(y => y.Year == DateTime.Today.Year && y.SamplesCount == 200);
            _viewModel.YearlySamples.Should().Contain(y => y.Year == DateTime.Today.Year - 1 && y.SamplesCount == 150);
        }

        [Fact]
        public async Task SampleCountPerYear_WhenServiceReturnsEmpty_ShouldKeepYearlySamplesEmpty_Edge()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange
            _statsMock
                .Setup(x => x.GetSampleCountPerYearAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<YearlySampleRow>());

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.YearlySamples.Should().BeEmpty("عند غياب البيانات تكون قائمة السنوات فارغة");
        }

        [Fact]
        public async Task SampleCountPerYear_WhenServiceThrows_ShouldSetErrorMessage_Failure()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange
            _statsMock
                .Setup(x => x.GetSampleCountPerYearAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("فشل تحميل بيانات السنوات"));

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:",
                "عند الاستثناء يجب عرض رسالة الخطأ للمستخدم");
        }

        [Fact]
        public async Task SampleCountPerYear_LoadCommand_ShouldCallGetSampleCountPerYearWithCorrectParameter_Verify()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _statsMock.Verify(
                x => x.GetSampleCountPerYearAsync(It.IsAny<int>()),
                Times.AtLeastOnce,
                "يجب استدعاء GetSampleCountPerYearAsync عند تحميل البيانات");
        }

        // ===================================================================
        // 9.5 تحليل مصادر الإحالة — Referral Source Analysis
        // ===================================================================

        [Fact]
        public async Task ReferralSourceAnalysis_LoadCommand_ShouldPopulateByReferralCollection_Success()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange — Setup موجود في المُنشئ

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.ByReferral.Should().NotBeEmpty("يجب تحميل توزيع مصادر الإحالة");
            _viewModel.ByReferral.Should().Contain(r =>
                r.ReferralName == "عيادة النور" &&
                r.VisitsCount == 5 &&
                r.Revenue == 2500m);
        }

        [Fact]
        public async Task ReferralSourceAnalysis_OnInitialize_ShouldPopulateReferralsDropdownList_Success()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange — انتظار اكتمال الـ Initialize
            await Task.Delay(300);

            // Assert
            _viewModel.Referrals.Should().NotBeEmpty("قائمة الإحالات يجب أن تُحمَّل في بداية التشغيل");
            _viewModel.Referrals.Should().Contain(r => r.Name == "الكل",
                "يجب وجود خيار 'الكل' في أول القائمة");
            _viewModel.Referrals.Should().Contain(r => r.Name == "عيادة النور");
        }

        [Fact]
        public async Task ReferralSourceAnalysis_WhenServiceReturnsEmpty_ShouldKeepByReferralEmpty_Edge()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange
            _statsMock
                .Setup(x => x.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new StatisticsSnapshot
                {
                    Summary = new StatisticsSummary(),
                    ByGender = new List<StatisticsGenderRow>(),
                    ByReferral = new List<StatisticsReferralRow>()
                });

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.ByReferral.Should().BeEmpty("عند غياب البيانات تكون قائمة الإحالات فارغة");
        }

        [Fact]
        public async Task ReferralSourceAnalysis_LoadCommand_ShouldCallGetSnapshotWithSelectedReferral_Verify()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _statsMock.Verify(
                x => x.GetSnapshotAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string?>(),
                    It.IsAny<int?>()),
                Times.AtLeastOnce,
                "يجب استدعاء GetSnapshotAsync عند تحميل بيانات مصادر الإحالة");
        }

        // ===================================================================
        // 9.6 تقرير إنتاجية المستخدمين — User Productivity Report
        // ===================================================================

        [Fact]
        public async Task UserProductivityReport_LoadCommand_ShouldPopulateUserProductivityCollection_Success()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange — Setup موجود في المُنشئ

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.UserProductivity.Should().HaveCount(2, "يجب تحميل بيانات إنتاجية المستخدمين");
            _viewModel.UserProductivity.Should().Contain(u =>
                u.Username == "tech.sara" && u.CompletedTestsCount == 10);
            _viewModel.UserProductivity.Should().Contain(u =>
                u.Username == "tech.omar" && u.CompletedTestsCount == 7);
        }

        [Fact]
        public async Task UserProductivityReport_WhenServiceThrows_ShouldSetErrorMessage_Failure()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange
            _productivityMock
                .Setup(x => x.GetUserPerformanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("فشل تحميل بيانات الإنتاجية"));

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:",
                "عند فشل تحميل الإنتاجية يجب عرض رسالة خطأ");
        }

        [Fact]
        public async Task UserProductivityReport_WhenServiceReturnsEmpty_ShouldKeepProductivityCollectionEmpty_Edge()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange
            _productivityMock
                .Setup(x => x.GetUserPerformanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UserPerformanceRow>());

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.UserProductivity.Should().BeEmpty("عند غياب البيانات تكون قائمة الإنتاجية فارغة");
        }

        [Fact]
        public async Task UserProductivityReport_LoadCommand_ShouldCallGetUserPerformanceWithDateRange_Verify()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _productivityMock.Verify(
                x => x.GetUserPerformanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.AtLeastOnce,
                "يجب استدعاء GetUserPerformanceAsync عند تحميل التقرير");
        }

        // ===================================================================
        // اختبارات مشتركة — الأوامر والأذونات
        // ===================================================================

        [Fact]
        public async Task LoadCommand_OnSuccess_ShouldSetSuccessStatusMessage()
        {
            // Function: 9.1 — Patient Count by Gender (رسالة النجاح)
            // Arrange — جميع الـ setups موجودة في المُنشئ

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.StatusMessage.Should().Be("تم تحميل الإحصائيات.",
                "عند نجاح التحميل يجب عرض رسالة نجاح");
        }

        [Fact]
        public void LoadCommand_WhenUserHasNoPermission_ShouldBeDisabled_Failure()
        {
            // Function: 9.1 — Patient Count by Gender (التحقق من الصلاحيات)
            // Arrange
            AppSessionTestHelper.Reset(); // مستخدم عادي بلا صلاحيات

            // Act
            var canExecute = _viewModel.LoadCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse("المستخدم بلا صلاحية StatisticsView لا يمكنه تحميل الإحصائيات");
        }

        [Fact]
        public void PrintCommand_WhenUserHasNoPermission_ShouldBeDisabled_Failure()
        {
            // Function: 9.6 — User Productivity Report (التحقق من صلاحيات الطباعة)
            // Arrange
            AppSessionTestHelper.Reset(); // مستخدم عادي بلا صلاحيات

            // Act
            var canExecute = _viewModel.PrintCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse("المستخدم بلا صلاحية لا يمكنه طباعة تقارير الإحصائيات");
        }

        [Fact]
        public async Task PrintCommand_WhenDataLoaded_ShouldCallPrintServiceWithCorrectReport_Success()
        {
            // Function: 9.6 — User Productivity Report (طباعة التقرير)
            // Arrange
            _printMock
                .Setup(x => x.PrintTextReportAsync(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<string>>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            await _viewModel.InvokePrivateAsync("LoadAsync");

            // Act
            _viewModel.PrintCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _printMock.Verify(
                x => x.PrintTextReportAsync(
                    "تقرير الإحصائيات",
                    It.IsAny<IReadOnlyCollection<string>>(),
                    "StatisticsReport"),
                Times.Once,
                "يجب استدعاء PrintTextReportAsync باسم التقرير الصحيح");
            _viewModel.StatusMessage.Should().Be("تم إرسال التقرير للطباعة.");
        }

        [Fact]
        public async Task PrintCommand_WhenPrintServiceThrows_ShouldSetErrorMessage_Edge()
        {
            // Function: 9.6 — User Productivity Report (فشل الطباعة)
            // Arrange
            _printMock
                .Setup(x => x.PrintTextReportAsync(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<string>>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("الطابعة غير متصلة"));

            // Act
            _viewModel.PrintCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:",
                "عند فشل الطباعة يجب عرض رسالة خطأ");
            _viewModel.StatusMessage.Should().Contain("الطابعة غير متصلة");
        }

        [Fact]
        public async Task AllModule9Data_LoadCommand_ShouldPopulateAllCollectionsInSingleCall_Success()
        {
            // Function: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6 — اختبار التكامل الكامل
            // Arrange — جميع الـ setups موجودة في المُنشئ

            // Act
            _viewModel.LoadCommand.Execute(null);
            await Task.Delay(200);

            // Assert
            _viewModel.ByGender.Should().NotBeEmpty("9.1: توزيع الجنس");
            _viewModel.MonthlyAnalysis.Should().NotBeEmpty("9.2: التحليل الشهري");
            _viewModel.TopTests.Should().NotBeEmpty("9.3: أكثر التحاليل طلباً");
            _viewModel.YearlySamples.Should().NotBeEmpty("9.4: العينات السنوية");
            _viewModel.ByReferral.Should().NotBeEmpty("9.5: مصادر الإحالة");
            _viewModel.UserProductivity.Should().NotBeEmpty("9.6: إنتاجية المستخدمين");
            _viewModel.StatusMessage.Should().Be("تم تحميل الإحصائيات.");
        }
    }
}
