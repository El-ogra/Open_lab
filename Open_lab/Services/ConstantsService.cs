using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class ConstantsService : IConstantsService
    {
        private const string Prefix = "Constants.";
        private readonly OpenLabDbContext _db;

        public ConstantsService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<List<Setting>> GetConstantsAsync()
        {
            return _db.Settings.AsNoTracking().Where(s => s.Key.StartsWith(Prefix)).OrderBy(s => s.Key).ToListAsync();
        }

        public async Task SaveConstantAsync(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new System.ArgumentException("Key is required.", nameof(key));
            }

            var normalized = key.StartsWith(Prefix) ? key.Trim() : Prefix + key.Trim();
            var setting = await _db.Settings.FirstOrDefaultAsync(s => s.Key == normalized);
            if (setting == null)
            {
                _db.Settings.Add(new Setting { Key = normalized, Value = value });
            }
            else
            {
                setting.Value = value;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteConstantAsync(string key)
        {
            var normalized = key.StartsWith(Prefix) ? key.Trim() : Prefix + key.Trim();
            var setting = await _db.Settings.FirstOrDefaultAsync(s => s.Key == normalized);
            if (setting == null)
            {
                return;
            }

            _db.Settings.Remove(setting);
            await _db.SaveChangesAsync();
        }

        public async Task SeedDefaultsAsync()
        {
            var defaults = new Dictionary<string, string>
            {
                [Prefix + "CBC.MCH.Formula"] = "HGB10_DIV_RBC",
                [Prefix + "CBC.MCHC.Formula"] = "HGB100_DIV_HCT",
                [Prefix + "PT.ControlTime"] = "12",
                [Prefix + "PT.ISI"] = "1.0",
                [Prefix + "PTT.ControlTime"] = "30"
            };

            foreach (var item in defaults)
            {
                if (!await _db.Settings.AnyAsync(s => s.Key == item.Key))
                {
                    _db.Settings.Add(new Setting { Key = item.Key, Value = item.Value });
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}


