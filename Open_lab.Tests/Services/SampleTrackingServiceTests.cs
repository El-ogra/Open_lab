using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class SampleTrackingServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly SampleTrackingService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public SampleTrackingServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new SampleTrackingService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task UpdateSeparationStatusAsync_Should_Keep_Status_Aligned_With_Tracking_State()
        {
            _db.SampleCollections.Add(new SampleCollection
            {
                VisitTestId = 11,
                CollectedBy = 1,
                CollectedAt = DateTime.Now,
                Status = "مسحوبة",
                IsSeparated = false
            });
            await _db.SaveChangesAsync();

            await _service.UpdateSeparationStatusAsync(11, true);

            var separated = await _db.SampleCollections.SingleAsync(s => s.VisitTestId == 11);
            separated.IsSeparated.Should().BeTrue();
            separated.Status.Should().Be("مفصولة");

            await _service.UpdateSeparationStatusAsync(11, false);

            var collected = await _db.SampleCollections.SingleAsync(s => s.VisitTestId == 11);
            collected.IsSeparated.Should().BeFalse();
            collected.Status.Should().Be("مسحوبة");
        }

        [Fact]
        public async Task UpdateSeparationStatusAsync_WithNonExistentSample_Should_ThrowException_FailureGuard()
        {
            // Arrange
            var invalidId = 9999;
            
            // Act
            Func<Task> act = async () => await _service.UpdateSeparationStatusAsync(invalidId, true);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*لم يتم العثور على سجل العينة المطلوب*");
        }

        [Fact]
        public async Task GetSampleStatusAsync_Should_ReturnSample_IfExists_LogicGuard()
        {
            // Arrange
            _db.Users.Add(new User { UserId = 1, Username = "TestUser", PasswordHash = "hash" });
            _db.SampleCollections.Add(new SampleCollection
            {
                VisitTestId = 22,
                CollectedBy = 1,
                CollectedAt = DateTime.Now,
                Status = "مسحوبة",
                IsSeparated = false
            });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetSampleStatusAsync(22);

            // Assert
            result.Should().NotBeNull();
            result!.VisitTestId.Should().Be(22);
            result.Status.Should().Be("مسحوبة");
        }

        [Fact]
        public async Task GetPendingTrackingSamplesAsync_Should_Return_OnlyPending_LogicGuard()
        {
            // Arrange
            var patient = new Patient { LabId = "P1", FullName = "Pending Test Patient" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "TEST1", NameReport = "T1", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt1 = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m }; // Pending
            var vt2 = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m }; // Completed
            _db.VisitTests.AddRange(vt1, vt2);
            await _db.SaveChangesAsync();

            _db.SampleCollections.Add(new SampleCollection
            {
                VisitTestId = vt1.VisitTestId,
                CollectedBy = 1,
                CollectedAt = DateTime.Now,
                Status = "مسحوبة",
                IsSeparated = false // This makes it pending
            });

            _db.SampleCollections.Add(new SampleCollection
            {
                VisitTestId = vt2.VisitTestId,
                CollectedBy = 1,
                CollectedAt = DateTime.Now.AddMinutes(-10),
                Status = "Verified",
                IsSeparated = true // Completed
            });
            await _db.SaveChangesAsync();

            // Act
            var pendingSamples = await _service.GetPendingTrackingSamplesAsync();

            // Assert
            pendingSamples.Should().NotBeEmpty();
            pendingSamples.Should().Contain(s => s.VisitTestId == vt1.VisitTestId);
            pendingSamples.Should().NotContain(s => s.VisitTestId == vt2.VisitTestId);
        }
    }
}
