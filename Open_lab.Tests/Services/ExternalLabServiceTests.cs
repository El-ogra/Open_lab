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
        public async Task CreateManifestAsync_Should_Create_Manifest_And_Items()
        {
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
        public async Task UpdateQueueStatusAsync_Should_Update_LogicGuard()
        {
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
        public async Task UpdateQueueStatusAsync_Should_Save_External_Result_Reference()
        {
            // 8.5 Enter External Lab Result
            var queue = new ExternalLabQueue { VisitTestId = 1, ReferralId = 2, Status = "Shipped", DateQueued = DateTime.Now };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            await _service.UpdateQueueStatusAsync(queue.QueueId, "Received", "EXT-RESULT-001");

            var updated = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            updated.Should().NotBeNull();
            updated!.Status.Should().Be("Received");
            updated.ExternalReference.Should().Be("EXT-RESULT-001");
        }

        // 8.6 External Lab Report Tests - NEW TEST

        [Fact]
        public async Task GetPendingQueueAsync_Should_Validate_Report_Data_Integrity_LogicGuard()
        {
            // 8.6 Print External Lab Report - Logic Guard: Verify report data integrity
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
    }
}
