using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests
{
    /// <summary>
    /// Additional comprehensive tests for Module 6: Sample Collection & Tracking
    /// Covers Functions 6.1-6.4 with Service and ViewModel layer tests
    /// </summary>
    public class Module6ServiceTests_Additional : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly SampleCollectionService _collectionService;
        private readonly SampleTrackingService _trackingService;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module6ServiceTests_Additional()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _collectionService = new SampleCollectionService(_db);
            _trackingService = new SampleTrackingService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        #region Function 6.1 - Register Sample Collection

        [Fact]
        public async Task MarkCollectedAsync_With_Valid_UserId_Should_Persist_UserId_SuccessGuard()
        {
            // Function: 6.1 — Register Sample Collection (User ID Persistence)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-USER1", "User Test");
            var userId = 99;

            // Act
            await _collectionService.MarkCollectedAsync(vtId, userId, false, null);

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.CollectedBy.Should().Be(userId);
            sample.CollectedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task MarkCollectedAsync_With_Null_ReceivedBy_Should_Persist_Null_EdgeGuard()
        {
            // Function: 6.1 — Register Sample Collection (Null Received By Edge Case)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-EXT1", "External Test");
            var userId = 5;

            // Act
            await _collectionService.MarkCollectedAsync(vtId, userId, true, null);

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.IsExternalSample.Should().BeTrue();
            sample.ReceivedBy.Should().BeNull();
        }

        [Fact]
        public async Task MarkCollectedAsync_With_Valid_ReceivedBy_Should_Persist_ReceivedBy_SuccessGuard()
        {
            // Function: 6.1 — Register Sample Collection (Valid Received By)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-EXT2", "External Test 2");
            var userId = 5;
            var receivedById = 10;

            // Act
            await _collectionService.MarkCollectedAsync(vtId, userId, true, receivedById);

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.IsExternalSample.Should().BeTrue();
            sample.ReceivedBy.Should().Be(receivedById);
        }

        [Fact]
        public async Task MarkCollectedAsync_Duplicate_Should_Update_Existing_Sample_SuccessGuard()
        {
            // Function: 6.1 — Register Sample Collection (Update Duplicate)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-DUP", "Duplicate Test");

            // First collection
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);

            // Act - Collection again (should update)
            await _collectionService.MarkCollectedAsync(vtId, userId: 2, false, null);

            // Assert - Only one sample exists, updated with new user
            var samples = await _db.SampleCollections.Where(s => s.VisitTestId == vtId).ToListAsync();
            samples.Should().HaveCount(1);
            samples[0].CollectedBy.Should().Be(2);
        }

        #endregion

        #region Function 6.2 - Record Sample Separation

        [Fact]
        public async Task MarkSeparatedAsync_With_Different_Separation_Types_Should_Update_Status_SuccessGuard()
        {
            // Function: 6.2 — Record Sample Separation (Different Separation Types)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-SEP", "Separation Test");
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);

            var separationTypes = new[] { "Centrifuge", "Filtration", "Centrifugation", "Extraction" };
            foreach (var type in separationTypes)
            {
                // Act
                await _collectionService.MarkSeparatedAsync(vtId, type);

                // Assert
                var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
                sample.Should().NotBeNull();
                sample!.Status.Should().Contain(type);
                sample.IsSeparated.Should().BeTrue();
            }
        }

        [Fact]
        public async Task MarkSeparatedAsync_With_Empty_String_Should_Set_Default_Status_EdgeGuard()
        {
            // Function: 6.2 — Record Sample Separation (Empty Separation Type Edge)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-EMPTY", "Empty Test");
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);

            // Act
            await _collectionService.MarkSeparatedAsync(vtId, "");

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.IsSeparated.Should().BeTrue();
            sample.Status.Should().Be("مفصولة");
        }

        [Fact]
        public async Task MarkSeparatedAsync_With_Whitespace_Should_Set_Default_Status_EdgeGuard()
        {
            // Function: 6.2 — Record Sample Separation (Whitespace Edge Case)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-WSPACE", "Whitespace Test");
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);

            // Act
            await _collectionService.MarkSeparatedAsync(vtId, "   ");

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.IsSeparated.Should().BeTrue();
            sample.Status.Should().Be("مفصولة");
        }

        [Fact]
        public async Task MarkSeparatedAsync_ReSeparation_Should_Update_Status_SuccessGuard()
        {
            // Function: 6.2 — Record Sample Separation (Re-Separation Logic)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-RSEP", "Re-Separation Test");
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);
            await _collectionService.MarkSeparatedAsync(vtId, "Centrifuge");

            // Act - Re-separate with different type
            await _collectionService.MarkSeparatedAsync(vtId, "Filtration");

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.Status.Should().Contain("Filtration");
            sample.IsSeparated.Should().BeTrue();
        }

        #endregion

        #region Function 6.3 - Track Sample Status

        [Fact]
        public async Task UpdateSeparationStatusAsync_With_True_Should_Set_IsSeparated_And_Status_SuccessGuard()
        {
            // Function: 6.3 — Track Sample Status (Set Separated State)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-TRK1", "Track Test 1");
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);

            // Act
            await _trackingService.UpdateSeparationStatusAsync(vtId, true);

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.IsSeparated.Should().BeTrue();
            sample.Status.Should().Be("مفصولة");
        }

        [Fact]
        public async Task UpdateSeparationStatusAsync_With_False_Should_Reset_IsSeparated_And_Status_SuccessGuard()
        {
            // Function: 6.3 — Track Sample Status (Reset Separated State)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-TRK2", "Track Test 2");
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);
            await _collectionService.MarkSeparatedAsync(vtId, "Centrifuge");

            // Act
            await _trackingService.UpdateSeparationStatusAsync(vtId, false);

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.IsSeparated.Should().BeFalse();
            sample.Status.Should().Be("مسحوبة");
        }

        [Fact]
        public async Task GetSampleStatusAsync_With_NonExistent_Id_Should_Return_Null_EdgeGuard()
        {
            // Function: 6.3 — Track Sample Status (Non-Existent Sample)
            // Act
            var result = await _trackingService.GetSampleStatusAsync(99999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPendingTrackingSamplesAsync_Should_Return_Only_Non_Separated_Samples_SuccessGuard()
        {
            // Function: 6.3 — Track Sample Status (Pending Samples Filter)
            // Arrange
            var (vtId1, _) = await SeedVisitTestAsync("L-PND1", "Pending 1");
            var (vtId2, _) = await SeedVisitTestAsync("L-PND2", "Pending 2");
            var (vtId3, _) = await SeedVisitTestAsync("L-PND3", "Pending 3");

            await _collectionService.MarkCollectedAsync(vtId1, userId: 1, false, null);
            await _collectionService.MarkCollectedAsync(vtId2, userId: 1, false, null);
            await _collectionService.MarkCollectedAsync(vtId3, userId: 1, false, null);

            await _collectionService.MarkSeparatedAsync(vtId1, "Centrifuge");

            // Act
            var pending = await _trackingService.GetPendingTrackingSamplesAsync();

            // Assert
            pending.Should().HaveCount(2);
            pending.Should().NotContain(s => s.VisitTestId == vtId1);
            pending.Should().Contain(s => s.VisitTestId == vtId2);
            pending.Should().Contain(s => s.VisitTestId == vtId3);
        }

        [Fact]
        public async Task GetPendingTrackingSamplesAsync_With_All_Separated_Should_Return_Empty_EdgeGuard()
        {
            // Function: 6.3 — Track Sample Status (All Separated Edge Case)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-ALLSEP", "All Separated");
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);
            await _collectionService.MarkSeparatedAsync(vtId, "Centrifuge");

            // Act
            var pending = await _trackingService.GetPendingTrackingSamplesAsync();

            // Assert
            pending.Should().BeEmpty();
        }

        [Fact]
        public async Task CompleteWorkflow_Created_Collected_Separated_Verified_Should_Work_SuccessGuard()
        {
            // Function: 6.3 — Track Sample Status (Complete Workflow)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-WF", "Workflow Test");

            // Act - Complete workflow
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);

            // Verify Collected state
            var collectedSample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            collectedSample!.Status.Should().Be("مسحوبة");

            await _collectionService.MarkSeparatedAsync(vtId, "Centrifuge");

            // Verify Separated state
            var separatedSample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            separatedSample!.Status.Should().Contain("Centrifuge");
            separatedSample.IsSeparated.Should().BeTrue();
        }

        #endregion

        #region Function 6.4 - Mark Taken Outside Lab

        [Fact]
        public async Task MarkCollectedAsync_External_With_Null_ReceivedBy_Should_Set_IsExternal_True_SuccessGuard()
        {
            // Function: 6.4 — Mark Taken Outside Lab (External Without ReceivedBy)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-EXTNULL", "External Null");

            // Act
            await _collectionService.MarkCollectedAsync(vtId, userId: 5, isExternal: true, receivedBy: null);

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.IsExternalSample.Should().BeTrue();
            sample.ReceivedBy.Should().BeNull();
        }

        [Fact]
        public async Task MarkCollectedAsync_External_With_ReceivedBy_Should_Set_All_Fields_Correctly_SuccessGuard()
        {
            // Function: 6.4 — Mark Taken Outside Lab (Full External Fields)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-EXTFULL", "External Full");
            var collectedById = 3;
            var receivedById = 7;

            // Act
            await _collectionService.MarkCollectedAsync(vtId, collectedById, isExternal: true, receivedBy: receivedById);

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().NotBeNull();
            sample!.IsExternalSample.Should().BeTrue();
            sample.CollectedBy.Should().Be(collectedById);
            sample.ReceivedBy.Should().Be(receivedById);
            sample.Status.Should().Be("مسحوبة");
        }

        [Fact]
        public async Task MarkCollectedAsync_External_With_NonExistent_VisitTest_Should_Throw_FailureGuard()
        {
            // Function: 6.4 — Mark Taken Outside Lab (Non-Existent VisitTest)
            // Act
            Func<Task> act = async () => await _collectionService.MarkCollectedAsync(88888, userId: 1, isExternal: true, receivedBy: 2);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task MarkNotCollectedAsync_With_Existing_Sample_Should_Remove_Sample_SuccessGuard()
        {
            // Function: 6.1 — Register Sample Collection (Cancel Collection)
            // Arrange
            var (vtId, _) = await SeedVisitTestAsync("L-CANCEL", "Cancel Test");
            await _collectionService.MarkCollectedAsync(vtId, userId: 1, false, null);

            // Act
            await _collectionService.MarkNotCollectedAsync(vtId);

            // Assert
            var sample = await _db.SampleCollections.FirstOrDefaultAsync(s => s.VisitTestId == vtId);
            sample.Should().BeNull();
        }

        [Fact]
        public async Task MarkNotCollectedAsync_With_NonExistent_Sample_Should_NotThrow_EdgeGuard()
        {
            // Function: 6.1 — Register Sample Collection (Cancel Non-Existent)
            // Act
            Func<Task> act = async () => await _collectionService.MarkNotCollectedAsync(77777);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region Helper Methods

        private async Task<(int VisitTestId, int PatientId)> SeedVisitTestAsync(string labId, string patientName)
        {
            var patient = new Patient { LabId = labId, FullName = patientName, Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = $"T-{Guid.NewGuid():N}".Substring(0, 10), NameReport = "Test", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 10m };
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            return (visitTest.VisitTestId, patient.PatientId);
        }

        #endregion
    }
}
