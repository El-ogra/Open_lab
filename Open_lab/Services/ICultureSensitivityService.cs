using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface ICultureSensitivityService
    {
        Task<List<Culture>> GetCulturesAsync();
        Task<Culture> CreateCultureAsync(Culture culture);
        Task DeleteCultureAsync(int cultureId);

        Task<List<Antibiotic>> GetAntibioticsAsync();
        Task<Antibiotic> CreateAntibioticAsync(Antibiotic antibiotic);
        Task DeleteAntibioticAsync(int antibioticId);

        Task<List<CultureAntibiotic>> GetCultureAntibioticsAsync(int cultureId);
        Task LinkAntibioticAsync(int cultureId, int antibioticId);
        Task UnlinkAntibioticAsync(int cultureId, int antibioticId);
    }
}
