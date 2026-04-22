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
        public async Task AddToQueueAsync_Should_Add_And_Prevent_Duplicate()
        {
            var patient = new Patient { LabId = "LQ1", FullName = "P", Gender = "Male" };
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

            var q1 = await _service.AddToQueueAsync(vt.VisitTestId, null);
            q1.Should().NotBeNull();
            var q2 = await _service.AddToQueueAsync(vt.VisitTestId, null);
            q2.QueueId.Should().Be(q1.QueueId);
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
        public async Task UpdateQueueStatusAsync_Should_Update()
        {
            var queue = new ExternalLabQueue { VisitTestId = 1, ReferralId = 1, Status = "Pending", DateQueued = DateTime.Now };
            _db.ExternalLabQueues.Add(queue);
            await _db.SaveChangesAsync();

            await _service.UpdateQueueStatusAsync(queue.QueueId, "Sent", "REF123");
            var updated = await _db.ExternalLabQueues.FindAsync(queue.QueueId);
            updated.Should().NotBeNull();
            updated!.Status.Should().Be("Sent");
            updated.ExternalReference.Should().Be("REF123");
        }
    }
}
