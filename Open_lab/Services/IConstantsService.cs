using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IConstantsService
    {
        Task<List<Setting>> GetConstantsAsync();
        Task SaveConstantAsync(string key, string? value);
        Task DeleteConstantAsync(string key);
        Task SeedDefaultsAsync();
    }
}
