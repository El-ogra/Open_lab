using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Open_lab.Tests.Services
{
    public class TestCatalogServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly TestCatalogService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public TestCatalogServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new TestCatalogService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateTestAsync_Should_Create_Test_When_Valid()
        {
            // Arrange
            var test = new Test { Code = "CBC", NameReport = "Complete Blood Count", NameReceipt = "CBC Receipt", Price = 50m };

            // Act
            var created = await _service.CreateTestAsync(test);

            // Assert
            created.TestId.Should().BeGreaterThan(0);
            (await _db.Tests.FindAsync(created.TestId)).Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTestAsync_DuplicateCode_Should_Throw()
        {
            // Arrange
            _db.Tests.Add(new Test { Code = "DUP", NameReport = "X", NameReceipt = "X", Price = 10m });
            await _db.SaveChangesAsync();

            var test = new Test { Code = "DUP", NameReport = "Y", NameReceipt = "Y", Price = 20m };

            // Act
            Func<Task> act = async () => await _service.CreateTestAsync(test);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateTestAsync_NegativePrice_Should_Throw()
        {
            // Arrange
            var test = new Test { Code = "NEG", NameReport = "N", NameReceipt = "N", Price = -1m };

            // Act
            Func<Task> act = async () => await _service.CreateTestAsync(test);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateReferenceRangeAsync_InvalidRange_Should_Throw()
        {
            // Arrange
            var range = new TestReferenceRange { TestId = 1, AgeFrom = 10, AgeTo = 5, LowValue = 1, HighValue = 2 };

            // Act
            Func<Task> act = async () => await _service.CreateReferenceRangeAsync(range);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateTestCommentAsync_Default_Should_Clear_Other_Defaults()
        {
            // Arrange
            var test = new Test { Code = "C1", NameReport = "T", NameReceipt = "T", Price = 1m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.TestComments.Add(new TestComment { TestId = test.TestId, CommentText = "c1", IsDefault = true });
            await _db.SaveChangesAsync();

            var comment = new TestComment { TestId = test.TestId, CommentText = "c2", IsDefault = true };

            // Act
            var created = await _service.CreateTestCommentAsync(comment);

            // Assert
            created.CommentId.Should().BeGreaterThan(0);
            var defaults = await _db.TestComments.CountAsync(c => c.TestId == test.TestId && c.IsDefault);
            defaults.Should().Be(1);
        }

        [Fact]
        public async Task CreatePriceListAsync_DuplicateForReferral_Should_Throw()
        {
            // Arrange
            var pl = new PriceList { Name = "PL1", ReferralId = 1 };
            _db.PriceLists.Add(pl);
            await _db.SaveChangesAsync();

            var newPl = new PriceList { Name = "PL1", ReferralId = 1 };

            // Act
            Func<Task> act = async () => await _service.CreatePriceListAsync(newPl);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AddPriceListItemAsync_InvalidReferences_Should_Throw()
        {
            // Arrange
            var item = new PriceListItem { PriceListId = 999, TestId = 999, Price = 10m };

            // Act
            Func<Task> act = async () => await _service.AddPriceListItemAsync(item);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateCustomGroupAsync_NegativePrice_Should_Throw()
        {
            // Arrange
            var group = new CustomGroup { Name = "G1", Price = -5m };

            // Act
            Func<Task> act = async () => await _service.CreateCustomGroupAsync(group);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
