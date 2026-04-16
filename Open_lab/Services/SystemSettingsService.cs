using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly OpenLabDbContext _db;

        public SystemSettingsService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<List<Setting>> GetSettingsAsync()
        {
            return _db.Settings.AsNoTracking().OrderBy(s => s.Key).ToListAsync();
        }

        public async Task SaveSettingAsync(string key, string? value)
        {
            var setting = await _db.Settings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                setting = new Setting { Key = key, Value = value };
                _db.Settings.Add(setting);
            }
            else
            {
                setting.Value = value;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteSettingAsync(string key)
        {
            var setting = await _db.Settings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                return;
            }

            _db.Settings.Remove(setting);
            await _db.SaveChangesAsync();
        }
    }
}
