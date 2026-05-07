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
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange

            // Act
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.PendingQueue.Should().ContainSingle();
            _viewModel.PendingQueue[0].PatientName.Should().Be("Patient 1");
            _viewModel.PendingQueue[0].VisitTest.Should().NotBeNull();
            _viewModel.PendingQueue[0].VisitTest!.VisitId.Should().Be(12);
        }

        [Fact]
        public async Task LoadReferralsCommand_When_Executed_Should_Load_ExternalLab_Referrals_Success()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange

            // Act
            _viewModel.LoadReferralsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Referrals.Should().ContainSingle(r => r.ReferralId == 3);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task LoadReferralsCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _testCatalogServiceMock.Setup(x => x.GetReferralsAsync()).ThrowsAsync(new Exception("referrals-failed"));

            // Act
            _viewModel.LoadReferralsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("referrals-failed");
        }

        [Fact]
        public async Task LoadQueueCommand_When_No_Pending_Items_Should_Clear_Queue_Edge()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _externalLabServiceMock.Setup(x => x.GetPendingQueueAsync()).ReturnsAsync(new List<ExternalLabQueue>());

            // Act
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.PendingQueue.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("0 عناصر");
        }

        [Fact]
        public async Task LoadQueueCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _externalLabServiceMock.Setup(x => x.GetPendingQueueAsync()).ThrowsAsync(new Exception("queue-load-failed"));

            // Act
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("queue-load-failed");
        }

        [Fact]
        public async Task LoadManifestsCommand_When_Executed_Should_Load_Manifest_List_Success()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _externalLabServiceMock.Setup(x => x.GetAllManifestsAsync()).ReturnsAsync(new List<ShipmentManifest>
            {
                new() { ManifestId = 9, ManifestNumber = "MAN-9", ReferralId = 3, DateCreated = DateTime.Now, Status = "Open" }
            });

            // Act
            _viewModel.LoadManifestsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Manifests.Should().ContainSingle(m => m.ManifestId == 9);
        }

        [Fact]
        public async Task LoadManifestsCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _externalLabServiceMock.Setup(x => x.GetAllManifestsAsync()).ThrowsAsync(new Exception("manifests-failed"));

            // Act
            _viewModel.LoadManifestsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("manifests-failed");
        }

        [Fact]
        public async Task PrintExternalReportAsync_Should_Print_When_Visit_Is_Available()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.SelectedQueueItem = _viewModel.PendingQueue[0];

            var report = new VisitReportData
            {
                Visit = new Visit { VisitId = 12, VisitDate = DateTime.Today },
                Patient = new Patient { FullName = "Patient 1", LabId = "L-1", Gender = "Male" }
            };
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(12)).ReturnsAsync(report);

            // Act
            _viewModel.PrintExternalReportCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _printServiceMock.Verify(x => x.PrintVisitReportAsync(report, true), Times.Once);
            _viewModel.StatusMessage.Should().Be("تم إرسال تقرير المعمل الخارجي للطباعة.");
        }

        [Fact]
        public async Task EnterExternalResultAsync_Should_Call_Service_And_Reset_Input()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.SelectedQueueItem = _viewModel.PendingQueue[0];
            _viewModel.ExternalResultValue = "Positive";
            _viewModel.ExternalResultComment = "comment";
            _viewModel.ExternalReference = "REF-9";

            // Act
            _viewModel.EnterExternalResultCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalLabServiceMock.Verify(x => x.EnterExternalLabResultAsync(1, "Positive", "comment", "REF-9"), Times.Once);
            _viewModel.ExternalResultValue.Should().BeEmpty();
            _viewModel.ExternalResultComment.Should().BeNull();
            _viewModel.StatusMessage.Should().Be("تم إدخال نتيجة المعمل الخارجي بنجاح.");
        }

        [Fact]
        public async Task CreateManifestCommand_With_Valid_Data_Should_Create_Manifest_Success()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = 3;
            _viewModel.SelectedQueueIds.Add(1);
            _externalLabServiceMock
                .Setup(x => x.CreateManifestAsync(3, It.IsAny<List<int>>(), It.IsAny<string?>()))
                .ReturnsAsync(new ShipmentManifest { ManifestId = 1, ManifestNumber = "MAN-NEW", ReferralId = 3, DateCreated = DateTime.Now, Status = "Open" });

            // Act
            _viewModel.CreateManifestCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalLabServiceMock.Verify(x => x.CreateManifestAsync(3, It.IsAny<List<int>>(), It.IsAny<string?>()), Times.Once);
            _viewModel.SelectedQueueIds.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("MAN-NEW");
        }

        [Fact]
        public async Task CreateManifestCommand_When_Referral_Missing_Should_Not_Call_Service_Failure()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = null;
            _viewModel.SelectedQueueIds.Add(1);

            // Act
            _viewModel.CreateManifestCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalLabServiceMock.Verify(x => x.CreateManifestAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string?>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateManifestCommand_When_Queue_Selection_Empty_Should_Not_Call_Service_Edge()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = 3;

            // Act
            _viewModel.CreateManifestCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalLabServiceMock.Verify(x => x.CreateManifestAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string?>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UpdateStatusCommand_When_SelectedItem_Exists_Should_Call_Service_Success()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.SelectedQueueItem = _viewModel.PendingQueue[0];

            // Act
            _viewModel.UpdateStatusCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalLabServiceMock.Verify(x => x.UpdateQueueStatusAsync(1, "InManifest", It.IsAny<string?>()), Times.Once);
            _viewModel.StatusMessage.Should().Contain("InManifest");
        }

        [Fact]
        public async Task UpdateStatusCommand_When_No_Selected_Item_Should_Not_Call_Service_Edge()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedQueueItem = null;

            // Act
            _viewModel.UpdateStatusCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalLabServiceMock.Verify(x => x.UpdateQueueStatusAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UpdateStatusCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.SelectedQueueItem = _viewModel.PendingQueue[0];
            _externalLabServiceMock
                .Setup(x => x.UpdateQueueStatusAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>()))
                .ThrowsAsync(new Exception("update-status-failed"));

            // Act
            _viewModel.UpdateStatusCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("update-status-failed");
        }

        [Fact]
        public async Task LoadSettlementAsync_Should_Load_History_And_TotalProfit()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = 3;
            _externalSettlementServiceMock.Setup(x => x.GetSettlementHistoryAsync(3))
                .ReturnsAsync(new List<ExternalLabSettlement>
                {
                    new() { SettlementId = 1, AmountPaid = 20m, Balance = 5m }
                });

            // Act
            _viewModel.LoadSettlementCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.SettlementHistory.Should().ContainSingle();
            _viewModel.TotalProfit.Should().Be(10m);
            _viewModel.StatusMessage.Should().Contain("الرصيد المعلق");
        }

        [Fact]
        public async Task LoadSettlementCommand_When_Referral_Selected_Should_Load_Settlement_Success()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = 3;

            // Act
            _viewModel.LoadSettlementCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalSettlementServiceMock.Verify(x => x.GetPendingBalanceAsync(3), Times.AtLeastOnce);
            _viewModel.StatusMessage.Should().Contain("الرصيد المعلق");
        }

        [Fact]
        public async Task LoadSettlementCommand_When_Referral_Not_Selected_Should_Not_Call_Service_Edge()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = null;

            // Act
            _viewModel.LoadSettlementCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalSettlementServiceMock.Verify(x => x.GetPendingBalanceAsync(It.IsAny<int>()), Times.Never);
            _externalSettlementServiceMock.Verify(x => x.GetSettlementHistoryAsync(It.IsAny<int>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadSettlementCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = 3;
            _externalSettlementServiceMock
                .Setup(x => x.GetPendingBalanceAsync(3))
                .ThrowsAsync(new Exception("settlement-load-failed"));

            // Act
            _viewModel.LoadSettlementCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("settlement-load-failed");
        }

        [Fact]
        public async Task CreateSettlementAsync_When_AmountIsZero_Should_NotCall_Service_EdgeGuard()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = 3;
            _viewModel.SettlementAmount = 0m;

            // Act
            _viewModel.CreateSettlementCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalSettlementServiceMock.Verify(x => x.CreateSettlementAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string?>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateSettlementCommand_When_Valid_Data_Should_Create_And_Reset_Amount_Success()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = 3;
            _viewModel.SettlementAmount = 15m;
            _viewModel.SettlementNote = "note";
            _externalSettlementServiceMock
                .Setup(x => x.CreateSettlementAsync(3, 15m, "note"))
                .ReturnsAsync(new ExternalLabSettlement { SettlementId = 1, ReferralId = 3, AmountPaid = 15m, Balance = 10m, SettlementDate = DateTime.Now });

            // Act
            _viewModel.CreateSettlementCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalSettlementServiceMock.Verify(x => x.CreateSettlementAsync(3, 15m, "note"), Times.Once);
            _viewModel.SettlementAmount.Should().Be(0m);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateSettlementCommand_When_Service_Throws_Should_Set_Error_Message_Failure()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.SelectedReferralId = 3;
            _viewModel.SettlementAmount = 20m;
            _externalSettlementServiceMock
                .Setup(x => x.CreateSettlementAsync(3, 20m, It.IsAny<string?>()))
                .ThrowsAsync(new Exception("settlement-create-failed"));

            // Act
            _viewModel.CreateSettlementCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("settlement-create-failed");
        }

        [Fact]
        public async Task EnterExternalResultAsync_When_ServiceThrows_Should_Set_ErrorMessage_FailureGuard()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.SelectedQueueItem = _viewModel.PendingQueue[0];
            _viewModel.ExternalResultValue = "Positive";
            _externalLabServiceMock
                .Setup(x => x.EnterExternalLabResultAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ThrowsAsync(new Exception("enter-failed"));

            // Act
            _viewModel.EnterExternalResultCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("enter-failed");
        }

        [Fact]
        public async Task EnterExternalResultCommand_When_Result_Value_Empty_Should_Not_Call_Service_Edge()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.SelectedQueueItem = _viewModel.PendingQueue[0];
            _viewModel.ExternalResultValue = " ";

            // Act
            _viewModel.EnterExternalResultCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _externalLabServiceMock.Verify(x => x.EnterExternalLabResultAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task PrintExternalReportCommand_When_VisitId_Invalid_Should_Set_User_Message_Failure()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.SelectedQueueItem = new ExternalLabQueue { QueueId = 3, VisitTest = new VisitTest { VisitId = 0 }, Status = "Pending" };

            // Act
            _viewModel.PrintExternalReportCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("لا يمكن تحديد الزيارة المرتبطة");
        }

        [Fact]
        public async Task PrintExternalReportCommand_When_Report_Not_Found_Should_Set_NotFound_Message_Edge()
        {
            // Function: 2.13 — Lab-to-Lab Settlement
            // Arrange
            _viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.SelectedQueueItem = _viewModel.PendingQueue[0];
            _reportServiceMock.Setup(x => x.GetVisitReportAsync(12)).ReturnsAsync((VisitReportData?)null);

            // Act
            _viewModel.PrintExternalReportCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("لم يتم العثور على تقرير للطباعة");
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
