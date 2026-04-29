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
        public async Task CreateTestAsync_Should_Create_Test_When_Valid_LogicGuard()
        {
            // Refactored to Logic Guard - verifies all fields persisted
            var test = new Test { Code = "CBC", NameReport = "Complete Blood Count", NameReceipt = "CBC Receipt", Price = 50m };

            var created = await _service.CreateTestAsync(test);

            // Assert - Logic Guard: Verify all fields are saved correctly
            created.TestId.Should().BeGreaterThan(0);
            var saved = await _db.Tests.FindAsync(created.TestId);
            saved.Should().NotBeNull();
            saved!.Code.Should().Be("CBC");
            saved.NameReport.Should().Be("Complete Blood Count");
            saved.NameReceipt.Should().Be("CBC Receipt");
            saved.Price.Should().Be(50m);
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
        public async Task CreateTestCommentAsync_Default_Should_Clear_Other_Defaults_LogicGuard()
        {
            // Refactored to Logic Guard - verifies default comment logic
            // Arrange
            var test = new Test { Code = "C1", NameReport = "T", NameReceipt = "T", Price = 1m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.TestComments.Add(new TestComment { TestId = test.TestId, CommentText = "c1", IsDefault = true });
            await _db.SaveChangesAsync();

            var comment = new TestComment { TestId = test.TestId, CommentText = "c2", IsDefault = true };

            // Act
            var created = await _service.CreateTestCommentAsync(comment);

            // Assert - Logic Guard: Verify only one default exists and it's the new one
            created.CommentId.Should().BeGreaterThan(0);
            created.CommentText.Should().Be("c2");
            created.IsDefault.Should().BeTrue();
            var defaults = await _db.TestComments.CountAsync(c => c.TestId == test.TestId && c.IsDefault);
            defaults.Should().Be(1);
            var allComments = await _db.TestComments.Where(c => c.TestId == test.TestId).ToListAsync();
            allComments.Should().HaveCount(2);
            allComments.Single(c => c.IsDefault).CommentText.Should().Be("c2");
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

        [Fact]
        public async Task CreateCustomGroupAsync_Should_Persist_Group_LogicGuard()
        {
            // Refactored to Logic Guard - verifies all fields persisted
            var group = new CustomGroup { Name = "Profile A", Price = 25 };
            var result = await _service.CreateCustomGroupAsync(group);

            // Assert - Logic Guard: Verify all fields saved correctly
            result.Name.Should().Be("Profile A");
            result.Price.Should().Be(25);
            var saved = await _db.CustomGroups.FindAsync(result.CustomGroupId);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Profile A");
            saved.Price.Should().Be(25);
        }

        [Fact]
        public async Task AddPriceListItemAsync_Should_Add_New_Item_LogicGuard()
        {
            // Refactored to Logic Guard - verifies price list item binding
            var pl = new PriceList { Name = "PL" };
            _db.PriceLists.Add(pl);
            var t = new Test { NameReport = "T", Code = "T", Price = 10 };
            _db.Tests.Add(t);
            await _db.SaveChangesAsync();

            var item = new PriceListItem { PriceListId = pl.PriceListId, TestId = t.TestId, Price = 15 };
            await _service.AddPriceListItemAsync(item);

            // Assert - Logic Guard: Verify exact binding and price
            var saved = await _db.PriceListItems.FirstAsync(i => i.PriceListId == pl.PriceListId);
            saved.Price.Should().Be(15);
            saved.TestId.Should().Be(t.TestId);
            saved.PriceListId.Should().Be(pl.PriceListId);
        }

        [Fact]
        public async Task UpdateTestAsync_Should_Update_Test_Data_Including_Outsource_Flags()
        {
            // Arrange
            var existing = new Test
            {
                Code = "OLD",
                NameReport = "Old Name",
                NameReceipt = "Old Receipt",
                Price = 10m,
                TurnaroundHours = 4,
                IsSendOut = false,
                CostPrice = 1m,
                PatientPrice = 10m
            };
            _db.Tests.Add(existing);
            await _db.SaveChangesAsync();

            existing.Code = "NEW";
            existing.NameReport = "New Name";
            existing.NameReceipt = "New Receipt";
            existing.Price = 30m;
            existing.TurnaroundHours = 12;
            existing.IsSendOut = true;
            existing.CostPrice = 8m;
            existing.PatientPrice = 40m;

            // Act
            await _service.UpdateTestAsync(existing);

            // Assert
            var updated = await _db.Tests.FindAsync(existing.TestId);
            updated.Should().NotBeNull();
            updated!.Code.Should().Be("NEW");
            updated.NameReport.Should().Be("New Name");
            updated.IsSendOut.Should().BeTrue();
            updated.CostPrice.Should().Be(8m);
            updated.PatientPrice.Should().Be(40m);
        }

        [Fact]
        public async Task CreateAndUpdateTestComment_Should_Persist_Low_High_Comments_LogicGuard()
        {
            // Refactored to Logic Guard - verifies low/high comment persistence
            // Arrange
            var test = new Test { Code = "LC1", NameReport = "T", NameReceipt = "T", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Act
            var created = await _service.CreateTestCommentAsync(new TestComment
            {
                TestId = test.TestId,
                CommentText = "Default",
                LowComment = "LOW MSG",
                HighComment = "HIGH MSG",
                IsDefault = true
            });

            // Assert - Logic Guard: Verify low/high comments are saved
            created.CommentId.Should().BeGreaterThan(0);
            created.CommentText.Should().Be("Default");
            created.LowComment.Should().Be("LOW MSG");
            created.HighComment.Should().Be("HIGH MSG");
            created.IsDefault.Should().BeTrue();

            created.LowComment = "LOW UPDATED";
            created.HighComment = "HIGH UPDATED";
            await _service.UpdateTestCommentAsync(created);

            // Assert - Logic Guard: Verify updates persist correctly
            var saved = await _db.TestComments.FindAsync(created.CommentId);
            saved.Should().NotBeNull();
            saved!.LowComment.Should().Be("LOW UPDATED");
            saved.HighComment.Should().Be("HIGH UPDATED");
            saved.CommentText.Should().Be("Default");
        }

        // 3.7 - Price List Consistency Tests
        [Fact]
        public async Task CreatePriceListAsync_Should_Create_With_Correct_Fields_LogicGuard()
        {
            // Refactored to Logic Guard - verifies all price list fields
            // Arrange
            var referral = new Referral { Name = "Ref" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var priceList = new PriceList { Name = "Standard PL", ReferralId = referral.ReferralId, IsDefault = true };

            // Act
            var created = await _service.CreatePriceListAsync(priceList);

            // Assert - Logic Guard: Verify all fields saved correctly
            created.PriceListId.Should().BeGreaterThan(0);
            created.Name.Should().Be("Standard PL");
            created.ReferralId.Should().Be(referral.ReferralId);
            created.IsDefault.Should().BeTrue();
            var saved = await _db.PriceLists.FindAsync(created.PriceListId);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Standard PL");
            saved.ReferralId.Should().Be(referral.ReferralId);
            saved.IsDefault.Should().BeTrue();
        }

        [Fact]
        public async Task CreatePriceListAsync_DuplicateName_For_Same_Referral_Should_Throw_LogicGuard()
        {
            // Refactored to Logic Guard - verifies duplicate detection
            // Arrange
            var referral = new Referral { Name = "Ref" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var pl1 = new PriceList { Name = "PL", ReferralId = referral.ReferralId };
            _db.PriceLists.Add(pl1);
            await _db.SaveChangesAsync();

            var pl2 = new PriceList { Name = "PL", ReferralId = referral.ReferralId };

            // Act
            Func<Task> act = async () => await _service.CreatePriceListAsync(pl2);

            // Assert - Logic Guard: Verify duplicate is rejected
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
            var count = await _db.PriceLists.CountAsync(pl => pl.Name == "PL" && pl.ReferralId == referral.ReferralId);
            count.Should().Be(1, "Only one price list with duplicate name should exist");
        }

        // 3.3 Reference Range Decision Tests - Additional Logic Guard

        [Fact]
        public async Task CreateReferenceRangeAsync_Should_Validate_Range_Boundaries_LogicGuard()
        {
            // Function: 3.3 — Reference Range - Logic Guard: Verify range boundaries are validated
            // Arrange
            var test = new Test { Code = "REF2", NameReport = "Ref Test", NameReceipt = "Ref", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Valid range
            var validRange = new TestReferenceRange
            {
                TestId = test.TestId,
                AgeFrom = 0,
                AgeTo = 100,
                LowValue = 3.5m,
                HighValue = 5.5m,
                Gender = "All"
            };

            // Act
            var created = await _service.CreateReferenceRangeAsync(validRange);

            // Assert - Logic Guard: Verify range is saved with correct boundaries
            created.RangeId.Should().BeGreaterThan(0);
            var saved = await _db.TestReferenceRanges.FindAsync(created.RangeId);
            saved.Should().NotBeNull();
            saved!.LowValue.Should().Be(3.5m);
            saved.HighValue.Should().Be(5.5m);
            saved.AgeFrom.Should().Be(0);
            saved.AgeTo.Should().Be(100);
            saved.Gender.Should().Be("All");
        }

        [Fact]
        public async Task CreateReferenceRangeAsync_Equal_Boundaries_Should_Be_Valid_LogicGuard()
        {
            // Function: 3.3 — Reference Range - Logic Guard: Verify equal boundaries are handled
            // Arrange
            var test = new Test { Code = "REF3", NameReport = "Ref Test 3", NameReceipt = "Ref3", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Range with equal low and high (single value)
            var equalRange = new TestReferenceRange
            {
                TestId = test.TestId,
                AgeFrom = 18,
                AgeTo = 65,
                LowValue = 7.0m,
                HighValue = 7.0m,
                Gender = "All"
            };

            // Act
            var created = await _service.CreateReferenceRangeAsync(equalRange);

            // Assert - Logic Guard: Verify equal boundaries are saved correctly
            created.RangeId.Should().BeGreaterThan(0);
            var saved = await _db.TestReferenceRanges.FindAsync(created.RangeId);
            saved.Should().NotBeNull();
            saved!.LowValue.Should().Be(7.0m);
            saved.HighValue.Should().Be(7.0m);
        }

        [Fact]
        public async Task CreateTestAsync_NegativeTurnaround_Should_Throw()
        {
            // Arrange
            var test = new Test { Code = "NEG2", NameReport = "N", NameReceipt = "N", Price = 10m, TurnaroundHours = -1 };

            // Act
            Func<Task> act = async () => await _service.CreateTestAsync(test);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Turnaround hours*");
        }

        [Fact]
        public async Task CreateReferralAsync_Should_Create_Entity_For_Module12_1()
        {
            // Function: 12.1 — `Create Contract Entity`
            var referral = new Referral
            {
                Name = "Insurance A",
                ReferralType = "Insurance",
                Phone = "0100000000",
                City = "Cairo"
            };

            var created = await _service.CreateReferralAsync(referral);

            created.ReferralId.Should().BeGreaterThan(0);
            var saved = await _db.Referrals.FindAsync(created.ReferralId);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Insurance A");
            saved.ReferralType.Should().Be("Insurance");
        }

        [Fact]
        public async Task UpdateReferralAsync_Should_Set_Discount_And_Commission_For_Module12_3_12_4()
        {
            // Function: 12.3 — `Set Entity Discount`
            var referral = new Referral { Name = "Company A", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.DiscountPercentage = 12.5m;
            referral.CommissionPercentage = 7.5m;
            referral.Phone = "0123456789";
            referral.City = "Giza";

            await _service.UpdateReferralAsync(referral);

            var saved = await _db.Referrals.FindAsync(referral.ReferralId);
            saved.Should().NotBeNull();
            saved!.DiscountPercentage.Should().Be(12.5m);
            saved.CommissionPercentage.Should().Be(7.5m);
            saved.Phone.Should().Be("0123456789");
            saved.City.Should().Be("Giza");
        }

        [Fact]
        public async Task UpdateReferralAsync_Invalid_Discount_Or_Commission_Should_Throw()
        {
            var referral = new Referral { Name = "Company B", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.DiscountPercentage = 150m;
            Func<Task> discountAct = async () => await _service.UpdateReferralAsync(referral);
            await discountAct.Should().ThrowAsync<ArgumentException>().WithMessage("*Discount percentage*");

            referral.DiscountPercentage = 10m;
            referral.CommissionPercentage = -1m;
            Func<Task> commissionAct = async () => await _service.UpdateReferralAsync(referral);
            await commissionAct.Should().ThrowAsync<ArgumentException>().WithMessage("*Commission percentage*");
        }
    }
}
