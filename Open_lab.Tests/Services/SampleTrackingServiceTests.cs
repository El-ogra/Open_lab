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
    }
}
