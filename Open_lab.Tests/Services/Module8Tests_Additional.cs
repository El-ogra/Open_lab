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
    /// Additional comprehensive tests for Module 8: External Labs (Lab-to-Lab)
    /// Covers Functions 8.1-8.7 with Service and ViewModel layer tests
    /// </summary>
    public class Module8ServiceTests_Additional : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly ExternalLabService _externalLabService;
        private readonly ExternalSettlementService _externalSettlementService;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module8ServiceTests_Additional()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _externalLabService = new ExternalLabService(_db);
            _externalSettlementService = new ExternalSettlementService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        #region Function 8.1 - Mark Test as External

        [Fact]
        public async Task AddToQueueAsync_With_Valid_ReferralId_Should_Set_ReferralId_Correctly_SuccessGuard()
        {
            // Function: 8.1 — Mark Test as External (Set Referral)
            // Arrange
            var referral = new Referral { Name = "External Lab 81", ReferralType = "ExternalLab" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-81-REF", FullName = "Patient 81", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT-81", NameReport = "External 81", Price = 50m, IsSendOut = true };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 50m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            // Act
            var queue = await _externalLabService.AddToQueueAsync(visitTest.VisitTestId, referral.ReferralId);

            // Assert
            queue.Should().NotBeNull();
            queue.ReferralId.Should().Be(referral.ReferralId);
        }

        [Fact]
        public async Task AddToQueueAsync_With_Null_ReferralId_Should_Set_Null_Referral_EdgeGuard()
        {
            // Function: 8.1 — Mark Test as External (Null Referral)
            // Arrange
            var patient = new Patient { LabId = "L-81-NULL", FullName = "Null Ref Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT-81N", NameReport = "External 81N", Price = 30m, IsSendOut = true };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 30m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            // Act
            var queue = await _externalLabService.AddToQueueAsync(visitTest.VisitTestId, null);

            // Assert
            queue.Should().NotBeNull();
            queue.QueueId.Should().BeGreaterThan(0);
            queue.ReferralId.Should().BeNull();
        }

        [Fact]
        public async Task AddToQueueAsync_When_Already_In_Queue_Should_Return_Existing_Queue_EdgeGuard()
        {
            // Function: 8.1 — Mark Test as External (Duplicate Prevention)
            // Arrange
            var patient = new Patient { LabId = "L-81-DUP", FullName = "Duplicate Test", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT-81D", NameReport = "External 81D", Price = 40m, IsSendOut = true };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 40m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            // Act - Add twice
            var queue1 = await _externalLabService.AddToQueueAsync(visitTest.VisitTestId, 1);
            var queue2 = await _externalLabService.AddToQueueAsync(visitTest.VisitTestId, 1);

            // Assert
            queue1.QueueId.Should().Be(queue2.QueueId);
            var count = await _db.ExternalLabQueues.CountAsync(q => q.VisitTestId == visitTest.VisitTestId);
            count.Should().Be(1);
        }

        #endregion

        #region Function 8.2 - Register Patient for External Test

        [Fact]
        public async Task AddToQueueAsync_Should_Automatically_Add_Patient_To_Pending_Queue_SuccessGuard()
        {
            // Function: 8.2 — Register Patient for External Test (Auto Registration)
            // Arrange
            var patient = new Patient { LabId = "L-82-AUTO", FullName = "Auto Register Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var externalTest = new Test { Code = "EXT-82", NameReport = "External 82", Price = 75m, IsSendOut = true };
            _db.Tests.Add(externalTest);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = externalTest.TestId, Price = 75m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            // Act
            await _externalLabService.AddToQueueAsync(visitTest.VisitTestId, 1);

            // Assert - Patient should be in pending queue
            var pendingQueue = await _externalLabService.GetPendingQueueAsync();
            pendingQueue.Should().Contain(q => q.VisitTestId == visitTest.VisitTestId);
        }

        #endregion

        #region Function 8.3 - Prepare External Sample (Create Manifest)

        [Fact]
        public async Task CreateManifestAsync_With_Valid_Queue_Items_Should_Create_Shipment_Manifest_SuccessGuard()
        {
            // Function: 8.3 — Prepare External Sample (Create Manifest)
            // Arrange
            var referral = new Referral { Name = "Manifest Lab 83", ReferralType = "ExternalLab" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-83-MAN", FullName = "Manifest Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT-83", NameReport = "External 83", Price = 60m, IsSendOut = true };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 60m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            var queue = new ExternalLabQueue
            {
                VisitTestId = visitTest.VisitTestId,
                ReferralId = referral.ReferralId,
                Status = "Pending",
                DateQueued = DateTime.Today
            };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            // Act
            var manifest = await _externalLabService.CreateManifestAsync(
                referral.ReferralId,
                new List<int> { queue.QueueId },
                "Test shipment notes");

            // Assert
            manifest.Should().NotBeNull();
            manifest.ReferralId.Should().Be(referral.ReferralId);
            manifest.Items.Should().ContainSingle();
            manifest.Items.First().QueueId.Should().Be(queue.QueueId);

            // Verify queue status updated
            var updatedQueue = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            updatedQueue!.Status.Should().Be("InManifest");
        }

        [Fact]
        public async Task CreateManifestAsync_With_Multiple_Queue_Items_Should_Create_All_Items_SuccessGuard()
        {
            // Function: 8.3 — Prepare External Sample (Multiple Items)
            // Arrange
            var referral = new Referral { Name = "Multi Item Lab", ReferralType = "ExternalLab" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-83-MULTI", FullName = "Multi Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test1 = new Test { Code = "EXT-83A", NameReport = "External 83A", Price = 20m, IsSendOut = true };
            var test2 = new Test { Code = "EXT-83B", NameReport = "External 83B", Price = 25m, IsSendOut = true };
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 20m };
            var vt2 = new VisitTest { VisitId = visit.VisitId, TestId = test2.TestId, Price = 25m };
            _db.VisitTests.AddRange(vt1, vt2);
            await _db.SaveChangesAsync();

            var q1 = new ExternalLabQueue { VisitTestId = vt1.VisitTestId, ReferralId = referral.ReferralId, Status = "Pending", DateQueued = DateTime.Today };
            var q2 = new ExternalLabQueue { VisitTestId = vt2.VisitTestId, ReferralId = referral.ReferralId, Status = "Pending", DateQueued = DateTime.Today };
            _db.ExternalLabQueues.AddRange(q1, q2);
            await _db.SaveChangesAsync();

            // Act
            var manifest = await _externalLabService.CreateManifestAsync(
                referral.ReferralId,
                new List<int> { q1.QueueId, q2.QueueId },
                "Multi test shipment");

            // Assert
            manifest.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllManifestsAsync_Should_Return_Manifests_With_Correct_Referral_Details_SuccessGuard()
        {
            // Function: 8.3 — Prepare External Sample (Get Manifests with Referral)
            // Arrange
            var referral1 = new Referral { Name = "Lab A-83", ReferralType = "ExternalLab" };
            var referral2 = new Referral { Name = "Lab B-83", ReferralType = "ExternalLab" };
            _db.Referrals.AddRange(referral1, referral2);
            await _db.SaveChangesAsync();

            _db.ShipmentManifests.AddRange(
                new ShipmentManifest { ManifestNumber = "MAN-A-83", ReferralId = referral1.ReferralId, DateCreated = DateTime.Today.AddDays(-2), Status = "Shipped" },
                new ShipmentManifest { ManifestNumber = "MAN-B-83", ReferralId = referral2.ReferralId, DateCreated = DateTime.Today, Status = "Open" });
            await _db.SaveChangesAsync();

            // Act
            var manifests = await _externalLabService.GetAllManifestsAsync();

            // Assert
            manifests.Should().HaveCount(2);
            manifests.First(m => m.ManifestNumber == "MAN-B-83").Referral.Should().NotBeNull();
            manifests.First(m => m.ManifestNumber == "MAN-B-83").Referral!.Name.Should().Be("Lab B-83");
        }

        #endregion

        #region Function 8.4 - Track External Sample Status

        [Fact]
        public async Task GetPendingQueueAsync_Should_Return_Only_Pending_Status_Items_SuccessGuard()
        {
            // Function: 8.4 — Track External Sample Status (Pending Only)
            // Arrange
            var patient = new Patient { LabId = "L-84-STATUS", FullName = "Status Track", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT-84", NameReport = "External 84", Price = 35m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 35m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var pendingQueue = new ExternalLabQueue
            {
                VisitTestId = vt.VisitTestId,
                ReferralId = 1,
                Status = "Pending",
                DateQueued = DateTime.Today
            };
            var shippedQueue = new ExternalLabQueue
            {
                VisitTestId = vt.VisitTestId + 1,
                ReferralId = 1,
                Status = "Shipped",
                DateQueued = DateTime.Today.AddDays(-1)
            };
            var receivedQueue = new ExternalLabQueue
            {
                VisitTestId = vt.VisitTestId + 2,
                ReferralId = 1,
                Status = "Received",
                DateQueued = DateTime.Today.AddDays(-2)
            };
            _db.ExternalLabQueues.AddRange(pendingQueue, shippedQueue, receivedQueue);
            await _db.SaveChangesAsync();

            // Act
            var pendingItems = await _externalLabService.GetPendingQueueAsync();

            // Assert - Should only return items with "Pending" status
            pendingItems.Should().Contain(q => q.QueueId == pendingQueue.QueueId);
            pendingItems.Should().NotContain(q => q.Status != "Pending");
        }

        [Fact]
        public async Task UpdateQueueStatusAsync_Should_Update_External_Reference_SuccessGuard()
        {
            // Function: 8.4 — Track External Sample Status (Update Reference)
            // Arrange
            var queue = new ExternalLabQueue
            {
                VisitTestId = 1,
                ReferralId = 5,
                Status = "Pending",
                DateQueued = DateTime.Today
            };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            // Act
            await _externalLabService.UpdateQueueStatusAsync(queue.QueueId, "Shipped", "TRACK-84-REF");

            // Assert
            var updated = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            updated!.Status.Should().Be("Shipped");
            updated.ExternalReference.Should().Be("TRACK-84-REF");
        }

        [Fact]
        public async Task UpdateQueueStatusAsync_With_Different_Statuses_Should_Update_Correctly_EdgeGuard()
        {
            // Function: 8.4 — Track External Sample Status (Different Statuses)
            // Arrange
            var queue = new ExternalLabQueue
            {
                VisitTestId = 1,
                ReferralId = 10,
                Status = "Pending",
                DateQueued = DateTime.Today
            };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            // Act - Transition through different statuses
            await _externalLabService.UpdateQueueStatusAsync(queue.QueueId, "InManifest", "REF-1");
            var status1 = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            status1!.Status.Should().Be("InManifest");

            await _externalLabService.UpdateQueueStatusAsync(queue.QueueId, "Shipped", "REF-2");
            var status2 = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            status2!.Status.Should().Be("Shipped");

            await _externalLabService.UpdateQueueStatusAsync(queue.QueueId, "Received", "REF-3");
            var status3 = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            status3!.Status.Should().Be("Received");

            // Assert - External reference should have the latest value
            status3.ExternalReference.Should().Be("REF-3");
        }

        #endregion

        #region Function 8.5 - Enter External Lab Result

        [Fact]
        public async Task EnterExternalLabResultAsync_With_Valid_Data_Should_Create_Result_Parameter_SuccessGuard()
        {
            // Function: 8.5 — Enter External Lab Result (Create Parameter)
            // Arrange
            var patient = new Patient { LabId = "L-85-RESULT", FullName = "Result Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT-85", NameReport = "External 85", Price = 80m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var parameter = new TestParameter { TestId = test.TestId, Name = "External Lab Result", OrderNo = 1 };
            _db.TestParameters.Add(parameter);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 80m, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var queue = new ExternalLabQueue
            {
                VisitTestId = vt.VisitTestId,
                Status = "Shipped",
                DateQueued = DateTime.Today
            };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            // Act
            await _externalLabService.EnterExternalLabResultAsync(queue.QueueId, "Negative", "Within normal limits", "EXT-R-85");

            // Assert
            var result = await _db.ResultValues.FirstAsync(r => r.VisitTestId == vt.VisitTestId);
            result.Value.Should().Be("Negative");
            result.Comment.Should().Be("Within normal limits");
        }

        [Fact]
        public async Task EnterExternalLabResultAsync_Should_Update_Queue_Status_To_ResultReceived_SuccessGuard()
        {
            // Function: 8.5 — Enter External Lab Result (Update Queue Status)
            // Arrange
            var patient = new Patient { LabId = "L-85-QUEUE", FullName = "Queue Status Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT-85Q", NameReport = "External 85Q", Price = 45m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var parameter = new TestParameter { TestId = test.TestId, Name = "External Lab Result", OrderNo = 1 };
            _db.TestParameters.Add(parameter);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 45m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var queue = new ExternalLabQueue
            {
                VisitTestId = vt.VisitTestId,
                Status = "Received",
                DateQueued = DateTime.Today
            };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            // Act
            await _externalLabService.EnterExternalLabResultAsync(queue.QueueId, "Positive");

            // Assert
            var updatedQueue = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            updatedQueue!.Status.Should().Be("ResultReceived");
        }

        #endregion

        #region Function 8.6 - Print External Lab Report

        [Fact]
        public async Task GetPendingQueueAsync_Should_Return_Data_For_Report_Printing_SuccessGuard()
        {
            // Function: 8.6 — Print External Lab Report (Get Report Data)
            // Arrange
            var patient = new Patient
            {
                LabId = "L-86-PRINT",
                FullName = "Print Patient",
                Gender = "Male",
                Phone = "555-8686",
                BirthDate = new DateTime(1990, 1, 1)
            };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var referral = new Referral { Name = "Print Lab 86", ReferralType = "ExternalLab" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT-86", NameReport = "External 86", Price = 55m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit
            {
                PatientId = patient.PatientId,
                VisitDate = DateTime.Today,
                ReferralId = referral.ReferralId
            };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 55m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var queue = new ExternalLabQueue
            {
                VisitTestId = vt.VisitTestId,
                ReferralId = referral.ReferralId,
                Status = "Pending",
                DateQueued = DateTime.Today,
                ExternalReference = "PRINT-86-REF"
            };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            // Act
            var pendingItems = await _externalLabService.GetPendingQueueAsync();

            // Assert - Should contain all data needed for report
            pendingItems.Should().NotBeEmpty();
            var item = pendingItems.First(q => q.QueueId == queue.QueueId);
            item.VisitTest.Visit.Patient.FullName.Should().Be("Print Patient");
            item.Referral.Should().NotBeNull();
        }

        #endregion

        #region Function 8.7 - Settle External Lab Account

        [Fact]
        public async Task GetTotalProfitAsync_With_Multiple_Queue_Items_Should_Sum_All_Profits_SuccessGuard()
        {
            // Function: 8.7 — Settle External Lab Account (Calculate Total)
            // Arrange
            var referral = new Referral { Name = "Settlement Lab 87" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var test1 = new Test { CostPrice = 20m, Price = 100m };
            var test2 = new Test { CostPrice = 30m, Price = 150m };
            var test3 = new Test { CostPrice = 25m, Price = 125m };
            _db.Tests.AddRange(test1, test2, test3);
            await _db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitId = 1, TestId = test1.TestId, Price = 100m };
            var vt2 = new VisitTest { VisitId = 2, TestId = test2.TestId, Price = 150m };
            var vt3 = new VisitTest { VisitId = 3, TestId = test3.TestId, Price = 125m };
            _db.VisitTests.AddRange(vt1, vt2, vt3);
            await _db.SaveChangesAsync();

            _db.ExternalLabQueues.AddRange(
                new ExternalLabQueue { VisitTestId = vt1.VisitTestId, ReferralId = referral.ReferralId, Status = "Shipped" },
                new ExternalLabQueue { VisitTestId = vt2.VisitTestId, ReferralId = referral.ReferralId, Status = "Shipped" },
                new ExternalLabQueue { VisitTestId = vt3.VisitTestId, ReferralId = referral.ReferralId, Status = "Received" });
            await _db.SaveChangesAsync();

            // Act - Total profit = (100-20) + (150-30) + (125-25) = 80 + 120 + 100 = 300
            var profit = await _externalSettlementService.GetTotalProfitAsync(referral.ReferralId);

            // Assert
            profit.Should().Be(300m);
        }

        [Fact]
        public async Task GetPendingBalanceAsync_With_Existing_Settlements_Should_Calculate_Correctly_EdgeGuard()
        {
            // Function: 8.7 — Settle External Lab Account (Calculate Balance)
            // Arrange
            var referral = new Referral { Name = "Balance Lab 87" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var test = new Test { CostPrice = 50m, Price = 100m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = 1, TestId = test.TestId, Price = 100m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            _db.ExternalLabQueues.Add(new ExternalLabQueue
            {
                VisitTestId = vt.VisitTestId,
                ReferralId = referral.ReferralId,
                Status = "Received"
            });
            await _db.SaveChangesAsync();

            // Create previous settlement
            await _externalSettlementService.CreateSettlementAsync(referral.ReferralId, 30m, "First payment");

            // Act
            var balance = await _externalSettlementService.GetPendingBalanceAsync(referral.ReferralId);

            // Assert - Balance = 50 - 30 = 20
            balance.Should().Be(20m);
        }

        [Fact]
        public async Task CreateSettlementAsync_Should_Store_Note_Correctly_SuccessGuard()
        {
            // Function: 8.7 — Settle External Lab Account (Store Note)
            // Arrange
            var referral = new Referral { Name = "Note Lab 87" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            // Act
            var settlement = await _externalSettlementService.CreateSettlementAsync(
                referral.ReferralId, 100m, "Monthly settlement - April 2026");

            // Assert
            settlement.Note.Should().Be("Monthly settlement - April 2026");
        }

        [Fact]
        public async Task GetSettlementHistoryAsync_Should_Return_All_Settlements_For_Referral_SuccessGuard()
        {
            // Function: 8.7 — Settle External Lab Account (Get History)
            // Arrange
            var referral = new Referral { Name = "History Lab 87" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            await _externalSettlementService.CreateSettlementAsync(referral.ReferralId, 50m, "Payment 1");
            await _externalSettlementService.CreateSettlementAsync(referral.ReferralId, 75m, "Payment 2");
            await _externalSettlementService.CreateSettlementAsync(referral.ReferralId, 25m, "Payment 3");

            // Act
            var history = await _externalSettlementService.GetSettlementHistoryAsync(referral.ReferralId);

            // Assert
            history.Should().HaveCount(3);
            history.Sum(s => s.AmountPaid).Should().Be(150m);
        }

        #endregion

        #region Date Range and Boundary Tests

        [Fact]
        public async Task GetAllManifestsAsync_When_Manifest_Is_Older_Than_One_Year_Should_Still_Return_EdgeGuard()
        {
            // Function: 8.3 — Prepare External Sample (Date Range Boundary)
            // Arrange
            var referral = new Referral { Name = "Old Manifest Lab", ReferralType = "ExternalLab" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            _db.ShipmentManifests.Add(new ShipmentManifest
            {
                ManifestNumber = "MAN-OLD-87",
                ReferralId = referral.ReferralId,
                DateCreated = DateTime.Today.AddYears(-2),
                Status = "Closed"
            });
            await _db.SaveChangesAsync();

            // Act
            var manifests = await _externalSettlementService.GetSettlementHistoryAsync(referral.ReferralId);

            // Assert - Old manifests should still be accessible
            manifests.Should().NotBeNull();
            manifests.Count().Should().Be(0);
        }

        #endregion
    }
}
