using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface ITestCatalogService
    {
        Task<Test?> GetTestByIdAsync(int testId);
        Task<List<Test>> GetAllTestsAsync();
        Task<Test> CreateTestAsync(Test test);
        Task UpdateTestAsync(Test test);
        Task DeleteTestAsync(int testId);

        Task<TestGroup> CreateTestGroupAsync(TestGroup group);
        Task<List<TestGroup>> GetTestGroupsAsync();

        Task<SampleType> CreateSampleTypeAsync(SampleType sampleType);
        Task<List<SampleType>> GetSampleTypesAsync();

        Task<Unit> CreateUnitAsync(Unit unit);
        Task<List<Unit>> GetUnitsAsync();

        Task<TestParameter> CreateParameterAsync(TestParameter parameter);
        Task<List<TestParameter>> GetParametersByTestAsync(int testId);

        Task<TestReferenceRange> CreateReferenceRangeAsync(TestReferenceRange range);
        Task<List<TestReferenceRange>> GetReferenceRangesAsync(int testId);
        Task UpdateReferenceRangeAsync(TestReferenceRange range);
        Task DeleteReferenceRangeAsync(int rangeId);

        Task<TestComment> CreateTestCommentAsync(TestComment comment);
        Task<List<TestComment>> GetTestCommentsAsync(int testId);
        Task UpdateTestCommentAsync(TestComment comment);
        Task DeleteTestCommentAsync(int commentId);

        Task<PriceList> CreatePriceListAsync(PriceList priceList);
        Task<List<PriceList>> GetPriceListsAsync();
        Task<PriceListItem> AddPriceListItemAsync(PriceListItem item);
        Task<List<PriceListItem>> GetPriceListItemsAsync(int priceListId);
        Task DeletePriceListItemAsync(int priceListItemId);

        Task<CustomGroup> CreateCustomGroupAsync(CustomGroup group);
        Task<List<CustomGroup>> GetCustomGroupsAsync();
        Task<CustomGroupItem> AddCustomGroupItemAsync(CustomGroupItem item);
        Task<List<CustomGroupItem>> GetCustomGroupItemsAsync(int customGroupId);
        Task DeleteCustomGroupItemAsync(int customGroupItemId);

        Task<Referral> CreateReferralAsync(Referral referral);
        Task<List<Referral>> GetReferralsAsync();
        Task DeleteReferralAsync(int referralId);
    }
}
