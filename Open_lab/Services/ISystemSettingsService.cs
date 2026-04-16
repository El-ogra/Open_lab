using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface ISystemSettingsService
    {
        Task<List<Setting>> GetSettingsAsync();
        Task SaveSettingAsync(string key, string? value);
        Task DeleteSettingAsync(string key);
    }
}
