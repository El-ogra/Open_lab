using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    /// <summary>
    /// Service for resolving the correct price for a test based on priority:
    /// 1. Physician's PriceList (if assigned)
    /// 2. Referral's PriceList (if assigned)
    /// 3. Test's default Price (Base Price)
    /// </summary>
    public interface IPriceResolutionService
    {
        /// <summary>
        /// Resolves the price for a specific test given the physician and referral context.
        /// Priority: Physician PriceList → Referral PriceList → Test Base Price
        /// </summary>
        /// <param name="testId">The test ID</param>
        /// <param name="physicianId">Optional physician ID</param>
        /// <param name="referralId">Optional referral ID</param>
        /// <returns>The resolved price</returns>
        Task<decimal> ResolvePriceAsync(int testId, int? physicianId, int? referralId);

        /// <summary>
        /// Resolves prices for multiple tests in a single call.
        /// </summary>
        /// <param name="testIds">Array of test IDs</param>
        /// <param name="physicianId">Optional physician ID</param>
        /// <param name="referralId">Optional referral ID</param>
        /// <returns>Array of resolved prices matching the test IDs order</returns>
        Task<decimal[]> ResolvePricesAsync(int[] testIds, int? physicianId, int? referralId);

        /// <summary>
        /// Gets the price source information for debugging/auditing.
        /// </summary>
        /// <param name="testId">The test ID</param>
        /// <param name="physicianId">Optional physician ID</param>
        /// <param name="referralId">Optional referral ID</param>
        /// <returns>Tuple of (Price, SourceType, SourceName)</returns>
        Task<(decimal Price, string SourceType, string? SourceName)> GetPriceSourceAsync(int testId, int? physicianId, int? referralId);
    }
}
