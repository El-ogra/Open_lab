using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests
{

    /// <summary>
    /// Additional ViewModel tests for Module 8
    /// </summary>
    public class Module8ViewModelTests_Additional
    {
        #region Authentication Tests

        [Fact]
        public async Task LoadQueueCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
        {
            // Function: 8.1 — Mark Test as External (Authentication Required)
            AppSessionTestHelper.Reset();
            var extLabMock = new Mock<IExternalLabService>();
            var extSettleMock = new Mock<IExternalSettlementService>();
            var testCatMock = new Mock<ITestCatalogService>();
            var printMock = new Mock<IPrintService>();
            var reportMock = new Mock<IReportService>();

            testCatMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral>());

            var viewModel = new ExternalLabManagementViewModel(
                extLabMock.Object,
                extSettleMock.Object,
                testCatMock.Object,
                printMock.Object,
                reportMock.Object);

            // Act
            viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            viewModel.StatusMessage.Should().Contain("تسجيل الدخول");
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task LoadSettlementCommand_Without_Login_Should_Set_Auth_Error_EdgeGuard()
        {
            // Function: 8.7 — Settle External Lab Account (Authentication Required)
            AppSessionTestHelper.Reset();
            var extLabMock = new Mock<IExternalLabService>();
            var extSettleMock = new Mock<IExternalSettlementService>();
            var testCatMock = new Mock<ITestCatalogService>();
            var printMock = new Mock<IPrintService>();
            var reportMock = new Mock<IReportService>();

            var viewModel = new ExternalLabManagementViewModel(
                extLabMock.Object,
                extSettleMock.Object,
                testCatMock.Object,
                printMock.Object,
                reportMock.Object);

            viewModel.SelectedReferralId = 1;

            // Act
            viewModel.LoadSettlementCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            viewModel.StatusMessage.Should().Contain("تسجيل الدخول");
            AppSessionTestHelper.Reset();
        }

        #endregion

        #region State Transition Tests

        [Fact]
        public async Task Complete_External_Lab_Workflow_Should_Work_SuccessGuard()
        {
            // Function: 8.1-8.5 — Complete External Lab Workflow
            AppSessionTestHelper.ResetToAdmin();
            var extLabMock = new Mock<IExternalLabService>();
            var extSettleMock = new Mock<IExternalSettlementService>();
            var testCatMock = new Mock<ITestCatalogService>();
            var printMock = new Mock<IPrintService>();
            var reportMock = new Mock<IReportService>();

            var referral = new Referral { ReferralId = 87, Name = "Workflow Lab", ReferralType = "ExternalLab" };
            testCatMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral> { referral });

            extLabMock.Setup(x => x.GetPendingQueueAsync()).ReturnsAsync(new List<ExternalLabQueue>
            {
                new() { QueueId = 1, VisitTestId = 1, Status = "Pending", ReferralId = 87 }
            });
            extLabMock.Setup(x => x.GetAllManifestsAsync()).ReturnsAsync(new List<ShipmentManifest>());
            extSettleMock.Setup(x => x.GetSettlementHistoryAsync(It.IsAny<int>())).ReturnsAsync(new List<ExternalLabSettlement>());
            extSettleMock.Setup(x => x.GetPendingBalanceAsync(It.IsAny<int>())).ReturnsAsync(0m);
            extSettleMock.Setup(x => x.GetTotalProfitAsync(It.IsAny<int>())).ReturnsAsync(0m);

            var viewModel = new ExternalLabManagementViewModel(
                extLabMock.Object,
                extSettleMock.Object,
                testCatMock.Object,
                printMock.Object,
                reportMock.Object);

            // Step 1: Load Referrals
            viewModel.LoadReferralsCommand.Execute(null);
            await Task.Delay(50);
            viewModel.Referrals.Should().ContainSingle();

            // Step 2: Load Queue
            viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(50);
            viewModel.PendingQueue.Should().NotBeEmpty();

            // Step 3: Load Manifests
            viewModel.LoadManifestsCommand.Execute(null);
            await Task.Delay(50);

            // Step 4: Load Settlement
            viewModel.SelectedReferralId = 87;
            viewModel.LoadSettlementCommand.Execute(null);
            await Task.Delay(50);

            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task CreateManifest_Then_Ship_Should_Update_Status_SuccessGuard()
        {
            // Function: 8.3 — Prepare External Sample (Workflow)
            AppSessionTestHelper.ResetToAdmin();
            var extLabMock = new Mock<IExternalLabService>();
            var extSettleMock = new Mock<IExternalSettlementService>();
            var testCatMock = new Mock<ITestCatalogService>();
            var printMock = new Mock<IPrintService>();
            var reportMock = new Mock<IReportService>();

            testCatMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            extLabMock.Setup(x => x.GetPendingQueueAsync()).ReturnsAsync(new List<ExternalLabQueue>());
            extLabMock.Setup(x => x.GetAllManifestsAsync()).ReturnsAsync(new List<ShipmentManifest>());
            extSettleMock.Setup(x => x.GetSettlementHistoryAsync(It.IsAny<int>())).ReturnsAsync(new List<ExternalLabSettlement>());
            extSettleMock.Setup(x => x.GetPendingBalanceAsync(It.IsAny<int>())).ReturnsAsync(0m);
            extSettleMock.Setup(x => x.GetTotalProfitAsync(It.IsAny<int>())).ReturnsAsync(0m);

            extLabMock.Setup(x => x.CreateManifestAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string?>()))
                .ReturnsAsync(new ShipmentManifest
                {
                    ManifestId = 1,
                    ManifestNumber = "WF-MAN-87",
                    ReferralId = 1,
                    DateCreated = DateTime.Today,
                    Status = "Open"
                });

            var viewModel = new ExternalLabManagementViewModel(
                extLabMock.Object,
                extSettleMock.Object,
                testCatMock.Object,
                printMock.Object,
                reportMock.Object);

            // Setup for manifest creation
            viewModel.SelectedReferralId = 1;
            viewModel.SelectedQueueIds.Add(1);

            // Act
            viewModel.CreateManifestCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            extLabMock.Verify(x => x.CreateManifestAsync(1, It.IsAny<List<int>>(), It.IsAny<string?>()), Times.Once);
            viewModel.SelectedQueueIds.Should().BeEmpty();
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task EnterExternalResultCommand_With_Null_Queue_Item_Should_Not_Call_Service_EdgeGuard()
        {
            // Function: 8.5 — Enter External Lab Result (No Selection)
            AppSessionTestHelper.ResetToAdmin();
            var extLabMock = new Mock<IExternalLabService>();
            var extSettleMock = new Mock<IExternalSettlementService>();
            var testCatMock = new Mock<ITestCatalogService>();
            var printMock = new Mock<IPrintService>();
            var reportMock = new Mock<IReportService>();

            testCatMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            extLabMock.Setup(x => x.GetPendingQueueAsync()).ReturnsAsync(new List<ExternalLabQueue>());
            extLabMock.Setup(x => x.GetAllManifestsAsync()).ReturnsAsync(new List<ShipmentManifest>());
            extSettleMock.Setup(x => x.GetSettlementHistoryAsync(It.IsAny<int>())).ReturnsAsync(new List<ExternalLabSettlement>());
            extSettleMock.Setup(x => x.GetPendingBalanceAsync(It.IsAny<int>())).ReturnsAsync(0m);
            extSettleMock.Setup(x => x.GetTotalProfitAsync(It.IsAny<int>())).ReturnsAsync(0m);

            var viewModel = new ExternalLabManagementViewModel(
                extLabMock.Object,
                extSettleMock.Object,
                testCatMock.Object,
                printMock.Object,
                reportMock.Object);

            viewModel.ExternalResultValue = "Positive";

            // Act
            viewModel.EnterExternalResultCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            extLabMock.Verify(x => x.EnterExternalLabResultAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task CreateSettlementCommand_With_Negative_Amount_Should_Not_Call_Service_EdgeGuard()
        {
            // Function: 8.7 — Settle External Lab Account (Negative Amount)
            AppSessionTestHelper.ResetToAdmin();
            var extLabMock = new Mock<IExternalLabService>();
            var extSettleMock = new Mock<IExternalSettlementService>();
            var testCatMock = new Mock<ITestCatalogService>();
            var printMock = new Mock<IPrintService>();
            var reportMock = new Mock<IReportService>();

            testCatMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            extLabMock.Setup(x => x.GetPendingQueueAsync()).ReturnsAsync(new List<ExternalLabQueue>());
            extLabMock.Setup(x => x.GetAllManifestsAsync()).ReturnsAsync(new List<ShipmentManifest>());
            extSettleMock.Setup(x => x.GetSettlementHistoryAsync(It.IsAny<int>())).ReturnsAsync(new List<ExternalLabSettlement>());
            extSettleMock.Setup(x => x.GetPendingBalanceAsync(It.IsAny<int>())).ReturnsAsync(100m);
            extSettleMock.Setup(x => x.GetTotalProfitAsync(It.IsAny<int>())).ReturnsAsync(100m);

            var viewModel = new ExternalLabManagementViewModel(
                extLabMock.Object,
                extSettleMock.Object,
                testCatMock.Object,
                printMock.Object,
                reportMock.Object);

            viewModel.SelectedReferralId = 1;
            viewModel.SettlementAmount = -50m;

            // Act
            viewModel.CreateSettlementCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            extSettleMock.Verify(x => x.CreateSettlementAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string?>()), Times.Never);
            viewModel.StatusMessage.Should().NotBeNullOrEmpty();
            AppSessionTestHelper.Reset();
        }

        #endregion

        #region Print Tests

        [Fact]
        public async Task PrintExternalReportCommand_With_Valid_Data_Should_Call_Print_Service_SuccessGuard()
        {
            // Function: 8.6 — Print External Lab Report (Print)
            AppSessionTestHelper.ResetToAdmin();
            var extLabMock = new Mock<IExternalLabService>();
            var extSettleMock = new Mock<IExternalSettlementService>();
            var testCatMock = new Mock<ITestCatalogService>();
            var printMock = new Mock<IPrintService>();
            var reportMock = new Mock<IReportService>();

            testCatMock.Setup(x => x.GetReferralsAsync()).ReturnsAsync(new List<Referral>());

            var patient = new Patient { PatientId = 4, FullName = "Print Ext Patient", LabId = "L-PRINT-87", Gender = "Male" };
            var visit = new Visit { VisitId = 12, PatientId = patient.PatientId, Patient = patient, VisitDate = DateTime.Today };
            var test = new Test { TestId = 8, NameReport = "External Print", Code = "EXT-P" };
            var visitTest = new VisitTest { VisitTestId = 2, VisitId = visit.VisitId, Visit = visit, TestId = test.TestId, Test = test, Price = 100m };

            extLabMock.Setup(x => x.GetPendingQueueAsync()).ReturnsAsync(new List<ExternalLabQueue>
            {
                new()
                {
                    QueueId = 1,
                    VisitTestId = visitTest.VisitTestId,
                    VisitTest = visitTest,
                    ReferralId = 3,
                    Referral = new Referral { ReferralId = 3, Name = "Ref Lab", ReferralType = "ExternalLab" },
                    Status = "Pending",
                    DateQueued = DateTime.Now,
                    ExternalReference = "EXT-PRINT-87"
                }
            });
            extLabMock.Setup(x => x.GetAllManifestsAsync()).ReturnsAsync(new List<ShipmentManifest>());
            extSettleMock.Setup(x => x.GetSettlementHistoryAsync(It.IsAny<int>())).ReturnsAsync(new List<ExternalLabSettlement>());
            extSettleMock.Setup(x => x.GetPendingBalanceAsync(It.IsAny<int>())).ReturnsAsync(0m);
            extSettleMock.Setup(x => x.GetTotalProfitAsync(It.IsAny<int>())).ReturnsAsync(0m);

            var report = new VisitReportData { Visit = visit, Patient = patient };
            reportMock.Setup(x => x.GetVisitReportAsync(12)).ReturnsAsync(report);

            var viewModel = new ExternalLabManagementViewModel(
                extLabMock.Object,
                extSettleMock.Object,
                testCatMock.Object,
                printMock.Object,
                reportMock.Object);

            viewModel.LoadQueueCommand.Execute(null);
            await Task.Delay(50);
            viewModel.SelectedQueueItem = viewModel.PendingQueue[0];

            // Act
            viewModel.PrintExternalReportCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            printMock.Verify(x => x.PrintVisitReportAsync(It.IsAny<VisitReportData>(), true), Times.Once);
            viewModel.StatusMessage.Should().Contain("تم إرسال تقرير المعمل الخارجي للطباعة");
            AppSessionTestHelper.Reset();
        }

        #endregion
    }
}
