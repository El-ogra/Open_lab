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

            NormalizeTest(test);
            ValidateTest(test);

            var duplicateCode = await _db.Tests.AnyAsync(t => t.Code == test.Code);
            if (duplicateCode)
            {
                throw new InvalidOperationException("Test code already exists.");
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

            NormalizeTest(test);
            ValidateTest(test);

            var current = await _db.Tests.FirstOrDefaultAsync(t => t.TestId == test.TestId);
            if (current == null)
            {
                throw new InvalidOperationException("Test not found.");
            }

            var duplicateCode = await _db.Tests.AnyAsync(t => t.TestId != test.TestId && t.Code == test.Code);
            if (duplicateCode)
            {
                throw new InvalidOperationException("Test code already exists.");
            }

            current.Code = test.Code;
            current.NameReport = test.NameReport;
            current.NameReceipt = test.NameReceipt;
            current.GroupId = test.GroupId;
            current.SampleTypeId = test.SampleTypeId;
            current.UnitId = test.UnitId;
            current.Price = test.Price;
            current.TurnaroundHours = test.TurnaroundHours;
            current.ReportOrder = test.ReportOrder;
            current.IsRoutine = test.IsRoutine;
            current.IsSendOut = test.IsSendOut;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteTestAsync(int testId)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if (test == null)
            {
                return;
            }

            var isUsed = await _db.VisitTests.AnyAsync(vt => vt.TestId == testId)
                || await _db.PriceListItems.AnyAsync(i => i.TestId == testId)
                || await _db.CustomGroupItems.AnyAsync(i => i.TestId == testId)
                || await _db.TestParameters.AnyAsync(p => p.TestId == testId);

            if (isUsed)
            {
                throw new InvalidOperationException("Cannot delete test that is already used.");
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

            group.GroupName = group.GroupName.Trim();
            var exists = await _db.TestGroups.AnyAsync(g => g.GroupName == group.GroupName);
            if (exists)
            {
                throw new InvalidOperationException("Group name already exists.");
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

            sampleType.Name = sampleType.Name.Trim();
            var exists = await _db.SampleTypes.AnyAsync(s => s.Name == sampleType.Name);
            if (exists)
            {
                throw new InvalidOperationException("Sample type already exists.");
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

            unit.Name = unit.Name.Trim();
            var exists = await _db.Units.AnyAsync(u => u.Name == unit.Name);
            if (exists)
            {
                throw new InvalidOperationException("Unit already exists.");
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

            var testExists = await _db.Tests.AnyAsync(t => t.TestId == parameter.TestId);
            if (!testExists)
            {
                throw new InvalidOperationException("Test not found.");
            }

            parameter.Name = parameter.Name.Trim();
            if (parameter.OrderNo <= 0)
            {
                parameter.OrderNo = await _db.TestParameters.Where(p => p.TestId == parameter.TestId).Select(p => (int?)p.OrderNo).MaxAsync() ?? 0;
                parameter.OrderNo += 1;
            }

            var duplicate = await _db.TestParameters.AnyAsync(p => p.TestId == parameter.TestId && p.Name == parameter.Name);
            if (duplicate)
            {
                throw new InvalidOperationException("Parameter already exists for this test.");
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

            ValidateReferenceRange(range);
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

            ValidateReferenceRange(range);
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

            comment.CommentText = comment.CommentText.Trim();
            if (comment.IsDefault)
            {
                await ClearDefaultCommentAsync(comment.TestId);
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

            comment.CommentText = comment.CommentText?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(comment.CommentText))
            {
                throw new ArgumentException("Comment text is required.", nameof(comment));
            }

            if (comment.IsDefault)
            {
                await ClearDefaultCommentAsync(comment.TestId, comment.CommentId);
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

            priceList.Name = priceList.Name.Trim();
            var duplicate = await _db.PriceLists.AnyAsync(p => p.Name == priceList.Name && p.ReferralId == priceList.ReferralId);
            if (duplicate)
            {
                throw new InvalidOperationException("Price list already exists for the same referral.");
            }

            if (priceList.IsDefault)
            {
                await ClearDefaultPriceListsAsync(priceList.ReferralId);
            }

            _db.PriceLists.Add(priceList);
            await _db.SaveChangesAsync();
            return priceList;
        }

        public async Task UpdatePriceListAsync(PriceList priceList)
        {
            if (priceList == null)
            {
                throw new ArgumentNullException(nameof(priceList));
            }

            if (string.IsNullOrWhiteSpace(priceList.Name))
            {
                throw new ArgumentException("Price list name is required.", nameof(priceList));
            }

            priceList.Name = priceList.Name.Trim();
            var current = await _db.PriceLists.FirstOrDefaultAsync(p => p.PriceListId == priceList.PriceListId);
            if (current == null)
            {
                throw new InvalidOperationException("Price list not found.");
            }

            var duplicate = await _db.PriceLists.AnyAsync(p => p.PriceListId != priceList.PriceListId && p.Name == priceList.Name && p.ReferralId == priceList.ReferralId);
            if (duplicate)
            {
                throw new InvalidOperationException("Price list already exists for the same referral.");
            }

            if (priceList.IsDefault)
            {
                await ClearDefaultPriceListsAsync(priceList.ReferralId);
            }

            current.Name = priceList.Name;
            current.ReferralId = priceList.ReferralId;
            current.IsDefault = priceList.IsDefault;
            await _db.SaveChangesAsync();
        }

        public Task<List<PriceList>> GetPriceListsAsync()
        {
            return _db.PriceLists.AsNoTracking().Include(p => p.Referral).OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<PriceListItem> AddPriceListItemAsync(PriceListItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.Price < 0)
            {
                throw new ArgumentException("Price cannot be negative.", nameof(item));
            }

            var priceListExists = await _db.PriceLists.AnyAsync(p => p.PriceListId == item.PriceListId);
            var testExists = await _db.Tests.AnyAsync(t => t.TestId == item.TestId);
            if (!priceListExists || !testExists)
            {
                throw new InvalidOperationException("Invalid price list or test.");
            }

            var duplicate = await _db.PriceListItems.AnyAsync(i => i.PriceListId == item.PriceListId && i.TestId == item.TestId);
            if (duplicate)
            {
                throw new InvalidOperationException("Test already exists in this price list.");
            }

            _db.PriceListItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task UpdatePriceListItemAsync(PriceListItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.Price < 0)
            {
                throw new ArgumentException("Price cannot be negative.", nameof(item));
            }

            var current = await _db.PriceListItems.FirstOrDefaultAsync(i => i.PriceListItemId == item.PriceListItemId);
            if (current == null)
            {
                throw new InvalidOperationException("Price list item not found.");
            }

            current.Price = item.Price;
            await _db.SaveChangesAsync();
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

            if (group.Price < 0)
            {
                throw new ArgumentException("Custom group price cannot be negative.", nameof(group));
            }

            group.Name = group.Name.Trim();
            var duplicate = await _db.CustomGroups.AnyAsync(g => g.Name == group.Name);
            if (duplicate)
            {
                throw new InvalidOperationException("Custom group already exists.");
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

            var groupExists = await _db.CustomGroups.AnyAsync(g => g.CustomGroupId == item.CustomGroupId);
            var testExists = await _db.Tests.AnyAsync(t => t.TestId == item.TestId);
            if (!groupExists || !testExists)
            {
                throw new InvalidOperationException("Invalid custom group or test.");
            }

            var duplicate = await _db.CustomGroupItems.AnyAsync(i => i.CustomGroupId == item.CustomGroupId && i.TestId == item.TestId);
            if (duplicate)
            {
                throw new InvalidOperationException("Test already exists in this custom group.");
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

            referral.Name = referral.Name.Trim();
            referral.ReferralType = referral.ReferralType.Trim();
            referral.Phone = string.IsNullOrWhiteSpace(referral.Phone) ? null : referral.Phone.Trim();
            referral.City = string.IsNullOrWhiteSpace(referral.City) ? null : referral.City.Trim();

            var duplicate = await _db.Referrals.AnyAsync(r => r.Name == referral.Name && r.ReferralType == referral.ReferralType);
            if (duplicate)
            {
                throw new InvalidOperationException("Referral already exists.");
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

            var inUse = await _db.Visits.AnyAsync(v => v.ReferralId == referralId)
                || await _db.PriceLists.AnyAsync(p => p.ReferralId == referralId);
            if (inUse)
            {
                throw new InvalidOperationException("Cannot delete referral in use.");
            }

            _db.Referrals.Remove(referral);
            await _db.SaveChangesAsync();
        }

        public async Task<Physician> CreatePhysicianAsync(Physician physician)
        {
            if (physician == null || string.IsNullOrWhiteSpace(physician.FullName))
            {
                throw new ArgumentException("Physician full name is required.", nameof(physician));
            }

            physician.FullName = physician.FullName.Trim();
            physician.Phone = string.IsNullOrWhiteSpace(physician.Phone) ? null : physician.Phone.Trim();
            physician.Specialty = string.IsNullOrWhiteSpace(physician.Specialty) ? null : physician.Specialty.Trim();
            physician.Address = string.IsNullOrWhiteSpace(physician.Address) ? null : physician.Address.Trim();

            if (physician.CommissionPercentage.HasValue && physician.CommissionPercentage.Value < 0)
            {
                throw new ArgumentException("Commission percentage cannot be negative.", nameof(physician));
            }

            var duplicate = await _db.Physicians.AnyAsync(p => p.FullName == physician.FullName && p.Phone == physician.Phone);
            if (duplicate)
            {
                throw new InvalidOperationException("Physician already exists.");
            }

            _db.Physicians.Add(physician);
            await _db.SaveChangesAsync();
            return physician;
        }

        public async Task<List<Physician>> GetPhysiciansAsync()
        {
            return await _db.Physicians.AsNoTracking()
                .Include(p => p.PriceList)
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<Physician?> GetPhysicianByIdAsync(int physicianId)
        {
            return await _db.Physicians.AsNoTracking()
                .Include(p => p.PriceList)
                .FirstOrDefaultAsync(p => p.PhysicianId == physicianId);
        }

        public async Task UpdatePhysicianAsync(Physician physician)
        {
            if (physician == null)
            {
                throw new ArgumentNullException(nameof(physician));
            }

            if (string.IsNullOrWhiteSpace(physician.FullName))
            {
                throw new ArgumentException("Physician full name is required.", nameof(physician));
            }

            physician.FullName = physician.FullName.Trim();
            physician.Phone = string.IsNullOrWhiteSpace(physician.Phone) ? null : physician.Phone.Trim();
            physician.Specialty = string.IsNullOrWhiteSpace(physician.Specialty) ? null : physician.Specialty.Trim();
            physician.Address = string.IsNullOrWhiteSpace(physician.Address) ? null : physician.Address.Trim();

            if (physician.CommissionPercentage.HasValue && physician.CommissionPercentage.Value < 0)
            {
                throw new ArgumentException("Commission percentage cannot be negative.", nameof(physician));
            }

            var current = await _db.Physicians.FirstOrDefaultAsync(p => p.PhysicianId == physician.PhysicianId);
            if (current == null)
            {
                throw new InvalidOperationException("Physician not found.");
            }

            current.FullName = physician.FullName;
            current.Phone = physician.Phone;
            current.Specialty = physician.Specialty;
            current.Address = physician.Address;
            current.IsActive = physician.IsActive;
            current.PriceListId = physician.PriceListId;
            current.CommissionPercentage = physician.CommissionPercentage;

            await _db.SaveChangesAsync();
        }

        public async Task DeletePhysicianAsync(int physicianId)
        {
            var physician = await _db.Physicians.FirstOrDefaultAsync(p => p.PhysicianId == physicianId);
            if (physician == null)
            {
                return;
            }

            var inUse = await _db.Visits.AnyAsync(v => v.PhysicianId == physicianId);
            if (inUse)
            {
                throw new InvalidOperationException("Cannot delete physician that is assigned to visits.");
            }

            _db.Physicians.Remove(physician);
            await _db.SaveChangesAsync();
        }

        private static void NormalizeTest(Test test)
        {
            test.Code = test.Code?.Trim() ?? string.Empty;
            test.NameReport = test.NameReport?.Trim() ?? string.Empty;
            test.NameReceipt = test.NameReceipt?.Trim() ?? string.Empty;
        }

        private static void ValidateTest(Test test)
        {
            if (string.IsNullOrWhiteSpace(test.Code) || string.IsNullOrWhiteSpace(test.NameReport) || string.IsNullOrWhiteSpace(test.NameReceipt))
            {
                throw new ArgumentException("Test code and names are required.", nameof(test));
            }

            if (test.Price < 0)
            {
                throw new ArgumentException("Test price cannot be negative.", nameof(test));
            }

            if (test.TurnaroundHours < 0)
            {
                throw new ArgumentException("Turnaround hours cannot be negative.", nameof(test));
            }
        }

        private static void ValidateReferenceRange(TestReferenceRange range)
        {
            if (range.TestId <= 0)
            {
                throw new ArgumentException("TestId is required.", nameof(range));
            }

            if (range.AgeFrom.HasValue && range.AgeTo.HasValue && range.AgeFrom > range.AgeTo)
            {
                throw new ArgumentException("AgeFrom cannot be greater than AgeTo.", nameof(range));
            }

            if (range.LowValue.HasValue && range.HighValue.HasValue && range.LowValue > range.HighValue)
            {
                throw new ArgumentException("LowValue cannot be greater than HighValue.", nameof(range));
            }

            if (!range.LowValue.HasValue && !range.HighValue.HasValue && string.IsNullOrWhiteSpace(range.NormalText))
            {
                throw new ArgumentException("Reference range requires numeric limits or normal text.", nameof(range));
            }
        }

        private async Task ClearDefaultCommentAsync(int testId, int? exceptCommentId = null)
        {
            var defaults = await _db.TestComments
                .Where(c => c.TestId == testId && c.IsDefault && (!exceptCommentId.HasValue || c.CommentId != exceptCommentId.Value))
                .ToListAsync();

            foreach (var item in defaults)
            {
                item.IsDefault = false;
            }
        }

        private async Task ClearDefaultPriceListsAsync(int? referralId)
        {
            var defaults = await _db.PriceLists
                .Where(p => p.IsDefault && p.ReferralId == referralId)
                .ToListAsync();

            foreach (var item in defaults)
            {
                item.IsDefault = false;
            }
        }
    }
}




