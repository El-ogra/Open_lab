using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class TestCatalogService : ITestCatalogService
    {
        private readonly OpenLabDbContext _db;

        public TestCatalogService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<Test?> GetTestByIdAsync(int testId)
        {
            return _db.Tests
                .AsNoTracking()
                .Include(t => t.Group)
                .Include(t => t.SampleType)
                .Include(t => t.Unit)
                .FirstOrDefaultAsync(t => t.TestId == testId);
        }

        public Task<List<Test>> GetAllTestsAsync()
        {
            return _db.Tests.AsNoTracking().OrderBy(t => t.NameReport).ToListAsync();
        }

        public async Task<Test> CreateTestAsync(Test test)
        {
            if (test == null)
            {
                throw new ArgumentNullException(nameof(test));
            }

            if (string.IsNullOrWhiteSpace(test.Code) || string.IsNullOrWhiteSpace(test.NameReport) || string.IsNullOrWhiteSpace(test.NameReceipt))
            {
                throw new ArgumentException("Test code and names are required.", nameof(test));
            }

            _db.Tests.Add(test);
            await _db.SaveChangesAsync();
            return test;
        }

        public async Task UpdateTestAsync(Test test)
        {
            if (test == null)
            {
                throw new ArgumentNullException(nameof(test));
            }

            _db.Tests.Update(test);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteTestAsync(int testId)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                return;
            }

            _db.Tests.Remove(test);
            await _db.SaveChangesAsync();
        }

        public async Task<TestGroup> CreateTestGroupAsync(TestGroup group)
        {
            if (group == null || string.IsNullOrWhiteSpace(group.GroupName))
            {
                throw new ArgumentException("Group name is required.", nameof(group));
            }

            _db.TestGroups.Add(group);
            await _db.SaveChangesAsync();
            return group;
        }

        public Task<List<TestGroup>> GetTestGroupsAsync()
        {
            return _db.TestGroups.AsNoTracking().OrderBy(g => g.GroupName).ToListAsync();
        }

        public async Task<SampleType> CreateSampleTypeAsync(SampleType sampleType)
        {
            if (sampleType == null || string.IsNullOrWhiteSpace(sampleType.Name))
            {
                throw new ArgumentException("Sample type name is required.", nameof(sampleType));
            }

            _db.SampleTypes.Add(sampleType);
            await _db.SaveChangesAsync();
            return sampleType;
        }

        public Task<List<SampleType>> GetSampleTypesAsync()
        {
            return _db.SampleTypes.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<Unit> CreateUnitAsync(Unit unit)
        {
            if (unit == null || string.IsNullOrWhiteSpace(unit.Name))
            {
                throw new ArgumentException("Unit name is required.", nameof(unit));
            }

            _db.Units.Add(unit);
            await _db.SaveChangesAsync();
            return unit;
        }

        public Task<List<Unit>> GetUnitsAsync()
        {
            return _db.Units.AsNoTracking().OrderBy(u => u.Name).ToListAsync();
        }

        public async Task<TestParameter> CreateParameterAsync(TestParameter parameter)
        {
            if (parameter == null || string.IsNullOrWhiteSpace(parameter.Name))
            {
                throw new ArgumentException("Parameter name is required.", nameof(parameter));
            }

            _db.TestParameters.Add(parameter);
            await _db.SaveChangesAsync();
            return parameter;
        }

        public Task<List<TestParameter>> GetParametersByTestAsync(int testId)
        {
            return _db.TestParameters.AsNoTracking().Where(p => p.TestId == testId).OrderBy(p => p.OrderNo).ToListAsync();
        }

        public async Task<TestReferenceRange> CreateReferenceRangeAsync(TestReferenceRange range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            _db.TestReferenceRanges.Add(range);
            await _db.SaveChangesAsync();
            return range;
        }

        public Task<List<TestReferenceRange>> GetReferenceRangesAsync(int testId)
        {
            return _db.TestReferenceRanges.AsNoTracking().Where(r => r.TestId == testId).OrderBy(r => r.RangeId).ToListAsync();
        }

        public async Task UpdateReferenceRangeAsync(TestReferenceRange range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            _db.TestReferenceRanges.Update(range);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteReferenceRangeAsync(int rangeId)
        {
            var range = await _db.TestReferenceRanges.FirstOrDefaultAsync(r => r.RangeId == rangeId);
            if (range == null)
            {
                return;
            }

            _db.TestReferenceRanges.Remove(range);
            await _db.SaveChangesAsync();
        }

        public async Task<TestComment> CreateTestCommentAsync(TestComment comment)
        {
            if (comment == null || string.IsNullOrWhiteSpace(comment.CommentText))
            {
                throw new ArgumentException("Comment text is required.", nameof(comment));
            }

            _db.TestComments.Add(comment);
            await _db.SaveChangesAsync();
            return comment;
        }

        public Task<List<TestComment>> GetTestCommentsAsync(int testId)
        {
            return _db.TestComments.AsNoTracking().Where(c => c.TestId == testId).OrderBy(c => c.CommentId).ToListAsync();
        }

        public async Task UpdateTestCommentAsync(TestComment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment));
            }

            _db.TestComments.Update(comment);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteTestCommentAsync(int commentId)
        {
            var comment = await _db.TestComments.FirstOrDefaultAsync(c => c.CommentId == commentId);
            if (comment == null)
            {
                return;
            }

            _db.TestComments.Remove(comment);
            await _db.SaveChangesAsync();
        }

        public async Task<PriceList> CreatePriceListAsync(PriceList priceList)
        {
            if (priceList == null || string.IsNullOrWhiteSpace(priceList.Name))
            {
                throw new ArgumentException("Price list name is required.", nameof(priceList));
            }

            _db.PriceLists.Add(priceList);
            await _db.SaveChangesAsync();
            return priceList;
        }

        public Task<List<PriceList>> GetPriceListsAsync()
        {
            return _db.PriceLists.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<PriceListItem> AddPriceListItemAsync(PriceListItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _db.PriceListItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public Task<List<PriceListItem>> GetPriceListItemsAsync(int priceListId)
        {
            return _db.PriceListItems.AsNoTracking()
                .Include(i => i.Test)
                .Where(i => i.PriceListId == priceListId)
                .OrderBy(i => i.PriceListItemId)
                .ToListAsync();
        }

        public async Task DeletePriceListItemAsync(int priceListItemId)
        {
            var item = await _db.PriceListItems.FirstOrDefaultAsync(i => i.PriceListItemId == priceListItemId);
            if (item == null)
            {
                return;
            }

            _db.PriceListItems.Remove(item);
            await _db.SaveChangesAsync();
        }

        public async Task<CustomGroup> CreateCustomGroupAsync(CustomGroup group)
        {
            if (group == null || string.IsNullOrWhiteSpace(group.Name))
            {
                throw new ArgumentException("Custom group name is required.", nameof(group));
            }

            _db.CustomGroups.Add(group);
            await _db.SaveChangesAsync();
            return group;
        }

        public Task<List<CustomGroup>> GetCustomGroupsAsync()
        {
            return _db.CustomGroups.AsNoTracking().OrderBy(g => g.Name).ToListAsync();
        }

        public async Task<CustomGroupItem> AddCustomGroupItemAsync(CustomGroupItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _db.CustomGroupItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public Task<List<CustomGroupItem>> GetCustomGroupItemsAsync(int customGroupId)
        {
            return _db.CustomGroupItems.AsNoTracking()
                .Include(i => i.Test)
                .Where(i => i.CustomGroupId == customGroupId)
                .OrderBy(i => i.CustomGroupItemId)
                .ToListAsync();
        }

        public async Task DeleteCustomGroupItemAsync(int customGroupItemId)
        {
            var item = await _db.CustomGroupItems.FirstOrDefaultAsync(i => i.CustomGroupItemId == customGroupItemId);
            if (item == null)
            {
                return;
            }

            _db.CustomGroupItems.Remove(item);
            await _db.SaveChangesAsync();
        }

        public async Task<Referral> CreateReferralAsync(Referral referral)
        {
            if (referral == null || string.IsNullOrWhiteSpace(referral.Name) || string.IsNullOrWhiteSpace(referral.ReferralType))
            {
                throw new ArgumentException("Referral name and type are required.", nameof(referral));
            }

            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();
            return referral;
        }

        public Task<List<Referral>> GetReferralsAsync()
        {
            return _db.Referrals.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
        }

        public async Task DeleteReferralAsync(int referralId)
        {
            var referral = await _db.Referrals.FirstOrDefaultAsync(r => r.ReferralId == referralId);
            if (referral == null)
            {
                return;
            }

            _db.Referrals.Remove(referral);
            await _db.SaveChangesAsync();
        }
    }
}
