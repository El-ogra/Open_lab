using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class ServiceTestTemplateTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public ServiceTestTemplateTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateAsync_WithValidPatient_Should_CreatePatient_SuccessTemplate()
        {
            // Function: 1.1 — Add New Patient
            // Arrange
            var service = new PatientService(_db);
            var patient = TestSeed.Patient();

            // Act
            var created = await service.CreateAsync(patient);

            // Assert
            created.PatientId.Should().BeGreaterThan(0);
            created.LabId.Should().Be("LAB-001");
        }
    }
}

