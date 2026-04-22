using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class PatientServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly PatientService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public PatientServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new PatientService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Patient_And_Generate_LabId()
        {
            // Arrange
            var patient = new Patient
            {
                FullName = "John Doe",
                Gender = "Male"
            };

            // Act
            var created = await _service.CreateAsync(patient);

            // Assert
            created.PatientId.Should().BeGreaterThan(0);
            created.LabId.Should().NotBeNullOrWhiteSpace();
            (await _db.Patients.FindAsync(created.PatientId)).Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Invalid_Gender_Should_Throw()
        {
            // Arrange
            var patient = new Patient
            {
                FullName = "Jane",
                Gender = "Unknown"
            };

            // Act
            Func<Task> act = async () => await _service.CreateAsync(patient);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GenerateNextLabIdAsync_Should_Increment_Sequence()
        {
            // Arrange - seed a patient with today's prefix
            var today = DateTime.Today;
            var prefix = today.ToString("yyyyMMdd");
            var existing = new Patient { LabId = prefix + "001", FullName = "A", Gender = "Male" };
            _db.Patients.Add(existing);
            await _db.SaveChangesAsync();

            // Act
            var next = await _service.GenerateNextLabIdAsync(today);

            // Assert
            next.Should().StartWith(prefix);
            next.Should().EndWith("002");
        }

        [Fact]
        public async Task DeleteAsync_With_Visits_Should_Throw()
        {
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.Visits.Add(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.DeleteAsync(patient.PatientId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
