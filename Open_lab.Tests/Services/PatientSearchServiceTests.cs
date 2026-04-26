using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class PatientSearchServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly PatientSearchService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public PatientSearchServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new PatientSearchService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task SearchPatientsAsync_When_LabIdMatches_Should_Return_Patient_SuccessGuard()
        {
            // Arrange
            _db.Patients.AddRange(
                new Patient { LabId = "LAB-001", FullName = "Ahmed Ali", Gender = "Male" },
                new Patient { LabId = "LAB-002", FullName = "Sara Ali", Gender = "Female" });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.SearchPatientsAsync(null, null, "LAB-001");

            // Assert
            rows.Should().ContainSingle();
            rows[0].LabId.Should().Be("LAB-001");
        }

        [Fact]
        public async Task SearchPatientsAsync_When_NoPatientMatchesCriteria_Should_Return_Empty_FailureGuard()
        {
            // Arrange
            _db.Patients.Add(new Patient { LabId = "LAB-010", FullName = "Patient A", Gender = "Male", Phone = "1111" });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.SearchPatientsAsync("DoesNotExist", "9999", "LAB-999");

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchPatientsAsync_When_AllFiltersNull_Should_Return_AllPatientsOrdered_EdgeGuard()
        {
            // Arrange
            _db.Patients.AddRange(
                new Patient { LabId = "L2", FullName = "Zain", Gender = "Male" },
                new Patient { LabId = "L1", FullName = "Adam", Gender = "Male" });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.SearchPatientsAsync(null, null, null);

            // Assert
            rows.Should().HaveCount(2);
            rows[0].FullName.Should().Be("Adam");
            rows[1].FullName.Should().Be("Zain");
        }

        [Fact]
        public async Task GetPatientVisitsAsync_When_VisitsExist_Should_Return_DescendingByVisitDate_SuccessGuard()
        {
            // Arrange
            var patient = new Patient { LabId = "LAB-V", FullName = "Visit P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.Visits.AddRange(
                new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-2) },
                new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today });
            await _db.SaveChangesAsync();

            // Act
            var visits = await _service.GetPatientVisitsAsync(patient.PatientId);

            // Assert
            visits.Should().HaveCount(2);
            visits[0].VisitDate.Should().BeAfter(visits[1].VisitDate);
        }

        [Fact]
        public async Task GetPatientVisitsAsync_When_PatientHasNoVisits_Should_Return_Empty_EdgeGuard()
        {
            // Arrange
            var patient = new Patient { LabId = "LAB-NOVISIT", FullName = "No Visit", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            // Act
            var visits = await _service.GetPatientVisitsAsync(patient.PatientId);

            // Assert
            visits.Should().BeEmpty();
        }
    }
}
