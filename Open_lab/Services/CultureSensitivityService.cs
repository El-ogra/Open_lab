using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class CultureSensitivityService : ICultureSensitivityService
    {
        private readonly OpenLabDbContext _db;

        public CultureSensitivityService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<List<Culture>> GetCulturesAsync()
        {
            return _db.Cultures.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Culture> CreateCultureAsync(Culture culture)
        {
            if (culture == null || string.IsNullOrWhiteSpace(culture.Name))
            {
                throw new ArgumentException("اسم المزرعة مطلوب.", nameof(culture));
            }

            _db.Cultures.Add(culture);
            await _db.SaveChangesAsync();
            return culture;
        }

        public async Task DeleteCultureAsync(int cultureId)
        {
            var culture = await _db.Cultures.FirstOrDefaultAsync(c => c.CultureId == cultureId);
            if (culture == null)
            {
                return;
            }

            _db.Cultures.Remove(culture);
            await _db.SaveChangesAsync();
        }

        public Task<List<Antibiotic>> GetAntibioticsAsync()
        {
            return _db.Antibiotics.AsNoTracking().OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<Antibiotic> CreateAntibioticAsync(Antibiotic antibiotic)
        {
            if (antibiotic == null || string.IsNullOrWhiteSpace(antibiotic.Name))
            {
                throw new ArgumentException("اسم المضاد الحيوي مطلوب.", nameof(antibiotic));
            }

            _db.Antibiotics.Add(antibiotic);
            await _db.SaveChangesAsync();
            return antibiotic;
        }

        public async Task DeleteAntibioticAsync(int antibioticId)
        {
            var antibiotic = await _db.Antibiotics.FirstOrDefaultAsync(a => a.AntibioticId == antibioticId);
            if (antibiotic == null)
            {
                return;
            }

            _db.Antibiotics.Remove(antibiotic);
            await _db.SaveChangesAsync();
        }

        public Task<List<CultureAntibiotic>> GetCultureAntibioticsAsync(int cultureId)
        {
            return _db.CultureAntibiotics
                .AsNoTracking()
                .Include(ca => ca.Antibiotic)
                .Where(ca => ca.CultureId == cultureId)
                .OrderBy(ca => ca.Antibiotic.Name)
                .ToListAsync();
        }

        public async Task LinkAntibioticAsync(int cultureId, int antibioticId)
        {
            var exists = await _db.CultureAntibiotics
                .AnyAsync(ca => ca.CultureId == cultureId && ca.AntibioticId == antibioticId);

            if (exists)
            {
                return;
            }

            _db.CultureAntibiotics.Add(new CultureAntibiotic
            {
                CultureId = cultureId,
                AntibioticId = antibioticId
            });
            await _db.SaveChangesAsync();
        }

        public async Task UnlinkAntibioticAsync(int cultureId, int antibioticId)
        {
            var link = await _db.CultureAntibiotics
                .FirstOrDefaultAsync(ca => ca.CultureId == cultureId && ca.AntibioticId == antibioticId);

            if (link == null)
            {
                return;
            }

            _db.CultureAntibiotics.Remove(link);
            await _db.SaveChangesAsync();
        }
    }
}
