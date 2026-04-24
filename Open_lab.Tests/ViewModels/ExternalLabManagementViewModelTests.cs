using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class ExternalLabManagementViewModelTests : IDisposable
    {
        private readonly Mock<IExternalLabService> _externalLabServiceMock = new();
        private readonly Mock<IExternalSettlementService> _externalSettlementServiceMock = new();
        private readonly Mock<ITestCatalogService> _testCatalogServiceMock = new();
        private readonly Mock<IPrintService> _printServiceMock = new();
        private readonly Mock<IReportService> _reportServiceMock = new();
        private readonly ExternalLabManagementViewModel _viewModel;

        public ExternalLabManagementViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();

            var queueItem = BuildQueueItem();
            _externalLabServiceMock.Setup(x => x.GetPendingQueueAsync()).ReturnsAsync(new List<ExternalLabQueue> { queueItem });
            _externalLabServiceMock.Setup(x => x.GetAllManifestsAsync()).ReturnsAsync(new List<ShipmentManifest>());
            _testCatalogServiceMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral>
            {
                new() { ReferralId = 3, Name = "Ref Lab", ReferralType = "ExternalLab" }
            });
            _externalSettlementServiceMock.Setup(x => x.GetSettlementHistoryAsync(It.IsAny<int>())).ReturnsAsync(new List<ExternalLabSettlement>());
            _externalSettlementServiceMock.Setup(x => x.GetPendingBalanceAsync(It.IsAny<int>())).ReturnsAsync(25m);
            _externalSettlementServiceMock.Setup(x => x.GetTotalProfitAsync(It.IsAny<int>())).ReturnsAsync(10m);

            _viewModel = new ExternalLabManagementViewModel(
                _externalLabServiceMock.Object,
                _externalSettlementServiceMock.Object,
                _testCatalogServiceMock.Object,
                _printServiceMock.Object,
                _reportServiceMock.Object);
        }

        [Fact]
        public async Task LoadQueueAsync_Should_Map_Row_With_VisitTest_For_Printing()
        {
            await _viewModel.InvokePrivateAsync("LoadQueueAsync");

            _viewModel.PendingQueue.Should().ContainSingle();
            _viewModel.PendingQueue[0].PatientName.Should().Be("Patient 1");
            _viewModel.PendingQueue[0].VisitTest.Should().NotBeNull();
            _viewModel.PendingQueue[0].VisitTest!.VisitId.Should().Be(12);
        }

        [Fact]
        public async Task PrintExternalReportAsync_Should_Print_When_Visit_Is_Available()
        {
            await _viewModel.InvokePrivateAsync("LoadQueueAsync");
            _viewModel.SelectedQueueItem = _viewModel.PendingQueue[0];

            var report = new VisitReportData
            {
                Visit = new Visit { VisitId = 12, VisitDate = DateTime.Today },
                Patient = new Patient { FullName = "Patient 1", LabId = "L-1", Gender = "Male" }
            };
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(12)).ReturnsAsync(report);

            await _viewModel.InvokePrivateAsync("PrintExternalReportAsync");

            _printServiceMock.Verify(x => x.PrintVisitReportAsync(report, true), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم إرسال تقرير المعمل الخارجي للطباعة.");
        }

        [Fact]
        public async Task EnterExternalResultAsync_Should_Call_Service_And_Reset_Input()
        {
            await _viewModel.InvokePrivateAsync("LoadQueueAsync");
            _viewModel.SelectedQueueItem = _viewModel.PendingQueue[0];
            _viewModel.ExternalResultValue = "Positive";
            _viewModel.ExternalResultComment = "comment";
            _viewModel.ExternalReference = "REF-9";

            await _viewModel.InvokePrivateAsync("EnterExternalResultAsync");

            _externalLabServiceMock.Verify(x => x.EnterExternalLabResultAsync(1, "Positive", "comment", "REF-9"), Times.Once);
            _viewModel.ExternalResultValue.Should().BeEmpty();
            _viewModel.ExternalResultComment.Should().BeNull();
            _viewModel.StatusMessage.Should().Be("تم إدخال نتيجة المعمل الخارجي بنجاح.");
        }

        [Fact]
        public async Task LoadSettlementAsync_Should_Load_History_And_TotalProfit()
        {
            _viewModel.SelectedReferralId = 3;
            _externalSettlementServiceMock.Setup(x => x.GetSettlementHistoryAsync(3))
                .ReturnsAsync(new List<ExternalLabSettlement>
                {
                    new() { SettlementId = 1, AmountPaid = 20m, Balance = 5m }
                });

            await _viewModel.InvokePrivateAsync("LoadSettlementAsync");

            _viewModel.SettlementHistory.Should().ContainSingle();
            _viewModel.TotalProfit.Should().Be(10m);
            _viewModel.StatusMessage.Should().Contain("الرصيد المعلق");
        }

        private static ExternalLabQueue BuildQueueItem()
        {
            var patient = new Patient { PatientId = 4, FullName = "Patient 1", LabId = "L-1", Gender = "Male" };
            var visit = new Visit { VisitId = 12, PatientId = patient.PatientId, Patient = patient, VisitDate = DateTime.Today };
            var test = new Test { TestId = 8, NameReport = "PCR", Code = "PCR" };
            var visitTest = new VisitTest { VisitTestId = 2, VisitId = visit.VisitId, Visit = visit, TestId = test.TestId, Test = test, Price = 100m };

            return new ExternalLabQueue
            {
                QueueId = 1,
                VisitTestId = visitTest.VisitTestId,
                VisitTest = visitTest,
                ReferralId = 3,
                Referral = new Referral { ReferralId = 3, Name = "Ref Lab", ReferralType = "ExternalLab" },
                Status = "Pending",
                DateQueued = DateTime.Now,
                ExternalReference = "REF-9"
            };
        }

        public void Dispose() => AppSessionTestHelper.Reset();
    }
}
