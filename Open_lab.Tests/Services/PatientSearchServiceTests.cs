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
        public async Task SearchPatient_WithMatchingLabId_ShouldReturnPatient()
        {
            // Function: 1.5 — Search Patient
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
        public async Task SearchPatient_WhenNoMatch_ShouldReturnEmptyList()
        {
            // Function: 1.5 — Search Patient
            // Arrange
            _db.Patients.Add(new Patient { LabId = "LAB-010", FullName = "Patient A", Gender = "Male", Phone = "1111" });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.SearchPatientsAsync("DoesNotExist", "9999", "LAB-999");

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchPatient_WhenAllFiltersEmpty_ShouldReturnAllOrderedByName()
        {
            // Function: 1.5 — Search Patient
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
        public async Task SearchPatient_WithAdvancedFilters_ShouldFilterByDateRangeAgeGroupAndNationalId()
        {
            // Function: 1.5 — Advanced patient search
            // Arrange
            var child = new Patient { LabId = "LAB-C", FullName = "Child Patient", Gender = "Male", Age = 10, NationalId = "111" };
            var adult = new Patient { LabId = "LAB-A", FullName = "Adult Patient", Gender = "Female", Age = 33, NationalId = "222" };
            _db.Patients.AddRange(child, adult);
            await _db.SaveChangesAsync();

            _db.Visits.AddRange(
                new Visit { PatientId = child.PatientId, VisitDate = new DateTime(2026, 5, 1) },
                new Visit { PatientId = adult.PatientId, VisitDate = new DateTime(2026, 5, 10) });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.SearchPatientsAsync(new PatientSearchCriteria
            {
                NationalId = "222",
                DateFrom = new DateTime(2026, 5, 1),
                DateTo = new DateTime(2026, 5, 15),
                AgeGroup = "بالغين"
            });

            // Assert
            rows.Should().ContainSingle();
            rows[0].LabId.Should().Be("LAB-A");
        }

        [Fact]
        public async Task SearchPatient_WithDateRange_ShouldLimitToPatientsWithVisitsInsideRange()
        {
            // Function: 1.5 — Date range search
            // Arrange
            var inside = new Patient { LabId = "LAB-IN", FullName = "Inside", Gender = "Male", Age = 40 };
            var outside = new Patient { LabId = "LAB-OUT", FullName = "Outside", Gender = "Male", Age = 40 };
            _db.Patients.AddRange(inside, outside);
            await _db.SaveChangesAsync();

            _db.Visits.AddRange(
                new Visit { PatientId = inside.PatientId, VisitDate = new DateTime(2026, 1, 10) },
                new Visit { PatientId = outside.PatientId, VisitDate = new DateTime(2026, 2, 10) });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.SearchPatientsAsync(new PatientSearchCriteria
            {
                DateFrom = new DateTime(2026, 1, 1),
                DateTo = new DateTime(2026, 1, 31)
            });

            // Assert
            rows.Should().ContainSingle();
            rows[0].LabId.Should().Be("LAB-IN");
        }

        [Fact]
        public async Task ViewPatientHistory_WhenVisitsExist_ShouldReturnDescendingByVisitDate()
        {
            // Function: 1.6 — View Patient History
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
        public async Task ViewPatientHistory_WhenNoVisits_ShouldReturnEmptyList()
        {
            // Function: 1.6 — View Patient History
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
