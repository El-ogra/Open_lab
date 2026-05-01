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
    // Additional tests for Module 3 functions - filling coverage gaps
    public partial class TestCatalogServiceTests
    {
        // ========== Function 3.1 - Add New Test ==========
        // Additional Success Test
        [Fact]
        public async Task CreateTestAsync_WithAllFields_ShouldPersistCorrectly()
        {
            // Function: 3.1 — Add New Test
            // Arrange
            var test = new Test
            {
                Code = "URINE",
                NameReport = "Urinalysis",
                NameReceipt = "Urine Analysis Receipt",
                Price = 75m,
                TurnaroundHours = 4,
                ReportOrder = 1,
                IsRoutine = true,
                IsSendOut = false
            };

            // Act
            var created = await _service.CreateTestAsync(test);

            // Assert
            created.TestId.Should().BeGreaterThan(0);
            var saved = await _db.Tests.FindAsync(created.TestId);
            saved.Should().NotBeNull();
            saved!.Code.Should().Be("URINE");
            saved.NameReport.Should().Be("Urinalysis");
            saved.NameReceipt.Should().Be("Urine Analysis Receipt");
            saved.Price.Should().Be(75m);
            saved.TurnaroundHours.Should().Be(4);
            saved.ReportOrder.Should().Be(1);
            saved.IsRoutine.Should().BeTrue();
            saved.IsSendOut.Should().BeFalse();
        }

        // Additional Failure Tests
        [Fact]
        public async Task CreateTestAsync_WithNullTest_ShouldThrowArgumentNullException()
        {
            // Function: 3.1 — Add New Test
            // Act
            Func<Task> act = async () => await _service.CreateTestAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateTestAsync_WithEmptyCode_ShouldThrowArgumentException()
        {
            // Function: 3.1 — Add New Test
            // Arrange
            var test = new Test { Code = "   ", NameReport = "Test", NameReceipt = "Test", Price = 10m };

            // Act
            Func<Task> act = async () => await _service.CreateTestAsync(test);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateTestAsync_WithEmptyNameReport_ShouldThrowArgumentException()
        {
            // Function: 3.1 — Add New Test
            // Arrange
            var test = new Test { Code = "CODE", NameReport = "", NameReceipt = "Test", Price = 10m };

            // Act
            Func<Task> act = async () => await _service.CreateTestAsync(test);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateTestAsync_WithEmptyNameReceipt_ShouldThrowArgumentException()
        {
            // Function: 3.1 — Add New Test
            // Arrange
            var test = new Test { Code = "CODE", NameReport = "Test", NameReceipt = "  ", Price = 10m };

            // Act
            Func<Task> act = async () => await _service.CreateTestAsync(test);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateTestAsync_WithZeroPrice_ShouldBeAllowed_EdgeGuard()
        {
            // Function: 3.1 — Add New Test
            // Arrange
            var test = new Test { Code = "FREE", NameReport = "Free Test", NameReceipt = "Free", Price = 0m };

            // Act
            var created = await _service.CreateTestAsync(test);

            // Assert
            created.TestId.Should().BeGreaterThan(0);
            var saved = await _db.Tests.FindAsync(created.TestId);
            saved!.Price.Should().Be(0m);
        }

        // ========== Function 3.2 - Edit Test Data ==========
        [Fact]
        public async Task UpdateTestAsync_WithValidData_ShouldUpdateAllFields()
        {
            // Function: 3.2 — Edit Test Data
            // Arrange
            var existing = new Test
            {
                Code = "OLD",
                NameReport = "Old Report",
                NameReceipt = "Old Receipt",
                Price = 10m,
                TurnaroundHours = 2,
                ReportOrder = 5
            };
            _db.Tests.Add(existing);
            await _db.SaveChangesAsync();

            existing.NameReport = "New Report";
            existing.TurnaroundHours = 8;
            existing.ReportOrder = 10;

            // Act
            await _service.UpdateTestAsync(existing);

            // Assert
            var updated = await _db.Tests.FindAsync(existing.TestId);
            updated!.NameReport.Should().Be("New Report");
            updated.TurnaroundHours.Should().Be(8);
            updated.ReportOrder.Should().Be(10);
        }

        [Fact]
        public async Task UpdateTestAsync_WithNullTest_ShouldThrowArgumentNullException()
        {
            // Function: 3.2 — Edit Test Data
            // Act
            Func<Task> act = async () => await _service.UpdateTestAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateTestAsync_WithNonExistentTest_ShouldThrowInvalidOperationException()
        {
            // Function: 3.2 — Edit Test Data
            // Arrange
            var test = new Test
            {
                TestId = 99999,
                Code = "NON",
                NameReport = "Non-existent",
                NameReceipt = "Non",
                Price = 10m
            };

            // Act
            Func<Task> act = async () => await _service.UpdateTestAsync(test);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task UpdateTestAsync_WithDuplicateCode_ShouldThrowInvalidOperationException()
        {
            // Function: 3.2 — Edit Test Data (BR-VAL-005)
            // Arrange
            _db.Tests.Add(new Test { Code = "DUPE", NameReport = "Dup1", NameReceipt = "Dup1", Price = 10m });
            var existing = new Test { Code = "DUPE2", NameReport = "Dup2", NameReceipt = "Dup2", Price = 10m };
            _db.Tests.Add(existing);
            await _db.SaveChangesAsync();

            existing.Code = "DUPE";

            // Act
            Func<Task> act = async () => await _service.UpdateTestAsync(existing);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
        }

        // ========== Function 3.3 - Set Reference Values ==========
        // Additional Edge Case Tests
        [Fact]
        public async Task CreateReferenceRangeAsync_WithOnlyNormalText_ShouldSucceed_EdgeGuard()
        {
            // Function: 3.3 — Set Reference Values
            // Arrange
            var test = new Test { Code = "TEXT", NameReport = "Text Test", NameReceipt = "Text", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var range = new TestReferenceRange
            {
                TestId = test.TestId,
                NormalText = "Positive/Negative",
                LowValue = null,
                HighValue = null
            };

            // Act
            var created = await _service.CreateReferenceRangeAsync(range);

            // Assert
            created.RangeId.Should().BeGreaterThan(0);
            created.NormalText.Should().Be("Positive/Negative");
            created.LowValue.Should().BeNull();
            created.HighValue.Should().BeNull();
        }

        [Fact]
        public async Task CreateReferenceRangeAsync_WithNullTestId_ShouldThrowArgumentException()
        {
            // Function: 3.3 — Set Reference Values
            // Arrange
            var range = new TestReferenceRange { TestId = 0, LowValue = 1, HighValue = 2 };

            // Act
            Func<Task> act = async () => await _service.CreateReferenceRangeAsync(range);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*TestId*");
        }

        [Fact]
        public async Task CreateReferenceRangeAsync_WithInvalidAgeRange_ShouldThrowArgumentException()
        {
            // Function: 3.3 — Set Reference Values
            // Arrange
            var test = new Test { Code = "AGE", NameReport = "Age Test", NameReceipt = "Age", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var range = new TestReferenceRange
            {
                TestId = test.TestId,
                AgeFrom = 50,
                AgeTo = 10,
                LowValue = 1,
                HighValue = 2
            };

            // Act
            Func<Task> act = async () => await _service.CreateReferenceRangeAsync(range);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*AgeFrom*");
        }

        [Fact]
        public async Task CreateReferenceRangeAsync_WithInvalidValueRange_ShouldThrowArgumentException()
        {
            // Function: 3.3 — Set Reference Values
            // Arrange
            var test = new Test { Code = "VAL", NameReport = "Value Test", NameReceipt = "Val", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var range = new TestReferenceRange
            {
                TestId = test.TestId,
                LowValue = 100,
                HighValue = 10
            };

            // Act
            Func<Task> act = async () => await _service.CreateReferenceRangeAsync(range);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*LowValue*");
        }

        [Fact]
        public async Task CreateReferenceRangeAsync_WithNoValuesOrText_ShouldThrowArgumentException()
        {
            // Function: 3.3 — Set Reference Values
            // Arrange
            var test = new Test { Code = "EMPTY", NameReport = "Empty Test", NameReceipt = "Empty", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var range = new TestReferenceRange
            {
                TestId = test.TestId,
                LowValue = null,
                HighValue = null,
                NormalText = null
            };

            // Act
            Func<Task> act = async () => await _service.CreateReferenceRangeAsync(range);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*numeric limits or normal text*");
        }

        [Fact]
        public async Task UpdateReferenceRangeAsync_ShouldPersistChanges()
        {
            // Function: 3.3 — Set Reference Values
            // Arrange
            var test = new Test { Code = "UPREF", NameReport = "Update Test", NameReceipt = "UpRef", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var range = new TestReferenceRange
            {
                TestId = test.TestId,
                AgeFrom = 0,
                AgeTo = 100,
                LowValue = 3.0m,
                HighValue = 6.0m
            };
            var created = await _service.CreateReferenceRangeAsync(range);

            // Update
            created.LowValue = 2.5m;
            created.HighValue = 5.5m;

            // Act
            await _service.UpdateReferenceRangeAsync(created);

            // Assert
            var updated = await _db.TestReferenceRanges.FindAsync(created.RangeId);
            updated!.LowValue.Should().Be(2.5m);
            updated.HighValue.Should().Be(5.5m);
        }

        [Fact]
        public async Task DeleteReferenceRangeAsync_ShouldRemoveRange()
        {
            // Function: 3.3 — Set Reference Values
            // Arrange
            var test = new Test { Code = "DELREF", NameReport = "Del Test", NameReceipt = "DelRef", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var range = new TestReferenceRange
            {
                TestId = test.TestId,
                LowValue = 1,
                HighValue = 2
            };
            var created = await _service.CreateReferenceRangeAsync(range);
            var rangeId = created.RangeId;

            // Act
            await _service.DeleteReferenceRangeAsync(rangeId);

            // Assert
            var deleted = await _db.TestReferenceRanges.FindAsync(rangeId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteReferenceRangeAsync_WhenNotFound_ShouldNotThrow_EdgeGuard()
        {
            // Function: 3.3 — Set Reference Values
            // Act
            Func<Task> act = async () => await _service.DeleteReferenceRangeAsync(99999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ========== Function 3.4 - Add Low/High Comments ==========
        [Fact]
        public async Task CreateTestCommentAsync_WithEmptyText_ShouldThrowArgumentException()
        {
            // Function: 3.4 — Add Low/High Comments
            // Arrange
            var test = new Test { Code = "CMT1", NameReport = "Cmt Test", NameReceipt = "Cmt", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var comment = new TestComment { TestId = test.TestId, CommentText = "   " };

            // Act
            Func<Task> act = async () => await _service.CreateTestCommentAsync(comment);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateTestCommentAsync_WithNullText_ShouldThrowArgumentException()
        {
            // Function: 3.4 — Add Low/High Comments
            // Arrange
            var test = new Test { Code = "CMT2", NameReport = "Cmt Test 2", NameReceipt = "Cmt2", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var comment = new TestComment { TestId = test.TestId, CommentText = null! };

            // Act
            Func<Task> act = async () => await _service.CreateTestCommentAsync(comment);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateTestCommentAsync_ShouldTrimWhitespace()
        {
            // Function: 3.4 — Add Low/High Comments
            // Arrange
            var test = new Test { Code = "TRIM", NameReport = "Trim Test", NameReceipt = "Trim", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var comment = new TestComment { TestId = test.TestId, CommentText = "  Comment Text  " };

            // Act
            var created = await _service.CreateTestCommentAsync(comment);

            // Assert
            created.CommentText.Should().Be("Comment Text");
        }

        [Fact]
        public async Task DeleteTestCommentAsync_ShouldRemoveComment()
        {
            // Function: 3.4 — Add Low/High Comments
            // Arrange
            var test = new Test { Code = "DELCMT", NameReport = "Del Cmt", NameReceipt = "DelCmt", Price = 5m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var comment = new TestComment { TestId = test.TestId, CommentText = "To Delete" };
            var created = await _service.CreateTestCommentAsync(comment);
            var commentId = created.CommentId;

            // Act
            await _service.DeleteTestCommentAsync(commentId);

            // Assert
            var deleted = await _db.TestComments.FindAsync(commentId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTestCommentAsync_WhenNotFound_ShouldNotThrow_EdgeGuard()
        {
            // Function: 3.4 — Add Low/High Comments
            // Act
            Func<Task> act = async () => await _service.DeleteTestCommentAsync(99999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ========== Function 3.5 - Create Custom Group ==========
        [Fact]
        public async Task CreateCustomGroupAsync_WithEmptyName_ShouldThrowArgumentException()
        {
            // Function: 3.5 — Create Custom Group
            // Arrange
            var group = new CustomGroup { Name = "   ", Price = 10m };

            // Act
            Func<Task> act = async () => await _service.CreateCustomGroupAsync(group);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateCustomGroupAsync_WithNullName_ShouldThrowArgumentException()
        {
            // Function: 3.5 — Create Custom Group
            // Arrange
            var group = new CustomGroup { Name = null!, Price = 10m };

            // Act
            Func<Task> act = async () => await _service.CreateCustomGroupAsync(group);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateCustomGroupAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
        {
            // Function: 3.5 — Create Custom Group
            // Arrange
            _db.CustomGroups.Add(new CustomGroup { Name = "Profile X", Price = 10m });
            await _db.SaveChangesAsync();

            var duplicate = new CustomGroup { Name = "Profile X", Price = 20m };

            // Act
            Func<Task> act = async () => await _service.CreateCustomGroupAsync(duplicate);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
        }

        [Fact]
        public async Task CreateCustomGroupAsync_WithZeroPrice_ShouldSucceed_EdgeGuard()
        {
            // Function: 3.5 — Create Custom Group
            // Arrange
            var group = new CustomGroup { Name = "Free Profile", Price = 0m };

            // Act
            var created = await _service.CreateCustomGroupAsync(group);

            // Assert
            created.CustomGroupId.Should().BeGreaterThan(0);
            created.Price.Should().Be(0m);
        }

        // ========== Function 3.6 - Add Test Comments ==========
        // Already covered by 3.4 tests (same function)

        // ========== Function 3.7 - Create Price List ==========
        [Fact]
        public async Task CreatePriceListAsync_WithEmptyName_ShouldThrowArgumentException()
        {
            // Function: 3.7 — Create Price List
            // Arrange
            var priceList = new PriceList { Name = "   " };

            // Act
            Func<Task> act = async () => await _service.CreatePriceListAsync(priceList);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreatePriceListAsync_WithNullName_ShouldThrowArgumentException()
        {
            // Function: 3.7 — Create Price List
            // Arrange
            var priceList = new PriceList { Name = null! };

            // Act
            Func<Task> act = async () => await _service.CreatePriceListAsync(priceList);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreatePriceListAsync_WithNullReferral_ShouldCreateSuccessfully()
        {
            // Function: 3.7 — Create Price List
            // Arrange
            var priceList = new PriceList { Name = "General List", ReferralId = null };

            // Act
            var created = await _service.CreatePriceListAsync(priceList);

            // Assert
            created.PriceListId.Should().BeGreaterThan(0);
            created.ReferralId.Should().BeNull();
        }

        [Fact]
        public async Task UpdatePriceListAsync_ShouldUpdateAllFields()
        {
            // Function: 3.7 — Create Price List
            // Arrange
            var referral = new Referral { Name = "RefPL" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var priceList = new PriceList { Name = "Original", ReferralId = referral.ReferralId };
            var created = await _service.CreatePriceListAsync(priceList);

            // Update
            created.Name = "Updated List";
            created.ReferralId = null;

            // Act
            await _service.UpdatePriceListAsync(created);

            // Assert
            var updated = await _db.PriceLists.FindAsync(created.PriceListId);
            updated!.Name.Should().Be("Updated List");
            updated.ReferralId.Should().BeNull();
        }

        [Fact]
        public async Task UpdatePriceListAsync_WithNullName_ShouldThrowArgumentException()
        {
            // Function: 3.7 — Create Price List
            // Arrange
            var priceList = new PriceList { PriceListId = 1, Name = "Test" };
            _db.PriceLists.Add(priceList);
            await _db.SaveChangesAsync();

            priceList.Name = null!;

            // Act
            Func<Task> act = async () => await _service.UpdatePriceListAsync(priceList);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        // ========== Function 3.8 - Update Prices ==========
        [Fact]
        public async Task AddPriceListItemAsync_WithNegativePrice_ShouldThrowArgumentException()
        {
            // Function: 3.8 — Update Prices
            // Arrange
            var pl = new PriceList { Name = "PLNeg" };
            _db.PriceLists.Add(pl);
            var t = new Test { Code = "TNEG", NameReport = "T", NameReceipt = "T", Price = 10m };
            _db.Tests.Add(t);
            await _db.SaveChangesAsync();

            var item = new PriceListItem { PriceListId = pl.PriceListId, TestId = t.TestId, Price = -5m };

            // Act
            Func<Task> act = async () => await _service.AddPriceListItemAsync(item);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*negative*");
        }

        [Fact]
        public async Task AddPriceListItemAsync_WithDuplicateTestAndPriceList_ShouldThrowInvalidOperationException()
        {
            // Function: 3.8 — Update Prices
            // Arrange
            var pl = new PriceList { Name = "PLDup" };
            _db.PriceLists.Add(pl);
            var t = new Test { Code = "TDUP", NameReport = "T", NameReceipt = "T", Price = 10m };
            _db.Tests.Add(t);
            await _db.SaveChangesAsync();

            _db.PriceListItems.Add(new PriceListItem { PriceListId = pl.PriceListId, TestId = t.TestId, Price = 10m });
            await _db.SaveChangesAsync();

            var item = new PriceListItem { PriceListId = pl.PriceListId, TestId = t.TestId, Price = 15m };

            // Act
            Func<Task> act = async () => await _service.AddPriceListItemAsync(item);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
        }

        [Fact]
        public async Task UpdatePriceListItemAsync_ShouldUpdatePrice()
        {
            // Function: 3.8 — Update Prices
            // Arrange
            var pl = new PriceList { Name = "PLUpd" };
            _db.PriceLists.Add(pl);
            var t = new Test { Code = "TUPD", NameReport = "T", NameReceipt = "T", Price = 10m };
            _db.Tests.Add(t);
            await _db.SaveChangesAsync();

            var item = new PriceListItem { PriceListId = pl.PriceListId, TestId = t.TestId, Price = 10m };
            var created = await _service.AddPriceListItemAsync(item);

            // Update
            created.Price = 25m;

            // Act
            await _service.UpdatePriceListItemAsync(created);

            // Assert
            var updated = await _db.PriceListItems.FindAsync(created.PriceListItemId);
            updated!.Price.Should().Be(25m);
        }

        [Fact]
        public async Task UpdatePriceListItemAsync_WithNegativePrice_ShouldThrowArgumentException()
        {
            // Function: 3.8 — Update Prices
            // Arrange
            var pl = new PriceList { Name = "PLNeg2" };
            _db.PriceLists.Add(pl);
            var t = new Test { Code = "TNEG2", NameReport = "T", NameReceipt = "T", Price = 10m };
            _db.Tests.Add(t);
            await _db.SaveChangesAsync();

            var item = new PriceListItem { PriceListId = pl.PriceListId, TestId = t.TestId, Price = 10m };
            var created = await _service.AddPriceListItemAsync(item);

            created.Price = -1m;

            // Act
            Func<Task> act = async () => await _service.UpdatePriceListItemAsync(created);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*negative*");
        }

        [Fact]
        public async Task DeletePriceListItemAsync_ShouldRemoveItem()
        {
            // Function: 3.8 — Update Prices
            // Arrange
            var pl = new PriceList { Name = "PLDel" };
            _db.PriceLists.Add(pl);
            var t = new Test { Code = "TDEL", NameReport = "T", NameReceipt = "T", Price = 10m };
            _db.Tests.Add(t);
            await _db.SaveChangesAsync();

            var item = new PriceListItem { PriceListId = pl.PriceListId, TestId = t.TestId, Price = 10m };
            var created = await _service.AddPriceListItemAsync(item);
            var itemId = created.PriceListItemId;

            // Act
            await _service.DeletePriceListItemAsync(itemId);

            // Assert
            var deleted = await _db.PriceListItems.FindAsync(itemId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeletePriceListItemAsync_WhenNotFound_ShouldNotThrow_EdgeGuard()
        {
            // Function: 3.8 — Update Prices
            // Act
            Func<Task> act = async () => await _service.DeletePriceListItemAsync(99999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ========== Function 3.9 - Mark as Outsourced ==========
        [Fact]
        public async Task CreateTestAsync_WithOutsourceFlags_ShouldPersist()
        {
            // Function: 3.9 — Mark as Outsourced (BR-ACC-007)
            // Arrange
            var test = new Test
            {
                Code = "OUTSRC",
                NameReport = "Outsource Test",
                NameReceipt = "Outsource",
                Price = 100m,
                IsSendOut = true,
                CostPrice = 30m,
                PatientPrice = 120m
            };

            // Act
            var created = await _service.CreateTestAsync(test);

            // Assert
            created.IsSendOut.Should().BeTrue();
            created.CostPrice.Should().Be(30m);
            created.PatientPrice.Should().Be(120m);

            var saved = await _db.Tests.FindAsync(created.TestId);
            saved!.IsSendOut.Should().BeTrue();
            saved.CostPrice.Should().Be(30m);
            saved.PatientPrice.Should().Be(120m);
        }

        [Fact]
        public async Task CreateTestAsync_WithNullCostPrice_ShouldBeAllowed_EdgeGuard()
        {
            // Function: 3.9 — Mark as Outsourced
            // Arrange
            var test = new Test
            {
                Code = "OUTNULL",
                NameReport = "Outsource Null",
                NameReceipt = "OutNull",
                Price = 50m,
                IsSendOut = true,
                CostPrice = null,
                PatientPrice = 60m
            };

            // Act
            var created = await _service.CreateTestAsync(test);

            // Assert
            created.CostPrice.Should().BeNull();
        }

        [Fact]
        public async Task CreateTestAsync_WithNullPatientPrice_ShouldBeAllowed_EdgeGuard()
        {
            // Function: 3.9 — Mark as Outsourced
            // Arrange
            var test = new Test
            {
                Code = "OUTPNULL",
                NameReport = "Outsource Patient Null",
                NameReceipt = "OutPNul",
                Price = 50m,
                IsSendOut = true,
                CostPrice = 25m,
                PatientPrice = null
            };

            // Act
            var created = await _service.CreateTestAsync(test);

            // Assert
            created.PatientPrice.Should().BeNull();
        }

        [Fact]
        public async Task CreateTestAsync_WithNegativeCostPrice_ShouldBeAllowed_EdgeGuard()
        {
            // Function: 3.9 — Mark as Outsourced (Edge Case)
            // Arrange
            var test = new Test
            {
                Code = "NEGCOST",
                NameReport = "Neg Cost",
                NameReceipt = "Neg",
                Price = 50m,
                IsSendOut = true,
                CostPrice = -10m
            };

            // Act
            var created = await _service.CreateTestAsync(test);

            // Assert
            created.CostPrice.Should().Be(-10m);
        }

        [Fact]
        public async Task CreateTestAsync_WithNegativePatientPrice_ShouldBeAllowed_EdgeGuard()
        {
            // Function: 3.9 — Mark as Outsourced (Edge Case)
            // Arrange
            var test = new Test
            {
                Code = "NEGPAT",
                NameReport = "Neg Pat",
                NameReceipt = "Neg",
                Price = 50m,
                IsSendOut = true,
                PatientPrice = -10m
            };

            // Act
            var created = await _service.CreateTestAsync(test);

            // Assert
            created.PatientPrice.Should().Be(-10m);
        }

        // ========== Additional Service Methods ==========
        [Fact]
        public async Task GetAllTestsAsync_ShouldReturnOrderedByNameReport()
        {
            // Function: 3.1 - Support Method
            // Arrange
            _db.Tests.Add(new Test { Code = "Z_TEST", NameReport = "Zebra", NameReceipt = "Z", Price = 10m });
            _db.Tests.Add(new Test { Code = "A_TEST", NameReport = "Alpha", NameReceipt = "A", Price = 10m });
            _db.Tests.Add(new Test { Code = "M_TEST", NameReport = "Mike", NameReceipt = "M", Price = 10m });
            await _db.SaveChangesAsync();

            // Act
            var tests = await _service.GetAllTestsAsync();

            // Assert
            tests.Should().HaveCount(3);
            tests[0].NameReport.Should().Be("Alpha");
            tests[1].NameReport.Should().Be("Mike");
            tests[2].NameReport.Should().Be("Zebra");
        }

        [Fact]
        public async Task GetTestByIdAsync_WithValidId_ShouldReturnTest()
        {
            // Function: 3.1 - Support Method
            // Arrange
            var test = new Test { Code = "GETBY", NameReport = "Get By Id", NameReceipt = "GetBy", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetTestByIdAsync(test.TestId);

            // Assert
            result.Should().NotBeNull();
            result!.TestId.Should().Be(test.TestId);
            result.Code.Should().Be("GETBY");
        }

        [Fact]
        public async Task GetTestByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Function: 3.1 - Support Method
            // Act
            var result = await _service.GetTestByIdAsync(99999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTestAsync_WhenTestInUse_ShouldThrowInvalidOperationException()
        {
            // Function: 3.1 - Delete Test
            // Arrange
            var test = new Test { Code = "USED", NameReport = "Used Test", NameReceipt = "Used", Price = 10m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Add to VisitTests
            _db.VisitTests.Add(new VisitTest { TestId = test.TestId, VisitId = 1 });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.DeleteTestAsync(test.TestId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already used*");
        }

        [Fact]
        public async Task DeleteTestAsync_WhenNotFound_ShouldNotThrow_EdgeGuard()
        {
            // Function: 3.1 - Delete Test
            // Act
            Func<Task> act = async () => await _service.DeleteTestAsync(99999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ========== Get Test Groups ==========
        [Fact]
        public async Task CreateTestGroupAsync_ShouldCreateGroup()
        {
            // Function: 3.5 - Support Method
            // Arrange
            var group = new TestGroup { GroupName = "Chemistry Tests" };

            // Act
            var created = await _service.CreateTestGroupAsync(group);

            // Assert
            created.GroupId.Should().BeGreaterThan(0);
            var saved = await _db.TestGroups.FindAsync(created.GroupId);
            saved.Should().NotBeNull();
            saved!.GroupName.Should().Be("Chemistry Tests");
        }

        [Fact]
        public async Task CreateTestGroupAsync_WithEmptyName_ShouldThrowArgumentException()
        {
            // Function: 3.5 - Support Method
            // Arrange
            var group = new TestGroup { GroupName = "   " };

            // Act
            Func<Task> act = async () => await _service.CreateTestGroupAsync(group);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateTestGroupAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
        {
            // Function: 3.5 - Support Method
            // Arrange
            _db.TestGroups.Add(new TestGroup { GroupName = "Duplicate Group" });
            await _db.SaveChangesAsync();

            var duplicate = new TestGroup { GroupName = "Duplicate Group" };

            // Act
            Func<Task> act = async () => await _service.CreateTestGroupAsync(duplicate);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task GetTestGroupsAsync_ShouldReturnOrderedGroups()
        {
            // Function: 3.5 - Support Method
            // Arrange
            _db.TestGroups.Add(new TestGroup { GroupName = "Zebra Group" });
            _db.TestGroups.Add(new TestGroup { GroupName = "Alpha Group" });
            await _db.SaveChangesAsync();

            // Act
            var groups = await _service.GetTestGroupsAsync();

            // Assert
            groups.Should().HaveCount(2);
            groups[0].GroupName.Should().Be("Alpha Group");
        }

        // ========== Sample Types ==========
        [Fact]
        public async Task CreateSampleTypeAsync_ShouldCreateSampleType()
        {
            // Function: 3.1 - Support Method
            // Arrange
            var sampleType = new SampleType { Name = "Blood Serum" };

            // Act
            var created = await _service.CreateSampleTypeAsync(sampleType);

            // Assert
            created.SampleTypeId.Should().BeGreaterThan(0);
            var saved = await _db.SampleTypes.FindAsync(created.SampleTypeId);
            saved!.Name.Should().Be("Blood Serum");
        }

        [Fact]
        public async Task CreateSampleTypeAsync_WithEmptyName_ShouldThrowArgumentException()
        {
            // Function: 3.1 - Support Method
            // Arrange
            var sampleType = new SampleType { Name = "" };

            // Act
            Func<Task> act = async () => await _service.CreateSampleTypeAsync(sampleType);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetSampleTypesAsync_ShouldReturnOrderedSampleTypes()
        {
            // Function: 3.1 - Support Method
            // Arrange
            _db.SampleTypes.Add(new SampleType { Name = "Urine" });
            _db.SampleTypes.Add(new SampleType { Name = "Blood" });
            await _db.SaveChangesAsync();

            // Act
            var sampleTypes = await _service.GetSampleTypesAsync();

            // Assert
            sampleTypes.Should().HaveCount(2);
            sampleTypes[0].Name.Should().Be("Blood");
        }

        // ========== Units ==========
        [Fact]
        public async Task CreateUnitAsync_ShouldCreateUnit()
        {
            // Function: 3.1 - Support Method
            // Arrange
            var unit = new Unit { Name = "mg/dL" };

            // Act
            var created = await _service.CreateUnitAsync(unit);

            // Assert
            created.UnitId.Should().BeGreaterThan(0);
            var saved = await _db.Units.FindAsync(created.UnitId);
            saved!.Name.Should().Be("mg/dL");
        }

        [Fact]
        public async Task CreateUnitAsync_WithEmptyName_ShouldThrowArgumentException()
        {
            // Function: 3.1 - Support Method
            // Arrange
            var unit = new Unit { Name = "   " };

            // Act
            Func<Task> act = async () => await _service.CreateUnitAsync(unit);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetUnitsAsync_ShouldReturnOrderedUnits()
        {
            // Function: 3.1 - Support Method
            // Arrange
            _db.Units.Add(new Unit { Name = "mg/dL" });
            _db.Units.Add(new Unit { Name = "g/L" });
            await _db.SaveChangesAsync();

            // Act
            var units = await _service.GetUnitsAsync();

            // Assert
            units.Should().HaveCount(2);
        }
    }
}