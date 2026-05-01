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
    public class Module3ServiceCompletionTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly TestCatalogService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module3ServiceCompletionTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new TestCatalogService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.1 / 3.2 — Edit Test Data & Parameters
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateTest_WithValidData_ShouldPersistAllFields_SuccessGuard()
        {
            // Function: 3.2 — Edit Test Data
            // Arrange
            var test = new Test { Code = "T1", NameReport = "Old", NameReceipt = "Old", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            test.NameReport = "Updated Name";
            test.TurnaroundHours = 48;
            test.IsRoutine = true;

            // Act
            await _service.UpdateTestAsync(test);

            // Assert
            var updated = await _db.Tests.FindAsync(test.TestId);
            updated.Should().NotBeNull();
            updated!.NameReport.Should().Be("Updated Name");
            updated.TurnaroundHours.Should().Be(48);
            updated.IsRoutine.Should().BeTrue();
        }

        [Fact]
        public async Task CreateTest_WithDuplicateCode_ShouldThrow_FailureGuard()
        {
            // Function: 3.1 — Add New Test (Failure: duplicate code)
            // Arrange
            _db.Tests.Add(new Test { Code = "DUP", NameReport = "T1", NameReceipt = "T1", Price = 1 });
            await _db.SaveChangesAsync();

            var newTest = new Test { Code = "DUP", NameReport = "T2", NameReceipt = "T2", Price = 1 };

            // Act
            Func<Task> act = async () => await _service.CreateTestAsync(newTest);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*exists*");
        }

        [Fact]
        public async Task UpdateTest_WithNonExistentId_ShouldThrow_FailureGuard()
        {
            // Function: 3.2 — Edit Test Data (Failure: not found)
            // Arrange
            var test = new Test { TestId = 9999, Code = "NF", NameReport = "NF", NameReceipt = "NF" };

            // Act
            Func<Task> act = async () => await _service.UpdateTestAsync(test);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task UpdateTest_WithNegativePrice_ShouldThrow_EdgeGuard()
        {
            // Function: 3.2 — Edit Test Data (Edge: negative price)
            // Arrange
            var test = new Test { Code = "NEG", NameReport = "N", NameReceipt = "N", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            test.Price = -5m;

            // Act
            Func<Task> act = async () => await _service.UpdateTestAsync(test);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*negative*");
        }

        [Fact]
        public async Task CreateParameter_Should_Persist_Correctly_SuccessGuard()
        {
            // Function: 3.1 — Add New Test (Parameters)
            // Arrange
            var test = new Test { Code = "TP1", NameReport = "Test", NameReceipt = "Test", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var param = new TestParameter { TestId = test.TestId, Name = "Glucose", OrderNo = 1 };

            // Act
            var created = await _service.CreateParameterAsync(param);

            // Assert
            created.ParameterId.Should().BeGreaterThan(0);
            var saved = await _db.TestParameters.FindAsync(created.ParameterId);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Glucose");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.3 — Set Reference Values
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateReferenceRange_ShouldPersistChanges_SuccessGuard()
        {
            // Function: 3.3 — Set Reference Values
            // Arrange
            var range = new TestReferenceRange { TestId = 1, Gender = "Male", LowValue = 10, HighValue = 20 };
            _db.TestReferenceRanges.Add(range);
            await _db.SaveChangesAsync();

            range.HighValue = 25;
            range.NormalText = "New Normal";

            // Act
            await _service.UpdateReferenceRangeAsync(range);

            // Assert
            var updated = await _db.TestReferenceRanges.FindAsync(range.RangeId);
            updated.Should().NotBeNull();
            updated!.HighValue.Should().Be(25);
            updated.NormalText.Should().Be("New Normal");
        }

        [Fact]
        public async Task DeleteReferenceRange_ShouldRemove_SuccessGuard()
        {
            // Function: 3.3 — Set Reference Values
            // Arrange
            var range = new TestReferenceRange { TestId = 1, Gender = "Male" };
            _db.TestReferenceRanges.Add(range);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteReferenceRangeAsync(range.RangeId);

            // Assert
            var exists = await _db.TestReferenceRanges.AnyAsync(r => r.RangeId == range.RangeId);
            exists.Should().BeFalse();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.4 / 3.6 — Test Comments
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetTestComments_ForSpecificTest_ShouldReturnOnlyRelevant_SuccessGuard()
        {
            // Function: 3.6 — Add Test Comments
            // Arrange
            _db.Tests.Add(new Test { TestId = 10, Code = "T10", NameReport = "R", NameReceipt = "R", Price = 1 });
            _db.Tests.Add(new Test { TestId = 20, Code = "T20", NameReport = "R", NameReceipt = "R", Price = 1 });
            _db.TestComments.Add(new TestComment { TestId = 10, CommentText = "Comment for T10" });
            _db.TestComments.Add(new TestComment { TestId = 20, CommentText = "Comment for T20" });
            await _db.SaveChangesAsync();

            // Act
            var comments = await _service.GetTestCommentsAsync(10);

            // Assert
            comments.Should().HaveCount(1);
            comments.First().CommentText.Should().Be("Comment for T10");
        }

        [Fact]
        public async Task DeleteTestComment_WhenExists_ShouldRemove_SuccessGuard()
        {
            // Function: 3.6 — Add Test Comments
            // Arrange
            var comment = new TestComment { TestId = 1, CommentText = "To Delete" };
            _db.TestComments.Add(comment);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteTestCommentAsync(comment.CommentId);

            // Assert
            var exists = await _db.TestComments.AnyAsync(c => c.CommentId == comment.CommentId);
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateTestComment_WithEmptyText_ShouldThrow_FailureGuard()
        {
            // Function: 3.6 — Add Test Comments (Failure: empty text)
            // Arrange
            var comment = new TestComment { TestId = 1, CommentText = "Original" };
            _db.TestComments.Add(comment);
            await _db.SaveChangesAsync();

            comment.CommentText = "";

            // Act
            Func<Task> act = async () => await _service.UpdateTestCommentAsync(comment);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteTestComment_WhenNotExists_ShouldNotThrow_EdgeGuard()
        {
            // Function: 3.6 — Add Test Comments (Edge: non-existent)
            // Act
            Func<Task> act = async () => await _service.DeleteTestCommentAsync(99999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.5 — Create Custom Group
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteCustomGroupItem_ShouldRemoveOnlyItem_SuccessGuard()
        {
            // Function: 3.5 — Create Custom Group
            // Arrange
            var item = new CustomGroupItem { CustomGroupId = 1, TestId = 1 };
            _db.CustomGroupItems.Add(item);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteCustomGroupItemAsync(item.CustomGroupItemId);

            // Assert
            (await _db.CustomGroupItems.AnyAsync(i => i.CustomGroupItemId == item.CustomGroupItemId)).Should().BeFalse();
        }

        [Fact]
        public async Task CreateCustomGroup_WithDuplicateName_ShouldThrow_FailureGuard()
        {
            // Function: 3.5 — Create Custom Group (Failure: duplicate name)
            // Arrange
            _db.CustomGroups.Add(new CustomGroup { Name = "DUPLICATE", Price = 100 });
            await _db.SaveChangesAsync();

            var newGroup = new CustomGroup { Name = "DUPLICATE", Price = 200 };

            // Act
            Func<Task> act = async () => await _service.CreateCustomGroupAsync(newGroup);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*exists*");
        }

        [Fact]
        public async Task DeleteCustomGroupItem_WithInvalidId_ShouldNotThrow_EdgeGuard()
        {
            // Function: 3.5 — Create Custom Group (Edge: invalid ID)
            // Act
            Func<Task> act = async () => await _service.DeleteCustomGroupItemAsync(99999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.7 — Create Price List
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdatePriceList_WithNewName_ShouldUpdate_SuccessGuard()
        {
            // Function: 3.7 — Create Price List
            // Arrange
            var pl = new PriceList { Name = "Old Name" };
            _db.PriceLists.Add(pl);
            await _db.SaveChangesAsync();

            pl.Name = "New Name";

            // Act
            await _service.UpdatePriceListAsync(pl);

            // Assert
            var updated = await _db.PriceLists.FindAsync(pl.PriceListId);
            updated!.Name.Should().Be("New Name");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.8 — Update Prices
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdatePriceListItem_ShouldUpdatePrice_SuccessGuard()
        {
            // Function: 3.8 — Update Prices
            // Arrange
            var item = new PriceListItem { PriceListId = 1, TestId = 1, Price = 50m };
            _db.PriceListItems.Add(item);
            await _db.SaveChangesAsync();

            item.Price = 75m;

            // Act
            await _service.UpdatePriceListItemAsync(item);

            // Assert
            var updated = await _db.PriceListItems.FindAsync(item.PriceListItemId);
            updated!.Price.Should().Be(75m);
        }

        [Fact]
        public async Task DeletePriceListItem_ShouldRemoveFromList_SuccessGuard()
        {
            // Function: 3.8 — Update Prices
            // Arrange
            var item = new PriceListItem { PriceListId = 1, TestId = 1, Price = 50m };
            _db.PriceListItems.Add(item);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeletePriceListItemAsync(item.PriceListItemId);

            // Assert
            (await _db.PriceListItems.AnyAsync(i => i.PriceListItemId == item.PriceListItemId)).Should().BeFalse();
        }
    }
}
