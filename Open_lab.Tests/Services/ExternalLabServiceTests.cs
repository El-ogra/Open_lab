using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class ExternalLabServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly ExternalLabService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public ExternalLabServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new ExternalLabService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task AddToQueueAsync_Should_Add_And_Prevent_Duplicate_LogicGuard()
        {
            // Function: 8.5 — Enter External Lab Result
            // Refactored to Logic Guard - verifies queue creation and duplicate prevention
            var patient = new Patient { LabId = "LQ1", FullName = "P", Gender = "Male", Phone = "555-2222" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT", NameReport = "Ext Test", Price = 5m, IsSendOut = true };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act - First add
            var q1 = await _service.AddToQueueAsync(vt.VisitTestId, null);
            
            // Assert - Logic Guard: Verify queue is created with correct data
            q1.Should().NotBeNull();
            q1.QueueId.Should().BeGreaterThan(0);
            q1.VisitTestId.Should().Be(vt.VisitTestId);
            q1.Status.Should().Be("Pending");
            q1.DateQueued.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));

            // Act - Second add (duplicate prevention)
            var q2 = await _service.AddToQueueAsync(vt.VisitTestId, null);
            
            // Assert - Logic Guard: Verify duplicate returns same queue ID
            q2.QueueId.Should().Be(q1.QueueId, "Duplicate should return existing queue ID");
            
            // Assert - Logic Guard: Verify only one queue entry exists
            var allQueues = await _db.ExternalLabQueues.Where(q => q.VisitTestId == vt.VisitTestId).ToListAsync();
            allQueues.Should().ContainSingle("Should have only one queue entry");
        }

        [Fact]
        public async Task AddToQueueAsync_When_VisitTest_Is_Missing_Should_Throw()
        {
            // Function: 8.5 — Enter External Lab Result
            Func<Task> act = async () => await _service.AddToQueueAsync(404, 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Visit test not found.");
        }

        [Fact]
        public async Task CreateManifestAsync_Should_Create_Manifest_And_Items()
        {
            // Function: 8.5 — Enter External Lab Result
            var patient = new Patient { LabId = "LM1", FullName = "P2", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT2", NameReport = "Ext2", Price = 5m, IsSendOut = true };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var queue = new ExternalLabQueue { VisitTestId = vt.VisitTestId, ReferralId = 1, Status = "Pending", DateQueued = DateTime.Now };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            var manifest = await _service.CreateManifestAsync(1, new List<int> { queue.QueueId }, "notes");
            manifest.Should().NotBeNull();
            manifest!.Items.Should().ContainSingle();

            var updatedQueue = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            updatedQueue.Should().NotBeNull();
            updatedQueue!.Status.Should().Be("InManifest");
        }

        [Fact]
        public async Task CreateManifestAsync_With_Empty_QueueIds_Should_Create_Manifest_Without_Items_Edge()
        {
            // Function: 8.5 — Enter External Lab Result
            // Act
            var manifest = await _service.CreateManifestAsync(7, new List<int>(), "empty");

            // Assert
            manifest.Should().NotBeNull();
            manifest.ReferralId.Should().Be(7);
            (await _db.ShipmentItems.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CreateManifestAsync_With_Null_QueueIds_Should_Throw_Failure()
        {
            // Function: 8.5 — Enter External Lab Result
            Func<Task> act = async () => await _service.CreateManifestAsync(1, null!, "notes");

            await act.Should().ThrowAsync<NullReferenceException>();
        }

        [Fact]
        public async Task UpdateQueueStatusAsync_Should_Update_LogicGuard()
        {
            // Function: 8.5 — Enter External Lab Result
            // Refactored to Logic Guard - verifies status update with complete data validation
            var queue = new ExternalLabQueue { VisitTestId = 1, ReferralId = 1, Status = "Pending", DateQueued = DateTime.Now };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            // Act
            await _service.UpdateQueueStatusAsync(queue.QueueId, "Sent", "REF123");
            
            var updated = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            
            // Assert - Logic Guard: Verify complete status update
            updated.Should().NotBeNull();
            updated!.Status.Should().Be("Sent");
            updated.ExternalReference.Should().Be("REF123");
            updated.VisitTestId.Should().Be(1);
            updated.ReferralId.Should().Be(1);
            updated.DateQueued.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task GetPendingQueueAsync_Should_Return_Pending_Items()
        {
            // Function: 8.5 — Enter External Lab Result
            // Arrange
            var patient = new Patient { FullName = "P", LabId = "L" };
            _db.Patients.Add(patient);
            var test = new Test { Code = "T", NameReport = "T", Price = 10 };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10 };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Explicitly set the navigation properties to help InMemoryDatabase if needed
            var q = new ExternalLabQueue 
            { 
                VisitTestId = vt.VisitTestId, 
                ReferralId = 5, 
                Status = "Pending", 
                DateQueued = DateTime.Now 
            };
            _db.ExternalLabQueues.Add(q);
            await _db.SaveChangesAsync();

            // Act
            var results = await _service.GetPendingQueueAsync();

            // Assert
            results.Should().NotBeEmpty();
            results.Any(r => r.QueueId == q.QueueId).Should().BeTrue();
            results[0].VisitTest.Visit.Patient.FullName.Should().Be("P");
        }

        [Fact]
        public async Task GetPendingQueueAsync_Should_Include_Referral_Details()
        {
            // Function: 8.5 — Enter External Lab Result
            var patient = new Patient { FullName = "P", LabId = "L", Gender = "Male" };
            var referral = new Referral { Name = "Ref Lab", ReferralType = "ExternalLab" };
            var test = new Test { Code = "TX", NameReport = "External Test", Price = 10m };
            _db.Patients.Add(patient);
            _db.Referrals.Add(referral);
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            _db.ExternalLabQueues.Add(new ExternalLabQueue
            {
                VisitTestId = visitTest.VisitTestId,
                ReferralId = referral.ReferralId,
                Status = "Pending",
                DateQueued = DateTime.Now
            });
            await _db.SaveChangesAsync();

            var queue = await _service.GetPendingQueueAsync();

            queue.Should().ContainSingle();
            queue[0].Referral.Should().NotBeNull();
            queue[0].Referral!.Name.Should().Be("Ref Lab");
        }

        [Fact]
        public async Task GetPendingQueueAsync_When_No_Pending_Items_Should_Return_Empty_Edge()
        {
            // Function: 8.5 — Enter External Lab Result
            // Arrange
            _db.ExternalLabQueues.Add(new ExternalLabQueue { VisitTestId = 1, Status = "Received", DateQueued = DateTime.Now });
            await _db.SaveChangesAsync();

            // Act
            var queue = await _service.GetPendingQueueAsync();

            // Assert
            queue.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateQueueStatusAsync_Should_Save_External_Result_Reference()
        {
            // Function: 8.5 — Enter External Lab Result
            var queue = new ExternalLabQueue { VisitTestId = 1, ReferralId = 2, Status = "Shipped", DateQueued = DateTime.Now };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            await _service.UpdateQueueStatusAsync(queue.QueueId, "Received", "EXT-RESULT-001");

            var updated = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            updated.Should().NotBeNull();
            updated!.Status.Should().Be("Received");
            updated.ExternalReference.Should().Be("EXT-RESULT-001");
        }

        [Fact]
        public async Task EnterExternalLabResultAsync_Should_Create_Result_And_Update_Queue_State()
        {
            // Function: 8.5 — Enter External Lab Result
            var patient = new Patient { LabId = "LR1", FullName = "P", Gender = "Male" };
            var test = new Test { Code = "EXT-R", NameReport = "External Result Test", Price = 25m };
            _db.Patients.Add(patient);
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 25m, Status = "Pending" };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            var queue = new ExternalLabQueue
            {
                VisitTestId = visitTest.VisitTestId,
                Status = "Sent",
                DateQueued = DateTime.Now
            };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            await _service.EnterExternalLabResultAsync(queue.QueueId, "Positive", "verified by ref lab", "EXT-77");

            var parameter = await _db.TestParameters.SingleAsync(p => p.TestId == test.TestId && p.Name == "External Lab Result");
            var result = await _db.ResultValues.SingleAsync(r => r.VisitTestId == visitTest.VisitTestId && r.ParameterId == parameter.ParameterId);
            var updatedQueue = await _db.ExternalLabQueues.SingleAsync(q => q.QueueId == queue.QueueId);
            var updatedVisitTest = await _db.VisitTests.SingleAsync(vt => vt.VisitTestId == visitTest.VisitTestId);

            result.Value.Should().Be("Positive");
            result.Comment.Should().Be("verified by ref lab");
            updatedQueue.Status.Should().Be("ResultReceived");
            updatedQueue.ExternalReference.Should().Be("EXT-77");
            updatedVisitTest.Status.Should().Be("InProgress");
        }

        [Fact]
        public async Task EnterExternalLabResultAsync_Should_Update_Existing_Result_Without_Duplicating()
        {
            // Function: 8.5 — Enter External Lab Result
            var patient = new Patient { LabId = "LR2", FullName = "P2", Gender = "Female" };
            var test = new Test { Code = "EXT-U", NameReport = "External Update", Price = 20m };
            _db.Patients.Add(patient);
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 20m };
            _db.VisitTests.Add(visitTest);

            var parameter = new TestParameter { TestId = test.TestId, Name = "External Lab Result", OrderNo = 1 };
            _db.TestParameters.Add(parameter);
            await _db.SaveChangesAsync();

            _db.ResultValues.Add(new ResultValue
            {
                VisitTestId = visitTest.VisitTestId,
                ParameterId = parameter.ParameterId,
                Value = "Old",
                Comment = "old comment",
                Flag = "H",
                VerifiedAt = DateTime.Now,
                VerifiedBy = 12
            });

            var queue = new ExternalLabQueue
            {
                VisitTestId = visitTest.VisitTestId,
                Status = "Received",
                DateQueued = DateTime.Now
            };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            await _service.EnterExternalLabResultAsync(queue.QueueId, "Updated", "new comment");

            var results = await _db.ResultValues.Where(r => r.VisitTestId == visitTest.VisitTestId).ToListAsync();
            results.Should().ContainSingle();
            results[0].Value.Should().Be("Updated");
            results[0].Comment.Should().Be("new comment");
            results[0].Flag.Should().BeNull();
            results[0].VerifiedAt.Should().BeNull();
            results[0].VerifiedBy.Should().BeNull();
        }

        [Fact]
        public async Task EnterExternalLabResultAsync_With_Empty_Result_Should_Throw_ArgumentException_Failure()
        {
            // Function: 8.5 — Enter External Lab Result
            // Act
            Func<Task> act = async () => await _service.EnterExternalLabResultAsync(1, "   ");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Result value is required*");
        }

        [Fact]
        public async Task EnterExternalLabResultAsync_With_Invalid_QueueId_Should_Throw_InvalidOperationException_Failure()
        {
            // Function: 8.5 — Enter External Lab Result
            // Act
            Func<Task> act = async () => await _service.EnterExternalLabResultAsync(9999, "OK");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*External queue item not found*");
        }

        [Fact]
        public async Task GetAllManifestsAsync_Should_Return_Manifests_Ordered_By_DateDescending_Success()
        {
            // Function: 8.5 — Enter External Lab Result
            // Arrange
            var referral = new Referral { Name = "Ref", ReferralType = "ExternalLab" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            _db.ShipmentManifests.AddRange(
                new ShipmentManifest { ManifestNumber = "MAN-1", ReferralId = referral.ReferralId, DateCreated = DateTime.Today.AddDays(-1), Status = "Open" },
                new ShipmentManifest { ManifestNumber = "MAN-2", ReferralId = referral.ReferralId, DateCreated = DateTime.Today, Status = "Open" });
            await _db.SaveChangesAsync();

            // Act
            var manifests = await _service.GetAllManifestsAsync();

            // Assert
            manifests.Should().HaveCount(2);
            manifests[0].ManifestNumber.Should().Be("MAN-2");
            manifests[1].ManifestNumber.Should().Be("MAN-1");
        }

        [Fact]
        public async Task UpdateQueueStatusAsync_When_Queue_NotFound_Should_Leave_Data_Unchanged_Edge()
        {
            // Function: 8.5 — Enter External Lab Result
            // Arrange
            (await _db.ExternalLabQueues.CountAsync()).Should().Be(0);

            // Act
            await _service.UpdateQueueStatusAsync(12345, "Received", "X");

            // Assert
            (await _db.ExternalLabQueues.CountAsync()).Should().Be(0);
        }


        // 8.6 External Lab Report Tests - NEW TEST

        [Fact]
        public async Task GetPendingQueueAsync_Should_Validate_Report_Data_Integrity_LogicGuard()
        {
            // Function: 8.6 — Print External Lab Report - Logic Guard: Verify report data integrity
            // Arrange
            var patient = new Patient { LabId = "L-EXT-REP", FullName = "External Report Patient", Gender = "Male", Phone = "555-9999" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var referral = new Referral { Name = "External Lab X", ReferralId = 10 };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "EXT-REP", NameReport = "External Test Report", Price = 15m, IsSendOut = true };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 15m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            var queue = new ExternalLabQueue 
            { 
                VisitTestId = vt.VisitTestId, 
                ReferralId = referral.ReferralId, 
                Status = "Pending", 
                DateQueued = DateTime.Now,
                ExternalReference = "EXT-REF-123"
            };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            // Act
            var results = await _service.GetPendingQueueAsync();

            // Assert - Logic Guard: Verify report data integrity
            results.Should().NotBeEmpty();
            var reportItem = results.FirstOrDefault(r => r.QueueId == queue.QueueId);
            reportItem.Should().NotBeNull();
            reportItem!.VisitTest.Visit.Patient.FullName.Should().Be("External Report Patient");
            reportItem.VisitTest.Visit.Patient.LabId.Should().Be("L-EXT-REP");
            reportItem.VisitTest.Visit.Patient.Phone.Should().Be("555-9999");
            reportItem.VisitTest.Test.NameReport.Should().Be("External Test Report");
            reportItem.Referral.Should().NotBeNull();
            reportItem.Referral!.Name.Should().Be("External Lab X");
            reportItem.Status.Should().Be("Pending");
            reportItem.ExternalReference.Should().Be("EXT-REF-123");
            reportItem.DateQueued.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }


        [Fact]
        public async Task GetAllManifestsAsync_When_None_Exist_Should_Return_Empty_Edge()
        {
            // Function: 8.6 — Print External Lab Report - Logic Guard: Verify report data integrity
            var manifests = await _service.GetAllManifestsAsync();

            manifests.Should().BeEmpty();
        }

    }
}
