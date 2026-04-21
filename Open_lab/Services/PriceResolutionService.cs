using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    /// <summary>
    /// Implementation of price resolution logic with priority:
    /// 1. Physician's PriceList
    /// 2. Referral's PriceList  
    /// 3. Test's Base Price
    /// </summary>
    public class PriceResolutionService : IPriceResolutionService
    {
        private readonly OpenLabDbContext _context;

        public PriceResolutionService(OpenLabDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ResolvePriceAsync(int testId, int? physicianId, int? referralId)
        {
            var (price, _, _) = await GetPriceSourceAsync(testId, physicianId, referralId);
            return price;
        }

        public async Task<decimal[]> ResolvePricesAsync(int[] testIds, int? physicianId, int? referralId)
        {
            var results = new decimal[testIds.Length];
            for (int i = 0; i < testIds.Length; i++)
            {
                results[i] = await ResolvePriceAsync(testIds[i], physicianId, referralId);
            }
            return results;
        }

        public async Task<(decimal Price, string SourceType, string? SourceName)> GetPriceSourceAsync(int testId, int? physicianId, int? referralId)
        {
            // Get the test base price
            var test = await _context.Tests.FindAsync(testId);
            if (test == null)
            {
                return (0, "Error", "Test not found");
            }

            var basePrice = test.Price;

            // Priority 1: Check physician's price list
            if (physicianId.HasValue)
            {
                var physician = await _context.Set<Physician>()
                    .Include(p => p.PriceList)
                    .ThenInclude(pl => pl!.Items)
                    .FirstOrDefaultAsync(p => p.PhysicianId == physicianId.Value);

                if (physician?.PriceListId.HasValue == true)
                {
                    var priceListItem = physician.PriceList?.Items
                        .FirstOrDefault(i => i.TestId == testId);
                    
                    if (priceListItem != null)
                    {
                        return (priceListItem.Price, "PhysicianPriceList", physician.PriceList?.Name);
                    }
                }
            }

            // Priority 2: Check referral's price list
            if (referralId.HasValue)
            {
                var referral = await _context.Referrals
                    .Include(r => r.PriceLists)
                    .ThenInclude(pl => pl.Items)
                    .FirstOrDefaultAsync(r => r.ReferralId == referralId.Value);

                if (referral?.PriceLists != null)
                {
                    foreach (var priceList in referral.PriceLists)
                    {
                        var priceListItem = priceList.Items.FirstOrDefault(i => i.TestId == testId);
                        if (priceListItem != null)
                        {
                            return (priceListItem.Price, "ReferralPriceList", priceList.Name);
                        }
                    }
                }
            }

            // Priority 3: Return test base price
            return (basePrice, "TestBasePrice", test.NameReceipt);
        }
    }
}
