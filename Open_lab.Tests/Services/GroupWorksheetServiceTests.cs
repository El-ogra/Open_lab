using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class GroupWorksheetServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly GroupWorksheetService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public GroupWorksheetServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new GroupWorksheetService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetGroupWorksheetByCustomGroupAsync_Should_Return_Visits_For_Custom_Group()
        {
            // Function: X.X — To Be Determined
            var customGroup = new CustomGroup { Name = "Package A" };
            var test = new Test { Code = "T1", NameReport = "CBC", Price = 10m };
            var patient = new Patient { LabId = "L1", FullName = "Patient 1", Gender = "Male" };
            _db.CustomGroups.Add(customGroup);
            _db.Tests.Add(test);
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.CustomGroupItems.Add(new CustomGroupItem { CustomGroupId = customGroup.CustomGroupId, TestId = test.TestId });

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m });
            await _db.SaveChangesAsync();

            var rows = await _service.GetGroupWorksheetByCustomGroupAsync(customGroup.CustomGroupId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            rows.Should().ContainSingle();
            rows[0].PatientName.Should().Be("Patient 1");
            rows[0].TestsCount.Should().Be(1);
        }

        [Fact]
        public async Task GetGroupWorksheetByGroupAsync_When_GroupHasNoTests_Should_Return_Empty_FailureGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var patient = new Patient { LabId = "L2", FullName = "Patient 2", Gender = "Female" };
            var otherGroupTest = new Test { Code = "T2", NameReport = "GLU", Price = 5m, GroupId = 99 };
            _db.Patients.Add(patient);
            _db.Tests.Add(otherGroupTest);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = otherGroupTest.TestId, Price = 5m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetGroupWorksheetByGroupAsync(1, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetGroupWorksheetByGroupAsync_Should_Return_Visits_For_Target_Group_Success()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var patient = new Patient { LabId = "L4", FullName = "Group Patient", Gender = "Female" };
            var targetTest = new Test { Code = "TG1", NameReport = "Target", Price = 11m, GroupId = 7 };
            var otherTest = new Test { Code = "OT1", NameReport = "Other", Price = 9m, GroupId = 8 };
            _db.Patients.Add(patient);
            _db.Tests.AddRange(targetTest, otherTest);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = targetTest.TestId, Price = 11m },
                new VisitTest { VisitId = visit.VisitId, TestId = otherTest.TestId, Price = 9m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetGroupWorksheetByGroupAsync(7, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
            rows[0].PatientName.Should().Be("Group Patient");
            rows[0].TestsCount.Should().Be(1);
        }

        [Fact]
        public async Task GetGroupWorksheetByCustomGroupAsync_When_VisitOutsideRange_Should_Return_Empty_EdgeGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var customGroup = new CustomGroup { Name = "Package B" };
            var test = new Test { Code = "T3", NameReport = "CRP", Price = 15m };
            var patient = new Patient { LabId = "L3", FullName = "Patient 3", Gender = "Male" };
            _db.CustomGroups.Add(customGroup);
            _db.Tests.Add(test);
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.CustomGroupItems.Add(new CustomGroupItem { CustomGroupId = customGroup.CustomGroupId, TestId = test.TestId });
            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-10) };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 15m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetGroupWorksheetByCustomGroupAsync(customGroup.CustomGroupId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetGroupWorksheetByCustomGroupAsync_When_CustomGroup_Not_Found_Should_Return_Empty_FailureGuard()
        {
            // Function: X.X — To Be Determined
            // Act
            var rows = await _service.GetGroupWorksheetByCustomGroupAsync(9999, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetGroupWorksheetByGroupAsync_When_Visit_Has_Multiple_Target_Tests_Should_Count_Only_Target_Tests_EdgeGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var patient = new Patient { LabId = "L5", FullName = "Edge Group", Gender = "Male" };
            var g1TestA = new Test { Code = "G1A", NameReport = "GroupA", Price = 10m, GroupId = 10 };
            var g1TestB = new Test { Code = "G1B", NameReport = "GroupB", Price = 12m, GroupId = 10 };
            var other = new Test { Code = "X1", NameReport = "Other", Price = 4m, GroupId = 11 };
            _db.Patients.Add(patient);
            _db.Tests.AddRange(g1TestA, g1TestB, other);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = g1TestA.TestId, Price = 10m },
                new VisitTest { VisitId = visit.VisitId, TestId = g1TestB.TestId, Price = 12m },
                new VisitTest { VisitId = visit.VisitId, TestId = other.TestId, Price = 4m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetGroupWorksheetByGroupAsync(10, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
            rows[0].TestsCount.Should().Be(2);
        }
    }
}
