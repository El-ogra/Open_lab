using System;
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
    public class SampleCollectionServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly SampleCollectionService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public SampleCollectionServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new SampleCollectionService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetRowsAsync_Should_Return_Rows_LogicGuard()
        {
            // Refactored to Logic Guard - verifies complete row data and filtering
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male", Phone = "555-1111" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T1", NameReport = "Test", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetRowsAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Logic Guard: Verify row contains complete patient and test data
            rows.Should().ContainSingle();
            var row = rows.Single();
            row.VisitTestId.Should().Be(vt.VisitTestId);
            row.PatientName.Should().Be("P");
            row.TestName.Should().Be("Test");
            row.VisitDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task MarkCollectedAsync_Should_Create_SampleCollection_LogicGuard()
        {
            // Refactored to Logic Guard - verifies all fields and side effects
            var patient = new Patient { LabId = "L2", FullName = "P2", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T2", NameReport = "Test2", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            await _service.MarkCollectedAsync(vt.VisitTestId, userId: 7, isExternal: false, receivedBy: null);

            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId);
            sample.Should().NotBeNull();
            sample!.Status.Should().Be("مسحوبة");
            sample.CollectedBy.Should().Be(7);
            sample.IsExternalSample.Should().BeFalse();
            sample.ReceivedBy.Should().BeNull();
            sample.CollectedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task MarkSeparatedAsync_Without_Sample_Should_Throw_LogicGuard()
        {
            // Refactored to Logic Guard - verifies exception message and side effect
            Func<Task> act = async () => await _service.MarkSeparatedAsync(999, "Centrifuge");
            
            // Assert - Logic Guard: Verify exception is thrown with appropriate message
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*يجب تسجيل السحب أولاً*");
            
            // Assert - Logic Guard: Verify no sample was created
            var samples = await _db.SampleCollections.ToListAsync();
            samples.Should().BeEmpty("No sample should be created when operation fails");
        }

        [Fact]
        public async Task MarkNotCollectedAsync_Should_Remove_Sample()
        {
            var patient = new Patient { LabId = "L3", FullName = "P3", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T3", NameReport = "Test3", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            _db.SampleCollections.Add(new SampleCollection { VisitTestId = vt.VisitTestId, CollectedBy = 1, CollectedAt = DateTime.Now, Status = "مسحوبة" });
            await _db.SaveChangesAsync();

            await _service.MarkNotCollectedAsync(vt.VisitTestId);
            (await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId)).Should().BeNull();
        }

        [Fact]
        public async Task GetRowsAsync_With_Inverted_Date_Range_Should_Return_Empty_FailureGuard()
        {
            // Arrange
            var patient = new Patient { LabId = "L-INV", FullName = "Inverted", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "INV", NameReport = "InvertedRange", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetRowsAsync(DateTime.Today.AddDays(1), DateTime.Today.AddDays(-1));

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task MarkSeparatedAsync_With_Whitespace_SeparationType_Should_Set_Default_Separated_Status_Edge()
        {
            // Arrange
            _db.SampleCollections.Add(new SampleCollection
            {
                VisitTestId = 55,
                CollectedBy = 1,
                CollectedAt = DateTime.Now,
                Status = "مسحوبة"
            });
            await _db.SaveChangesAsync();

            // Act
            await _service.MarkSeparatedAsync(55, "   ");

            // Assert
            var sample = await _db.SampleCollections.SingleAsync(s => s.VisitTestId == 55);
            sample.IsSeparated.Should().BeTrue();
            sample.Status.Should().Be("مفصولة");
        }

        [Fact]
        public async Task MarkNotCollectedAsync_When_Sample_Missing_Should_Keep_State_Unchanged_Edge()
        {
            // Arrange
            (await _db.SampleCollections.CountAsync()).Should().Be(0);

            // Act
            await _service.MarkNotCollectedAsync(404);

            // Assert
            (await _db.SampleCollections.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task MarkSeparatedAsync_Should_Update_Status_And_IsSeparated()
        {
            // Arrange
            var vtId = 1;
            _db.SampleCollections.Add(new SampleCollection { VisitTestId = vtId, Status = "مسحوبة", CollectedAt = DateTime.Now });
            await _db.SaveChangesAsync();

            // Act
            await _service.MarkSeparatedAsync(vtId, "Centrifuge");

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample!.Status.Should().Be("مفصولة - Centrifuge");
            sample.IsSeparated.Should().BeTrue();
        }

        // 6.4 - External Sample Flagging - State Flagging Tests
        [Fact]
        public async Task MarkCollectedAsync_WithExternalFlag_Should_Set_IsExternalSample_True()
        {
            // Arrange
            var patient = new Patient { LabId = "LEXT1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T4", NameReport = "Test4", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            await _service.MarkCollectedAsync(vt.VisitTestId, userId: 7, isExternal: true, receivedBy: 8);

            // Assert - Logic Guard: Verify external flag is set correctly
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId);
            sample.Should().NotBeNull();
            sample!.IsExternalSample.Should().BeTrue();
            sample.ReceivedBy.Should().Be(8);
            sample.CollectedBy.Should().Be(7);
            sample.Status.Should().Be("مسحوبة");
        }

        [Fact]
        public async Task MarkCollectedAsync_WithoutExternalFlag_Should_Set_IsExternalSample_False()
        {
            // Arrange
            var patient = new Patient { LabId = "LEXT2", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T5", NameReport = "Test5", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            await _service.MarkCollectedAsync(vt.VisitTestId, userId: 9, isExternal: false, receivedBy: null);

            // Assert - Logic Guard: Verify external flag is false for internal samples
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId);
            sample.Should().NotBeNull();
            sample!.IsExternalSample.Should().BeFalse();
            sample.ReceivedBy.Should().BeNull();
            sample.CollectedBy.Should().Be(9);
        }

        [Fact]
        public async Task MarkCollectedAsync_UpdatingExisting_Should_Update_ExternalFlag()
        {
            // Arrange
            var patient = new Patient { LabId = "LEXT3", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T6", NameReport = "Test6", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // First mark as internal
            await _service.MarkCollectedAsync(vt.VisitTestId, userId: 1, isExternal: false, receivedBy: null);

            // Act - Update to external
            await _service.MarkCollectedAsync(vt.VisitTestId, userId: 2, isExternal: true, receivedBy: 3);

            // Assert - Logic Guard: Verify external flag is updated
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId);
            sample.Should().NotBeNull();
            sample!.IsExternalSample.Should().BeTrue();
            sample.ReceivedBy.Should().Be(3);
            sample.CollectedBy.Should().Be(2);
        }

        [Fact]
        public async Task MarkCollectedAsync_NonExistentVisitTest_Should_Throw()
        {
            // Act
            Func<Task> act = async () => await _service.MarkCollectedAsync(99999, userId: 1, isExternal: false, receivedBy: null);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*العنصر غير موجود*");
        }

        // 6.3 Sample Workflow State Transition Tests - NEW TESTS

        [Fact]
        public async Task CompleteStateTransition_Collected_To_Separated_Should_Update_Status_LogicGuard()
        {
            // Function: 6.3 — State Transition: Collected → Separated
            // Arrange
            var patient = new Patient { LabId = "L-ST1", FullName = "State Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "ST1", NameReport = "State Test", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act - Mark as Collected
            await _service.MarkCollectedAsync(vt.VisitTestId, userId: 10, isExternal: false, receivedBy: null);
            
            var collectedSample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId);
            collectedSample.Should().NotBeNull();
            collectedSample!.Status.Should().Be("مسحوبة");

            // Act - Mark as Separated
            await _service.MarkSeparatedAsync(vt.VisitTestId, "Centrifuge");

            // Assert - Logic Guard: Verify complete state transition
            var separatedSample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId);
            separatedSample.Should().NotBeNull();
            separatedSample!.Status.Should().Be("مفصولة - Centrifuge");
            separatedSample.IsSeparated.Should().BeTrue();
            separatedSample.CollectedBy.Should().Be(10);
        }

        [Fact]
        public async Task StateTransition_Invalid_Skip_Should_Fail_LogicGuard()
        {
            // Function: 6.3 — Break Case: Attempt to skip Collected state and go directly to Separated
            // Arrange
            var patient = new Patient { LabId = "L-ST2", FullName = "Skip Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "ST2", NameReport = "Skip Test", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 5m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act - Attempt to mark as Separated without Collected state
            Func<Task> act = async () => await _service.MarkSeparatedAsync(vt.VisitTestId, "Centrifuge");

            // Assert - Logic Guard: Verify invalid state transition is rejected
            await act.Should().ThrowAsync<InvalidOperationException>();
            
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vt.VisitTestId);
            sample.Should().BeNull("Sample should not be created when state is skipped");
        }
    }
}
