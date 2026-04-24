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
    public class StatisticsViewModelTests : IDisposable
    {
        private readonly Mock<IStatisticsService> _statisticsServiceMock;
        private readonly Mock<IUserProductivityService> _userProductivityServiceMock;
        private readonly Mock<IPrintService> _printServiceMock;
        private readonly StatisticsViewModel _viewModel;

        public StatisticsViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _statisticsServiceMock = new Mock<IStatisticsService>();
            _userProductivityServiceMock = new Mock<IUserProductivityService>();
            _printServiceMock = new Mock<IPrintService>();

            _statisticsServiceMock
                .Setup(x => x.GetReferralsAsync())
                .ReturnsAsync(new List<StatisticsReferralLookup>
                {
                    new() { ReferralId = 7, Name = "Referral A" }
                });

            _statisticsServiceMock
                .Setup(x => x.GetSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(new StatisticsSnapshot
                {
                    Summary = new StatisticsSummary
                    {
                        VisitCount = 2,
                        PatientCount = 2,
                        TestCount = 3,
                        TotalRevenue = 150m,
                        TotalPaid = 80m
                    },
                    ByGender = new List<StatisticsGenderRow>
                    {
                        new() { Gender = "ذكر", VisitsCount = 2, TestsCount = 3, Revenue = 150m }
                    },
                    ByReferral = new List<StatisticsReferralRow>
                    {
                        new() { ReferralName = "Referral A", VisitsCount = 2, TestsCount = 3, Revenue = 150m, Paid = 80m }
                    }
                });

            _statisticsServiceMock
                .Setup(x => x.GetMonthlyAnalysisAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<MonthlyAnalysisRow> { new() { Month = 1, MonthName = "يناير", VisitCount = 2, TestCount = 3, Revenue = 150m } });

            _statisticsServiceMock
                .Setup(x => x.GetTop10TestsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<TopTestRow> { new() { TestName = "CBC", DemandCount = 2, TotalRevenue = 90m } });

            _statisticsServiceMock
                .Setup(x => x.GetSampleCountPerYearAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<YearlySampleRow> { new() { Year = DateTime.Today.Year, SamplesCount = 3 } });

            _userProductivityServiceMock
                .Setup(x => x.GetUserPerformanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UserPerformanceRow> { new() { UserId = 1, Username = "tech1", CompletedTestsCount = 5 } });

            _viewModel = new StatisticsViewModel(
                _statisticsServiceMock.Object,
                _userProductivityServiceMock.Object,
                _printServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadAsync_Should_Populate_All_Module9_Collections()
        {
            await _viewModel.InvokePrivateAsync("LoadAsync");

            _viewModel.VisitCount.Should().Be(2);
            _viewModel.PatientCount.Should().Be(2);
            _viewModel.TestCount.Should().Be(3);
            _viewModel.TotalRevenue.Should().Be(150m);
            _viewModel.TotalPaid.Should().Be(80m);
            _viewModel.ByGender.Should().ContainSingle();
            _viewModel.ByReferral.Should().ContainSingle();
            _viewModel.MonthlyAnalysis.Should().ContainSingle();
            _viewModel.TopTests.Should().ContainSingle();
            _viewModel.YearlySamples.Should().ContainSingle();
            _viewModel.UserProductivity.Should().ContainSingle();
            _viewModel.StatusMessage.Should().Be("تم تحميل الإحصائيات.");
        }
    }
}
