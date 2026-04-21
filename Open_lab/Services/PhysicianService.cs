using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    /// <summary>
    /// Implementation of physician management service.
    /// </summary>
    public class PhysicianService : IPhysicianService
    {
        private readonly OpenLabDbContext _context;

        public PhysicianService(OpenLabDbContext context)
        {
            _context = context;
        }

        public async Task<List<Physician>> GetAllAsync()
        {
            return await _context.Set<Physician>()
                .Include(p => p.PriceList)
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<List<Physician>> GetActiveAsync()
        {
            return await _context.Set<Physician>()
                .Where(p => p.IsActive)
                .Include(p => p.PriceList)
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<Physician?> GetByIdAsync(int physicianId)
        {
            return await _context.Set<Physician>()
                .Include(p => p.PriceList)
                .FirstOrDefaultAsync(p => p.PhysicianId == physicianId);
        }

        public async Task<Physician> CreateAsync(Physician physician)
        {
            _context.Set<Physician>().Add(physician);
            await _context.SaveChangesAsync();
            return physician;
        }

        public async Task<Physician> UpdateAsync(Physician physician)
        {
            _context.Set<Physician>().Update(physician);
            await _context.SaveChangesAsync();
            return physician;
        }

        public async Task DeleteAsync(int physicianId)
        {
            var physician = await GetByIdAsync(physicianId);
            if (physician != null)
            {
                _context.Set<Physician>().Remove(physician);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Physician>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.Set<Physician>()
                .Where(p => p.FullName.ToLower().Contains(term) ||
                           (p.Specialty != null && p.Specialty.ToLower().Contains(term)) ||
                           (p.Phone != null && p.Phone.Contains(term)))
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }
    }
}
